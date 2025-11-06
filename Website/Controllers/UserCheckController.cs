using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Users;

namespace RobloxWebserver.Controllers
{
    [Route("UserCheck")]
    public class UserCheckController : Controller
    {
        private readonly IConfiguration _configuration;

        public UserCheckController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("doesusernameexist")]
        public async Task<IActionResult> DoesUserNameExist([FromQuery] string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return Json(new UsernameCheckResult { success = false });
            }

            var connStr = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
            {
                return Json(new UsernameCheckResult { success = false });
            }

            var result = await UserQueries.DoesUsernameExistAsync(connStr, username);
            return Json(result);
        }
    }
}
