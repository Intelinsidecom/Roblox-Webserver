using System;
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
    }
}
