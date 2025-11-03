using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("users")]
    public class UsersController : ControllerBase
    {
        // GET /users/account-info
        [HttpGet("account-info")]
        public IActionResult GetAccountInfo()
        {
            // TODO: Replace with real user lookup once auth/session is implemented.
            // For now, return the minimal payload expected by legacy clients.
            var payload = new
            {
                Username = string.Empty,
                HasPasswordSet = false,
                Email = string.Empty
            };

            return Ok(payload);
        }
    }
}
