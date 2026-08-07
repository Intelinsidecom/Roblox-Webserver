using Microsoft.AspNetCore.Mvc;

namespace Website.Controllers;

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
