using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Api.Controllers
{
    [ApiController]
    [Route("sign-out")]
    public class SignOutController : ControllerBase
    {
        // POST /sign-out/v1
        [HttpPost("v1")]
        public IActionResult SignOutV1()
        {
            // Expire the session cookie
            Response.Cookies.Append(
                ".ROBLOSECURITY",
                string.Empty,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTimeOffset.UnixEpoch,
                    MaxAge = TimeSpan.Zero
                }
            );

            return Ok();
        }
    }
}
