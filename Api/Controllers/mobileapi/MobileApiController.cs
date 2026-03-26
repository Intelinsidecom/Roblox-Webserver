using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.mobileapi;

[ApiController]
[Route("mobileapi")]
public class MobileApiController : ControllerBase
{
    [HttpGet("check-app-version")]
    public IActionResult CheckAppVersion()
    {
        return Ok(new
        {
            data = new
            {
                UpgradeAction = "None"
            }
        });
    }
}
