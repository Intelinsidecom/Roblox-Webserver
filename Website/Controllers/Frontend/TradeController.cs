using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Economy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Users;
using Website.Hubs;
using Website.Services;

namespace Website.Controllers.Frontend;

[ApiController]
[Route("Trade")]
public class TradeController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly TradeQueries _tradeQueries;
    private readonly IHubContext<NotificationHub> _hubContext;

    public TradeController(IConfiguration configuration, IHubContext<NotificationHub> hubContext)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        var connStr = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' not found.");
        _tradeQueries = new TradeQueries(connStr);
    }

    private static string Serialize(object obj)
    {
        return JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            PropertyNamingPolicy = null
        });
    }

    [Authorize]
    [HttpPost("TradeHandler.ashx")]
    public async Task<IActionResult> HandleTrade(
        [FromForm] string? cmd,
        [FromForm] string? TradeJSON,
        [FromForm] long? TradeID,
        CancellationToken cancellationToken = default)
    {
        var (isValid, userId) = AuthenticationHelper.GetCurrentUserId(User);
        if (!isValid)
        {
            return Content(Serialize(new { success = false, data = "", msg = "User not authenticated" }), "application/json");
        }

        if (string.IsNullOrWhiteSpace(cmd))
        {
            return Content(Serialize(new { success = false, data = "", msg = "No command specified" }), "application/json");
        }

        try
        {
            return cmd.ToLowerInvariant() switch
            {
                "send" => await HandleSendAsync(userId, TradeJSON, cancellationToken).ConfigureAwait(false),
                "pull" => await HandlePullAsync(userId, TradeID, cancellationToken).ConfigureAwait(false),
                "counter" => await HandleCounterAsync(userId, TradeID, TradeJSON, cancellationToken).ConfigureAwait(false),
                "decline" => await HandleDeclineAsync(userId, TradeID, cancellationToken).ConfigureAwait(false),
                "maketrade" => await HandleAcceptAsync(userId, TradeID, TradeJSON, cancellationToken).ConfigureAwait(false),
                _ => Content(Serialize(new { success = false, data = "", msg = $"Unknown command: {cmd}" }), "application/json")
            };
        }
        catch (Exception ex)
        {
            return Content(Serialize(new { success = false, data = "", msg = ex.Message }), "application/json");
        }
    }

    private async Task<IActionResult> HandleSendAsync(
        long userId, string? tradeJson, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(tradeJson))
        {
            return Content(Serialize(new { success = false, data = "", msg = "Trade data is required" }), "application/json");
        }

        var tradeDoc = JsonDocument.Parse(tradeJson);
        var root = tradeDoc.RootElement;

        long? partnerId = null;
        if (root.TryGetProperty("AgentOfferList", out var agentList))
        {
            foreach (var agent in agentList.EnumerateArray())
            {
                if (agent.TryGetProperty("AgentID", out var agentIdProp))
                {
                    var agentId = agentIdProp.GetInt64();
                    if (agentId != userId)
                    {
                        partnerId = agentId;
                        break;
                    }
                }
            }
        }

        if (partnerId == null || partnerId.Value <= 0)
        {
            return Content(Serialize(new { success = false, data = "", msg = "Invalid trade partner" }), "application/json");
        }

        var tradeId = await _tradeQueries.CreateTradeAsync(userId, partnerId.Value, tradeJson, ct)
            .ConfigureAwait(false);

        var connStr = _configuration.GetConnectionString("Default");
        if (!string.IsNullOrEmpty(connStr))
        {
            var senderName = "";
            try
            {
                await using var nameConn = new Npgsql.NpgsqlConnection(connStr);
                await nameConn.OpenAsync(ct).ConfigureAwait(false);
                using var nameCmd = new Npgsql.NpgsqlCommand("SELECT user_name FROM users WHERE user_id = @id", nameConn);
                nameCmd.Parameters.AddWithValue("id", userId);
                var result = await nameCmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
                if (result != null && result != DBNull.Value) senderName = result.ToString() ?? "";
            }
            catch { }

            var notificationService = new NotificationService(connStr);
            await notificationService.CreateNotificationAsync(
                partnerId.Value,
                "TradeRequestReceived",
                userId,
                senderName,
                "Trade",
                tradeId,
                "",
                ct).ConfigureAwait(false);

            await NotificationBroadcaster.BroadcastNewNotification(_hubContext, partnerId.Value, ct)
                .ConfigureAwait(false);
        }

        return Content(Serialize(new { success = true, data = tradeId.ToString(), msg = "" }), "application/json");
    }

    private async Task<IActionResult> HandlePullAsync(
        long userId, long? tradeId, CancellationToken ct)
    {
        if (tradeId == null || tradeId.Value <= 0)
        {
            return Content(Serialize(new { success = false, data = "", msg = "Invalid trade ID" }), "application/json");
        }

        var ownership = await _tradeQueries.ValidateTradeOwnershipAsync(tradeId.Value, userId, ct)
            .ConfigureAwait(false);

        if (!ownership.IsValid)
        {
            return Content(Serialize(new { success = false, data = "", msg = "Trade not found or access denied" }), "application/json");
        }

        var tradeDoc = await _tradeQueries.GetTradeByIdAsync(tradeId.Value, userId, ct)
            .ConfigureAwait(false);

        if (tradeDoc == null)
        {
            return Content(Serialize(new { success = false, data = "", msg = "Trade not found" }), "application/json");
        }

        var tradeJson = tradeDoc.RootElement.GetRawText();
        return Content(Serialize(new { success = true, data = tradeJson, msg = "" }), "application/json");
    }

    private async Task<IActionResult> HandleCounterAsync(
        long userId, long? tradeId, string? tradeJson, CancellationToken ct)
    {
        if (tradeId == null || tradeId.Value <= 0)
        {
            return Content(Serialize(new { success = false, data = "", msg = "Invalid trade ID" }), "application/json");
        }

        if (string.IsNullOrWhiteSpace(tradeJson))
        {
            return Content(Serialize(new { success = false, data = "", msg = "Trade data is required" }), "application/json");
        }

        var ownership = await _tradeQueries.ValidateTradeOwnershipAsync(tradeId.Value, userId, ct)
            .ConfigureAwait(false);

        if (!ownership.IsValid)
        {
            return Content(Serialize(new { success = false, data = "", msg = "Trade not found or access denied" }), "application/json");
        }

        var newTradeId = await _tradeQueries.CounterTradeAsync(tradeId.Value, userId, tradeJson, ct)
            .ConfigureAwait(false);

        if (newTradeId <= 0)
        {
            return Content(Serialize(new { success = false, data = "", msg = "Failed to create counter trade" }), "application/json");
        }

        return Content(Serialize(new { success = true, data = newTradeId.ToString(), msg = "" }), "application/json");
    }

    private async Task<IActionResult> HandleDeclineAsync(
        long userId, long? tradeId, CancellationToken ct)
    {
        if (tradeId == null || tradeId.Value <= 0)
        {
            return Content(Serialize(new { success = false, data = "", msg = "Invalid trade ID" }), "application/json");
        }

        var ownership = await _tradeQueries.ValidateTradeOwnershipAsync(tradeId.Value, userId, ct)
            .ConfigureAwait(false);

        if (!ownership.IsValid)
        {
            return Content(Serialize(new { success = false, data = "", msg = "Trade not found or access denied" }), "application/json");
        }

        var success = await _tradeQueries.DeclineTradeAsync(tradeId.Value, userId, ct)
            .ConfigureAwait(false);

        return Content(Serialize(new { success, data = "", msg = success ? "" : "Failed to decline trade" }), "application/json");
    }

    private async Task<IActionResult> HandleAcceptAsync(
        long userId, long? tradeId, string? tradeJson, CancellationToken ct)
    {
        if (tradeId == null || tradeId.Value <= 0)
        {
            return Content(Serialize(new { success = false, data = "", msg = "Invalid trade ID" }), "application/json");
        }

        var ownership = await _tradeQueries.ValidateTradeOwnershipAsync(tradeId.Value, userId, ct)
            .ConfigureAwait(false);

        if (!ownership.IsValid)
        {
            return Content(Serialize(new { success = false, data = "", msg = "Trade not found or access denied" }), "application/json");
        }

        var success = await _tradeQueries.AcceptTradeAsync(tradeId.Value, userId, tradeJson ?? "{}", ct)
            .ConfigureAwait(false);

        return Content(Serialize(new { success, data = "", msg = success ? "" : "Failed to accept trade" }), "application/json");
    }
}
