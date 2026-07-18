using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Economy
{
    public sealed class PriceHistoryService
    {
        public async Task RecordSaleAsync(
            NpgsqlConnection conn, NpgsqlTransaction tx,
            long assetId, long price, CancellationToken ct = default)
        {
            const string sql = @"INSERT INTO price_history (asset_id, price) VALUES (@assetId, @price)";
            using var cmd = new NpgsqlCommand(sql, conn, tx);
            cmd.Parameters.AddWithValue("assetId", assetId);
            cmd.Parameters.AddWithValue("price", price);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        }

        public async Task<PriceChartResult> GetPriceChartAsync(
            NpgsqlConnection conn, long assetId, int days, CancellationToken ct = default)
        {
            var result = new PriceChartResult
            {
                Prices = Array.Empty<PricePoint>(),
                Volume = Array.Empty<VolumePoint>()
            };

            const string pricesSql = @"SELECT date_trunc('day', recorded_at) AS day, MIN(price)
                                       FROM price_history
                                       WHERE asset_id = @assetId AND recorded_at >= now() - (@days || ' days')::interval
                                       GROUP BY date_trunc('day', recorded_at)
                                       ORDER BY day ASC";
            using (var cmd = new NpgsqlCommand(pricesSql, conn))
            {
                cmd.Parameters.AddWithValue("assetId", assetId);
                cmd.Parameters.AddWithValue("days", days);
                await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
                var prices = new List<PricePoint>();
                while (await reader.ReadAsync(ct).ConfigureAwait(false))
                {
                    prices.Add(new PricePoint
                    {
                        Date = reader.GetDateTime(0),
                        Price = reader.GetInt64(1)
                    });
                }
                result.Prices = prices;
            }

            const string volumeSql = @"SELECT date_trunc('day', recorded_at) AS day, COUNT(*)::int
                                       FROM price_history
                                       WHERE asset_id = @assetId AND recorded_at >= now() - (@days || ' days')::interval
                                       GROUP BY date_trunc('day', recorded_at)
                                       ORDER BY day ASC";
            using (var cmd = new NpgsqlCommand(volumeSql, conn))
            {
                cmd.Parameters.AddWithValue("assetId", assetId);
                cmd.Parameters.AddWithValue("days", days);
                await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
                var volume = new List<VolumePoint>();
                while (await reader.ReadAsync(ct).ConfigureAwait(false))
                {
                    volume.Add(new VolumePoint
                    {
                        Date = reader.GetDateTime(0),
                        Count = reader.GetInt32(1)
                    });
                }
                result.Volume = volume;
            }

            const string statsSql = @"SELECT recent_average_price, price, coalesce(sales, 0)
                                      FROM assets WHERE asset_id = @assetId";
            using (var cmd = new NpgsqlCommand(statsSql, conn))
            {
                cmd.Parameters.AddWithValue("assetId", assetId);
                await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
                if (await reader.ReadAsync(ct).ConfigureAwait(false))
                {
                    result.AveragePrice = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
                    result.OriginalPrice = reader.IsDBNull(1) ? 0 : reader.GetInt64(1);
                    result.QuantitySold = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                }
            }

            return result;
        }

        public async Task<IReadOnlyList<PricePoint>> GetPriceHistoryAsync(
            NpgsqlConnection conn, long assetId, int days, CancellationToken ct = default)
        {
            const string sql = @"SELECT recorded_at, price FROM price_history
                                 WHERE asset_id = @assetId AND recorded_at >= now() - (@days || ' days')::interval
                                 ORDER BY recorded_at ASC";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("assetId", assetId);
            cmd.Parameters.AddWithValue("days", days);
            await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);

            var points = new List<PricePoint>();
            while (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                points.Add(new PricePoint
                {
                    Date = reader.GetDateTime(0),
                    Price = reader.GetInt64(1)
                });
            }
            return points;
        }
    }

    public class PricePoint
    {
        public DateTime Date { get; set; }
        public long Price { get; set; }
    }

    public class VolumePoint
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }

    public class PriceChartResult
    {
        public IReadOnlyList<PricePoint> Prices { get; set; } = Array.Empty<PricePoint>();
        public IReadOnlyList<VolumePoint> Volume { get; set; } = Array.Empty<VolumePoint>();
        public long OriginalPrice { get; set; }
        public long AveragePrice { get; set; }
        public long QuantitySold { get; set; }
    }
}
