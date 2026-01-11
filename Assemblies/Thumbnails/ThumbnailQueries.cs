using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Thumbnails
{
    public static class ThumbnailQueries
    {
        public static async Task<string?> GetUserHeadshotUrlAsync(string connectionString, long userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId));

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            using var cmd = new NpgsqlCommand(
                "select coalesce(avatar_render_urls->>'headshot', headshot_url, headshot_thumbnail_url) from users where user_id = @id",
                conn);
            cmd.Parameters.AddWithValue("id", userId);
            var obj = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            return obj is string s && !string.IsNullOrWhiteSpace(s) ? s : null;
        }

        public static async Task<string?> GetUserThumbnailUrlAsync(string connectionString, long userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId));

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            using var cmd = new NpgsqlCommand(
                "select coalesce(avatar_render_urls->>'avatar', thumbnail_url, avatar_thumbnail_url) from users where user_id = @id",
                conn);
            cmd.Parameters.AddWithValue("id", userId);
            var obj = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            return obj is string s && !string.IsNullOrWhiteSpace(s) ? s : null;
        }

        public static async Task<Dictionary<long, string?>> GetUserThumbnailUrlsAsync(string connectionString, long[] userIds, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            var result = new Dictionary<long, string?>();
            if (userIds == null || userIds.Length == 0)
                return result;

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            using var cmd = new NpgsqlCommand(
                "select user_id, coalesce(avatar_render_urls->>'avatar', thumbnail_url, avatar_thumbnail_url) as url from users where user_id = ANY(@ids)",
                conn);
            cmd.Parameters.AddWithValue("ids", userIds);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var id = reader.GetInt64(0);
                var url = reader.IsDBNull(1) ? null : reader.GetString(1);
                result[id] = url;
            }
            return result;
        }

        public static async Task<Dictionary<long, string?>> GetUserHeadshotUrlsAsync(string connectionString, long[] userIds, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            var result = new Dictionary<long, string?>();
            if (userIds == null || userIds.Length == 0)
                return result;

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            using var cmd = new NpgsqlCommand(
                "select user_id, coalesce(avatar_render_urls->>'headshot', headshot_url, headshot_thumbnail_url) as url from users where user_id = ANY(@ids)",
                conn);
            cmd.Parameters.AddWithValue("ids", userIds);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var id = reader.GetInt64(0);
                var url = reader.IsDBNull(1) ? null : reader.GetString(1);
                result[id] = url;
            }
            return result;
        }

        public static async Task SetUserHeadshotUrlAsync(string connectionString, long userId, string url, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId));
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            using var cmd = new NpgsqlCommand(
                @"update users
                  set avatar_render_urls = coalesce(avatar_render_urls, '{}'::jsonb) || jsonb_build_object('headshot', @u),
                      headshot_url       = @u,
                      headshot_thumbnail_url = coalesce(headshot_thumbnail_url, @u)
                  where user_id = @id",
                conn);
            cmd.Parameters.AddWithValue("u", (object?)url ?? DBNull.Value);
            cmd.Parameters.AddWithValue("id", userId);
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        public static async Task SetUserThumbnailUrlAsync(string connectionString, long userId, string url, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId));
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            using var cmd = new NpgsqlCommand(
                @"update users
                  set avatar_render_urls    = (coalesce(avatar_render_urls, '{}'::jsonb) - 'headshot') || jsonb_build_object('avatar', @u),
                      thumbnail_url         = @u,
                      avatar_thumbnail_url  = coalesce(avatar_thumbnail_url, @u),
                      headshot_url          = null,
                      headshot_thumbnail_url = null
                  where user_id = @id",
                conn);
            cmd.Parameters.AddWithValue("u", (object?)url ?? DBNull.Value);
            cmd.Parameters.AddWithValue("id", userId);
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        public static async Task UpdateAssetThumbnailUrlsAsync(string connectionString, long assetId, string thumbnailUrl, string? highResThumbnailUrl = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string is required", nameof(connectionString));
            if (assetId <= 0)
                throw new ArgumentOutOfRangeException(nameof(assetId));
            if (string.IsNullOrWhiteSpace(thumbnailUrl))
                throw new ArgumentException("Thumbnail URL is required", nameof(thumbnailUrl));

            const string sql = @"UPDATE assets 
                               SET thumbnail_url = @thumbnailUrl,
                                   high_res_thumbnail_url = @highResThumbnailUrl
                               WHERE asset_id = @assetId AND is_place = true";

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("thumbnailUrl", thumbnailUrl);
            cmd.Parameters.AddWithValue("highResThumbnailUrl", highResThumbnailUrl ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("assetId", assetId);
            
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        public static async Task SetPlaceGeneratedIconFlagsAsync(string connectionString, long placeId, string iconUrl, string iconUrlHighRes, string iconHash, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string is required", nameof(connectionString));
            if (placeId <= 0)
                throw new ArgumentOutOfRangeException(nameof(placeId));
            if (string.IsNullOrWhiteSpace(iconUrl))
                throw new ArgumentException("Icon URL is required", nameof(iconUrl));

            const string sql = @"UPDATE assets 
                               SET generated_icon = true,
                                   place_generated_icon_url = @iconUrl,
                                   place_generated_icon_high_res_url = @iconUrlHighRes,
                                   place_generated_icon_hash = @iconHash,
                                   custom_icon = false
                               WHERE asset_id = @placeId AND is_place = true";

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("iconUrl", iconUrl);
            cmd.Parameters.AddWithValue("iconUrlHighRes", iconUrlHighRes);
            cmd.Parameters.AddWithValue("iconHash", iconHash);
            cmd.Parameters.AddWithValue("placeId", placeId);
            
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        public static async Task ClearPlaceGeneratedIconAsync(string connectionString, long placeId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string is required", nameof(connectionString));
            if (placeId <= 0)
                throw new ArgumentOutOfRangeException(nameof(placeId));

            const string sql = @"UPDATE assets 
                               SET generated_icon = false,
                                   place_generated_icon_url = NULL,
                                   place_generated_icon_high_res_url = NULL,
                                   place_generated_icon_hash = NULL
                               WHERE asset_id = @placeId AND is_place = true";

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("placeId", placeId);
            
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        public static async Task ClearPlaceCustomIconAsync(string connectionString, long placeId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string is required", nameof(connectionString));
            if (placeId <= 0)
                throw new ArgumentOutOfRangeException(nameof(placeId));

            const string sql = @"UPDATE assets 
                               SET custom_icon = false,
                                   place_custom_icon_url = NULL,
                                   place_custom_icon_high_res_url = NULL,
                                   place_custom_icon_hash = NULL
                               WHERE asset_id = @placeId AND is_place = true";

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("placeId", placeId);
            
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        public static async Task SetPlaceCustomThumbnailFlagAsync(string connectionString, long placeId, bool isCustom, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string is required", nameof(connectionString));
            if (placeId <= 0)
                throw new ArgumentOutOfRangeException(nameof(placeId));

            const string sql = @"UPDATE assets 
                               SET place_custom_thumbnail = @isCustom,
                place_autogenerated_thumbnail = false,
                place_video_thumbnail = false
                               WHERE asset_id = @placeId AND is_place = true";

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("isCustom", isCustom);
            cmd.Parameters.AddWithValue("placeId", placeId);
            
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        public static async Task SetPlaceVideoThumbnailFlagAsync(string connectionString, long placeId, bool isVideo, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string is required", nameof(connectionString));
            if (placeId <= 0)
                throw new ArgumentOutOfRangeException(nameof(placeId));

            const string sql = @"UPDATE assets 
                               SET place_video_thumbnail = @isVideo
                               WHERE asset_id = @placeId AND is_place = true";

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("isVideo", isVideo);
            cmd.Parameters.AddWithValue("placeId", placeId);
            
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        public static async Task SetPlaceAutoGeneratedThumbnailFlagAsync(string connectionString, long placeId, bool isAutoGenerated, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string is required", nameof(connectionString));
            if (placeId <= 0)
                throw new ArgumentOutOfRangeException(nameof(placeId));

            const string sql = @"UPDATE assets 
                               SET place_autogenerated_thumbnail = @isAutoGenerated
                               WHERE asset_id = @placeId AND is_place = true";

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("isAutoGenerated", isAutoGenerated);
            cmd.Parameters.AddWithValue("placeId", placeId);
            
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        public static async Task ClearPlaceVideoThumbnailAsync(string connectionString, long placeId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string is required", nameof(connectionString));
            if (placeId <= 0)
                throw new ArgumentOutOfRangeException(nameof(placeId));

            const string sql = @"UPDATE assets 
                               SET place_video_thumbnail = false
                               WHERE asset_id = @placeId AND is_place = true";

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("placeId", placeId);
            
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        public static async Task ClearPlaceAutoGeneratedThumbnailAsync(string connectionString, long placeId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string is required", nameof(connectionString));
            if (placeId <= 0)
                throw new ArgumentOutOfRangeException(nameof(placeId));

            const string sql = @"UPDATE assets 
                               SET place_autogenerated_thumbnail = false
                               WHERE asset_id = @placeId AND is_place = true";

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("placeId", placeId);
            
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        public static async Task ClearPlaceCustomThumbnailAsync(string connectionString, long placeId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string is required", nameof(connectionString));
            if (placeId <= 0)
                throw new ArgumentOutOfRangeException(nameof(placeId));

            const string sql = @"UPDATE assets 
                               SET place_custom_thumbnail = false
                               WHERE asset_id = @placeId AND is_place = true";

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("placeId", placeId);
            
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
