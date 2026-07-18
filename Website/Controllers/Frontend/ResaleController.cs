using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Economy;

namespace Website.Controllers
{
    [ApiController]
    [Route("catalog-api")]
    public class ResaleController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ResaleController(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        [HttpGet("{assetId:long}/resellers")]
        public async Task<IActionResult> GetResellers(
            long assetId,
            [FromQuery(Name = "offset")] int offset = 0,
            [FromQuery(Name = "count")] int count = 10,
            CancellationToken cancellationToken = default)
        {
            if (assetId <= 0)
                return BadRequest(new { error = "Invalid asset ID" });

            var connStr = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
                return StatusCode(500);

            try
            {
                await using var conn = new NpgsqlConnection(connStr);
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

                var feeService = new MarketplaceFeeService(_configuration);
                var service = new ResaleListingService(feeService);
                var listings = await service.GetResellersAsync(conn, assetId, offset, count, cancellationToken).ConfigureAwait(false);
                var total = await service.GetResellerCountAsync(conn, assetId, cancellationToken).ConfigureAwait(false);

                var result = listings.Select(l => new
                {
                    userAssetId = l.ListingId,
                    seller = new
                    {
                        id = l.SellerUserId,
                        name = l.SellerName,
                        type = "User"
                    },
                    serialNumber = l.SerialNumber,
                    price = l.Price,
                    listingId = l.ListingId
                });

                return Ok(new { data = result, totalPages = (int)Math.Ceiling(total / (double)Math.Max(count, 1)) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("{assetId:long}/pricechart")]
        public async Task<IActionResult> GetPriceChart(
            long assetId,
            [FromQuery(Name = "days")] int days = 30,
            CancellationToken cancellationToken = default)
        {
            if (assetId <= 0)
                return BadRequest(new { error = "Invalid asset ID" });

            var connStr = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
                return StatusCode(500);

            try
            {
                await using var conn = new NpgsqlConnection(connStr);
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

                var service = new PriceHistoryService();
                var chart = await service.GetPriceChartAsync(conn, assetId, days, cancellationToken).ConfigureAwait(false);

                return Ok(new
                {
                    priceData = chart.Prices.Select(p => new { date = p.Date, price = p.Price }),
                    volumeData = chart.Volume.Select(v => new { date = v.Date, volume = v.Count }),
                    originalPrice = chart.OriginalPrice,
                    averagePrice = chart.AveragePrice,
                    quantitySold = chart.QuantitySold
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("listsell")]
        public async Task<IActionResult> ListForResale(
            [FromBody] JsonElement body,
            CancellationToken cancellationToken = default)
        {
            var idStr = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(idStr) || !long.TryParse(idStr, out var userId) || userId <= 0)
                return StatusCode(403);

            var connStr = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
                return StatusCode(500);

            if (!body.TryGetProperty("assetId", out var assetIdEl) || !assetIdEl.TryGetInt64(out var assetId))
                return BadRequest(new { error = "assetId is required" });
            if (!body.TryGetProperty("price", out var priceEl) || !priceEl.TryGetInt64(out var price) || price <= 0)
                return BadRequest(new { error = "price must be > 0" });

            long? serialNumber = null;
            if (body.TryGetProperty("serialNumber", out var serialEl) && serialEl.ValueKind == JsonValueKind.Number)
                serialNumber = serialEl.GetInt64();

            try
            {
                await using var conn = new NpgsqlConnection(connStr);
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
                using var tx = conn.BeginTransaction();

                var service = new ResaleListingService(new MarketplaceFeeService(_configuration));
                var (success, error) = await service.CreateListingAsync(conn, tx, userId, assetId, serialNumber, price, cancellationToken).ConfigureAwait(false);

                if (success)
                    await tx.CommitAsync(cancellationToken).ConfigureAwait(false);
                else
                    await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);

                return success ? Ok(new { success = true }) : BadRequest(new { error });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("cancelsell")]
        public async Task<IActionResult> CancelSale(
            [FromBody] JsonElement body,
            CancellationToken cancellationToken = default)
        {
            var idStr = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(idStr) || !long.TryParse(idStr, out var userId) || userId <= 0)
                return StatusCode(403);

            var connStr = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
                return StatusCode(500);

            if (!body.TryGetProperty("listingId", out var listingIdEl) || !listingIdEl.TryGetInt64(out var listingId))
                return BadRequest(new { error = "listingId is required" });

            try
            {
                await using var conn = new NpgsqlConnection(connStr);
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
                using var tx = conn.BeginTransaction();

                var service = new ResaleListingService(new MarketplaceFeeService(_configuration));
                var (success, error) = await service.CancelListingAsync(conn, tx, listingId, userId, cancellationToken).ConfigureAwait(false);

                if (success)
                    await tx.CommitAsync(cancellationToken).ConfigureAwait(false);
                else
                    await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);

                return success ? Ok(new { success = true }) : BadRequest(new { error });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("buyresale")]
        public async Task<IActionResult> BuyResale(
            [FromBody] JsonElement body,
            CancellationToken cancellationToken = default)
        {
            var idStr = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(idStr) || !long.TryParse(idStr, out var userId) || userId <= 0)
                return StatusCode(403);

            var connStr = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
                return StatusCode(500);

            if (!body.TryGetProperty("listingId", out var listingIdEl) || !listingIdEl.TryGetInt64(out var listingId))
                return BadRequest(new { error = "listingId is required" });

            try
            {
                await using var conn = new NpgsqlConnection(connStr);
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
                using var tx = conn.BeginTransaction();

                var service = new ResaleListingService(new MarketplaceFeeService(_configuration));
                var (success, error) = await service.PurchaseResaleAsync(conn, tx, userId, listingId, cancellationToken).ConfigureAwait(false);

                if (success)
                    await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

                return success ? Ok(new { success = true }) : BadRequest(new { error });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
