using Microsoft.AspNetCore.Mvc;

namespace RobloxWebserver.Controllers
{
    public class PageController : Controller
    {


        [HttpGet("ide/clienttoolbox")]
        public IActionResult Toolbox()
        {
            return View("~/Views/Pages/ide/ClientToolbox.aspx.cshtml");
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
