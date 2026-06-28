using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using Games;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;
using Npgsql;
using Common;

namespace Website.Controllers.Client
{
    [ApiController]
    public class LoginController : Controller
    {

        private readonly IConfiguration _configuration;

        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        [HttpGet("login/RequestAuth.ashx")]
        public async Task<IActionResult> RequestAuth([FromServices] IConfiguration config, [FromServices] TokenService tokenService)
        {
            if (User?.Identity?.IsAuthenticated != true)
                return Unauthorized("User is not authorized.");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdClaim, out var userId) || userId <= 0)
                return Unauthorized("User is not authorized.");

            var baseUrl = config["PublicBaseUrl"] ?? $"{Request.Scheme}://{Request.Host}";
            if (baseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                baseUrl = "http://" + baseUrl.Substring(8);
            }

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var token = await tokenService.CreateSessionAsync(userId, ip);
            return Content($"{baseUrl}/Login/Negotiate.ashx?suggest={token}", "text/plain; charset=utf-8");
        }

        [HttpGet("Login/Negotiate.ashx")]
        [HttpPost("Login/Negotiate.ashx")]
        public async Task<IActionResult> Negotiate([FromQuery] string suggest)
        {
            if (string.IsNullOrEmpty(suggest))
            {
                Response.StatusCode = 401;
                return Content(string.Empty, "text/plain");
            }

            var tokenService = HttpContext.RequestServices.GetRequiredService<TokenService>();

            long? userId = null;
            string sessionToken = suggest;

            if (suggest.StartsWith("game_"))
            {
                var ticket = await tokenService.ValidateGameTicketAsync(suggest);
                if (ticket == null)
                {
                    Response.StatusCode = 401;
                    return Content(string.Empty, "text/plain");
                }
                userId = ticket.UserId;
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                sessionToken = await tokenService.CreateSessionAsync(userId.Value, ip);
            }
            else
            {
                    userId = await tokenService.ValidateSessionAsync(suggest);
                if (userId == null)
                {
                    Response.StatusCode = 401;
                    return Content(string.Empty, "text/plain");
                }
                sessionToken = suggest;
            }

            var rawDomain = _configuration["Auth:CookieDomain"];
            var cookieDomain = string.IsNullOrWhiteSpace(rawDomain) ? null
                : rawDomain.StartsWith(".") ? rawDomain : "." + rawDomain;

            var cookieOptions = new CookieOptions
            {
                Domain = cookieDomain,
                Secure = false,
                Expires = DateTimeOffset.Now.AddDays(364),
                IsEssential = true,
                Path = "/",
                SameSite = SameSiteMode.Lax,
            };

            Response.Cookies.Append(".ROBLOSECURITY", sessionToken, cookieOptions);

            return Content(string.Empty, "text/plain");
        }
    }
}