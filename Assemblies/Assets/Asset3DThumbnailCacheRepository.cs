using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Assets
{
    public sealed class Asset3DThumbnailCacheRepository
    {
        public sealed class CacheEntry
        {
            public string ModelHash { get; set; } = string.Empty;
            public string ObjFileName { get; set; } = string.Empty;
            public string MtlFileName { get; set; } = string.Empty;
            public int Width { get; set; }
            public int Height { get; set; }
        }

        public async Task<(bool found, CacheEntry? entry)> TryGetAsync(string connectionString, long assetId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (assetId <= 0)
                throw new ArgumentException("assetId must be positive", nameof(assetId));

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = "select model_hash, obj_file_name, mtl_file_name, width, height from asset_3d_cache where asset_id = @id";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", assetId);
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
            long assetId,
            string modelHash,
            string objFileName,
            string mtlFileName,
            int width,
            int height,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (assetId <= 0)
                throw new ArgumentException("assetId must be positive", nameof(assetId));
            if (string.IsNullOrWhiteSpace(modelHash))
                throw new ArgumentException("modelHash is required", nameof(modelHash));
            if (string.IsNullOrWhiteSpace(objFileName))
                throw new ArgumentException("objFileName is required", nameof(objFileName));
            if (string.IsNullOrWhiteSpace(mtlFileName))
                throw new ArgumentException("mtlFileName is required", nameof(mtlFileName));

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = @"insert into asset_3d_cache(asset_id, model_hash, obj_file_name, mtl_file_name, width, height)
values(@id, @model, @obj, @mtl, @w, @hgt)
on conflict (asset_id) do update set
    model_hash    = excluded.model_hash,
    obj_file_name = excluded.obj_file_name,
    mtl_file_name = excluded.mtl_file_name,
    width         = excluded.width,
    height        = excluded.height,
    created_at    = now();";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", assetId);
            cmd.Parameters.AddWithValue("model", modelHash);
            cmd.Parameters.AddWithValue("obj", objFileName);
            cmd.Parameters.AddWithValue("mtl", mtlFileName);
            cmd.Parameters.AddWithValue("w", width);
            cmd.Parameters.AddWithValue("hgt", height);

            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
