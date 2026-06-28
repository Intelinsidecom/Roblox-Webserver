using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Net;

namespace Website.Middleware;

/// <summary>
/// Rate limiting middleware to prevent API abuse
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly int _defaultTimeWindowSeconds;
    private readonly int _maxRequestsPerWindow = 100;

    public RateLimitingMiddleware(RequestDelegate next, IMemoryCache cache, ILogger<RateLimitingMiddleware> logger, IConfiguration configuration)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
        _defaultTimeWindowSeconds = configuration.GetValue<int>("Apis:DefaultApiRateLimitTimeInSeconds", 10);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!IsApiRoute(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var clientIdentifier = GetClientIdentifier(context);
        var cacheKey = $"rate_limit_{clientIdentifier}_{context.Request.Path}";
        
        if (_cache.TryGetValue(cacheKey, out int requestCount))
        {
            if (requestCount >= _maxRequestsPerWindow)
            {
                _logger.LogWarning("Rate limit exceeded for {ClientIdentifier} on {Path}. Requests: {Count}", 
                    clientIdentifier, context.Request.Path, requestCount);
                
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
                return;
            }
            
            _cache.Set(cacheKey, requestCount + 1, TimeSpan.FromSeconds(_defaultTimeWindowSeconds));
        }
        else
        {
            _cache.Set(cacheKey, 1, TimeSpan.FromSeconds(_defaultTimeWindowSeconds));
        }

        await _next(context);
    }

    private static bool IsApiRoute(string path)
    {
        return path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase) ||
               path.StartsWith("/places/", StringComparison.OrdinalIgnoreCase) ||
               path.StartsWith("/thumbs/", StringComparison.OrdinalIgnoreCase) ||
               path.StartsWith("/item-thumbnails", StringComparison.OrdinalIgnoreCase) ||
               path.StartsWith("/avatar-thumbnails", StringComparison.OrdinalIgnoreCase) ||
               path.StartsWith("/headshot-thumbnail", StringComparison.OrdinalIgnoreCase) ||
               path.StartsWith("/bust-thumbnail", StringComparison.OrdinalIgnoreCase) ||
               path.StartsWith("/outfit-thumbnail", StringComparison.OrdinalIgnoreCase) ||
               path.StartsWith("/game-thumbnails", StringComparison.OrdinalIgnoreCase) ||
               path.StartsWith("/game-icons", StringComparison.OrdinalIgnoreCase) ||
               path.StartsWith("/thumbnail/", StringComparison.OrdinalIgnoreCase) ||
               path.StartsWith("/Thumbs/", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetClientIdentifier(HttpContext context)
    {
        var userIdClaim = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim != null && !string.IsNullOrWhiteSpace(userIdClaim.Value))
        {
            return $"user_{userIdClaim.Value}";
        }

        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        return $"ip_{ipAddress ?? "unknown"}";
    }
}

/// <summary>
/// Extension method to register the rate limiting middleware
/// </summary>
public static class RateLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RateLimitingMiddleware>();
    }
}
