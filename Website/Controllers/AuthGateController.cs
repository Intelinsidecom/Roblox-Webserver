using Microsoft.AspNetCore.Mvc;

namespace RobloxWebserver.Controllers
{
    public class AuthGateController : Controller
    {
        [HttpGet("/")]
        public IActionResult Root()
        {
            if (User?.Identity?.IsAuthenticated == true)
                return Redirect("/home");
            return View("~/Views/Pages/Index.cshtml");
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            if (User?.Identity?.IsAuthenticated == true)
                return Redirect("/home");
            return View("~/Views/Pages/Login.cshtml");
        }

        [HttpPost("login")]
        public IActionResult LoginPost()
        {
            if (User?.Identity?.IsAuthenticated == true)
                return Redirect("/home");
            return View("~/Views/Pages/Login.cshtml");
        }

        [HttpGet("newlogin")]
        public IActionResult NewLogin()
        {
            if (User?.Identity?.IsAuthenticated == true)
                return Redirect("/home");
            return View("~/Views/Pages/Login.cshtml");
        }

        [HttpPost("newlogin")]
        public IActionResult NewLoginPost()
        {
            if (User?.Identity?.IsAuthenticated == true)
                return Redirect("/home");
            return View("~/Views/Pages/Login.cshtml");
        }
    }
}
