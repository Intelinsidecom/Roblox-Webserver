using Npgsql;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Games;

public static class GameCreationService
{
    public static async Task<UniverseInfo> CreateUniverseWithRootPlaceAsync(
        string? connectionString,
        long creatorUserId,
        string creatorUserName,
        string? assetsRoot,
        string? starterPlacePath,
        bool enableCreationCooldown,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));
        if (creatorUserId <= 0)
            throw new ArgumentOutOfRangeException(nameof(creatorUserId));

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        var tx = conn.BeginTransaction();

        if (enableCreationCooldown)
        {
            // Enforce a simple 2-hour cooldown between game creations per user when enabled.
            const string lastUniverseSql = @"select created_at from universes where creator_user_id = @uid order by created_at desc limit 1;";
            using (var lastCmd = new NpgsqlCommand(lastUniverseSql, conn, (NpgsqlTransaction)tx))
            {
                lastCmd.Parameters.AddWithValue("uid", creatorUserId);
                await using var reader = await lastCmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false) && !reader.IsDBNull(0))
                {
                    var lastCreated = reader.GetDateTime(0);
                    if (lastCreated > DateTime.UtcNow.AddHours(-2))
                    {
                        throw new InvalidOperationException("User has created a game too recently.");
                    }
                }
            }
        }

        // Determine the next place sequence for this user.
        const string maxPlaceSeqSql = @"select coalesce(count(*), 0) from assets where owner_user_id = @uid and is_place = true;";
        int nextPlaceNumber;
        using (var seqCmd = new NpgsqlCommand(maxPlaceSeqSql, conn, (NpgsqlTransaction)tx))
        {
            seqCmd.Parameters.AddWithValue("uid", creatorUserId);
            var obj = await seqCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            var existingCount = Convert.ToInt32(obj ?? 0);
            nextPlaceNumber = existingCount + 1;
        }

        var safeUserName = string.IsNullOrWhiteSpace(creatorUserName) ? "Player" : creatorUserName;
        var placeName = $"{safeUserName}'s Place Number: {nextPlaceNumber}";

        // Attempt to hash and save the Starter Place .rbxl into the asset directory.
        // If anything fails, we fall back to a placeholder content hash.
        string contentHash = "pending-place-content";
        const string fileExtension = ".rbxl";

        try
        {
            if (!string.IsNullOrWhiteSpace(assetsRoot) &&
                !string.IsNullOrWhiteSpace(starterPlacePath) &&
                File.Exists(starterPlacePath))
            {
                // Older target frameworks for this assembly do not support File.ReadAllBytesAsync,
                // so use the synchronous variant here.
                var bytes = File.ReadAllBytes(starterPlacePath);

                using (var sha = SHA256.Create())
                {
                    var hashBytes = sha.ComputeHash(bytes);
                    var sb = new StringBuilder(hashBytes.Length * 2);
                    foreach (var b in hashBytes)
                        sb.Append(b.ToString("x2"));
                    contentHash = sb.ToString();
                }

                var assetFolder = Path.Combine(assetsRoot, "asset");
                Directory.CreateDirectory(assetFolder);

                var fileName = contentHash + fileExtension;
                var fullPath = Path.Combine(assetFolder, fileName);

                if (!File.Exists(fullPath))
                {
                    // Older target frameworks for this assembly do not support File.WriteAllBytesAsync,
                    // so use the synchronous variant here.
                    File.WriteAllBytes(fullPath, bytes);
                }
            }
        }
        catch
        {
            // Swallow any errors and keep using the placeholder content hash.
            contentHash = "pending-place-content";
        }

        long rootPlaceId;
        const int placeAssetTypeId = 9;

        const string insertPlaceSql = @"insert into assets
(name, asset_type_id, owner_user_id, content_hash, file_extension, content_type, thumbnail_url, is_place, privacy_level)
values (@name, @assetTypeId, @ownerUserId, @contentHash, @fileExtension, @contentType, null, @isPlace, @privacyLevel)
returning asset_id;";

        using (var cmd = new NpgsqlCommand(insertPlaceSql, conn, (NpgsqlTransaction)tx))
        {
            cmd.Parameters.AddWithValue("name", placeName);
            cmd.Parameters.AddWithValue("assetTypeId", placeAssetTypeId);
            cmd.Parameters.AddWithValue("ownerUserId", creatorUserId);
            cmd.Parameters.AddWithValue("contentHash", contentHash);
            cmd.Parameters.AddWithValue("fileExtension", fileExtension);
            cmd.Parameters.AddWithValue("contentType", "application/octet-stream");
            cmd.Parameters.AddWithValue("isPlace", true);
            // 3 = Private, per privacy enum comment; user requested private by default.
            cmd.Parameters.AddWithValue("privacyLevel", (short)3);

            var obj = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            rootPlaceId = (long)(obj ?? 0L);
        }

        if (rootPlaceId <= 0)
            throw new InvalidOperationException("Failed to insert root place asset");

        long universeId;
        const string insertUniverseSql = @"insert into universes
(name, creator_user_id, place_ids)
values (@name, @creatorUserId, array[ @rootPlaceId ]::bigint[])
returning universe_id;";

        using (var cmd = new NpgsqlCommand(insertUniverseSql, conn, (NpgsqlTransaction)tx))
        {
            cmd.Parameters.AddWithValue("name", placeName);
            cmd.Parameters.AddWithValue("creatorUserId", creatorUserId);
            cmd.Parameters.AddWithValue("rootPlaceId", rootPlaceId);

            var obj = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            universeId = (long)(obj ?? 0L);
        }

        if (universeId <= 0)
            throw new InvalidOperationException("Failed to insert universe");

        // Update owned_places and owned_universes JSON arrays on the user for easier fetching.
        const string updateUserSql = @"update users
set owned_places = coalesce(owned_places, '[]'::jsonb) || to_jsonb(@placeId::bigint),
    owned_universes = coalesce(owned_universes, '[]'::jsonb) || to_jsonb(@universeId::bigint)
where user_id = @uid;";

        using (var userCmd = new NpgsqlCommand(updateUserSql, conn, (NpgsqlTransaction)tx))
        {
            userCmd.Parameters.AddWithValue("placeId", rootPlaceId);
            userCmd.Parameters.AddWithValue("universeId", universeId);
            userCmd.Parameters.AddWithValue("uid", creatorUserId);
            await userCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        tx.Commit();

        return new UniverseInfo
        {
            UniverseId = universeId,
            RootPlaceId = rootPlaceId,
            CreatorUserId = creatorUserId,
            Name = placeName,
        };
    }
}
