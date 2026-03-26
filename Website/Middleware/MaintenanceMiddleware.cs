using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Npgsql;

namespace Website.Middleware
{

    public class LockdownMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ICompositeViewEngine _viewEngine;
        private static readonly string[] AllowedExtensions = { ".js", ".css", ".png", ".jpg", ".jpeg", ".webp", ".gif" };

        public LockdownMiddleware(RequestDelegate next, IConfiguration configuration, ICompositeViewEngine viewEngine)
        {
            _next = next;
            _configuration = configuration;
            _viewEngine = viewEngine;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var connectionString = _configuration.GetConnectionString("Default");
            var isLockdownEnabled = false;

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                try
                {
                    await using var conn = new NpgsqlConnection(connectionString);
                    await conn.OpenAsync();
                    await using var cmd = new NpgsqlCommand("SELECT lockdown_mode_enabled FROM website_settings LIMIT 1", conn);
                    var result = await cmd.ExecuteScalarAsync();
                    if (result is bool enabled)
                    {
                        isLockdownEnabled = enabled;
                    }
                }
                catch {}
            }

            if (!isLockdownEnabled)
            {
                await _next(context);
                return;
            }

            var path = context.Request.Path.Value?.ToLower();
            
            if (path != null && AllowedExtensions.Any(ext => path.EndsWith(ext)))
            {
                await _next(context);
                return;
            }
            const string viewPath = "~/Views/Shared/Maintenance.cshtml";
            var actionContext = new ActionContext(context, new RouteData(), new ActionDescriptor());
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());
            var tempData = new TempDataDictionary(context, new SimpleTempDataProvider());
            string detailedInfo = null;
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                try
                {
                    await using var conn = new NpgsqlConnection(connectionString);
                    await conn.OpenAsync();
                    await using var cmd = new NpgsqlCommand("SELECT lockdown_mode_reason FROM website_settings LIMIT 1", conn);
                    var result = await cmd.ExecuteScalarAsync();
                    if (result is string info && !string.IsNullOrWhiteSpace(info))
                    {
                        detailedInfo = info;
                    }
                }
                catch {}
            }

            viewData["DetailedInfo"] = detailedInfo;

            var view = new ViewResult
            {
                ViewName = viewPath,
                ViewData = viewData,
                TempData = tempData
            };

            await view.ExecuteResultAsync(actionContext);
            return;
        }
    }
}
