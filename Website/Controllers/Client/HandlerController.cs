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
            if (string.IsNullOrEmpty(suggest))
                return Content("Invalid request: missing suggest parameter", "text/plain");
 
            var ticketService = HttpContext.RequestServices.GetRequiredService<AuthenticationTicketService>();
            var ticket = await ticketService.ValidateTicketAsync(suggest);
            if (ticket == null)
                return Content("Invalid ticket", "text/plain");
 
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