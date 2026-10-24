using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Avatar
{
    public sealed class AvatarThumbnailCacheEntry
    {
        public string ConfigHash { get; set; } = "";
        public string FileName { get; set; } = "";
        public string ImageHash { get; set; } = "";
        public string RenderType { get; set; } = "";
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public sealed class AvatarThumbnailCacheRepository
    {
        public async Task<(bool found, string? fileName)> TryGetAsync(string connectionString, string configHash, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (string.IsNullOrWhiteSpace(configHash))
                throw new ArgumentException("configHash is required", nameof(configHash));

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = "select file_name from avatar_thumbnail_cache where config_hash = @h";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("h", configHash);
            var obj = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            if (obj == null || obj == DBNull.Value)
                return (false, null);

            return (true, Convert.ToString(obj));
        }

        public async Task UpsertAsync(
            string connectionString,
            string configHash,
            string imageHash,
            string fileName,
            string renderType,
            int width,
            int height,
            long? userId = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (string.IsNullOrWhiteSpace(configHash))
                throw new ArgumentException("configHash is required", nameof(configHash));
            if (string.IsNullOrWhiteSpace(imageHash))
                throw new ArgumentException("imageHash is required", nameof(imageHash));
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("fileName is required", nameof(fileName));
            if (string.IsNullOrWhiteSpace(renderType))
                throw new ArgumentException("renderType is required", nameof(renderType));

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = @"insert into avatar_thumbnail_cache(config_hash, image_hash, file_name, render_type, width, height, user_id)
values(@h, @img, @file, @type, @w, @hgt, @uid)
on conflict (config_hash) do update set
    image_hash = excluded.image_hash,
    file_name  = excluded.file_name,
    render_type = excluded.render_type,
    width      = excluded.width,
    height     = excluded.height,
    user_id    = excluded.user_id,
    created_at = now();";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("h", configHash);
            cmd.Parameters.AddWithValue("img", imageHash);
            cmd.Parameters.AddWithValue("file", fileName);
            cmd.Parameters.AddWithValue("type", renderType);
            cmd.Parameters.AddWithValue("w", width);
            cmd.Parameters.AddWithValue("hgt", height);
            cmd.Parameters.AddWithValue("uid", (object?)userId ?? DBNull.Value);

            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<AvatarThumbnailCacheEntry>> GetUserEntriesAsync(string connectionString, long userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId));

            var entries = new List<AvatarThumbnailCacheEntry>();

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = @"select config_hash, file_name, image_hash, render_type, width, height
from avatar_thumbnail_cache
where user_id = @uid";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("uid", userId);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                entries.Add(new AvatarThumbnailCacheEntry
                {
                    ConfigHash = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                    FileName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    ImageHash = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    RenderType = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    Width = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                    Height = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                });
            }

            return entries;
        }

        public async Task<int> CountRowsByConfigHashAsync(string connectionString, string configHash, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (string.IsNullOrWhiteSpace(configHash))
                throw new ArgumentException("configHash is required", nameof(configHash));

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = "select count(*) from avatar_thumbnail_cache where config_hash = @h";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("h", configHash);
            var obj = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            if (obj == null || obj == DBNull.Value)
                return 0;
            return Convert.ToInt32(obj);
        }

        public async Task DeleteByConfigHashAsync(string connectionString, string configHash, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (string.IsNullOrWhiteSpace(configHash))
                throw new ArgumentException("configHash is required", nameof(configHash));

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var cmd = new NpgsqlCommand("delete from avatar_thumbnail_cache where config_hash = @h", conn);
            cmd.Parameters.AddWithValue("h", configHash);
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task WipeUserCacheAsync(string connectionString, long userId, string? outputDirectory = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId));

            if (string.IsNullOrWhiteSpace(outputDirectory))
                outputDirectory = Path.Combine(AppContext.BaseDirectory, "thumbnails");

            var entries = await GetUserEntriesAsync(connectionString, userId, cancellationToken).ConfigureAwait(false);

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            foreach (var entry in entries)
            {
                var count = await CountRowsByConfigHashAsync(connectionString, entry.ConfigHash, cancellationToken).ConfigureAwait(false);
                if (count <= 1 && !string.IsNullOrWhiteSpace(entry.FileName))
                {
                    try
                    {
                        var filePath = Path.Combine(outputDirectory!, entry.FileName);
                        if (System.IO.File.Exists(filePath))
                            System.IO.File.Delete(filePath);
                    }
                    catch
                    {
                        // File deletion is best-effort; continue with DB cleanup.
                    }
                }

                await DeleteByConfigHashAsync(connectionString, entry.ConfigHash, cancellationToken).ConfigureAwait(false);
            }

            using (var cmd = new NpgsqlCommand("delete from avatar_3d_cache where config_hash = any(@hashes)", conn))
            {
                var hashes = entries.Select(e => e.ConfigHash).Distinct().ToArray();
                if (hashes.Length > 0)
                {
                    cmd.Parameters.AddWithValue("hashes", hashes);
                    await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                }
            }

            using (var cmd = new NpgsqlCommand(@"
                update users
                set avatar_render_urls     = (coalesce(avatar_render_urls, '{}'::jsonb) - 'headshot') - 'avatar',
                    thumbnail_url          = null,
                    avatar_thumbnail_url   = null,
                    headshot_url           = null,
                    headshot_thumbnail_url = null,
                    avatar_state_hash      = null,
                    last_avatar_thumbnail  = null
                where user_id = @id", conn))
            {
                cmd.Parameters.AddWithValue("id", userId);
                await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
