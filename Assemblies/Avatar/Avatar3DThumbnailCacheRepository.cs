using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Avatar
{
    public sealed class Avatar3DThumbnailCacheRepository
    {
        public sealed class CacheEntry
        {
            public string ModelHash { get; set; } = string.Empty;
            public string ObjFileName { get; set; } = string.Empty;
            public string MtlFileName { get; set; } = string.Empty;
            public int Width { get; set; }
            public int Height { get; set; }
        }

        public async Task<(bool found, CacheEntry? entry)> TryGetAsync(string connectionString, string configHash, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (string.IsNullOrWhiteSpace(configHash))
                throw new ArgumentException("configHash is required", nameof(configHash));

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = "select model_hash, obj_file_name, mtl_file_name, width, height from avatar_3d_cache where config_hash = @h";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("h", configHash);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                return (false, null);

            var entry = new CacheEntry
            {
                ModelHash = reader.GetString(0),
                ObjFileName = reader.GetString(1),
                MtlFileName = reader.GetString(2),
                Width = reader.GetInt32(3),
                Height = reader.GetInt32(4)
            };

            return (true, entry);
        }

        public async Task UpsertAsync(
            string connectionString,
            string configHash,
            string modelHash,
            string objFileName,
            string mtlFileName,
            int width,
            int height,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (string.IsNullOrWhiteSpace(configHash))
                throw new ArgumentException("configHash is required", nameof(configHash));
            if (string.IsNullOrWhiteSpace(modelHash))
                throw new ArgumentException("modelHash is required", nameof(modelHash));
            if (string.IsNullOrWhiteSpace(objFileName))
                throw new ArgumentException("objFileName is required", nameof(objFileName));
            if (string.IsNullOrWhiteSpace(mtlFileName))
                throw new ArgumentException("mtlFileName is required", nameof(mtlFileName));

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = @"insert into avatar_3d_cache(config_hash, model_hash, obj_file_name, mtl_file_name, width, height)
values(@h, @model, @obj, @mtl, @w, @hgt)
on conflict (config_hash) do update set
    model_hash   = excluded.model_hash,
    obj_file_name = excluded.obj_file_name,
    mtl_file_name = excluded.mtl_file_name,
    width        = excluded.width,
    height       = excluded.height,
    created_at   = now();";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("h", configHash);
            cmd.Parameters.AddWithValue("model", modelHash);
            cmd.Parameters.AddWithValue("obj", objFileName);
            cmd.Parameters.AddWithValue("mtl", mtlFileName);
            cmd.Parameters.AddWithValue("w", width);
            cmd.Parameters.AddWithValue("hgt", height);

            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
