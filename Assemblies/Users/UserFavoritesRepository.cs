using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Users
{
    public sealed class UserFavoritesRepository
    {
        public async Task AddUserFavoriteAsync(string connectionString, long userId, long assetId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId));
            if (assetId <= 0)
                throw new ArgumentOutOfRangeException(nameof(assetId));

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = @"update users
set favorites = case
    when exists (
        select 1
        from jsonb_array_elements_text(favorites) as x(v)
        where v = @asset_id::text
    ) then favorites
    else favorites || to_jsonb(@asset_id::bigint)
end
where user_id = @user_id;";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("user_id", userId);
            cmd.Parameters.AddWithValue("asset_id", assetId);

            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task RemoveUserFavoriteAsync(string connectionString, long userId, long assetId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId));
            if (assetId <= 0)
                throw new ArgumentOutOfRangeException(nameof(assetId));

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = @"update users
set favorites = coalesce(
    (
        select jsonb_agg(value::bigint)
        from jsonb_array_elements(favorites) as value
        where value::text <> @asset_id::text
    ),
    '[]'::jsonb
)
where user_id = @user_id;";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("user_id", userId);
            cmd.Parameters.AddWithValue("asset_id", assetId);

            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
