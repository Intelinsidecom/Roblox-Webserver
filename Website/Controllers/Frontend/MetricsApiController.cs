using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Website.Controllers.Frontend;

[ApiController]
public class MetricsApiController : ControllerBase
{
    [Authorize]
    [HttpPost("v1/performance/send-measurement")]
    public IActionResult SendMeasurement()
    {
        return Ok();
    }
}
