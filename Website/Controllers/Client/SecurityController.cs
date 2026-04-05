using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Website.Controllers.Client
{
    [ApiController]
    
    public class SecurityController : Controller
    {
        [HttpGet("GetSecurityKeys")]
        public IActionResult GetSecurityKey()
        {
            var response = new
            {
                data = new[]
                {
                    // Roblox client version strings from Network/API.cpp
                    // Format: "^" + classDescriptors + (char)17 + other components
                    "test",           // For DebugLocalRccServerConnection (from Client.cpp:171)
                    "^7O^lE^",        // Basic version pattern from initVersion1/2 + initWithoutSecurity
                    "^7O^lE^\u0011",  // Version with explicit control character
                    "dev",            // Development versions
                    "0.1.0.0",        // Version fallback
                    "0.1.0.1",
                    "0.1.0.2"
                }
            };

            Response.Headers["Content-Type"] = "application/json";
            return Json(response);
        }

        [HttpGet("GetAllowedSecurityVersions")]
        public IActionResult GetAllowedSecurityVersions()
        {
            // Match Rocknet's exact response format (including the JSON syntax error)
            Response.Headers["Content-Type"] = "application/json";
            return Content("{\"data\":[0.271.0pcplayer\",\"0.360.0pcplayer\",\"INTERNALiosapp\",\"0.450.0pcplayer\",\"0.205.0pcplayer\"]}", "application/json");
        }

        [HttpGet("GetAllowedMD5Hashes")]
        // certutil -hashfile <path_to_file> MD5
        public IActionResult GetAllowedMD5Hashes()
        {
            var response = new
            {
                data = new[]
                {
                    "8bdf3e7b196d3d70f53e5c470489f852",
                    "random"
                }
            };

            Response.Headers["Content-Type"] = "application/json";
            return Json(response);
        }

        [HttpGet("GetCurrentClientVersionUpload")]
        public IActionResult GetCurrentClientVersionUpload([FromQuery] string binaryType)
        {
            Response.Headers["Content-Type"] = "application/json; charset=utf-8";
            
            if (binaryType == "WindowsStudio")
            {
                return Content("\"version-phqw6fselwuqtrvr\"", "application/json");
            }
            else
            {
                return Content("\"version-d30b938ea76a153e\"", "application/json");
            }
        }

    }
}
