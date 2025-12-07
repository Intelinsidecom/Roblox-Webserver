using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Assets
{
    public sealed class AssetMetadataRepository
    {
        public async Task<AssetRecord?> GetAssetByIdAsync(string connectionString, long assetId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));

            using (var conn = new NpgsqlConnection(connectionString))
            {
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

                const string sql = @"select asset_id, name, asset_type_id, owner_user_id, content_hash, file_extension, content_type, thumbnail_url, high_res_thumbnail_url, description, asset_image, asset_link, on_sale, price, allow_comments, genre
from assets
where asset_id = @id";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("id", assetId);

                    using (var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                    {
                        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                            return null;

                        var record = new AssetRecord
                        {
                            AssetId = reader.GetInt64(0),
                            Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                            AssetTypeId = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                            OwnerUserId = reader.IsDBNull(3) ? 0 : reader.GetInt64(3),
                            ContentHash = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                            FileExtension = reader.IsDBNull(5) ? null : reader.GetString(5),
                            ContentType = reader.IsDBNull(6) ? null : reader.GetString(6),
                            ThumbnailUrl = reader.IsDBNull(7) ? null : reader.GetString(7),
                            HighResThumbnailUrl = reader.IsDBNull(8) ? null : reader.GetString(8),
                            Description = reader.IsDBNull(9) ? null : reader.GetString(9),
                            AssetImage = !reader.IsDBNull(10) && reader.GetBoolean(10),
                            AssetLink = reader.IsDBNull(11) ? (long?)null : reader.GetInt64(11),
                            OnSale = !reader.IsDBNull(12) && reader.GetBoolean(12),
                            Price = reader.IsDBNull(13) ? 0L : reader.GetInt64(13),
                            AllowComments = reader.IsDBNull(14) || reader.GetBoolean(14),
                            Genre = reader.IsDBNull(15) ? 1 : reader.GetInt32(15)
                        };

                        return record;
                    }
                }
            }
        }
    }
}
