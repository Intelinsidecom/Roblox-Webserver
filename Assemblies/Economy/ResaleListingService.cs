using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Economy
{
    public sealed class ResaleListingService
    {
        private readonly MarketplaceFeeService _feeService;

        public ResaleListingService(MarketplaceFeeService feeService)
        {
            _feeService = feeService;
        }

        public async Task<ResaleListing?> GetCheapestListingAsync(
            NpgsqlConnection conn, long assetId, CancellationToken ct = default)
        {
            const string sql = @"SELECT l.listing_id, l.asset_id, l.seller_user_id, u.user_name,
                                        l.serial_number, l.price, l.listed_at
                                 FROM resale_listings l
                                 JOIN users u ON u.user_id = l.seller_user_id
                                 WHERE l.asset_id = @assetId
                                 ORDER BY l.price ASC
                                 LIMIT 1";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("assetId", assetId);
            await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
            if (!await reader.ReadAsync(ct).ConfigureAwait(false))
                return null;

            return ReadListing(reader);
        }

        public async Task<IReadOnlyList<ResaleListing>> GetResellersAsync(
            NpgsqlConnection conn, long assetId, int offset, int count, CancellationToken ct = default)
        {
            const string sql = @"SELECT l.listing_id, l.asset_id, l.seller_user_id, u.user_name,
                                        l.serial_number, l.price, l.listed_at
                                 FROM resale_listings l
                                 JOIN users u ON u.user_id = l.seller_user_id
                                 WHERE l.asset_id = @assetId
                                 ORDER BY l.price ASC
                                 LIMIT @limit OFFSET @offset";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("assetId", assetId);
            cmd.Parameters.AddWithValue("limit", count);
            cmd.Parameters.AddWithValue("offset", offset);
            await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);

            var listings = new List<ResaleListing>();
            while (await reader.ReadAsync(ct).ConfigureAwait(false))
                listings.Add(ReadListing(reader));

            return listings;
        }

        public async Task<int> GetResellerCountAsync(
            NpgsqlConnection conn, long assetId, CancellationToken ct = default)
        {
            const string sql = @"SELECT COUNT(*) FROM resale_listings WHERE asset_id = @assetId";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("assetId", assetId);
            var result = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
            return result is int i ? i : 0;
        }

        public async Task<ResaleListing?> GetUserListingAsync(
            NpgsqlConnection conn, long userId, long assetId, CancellationToken ct = default)
        {
            const string sql = @"SELECT l.listing_id, l.asset_id, l.seller_user_id, u.user_name,
                                        l.serial_number, l.price, l.listed_at
                                 FROM resale_listings l
                                 JOIN users u ON u.user_id = l.seller_user_id
                                 WHERE l.seller_user_id = @userId AND l.asset_id = @assetId
                                 LIMIT 1";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("userId", userId);
            cmd.Parameters.AddWithValue("assetId", assetId);
            await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
            if (!await reader.ReadAsync(ct).ConfigureAwait(false))
                return null;

            return ReadListing(reader);
        }

        public async Task<(bool Success, string? Error)> CreateListingAsync(
            NpgsqlConnection conn, NpgsqlTransaction tx,
            long userId, long assetId, long? serialNumber, long price,
            CancellationToken ct = default)
        {
            if (price <= 0)
                return (false, "Price must be greater than zero.");

            var existing = await GetUserListingAsync(conn, userId, assetId, ct).ConfigureAwait(false);
            if (existing != null)
                return (false, "You already have this item listed for sale.");

            const string ownSql = @"SELECT 1 FROM user_assets WHERE user_id = @userId AND asset_id = @assetId LIMIT 1";
            using (var ownCmd = new NpgsqlCommand(ownSql, conn, tx))
            {
                ownCmd.Parameters.AddWithValue("userId", userId);
                ownCmd.Parameters.AddWithValue("assetId", assetId);
                var owns = await ownCmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
                if (owns == null)
                    return (false, "You do not own this asset.");
            }

            const string sql = @"INSERT INTO resale_listings (asset_id, seller_user_id, serial_number, price)
                                 VALUES (@assetId, @userId, @serial, @price)";
            using var cmd = new NpgsqlCommand(sql, conn, tx);
            cmd.Parameters.AddWithValue("assetId", assetId);
            cmd.Parameters.AddWithValue("userId", userId);
            cmd.Parameters.AddWithValue("serial", (object?)serialNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("price", price);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

            const string removeSql = @"DELETE FROM user_assets WHERE user_id = @userId AND asset_id = @assetId";
            using var removeCmd = new NpgsqlCommand(removeSql, conn, tx);
            removeCmd.Parameters.AddWithValue("userId", userId);
            removeCmd.Parameters.AddWithValue("assetId", assetId);
            await removeCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

            return (true, null);
        }

        public async Task<(bool Success, string? Error)> CancelListingAsync(
            NpgsqlConnection conn, NpgsqlTransaction? tx, long listingId, long userId, CancellationToken ct = default)
        {
            var useTx = tx ?? conn.BeginTransaction();
            bool ownTx = tx == null;

            try
            {
                long assetId = 0;
                const string fetchSql = @"DELETE FROM resale_listings
                                         WHERE listing_id = @listingId AND seller_user_id = @userId
                                         RETURNING asset_id";
                using (var fetchCmd = new NpgsqlCommand(fetchSql, conn, useTx))
                {
                    fetchCmd.Parameters.AddWithValue("listingId", listingId);
                    fetchCmd.Parameters.AddWithValue("userId", userId);
                    var result = await fetchCmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
                    if (result == null || result == DBNull.Value)
                    {
                        if (ownTx) useTx.Rollback();
                        return (false, "Listing not found or not owned by you.");
                    }
                    assetId = (long)result;
                }

                if (assetId > 0)
                {
                    const string restoreSql = @"INSERT INTO user_assets (user_id, asset_id) SELECT @userId, @assetId
                                                WHERE NOT EXISTS (SELECT 1 FROM user_assets WHERE user_id = @userId AND asset_id = @assetId)";
                    using var restoreCmd = new NpgsqlCommand(restoreSql, conn, useTx);
                    restoreCmd.Parameters.AddWithValue("userId", userId);
                    restoreCmd.Parameters.AddWithValue("assetId", assetId);
                    await restoreCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                }

                if (ownTx) useTx.Commit();
                return (true, null);
            }
            catch
            {
                if (ownTx) useTx.Rollback();
                throw;
            }
        }

        public async Task<(bool Success, string? Error)> PurchaseResaleAsync(
            NpgsqlConnection conn, NpgsqlTransaction tx,
            long buyerUserId, long listingId,
            CancellationToken ct = default)
        {
            using (var lockCmd = new NpgsqlCommand(
                @"SELECT l.listing_id, l.asset_id, l.seller_user_id, l.serial_number, l.price
                  FROM resale_listings l
                  WHERE l.listing_id = @listingId FOR UPDATE", conn, tx))
            {
                lockCmd.Parameters.AddWithValue("listingId", listingId);
                await using var reader = await lockCmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
                if (!await reader.ReadAsync(ct).ConfigureAwait(false))
                {
                    await tx.RollbackAsync(ct).ConfigureAwait(false);
                    return (false, "Listing not found.");
                }

                var assetId = reader.GetInt64(1);
                var sellerUserId = reader.GetInt64(2);
                var serialNumber = reader.IsDBNull(3) ? (long?)null : reader.GetInt64(3);
                var price = reader.GetInt64(4);
                await reader.CloseAsync().ConfigureAwait(false);

                if (sellerUserId == buyerUserId)
                {
                    await tx.RollbackAsync(ct).ConfigureAwait(false);
                    return (false, "You cannot buy your own listing.");
                }

                using (var balCmd = new NpgsqlCommand(
                    @"SELECT robux_balance FROM users WHERE user_id = @uid FOR UPDATE", conn, tx))
                {
                    balCmd.Parameters.AddWithValue("uid", buyerUserId);
                    var balResult = await balCmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
                    var balance = balResult is long l ? l : 0;
                    if (balance < price)
                    {
                        await tx.RollbackAsync(ct).ConfigureAwait(false);
                        return (false, "Insufficient funds.");
                    }
                }

                var sellerFee = _feeService.CalculateSellerProceeds(price);
                var marketplaceFee = price - sellerFee;

                using (var deductCmd = new NpgsqlCommand(
                    @"UPDATE users SET robux_balance = robux_balance - @price WHERE user_id = @uid", conn, tx))
                {
                    deductCmd.Parameters.AddWithValue("price", price);
                    deductCmd.Parameters.AddWithValue("uid", buyerUserId);
                    await deductCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                }

                using (var creditCmd = new NpgsqlCommand(
                    @"UPDATE users SET robux_balance = robux_balance + @amount WHERE user_id = @uid", conn, tx))
                {
                    creditCmd.Parameters.AddWithValue("amount", sellerFee);
                    creditCmd.Parameters.AddWithValue("uid", sellerUserId);
                    await creditCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                }

                if (marketplaceFee > 0)
                {
                    var platformId = MarketplaceFeeService.GetPlatformAccountId();
                    using var ensureCmd = new NpgsqlCommand(
                        @"INSERT INTO users (user_id, user_name, robux_balance) VALUES (@uid, 'Marketplace', 0)
                          ON CONFLICT (user_id) DO NOTHING", conn, tx);
                    ensureCmd.Parameters.AddWithValue("uid", platformId);
                    await ensureCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

                    using var feeCmd = new NpgsqlCommand(
                        @"UPDATE users SET robux_balance = robux_balance + @amount WHERE user_id = @uid", conn, tx);
                    feeCmd.Parameters.AddWithValue("amount", marketplaceFee);
                    feeCmd.Parameters.AddWithValue("uid", platformId);
                    await feeCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                }

                using (var delistCmd = new NpgsqlCommand(
                    @"DELETE FROM resale_listings WHERE listing_id = @listingId", conn, tx))
                {
                    delistCmd.Parameters.AddWithValue("listingId", listingId);
                    await delistCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                }

                using (var transferCmd = new NpgsqlCommand(
                    @"DELETE FROM user_assets WHERE user_id = @seller AND asset_id = @assetId", conn, tx))
                {
                    transferCmd.Parameters.AddWithValue("seller", sellerUserId);
                    transferCmd.Parameters.AddWithValue("assetId", assetId);
                    await transferCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                }

                using (var insertCmd = new NpgsqlCommand(
                    @"INSERT INTO user_assets (user_id, asset_id) SELECT @uid, @assetId
                      WHERE NOT EXISTS (SELECT 1 FROM user_assets WHERE user_id = @uid AND asset_id = @assetId)", conn, tx))
                {
                    insertCmd.Parameters.AddWithValue("uid", buyerUserId);
                    insertCmd.Parameters.AddWithValue("assetId", assetId);
                    await insertCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                }

                if (serialNumber.HasValue)
                {
                    using (var serialCmd = new NpgsqlCommand(
                        @"UPDATE asset_serials SET owner_user_id = @toUser
                          WHERE asset_id = @assetId AND serial_number = @serial AND owner_user_id = @fromUser", conn, tx))
                    {
                        serialCmd.Parameters.AddWithValue("toUser", buyerUserId);
                        serialCmd.Parameters.AddWithValue("fromUser", sellerUserId);
                        serialCmd.Parameters.AddWithValue("assetId", assetId);
                        serialCmd.Parameters.AddWithValue("serial", serialNumber.Value);
                        await serialCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                    }
                }

                using (var salesCmd = new NpgsqlCommand(
                    @"UPDATE assets SET sales = coalesce(sales, 0) + 1 WHERE asset_id = @assetId", conn, tx))
                {
                    salesCmd.Parameters.AddWithValue("assetId", assetId);
                    await salesCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                }

                using (var rapCmd = new NpgsqlCommand(
                    @"UPDATE assets
                      SET recent_average_price =
                          CASE WHEN coalesce(sales, 0) <= 1 THEN @price
                               ELSE (recent_average_price * (coalesce(sales, 0) - 1) + @price) / coalesce(sales, 0)
                          END,
                          last_updated = now()
                      WHERE asset_id = @assetId", conn, tx))
                {
                    rapCmd.Parameters.AddWithValue("assetId", assetId);
                    rapCmd.Parameters.AddWithValue("price", price);
                    await rapCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                }

                using (var logCmd = new NpgsqlCommand(
                    @"INSERT INTO asset_sales_log (asset_id, buyer_user_id, price, currency, sold_at)
                      VALUES (@assetId, @uid, @price, 1, now())", conn, tx))
                {
                    logCmd.Parameters.AddWithValue("assetId", assetId);
                    logCmd.Parameters.AddWithValue("uid", buyerUserId);
                    logCmd.Parameters.AddWithValue("price", price);
                    await logCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                }

                using (var histCmd = new NpgsqlCommand(
                    @"INSERT INTO price_history (asset_id, price) VALUES (@assetId, @price)", conn, tx))
                {
                    histCmd.Parameters.AddWithValue("assetId", assetId);
                    histCmd.Parameters.AddWithValue("price", price);
                    await histCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                }
            }

            return (true, null);
        }

        private static ResaleListing ReadListing(NpgsqlDataReader reader)
        {
            return new ResaleListing
            {
                ListingId = reader.GetInt64(0),
                AssetId = reader.GetInt64(1),
                SellerUserId = reader.GetInt64(2),
                SellerName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                SerialNumber = reader.IsDBNull(4) ? null : (long?)reader.GetInt64(4),
                Price = reader.GetInt64(5),
                ListedAt = reader.GetDateTime(6)
            };
        }
    }

    public class ResaleListing
    {
        public long ListingId { get; set; }
        public long AssetId { get; set; }
        public long SellerUserId { get; set; }
        public string SellerName { get; set; } = string.Empty;
        public long? SerialNumber { get; set; }
        public long Price { get; set; }
        public DateTime ListedAt { get; set; }
    }
}
