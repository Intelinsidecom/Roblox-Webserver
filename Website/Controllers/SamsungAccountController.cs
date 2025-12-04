using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace Website.Controllers;

[ApiController]
public class SamsungAccountController : ControllerBase
{
    [HttpGet("mobile/account/check.do")]
    public IActionResult Check(
        [FromQuery] string? serviceID,
        [FromQuery] string? actionID,
        [FromQuery] string? countryCode,
        [FromQuery] string? languageCode,
        [FromQuery] string? returnType)
    {
        var scheme = string.IsNullOrWhiteSpace(Request.Scheme) ? "https" : Request.Scheme;
        var host = Request.Host.HasValue ? Request.Host.Value : "account.samsung.com";
        var redirectUrl = $"{scheme}://{host}/mobile/account/complete.do?code=FAKE_CODE";
        return Redirect(redirectUrl);
    }
}
