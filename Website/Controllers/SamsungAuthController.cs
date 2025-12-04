using Microsoft.AspNetCore.Mvc;

namespace Website.Controllers;

[ApiController]
[Route("auth/oauth2")]
public class SamsungAuthController : ControllerBase
{
    [HttpPost("token")]
    public IActionResult Token(
        [FromQuery] string? grant_type,
        [FromQuery] string? code,
        [FromQuery] string? client_id,
        [FromQuery] string? client_secret)
    {
        var response = new
        {
            access_token = "FAKE_ACCESS_TOKEN",
            userId = "1234567890"
        };

        return new JsonResult(response);
    }
}
