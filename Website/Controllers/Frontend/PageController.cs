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
            return View("~/Views/Pages/Robux.cshtml");
        }

	    }
}
