using Microsoft.AspNetCore.Mvc;

namespace Website.Controllers
{
    [ApiController]
    [Route("mobileapi")]
    public class MobileApiController : ControllerBase
    {
        // GET /mobileapi/check-app-version?appVersion=...
        [HttpGet("check-app-version")]
        public IActionResult CheckAppVersion([FromQuery] string appVersion)
        {
            // Always return true for compatibility with legacy/mobile clients
            return Content("true", "text/plain");
        }
    }
}
