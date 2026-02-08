using Npgsql;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

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

        Console.WriteLine($"DEBUG: GetUniverseIdFromPlaceIdAsync - Looking for place {placeId}");
        Console.WriteLine($"DEBUG: SQL: {sql}");

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("placeId", placeId);

        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        
        Console.WriteLine($"DEBUG: Query result: {result}");
        
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

        const string sql = @"SELECT universe_id, name, creator_user_id, place_ids, privacy_level, Studio_Access_To_APIs, root_place_id 
                               FROM universes 
                               WHERE universe_id = @universeId";

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
                PrivacyLevel = reader.IsDBNull(4) ? 1 : reader.GetInt16(4), // Default to Public if null
                Studio_Access_To_APIs = reader.IsDBNull(5) ? false : reader.GetBoolean(5), // Default to false if null
                Description = null // Description not in database schema yet
            };
        }
        
        return null;
    }
}
