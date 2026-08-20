using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Users;

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
        appDeviceIdentifier = "generated-device-id"
    };
    
    return Ok(result);
    }

    [HttpPost("client-status/set")]
    public async Task<IActionResult> SetClientStatus(
        [FromQuery] string browserTrackerId,
        [FromQuery] string status,
        [FromServices] IConfiguration config)
    {
        if (string.IsNullOrWhiteSpace(browserTrackerId))
            return Ok();

        try
        {
            var connStr = config.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
                return Ok();

            await UserQueries.UpdateLastActivityBySessionTokenAsync(connStr, browserTrackerId);
        }
        catch
        {
        }

        return Ok();
    }
}
