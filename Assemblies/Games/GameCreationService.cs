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
using Assets;

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
        CancellationToken cancellationToken = default,
        string? templateContentHash = null)
    {
        return await CreateUniverseWithRootPlaceAsync(
            connectionString,
            creatorUserId,
            creatorUserName,
            assetsRoot,
            starterPlacePath,
            enableCreationCooldown,
            thumbnailService: null,
            configuration: null,
            cancellationToken,
            customName: null,
            templateContentHash: templateContentHash);
    }

    public static async Task<UniverseInfo> CreateUniverseWithRootPlaceAsync(
        string? connectionString,
        long creatorUserId,
        string creatorUserName,
        string? assetsRoot,
        string? starterPlacePath,
        bool enableCreationCooldown,
        IThumbnailService? thumbnailService,
        CancellationToken cancellationToken = default,
        string? templateContentHash = null)
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
            cancellationToken,
            customName: null,
            templateContentHash: templateContentHash);
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
        CancellationToken cancellationToken,
        string? customName = null,
        string? templateContentHash = null)
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
        
        // Use custom name if provided, otherwise use auto-generated name
        string placeName;
        if (!string.IsNullOrWhiteSpace(customName))
        {
            placeName = customName;
        }
        else
        {
            placeName = $"{safeUserName}'s Place Number: {nextPlaceNumber}";
        }

        // Use template content hash if provided, otherwise process starter place
        string contentHash;
        const string fileExtension = ".rbxl";

        if (!string.IsNullOrWhiteSpace(templateContentHash))
        {
            // Template was pre-processed, use its hash directly
            contentHash = templateContentHash;
        }
        else
        {
            // No template provided, process starter place as before
            contentHash = "pending-place-content";
            
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
        }

        long rootPlaceId;
        const int placeAssetTypeId = 9;

        const string insertPlaceSql = @"insert into assets
(name, asset_type_id, owner_user_id, content_hash, file_extension, content_type, thumbnail_url, is_place, privacy_level, custom_icon, place_custom_icon_url, place_custom_icon_hash, generated_icon, place_generated_icon_url, place_generated_icon_hash, in_universe)
values (@name, @assetTypeId, @ownerUserId, @contentHash, @fileExtension, @contentType, null, @isPlace, @privacyLevel, @customIcon, @placeCustomIconUrl, @placeCustomIconHash, @generatedIcon, @placeGeneratedIconUrl, @placeGeneratedIconHash, @inUniverse)
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
            // Set in_universe to true since this place is being created as part of a universe
            cmd.Parameters.AddWithValue("inUniverse", true);

            var obj = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            rootPlaceId = (long)(obj ?? 0L);
        }

        if (rootPlaceId <= 0)
            throw new InvalidOperationException("Failed to insert root place asset");

        long universeId;
        const string insertUniverseSql = @"insert into universes
(name, creator_user_id, place_ids, root_place_id)
values (@name, @creatorUserId, array[ @rootPlaceId ]::bigint[], @rootPlaceId)
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
                    var placeAssetHash = await GamesRepository.GetPlaceAssetHashAsync(connectionString, rootPlaceId, CancellationToken.None);
                    
                    // Generate icon thumbnails (256x256 and 1024x1024) for the place
                    await PlaceThumbnail.GeneratePlaceThumbnailAsync(thumbnailService, connectionString, rootPlaceId, placeAssetHash, GetThumbnailBaseUrl(configuration), placeName: placeName, cancellationToken: CancellationToken.None);

                    // Generate 720p auto-generated thumbnail for the place
                    await PlaceThumbnail.GenerateAutoGeneratedThumbnailAsync(thumbnailService, connectionString, rootPlaceId, placeAssetHash, GetThumbnailBaseUrl(configuration), CancellationToken.None);

                }
                catch (Exception ex)
                {
                    // Log the error but don't fail anything
                    Console.WriteLine($"Failed to generate thumbnail for place {rootPlaceId}: {ex.Message}");
                    Console.WriteLine($"Full exception: {ex}");
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

    /// <summary>
    /// Converts social slot type string to database integer value
    /// </summary>
    /// <param name="socialSlotType">Social slot type string</param>
    /// <returns>Integer value for database storage</returns>
    private static int GetServerFillTypeValue(string socialSlotType)
    {
        return socialSlotType?.ToLower() switch
        {
            "automatic" => 0,
            "empty" => 1,
            "custom" => 2,
            _ => 0 // default to automatic
        };
    }

    /// <summary>
    /// Converts access string to database integer value
    /// </summary>
    /// <param name="access">Access string ("Everyone" or "Friends")</param>
    /// <returns>Integer value for database storage (1 = Everyone, 2 = Friends)</returns>
    private static int GetAccessTypeValue(string access)
    {
        return access?.ToLower() switch
        {
            "friends" => 2,
            "everyone" => 1,
            _ => 1 // default to everyone
        };
    }

    /// <summary>
        /// Update place settings after creation
        /// </summary>
        public static async Task UpdatePlaceSettingsAsync(
            long placeId,
            string? connectionString,
            string? description,
            string? genre,
            int maxPlayers,
            string? serverFillType,
            int customSocialSlots,
            string? accessType,
            bool privateServersAllowed,
            bool privateServersFree,
            int privateServersPrice,
            List<int> deviceCompatibility,
            bool isAllGenresAllowed,
            List<string> allowedGearTypes,
            bool isCopyingAllowed,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string is required", nameof(connectionString));
            if (placeId <= 0)
                throw new ArgumentOutOfRangeException(nameof(placeId));

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string updateSql = @"
                UPDATE assets SET 
                    description = @description,
                    genre = @genre,
                    max_visitor_count = @maxPlayers,
                    server_fill_type = @serverFillType,
                    number_of_custom_social_slots = @customSocialSlots,
                    access_type = @accessType,
                    private_servers_allowed = @privateServersAllowed,
                    private_servers_free = @privateServersFree,
                    private_servers_price = @privateServersPrice,
                    device_compatibility = @deviceCompatibility::jsonb,
                    is_all_genres_allowed = @isAllGenresAllowed,
                    allowed_gear_types = @allowedGearTypes::jsonb,
                    is_copying_allowed = @isCopyingAllowed
                WHERE asset_id = @placeId";

            using (var cmd = new NpgsqlCommand(updateSql, conn))
            {
                cmd.Parameters.AddWithValue("description", (object?)description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("genre", AssetGenreNames.GetGenreIdFromString(genre ?? "All"));
                cmd.Parameters.AddWithValue("maxPlayers", maxPlayers);
                cmd.Parameters.AddWithValue("serverFillType", GetServerFillTypeValue(serverFillType ?? "Automatic"));
                cmd.Parameters.AddWithValue("customSocialSlots", customSocialSlots);
                cmd.Parameters.AddWithValue("accessType", GetAccessTypeValue(accessType ?? "Everyone"));
                cmd.Parameters.AddWithValue("privateServersAllowed", privateServersAllowed);
                cmd.Parameters.AddWithValue("privateServersFree", privateServersFree);
                cmd.Parameters.AddWithValue("privateServersPrice", privateServersPrice);
                cmd.Parameters.AddWithValue("deviceCompatibility", System.Text.Json.JsonSerializer.Serialize(deviceCompatibility));
                cmd.Parameters.AddWithValue("isAllGenresAllowed", isAllGenresAllowed);
                cmd.Parameters.AddWithValue("allowedGearTypes", System.Text.Json.JsonSerializer.Serialize(allowedGearTypes));
                cmd.Parameters.AddWithValue("isCopyingAllowed", isCopyingAllowed);
                cmd.Parameters.AddWithValue("placeId", placeId);


                try
                {
                    await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Update universe name
        /// </summary>
        public static async Task UpdateUniverseNameAsync(
            long universeId,
            string? connectionString,
            string name,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string is required", nameof(connectionString));
            if (universeId <= 0)
                throw new ArgumentOutOfRangeException(nameof(universeId));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name is required", nameof(name));

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string updateUniverseSql = @"
                UPDATE universes SET 
                    name = @name
                WHERE universe_id = @universeId";

            using (var cmd = new NpgsqlCommand(updateUniverseSql, conn))
            {
                cmd.Parameters.AddWithValue("name", name);
                cmd.Parameters.AddWithValue("universeId", universeId);

                await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

    private static string GetThumbnailBaseUrl(IConfiguration? configuration)
    {
        // Get the base URL from configuration, fallback to a default
        var baseUrl = configuration?["Thumbnails:ThumbnailUrl"] ?? "https://cdn.freblx.xyz/";
        return baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/";
    }

    public static async Task<UniverseInfo?> CreateUsersFirstPlaceAsync(
        long userId,
        string username,
        string connectionString,
        IConfiguration? configuration,
        CancellationToken cancellationToken = default)
    {
        var starterPlacePath = configuration?["Games:StarterPlacePath"];
        var assetsRoot = configuration?["Assets:Directory"];
        var enableCreationCooldown = bool.TryParse(configuration?["Games:EnableCreationCooldown"], out var cooldown) && cooldown;
        
        if (string.IsNullOrWhiteSpace(starterPlacePath) || string.IsNullOrWhiteSpace(assetsRoot))
        {
            return null;
        }

        try
        {
            var universeInfo = await CreateUniverseWithRootPlaceAsync(
                connectionString,
                userId,
                username,
                assetsRoot,
                starterPlacePath,
                enableCreationCooldown,
                thumbnailService: null,
                configuration: configuration,
                cancellationToken,
                customName: $"{username}'s Place");

            // Generate thumbnail for the starter place
            if (configuration != null)
            {
                try
                {
                    var placeAssetHash = await GamesRepository.GetPlaceAssetHashAsync(connectionString, universeInfo.RootPlaceId, cancellationToken);
                    var thumbnailService = new ThumbnailService(configuration);
                    
                    // Generate icon thumbnails (256x256 and 1024x1024) for the place
                    await PlaceThumbnail.GeneratePlaceThumbnailAsync(thumbnailService, connectionString, universeInfo.RootPlaceId, placeAssetHash, GetThumbnailBaseUrl(configuration), placeName: $"{username}'s Place", cancellationToken: cancellationToken);

                    // Generate 720p auto-generated thumbnail for the place
                    await PlaceThumbnail.GenerateAutoGeneratedThumbnailAsync(thumbnailService, connectionString, universeInfo.RootPlaceId, placeAssetHash, GetThumbnailBaseUrl(configuration), cancellationToken: cancellationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to generate thumbnail for starter place {universeInfo.RootPlaceId}: {ex.Message}");
                }
            }

            await UpdatePlaceSettingsAsync(
                universeInfo.RootPlaceId,
                connectionString,
                description: "This is your very first Roblox creation. Check it out, then make it your own with Roblox Studio!",
                genre: "All",
                maxPlayers: 10,
                serverFillType: "Automatic",
                customSocialSlots: 0,
                accessType: "Everyone",
                privateServersAllowed: false,
                privateServersFree: false,
                privateServersPrice: 0,
                deviceCompatibility: new List<int> { 1, 2, 3 },
                isAllGenresAllowed: true,
                allowedGearTypes: new List<string>(),
                isCopyingAllowed: true,
                cancellationToken);

            return universeInfo;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to create starter place for user {userId}: {ex.Message}");
            return null;
        }
    }

    private static string CombineUrl(string baseUrl, string relative)
    {
        if (string.IsNullOrEmpty(baseUrl)) return relative;
        if (string.IsNullOrEmpty(relative)) return baseUrl;
        var trimmedBase = baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/";
        return trimmedBase + relative.TrimStart('/');
    }

    /// <summary>
    /// Wipes all thumbnail-related URLs for a place to ensure fresh regeneration
    /// </summary>
    /// <param name="placeId">The place ID to clear thumbnails for</param>
    /// <param name="connectionString">Database connection string</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public static async Task ClearPlaceThumbnailUrlsAsync(
        long placeId,
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));
        if (placeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(placeId));

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        // Clear all thumbnail-related URLs and reset flags to defaults
        const string clearThumbnailsSql = @"
            UPDATE assets SET 
                thumbnail_url = NULL,
                place_custom_icon_url = NULL,
                place_custom_icon_high_res_url = NULL,
                place_custom_icon_hash = NULL,
                place_generated_icon_url = NULL,
                place_generated_icon_high_res_url = NULL,
                place_generated_icon_hash = NULL,
                place_generated_thumbnail_url = NULL,
                place_custom_thumbnail = false,
                place_video_thumbnail = false,
                place_autogenerated_thumbnail = false,
                generated_icon = true,
                custom_icon = false
            WHERE asset_id = @placeId";

        using var cmd = new NpgsqlCommand(clearThumbnailsSql, conn);
        cmd.Parameters.AddWithValue("placeId", placeId);
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }
}
