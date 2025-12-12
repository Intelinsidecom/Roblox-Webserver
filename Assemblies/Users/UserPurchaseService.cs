using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Users
{
    public sealed class UserPurchaseService
    {
        public enum CurrencyKind
        {
            Robux = 1,
            Tix = 2
        }

        public async Task<(bool Success, string? Error)> PurchaseAssetAsync(
            string connectionString,
            long userId,
            long assetId,
            CurrencyKind currency,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                return (false, "Invalid user");
            if (assetId <= 0)
                return (false, "Invalid asset");

            using (var conn = new NpgsqlConnection(connectionString))
            {
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        long price = 0;
                        bool onSale = false;

                        using (var assetCmd = new NpgsqlCommand("select price, on_sale from assets where asset_id = @aid for update", conn, tx))
                        {
                            assetCmd.Parameters.AddWithValue("aid", assetId);
                            using var reader = await assetCmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                            if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                            {
                                tx.Rollback();
                                return (false, "Asset not found");
                            }

                            price = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
                            onSale = !reader.IsDBNull(1) && reader.GetBoolean(1);
                        }

                        if (!onSale || price <= 0)
                        {
                            tx.Rollback();
                            return (false, "Asset is not for sale");
                        }

                        using (var ownCmd = new NpgsqlCommand("select 1 from user_assets where user_id = @uid and asset_id = @aid limit 1", conn, tx))
                        {
                            ownCmd.Parameters.AddWithValue("uid", userId);
                            ownCmd.Parameters.AddWithValue("aid", assetId);
                            var alreadyOwns = await ownCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                            if (alreadyOwns != null)
                            {
                                tx.Rollback();
                                return (false, "User already owns this asset");
                            }
                        }

                        string balanceColumn = currency == CurrencyKind.Robux ? "robux_balance" : "tix_balance";

                        long balance = 0;
                        using (var balCmd = new NpgsqlCommand($"select coalesce({balanceColumn},0) from users where user_id = @uid for update", conn, tx))
                        {
                            balCmd.Parameters.AddWithValue("uid", userId);
                            var obj = await balCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                            balance = obj is long l ? l : (obj is int i ? i : 0);
                        }

                        if (balance < price)
                        {
                            tx.Rollback();
                            return (false, "Insufficient funds");
                        }

                        using (var updCmd = new NpgsqlCommand($"update users set {balanceColumn} = {balanceColumn} - @p where user_id = @uid", conn, tx))
                        {
                            updCmd.Parameters.AddWithValue("p", price);
                            updCmd.Parameters.AddWithValue("uid", userId);
                            var affected = await updCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                            if (affected != 1)
                            {
                                tx.Rollback();
                                return (false, "Failed to update balance");
                            }
                        }

                        const string insertSql = @"insert into user_assets (user_id, asset_id)
values (@uid, @aid)
on conflict (user_id, asset_id) do nothing;";
                        using (var insCmd = new NpgsqlCommand(insertSql, conn, tx))
                        {
                            insCmd.Parameters.AddWithValue("uid", userId);
                            insCmd.Parameters.AddWithValue("aid", assetId);
                            await insCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                        }

                        tx.Commit();
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            tx.Rollback();
                        }
                        catch
                        {
                        }

                        return (false, ex.Message);
                    }
                }
            }
        }
    }
}
