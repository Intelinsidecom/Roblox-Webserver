using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RobloxWebserver.Filters
{
    /// <summary>
    /// Global action filter to automatically apply CSRF protection to all POST requests
    /// </summary>
    public class GlobalValidateAntiForgeryTokenAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Only apply to POST, PUT, DELETE, PATCH requests
            var method = context.HttpContext.Request.Method;
            if (method == "POST" || method == "PUT" || method == "DELETE" || method == "PATCH")
            {
                // Skip validation for API endpoints that don't use forms (like file uploads, webhooks, etc.)
                var path = context.HttpContext.Request.Path.Value?.ToLower() ?? "";
                
                // List of endpoints to skip CSRF validation
                var skipCsrfPaths = new[]
                {
                    "/api/",           // API endpoints
                    "/webhooks/",       // Webhook endpoints
                    "/upload",          // File upload endpoints that handle their own validation
                    "/thumbnails/",      // Thumbnail generation
                    "/game-assets/",    // Asset serving
                    "/heartbeat"        // Heartbeat endpoints
                };

                var shouldSkip = skipCsrfPaths.Any(skipPath => path.Contains(skipPath));
                
                if (!shouldSkip)
                {
                    try
                    {
                        var antiforgery = context.HttpContext.RequestServices.GetService<IAntiforgery>();
                        if (antiforgery != null)
                        {
                            antiforgery.ValidateRequestAsync(context.HttpContext).GetAwaiter().GetResult();
                        }
                    }
                    catch (AntiforgeryValidationException)
                    {
                        context.Result = new BadRequestObjectResult(new { 
                            success = false, 
                            message = "Invalid request token" 
                        });
                        return;
                    }
                }
            }

            base.OnActionExecuting(context);
        }
    }
}
