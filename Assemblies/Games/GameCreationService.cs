using Npgsql;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Microsoft.Extensions.Configuration;
using Thumbnails;

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
        return await CreateUniverseWithRootPlaceAsync(
            connectionString,
            creatorUserId,
            creatorUserName,
            assetsRoot,
            starterPlacePath,
            enableCreationCooldown,
            thumbnailService: null,
            cancellationToken);
    }

    public static async Task<UniverseInfo> CreateUniverseWithRootPlaceAsync(
        string? connectionString,
        long creatorUserId,
        string creatorUserName,
        string? assetsRoot,
        string? starterPlacePath,
        bool enableCreationCooldown,
        IThumbnailService? thumbnailService,
        CancellationToken cancellationToken = default)
    {
        return await CreateUniverseWithRootPlaceAsync(
            connectionString,
            creatorUserId,
            creatorUserName,
            assetsRoot,
            starterPlacePath,
            enableCreationCooldown,
            thumbnailService,
            configuration: null,
            cancellationToken);
    }

    public static async Task<UniverseInfo> CreateUniverseWithRootPlaceAsync(
        string? connectionString,
        long creatorUserId,
        string creatorUserName,
        string? assetsRoot,
        string? starterPlacePath,
        bool enableCreationCooldown,
        IThumbnailService? thumbnailService,
        IConfiguration? configuration,
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
                    contentHash = HashingUtilities.GenerateFileHash(bytes);
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
(name, asset_type_id, owner_user_id, content_hash, file_extension, content_type, thumbnail_url, is_place, privacy_level, custom_icon, place_custom_icon_url, place_custom_icon_hash, generated_icon, place_generated_icon_url, place_generated_icon_hash)
values (@name, @assetTypeId, @ownerUserId, @contentHash, @fileExtension, @contentType, null, @isPlace, @privacyLevel, @customIcon, @placeCustomIconUrl, @placeCustomIconHash, @generatedIcon, @placeGeneratedIconUrl, @placeGeneratedIconHash)
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
            // Icon fields - custom icons disabled by default, generated icons enabled by default
            cmd.Parameters.AddWithValue("customIcon", false);
            cmd.Parameters.AddWithValue("placeCustomIconUrl", (object?)null ?? DBNull.Value);
            cmd.Parameters.AddWithValue("placeCustomIconHash", (object?)null ?? DBNull.Value);
            cmd.Parameters.AddWithValue("generatedIcon", true);
            cmd.Parameters.AddWithValue("placeGeneratedIconUrl", (object?)null ?? DBNull.Value);
            cmd.Parameters.AddWithValue("placeGeneratedIconHash", (object?)null ?? DBNull.Value);

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

        // Fire-and-forget thumbnail generation for newly created place
        if (thumbnailService != null)
        {
            // Don't await this - let it run in the background
            _ = Task.Run(async () =>
            {
                try
                {
                    // Get place asset hash for caching
                    var placeAssetHash = await GamesRepository.GetPlaceAssetHashAsync(connectionString, rootPlaceId, cancellationToken);
                    
                    // Generate thumbnail for the place
                    await PlaceThumbnail.GeneratePlaceThumbnailAsync(thumbnailService, connectionString, rootPlaceId, placeAssetHash, GetThumbnailBaseUrl(configuration));

                    // After generation (or cache hit), fetch the generated icon URL and apply it to the universe thumbnail
                    var iconUrl = await GamesRepository.GetPlaceGeneratedIconUrlAsync(connectionString, rootPlaceId, cancellationToken);
                    if (!string.IsNullOrWhiteSpace(iconUrl))
                    {
                        await UniverseThumbnailQueries.SetUniverseThumbnailUrlAsync(connectionString, universeId, iconUrl, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    // Log the error but don't fail anything
                    Console.WriteLine($"Failed to generate thumbnail for place {rootPlaceId}: {ex.Message}");
                }
            });
        }

        return new UniverseInfo
        {
            UniverseId = universeId,
            RootPlaceId = rootPlaceId,
            CreatorUserId = creatorUserId,
            Name = placeName,
            ThumbnailUrl = null // Will be populated when background rendering completes
        };
    }

    private static string GetThumbnailBaseUrl(IConfiguration? configuration)
    {
        // Get the base URL from configuration, fallback to a default
        var baseUrl = configuration?["Thumbnails:ThumbnailUrl"] ?? "https://cdn.freblx.xyz/";
        return baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/";
    }

    private static string CombineUrl(string baseUrl, string relative)
    {
        if (string.IsNullOrEmpty(baseUrl)) return relative;
        if (string.IsNullOrEmpty(relative)) return baseUrl;
        var trimmedBase = baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/";
        return trimmedBase + relative.TrimStart('/');
    }
}
