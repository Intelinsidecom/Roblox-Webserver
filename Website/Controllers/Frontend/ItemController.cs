using System;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Users;
using Website.Hubs;
using Website.Services;

namespace Website.Controllers
{
    [ApiController]
    [Route("API/Item.ashx")]
    public class ItemController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHubContext<NotificationHub> _hubContext;

        public ItemController(IConfiguration configuration, IHubContext<NotificationHub> hubContext)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _hubContext = hubContext;
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

            using (var checkConn = new NpgsqlConnection(connStr))
            {
                await checkConn.OpenAsync(cancellationToken).ConfigureAwait(false);
                using var checkCmd = new NpgsqlCommand("select on_sale from assets where asset_id = @aid", checkConn);
                checkCmd.Parameters.AddWithValue("aid", productId);
                var onSaleObj = await checkCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                if (onSaleObj == null || !(bool)onSaleObj)
                {
                    return Ok(new { showDivID = "TransactionFailureView", title = "Error", errorMsg = "Asset is not for sale.", statusCode = 500 });
                }
            }

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

            _ = Task.Run(async () =>
            {
                try
                {
                    long? sellerUserId = null;
                    string assetName = "";
                    string buyerName = "";
                    string sellerName = "";

                    await using var notifyConn = new NpgsqlConnection(connStr);
                    await notifyConn.OpenAsync(cancellationToken).ConfigureAwait(false);

                    const string assetSql = @"SELECT owner_user_id, name FROM assets WHERE asset_id = @aid";
                    await using var assetCmd = new NpgsqlCommand(assetSql, notifyConn);
                    assetCmd.Parameters.AddWithValue("aid", productId);
                    await using var assetReader = await assetCmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                    if (await assetReader.ReadAsync(cancellationToken).ConfigureAwait(false))
                    {
                        if (!assetReader.IsDBNull(0)) sellerUserId = assetReader.GetInt64(0);
                        if (!assetReader.IsDBNull(1)) assetName = assetReader.GetString(1);
                    }

                    string buyerNameResult = await UserQueries.GetUserNameByIdAsync(connStr, userId).ConfigureAwait(false) ?? "";
                    if (sellerUserId.HasValue)
                        sellerName = await UserQueries.GetUserNameByIdAsync(connStr, sellerUserId.Value).ConfigureAwait(false) ?? "";

                    if (sellerUserId.HasValue && sellerUserId.Value != userId)
                    {
                        var svc = new NotificationService(connStr);
                        await svc.CreateNotificationAsync(
                            sellerUserId.Value,
                            "AssetPurchased",
                            userId,
                            buyerNameResult,
                            "Asset",
                            productId,
                            assetName,
                            cancellationToken
                        ).ConfigureAwait(false);
                        await NotificationBroadcaster.BroadcastNewNotification(_hubContext, sellerUserId.Value, cancellationToken).ConfigureAwait(false);
                    }
                }
                catch
                {
                }
            }, cancellationToken);

            string assetDisplayName = "Item";
            string assetType = "Item";
            string sellerDisplayName = "Seller";
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
                        assetDisplayName = reader.GetString(0);
                    if (!reader.IsDBNull(1))
                        assetType = "Item";
                    if (!reader.IsDBNull(2))
                        sellerDisplayName = reader.GetString(2);
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
                AssetName = assetDisplayName,
                SellerName = sellerDisplayName,
                TransactionVerb = "purchased ",
                AssetID = productId
            };

            return Ok(payload);
        }
    }
}
