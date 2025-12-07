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
        public string? HighResThumbnailUrl { get; set; }
        public string? Description { get; set; }
        public bool AssetImage { get; set; }
        public long? AssetLink { get; set; }
        public bool OnSale { get; set; }
        public long Price { get; set; }
        public bool AllowComments { get; set; }
        public int Genre { get; set; }
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
        public string? HighResThumbnailUrl { get; set; }
        public string? Description { get; set; }
        public bool AssetImage { get; set; }
        public long? AssetLink { get; set; }
    }

    public sealed class AssetsRepository
    {
        public async Task<long> CreateAssetAsync(string connectionString, AssetCreateParams p, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (p == null)
                throw new ArgumentNullException(nameof(p));

            if (string.IsNullOrWhiteSpace(p.Description))
            {
                if (p.AssetTypeId == 2)
                {
                    p.Description = "T-shirt";
                }
                else
                {
                    var label = AssetTypeNames.GetConfigureLabel(p.AssetTypeId);
                    if (!string.IsNullOrWhiteSpace(label))
                        p.Description = label;
                }
            }

            using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = @"insert into assets (
    name,
    asset_type_id,
    owner_user_id,
    content_hash,
    file_extension,
    content_type,
    thumbnail_url,
    high_res_thumbnail_url,
    description,
    asset_image,
    asset_link
) values (
    @name,
    @asset_type_id,
    @owner_user_id,
    @content_hash,
    @file_extension,
    @content_type,
    @thumbnail_url,
    @high_res_thumbnail_url,
    @description,
    @asset_image,
    @asset_link
) returning asset_id;";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("name", p.Name);
            cmd.Parameters.AddWithValue("asset_type_id", p.AssetTypeId);
            cmd.Parameters.AddWithValue("owner_user_id", p.OwnerUserId);
            cmd.Parameters.AddWithValue("content_hash", p.ContentHash);
            cmd.Parameters.AddWithValue("file_extension", (object?)p.FileExtension ?? DBNull.Value);
            cmd.Parameters.AddWithValue("content_type", (object?)p.ContentType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("thumbnail_url", (object?)p.ThumbnailUrl ?? DBNull.Value);
            cmd.Parameters.AddWithValue("high_res_thumbnail_url", (object?)p.HighResThumbnailUrl ?? DBNull.Value);
            cmd.Parameters.AddWithValue("description", (object?)p.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("asset_image", p.AssetImage);
            cmd.Parameters.AddWithValue("asset_link", (object?)p.AssetLink ?? DBNull.Value);

            var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            if (result == null || result == DBNull.Value)
                throw new InvalidOperationException("Failed to insert asset record.");

            return Convert.ToInt64(result);
        }

        public async Task UpdateAssetImageLinkAsync(string connectionString, long assetId, bool assetImage, long? assetLink, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (assetId <= 0)
                throw new ArgumentOutOfRangeException(nameof(assetId));

            using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = @"update assets
set asset_image = @asset_image,
    asset_link = @asset_link
where asset_id = @asset_id;";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("asset_id", assetId);
            cmd.Parameters.AddWithValue("asset_image", assetImage);
            cmd.Parameters.AddWithValue("asset_link", (object?)assetLink ?? DBNull.Value);

            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task UpdateAssetMetadataAsync(string connectionString, long assetId, string name, string? description, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (assetId <= 0)
                throw new ArgumentOutOfRangeException(nameof(assetId));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("name is required", nameof(name));

            using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = @"update assets
set name = @name,
    description = @description,
    last_updated = now()
where asset_id = @asset_id;";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("asset_id", assetId);
            cmd.Parameters.AddWithValue("name", name);
            cmd.Parameters.AddWithValue("description", (object?)description ?? DBNull.Value);

            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task UpdateAssetSaleAsync(string connectionString, long assetId, bool onSale, long price, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (assetId <= 0)
                throw new ArgumentOutOfRangeException(nameof(assetId));
            if (price < 0)
                throw new ArgumentOutOfRangeException(nameof(price));

            using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = @"update assets
set on_sale = @on_sale,
    price = @price,
    last_updated = now()
where asset_id = @asset_id;";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("asset_id", assetId);
            cmd.Parameters.AddWithValue("on_sale", onSale);
            cmd.Parameters.AddWithValue("price", price);

            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task UpdateAssetAllowCommentsAsync(string connectionString, long assetId, bool allowComments, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (assetId <= 0)
                throw new ArgumentOutOfRangeException(nameof(assetId));

            using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = @"update assets
set allow_comments = @allow_comments,
    last_updated = now()
where asset_id = @asset_id;";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("asset_id", assetId);
            cmd.Parameters.AddWithValue("allow_comments", allowComments);

            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task UpdateAssetGenreAsync(string connectionString, long assetId, int genre, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (assetId <= 0)
                throw new ArgumentOutOfRangeException(nameof(assetId));
            if (genre < 1 || genre > 15)
                throw new ArgumentOutOfRangeException(nameof(genre));

            using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = @"update assets
set genre = @genre,
    last_updated = now()
where asset_id = @asset_id;";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("asset_id", assetId);
            cmd.Parameters.AddWithValue("genre", genre);

            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
