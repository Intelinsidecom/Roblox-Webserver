using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Economy
{
    public sealed class AssetPurchaseLogging
    {
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
