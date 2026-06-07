using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("device")]
public class DeviceController : ControllerBase
{
    [HttpPost("initialize")]
    public IActionResult Initialize()
    {
    var result = new
    {
        browserTrackerId = 0,
        appDeviceIdentifier = (object)null
    };
    
    return Ok(result);
    }
}
