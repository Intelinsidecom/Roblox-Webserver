using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;

namespace RobloxWebserver.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly IConfiguration _configuration;

        public AuthenticationController(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        // GET /authentication/logout
        [HttpGet("authentication/logout")]
        public IActionResult Logout()
        {
            var isHttps = Request.IsHttps;
            var cookieDomain = _configuration["Auth:CookieDomain"]; // same semantics as Api SignOutController

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

            // After logging out, send the user to the landing page
            return Redirect("/");
        }
    }
}
