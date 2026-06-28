using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api.Data;

namespace Api.Controllers
{
    [ApiController]
    public class ClientSettingsController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly AppDbContext _db;

        public ClientSettingsController(IWebHostEnvironment env, AppDbContext db)
        {
            _env = env;
            _db = db;
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

        [HttpGet]
        [Route("Setting/QuietGet/GetCurrentClientVersionUpload")]
        public async Task<IActionResult> GetCurrentClientVersionUpload([FromQuery] string? apiKey, [FromQuery] string? binaryType)
        {
            if (string.IsNullOrEmpty(binaryType))
                return BadRequest("binaryType parameter is required");

            string? version = null;

            try
            {
                var setupRecord = await _db.Setup.OrderByDescending(s => s.Id).FirstOrDefaultAsync();

                if (setupRecord == null)
                    return NotFound("No setup record found");

                switch (binaryType.ToLower())
                {
                    case "windowsstudio":
                        version = setupRecord.CurrentStudioVersion;
                        break;
                    case "windowsplayer":
                        version = setupRecord.CurrentWindowsplayerVersion;
                        break;
                    default:
                        return BadRequest($"Unknown binaryType: {binaryType}");
                }

                if (string.IsNullOrEmpty(version))
                    return NotFound($"No version found for binaryType: {binaryType}");

                return Content($"\"{version}\"", "application/json", Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving version: {ex.Message}");
            }
        }
    }
}
