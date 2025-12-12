using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Users;

namespace Website.Controllers
{
    [ApiController]
    [Route("API/Item.ashx")]
    public class ItemController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ItemController(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Handle(
            [FromQuery(Name = "rqtype")] string rqType,
            [FromQuery(Name = "productID")] long productId,
            [FromQuery(Name = "expectedCurrency")] int expectedCurrency,
            [FromQuery(Name = "expectedPrice")] long expectedPrice,
            [FromQuery(Name = "expectedSellerID")] long expectedSellerId,
            [FromQuery(Name = "userAssetID")] long? userAssetId,
            CancellationToken cancellationToken = default)
        {
            if (!string.Equals(rqType, "purchase", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Bad Request");

            var idStr = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(idStr) || !long.TryParse(idStr, out var userId) || userId <= 0)
                return StatusCode(403);

            var connStr = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
                return StatusCode(500);

            var currencyKind = expectedCurrency == 2
                ? UserPurchaseService.CurrencyKind.Tix
                : UserPurchaseService.CurrencyKind.Robux;

            var purchaseService = new UserPurchaseService();
            var (success, error) = await purchaseService
                .PurchaseAssetAsync(connStr, userId, productId, currencyKind, cancellationToken)
                .ConfigureAwait(false);

            if (!success)
            {
                var errorPayload = new
                {
                    showDivID = "TransactionFailureView",
                    title = "Error",
                    errorMsg = error ?? "Purchase failed.",
                    statusCode = 500
                };
                return Ok(errorPayload);
            }

            string assetName = "Item";
            string assetType = "Item";
            string sellerName = "Seller";
            long price = expectedPrice;

            try
            {
                await using var conn = new NpgsqlConnection(connStr);
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

                const string sql = @"select a.name, a.asset_type_id, u.user_name, a.price
from assets a
left join users u on u.user_id = a.owner_user_id
where a.asset_id = @aid";

                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("aid", productId);
                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    if (!reader.IsDBNull(0))
                        assetName = reader.GetString(0);
                    if (!reader.IsDBNull(1))
                        assetType = "Item";
                    if (!reader.IsDBNull(2))
                        sellerName = reader.GetString(2);
                    if (!reader.IsDBNull(3))
                        price = reader.GetInt64(3);
                }
            }
            catch
            {
            }

            var payload = new
            {
                statusCode = 200,
                Price = price,
                AssetType = assetType,
                AssetName = assetName,
                SellerName = sellerName,
                TransactionVerb = "purchased ",
                AssetID = productId
            };

            return Ok(payload);
        }
    }
}
