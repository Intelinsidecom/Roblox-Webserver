using Microsoft.AspNetCore.Mvc;

namespace RobloxWebserver.Controllers
{
    public class MainController : Controller
    {
        [HttpGet("login")]
        [HttpGet("newlogin")]
        public IActionResult Login()
        {
            return View("~/Views/Main/Login.cshtml");
        }

        [HttpPost("login")]
        [HttpPost("newlogin")]
        public IActionResult LoginPost()
        {
            // Accept legacy form POSTs and render the same page.
            return View("~/Views/Home/Login.cshtml");
        }
    }
}
