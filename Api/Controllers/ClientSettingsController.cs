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
        [HttpGet]
        [Route("Setting/QuietGet/{group}")]
        public async Task<IActionResult> QuietGet([FromRoute] string group, [FromQuery] string? apiKey)
        {
            if (string.IsNullOrEmpty(group))
                return NotFound();

            // For RCCService: group is "RCCService" + SettingsKey (e.g., "RCCService2015").
            const string rccPrefix = "RCCService";
            if (!group.StartsWith(rccPrefix, StringComparison.OrdinalIgnoreCase))
                return NotFound();

            var accessKey = group.Substring(rccPrefix.Length); // "2015" from "RCCService2015"
            if (string.IsNullOrWhiteSpace(accessKey))
                return NotFound();

            var fflagDirectory = Path.Combine(_env.ContentRootPath, "FFLAG");
            var settingsPath = Path.Combine(fflagDirectory, accessKey + ".json");

            if (!System.IO.File.Exists(settingsPath))
                return NotFound();

            string json;
            try
            {
                json = await System.IO.File.ReadAllTextAsync(settingsPath, Encoding.UTF8);
            }
            catch
            {
                // If we cannot read the file for some reason, behave as if no settings exist.
                return NotFound();
            }

            // Return raw JSON content as expected by LoadClientSettingsFromString.
            return Content(json, "application/json", Encoding.UTF8);
        }
    }
}
