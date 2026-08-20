using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Users;

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

        [HttpGet("games/list")]
        public IActionResult GamesList()
        {
            return View("~/Views/Pages/Games.cshtml");
        }

        [HttpPost("games/list")]
        public IActionResult GamesListPost()
        {
            return View("~/Views/Pages/Games.cshtml");
        }

        [HttpGet("friends.aspx")]
        public IActionResult FriendsPage()
        {
            return Redirect("/friends");
        }

        [HttpPost("friends.aspx")]
        public IActionResult FriendsPagePost()
        {
            return Redirect("/friends");
        }

        [HttpGet("User.aspx")]
        public IActionResult UserPage()
        {
            if (User?.Identity?.IsAuthenticated != true)
                return Redirect("/");
            var (isValid, userId) = AuthenticationHelper.GetCurrentUserId(User);
            return Redirect("/users/" + userId + "/profile");
        }

        [HttpPost("User.aspx")]
        public IActionResult UserPagePost()
        {
            if (User?.Identity?.IsAuthenticated != true)
                return Redirect("/");
            var (isValid, userId) = AuthenticationHelper.GetCurrentUserId(User);
            return Redirect("/users/" + userId + "/profile");
        }

        [HttpGet("inbox")]
        public IActionResult inbox()
        {
            return View("~/Views/Pages/My/Messages.cshtml");
        }

        [HttpPost("inbox")]
        public IActionResult inboxPost()
        {
            return View("~/Views/Pages/My/Messages.cshtml");
        }

        [HttpGet("mobile-app-upgrades/native-ios/robux")]
        public IActionResult AppRobux()
        {
            return View("~/Views/Pages/upgrades/robux.cshtml");
        }

        [HttpPost("mobile-app-upgrades/native-ios/robux")]
        public IActionResult AppRobuxPost()
        {
            return View("~/Views/Pages/upgrades/robux.cshtml");
        }

        [HttpGet("mobile-app-upgrades/native-ios/bc")]
        public IActionResult AppBC()
        {
            return View("~/Views/Pages/premium/membership.cshtml");
        }

        [HttpPost("mobile-app-upgrades/native-ios/bc")]
        public IActionResult AppBCPost()
        {
            return View("~/Views/Pages/premium/membership.cshtml");
        }

	    }
}
