using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Avatar
{
    public sealed class AvatarWornAssetsRepository
    {
        public async Task<long[]> GetWornAssetIdsAsync(string connectionString, long userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId));

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = @"select asset_id from avatar_worn_assets where user_id = @uid order by asset_id";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("uid", userId);

            var results = new List<long>();
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                results.Add(reader.GetInt64(0));
            }

            return results.ToArray();
        }

        public async Task SetWornAssetIdsAsync(string connectionString, long userId, IReadOnlyCollection<long> assetIds, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId));
            if (assetIds == null)
                throw new ArgumentNullException(nameof(assetIds));

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            await using var tx = await conn.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

            const string deleteSql = @"delete from avatar_worn_assets where user_id = @uid";
            await using (var deleteCmd = new NpgsqlCommand(deleteSql, conn, tx))
            {
                deleteCmd.Parameters.AddWithValue("uid", userId);
                await deleteCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            if (assetIds.Count > 0)
            {
                const string insertSql = @"insert into avatar_worn_assets(user_id, asset_id) values(@uid, @aid)";
                await using var insertCmd = new NpgsqlCommand(insertSql, conn, tx);
                insertCmd.Parameters.AddWithValue("uid", userId);
                var assetParam = insertCmd.Parameters.Add("aid", NpgsqlTypes.NpgsqlDbType.Bigint);

                foreach (var id in assetIds)
                {
                    assetParam.Value = id;
                    await insertCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                }
            }

            await tx.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
