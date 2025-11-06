using Microsoft.AspNetCore.Mvc;

namespace RobloxWebserver.Controllers
{
    public class PageController : Controller
    {
        [HttpGet("upgrades/robux")]
        public IActionResult Robux()
        {
            return View("~/Views/Pages/Robux.cshtml");
        }

        [HttpPost("upgrades/robux")]
        public IActionResult RobuxPost()
        {
            // Accept legacy form POSTs and render the same page.
            if (User?.Identity?.IsAuthenticated == true)
                return Redirect("/home");
            return View("~/Views/Pages/Robux.cshtml");
        }
    }
}
