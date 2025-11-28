using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Assets
{
    public sealed class AssetRecord
    {
        public long AssetId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int AssetTypeId { get; set; }
        public long OwnerUserId { get; set; }
        public string ContentHash { get; set; } = string.Empty;
        public string? FileExtension { get; set; }
        public string? ContentType { get; set; }
        public string? ThumbnailUrl { get; set; }
    }

    public sealed class AssetCreateParams
    {
        public string Name { get; set; } = string.Empty;
        public int AssetTypeId { get; set; }
        public long OwnerUserId { get; set; }
        public string ContentHash { get; set; } = string.Empty;
        public string? FileExtension { get; set; }
        public string? ContentType { get; set; }
        public string? ThumbnailUrl { get; set; }
    }

    public sealed class AssetsRepository
    {
        public async Task<long> CreateAssetAsync(string connectionString, AssetCreateParams p, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (p == null)
                throw new ArgumentNullException(nameof(p));

            using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = @"insert into assets (
    name,
    asset_type_id,
    owner_user_id,
    content_hash,
    file_extension,
    content_type,
    thumbnail_url
) values (
    @name,
    @asset_type_id,
    @owner_user_id,
    @content_hash,
    @file_extension,
    @content_type,
    @thumbnail_url
) returning asset_id;";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("name", p.Name);
            cmd.Parameters.AddWithValue("asset_type_id", p.AssetTypeId);
            cmd.Parameters.AddWithValue("owner_user_id", p.OwnerUserId);
            cmd.Parameters.AddWithValue("content_hash", p.ContentHash);
            cmd.Parameters.AddWithValue("file_extension", (object?)p.FileExtension ?? DBNull.Value);
            cmd.Parameters.AddWithValue("content_type", (object?)p.ContentType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("thumbnail_url", (object?)p.ThumbnailUrl ?? DBNull.Value);

            var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            if (result == null || result == DBNull.Value)
                throw new InvalidOperationException("Failed to insert asset record.");

            return Convert.ToInt64(result);
        }
    }
}
