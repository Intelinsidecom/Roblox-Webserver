using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;

namespace RobloxWebserver
{
    public abstract class RobloxRazorPage<TModel> : RazorPage<TModel>
    {
        public bool UseBundledAssets
        {
            get
            {
                var configuration = Context.RequestServices?.GetService(typeof(IConfiguration)) as IConfiguration;
                return configuration != null
                    && bool.TryParse(configuration["Features:UseBundledAssets"], out var bundled)
                    && bundled;
            }
        }
    }
}
