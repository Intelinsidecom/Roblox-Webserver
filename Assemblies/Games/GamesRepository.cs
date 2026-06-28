using Npgsql;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Assets;
using Common;

namespace Games;

public static class GamesRepository
{
    public static async Task<string?> GetPlaceAssetHashAsync(string connectionString, long placeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("connectionString is required", nameof(connectionString));
        if (placeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(placeId));

        using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"SELECT content_hash 
                               FROM assets 
                               WHERE asset_id = @placeId AND is_place = true";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("placeId", placeId);

        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return result as string;
    }

    public static async Task<string?> GetPlaceGeneratedIconUrlAsync(string connectionString, long placeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("connectionString is required", nameof(connectionString));
        if (placeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(placeId));

        using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"SELECT place_generated_icon_url
                               FROM assets
                               WHERE asset_id = @placeId AND is_place = true";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("placeId", placeId);

        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return result as string;
    }

    public static async Task<long?> GetUniverseIdFromPlaceIdAsync(string connectionString, long placeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("connectionString is required", nameof(connectionString));
        if (placeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(placeId));

        using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"SELECT universe_id 
                               FROM universes 
                               WHERE @placeId = ANY(place_ids) 
                               OR root_place_id = @placeId";


        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("placeId", placeId);
        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        
        if (result == null || result == DBNull.Value)
            return null;
            
        return Convert.ToInt64(result);
    }

    public static async Task<bool> AddDeveloperProductToUniverseAsync(string connectionString, long universeId, JsonElement developerProduct, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("connectionString is required", nameof(connectionString));
        if (universeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(universeId));

        using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"UPDATE universes 
                               SET developer_products = developer_products || @developerProduct::jsonb
                               WHERE universe_id = @universeId";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("universeId", universeId);
        cmd.Parameters.AddWithValue("developerProduct", developerProduct.GetRawText());

        var rowsAffected = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        return rowsAffected > 0;
    }

    public static async Task<bool> UpdateDeveloperProductInUniverseAsync(string connectionString, long universeId, long developerProductId, JsonElement developerProduct, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("connectionString is required", nameof(connectionString));
        if (universeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(universeId));

        using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"UPDATE universes 
                               SET developer_products = 
                                   jsonb_set(
                                       developer_products, 
                                       (elem - 1)::text[], 
                                       @developerProduct::jsonb
                                   )
                               FROM jsonb_array_elements(developer_products) WITH ORDINALITY arr(elem, index)
                               WHERE universe_id = @universeId 
                                 AND elem->>'developerProductId' = @developerProductId";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("universeId", universeId);
        cmd.Parameters.AddWithValue("developerProductId", developerProductId);
        cmd.Parameters.AddWithValue("developerProduct", developerProduct.GetRawText());

        var rowsAffected = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        return rowsAffected > 0;
    }

    public static async Task<bool> RemoveDeveloperProductFromUniverseAsync(string connectionString, long universeId, long developerProductId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("connectionString is required", nameof(connectionString));
        if (universeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(universeId));

        using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"UPDATE universes 
                               SET developer_products = (
                                   SELECT jsonb_agg(elem)
                                   FROM jsonb_array_elements(developer_products) elem
                                   WHERE elem->>'developerProductId' != @developerProductId
                               )
                               WHERE universe_id = @universeId";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("universeId", universeId);
        cmd.Parameters.AddWithValue("developerProductId", developerProductId);

        var rowsAffected = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        return rowsAffected > 0;
    }

    public static async Task<List<JsonElement>?> GetUniverseDeveloperProductsAsync(string connectionString, long universeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("connectionString is required", nameof(connectionString));
        if (universeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(universeId));

        using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"SELECT developer_products 
                               FROM universes 
                               WHERE universe_id = @universeId";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("universeId", universeId);

        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        
        if (result == null || result == DBNull.Value)
            return null;

        var developerProductsJson = result.ToString();
        if (string.IsNullOrWhiteSpace(developerProductsJson) || developerProductsJson == "[]")
            return new List<JsonElement>();

        var developerProducts = JsonSerializer.Deserialize<List<JsonElement>>(developerProductsJson);
        return developerProducts;
    }

    public static async Task<long> GenerateUniverseDeveloperProductIdAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("connectionString is required", nameof(connectionString));

        using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"SELECT COALESCE(MAX((elem->>'developerProductId')::bigint), 0) + 1
                               FROM universes, jsonb_array_elements(developer_products) elem";

        using var cmd = new NpgsqlCommand(sql, conn);
        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        
        return Convert.ToInt64(result);
    }

    public static async Task<long?> GetUniverseOwnerAsync(string connectionString, long universeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("connectionString is required", nameof(connectionString));
        if (universeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(universeId));

        using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"SELECT creator_user_id 
                               FROM universes 
                               WHERE universe_id = @universeId";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("universeId", universeId);

        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        
        if (result == null || result == DBNull.Value)
            return null;
            
        return Convert.ToInt64(result);
    }

    public static async Task<UniverseInfo?> GetUniverseAsync(string connectionString, long universeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("connectionString is required", nameof(connectionString));
        if (universeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(universeId));

        using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"SELECT u.universe_id, u.name, u.creator_user_id, u.place_ids, u.privacy_level, u.Studio_Access_To_APIs, u.root_place_id, 
                                    COALESCE(u.visit_count, 0) as visit_count
                               FROM universes u
                               WHERE u.universe_id = @universeId";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("universeId", universeId);

        using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        
        if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var placeIds = reader.GetFieldValue<long[]>(3);
            var rootPlaceId = !reader.IsDBNull(6) ? reader.GetInt64(6) : (placeIds.Length > 0 ? placeIds[0] : 0);
            
            return new UniverseInfo
            {
                UniverseId = reader.GetInt64(0),
                Name = reader.GetString(1),
                CreatorUserId = reader.GetInt64(2),
                RootPlaceId = rootPlaceId,
                PrivacyLevel = reader.IsDBNull(4) ? 1 : reader.GetInt16(4),
                Studio_Access_To_APIs = reader.IsDBNull(5) ? false : reader.GetBoolean(5),
                VisitCount = reader.GetInt32(7),
                PlayingCount = 0,
                Description = null
            };
        }
        
        return null;
    }

    public static async Task<string?> GetPlaceDescriptionAsync(string connectionString, long placeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("connectionString is required", nameof(connectionString));
        if (placeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(placeId));

        using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"SELECT description 
                                FROM assets 
                                WHERE asset_id = @placeId AND is_place = true";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("placeId", placeId);

        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return result as string;
    }

    /// <summary>
    /// Gets the owner user ID for an asset. Returns null if the asset doesn't exist.
    /// </summary>
    public static async Task<long?> GetAssetOwnerAsync(string connectionString, long assetId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("connectionString is required", nameof(connectionString));
        if (assetId <= 0)
            throw new ArgumentOutOfRangeException(nameof(assetId));

        using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"SELECT owner_user_id FROM assets WHERE asset_id = @assetId";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("assetId", assetId);

        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        if (result == null || result == DBNull.Value)
            return null;

        return Convert.ToInt64(result);
    }

    /// <summary>
    /// Replaces a place asset with new file content. Computes the hash, saves the file,
    /// archives the old hash in version history, and updates the database.
    /// Returns (true, null) on success, or (false, errorMessage) on failure.
    /// </summary>
    public static async Task<(bool Success, string? Error)> ReplacePlaceAssetAsync(
        string connectionString,
        string assetsDirectory,
        long placeId,
        byte[] fileBytes,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return (false, "Connection string is required");
        if (string.IsNullOrWhiteSpace(assetsDirectory))
            return (false, "Assets directory not configured");
        if (placeId <= 0)
            return (false, "Invalid place ID");
        if (fileBytes == null || fileBytes.Length == 0)
            return (false, "Empty file body");

        // Verify the asset exists and is a place
        long? ownerId;
        string? currentHash;
        using (var conn = new NpgsqlConnection(connectionString))
        {
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = @"SELECT asset_id, content_hash, owner_user_id FROM assets WHERE asset_id = @placeId AND is_place = true";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("placeId", placeId);

            using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                return (false, $"Asset {placeId} not found or is not a place.");

            currentHash = reader.IsDBNull(1) ? null : reader.GetString(1);
            ownerId = reader.IsDBNull(2) ? (long?)null : reader.GetInt64(2);
        }

        var newHash = HashingUtilities.GenerateFileHash(fileBytes);

        if (string.Equals(currentHash, newHash, StringComparison.OrdinalIgnoreCase))
            return (true, null);

        // Save the file to the assets directory
        var assetFolder = Path.Combine(assetsDirectory, "asset");
        Directory.CreateDirectory(assetFolder);
        var fileName = newHash + ".rbxl";
        var filePath = Path.Combine(assetFolder, fileName);

        try
        {
            File.WriteAllBytes(filePath, fileBytes);
        }
        catch (Exception ex)
        {
            return (false, $"Failed to save file: {ex.Message}");
        }

        // Archive the old hash in version history (if there was one)
        if (!string.IsNullOrWhiteSpace(currentHash))
        {
            try
            {
                await VersionHistory.AddVersionEntryAsync(connectionString, placeId, currentHash, cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                // Version history failure should not block the upload
            }
        }

        // Update the asset record
        using (var conn = new NpgsqlConnection(connectionString))
        {
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string updateSql = @"
                UPDATE assets 
                SET content_hash = @newHash,
                    file_extension = '.rbxl',
                    content_type = 'application/octet-stream',
                    last_updated = CURRENT_TIMESTAMP
                WHERE asset_id = @placeId AND is_place = true";

            using var cmd = new NpgsqlCommand(updateSql, conn);
            cmd.Parameters.AddWithValue("newHash", newHash);
            cmd.Parameters.AddWithValue("placeId", placeId);

            var rowsAffected = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            if (rowsAffected == 0)
                return (false, $"Failed to update asset {placeId}.");
        }

        return (true, null);
    }
}
