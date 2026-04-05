using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Games;
using System.Threading.Tasks;

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
 
            var ticketService = HttpContext.RequestServices.GetRequiredService<AuthenticationTicketService>();
            var ticket = await ticketService.ValidateTicketAsync(suggest);
            if (ticket == null)
            {
                Response.StatusCode = 401;
                return Content(string.Empty, "text/plain");
            }
 
            Response.Cookies.Append(".ROBLOSECURITY", ticket.TicketToken, new Microsoft.AspNetCore.Http.CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(30),
                Path = "/",
                HttpOnly = true
            });
 
            return Content(ticket.TicketToken, "text/plain");
        }

    }
}