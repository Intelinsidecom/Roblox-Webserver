using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Games;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;

namespace Website.Controllers.Client
{
    [ApiController]
    public class HandlerController : Controller
    {
        [HttpGet("Login/Negotiate.ashx")]
        public async Task<IActionResult> Negotiate([FromQuery] string suggest)
        {
            Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
            Response.Headers["Content-Type"] = "text/plain";

            if (string.IsNullOrEmpty(suggest))
            {
                Response.StatusCode = 401;
                return Content(string.Empty, "text/plain");
            }
 
            var tokenService = HttpContext.RequestServices.GetRequiredService<TokenService>();
            var config = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var ticket = await tokenService.ValidateGameTicketAsync(suggest);
            if (ticket == null)
            {
                var allowInsecureDelete = config.GetValue<bool>("Auth:AllowInsecureCookies");
                var cookieDomainDelete = config["Auth:CookieDomain"];
                Response.Cookies.Delete(".ROBLOSECURITY", new CookieOptions 
                { 
                    Path = "/",
                    Domain = string.IsNullOrWhiteSpace(cookieDomainDelete) ? null : cookieDomainDelete,
                    Secure = Request.IsHttps && !allowInsecureDelete,
                    SameSite = SameSiteMode.Unspecified
                });
                Response.StatusCode = 401;
                return Content(string.Empty, "text/plain");
            }
            
            await tokenService.MarkTicketUsedAsync(suggest);
            var sessionToken = await tokenService.CreateSessionAsync(ticket.UserId, HttpContext.Connection.RemoteIpAddress?.ToString());
            var isHttps = Request.IsHttps;
            var allowInsecure = config.GetValue<bool>("Auth:AllowInsecureCookies");
            var cookieDomain = config["Auth:CookieDomain"];
            Response.Cookies.Append(".ROBLOSECURITY", sessionToken, new Microsoft.AspNetCore.Http.CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                Path = "/",
                HttpOnly = true,
                Secure = isHttps && !allowInsecure,
                SameSite = SameSiteMode.Unspecified,
                Domain = string.IsNullOrWhiteSpace(cookieDomain) ? null : cookieDomain
            });
 
            return Content(sessionToken, "text/plain");
        }

    }
}
