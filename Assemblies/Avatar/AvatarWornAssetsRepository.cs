using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using System.Linq;

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

            // Normalize input to avoid duplicate key violations on avatar_worn_assets_pkey.
            // Even if callers accidentally pass duplicate IDs or race with each other,
            // this method will only ever attempt a single insert per (user_id, asset_id)
            // within its transaction.
            var normalizedIds = assetIds
                .Where(id => id > 0)
                .Distinct()
                .OrderBy(id => id)
                .ToArray();

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            await using var tx = await conn.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

            const string deleteSql = @"delete from avatar_worn_assets where user_id = @uid";
            await using (var deleteCmd = new NpgsqlCommand(deleteSql, conn, tx))
            {
                deleteCmd.Parameters.AddWithValue("uid", userId);
                await deleteCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            if (normalizedIds.Length > 0)
            {
                const string insertSql = @"insert into avatar_worn_assets(user_id, asset_id) values(@uid, @aid)";
                await using var insertCmd = new NpgsqlCommand(insertSql, conn, tx);
                insertCmd.Parameters.AddWithValue("uid", userId);
                var assetParam = insertCmd.Parameters.Add("aid", NpgsqlTypes.NpgsqlDbType.Bigint);

                foreach (var id in normalizedIds)
                {
                    assetParam.Value = id;
                    await insertCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                }
            }

            await tx.CommitAsync(cancellationToken).ConfigureAwait(false);
        }

        public sealed class WearAssetResult
        {
            public bool AlreadyWorn { get; set; }
            public long[] AssetIds { get; set; } = Array.Empty<long>();
        }

        public async Task<WearAssetResult> WearAssetAsync(string connectionString, long userId, long assetId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId));
            if (assetId <= 0)
                throw new ArgumentOutOfRangeException(nameof(assetId));

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string wornSql = @"select asset_id from avatar_worn_assets where user_id = @uid order by asset_id";
            await using var wornCmd = new NpgsqlCommand(wornSql, conn);
            wornCmd.Parameters.AddWithValue("uid", userId);

            var current = new List<long>();
            await using (var reader = await wornCmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
            {
                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    current.Add(reader.GetInt64(0));
                }
            }

            if (current.Contains(assetId))
            {
                return new WearAssetResult
                {
                    AlreadyWorn = true,
                    AssetIds = current.ToArray()
                };
            }

            var updated = current.ToList();

            const string typeSql = @"select asset_type_id from assets where asset_id = @aid";
            await using (var typeCmd = new NpgsqlCommand(typeSql, conn))
            {
                typeCmd.Parameters.AddWithValue("aid", assetId);

                var typeObj = await typeCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                if (typeObj is int assetTypeId)
                {
                    var singleSlotTypes = new HashSet<int>
                    {
                        2, 11, 12, 17, 18, 19,
                        27, 28, 29, 30, 31,
                        41, 42, 43, 44, 45, 46, 47,
                        48, 50, 51, 52, 53, 54, 55
                    };

                    if (singleSlotTypes.Contains(assetTypeId) && updated.Count > 0)
                    {
                        const string sameTypeSql = @"select asset_id from assets where asset_type_id = @typeId and asset_id = any(@ids) order by asset_id";
                        await using (var sameTypeCmd = new NpgsqlCommand(sameTypeSql, conn))
                        {
                            sameTypeCmd.Parameters.AddWithValue("typeId", assetTypeId);
                            sameTypeCmd.Parameters.AddWithValue("ids", updated.ToArray());

                            var sameTypeIds = new List<long>();
                            await using var sameReader = await sameTypeCmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                            while (await sameReader.ReadAsync(cancellationToken).ConfigureAwait(false))
                            {
                                sameTypeIds.Add(sameReader.GetInt64(0));
                            }

                            if (sameTypeIds.Count > 0)
                            {
                                var sameTypeSet = new HashSet<long>(sameTypeIds);
                                updated = updated
                                    .Where(id => !sameTypeSet.Contains(id))
                                    .ToList();
                            }
                        }
                    }
                }
            }

            updated.Add(assetId);

            return new WearAssetResult
            {
                AlreadyWorn = false,
                AssetIds = updated.ToArray()
            };
        }
    }
}
