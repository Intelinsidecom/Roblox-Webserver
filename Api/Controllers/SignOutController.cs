using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.Extensions.Configuration;

namespace Api.Controllers
{
    [ApiController]
    [Route("sign-out")]
    public class SignOutController : ControllerBase
    {
        // POST /sign-out/v1
        [HttpPost("v1")]
        public IActionResult SignOutV1([FromServices] IConfiguration config)
        {
            // Expire the session cookie
            var isHttps = Request.IsHttps;
            var cookieDomain = config["Auth:CookieDomain"]; // e.g. .example.com
            Response.Cookies.Append(
                ".ROBLOSECURITY",
                string.Empty,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = isHttps,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UnixEpoch,
                    MaxAge = TimeSpan.Zero,
                    Path = "/",
                    Domain = string.IsNullOrWhiteSpace(cookieDomain) ? null : cookieDomain
                }
            );

            return Ok();
        }
    }
}
