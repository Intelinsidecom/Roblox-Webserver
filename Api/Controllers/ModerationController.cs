using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("moderation")]
    public class ModerationController : ControllerBase
    {
        [HttpPost("reportabuse")]
        public IActionResult ReportAbuse()
        {
            return Ok();
        }
    }
}
