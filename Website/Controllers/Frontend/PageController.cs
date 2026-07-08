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

        [HttpGet("ide/clienttoolbox")]
        public IActionResult Toolbox()
        {
            return View("~/Views/Pages/ide/ClientToolbox.aspx.cshtml");
        }

        [HttpPost("upgrades/robux")]
        public IActionResult RobuxPost()
        {
            return View("~/Views/Pages/Robux.cshtml");
        }

        [HttpGet("Games.aspx")]
        public IActionResult GamesPage()
        {
            return View("~/Views/Pages/Games.cshtml");
        }

        [HttpPost("Games.aspx")]
        public IActionResult GamesPagePost()
        {
            return View("~/Views/Pages/Games.cshtml");
        }

	    }
}
