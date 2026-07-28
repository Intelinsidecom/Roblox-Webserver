using System;
using System.Threading;
using System.Threading.Tasks;
using Economy;
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

            var limitedService = new LimitedItemService();
            var priceHistoryService = new PriceHistoryService();

            using (var conn = new NpgsqlConnection(connectionString))
            {
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        long price = 0;
                        bool onSale = false;
                        bool isLimitedUnique = false;
                        long? limitedRemaining = null;
                        DateTime? limitedUntil = null;

                        using (var assetCmd = new NpgsqlCommand(
                            @"select price, on_sale, limited_unique, limited_remaining, limited_until
                              from assets where asset_id = @aid for update", conn, tx))
                        {
                            assetCmd.Parameters.AddWithValue("aid", assetId);
                            using var reader = await assetCmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                            if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                            {
                                await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
                                return (false, "Asset not found");
                            }

                            price = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
                            onSale = !reader.IsDBNull(1) && reader.GetBoolean(1);
                            isLimitedUnique = !reader.IsDBNull(2) && reader.GetBoolean(2);
                            limitedRemaining = reader.IsDBNull(3) ? null : (long?)reader.GetInt64(3);
                            limitedUntil = reader.IsDBNull(4) ? null : (DateTime?)reader.GetDateTime(4);
                        }

                        if (!onSale || price <= 0)
                        {
                            await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
                            return (false, "Asset is not for sale");
                        }

                        bool isLimited = isLimitedUnique || (limitedRemaining.HasValue);
                        if (isLimited)
                        {
                            if (limitedUntil.HasValue && limitedUntil.Value <= DateTime.UtcNow)
                            {
                                await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
                                return (false, "This limited item is no longer on sale.");
                            }

                            if (limitedRemaining.HasValue && limitedRemaining.Value <= 0)
                            {
                                await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
                                return (false, "This limited item is sold out.");
                            }

                            await limitedService.DecrementStockAsync(conn, tx, assetId, cancellationToken).ConfigureAwait(false);
                        }

                        using (var ownCmd = new NpgsqlCommand("select 1 from user_assets where user_id = @uid and asset_id = @aid limit 1", conn, tx))
                        {
                            ownCmd.Parameters.AddWithValue("uid", userId);
                            ownCmd.Parameters.AddWithValue("aid", assetId);
                            var alreadyOwns = await ownCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                            if (alreadyOwns != null)
                            {
                                await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
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
                            await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
                            return (false, "Insufficient funds");
                        }

                        using (var updCmd = new NpgsqlCommand($"update users set {balanceColumn} = {balanceColumn} - @p where user_id = @uid", conn, tx))
                        {
                            updCmd.Parameters.AddWithValue("p", price);
                            updCmd.Parameters.AddWithValue("uid", userId);
                            var affected = await updCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                            if (affected != 1)
                            {
                                await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
                                return (false, "Failed to update balance");
                            }
                        }

                        const string insertSql = @"insert into user_assets (user_id, asset_id)
select @uid, @aid
where not exists (
    select 1 from user_assets where user_id = @uid and asset_id = @aid
);";
                        using (var insCmd = new NpgsqlCommand(insertSql, conn, tx))
                        {
                            insCmd.Parameters.AddWithValue("uid", userId);
                            insCmd.Parameters.AddWithValue("aid", assetId);
                            await insCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                        }

                        if (isLimitedUnique)
                        {
                            await limitedService.AssignSerialNumberAsync(conn, tx, assetId, userId, cancellationToken).ConfigureAwait(false);
                        }

                        using (var salesCmd = new NpgsqlCommand("update assets set sales = coalesce(sales, 0) + 1 where asset_id = @aid", conn, tx))
                        {
                            salesCmd.Parameters.AddWithValue("aid", assetId);
                            await salesCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                        }

                        await limitedService.UpdateRapAsync(conn, tx, assetId, price, cancellationToken).ConfigureAwait(false);

                        using (var histCmd = new NpgsqlCommand(
                            "INSERT INTO price_history (asset_id, price) VALUES (@aid, @price)", conn, tx))
                        {
                            histCmd.Parameters.AddWithValue("aid", assetId);
                            histCmd.Parameters.AddWithValue("price", price);
                            await histCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                        }

                        using (var logCmd = new NpgsqlCommand(
                            "INSERT INTO asset_sales_log (asset_id, buyer_user_id, price, currency, sold_at) VALUES (@aid, @uid, @price, @cur, now())", conn, tx))
                        {
                            logCmd.Parameters.AddWithValue("aid", assetId);
                            logCmd.Parameters.AddWithValue("uid", userId);
                            logCmd.Parameters.AddWithValue("price", price);
                            logCmd.Parameters.AddWithValue("cur", (int)currency);
                            await logCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                        }

                        await tx.CommitAsync(cancellationToken).ConfigureAwait(false);
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
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
