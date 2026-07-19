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

        [HttpGet("My/Stuff.aspx")]
        public IActionResult Inventory()
        {
            return Redirect("/users/inventory");
        }

        [HttpPost("My/Stuff.aspx")]
        public IActionResult InventoryPost()
        {
            return Redirect("/users/inventory");
        }

        [HttpGet("premium/windows/bc")]
        public IActionResult BC()
        {
            return Redirect("/premium/membership");
        }

        [HttpGet("premium/windows/robux")]
        public IActionResult Robux()
        {
            return Redirect("/upgrades/robux");
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
