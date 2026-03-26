using Microsoft.AspNetCore.Http;

namespace Website.Middleware
{
    public class AntiCacheMiddleware
    {
        private readonly RequestDelegate _next;

        public AntiCacheMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            context.Response.Headers["Pragma"] = "no-cache";
            context.Response.Headers["Expires"] = "0";
            context.Response.Headers["Connection"] = "close";

            await _next(context);
        }
    }
}
