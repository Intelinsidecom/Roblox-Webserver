using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Games;

namespace RobloxWebserver.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly TokenService _tokenService;

        public AuthenticationController(IConfiguration configuration, TokenService tokenService)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        }

        // GET/POST /authentication/logout
        [AcceptVerbs("GET", "POST")]
        [Route("authentication/logout")]
        public async Task<IActionResult> Logout(string? returnUrl = null)
        {
            var token = Request.Cookies[".ROBLOSECURITY"];

            if (!string.IsNullOrEmpty(token))
            {
                await _tokenService.RevokeSessionAsync(token);
            }

            var isHttps = Request.IsHttps;
            var allowInsecure = _configuration.GetValue<bool>("Auth:AllowInsecureCookies");
            var cookieDomain = _configuration["Auth:CookieDomain"];
            Response.Cookies.Append(
                ".ROBLOSECURITY",
                string.Empty,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = isHttps && !allowInsecure,
                    SameSite = SameSiteMode.Unspecified,
                    Expires = DateTimeOffset.UnixEpoch,
                    MaxAge = TimeSpan.Zero,
                    Path = "/",
                    Domain = string.IsNullOrWhiteSpace(cookieDomain) ? null : cookieDomain
                }
            );

            // After logging out, send the user to the landing page
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return Redirect("/");
        }
    }
}
