using Npgsql;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Thumbnails
{
    /// <summary>
    /// Repository for managing place thumbnail cache in the generated_place_images table
    /// </summary>
    public class PlaceThumbnailCacheRepository
    {
        /// <summary>
        /// Tries to get cached thumbnail hashes for a place asset hash with specific dimensions
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="placeAssetHash">Hash of the place asset</param>
        /// <param name="width">Width of the thumbnail (null for any)</param>
        /// <param name="height">Height of the thumbnail (null for any)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Tuple containing (found, iconHash, thumbnailHash)</returns>
        public async Task<(bool found, string? iconHash, string? thumbnailHash)> TryGetAsync(
            string connectionString,
            string placeAssetHash,
            int? width,
            int? height,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string is required", nameof(connectionString));
            if (string.IsNullOrWhiteSpace(placeAssetHash))
                throw new ArgumentException("Place asset hash is required", nameof(placeAssetHash));

            // Build a simpler query that handles NULL values better
            string sql;
            if (width.HasValue && height.HasValue)
            {
                sql = @"
                    SELECT generated_icon_hash, generated_thumbnail_hash 
                    FROM generated_place_images 
                    WHERE place_asset_hash = @placeAssetHash 
                    AND width = @width AND height = @height
                    LIMIT 1";
            }
            else
            {
                sql = @"
                    SELECT generated_icon_hash, generated_thumbnail_hash 
                    FROM generated_place_images 
                    WHERE place_asset_hash = @placeAssetHash 
                    AND width IS NULL AND height IS NULL
                    LIMIT 1";
            }

            try
            {
                await using var conn = new NpgsqlConnection(connectionString);
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
                
                using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("placeAssetHash", placeAssetHash);
                
                if (width.HasValue && height.HasValue)
                {
                    cmd.Parameters.AddWithValue("width", width.Value);
                    cmd.Parameters.AddWithValue("height", height.Value);
                }
                
                using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                
                if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    var iconHash = reader["generated_icon_hash"] as string;
                    var thumbnailHash = reader["generated_thumbnail_hash"] as string;
                    
                    return (true, iconHash, thumbnailHash);
                }

                return (false, null, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CACHE ERROR] Database error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Inserts or updates cached thumbnail hashes for a place asset hash with specific dimensions
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="placeAssetHash">Hash of the place asset</param>
        /// <param name="iconHash">Hash of the generated icon</param>
        /// <param name="thumbnailHash">Hash of the generated thumbnail</param>
        /// <param name="width">Width of the thumbnail</param>
        /// <param name="height">Height of the thumbnail</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task UpsertAsync(
            string connectionString,
            string placeAssetHash,
            string iconHash,
            string thumbnailHash,
            int? width,
            int? height,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string is required", nameof(connectionString));
            if (string.IsNullOrWhiteSpace(placeAssetHash))
                throw new ArgumentException("Place asset hash is required", nameof(placeAssetHash));
            if (string.IsNullOrWhiteSpace(iconHash))
                throw new ArgumentException("Icon hash is required", nameof(iconHash));
            if (string.IsNullOrWhiteSpace(thumbnailHash))
                throw new ArgumentException("Thumbnail hash is required", nameof(thumbnailHash));

            const string sql = @"
                INSERT INTO generated_place_images (place_asset_hash, generated_icon_hash, generated_thumbnail_hash, width, height)
                VALUES (@placeAssetHash, @iconHash, @thumbnailHash, @width, @height)
                ON CONFLICT (place_asset_hash, width, height) 
                DO UPDATE SET 
                    generated_icon_hash = EXCLUDED.generated_icon_hash,
                    generated_thumbnail_hash = EXCLUDED.generated_thumbnail_hash,
                    updated_at = NOW()";

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("placeAssetHash", placeAssetHash);
            cmd.Parameters.AddWithValue("iconHash", iconHash);
            cmd.Parameters.AddWithValue("thumbnailHash", thumbnailHash);
            cmd.Parameters.AddWithValue("width", (object)width ?? DBNull.Value);
            cmd.Parameters.AddWithValue("height", (object)height ?? DBNull.Value);
            
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes cached entry for a place asset hash
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="placeAssetHash">Hash of the place asset</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task DeleteAsync(
            string connectionString,
            string placeAssetHash,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string is required", nameof(connectionString));
            if (string.IsNullOrWhiteSpace(placeAssetHash))
                throw new ArgumentException("Place asset hash is required", nameof(placeAssetHash));

            const string sql = @"
                DELETE FROM generated_place_images 
                WHERE place_asset_hash = @placeAssetHash";

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("placeAssetHash", placeAssetHash);
            
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Clears all cached entries (useful for maintenance)
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task ClearAllAsync(
            string connectionString,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string is required", nameof(connectionString));

            const string sql = "TRUNCATE TABLE generated_place_images";

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            
            using var cmd = new NpgsqlCommand(sql, conn);
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
