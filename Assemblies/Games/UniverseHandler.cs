using Npgsql;
using System.Text.Json;
using Common;
using Assets;
using System.Text;

namespace Games
{
    /// <summary>
    /// Handles universe-related business logic and database operations
    /// </summary>
    public static class UniverseHandler
    {
        /// <summary>
        /// Updates universe name in database
        /// </summary>
        public static async Task UpdateUniverseNameAsync(long universeId, string connectionString, string name, CancellationToken cancellationToken = default)
        {
            await GameCreationService.UpdateUniverseNameAsync(universeId, connectionString, name, cancellationToken);
        }

        /// <summary>
        /// Updates the in_universe field for a place
        /// </summary>
        public static async Task UpdatePlaceInUniverseFieldAsync(long placeId, bool inUniverse, string connectionString, CancellationToken cancellationToken = default)
        {
            const string updateSql = @"
                UPDATE assets 
                SET in_universe = @inUniverse 
                WHERE asset_id = @placeId AND is_place = true";

            await using (var conn = new NpgsqlConnection(connectionString))
            {
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
                using (var cmd = new NpgsqlCommand(updateSql, conn))
                {
                    cmd.Parameters.AddWithValue("inUniverse", inUniverse);
                    cmd.Parameters.AddWithValue("placeId", placeId);
                    await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Syncs the in_universe field for all places in a universe
        /// </summary>
        public static async Task SyncUniversePlacesInUniverseFieldAsync(long universeId, string connectionString, CancellationToken cancellationToken = default)
        {
            const string getUniversePlacesSql = @"
                SELECT u.root_place_id, u.place_ids 
                FROM universes u 
                WHERE u.universe_id = @universeId";

            const string updatePlaceSql = @"
                UPDATE assets 
                SET in_universe = @inUniverse 
                WHERE asset_id = @placeId AND is_place = true";

            await using (var conn = new NpgsqlConnection(connectionString))
            {
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
                
                // Get all place IDs in the universe
                var allPlaceIds = new List<long>();
                
                using (var cmd = new NpgsqlCommand(getUniversePlacesSql, conn))
                {
                    cmd.Parameters.AddWithValue("universeId", universeId);
                    await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                    
                    if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                    {
                        var rootPlaceId = reader.IsDBNull(0) ? 0L : reader.GetInt64(0);
                        var placeIds = reader.IsDBNull(1) ? new long[0] : (long[])reader.GetValue(1);
                        
                        if (rootPlaceId > 0)
                            allPlaceIds.Add(rootPlaceId);
                        
                        allPlaceIds.AddRange(placeIds.Where(pid => pid > 0));
                    }
                }

                // Update in_universe field for all places in this universe
                foreach (var placeId in allPlaceIds)
                {
                    using (var updateCmd = new NpgsqlCommand(updatePlaceSql, conn))
                    {
                        updateCmd.Parameters.AddWithValue("inUniverse", true);
                        updateCmd.Parameters.AddWithValue("placeId", placeId);
                        await updateCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                    }
                }
            }
        }

        /// <summary>
        /// Updates universe configuration in database
        /// </summary>
        public static async Task<bool> UpdateUniverseConfigurationAsync(long universeId, long userId, string name, int privacyLevel, bool studioAccessToApis, string connectionString)
        {
            const string sql = @"UPDATE universes 
                               SET name = @name, 
                                   privacy_level = @privacyLevel,
                                   Studio_Access_To_APIs = @studioAccessToApis
                               WHERE universe_id = @universeId 
                                 AND universe_id IN (
                                     SELECT value::bigint 
                                     FROM users, jsonb_array_elements_text(owned_universes) 
                                     WHERE user_id = @userId
                                 )";

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("name", name);
            cmd.Parameters.AddWithValue("privacyLevel", privacyLevel);
            cmd.Parameters.AddWithValue("studioAccessToApis", studioAccessToApis);
            cmd.Parameters.AddWithValue("universeId", universeId);
            cmd.Parameters.AddWithValue("userId", userId);

            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        /// <summary>
        /// Gets the root place ID for a universe
        /// </summary>
        public static async Task<long?> GetUniverseRootPlaceIdAsync(string connectionString, long universeId)
        {
            await using (var conn = new NpgsqlConnection(connectionString))
            {
                await conn.OpenAsync().ConfigureAwait(false);
                const string getRootPlaceSql = @"SELECT root_place_id FROM universes WHERE universe_id = @universeId AND root_place_id IS NOT NULL";
                
                using (var cmd = new NpgsqlCommand(getRootPlaceSql, conn))
                {
                    cmd.Parameters.AddWithValue("universeId", universeId);
                    var result = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt64(result);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Clears the root place for a universe
        /// </summary>
        public static async Task<bool> ClearUniverseRootPlaceAsync(long universeId, string connectionString)
        {
            await using (var conn = new NpgsqlConnection(connectionString))
            {
                await conn.OpenAsync().ConfigureAwait(false);
                
                const string clearRootPlaceSql = @"
                    UPDATE universes 
                    SET root_place_id = NULL 
                    WHERE universe_id = @universeId";

                using var cmd = new NpgsqlCommand(clearRootPlaceSql, conn);
                cmd.Parameters.AddWithValue("universeId", universeId);
                
                var rowsAffected = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                return rowsAffected > 0;
            }
        }

        /// <summary>
        /// Gets all place IDs for a universe (optionally excluding root place)
        /// </summary>
        public static async Task<long[]> GetUniversePlaceIdsAsync(string connectionString, long universeId, bool excludeRootPlace = false)
        {
            var placeIds = new List<long>();
            await using (var conn = new NpgsqlConnection(connectionString))
            {
                await conn.OpenAsync().ConfigureAwait(false);
                const string getPlacesSql = @"
                    SELECT unnest(place_ids) 
                    FROM universes 
                    WHERE universe_id = @universeId 
                    AND place_ids IS NOT NULL";
                
                using (var cmd = new NpgsqlCommand(getPlacesSql, conn))
                {
                    cmd.Parameters.AddWithValue("universeId", universeId);
                    await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var placeId = reader.GetInt64(0);
                        placeIds.Add(placeId);
                    }
                }
            }

            if (excludeRootPlace)
            {
                var rootPlaceId = await GetUniverseRootPlaceIdAsync(connectionString, universeId);
                if (rootPlaceId.HasValue)
                {
                    placeIds.Remove(rootPlaceId.Value);
                }
            }

            return placeIds.ToArray();
        }

        /// <summary>
        /// Gets places by context for place selector modal
        /// </summary>
        public static async Task<(List<object> places, int totalCount, int totalPages, int pageNumber)> GetPlacesByContextAsync(
            string connectionString, 
            long currentUserId, 
            string creationContext, 
            long universeId = 0, 
            int startIndex = 0, 
            int maxRows = 10)
        {
            var places = new List<object>();
            var totalCount = 0;

            // Optimized approach: Get all user-owned assets, filter places, then check universe membership
            const string getUserAssetsSql = @"
                SELECT asset_id, name, created_at, is_place, in_universe
                FROM assets 
                WHERE owner_user_id = @userId 
                ORDER BY created_at DESC
                LIMIT @limit OFFSET @offset";

            const string getCountSql = @"
                SELECT COUNT(*)
                FROM assets 
                WHERE owner_user_id = @userId 
                AND is_place = true";

            const string getUniverseInfoSql = @"
                SELECT 
                    u.universe_id,
                    u.name,
                    u.root_place_id,
                    u.place_ids
                FROM universes u
                WHERE u.universe_id IN (
                    SELECT value::bigint 
                    FROM users, jsonb_array_elements_text(owned_universes) 
                    WHERE user_id = @userId
                )";

            await using (var conn = new NpgsqlConnection(connectionString))
            {
                await conn.OpenAsync().ConfigureAwait(false);

                // Get user's assets with pagination, then filter for places
                var userPlaces = new List<(long placeId, string name, DateTime createdAt, bool inUniverse)>();
                var allPlaces = new Dictionary<long, (string name, DateTime createdAt, bool inUniverse)>();
                
                // Get user assets and filter for places
                using (var cmd = new NpgsqlCommand(getUserAssetsSql, conn))
                {
                    cmd.Parameters.AddWithValue("userId", currentUserId);
                    cmd.Parameters.AddWithValue("limit", maxRows);
                    cmd.Parameters.AddWithValue("offset", startIndex);
                
                    await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var assetId = reader.GetInt64(0);
                        var name = reader.IsDBNull(1) ? "Unnamed Asset" : reader.GetString(1);
                        var createdAt = reader.GetDateTime(2);
                        var isPlace = reader.GetBoolean(3);
                        var inUniverse = reader.GetBoolean(4);
                        
                        // Only include place assets
                        if (isPlace)
                        {
                            userPlaces.Add((assetId, name, createdAt, inUniverse));
                            allPlaces[assetId] = (name, createdAt, inUniverse);
                        }
                    }
                }
                
                // Get total count
                if (creationContext == "NonGameCreation" && universeId > 0)
                {
                    var totalUserPlacesCount = 0;
                    using (var countCmd = new NpgsqlCommand(getCountSql, conn))
                    {
                        countCmd.Parameters.AddWithValue("userId", currentUserId);
                        var countResult = await countCmd.ExecuteScalarAsync().ConfigureAwait(false);
                        totalUserPlacesCount = Convert.ToInt32(countResult);
                    }
                    totalCount = totalUserPlacesCount;
                }

                // Get universe info for this user (cached per request)
                var universeDict = new Dictionary<long, (string name, long rootPlaceId, long[] placeIds)>();
                using (var universeCmd = new NpgsqlCommand(getUniverseInfoSql, conn))
                {
                    universeCmd.Parameters.AddWithValue("userId", currentUserId);
                    await using var universeReader = await universeCmd.ExecuteReaderAsync().ConfigureAwait(false);
                    while (await universeReader.ReadAsync().ConfigureAwait(false))
                    {
                        var uId = universeReader.GetInt64(0);
                        universeDict[uId] = (
                            universeReader.IsDBNull(1) ? "No Game" : universeReader.GetString(1),
                            universeReader.IsDBNull(2) ? 0L : universeReader.GetInt64(2),
                            universeReader.IsDBNull(3) ? new long[0] : (long[])universeReader.GetValue(3)
                        );
                    }
                }

                // Add places from current universe to allPlaces dictionary (only if not already present)
                if (creationContext == "NonGameCreation" && universeId > 0 && universeDict.ContainsKey(universeId))
                {
                    var currentUniverse = universeDict[universeId];
                    var universePlaceCount = 0;
                    var allUniversePlaceIds = new List<long>();
                    
                    // Collect all place IDs from this universe
                    if (currentUniverse.rootPlaceId > 0)
                    {
                        allUniversePlaceIds.Add(currentUniverse.rootPlaceId);
                    }
                    allUniversePlaceIds.AddRange(currentUniverse.placeIds.Where(pid => pid > 0));
                    
                    // Get actual place names from assets table
                    if (allUniversePlaceIds.Count > 0)
                    {
                        const string getPlaceNamesSql = @"
                            SELECT asset_id, name, in_universe 
                            FROM assets 
                            WHERE asset_id = ANY(@placeIds)";
                        
                        using (var placesCmd = new NpgsqlCommand(getPlaceNamesSql, conn))
                        {
                            placesCmd.Parameters.AddWithValue("placeIds", allUniversePlaceIds.ToArray());
                            await using var placesReader = await placesCmd.ExecuteReaderAsync().ConfigureAwait(false);
                            
                            while (await placesReader.ReadAsync().ConfigureAwait(false))
                            {
                                var placeId = placesReader.GetInt64(0);
                                var placeName = placesReader.IsDBNull(1) ? "Unnamed Place" : placesReader.GetString(1);
                                var placeInUniverse = placesReader.GetBoolean(2);
                                
                                // Only add if not already in allPlaces (prevent duplicates)
                                if (!allPlaces.ContainsKey(placeId))
                                {
                                    allPlaces[placeId] = (placeName, DateTime.UtcNow, placeInUniverse);
                                    universePlaceCount++;
                                }
                            }
                        }
                    }
                    
                    // Update totalCount to include only NEW universe places
                    totalCount += universePlaceCount;
                }

                // Apply filtering first, then pagination
                var filteredPlaces = new List<object>();
                foreach (var kvp in allPlaces)
                {
                    var placeId = kvp.Key;
                    var (placeName, createdAt, inUniverse) = kvp.Value;
                    string gameName = "No Game";
                    long placeUniverseId = 0L;
                    bool isRootPlace = false;

                    // Always check if place belongs to any universe (regardless of database field consistency)
                    foreach (var universeKvp in universeDict)
                    {
                        var universeIdKey = universeKvp.Key;
                        var (name, rootPlaceId, placeIds) = universeKvp.Value;
                        
                        if (rootPlaceId == placeId)
                        {
                            gameName = name;
                            placeUniverseId = universeIdKey;
                            isRootPlace = true;
                            break;
                        }
                        else if (Array.Exists(placeIds, pid => pid == placeId))
                        {
                            gameName = name;
                            placeUniverseId = universeIdKey;
                            isRootPlace = false;
                            break;
                        }
                    }

                    var ignoreRootPlace = creationContext == "NonGameCreation";
                    var shouldInclude = false;

                    if (creationContext == "NonGameCreation" && universeId > 0)
                    {
                        if (placeUniverseId == 0)
                        {
                            shouldInclude = true; // Places not in any universe
                        }
                        else if (placeUniverseId == universeId && !isRootPlace)
                        {
                            shouldInclude = false;
                        }
                        else if (placeUniverseId == universeId && isRootPlace)
                        {
                            shouldInclude = true;
                        }
                    }
                    else
                    {
                        shouldInclude = true;
                    }

                    var canRemoveFromUniverse = (placeUniverseId == universeId) || placeUniverseId == 0;

                    if (shouldInclude)
                    {
                        filteredPlaces.Add(new
                        {
                            placeId = placeId,
                            name = placeName,
                            gameName = gameName,
                            isRootPlace = isRootPlace,
                            isInUniverse = placeUniverseId != 0 && placeUniverseId != universeId,
                            ignoreRootPlace = ignoreRootPlace && isRootPlace,
                            canRemoveFromUniverse = canRemoveFromUniverse
                        });
                    }
                }

                totalCount = filteredPlaces.Count;
                var pagedPlaces = filteredPlaces.Skip(startIndex).Take(maxRows).ToList();
                var totalPages = (int)Math.Ceiling((double)totalCount / maxRows);
                var pageNumber = (startIndex / maxRows) + 1;

                return (pagedPlaces, totalCount, totalPages, pageNumber);
            }
        }

        /// <summary>
        /// Generates HTML for universe places configuration
        /// </summary>
        public static async Task<string> GenerateUniversePlacesHtmlAsync(long universeId, string connectionString, AssetMetadataRepository assetRepository)
        {
            var htmlBuilder = new StringBuilder();

            // Generate Start Place section
            htmlBuilder.Append(@"
            <div id=""set-startplace-container"" class=""start-place-container divider-bottom"">
                <div class=""configure-places-title"">
                    <h3>Start Place </h3><img class=""TipsyImg tooltip-right"" style=""margin-left: 3px;""
                        src=""/images/65cb6e4009a00247ca02800047aafb87.png""
                        data-toggle=""tooltip""
                        title=""This place will be the starting point for your Game."" />
                </div>
                <div id=""startplace-container"" class=""start-place"">");

            // Get start place (root place)
            var rootPlaceId = await GetUniverseRootPlaceIdAsync(connectionString, universeId);
            
            if (rootPlaceId.HasValue)
            {
                var rootPlaceAsset = await assetRepository.GetAssetByIdAsync(connectionString, rootPlaceId.Value);
                if (rootPlaceAsset != null)
                {
                    var placeName = string.IsNullOrEmpty(rootPlaceAsset.Name) ? "Unnamed Place" : rootPlaceAsset.Name;
                    var thumbnailUrl = $"/game-thumbnails/image?assetId={rootPlaceId.Value}&width=160&height=100&format=jpeg";

                    htmlBuilder.Append(@$"
                    <div class=""start-place-content"" style=""display: block;"">
                        <div class=""universe-place-container"">
                            <div class=""universe-place-thumb"">
                                <a href=""/places/{rootPlaceId.Value}/update"" class=""universe-place"">
                                    <div data-retry-url='/game-thumbnails/json?assetId={rootPlaceId.Value}&width=160&height=100&format=jpeg' style='width: 160px; height: 100px; overflow: hidden;'>
                                        <img class='universe-place-image' 
                                             style='width: 160px; height: 100px; object-fit: cover; display: block;'
                                             src='/images/ec5c01d220bf1b73403fa51519267742.gif' alt=""Place thumbnail"" />
                                    </div>
                                </a>
                            </div>
                            <div class=""universe-detail"">
                                <a href=""/places/{rootPlaceId.Value}/update"" class=""start-place-name"">{placeName}</a>
                                <div class=""start-place-actions"">
                                    <a class=""btn-small btn-primary remove-startplace-button"" data-placeid=""{rootPlaceId.Value}"">Remove</a>
                                </div>
                            </div>
                            <div class=""clear""></div>
                        </div>
                    </div>");
                }
                else
                {
                    htmlBuilder.Append(@"
                    <div class=""no-start-place"" style=""display: block;"">
                        <a class=""btn-small btn-neutral set-startplace-button"">Set</a>
                        <div class=""missing-start-place blank-box"">
                            <span style=""color: red; display: block; margin-left: 0;"">You need to set a start place and set the place as Active in order to make your game playable.</span>
                        </div>
                    </div>");
                }
            }
            else
            {
                htmlBuilder.Append(@"
                <div class=""no-start-place"" style=""display: block;"">
                    <a class=""btn-small btn-neutral set-startplace-button"">Set</a>
                    <div class=""missing-start-place blank-box"">
                        <span style=""color: red; display: block; margin-left: 0;"">You need to set a start place and set the place as Active in order to make your game playable.</span>
                    </div>
                </div>");
            }

            htmlBuilder.Append(@"
                </div>
            </div>");

            // Generate Other Places section
            htmlBuilder.Append(@"
            <div class=""configure-places-title"">
                    <h3>Other Places </h3><img class=""TipsyImg tooltip-top"" style=""margin-left: 3px;""
                        src=""/images/65cb6e4009a00247ca02800047aafb87.png""
                        data-toggle=""tooltip""
                        title=""Add more places to your Game."" />
                </div>
            </div>
            <div id=""add-universe-places"" class=""add-places-container"">
                <a class=""btn-small btn-primary add-place-button"" id=""add-place-modal-button"">Add Place</a>
                <div class=""universe-places-paged"" data-isuniversecreation=""false"">
                    <div id=""current-places"" data-startrow=""0"" data-universeid=""" + universeId + @""" data-totalcount=""0"">");

            // Get universe places (excluding root place for the "Other Places" section)
            var placeIds = await GetUniversePlaceIdsAsync(connectionString, universeId, excludeRootPlace: true);
            
            if (placeIds.Length > 0)
            {
                foreach (var pid in placeIds)
                {
                    var placeAsset = await assetRepository.GetAssetByIdAsync(connectionString, pid);
                    if (placeAsset != null)
                    {
                        var placeName = string.IsNullOrEmpty(placeAsset.Name) ? "Unnamed Place" : placeAsset.Name;
                        var thumbnailUrl = $"/game-thumbnails/image?assetId={pid}&width=160&height=100&format=jpeg";
                        
                        var placeHtml = $@"<div class=""universe-place-container"">
                            <div class=""universe-place-thumb"">
                                <a href=""/places/{pid}/update"" class=""universe-place"">
                                    <div data-retry-url='/game-thumbnails/json?assetId={pid}&width=160&height=100&format=jpeg' style='width: 160px; height: 100px;'>
                                        <img class='universe-place-image' 
                                             style='width: 160px; height: 100px; object-fit: cover; display: block;'
                                             src='/images/ec5e01d220bf1b73403fa51519267742.gif' alt=""Place thumbnail"" />
                                    </div>
                                </a>
                            </div>
                            <div class=""universe-detail"">
                                <a href=""/places/{pid}/update"" class=""place-name"">{placeName}</a>
                                <div class=""place-actions"">
                                    <a class=""btn-small btn-primary remove-place-button"" data-placeid=""{pid}"">Remove</a>
                                </div>
                            </div>
                            <div class=""clear""></div>
                        </div>";
                        
                        htmlBuilder.Append(placeHtml);
                    }
                }
            }
            else
            {
                htmlBuilder.Append(@"
                    <div class=""missing-start-place blank-box"">
                        <span>You can add more places to your Game.</span>
                    </div>");
            }

            htmlBuilder.Append(@"
                    </div>
                </div>
            </div>
            <div class=""clear""></div>");

            return htmlBuilder.ToString();
        }

        /// <summary>
        /// Generates HTML for universe places listing
        /// </summary>
        public static async Task<string> GenerateUniversePlacesListingHtmlAsync(long universeId, int startRow, string connectionString, AssetMetadataRepository assetRepository)
        {
            // Get universe places (excluding root place)
            var placeIds = await GetUniversePlaceIdsAsync(connectionString, universeId, excludeRootPlace: true);
            
            if (placeIds == null || placeIds.Length == 0)
            {
                return "<div class=\"missing-start-place blank-box\"><span>No additional places in this universe.</span></div>";
            }

            // Get place assets for the IDs
            var places = new List<JsonElement>();
            foreach (var placeId in placeIds.Skip(startRow))
            {
                var placeAsset = await assetRepository.GetAssetByIdAsync(connectionString, placeId);
                if (placeAsset != null)
                {
                    var placeObj = JsonSerializer.SerializeToElement(new
                    {
                        id = placeAsset.AssetId,
                        name = placeAsset.Name,
                        description = placeAsset.Description
                    });
                    places.Add(placeObj);
                }
            }

            // Generate HTML for places
            var placesHtml = places.Select(place => 
            {
                var id = place.GetProperty("id").GetInt64();
                var name = place.GetProperty("name").GetString() ?? "";
                var description = place.GetProperty("description").GetString() ?? "";
                
                return $@"<div class=""universe-place-container"">
                    <div class=""universe-place-thumb"">
                        <a href=""/places/{id}/configure"" class=""universe-place"">
                            <div data-retry-url=""/game-thumbnails/json?assetId={id}&width=160&height=100&format=jpeg"" style='width: 160px; height: 100px; overflow: hidden;'>
                                <img class='universe-place-image' 
                                     style='width: 160px; height: 100px; object-fit: cover; display: block;'
                                     src=""/images/ec5e01d220bf1b73403fa51519267742.gif"" 
                                     alt=""Place thumbnail"" />
                            </div>
                        </a>
                    </div>
                    <div class=""universe-detail"">
                        <a href=""/places/{id}/configure"" class=""place-name"">{name}</a>
                        <div class=""place-actions"">
                            <a class=""btn-small btn-primary remove-place-button"" data-placeid=""{id}"">Remove</a>
                        </div>
                    </div>
                    <div class=""clear""></div>
                </div>";
            }).ToList();

            return string.Join("", placesHtml);
        }

        /// <summary>
        /// Gets all place IDs (root + additional) for a universe
        /// </summary>
        public static async Task<List<long>?> GetUniversePlaceIdsAsync(long universeId, string connectionString, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT COALESCE(root_place_id, 0), COALESCE(place_ids, ARRAY[]::bigint[])
                FROM universes WHERE universe_id = @uid";

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("uid", universeId);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

            if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                return null;

            var rootPlaceId = reader.IsDBNull(0) ? 0L : reader.GetInt64(0);
            var raw = (long[])reader.GetValue(1);
            var placeIds = raw.ToList();

            if (rootPlaceId > 0 && !placeIds.Contains(rootPlaceId))
                placeIds.Insert(0, rootPlaceId);

            return placeIds;
        }
    }
}