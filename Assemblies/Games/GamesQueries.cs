using Npgsql;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Games;

/// <summary>
/// Database query helpers for games and universe operations
/// This class contains SQL helpers that were moved from PlacesController to better organize code
/// </summary>
public static class GamesQueries
{
    /// <summary>
    /// Gets the next place number for a user (for auto-generating place names)
    /// </summary>
    public static async Task<int> GetNextPlaceNumberAsync(long userId, string connectionString, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));
        if (userId <= 0)
            throw new ArgumentOutOfRangeException(nameof(userId));

        const string maxPlaceSeqSql = @"select coalesce(count(*), 0) from assets where owner_user_id = @uid and is_place = true;";

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        using var seqCmd = new NpgsqlCommand(maxPlaceSeqSql, conn);
        seqCmd.Parameters.AddWithValue("uid", userId);
        
        var obj = await seqCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        var existingCount = Convert.ToInt32(obj ?? 0);
        
        return existingCount + 1;
    }

    /// <summary>
    /// Updates the content hash for a place asset
    /// </summary>
    public static async Task UpdatePlaceAssetContentHashAsync(long placeId, string contentHash, string connectionString, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));
        if (placeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(placeId));
        if (string.IsNullOrWhiteSpace(contentHash))
            throw new ArgumentException("Content hash is required", nameof(contentHash));

        const string updateSql = @"UPDATE assets SET content_hash = @contentHash WHERE asset_id = @placeId";

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        using var cmd = new NpgsqlCommand(updateSql, conn);
        cmd.Parameters.AddWithValue("contentHash", contentHash);
        cmd.Parameters.AddWithValue("placeId", placeId);
        
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the first place ID from a universe (typically the root place)
    /// Note: This is different from GamesRepository.GetUniverseIdFromPlaceIdAsync which gets universe ID from place ID
    /// </summary>
    public static async Task<long> GetFirstPlaceIdFromUniverseAsync(long universeId, string connectionString, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));
        if (universeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(universeId));

        using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync().ConfigureAwait(false);

        const string sql = @"SELECT place_ids[1] 
                           FROM universes 
                           WHERE universe_id = @universeId AND place_ids IS NOT NULL AND array_length(place_ids, 1) > 0";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("universeId", universeId);

        var result = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
        
        if (result == null || result == DBNull.Value)
        {
            throw new InvalidOperationException($"Universe {universeId} does not have any places");
        }

        return Convert.ToInt64(result);
    }
}
