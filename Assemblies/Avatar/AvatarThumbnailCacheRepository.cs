using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Avatar
{
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
            await using var cmd = new NpgsqlCommand(sql, conn);
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

            const string sql = @"insert into avatar_thumbnail_cache(config_hash, image_hash, file_name, render_type, width, height)
values(@h, @img, @file, @type, @w, @hgt)
on conflict (config_hash) do update set
    image_hash = excluded.image_hash,
    file_name  = excluded.file_name,
    render_type = excluded.render_type,
    width      = excluded.width,
    height     = excluded.height,
    created_at = now();";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("h", configHash);
            cmd.Parameters.AddWithValue("img", imageHash);
            cmd.Parameters.AddWithValue("file", fileName);
            cmd.Parameters.AddWithValue("type", renderType);
            cmd.Parameters.AddWithValue("w", width);
            cmd.Parameters.AddWithValue("hgt", height);

            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
