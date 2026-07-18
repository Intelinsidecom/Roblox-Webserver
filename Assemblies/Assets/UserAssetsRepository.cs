using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Assets
{
    public sealed class UserAssetsRepository
    {
        public async Task AddUserAssetAsync(string connectionString, long userId, long assetId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId));
            if (assetId <= 0)
                throw new ArgumentOutOfRangeException(nameof(assetId));

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = @"insert into user_assets (user_id, asset_id)
values (@user_id, @asset_id)
on conflict (user_id, asset_id) do nothing;";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("user_id", userId);
            cmd.Parameters.AddWithValue("asset_id", assetId);

            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task RemoveUserAssetAsync(string connectionString, long userId, long assetId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId));
            if (assetId <= 0)
                throw new ArgumentOutOfRangeException(nameof(assetId));

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = @"delete from user_assets where user_id = @user_id and asset_id = @asset_id";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("user_id", userId);
            cmd.Parameters.AddWithValue("asset_id", assetId);

            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> UserOwnsAssetAsync(string connectionString, long userId, long assetId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId));
            if (assetId <= 0)
                throw new ArgumentOutOfRangeException(nameof(assetId));

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = @"select 1 from user_assets where user_id = @user_id and asset_id = @asset_id limit 1";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("user_id", userId);
            cmd.Parameters.AddWithValue("asset_id", assetId);

            var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            return result != null;
        }

        public sealed class UserClothingItem
        {
            public long AssetId { get; set; }
            public string Name { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
            public string? ThumbnailUrl { get; set; }
            public long? ImageAssetId { get; set; }
            public long Sales { get; set; }
        }

        public async Task<IReadOnlyList<UserClothingItem>> GetUserPantsWithImagesAsync(string connectionString, long userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId));

            var results = new List<UserClothingItem>();

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = @"select a.asset_id,
       a.name,
       a.created_at,
       a.thumbnail_url,
       i.asset_id as image_asset_id,
       COALESCE(a.sales, 0) as sales
from user_assets ua
join assets a on a.asset_id = ua.asset_id and a.asset_type_id = 12 and a.owner_user_id = @uid
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
                var item = new UserClothingItem
                {
                    AssetId = reader.GetInt64(0),
                    Name = reader.IsDBNull(1) ? "Unnamed" : reader.GetString(1),
                    CreatedAt = reader.GetDateTime(2),
                    ThumbnailUrl = reader.IsDBNull(3) ? null : reader.GetString(3),
                    ImageAssetId = reader.IsDBNull(4) ? (long?)null : reader.GetInt64(4),
                    Sales = reader.IsDBNull(5) ? 0L : reader.GetInt64(5),
                };

                results.Add(item);
            }

            return results;
        }

        public async Task<bool> HasUploadedClothingInLastHourAsync(string connectionString, long userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId));

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = @"select 1
from user_assets ua
join assets a on a.asset_id = ua.asset_id
where ua.user_id = @uid
  and a.asset_type_id in (2, 11, 12)
  and ua.created_at >= (now() at time zone 'utc') - interval '1 hour'
limit 1;";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("uid", userId);

            var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            return result != null;
        }
    }
}
