using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Website.Hubs;

public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId.HasValue)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId.Value}");
        }

        await Clients.Caller.SendAsync("subscriptionStatus", "Subscribed",
            System.Text.Json.JsonSerializer.Serialize(new { SequenceNumber = 0, MillisecondsBeforeHandlingReconnect = 0 }));

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId.HasValue)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId.Value}");
        }

        await base.OnDisconnectedAsync(exception);
    }

    private long? GetUserId()
    {
        var claim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(claim) || !long.TryParse(claim, out var userId) || userId <= 0)
            return null;
        return userId;
    }
}
