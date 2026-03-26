using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("device")]
public class DeviceController : ControllerBase
{
    [HttpPost("initialize")]
    public IActionResult Initialize()
    {
        return Ok(new { 
            browserTrackerId = "20000000000"
        });
    }
}
