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

        [HttpGet]
        [Route("Setting/QuietGet/{group}")]
        public async Task<IActionResult> QuietGet([FromRoute] string group, [FromQuery] string? apiKey)
        {
            if (string.IsNullOrEmpty(group))
                return NotFound();

            var fflagDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FFLAG");

            string? settingsPath = null;

            if (!string.IsNullOrEmpty(apiKey))
            {
                var apiKeyPath = Path.Combine(fflagDirectory, $"{group}-{apiKey}.json");
                if (System.IO.File.Exists(apiKeyPath))
                    settingsPath = apiKeyPath;
            }

            if (settingsPath == null)
            {
                var defaultPath = Path.Combine(fflagDirectory, $"{group}.json");
                if (System.IO.File.Exists(defaultPath))
                    settingsPath = defaultPath;
            }

            if (settingsPath == null)
                return NotFound();

            try
            {
                var jsonContent = await System.IO.File.ReadAllTextAsync(settingsPath, Encoding.UTF8);
                return Content(jsonContent, "application/json", Encoding.UTF8);
            }
            catch
            {
                return NotFound();
            }
        }
    }
}
