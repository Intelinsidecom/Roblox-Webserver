using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Website.Middleware
{
    /// <summary>
    /// Middleware to redirect 401 to login and 400 to /404 for page requests only (not API requests)
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

            if (context.Response.HasStarted)
            {
                return;
            }

            var path = context.Request.Path.Value?.ToLower() ?? "";
            var isApiRequest = path.StartsWith("/api/") || 
                               path.StartsWith("/v1/") ||
                               path.StartsWith("/game/") ||
                               path.StartsWith("/gs/") ||
                               path.StartsWith("/login/") ||
                               context.Request.Headers.ContainsKey("X-Requested-With") ||
                               context.Request.ContentType?.Contains("application/json") == true;

            if (isApiRequest)
            {
                return;
            }

            var statusCode = context.Response.StatusCode;

            if (statusCode == 401)
            {
                if (!path.StartsWith("/login") && !path.StartsWith("/404"))
                {
                    context.Response.StatusCode = 302;
                    context.Response.Headers["Location"] = "/login?returnUrl=" + Uri.EscapeDataString(context.Request.Path.Value + context.Request.QueryString.Value);
                    context.Response.ContentLength = null;
                    context.Response.ContentType = null;
                }
            }
            else if (statusCode == 400)
            {
                if (!path.StartsWith("/404") && !path.StartsWith("/develop/"))
                {
                    context.Response.StatusCode = 302;
                    context.Response.Headers["Location"] = "/404";
                    context.Response.ContentLength = null;
                    context.Response.ContentType = null;
                }
            }
        }
    }
}
