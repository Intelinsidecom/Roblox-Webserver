using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Economy
{
    public sealed class AssetPurchaseLogging
    {
        public sealed class SaleRecord
        {
            public long BuyerUserId { get; set; }
            public long AssetId { get; set; }
            public long Price { get; set; }
            public int Currency { get; set; }
        }

        public async Task<SaleRecord?> GetSaleByReceiptAsync(string connectionString, string receipt, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new System.ArgumentException("connectionString is required", nameof(connectionString));
            if (string.IsNullOrWhiteSpace(receipt))
                throw new System.ArgumentException("receipt is required", nameof(receipt));

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = @"SELECT buyer_user_id, asset_id, price, currency
                FROM asset_sales_log
                WHERE receipt_id = @receipt OR asset_sales_log_id::text = @receipt
                LIMIT 1";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("receipt", receipt);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                return null;

            return new SaleRecord
            {
                BuyerUserId = reader.IsDBNull(0) ? 0 : reader.GetInt64(0),
                AssetId = reader.IsDBNull(1) ? 0 : reader.GetInt64(1),
                Price = reader.IsDBNull(2) ? 0 : reader.GetInt64(2),
                Currency = reader.IsDBNull(3) ? 1 : reader.GetInt32(3),
            };
        }

        public async Task<Dictionary<long, long>> GetSalesLast7DaysAsync(
            NpgsqlConnection conn,
            IReadOnlyCollection<long> assetIds,
            CancellationToken cancellationToken = default)
        {
            if (assetIds == null || assetIds.Count == 0)
                return new Dictionary<long, long>();

            const string sql = @"SELECT asset_id, COUNT(*)
                                 FROM asset_sales_log
                                 WHERE asset_id = ANY(@ids) AND sold_at >= now() - interval '7 days'
                                 GROUP BY asset_id";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("ids", assetIds);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            var dict = new Dictionary<long, long>();
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                dict[reader.GetInt64(0)] = reader.GetInt64(1);
            }
            return dict;
        }
    }
}
