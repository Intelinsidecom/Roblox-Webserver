using System.IO;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Games;

/// <summary>
/// Represents the settings/creator information for a place
/// </summary>
public class PlaceSettings
{
    public long OwnerId { get; set; }
    public string OwnerName { get; set; } = "Unknown";
    public int CreatorType { get; set; } // 0 = User, 1 = Group
    public long CreatorTargetId { get; set; }
}

/// <summary>
/// Handles universe place management operations including root place management
/// and place array operations for universes
/// </summary>
public static class PlacesHandler
{
    /// <summary>
    /// Sets the root place for a universe
    /// </summary>
    public static async Task<bool> SetUniverseRootPlaceAsync(string connectionString, long universeId, long rootPlaceId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("connectionString is required", nameof(connectionString));
        if (universeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(universeId));
        if (rootPlaceId <= 0)
            throw new ArgumentOutOfRangeException(nameof(rootPlaceId));

        using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        using var tx = conn.BeginTransaction();

        try
        {
            // Update the universe root place
            const string sql = @"UPDATE universes 
                                   SET root_place_id = @rootPlaceId
                                   WHERE universe_id = @universeId";

            using var cmd = new NpgsqlCommand(sql, conn, (NpgsqlTransaction)tx);
            cmd.Parameters.AddWithValue("rootPlaceId", rootPlaceId);
            cmd.Parameters.AddWithValue("universeId", universeId);

            var rowsAffected = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            
            if (rowsAffected > 0)
            {
                // Update the in_universe field for the new root place
                const string updatePlaceSql = @"UPDATE assets 
                                              SET in_universe = true 
                                              WHERE asset_id = @rootPlaceId AND is_place = true";

                using var placeCmd = new NpgsqlCommand(updatePlaceSql, conn, (NpgsqlTransaction)tx);
                placeCmd.Parameters.AddWithValue("rootPlaceId", rootPlaceId);
                await placeCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            tx.Commit();
            return rowsAffected > 0;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    /// <summary>
    /// Removes the root place from a universe (sets to null)
    /// </summary>
    public static async Task<bool> RemoveUniverseRootPlaceAsync(string connectionString, long universeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("connectionString is required", nameof(connectionString));
        if (universeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(universeId));

        using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        using var tx = conn.BeginTransaction();

        try
        {
            // Get the current root place ID before removing it
            const string getRootPlaceSql = @"SELECT root_place_id FROM universes WHERE universe_id = @universeId";
            long? oldRootPlaceId = null;

            using (var getCmd = new NpgsqlCommand(getRootPlaceSql, conn, (NpgsqlTransaction)tx))
            {
                getCmd.Parameters.AddWithValue("universeId", universeId);
                var result = await getCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                if (result != null && result != DBNull.Value)
                {
                    oldRootPlaceId = Convert.ToInt64(result);
                }
            }

            // Remove the root place from universe
            const string sql = @"UPDATE universes 
                                   SET root_place_id = NULL
                                   WHERE universe_id = @universeId";

            using var cmd = new NpgsqlCommand(sql, conn, (NpgsqlTransaction)tx);
            cmd.Parameters.AddWithValue("universeId", universeId);

            var rowsAffected = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            
            if (rowsAffected > 0 && oldRootPlaceId.HasValue)
            {
                // Update the in_universe field for the old root place (set to false)
                const string updatePlaceSql = @"UPDATE assets 
                                              SET in_universe = false 
                                              WHERE asset_id = @oldRootPlaceId AND is_place = true";

                using var placeCmd = new NpgsqlCommand(updatePlaceSql, conn, (NpgsqlTransaction)tx);
                placeCmd.Parameters.AddWithValue("oldRootPlaceId", oldRootPlaceId.Value);
                await placeCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            tx.Commit();
            return rowsAffected > 0;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    /// <summary>
    /// Adds a place to the universe's place_ids array
    /// </summary>
    public static async Task<bool> AddPlaceToUniverseAsync(string connectionString, long universeId, long placeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("connectionString is required", nameof(connectionString));
        if (universeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(universeId));
        if (placeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(placeId));

        using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        using var tx = conn.BeginTransaction();

        try
        {
            // Add place to universe
            const string sql = @"UPDATE universes 
                                   SET place_ids = array_append(place_ids, @placeId)
                                   WHERE universe_id = @universeId 
                                   AND NOT (@placeId = ANY(place_ids))";

            using var cmd = new NpgsqlCommand(sql, conn, (NpgsqlTransaction)tx);
            cmd.Parameters.AddWithValue("placeId", placeId);
            cmd.Parameters.AddWithValue("universeId", universeId);

            var rowsAffected = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            
            if (rowsAffected > 0)
            {
                // Update the in_universe field for the place
                const string updatePlaceSql = @"UPDATE assets 
                                              SET in_universe = true 
                                              WHERE asset_id = @placeId AND is_place = true";

                using var placeCmd = new NpgsqlCommand(updatePlaceSql, conn, (NpgsqlTransaction)tx);
                placeCmd.Parameters.AddWithValue("placeId", placeId);
                await placeCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            tx.Commit();
            return rowsAffected > 0;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    /// <summary>
    /// Removes a place from the universe's place_ids array
    /// </summary>
    public static async Task<bool> RemovePlaceFromUniverseAsync(string connectionString, long universeId, long placeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("connectionString is required", nameof(connectionString));
        if (universeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(universeId));
        if (placeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(placeId));

        using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        using var tx = conn.BeginTransaction();

        try
        {
            // Remove place from universe
            const string sql = @"UPDATE universes 
                                   SET place_ids = array_remove(place_ids, @placeId)
                                   WHERE universe_id = @universeId";

            using var cmd = new NpgsqlCommand(sql, conn, (NpgsqlTransaction)tx);
            cmd.Parameters.AddWithValue("placeId", placeId);
            cmd.Parameters.AddWithValue("universeId", universeId);

            var rowsAffected = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            
            if (rowsAffected > 0)
            {
                // Update the in_universe field for the place (set to false since it's being removed from universe)
                const string updatePlaceSql = @"UPDATE assets 
                                              SET in_universe = false 
                                              WHERE asset_id = @placeId AND is_place = true";

                using var placeCmd = new NpgsqlCommand(updatePlaceSql, conn, (NpgsqlTransaction)tx);
                placeCmd.Parameters.AddWithValue("placeId", placeId);
                await placeCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

        tx.Commit();
        return rowsAffected > 0;
    }
    catch
    {
        tx.Rollback();
        throw;
    }
    }

    /// <summary>
    /// Gets all place IDs for a universe (excluding the root place if specified)
    /// </summary>
    public static async Task<long[]> GetUniversePlaceIdsAsync(string connectionString, long universeId, bool excludeRootPlace = false, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("connectionString is required", nameof(connectionString));
        if (universeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(universeId));

        using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"SELECT place_ids 
                             FROM universes 
                             WHERE universe_id = @universeId";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("universeId", universeId);

        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        
        if (result == null || result == DBNull.Value)
            return Array.Empty<long>();

        var placeIds = (long[])result;
        
        if (excludeRootPlace)
        {
            // Get root place ID and exclude it
            var rootPlaceId = await GetUniverseRootPlaceIdAsync(connectionString, universeId, cancellationToken);
            if (rootPlaceId.HasValue)
            {
                placeIds = placeIds.Where(id => id != rootPlaceId.Value).ToArray();
            }
        }

        return placeIds;
    }

    /// <summary>
    /// Gets the root place ID for a universe
    /// </summary>
    public static async Task<long?> GetUniverseRootPlaceIdAsync(string connectionString, long universeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("connectionString is required", nameof(connectionString));
        if (universeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(universeId));

        using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"SELECT root_place_id 
                               FROM universes 
                               WHERE universe_id = @universeId";

        Console.WriteLine($"DEBUG: GetUniverseRootPlaceIdAsync - Looking for universe {universeId}");
        Console.WriteLine($"DEBUG: SQL: {sql}");

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("universeId", universeId);

        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        
        Console.WriteLine($"DEBUG: GetUniverseRootPlaceIdAsync result: {result}");
        
        if (result == null || result == DBNull.Value)
            return null;
            
        return Convert.ToInt64(result);
    }

    /// <summary>
    /// Gets the creator/settings information for a place
    /// </summary>
    public static async Task<PlaceSettings?> GetPlaceSettingsAsync(string connectionString, long placeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("connectionString is required", nameof(connectionString));
        if (placeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(placeId));

        using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"SELECT a.owner_user_id, u.user_name
                             FROM assets a
                             LEFT JOIN users u ON a.owner_user_id = u.user_id
                             WHERE a.asset_id = @placeId AND a.is_place = true
                             LIMIT 1";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("placeId", placeId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var ownerId = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
            var ownerName = reader.IsDBNull(1) ? "Unknown" : reader.GetString(1);

            return new PlaceSettings
            {
                OwnerId = ownerId,
                OwnerName = ownerName,
                CreatorType = 0,
                CreatorTargetId = ownerId
            };
        }

        return null;
    }

    /// <summary>
    /// Resolves a template content hash from a template place ID, with fallback to .rbxl file processing.
    /// First checks the database for an existing content hash, then falls back to processing the template file.
    /// </summary>
    /// <param name="connectionString">Database connection string</param>
    /// <param name="templatePlaceId">Template place asset ID</param>
    /// <param name="assetsRoot">CDN assets root directory</param>
    /// <param name="templateFilePaths">Dictionary mapping template ID strings to full .rbxl file paths</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Content hash, or "pending-place-content" if unable to resolve</returns>
    public static async Task<string> ResolveTemplateContentHashAsync(
        string connectionString,
        long templatePlaceId,
        string assetsRoot,
        IReadOnlyDictionary<string, string> templateFilePaths,
        CancellationToken cancellationToken = default)
    {
        var hash = await GamesRepository.GetPlaceAssetHashAsync(connectionString, templatePlaceId, cancellationToken);
        if (!string.IsNullOrWhiteSpace(hash) && TemplateHelper.DoesTemplateExist(hash, assetsRoot))
            return hash;

        var templateIdStr = templatePlaceId.ToString();
        if (templateFilePaths.TryGetValue(templateIdStr, out var templatePath) && File.Exists(templatePath))
        {
            var processedHash = TemplateHelper.ProcessAndSaveTemplateAsync(templatePath, assetsRoot, cancellationToken);
            if (!string.IsNullOrWhiteSpace(processedHash))
                return processedHash;
        }

        return "pending-place-content";
    }
}
