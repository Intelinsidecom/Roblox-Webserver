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
        public IActionResult NewLogin(int? failureReason)
        {
            if (User?.Identity?.IsAuthenticated == true)
                return Redirect("/home");
            
            string errorMessage = null;
            
            if (failureReason.HasValue)
            {
                switch (failureReason.Value)
                {
                    case 3: // credentials error
                        errorMessage = "Username or password is incorrect!";
                        break;
                    default:
                        errorMessage = "Login failed. Please try again.";
                        break;
                }
            }
            
            ViewBag.ErrorMessage = errorMessage;
            return View("~/Views/Pages/Login.cshtml");
        }

        [HttpPost("newlogin")]
        public IActionResult NewLoginPost()
        {
            if (User?.Identity?.IsAuthenticated == true)
                return Redirect("/home");
            return View("~/Views/Pages/Login.cshtml");
        }

        [HttpGet("home")]
        public IActionResult Home()
        {
            if (User?.Identity?.IsAuthenticated == false)
                return Redirect("/");
            return View("~/Views/Pages/Home.cshtml");
        }

                [HttpGet("develop")]
        public IActionResult Develop()
        {
            if (User?.Identity?.IsAuthenticated == false)
                return Redirect("/");
            return View("~/Views/Pages/Develop.cshtml");
        }

        [HttpPost("develop")]
        public IActionResult DevelopPost()
        {
            if (User?.Identity?.IsAuthenticated == false)
                return Redirect("/");
            return View("~/Views/Pages/Develop.cshtml");
        }
    }
}
