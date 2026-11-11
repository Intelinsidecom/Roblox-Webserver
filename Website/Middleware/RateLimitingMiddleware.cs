using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Website.Middleware;

/// <summary>
/// Global sliding-window rate limiter based on client IP.
/// Applies to every non-healthcheck request. Returns 429 when the per-IP
/// request budget for the current window is exhausted.
/// </summary>
public class RateLimitingMiddleware
{
    private static readonly string[] SkipPrefixes = { "/v2/CreateOrUpdate" };

    private readonly RequestDelegate _next;
    private readonly RateLimitOptions _options;
    private readonly ConcurrentDictionary<string, List<long>> _requestLogs = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new();
    private readonly ILogger<RateLimitingMiddleware> _logger;

    public RateLimitingMiddleware(
        RequestDelegate next,
        IOptions<RateLimitOptions> options,
        ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _options = options.Value;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Never rate-limit the internal health/check endpoint itself, or RCC game-server heartbeats.
        if (context.Request.Path.StartsWithSegments(_options.SkipPath, StringComparison.OrdinalIgnoreCase) ||
            Array.Exists(SkipPrefixes, prefix => context.Request.Path.StartsWithSegments(prefix, StringComparison.OrdinalIgnoreCase)))
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
            var requests = _requestLogs.GetOrAdd(clientIp, _ => new List<long>());
            requests.RemoveAll(ts => ts <= windowStart);

            if (requests.Count >= _options.MaxRequests)
            {
                _logger.LogWarning(
                    "Rate limit exceeded for {Ip} on {Path}. Requests in window: {Count}",
                    clientIp, context.Request.Path, requests.Count);

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
    /// <summary>Maximum number of requests allowed per sliding window per IP.</summary>
    public int MaxRequests { get; set; } = 60;

    /// <summary>Sliding window size in seconds.</summary>
    public int WindowSeconds { get; set; } = 60;

    /// <summary>Path prefix to skip entirely (e.g. "/health").</summary>
    public string SkipPath { get; set; } = "/health";
}

/// <summary>
/// Extension method to register the rate limiting middleware.
/// </summary>
public static class RateLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app, RateLimitOptions options)
        => app.UseMiddleware<RateLimitingMiddleware>(Options.Create(options));

    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app)
        => app.UseMiddleware<RateLimitingMiddleware>();
}
