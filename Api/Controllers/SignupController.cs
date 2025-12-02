using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Users;
using Npgsql;
using System.Text.Json;
using System.IO;

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

        private static async Task<SignupRequest> TryReadJsonAsync(HttpRequest request)
        {
            try
            {
                var ct = request.ContentType ?? string.Empty;
                if (!ct.Contains("application/json", StringComparison.OrdinalIgnoreCase))
                    return null;

                request.EnableBuffering();
                request.Body.Position = 0;
                using var reader = new StreamReader(request.Body);
                var body = await reader.ReadToEndAsync();
                request.Body.Position = 0;
                if (string.IsNullOrWhiteSpace(body)) return null;
                return JsonSerializer.Deserialize<SignupRequest>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch
            {
                return null;
            }
        }

        [HttpGet("is-password-valid")]
        public IActionResult IsPasswordValid([FromQuery] string username, [FromQuery] string password)
        {
            var pwd = password ?? string.Empty;

            if (string.IsNullOrWhiteSpace(pwd))
                return Ok(new { IsValid = false, ErrorMessage = "Password is required." });

            // Frontend v2 rules: min length 8, allow symbols, no letter/digit requirement
            if (pwd.Length < 8 || pwd.Length > 128)
                return Ok(new { IsValid = false, ErrorMessage = "Password must be between 8 and 128 characters." });

            // Disallow password equal to username
            if (!string.IsNullOrEmpty(username) && string.Equals(pwd, username, StringComparison.Ordinal))
                return Ok(new { IsValid = false, ErrorMessage = "Password shouldn't match username." });

            // Disallow weak/common passwords
            if (IsWeakPassword(pwd))
                return Ok(new { IsValid = false, ErrorMessage = "Please create a more complex password." });

            return Ok(new { IsValid = true, ErrorMessage = string.Empty });
        }

        [HttpPost("v1")]
        [Consumes("application/json", "application/x-www-form-urlencoded", "multipart/form-data", "text/plain")]
        public async Task<IActionResult> SignupV1([FromServices] IConfiguration config)
        {
            // Accept JSON or form submissions. Attempt to parse JSON if content-type indicates.
            var request = await TryReadJsonAsync(Request) ?? new SignupRequest();

            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.Birthday) || string.IsNullOrWhiteSpace(request.Gender))
            {
                // Read from form first if present, else query
                if (Request.HasFormContentType)
                {
                    var f = Request.Form;
                    request.Username = Coalesce(request.Username, f["username"], f["userName"], f["UserName"], f["SignupUsername"], f["signup-username"]);
                    request.Password = Coalesce(request.Password, f["password"], f["Password"], f["SignupPassword"], f["signup-password"]);
                    // Birthday can be split into components
                    var bm = FirstNonEmpty(f["birthdayMonth"], f["MonthDropdown"], f["month"], f["Month"]);
                    var bd = FirstNonEmpty(f["birthdayDay"], f["DayDropdown"], f["day"], f["Day"]);
                    var by = FirstNonEmpty(f["birthdayYear"], f["YearDropdown"], f["year"], f["Year"]);
                    if (!string.IsNullOrWhiteSpace(bm) && !string.IsNullOrWhiteSpace(bd) && !string.IsNullOrWhiteSpace(by))
                    {
                        // Normalize month which may be Jan/Feb or numeric
                        var month = NormalizeMonth(bm);
                        request.Birthday = $"{by}-{month:D2}-{int.Parse(bd):D2}";
                    }
                    request.Birthday = Coalesce(request.Birthday, f["birthday"], f["Birthday"]);
                    request.Gender = Coalesce(request.Gender, f["gender"], f["Gender"], f["genderId"], f["GenderId"]);
                    request.Email = Coalesce(request.Email, f["email"], f["Email"], f["signup-email"]);
                }

                if (string.IsNullOrWhiteSpace(request.Username)) request.Username = FirstNonEmpty(Request.Query["username"], Request.Query["userName"], Request.Query["UserName"]);
                if (string.IsNullOrWhiteSpace(request.Password)) request.Password = FirstNonEmpty(Request.Query["password"], Request.Query["Password"]);
                if (string.IsNullOrWhiteSpace(request.Birthday))
                {
                    var bm = FirstNonEmpty(Request.Query["birthdayMonth"], Request.Query["MonthDropdown"], Request.Query["month"], Request.Query["Month"]);
                    var bd = FirstNonEmpty(Request.Query["birthdayDay"], Request.Query["DayDropdown"], Request.Query["day"], Request.Query["Day"]);
                    var by = FirstNonEmpty(Request.Query["birthdayYear"], Request.Query["YearDropdown"], Request.Query["year"], Request.Query["Year"]);
                    if (!string.IsNullOrWhiteSpace(bm) && !string.IsNullOrWhiteSpace(bd) && !string.IsNullOrWhiteSpace(by))
                    {
                        var month = NormalizeMonth(bm);
                        request.Birthday = $"{by}-{month:D2}-{int.Parse(bd):D2}";
                    }
                    else
                    {
                        request.Birthday = FirstNonEmpty(Request.Query["birthday"], Request.Query["Birthday"]);
                    }
                }
                if (string.IsNullOrWhiteSpace(request.Gender)) request.Gender = FirstNonEmpty(Request.Query["gender"], Request.Query["Gender"], Request.Query["genderId"], Request.Query["GenderId"]);
                if (string.IsNullOrWhiteSpace(request.Email)) request.Email = FirstNonEmpty(Request.Query["email"], Request.Query["Email"]);
            }

            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { errors = new[] { new { code = 1, message = "Invalid request" } } });

            var username = (request.Username ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(username) || username.Length < 3 || username.Length > 20 || !username.All(char.IsLetterOrDigit))
                return BadRequest(new { errors = new[] { new { code = 2, message = "Invalid username" } } });

            var password = request.Password ?? string.Empty;
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8 || password.Length > 128)
                return BadRequest(new { errors = new[] { new { code = 3, message = "Invalid password" } } });

            // Align with frontend v2 password checks
            if (!string.IsNullOrEmpty(username) && string.Equals(password, username, StringComparison.Ordinal))
                return BadRequest(new { errors = new[] { new { code = 3, message = "Invalid password" } } });

            if (IsWeakPassword(password))
                return BadRequest(new { errors = new[] { new { code = 3, message = "Invalid password" } } });

            DateTime? birthday = null;
            if (!string.IsNullOrWhiteSpace(request.Birthday))
            {
                if (DateTime.TryParse(request.Birthday, out var dt))
                    birthday = dt.Date;
                else
                    return BadRequest(new { errors = new[] { new { code = 4, message = "Invalid birthday" } } });
            }

            var gender = NormalizeGender(request.Gender);

            var connString = config.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connString))
                return StatusCode(500, new { errors = new[] { new { code = 5, message = "Database is not configured" } } });

            long newUserId;
            try
            {
                newUserId = await GetNextUserIdAsync(connString);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { errors = new[] { new { code = 6, message = "Failed to allocate user id" } }, detail = ex.Message });
            }

            var createParams = new UserCreateParams
            {
                UserId = newUserId,
                UserName = username,
                Password = password,
                Birthday = birthday,
                Gender = gender,
                Email = request.Email
            };

            try
            {
                var repo = new UsersRepository();
                await repo.CreateUserAsync(connString, createParams, failIfExists: true);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { errors = new[] { new { code = 7, message = "Failed to create user" } }, detail = ex.Message });
            }

            // Set .ROBLOSECURITY cookie to remember the user
            var cookieValue = $"UserId={newUserId}";

            var isHttps = Request.IsHttps;
            var cookieDomain = config["Auth:CookieDomain"];
            var sameSiteConfig = (config["Auth:CookieSameSite"] ?? "Lax").Trim();
            var sameSite = SameSiteMode.Lax;
            if (sameSiteConfig.Equals("None", StringComparison.OrdinalIgnoreCase)) sameSite = SameSiteMode.None;
            else if (sameSiteConfig.Equals("Strict", StringComparison.OrdinalIgnoreCase)) sameSite = SameSiteMode.Strict;
            if (sameSite == SameSiteMode.None) isHttps = true;

            Response.Cookies.Append(
                ".ROBLOSECURITY",
                cookieValue,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = isHttps,
                    SameSite = sameSite,
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    Path = "/",
                    Domain = string.IsNullOrWhiteSpace(cookieDomain) ? null : cookieDomain
                }
            );

            return Ok(new { userId = newUserId, username });
        }

        private static string NormalizeGender(string input)
        {
            var g = (input ?? string.Empty).Trim().ToLowerInvariant();
            if (g == "2" || g == "male") return "male";
            if (g == "3" || g == "female") return "female";
            return "none";
        }

        private static string Coalesce(string first, params Microsoft.Extensions.Primitives.StringValues[] values)
        {
            if (!string.IsNullOrWhiteSpace(first)) return first;
            foreach (var v in values)
            {
                var s = v.ToString();
                if (!string.IsNullOrWhiteSpace(s)) return s;
            }
            return null;
        }

        private static string FirstNonEmpty(params Microsoft.Extensions.Primitives.StringValues[] values)
        {
            foreach (var v in values)
            {
                var s = v.ToString();
                if (!string.IsNullOrWhiteSpace(s)) return s;
            }
            return null;
        }

        private static int NormalizeMonth(string month)
        {
            if (int.TryParse(month, out var m)) return Math.Clamp(m, 1, 12);
            var s = (month ?? string.Empty).Trim().ToLowerInvariant();
            return s switch
            {
                "jan" => 1,
                "feb" => 2,
                "mar" => 3,
                "apr" => 4,
                "may" => 5,
                "jun" => 6,
                "jul" => 7,
                "aug" => 8,
                "sep" => 9,
                "oct" => 10,
                "nov" => 11,
                "dec" => 12,
                _ => 1
            };
        }

        private static bool IsWeakPassword(string pwd)
        {
            if (pwd == null) return true;
            var p = pwd.Trim();
            if (p.Length == 0) return true; // whitespace-only

            var lower = p.ToLowerInvariant();
            // Common/weak passwords from frontend list
            string[] weak = new[]
            {
                "roblox123","password","password1","password12","password123","trustno1","iloveyou","princess","abcd1234",
                "qwertyui","qwerty","football","baseball","michael","jennifer","michelle","babygirl","superman",
                "12345678","123456789","1234567890","123123123","69696969","11111111","22222222","33333333","44444444",
                "55555555","66666666","77777777","88888888","99999999","00000000"
            };
            if (weak.Contains(lower)) return true;

            return false;
        }

        private static async Task<long> GetNextUserIdAsync(string connectionString)
        {
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand("select coalesce(max(user_id), 0) + 1 from users", conn);
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt64(result);
        }

        public class SignupRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public string Birthday { get; set; } // yyyy-MM-dd or any parseable format
            public string Gender { get; set; } // "male"|"female"|"none" or 2/3
            public string? Email { get; set; } // optional
        }
    }
}

