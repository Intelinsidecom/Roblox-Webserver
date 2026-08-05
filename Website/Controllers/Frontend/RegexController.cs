using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace RobloxWebserver.Controllers.Frontend
{
    [ApiController]
    public class RegexController : Controller
    {
        private readonly IConfiguration _configuration;

        public RegexController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Returns the email validation regex the client modals apply via
        /// ng-pattern. Response shape: { Regex: "..." }.
        /// Configurable via appsettings.json key "Validation:EmailRegex".
        /// </summary>
        [HttpGet("regex/email")]
        public IActionResult GetEmailRegex()
        {
            var regex = _configuration["Validation:EmailRegex"]
                ?? @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*@((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$";
            return Json(new { Regex = regex });
        }
    }
}
