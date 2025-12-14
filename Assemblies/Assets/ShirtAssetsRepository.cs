using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Assets
{
    public sealed class ShirtAssetsRepository
    {
        public sealed class UserShirtItem
        {
            public long AssetId { get; set; }
            public string Name { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
            public string? ThumbnailUrl { get; set; }
            public long? ImageAssetId { get; set; }
        }

        public async Task<IReadOnlyList<UserShirtItem>> GetUserShirtsWithImagesAsync(string connectionString, long userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId));

            var results = new List<UserShirtItem>();

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = @"select a.asset_id,
       a.name,
       a.created_at,
       a.thumbnail_url,
       i.asset_id as image_asset_id
from user_assets ua
join assets a on a.asset_id = ua.asset_id and a.asset_type_id = 11
left join assets i on i.owner_user_id = a.owner_user_id
                  and i.asset_type_id = 1
                  and i.name = a.name || ' Image'
where ua.user_id = @uid
order by ua.created_at desc, a.asset_id desc
limit 50;";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("uid", userId);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var item = new UserShirtItem
                {
                    AssetId = reader.GetInt64(0),
                    Name = reader.IsDBNull(1) ? "Unnamed" : reader.GetString(1),
                    CreatedAt = reader.GetDateTime(2),
                    ThumbnailUrl = reader.IsDBNull(3) ? null : reader.GetString(3),
                    ImageAssetId = reader.IsDBNull(4) ? (long?)null : reader.GetInt64(4)
                };

                results.Add(item);
            }

            return results;
        }
    }
}
