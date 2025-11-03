using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Api.Controllers
{
    [ApiController]
    [Route("signup")]
    public class SignupController : ControllerBase
    {
        [HttpGet("is-username-valid")]
        public IActionResult IsUsernameValid([FromQuery] string username)
        {
            // Normalize
            var name = username?.Trim();

            // Basic checks based on Roblox-like requirements
            if (string.IsNullOrEmpty(name))
                return Ok(new { IsValid = false, ErrorMessage = "Username is required." });

            if (name.Length < 3 || name.Length > 20)
                return Ok(new { IsValid = false, ErrorMessage = "Username must be between 3 and 20 characters." });

            // Only letters and digits; no symbols or spaces
            if (!name.All(char.IsLetterOrDigit))
                return Ok(new { IsValid = false, ErrorMessage = "Username can only contain letters and numbers." });

            // If needed later: check profanity, reserved words, availability, etc.

            return Ok(new { IsValid = true, ErrorMessage = string.Empty });
        }

        [HttpGet("is-password-valid")]
        public IActionResult IsPasswordValid([FromQuery] string username, [FromQuery] string password)
        {
            var pwd = password ?? string.Empty;

            if (string.IsNullOrWhiteSpace(pwd))
                return Ok(new { IsValid = false, ErrorMessage = "Password is required." });

            // Typical constraints: 8–20 length, only letters and digits, must include at least one of each
            if (pwd.Length < 8 || pwd.Length > 20)
                return Ok(new { IsValid = false, ErrorMessage = "Password must be between 8 and 20 characters." });

            if (!pwd.All(char.IsLetterOrDigit))
                return Ok(new { IsValid = false, ErrorMessage = "Password can only contain letters and numbers." });

            var hasLetter = pwd.Any(char.IsLetter);
            var hasDigit = pwd.Any(char.IsDigit);
            if (!hasLetter || !hasDigit)
                return Ok(new { IsValid = false, ErrorMessage = "Password must include at least one letter and one number." });

            return Ok(new { IsValid = true, ErrorMessage = string.Empty });
        }
    }
}
