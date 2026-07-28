using System.Security.Claims;
using Website.Services;

namespace Website.Middleware;

public class PresenceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly PresenceTracker _tracker;

    public PresenceMiddleware(RequestDelegate next, PresenceTracker tracker)
    {
        _next = next;
        _tracker = tracker;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userIdStr = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(userIdStr) && long.TryParse(userIdStr, out var userId) && userId > 0)
            {
                var userAgent = context.Request.Headers.UserAgent.ToString() ?? "";
                var isStudio = userAgent.Contains("RobloxStudio", StringComparison.OrdinalIgnoreCase);
                _tracker.TrackRequest(userId, isStudio);
            }
        }

        await _next(context);
    }
}
