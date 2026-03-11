using Npgsql;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Thumbnails;

public static class UniverseThumbnailQueries
{
    /// <summary>
    /// Updates the thumbnail URL for a universe in the database.
    /// </summary>
    public static async Task SetUniverseThumbnailUrlAsync(
        string connectionString,
        long universeId,
        string thumbnailUrl,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));
        if (universeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(universeId));
        if (string.IsNullOrWhiteSpace(thumbnailUrl))
            throw new ArgumentException("Thumbnail URL is required", nameof(thumbnailUrl));

        const string sql = @"UPDATE universes 
                           SET thumbnail_url = @thumbnailUrl 
                           WHERE universe_id = @universeId";

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("thumbnailUrl", thumbnailUrl);
        cmd.Parameters.AddWithValue("universeId", universeId);
        
        var rowsAffected = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        
        if (rowsAffected == 0)
            throw new InvalidOperationException($"Universe with ID {universeId} not found");
    }

    /// <summary>
    /// Updates the thumbnail URL for a place asset in the database.
    /// </summary>
    public static async Task SetPlaceThumbnailUrlAsync(
        string connectionString,
        long placeId,
        string thumbnailUrl,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));
        if (placeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(placeId));
        if (string.IsNullOrWhiteSpace(thumbnailUrl))
            throw new ArgumentException("Thumbnail URL is required", nameof(thumbnailUrl));

        const string sql = @"UPDATE assets 
                           SET thumbnail_url = @thumbnailUrl 
                           WHERE asset_id = @placeId AND is_place = true";

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("thumbnailUrl", thumbnailUrl);
        cmd.Parameters.AddWithValue("placeId", placeId);
        
        var rowsAffected = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        
        if (rowsAffected == 0)
            throw new InvalidOperationException($"Place asset with ID {placeId} not found");
    }

    /// <summary>
    /// Updates the generated icon URL and hash for a place asset in the database.
    /// </summary>
    public static async Task SetPlaceGeneratedIconAsync(
        string connectionString,
        long placeId,
        string iconUrl,
        string iconHash,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));
        if (placeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(placeId));
        if (string.IsNullOrWhiteSpace(iconUrl))
            throw new ArgumentException("Icon URL is required", nameof(iconUrl));
        if (string.IsNullOrWhiteSpace(iconHash))
            throw new ArgumentException("Icon hash is required", nameof(iconHash));

        const string sql = @"UPDATE assets 
                           SET generated_icon = true,
                               place_generated_icon_url = @iconUrl,
                               place_generated_icon_hash = @iconHash
                           WHERE asset_id = @placeId AND is_place = true";

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("iconUrl", iconUrl);
        cmd.Parameters.AddWithValue("iconHash", iconHash);
        cmd.Parameters.AddWithValue("placeId", placeId);
        
        var rowsAffected = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        
        if (rowsAffected == 0)
            throw new InvalidOperationException($"Place asset with ID {placeId} not found");
    }

    /// <summary>
    /// Updates high-resolution generated icon URL and hash for a place asset in the database.
    /// </summary>
    public static async Task SetPlaceGeneratedIconHighResAsync(
        string connectionString,
        long placeId,
        string iconUrlHighRes,
        string iconHashHighRes,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));
        if (placeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(placeId));
        if (string.IsNullOrWhiteSpace(iconUrlHighRes))
            throw new ArgumentException("High-res icon URL is required", nameof(iconUrlHighRes));
        if (string.IsNullOrWhiteSpace(iconHashHighRes))
            throw new ArgumentException("High-res icon hash is required", nameof(iconHashHighRes));

        const string sql = @"UPDATE assets 
                           SET place_generated_icon_high_res_url = @iconUrlHighRes
                           WHERE asset_id = @placeId AND is_place = true";

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("iconUrlHighRes", iconUrlHighRes);
        cmd.Parameters.AddWithValue("placeId", placeId);
        
        var rowsAffected = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        
        if (rowsAffected == 0)
            throw new InvalidOperationException($"Place asset with ID {placeId} not found");
    }
}
