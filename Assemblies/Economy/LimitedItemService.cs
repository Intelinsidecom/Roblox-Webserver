using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;

namespace Economy
{
    public sealed class LimitedItemService
    {
        public async Task<bool> IsLimitedAsync(string connectionString, long assetId, CancellationToken ct = default)
        {
            using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);
            const string sql = @"SELECT limited_unique OR (limited_quantity IS NOT NULL AND limited_quantity > 0)
                                 FROM assets WHERE asset_id = @id";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", assetId);
            var result = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
            return result is bool b && b;
        }

        public async Task<LimitedItemData?> GetLimitedDataAsync(string connectionString, long assetId, CancellationToken ct = default)
        {
            using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);
            const string sql = @"SELECT limited_unique, limited_quantity, limited_remaining, limited_until, recent_average_price
                                 FROM assets WHERE asset_id = @id";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", assetId);
            await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
            if (!await reader.ReadAsync(ct).ConfigureAwait(false))
                return null;

            return new LimitedItemData
            {
                IsLimitedUnique = !reader.IsDBNull(0) && reader.GetBoolean(0),
                Quantity = reader.IsDBNull(1) ? null : reader.GetInt64(1),
                Remaining = reader.IsDBNull(2) ? null : reader.GetInt64(2),
                Until = reader.IsDBNull(3) ? null : (DateTime?)reader.GetDateTime(3),
                RecentAveragePrice = reader.IsDBNull(4) ? 0 : reader.GetInt64(4)
            };
        }

        public async Task<(bool Allowed, string? Reason)> ValidateLimitedPurchaseAsync(
            NpgsqlConnection conn, NpgsqlTransaction tx, long assetId, CancellationToken ct = default)
        {
            const string sql = @"SELECT limited_unique, limited_quantity, limited_remaining, limited_until
                                 FROM assets WHERE asset_id = @id FOR UPDATE";
            using var cmd = new NpgsqlCommand(sql, conn, tx);
            cmd.Parameters.AddWithValue("id", assetId);
            await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
            if (!await reader.ReadAsync(ct).ConfigureAwait(false))
                return (false, "Asset not found");

            var isLimitedUnique = !reader.IsDBNull(0) && reader.GetBoolean(0);
            var quantity = reader.IsDBNull(1) ? (long?)null : reader.GetInt64(1);
            var remaining = reader.IsDBNull(2) ? (long?)null : reader.GetInt64(2);
            var until = reader.IsDBNull(3) ? null : (DateTime?)reader.GetDateTime(3);

            bool isLimited = isLimitedUnique || (quantity.HasValue && quantity.Value > 0);
            if (!isLimited)
                return (true, null);

            if (until.HasValue && until.Value <= DateTime.UtcNow)
                return (false, "This limited item is no longer on sale.");

            if (remaining.HasValue && remaining.Value <= 0)
                return (false, "This limited item is sold out.");

            return (true, null);
        }

        public async Task<long> GetNextSerialAsync(
            NpgsqlConnection conn, NpgsqlTransaction tx, long assetId, CancellationToken ct = default)
        {
            const string sql = @"SELECT COALESCE(MAX(serial_number), 0) + 1
                                 FROM asset_serials WHERE asset_id = @id";
            using var cmd = new NpgsqlCommand(sql, conn, tx);
            cmd.Parameters.AddWithValue("id", assetId);
            var result = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
            return result is long l ? l : 1;
        }

        public async Task<long> AssignSerialNumberAsync(
            NpgsqlConnection conn, NpgsqlTransaction tx, long assetId, long buyerUserId, CancellationToken ct = default)
        {
            var serial = await GetNextSerialAsync(conn, tx, assetId, ct).ConfigureAwait(false);
            const string sql = @"INSERT INTO asset_serials (asset_id, serial_number, owner_user_id)
                                 VALUES (@assetId, @serial, @userId)";
            using var cmd = new NpgsqlCommand(sql, conn, tx);
            cmd.Parameters.AddWithValue("assetId", assetId);
            cmd.Parameters.AddWithValue("serial", serial);
            cmd.Parameters.AddWithValue("userId", buyerUserId);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            return serial;
        }

        public async Task TransferSerialAsync(
            NpgsqlConnection conn, NpgsqlTransaction tx, long assetId, long fromUserId, long toUserId, CancellationToken ct = default)
        {
            const string sql = @"UPDATE asset_serials SET owner_user_id = @toUserId
                                 WHERE asset_id = @assetId AND owner_user_id = @fromUserId";
            using var cmd = new NpgsqlCommand(sql, conn, tx);
            cmd.Parameters.AddWithValue("assetId", assetId);
            cmd.Parameters.AddWithValue("fromUserId", fromUserId);
            cmd.Parameters.AddWithValue("toUserId", toUserId);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        }

        public async Task<long?> GetUserSerialAsync(string connectionString, long assetId, long userId, CancellationToken ct = default)
        {
            using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);
            const string sql = @"SELECT serial_number FROM asset_serials
                                 WHERE asset_id = @assetId AND owner_user_id = @userId";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("assetId", assetId);
            cmd.Parameters.AddWithValue("userId", userId);
            var result = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
            return result is long l ? l : null;
        }

        public async Task DecrementStockAsync(
            NpgsqlConnection conn, NpgsqlTransaction tx, long assetId, CancellationToken ct = default)
        {
            const string sql = @"UPDATE assets
                                 SET limited_remaining = GREATEST(limited_remaining - 1, 0),
                                     last_updated = now()
                                 WHERE asset_id = @assetId
                                   AND limited_unique = true
                                   AND limited_remaining > 0";
            using var cmd = new NpgsqlCommand(sql, conn, tx);
            cmd.Parameters.AddWithValue("assetId", assetId);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        }

        public async Task UpdateRapAsync(
            NpgsqlConnection conn, NpgsqlTransaction tx, long assetId, long salePrice, CancellationToken ct = default)
        {
            const string sql = @"UPDATE assets
                                 SET recent_average_price =
                                     CASE WHEN coalesce(sales, 0) <= 0 THEN @price
                                          ELSE (recent_average_price * (coalesce(sales, 0) - 1) + @price) / coalesce(sales, 0)
                                     END,
                                     last_updated = now()
                                 WHERE asset_id = @assetId";
            using var cmd = new NpgsqlCommand(sql, conn, tx);
            cmd.Parameters.AddWithValue("assetId", assetId);
            cmd.Parameters.AddWithValue("price", salePrice);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        }

        public async Task<int> GetTotalSalesAsync(
            NpgsqlConnection conn, long assetId, CancellationToken ct = default)
        {
            const string sql = @"SELECT coalesce(sales, 0) FROM assets WHERE asset_id = @id";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", assetId);
            var result = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
            return result is int i ? i : 0;
        }
    }

    public class LimitedItemData
    {
        public bool IsLimitedUnique { get; set; }
        public long? Quantity { get; set; }
        public long? Remaining { get; set; }
        public DateTime? Until { get; set; }
        public long RecentAveragePrice { get; set; }
    }
}
