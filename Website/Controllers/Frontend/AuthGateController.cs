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
        public IActionResult Login(string? returnUrl = null)
        {
            if (User?.Identity?.IsAuthenticated == true)
                return Redirect("/home");
            ViewBag.ReturnUrl = returnUrl;
            return View("~/Views/Pages/Login.cshtml");
        }

        [HttpPost("login")]
        public IActionResult LoginPost(string? returnUrl = null)
        {
            if (User?.Identity?.IsAuthenticated == true)
                return Redirect("/home");
            ViewBag.ReturnUrl = returnUrl;
            return View("~/Views/Pages/Login.cshtml");
        }

        [HttpGet("newlogin")]
        public IActionResult NewLogin(int? failureReason, string? returnUrl = null)
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
            ViewBag.ReturnUrl = returnUrl;
            return View("~/Views/Pages/Login.cshtml");
        }

        [HttpPost("newlogin")]
        public IActionResult NewLoginPost(string? returnUrl = null)
        {
            if (User?.Identity?.IsAuthenticated == true)
                return Redirect("/home");
            ViewBag.ReturnUrl = returnUrl;
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
        public IActionResult DevelopPage()
        {
            if (User?.Identity?.IsAuthenticated == false)
            {
            return View("~/Views/Develop/Guest.cshtml");
            }
            else
            {
            return View("~/Views/Pages/Develop.cshtml");
            }
        }


        [HttpPost("develop")]
        public IActionResult DevelopPagePost()
        {
            if (User?.Identity?.IsAuthenticated == false)
            {
            return View("~/Views/Develop/Guest.cshtml");
            }
            else
            {
            return View("~/Views/Pages/Develop.cshtml");
            }
        }
    }
}
