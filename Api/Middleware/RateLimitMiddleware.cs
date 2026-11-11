using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Api.Middleware;

/// <summary>
/// Simple in-memory sliding window rate limiter based on client IP.
/// Defaults to 60 requests per 60-second window. Returns 429 when exceeded.
/// </summary>
public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RateLimitOptions _options;
    private readonly ConcurrentDictionary<string, List<long>> _requestLogs = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new();

    public RateLimitMiddleware(RequestDelegate next, IOptions<RateLimitOptions> options)
    {
        _next = next;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = GetClientIp(context);

        if (string.IsNullOrEmpty(clientIp))
        {
            await _next(context);
            return;
        }

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var windowStart = now - _options.WindowSeconds;

        // Get or create the semaphore for this IP to serialize access
        var semaphore = _semaphores.GetOrAdd(clientIp, _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync();
        try
        {
            var requests = _requestLogs.GetOrAdd(clientIp, _ => new List<long>());

            // Remove expired entries outside the sliding window
            requests.RemoveAll(ts => ts <= windowStart);

            if (requests.Count >= _options.MaxRequests)
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.Headers.Append("Retry-After", _options.WindowSeconds.ToString());
                await context.Response.WriteAsync("{\"errors\":[{\"code\":0,\"message\":\"Rate limit exceeded\"}]}");
                return;
            }

            requests.Add(now);
        }
        finally
        {
            semaphore.Release();
        }

        await _next(context);
    }

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

public class RateLimitOptions
{
    public int MaxRequests { get; set; } = 60;
    public int WindowSeconds { get; set; } = 60;
}
