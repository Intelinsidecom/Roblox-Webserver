using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Website.Hubs;

namespace Website.Services;

public static class NotificationBroadcaster
{
    private static long _sequenceNumber;

    public static async Task BroadcastNewNotification(
        IHubContext<NotificationHub> hubContext,
        long userId,
        CancellationToken cancellationToken = default)
    {
        var seq = System.Threading.Interlocked.Increment(ref _sequenceNumber);
        var detail = JsonSerializer.Serialize(new { Type = "NewNotification" });

        await hubContext.Clients
            .Group($"user_{userId}")
            .SendAsync("notification", "NotificationStream", detail, seq, cancellationToken)
            .ConfigureAwait(false);
    }
}
