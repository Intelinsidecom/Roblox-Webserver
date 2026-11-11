using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Api.Middleware;

/// <summary>
/// Stricter per-IP rate limiter specifically for the signup endpoint.
/// Prevents an IP from creating more than MaxSignupsPerWindow accounts
/// within the configured WindowSeconds window.
/// </summary>
public class SignupRateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SignupRateLimitOptions _options;
    private readonly ConcurrentDictionary<string, List<long>> _signupLogs = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new();

    public SignupRateLimitMiddleware(RequestDelegate next, IOptions<SignupRateLimitOptions> options)
    {
        _next = next;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only intercept the signup endpoint
        if (!IsSignupPath(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var clientIp = GetClientIp(context);
        if (string.IsNullOrEmpty(clientIp))
        {
            await _next(context);
            return;
        }

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var windowStart = now - _options.WindowSeconds;

        var semaphore = _semaphores.GetOrAdd(clientIp, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync();
        try
        {
            var timestamps = _signupLogs.GetOrAdd(clientIp, _ => new List<long>());
            timestamps.RemoveAll(ts => ts <= windowStart);

            if (timestamps.Count >= _options.MaxSignupsPerWindow)
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.Headers.Append("Retry-After", _options.WindowSeconds.ToString());
                await context.Response.WriteAsync(
                    "{\"errors\":[{\"code\":0,\"message\":\"Too many account creations from this IP. Try again later.\"}]}");
                return;
            }

            timestamps.Add(now);
        }
        finally
        {
            semaphore.Release();
        }

        await _next(context);
    }

    private static bool IsSignupPath(PathString path)
        => path.HasValue
           && path.Value.StartsWith("/signup", StringComparison.OrdinalIgnoreCase)
           && path.Value.Contains("v1", StringComparison.OrdinalIgnoreCase);

    private static string? GetClientIp(HttpContext context)
    {
        var headers = context.Request.Headers;
        if (headers.TryGetValue("X-Forwarded-For", out var xfFor) && !string.IsNullOrEmpty(xfFor))
            return xfFor.ToString().Split(',')[0].Trim();
        if (headers.TryGetValue("X-Real-IP", out var xReal) && !string.IsNullOrEmpty(xReal))
            return xReal.ToString();
        if (headers.TryGetValue("CF-Connecting-IP", out var cfIp) && !string.IsNullOrEmpty(cfIp))
            return cfIp.ToString();
        if (headers.TryGetValue("True-Client-IP", out var tcIp) && !string.IsNullOrEmpty(tcIp))
            return tcIp.ToString();
        return context.Connection.RemoteIpAddress?.ToString();
    }
}

public class SignupRateLimitOptions
{
    /// <summary>Max signup attempts allowed per IP per window.</summary>
    public int MaxSignupsPerWindow { get; set; } = 10;

    /// <summary>Window size in seconds (default 24 hours).</summary>
    public int WindowSeconds { get; set; } = 86400;
}

public static class SignupRateLimitMiddlewareExtensions
{
    public static IApplicationBuilder UseSignupRateLimit(this IApplicationBuilder app)
        => app.UseMiddleware<SignupRateLimitMiddleware>();
}
