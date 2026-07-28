using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Economy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Users;

namespace Website.Controllers.Frontend;

[ApiController]
public class MoneyApiController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public MoneyApiController(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    private long? GetCurrentUserId()
    {
        var (isValid, userId) = AuthenticationHelper.GetCurrentUserId(User);
        return isValid ? userId : null;
    }

    private static string WrapD(object data)
    {
        return JsonSerializer.Serialize(new { d = JsonSerializer.Serialize(data) });
    }

    [Authorize]
    [HttpPost("My/Money.aspx/GetSummary")]
    public IActionResult GetSummary([FromBody] GetSummaryRequest? request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Content(WrapD(new { HasCurrencyOperationError = false, Total = "0" }), "application/json");

        try
        {
            var connStr = _configuration.GetConnectionString("Default");
            if (string.IsNullOrEmpty(connStr))
                return Content(WrapD(new { HasCurrencyOperationError = false, Total = "0" }), "application/json");

            using var conn = new Npgsql.NpgsqlConnection(connStr);
            conn.Open();

            using var cmd = new Npgsql.NpgsqlCommand("SELECT robux FROM users WHERE user_id = @userId", conn);
            cmd.Parameters.AddWithValue("userId", userId.Value);
            var result = cmd.ExecuteScalar();
            var total = result != null && result != DBNull.Value ? Convert.ToInt64(result).ToString() : "0";

            var response = new Dictionary<string, object>
            {
                ["HasCurrencyOperationError"] = false,
                ["CurrencyOperationErrorMessage"] = "",
                ["Total"] = total,
                ["BuildersClubStipend"] = "",
                ["BuildersClubStipendBonus"] = "",
                ["PendingRobux"] = "",
                ["CurrencyPurchase"] = "",
                ["PremiumPendingRobux"] = "",
                ["PremiumStipend"] = "",
                ["PendingRobuxGamePass"] = "",
                ["PendingRobuxDeveloperExchange"] = "",
                ["PendingRobuxEngagementPayout"] = "",
                ["PendingRobuxGlobalDevExChange"] = "",
                ["Sales"] = "",
                ["GroupPayouts"] = "",
                ["AffiliateRevenue"] = "",
                ["PendingRobuxItemSales"] = ""
            };

            return Content(WrapD(response), "application/json");
        }
        catch (Exception)
        {
            return Content(WrapD(new { HasCurrencyOperationError = true, CurrencyOperationErrorMessage = "Sorry, something went wrong." }), "application/json");
        }
    }

    [Authorize]
    [HttpPost("My/Money.aspx/GetMyItemTrades")]
    public async Task<IActionResult> GetMyItemTrades(
        [FromBody] GetItemTradesRequest? request,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Content(WrapD(new { Data = new object[0], totalCount = 0, tradeWriteEnabled = "True" }), "application/json");

        var statusType = request?.statustype ?? "inbound";
        var startIndex = request?.startindex ?? 0;

        try
        {
            var connStr = _configuration.GetConnectionString("Default");
            if (string.IsNullOrEmpty(connStr))
                return Content(WrapD(new { Data = new object[0], totalCount = 0, tradeWriteEnabled = "True" }), "application/json");

            var tradeQueries = new TradeQueries(connStr);
            var trades = await tradeQueries.GetUserTradesAsync(userId.Value, statusType, startIndex, ct)
                .ConfigureAwait(false);
            var totalCount = await tradeQueries.GetUserTradeCountAsync(userId.Value, statusType, ct)
                .ConfigureAwait(false);

            var data = new string[trades.Count];
            for (int i = 0; i < trades.Count; i++)
            {
                data[i] = JsonSerializer.Serialize(trades[i]);
            }

            var response = new Dictionary<string, object>
            {
                ["Data"] = data,
                ["totalCount"] = totalCount,
                ["tradeWriteEnabled"] = "True"
            };

            return Content(WrapD(response), "application/json");
        }
        catch (Exception)
        {
            return Content(WrapD(new { Data = new object[0], totalCount = 0, tradeWriteEnabled = "True" }), "application/json");
        }
    }

    [Authorize]
    [HttpPost("My/Money.aspx/GetMyTransactions")]
    public IActionResult GetMyTransactions(
        [FromBody] GetTransactionsRequest? request,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Content(WrapD(new { Data = new object[0], StartIndex = 0, TotalCount = 0 }), "application/json");

        var response = new Dictionary<string, object>
        {
            ["Data"] = new object[0],
            ["StartIndex"] = request?.startindex ?? 0,
            ["TotalCount"] = 0
        };

        return Content(WrapD(response), "application/json");
    }

    public class GetSummaryRequest
    {
        [System.Text.Json.Serialization.JsonPropertyName("timePeriod")]
        public string? TimePeriod { get; set; }
    }

    public class GetItemTradesRequest
    {
        [System.Text.Json.Serialization.JsonPropertyName("statustype")]
        public string? statustype { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("startindex")]
        public int startindex { get; set; }
    }

    public class GetTransactionsRequest
    {
        [System.Text.Json.Serialization.JsonPropertyName("transactiontype")]
        public string? transactiontype { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("startindex")]
        public int startindex { get; set; }
    }
}
