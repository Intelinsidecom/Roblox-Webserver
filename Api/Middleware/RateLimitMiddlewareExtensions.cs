using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Api.Middleware;

public static class RateLimitMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimit(this IApplicationBuilder app, RateLimitOptions options)
    {
        return app.UseMiddleware<RateLimitMiddleware>(Options.Create(options));
    }

    public static IApplicationBuilder UseRateLimit(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RateLimitMiddleware>();
    }
}
