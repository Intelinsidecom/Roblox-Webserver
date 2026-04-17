using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Website.Middleware
{
    /// <summary>
    /// Middleware to redirect 401 and 400 status codes to /404 for page requests only (not API requests)
    /// </summary>
    public class PageErrorRedirectMiddleware
    {
        private readonly RequestDelegate _next;

        public PageErrorRedirectMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            // Only process page requests (not API requests)
            var path = context.Request.Path.Value?.ToLower() ?? "";
            var isApiRequest = path.StartsWith("/api/") || 
                               path.StartsWith("/v1/") ||
                               path.StartsWith("/game/") ||
                               context.Request.Headers.ContainsKey("X-Requested-With") ||
                               context.Request.ContentType?.Contains("application/json") == true;

            // Skip if it's an API request
            if (isApiRequest)
            {
                return;
            }

            // Check if response is 401 or 400
            var statusCode = context.Response.StatusCode;
            if (statusCode == 401 || statusCode == 400)
            {
                // Don't redirect if already at 404 page to prevent infinite loop
                if (!path.StartsWith("/404"))
                {
                    context.Response.StatusCode = 302; // Found (redirect)
                    context.Response.Headers["Location"] = "/404";
                }
            }
        }
    }
}
