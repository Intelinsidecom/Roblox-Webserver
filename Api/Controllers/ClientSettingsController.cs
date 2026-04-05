using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    public class ClientSettingsController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public ClientSettingsController(IWebHostEnvironment env)
        {
            _env = env;
        }

        // Matches RCCService/Client expectations:
        //   GET http://clientsettings.api.&lt;basehost&gt;/Setting/QuietGet/{group}/?apiKey=...
        // Example group for RCCService: "RCCService2015" (when SettingsKey=2015 in registry)
        // Example group for Bootstrapper: "WindowsBootstrapperSettings"
        [HttpGet]
        [Route("Setting/QuietGet/{group}")]
        public async Task<IActionResult> QuietGet([FromRoute] string group, [FromQuery] string? apiKey)
        {
            if (string.IsNullOrEmpty(group))
                return NotFound();

            string settingsPath;
            string jsonContent;

            // Handle WindowsBootstrapperSettings group
            if (group.Equals("WindowsBootstrapperSettings", StringComparison.OrdinalIgnoreCase))
            {
                settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FFLAG", "WindowsBootstrapperSettings.json");
                
                if (!System.IO.File.Exists(settingsPath))
                    return NotFound();

                try
                {
                    jsonContent = await System.IO.File.ReadAllTextAsync(settingsPath, Encoding.UTF8);
                }
                catch
                {
                    return NotFound();
                }

                return Content(jsonContent, "application/json", Encoding.UTF8);
            }

            // Handle RCCService groups (original logic)
            const string rccPrefix = "RCCService";
            if (!group.StartsWith(rccPrefix, StringComparison.OrdinalIgnoreCase))
                return NotFound();

            var accessKey = group.Substring(rccPrefix.Length); // "2015" from "RCCService2015"
            if (string.IsNullOrWhiteSpace(accessKey))
                return NotFound();

            var fflagDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FFLAG");
            settingsPath = Path.Combine(fflagDirectory, accessKey + ".json");

            if (!System.IO.File.Exists(settingsPath))
                return NotFound();

            try
            {
                jsonContent = await System.IO.File.ReadAllTextAsync(settingsPath, Encoding.UTF8);
            }
            catch
            {
                // If we cannot read the file for some reason, behave as if no settings exist.
                return NotFound();
            }

            // Return raw JSON content as expected by LoadClientSettingsFromString.
            return Content(jsonContent, "application/json", Encoding.UTF8);
        }
    }
}
