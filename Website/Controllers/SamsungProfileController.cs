using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace Website.Controllers;

[ApiController]
[Route("v2/profile/user/user")]
public class SamsungProfileController : ControllerBase
{
    [HttpGet("{userId}")]
    public IActionResult GetProfile(string userId)
    {
        var xml = $"""<profile><loginID>test-{userId}@example.com</loginID><countryCode>US</countryCode></profile>""";
        return Content(xml, "application/xml", Encoding.UTF8);
    }
}
