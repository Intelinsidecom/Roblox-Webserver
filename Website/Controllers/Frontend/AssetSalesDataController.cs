using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Economy;

namespace Website.Controllers
{
    [ApiController]
    [Route("asset")]
    public class AssetSalesDataController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AssetSalesDataController(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        [HttpGet("{assetId:long}/sales-data")]
        public async Task<IActionResult> GetSalesData(
            long assetId,
            CancellationToken cancellationToken = default)
        {
            var connStr = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
                return StatusCode(500);

            try
            {
                await using var conn = new NpgsqlConnection(connStr);
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

                var service = new PriceHistoryService();
                var chart = await service.GetPriceChartAsync(conn, assetId, 180, cancellationToken).ConfigureAwait(false);

                if (chart.Prices.Count == 0 && chart.QuantitySold == 0)
                {
                    return Ok(new { isValid = false });
                }

                var salesChart = string.Join("|",
                    chart.Prices.Select(p => p.Date.ToString("M/d/yyyy") + "," + p.Price));

                var volumeChart = string.Join("|",
                    chart.Volume.Select(v => v.Date.ToString("M/d/yyyy") + "," + v.Count));

                return Ok(new
                {
                    isValid = true,
                    data = new
                    {
                        QuantitySold = chart.QuantitySold,
                        OriginalPrice = chart.OriginalPrice,
                        AveragePrice = chart.AveragePrice,
                        HundredEightyDaySalesChart = salesChart,
                        HundredEightyDayVolumeChart = volumeChart
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
