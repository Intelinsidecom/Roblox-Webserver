using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Npgsql;
using Users;
using Games;

namespace Website.Controllers.Client
{
    [ApiController]
    [Route("mobileapi")]
    public class MobileApiController : ControllerBase
    {
        [HttpGet("check-app-version")]
        public IActionResult CheckAppVersion()
        {
            return Ok(new
            {
                data = new
                {
                    UpgradeAction = "None"
                }
            });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromServices] IConfiguration config, [FromServices] Games.TokenService tokenService)
        {
            var username = string.Empty;
            var password = string.Empty;

            if (Request.HasFormContentType)
            {
                var f = Request.Form;
                username = FirstNonEmpty(f["username"], f["UserName"]);
                password = FirstNonEmpty(f["password"], f["Password"]);
            }
            else if ((Request.ContentType ?? "").Contains("application/json", System.StringComparison.OrdinalIgnoreCase))
            {
                Request.EnableBuffering();
                Request.Body.Position = 0;
                using var reader = new System.IO.StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();
                Request.Body.Position = 0;
                if (!string.IsNullOrWhiteSpace(body))
                {
                    try
                    {
                        var dto = System.Text.Json.JsonSerializer.Deserialize<LoginRequest>(body, 
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        username = dto?.Username ?? username;
                        password = dto?.Password ?? password;
                    }
                    catch { /* ignore */ }
                }
            }

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return BadRequest(new { Status = "MissingRequiredField" });
            }

            var connString = config.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connString))
            {
                return StatusCode(500, new { Status = "DatabaseConfigurationError" });
            }

            long userId = 0;
            string storedPassword = null;
            string userName = null;
            try
            {
                await using var conn = new NpgsqlConnection(connString);
                await conn.OpenAsync();
                await using var cmd = new NpgsqlCommand("select user_id, user_name, password from users where lower(user_name) = lower(@u) limit 1", conn);
                cmd.Parameters.AddWithValue("u", username);
                await using var rdr = await cmd.ExecuteReaderAsync();
                if (await rdr.ReadAsync())
                {
                    userId = rdr.IsDBNull(0) ? 0 : rdr.GetInt64(0);
                    userName = rdr.IsDBNull(1) ? null : rdr.GetString(1);
                    storedPassword = rdr.IsDBNull(2) ? null : rdr.GetString(2);
                }
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { Status = "LoginFailed", Message = ex.Message });
            }

            if (userId <= 0 || string.IsNullOrEmpty(storedPassword) || !PasswordHasher.VerifyPassword(password, storedPassword))
            {
                return Ok(new { Status = "InvalidUsername" }); // Match Roblox's response format
            }

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            string token;
            try
            {
                token = await tokenService.CreateSessionAsync(userId, ip);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Status = "SessionCreateFailed", Message = ex.Message });
            }

            var expires = System.DateTimeOffset.UtcNow.AddYears(1);
            var rawDomain = config["Auth:CookieDomain"];
            var cookieDomain = string.IsNullOrWhiteSpace(rawDomain) ? null
                : rawDomain.StartsWith(".") ? rawDomain : "." + rawDomain;

            void SetLoginCookie(string name, string value)
            {
                Response.Cookies.Append(name, value, new Microsoft.AspNetCore.Http.CookieOptions
                {
                    HttpOnly = false,
                    Secure = false,
                    SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax,
                    Expires = expires,
                    Path = "/",
                    Domain = cookieDomain
                });
            }

            SetLoginCookie(".ROBLOSECURITY", token);

            var userInfo = await GetUserInfoAsync(config, connString, userId, userName);

            return Ok(new
            {
                Status = "OK",
                UserInfo = userInfo
            });
        }

        [HttpPost("securesignup")]
        public async Task<IActionResult> SecureSignUp([FromServices] IConfiguration config, [FromServices] Games.TokenService tokenService)
        {
            string userName;
            string password;
            string gender;
            string dateOfBirth;
            string email;
            try
            {
                if (!Request.HasFormContentType)
                    return Ok(new { Status = "InvalidForm" });

                var f = Request.Form;
                userName    = (f["userName"]    .ToString() ?? string.Empty).Trim();
                password    =  f["password"]    .ToString() ?? string.Empty;
                gender      = (f["gender"]      .ToString() ?? string.Empty).Trim();
                dateOfBirth = (f["dateOfBirth"] .ToString() ?? string.Empty).Trim();
                email       = (f["email"]       .ToString() ?? string.Empty).Trim();
            }
            catch
            {
                return Ok(new { Status = "InvalidForm" });
            }

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                return Ok(new { Status = "InvalidForm" });

            if (userName.Contains(' '))
                return Ok(new { Status = "Username Cannot Contain Spaces" });

            if (userName.Length < 3 || userName.Length > 20 || !userName.All(c => char.IsLetterOrDigit(c) || c == '_'))
                return Ok(new { Status = "Invalid Characters Used" });

            if (password.Length < 4 || password.Length > 20)
                return Ok(new { Status = "InvalidForm" });

            var connString = config.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connString))
                return Ok(new { Status = "DatabaseConfigurationError" });

            bool exists;
            try
            {
                exists = await UserQueries.UsernameExistsAsync(connString, userName);
            }
            catch (System.Exception ex)
            {
                return Ok(new { Status = "DatabaseError", Message = ex.Message });
            }
            if (exists)
                return Ok(new { Status = "Already Taken" });

            DateTime? birthday = null;
            if (!string.IsNullOrEmpty(dateOfBirth))
            {
                if (DateTime.TryParseExact(dateOfBirth, new[] { "M/d/yyyy", "MM/dd/yyyy" },
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out var dt))
                {
                    birthday = dt.Date;
                }
                else if (DateTime.TryParse(dateOfBirth, System.Globalization.CultureInfo.InvariantCulture,
                             System.Globalization.DateTimeStyles.None, out var dtFallback))
                {
                    birthday = dtFallback.Date;
                }
            }

            var normalizedGender = NormalizeMobileGender(gender);

            long newUserId;
            try
            {
                newUserId = await GetNextUserIdAsync(connString);
            }
            catch (System.Exception ex)
            {
                return Ok(new { Status = "ServerError", Message = ex.Message });
            }

            var hashedPassword = Common.PasswordHasher.HashPassword(password);

            var createParams = new Users.UserCreateParams
            {
                UserId   = newUserId,
                UserName = userName,
                Password = hashedPassword,
                Birthday = birthday,
                Gender   = normalizedGender,
                Email    = string.IsNullOrEmpty(email) ? null : email
            };

            try
            {
                var repo = new Users.UsersRepository();
                await repo.CreateUserAsync(connString, createParams, failIfExists: true);

                await GameCreationService.CreateUsersFirstPlaceAsync(
                    newUserId, userName, connString, config, System.Threading.CancellationToken.None);
            }
            catch (System.Exception ex)
            {
                return Ok(new { Status = "FailedToCreateUser", Message = ex.Message });
            }

            string token;
            try
            {
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                token = await tokenService.CreateSessionAsync(newUserId, ip);
            }
            catch (System.Exception ex)
            {
                return Ok(new { Status = "SessionCreateFailed", Message = ex.Message });
            }

            var expires = System.DateTimeOffset.UtcNow.AddYears(1);
            var rawDomain = config["Auth:CookieDomain"];
            var cookieDomain = string.IsNullOrWhiteSpace(rawDomain) ? null
                : rawDomain.StartsWith(".") ? rawDomain : "." + rawDomain;

            Response.Cookies.Append(".ROBLOSECURITY", token, new Microsoft.AspNetCore.Http.CookieOptions
            {
                HttpOnly = false,
                Secure = false,
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax,
                Expires = expires,
                Path = "/",
                Domain = cookieDomain
            });

            var userInfo = await GetUserInfoAsync(config, connString, newUserId, userName);

            return Ok(new
            {
                Status = "OK",
                UserInfo = userInfo
            });
        }

        [HttpGet("userinfo")]
        public async Task<IActionResult> UserInfo([FromServices] IConfiguration config, [FromServices] Games.TokenService tokenService)
        {
            try
            {
                var token = Request.Cookies[".ROBLOSECURITY"];
                if (string.IsNullOrWhiteSpace(token))
                {
                    return Unauthorized(new { Status = "NotLoggedIn" });
                }

                long? userId;
                try
                {
                    userId = await tokenService.ValidateSessionAsync(token);
                }
                catch (System.Exception ex)
                {
                    return StatusCode(500, new { Status = "SessionValidationFailed", Message = ex.Message });
                }

                if (userId == null || userId <= 0)
                {
                    return Unauthorized(new { Status = "InvalidSession" });
                }

                var connString = config.GetConnectionString("Default");
                if (string.IsNullOrWhiteSpace(connString))
                {
                    return StatusCode(500, new { Status = "DatabaseConfigurationError" });
                }

                string userName = null;
                try
                {
                    userName = await UserQueries.GetUserNameByIdAsync(connString, userId.Value);
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to get username for user {userId.Value}: {ex.Message}");
                }

                var userInfo = await GetUserInfoAsync(config, connString, userId.Value, userName);

                return Ok(userInfo);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Unhandled /mobileapi/userinfo error: {ex}");
                return StatusCode(500, new { Status = "ServerError", Message = ex.Message });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromServices] Games.TokenService tokenService, [FromServices] IConfiguration config)
        {
            var token = Request.Cookies[".ROBLOSECURITY"];
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    await tokenService.RevokeSessionAsync(token);
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to revoke session: {ex.Message}");
                }
            }

            var rawDomain = config["Auth:CookieDomain"];
            var cookieDomain = string.IsNullOrWhiteSpace(rawDomain) ? null
                : rawDomain.StartsWith(".") ? rawDomain : "." + rawDomain;

            Response.Cookies.Append(".ROBLOSECURITY", string.Empty, new Microsoft.AspNetCore.Http.CookieOptions
            {
                HttpOnly = false,
                Secure = false,
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax,
                Expires = System.DateTimeOffset.UnixEpoch,
                MaxAge = System.TimeSpan.Zero,
                Path = "/",
                Domain = cookieDomain
            });

            return Ok();
        }

        private async Task<MobileUserInfo> GetUserInfoAsync(IConfiguration config, string connString, long userId, string userName)
        {
            string avatarThumbnailUrl = (config["PublicBaseUrl"] ?? string.Empty) + (config["Thumbnails:DefaultThumbnailUrl"] ?? string.Empty);
            bool isAnyBC = false;
            long robuxBalance = 0;
            long tixBalance = 0;

            try
            {
                var avatarUrl = await Thumbnails.ThumbnailQueries.GetUserThumbnailUrlAsync(connString, userId);
                if (!string.IsNullOrWhiteSpace(avatarUrl))
                {
                    avatarThumbnailUrl = avatarUrl;
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to get thumbnail url for user {userId}: {ex.Message}");
            }

            try
            {
                var profileData = await UserQueries.GetUserProfileDataAsync(connString, userId);
                if (profileData != null && profileData.TryGetValue("membershipStatus", out var memObj) && memObj != null)
                {
                    short memStatus = Convert.ToInt16(memObj);
                    isAnyBC = memStatus >= 1;
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to get profile data for user {userId}: {ex.Message}");
            }

            try
            {
                robuxBalance = await UserQueries.GetCurrencyByIdAsync(connString, userId, "robux");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to get robux balance for user {userId}: {ex.Message}");
            }

            try
            {
                tixBalance = await UserQueries.GetCurrencyByIdAsync(connString, userId, "tix");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to get tix balance for user {userId}: {ex.Message}");
            }

            return new MobileUserInfo
            {
                UserID = userId,
                UserName = userName,
                DisplayName = userName,
                RobuxBalance = robuxBalance,
                TicketsBalance = tixBalance,
                ThumbnailUrl = avatarThumbnailUrl,
                IsAnyBuildersClubMember = isAnyBC
            };
        }

        private static string FirstNonEmpty(params Microsoft.Extensions.Primitives.StringValues[] values)
        {
            foreach (var v in values)
            {
                var s = v.ToString();
                if (!string.IsNullOrWhiteSpace(s)) return s;
            }
            return string.Empty;
        }

        private static string NormalizeMobileGender(string input)
        {
            var g = (input ?? string.Empty).Trim().ToLowerInvariant();
            if (g == "male") return "male";
            if (g == "female") return "female";
            return "none";
        }

        private static async Task<long> GetNextUserIdAsync(string connectionString)
        {
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand("select coalesce(max(user_id), 0) + 1 from users", conn);
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt64(result);
        }

        public class LoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public class MobileUserInfo
        {
            public long UserID { get; set; }
            public string UserName { get; set; }
            public string DisplayName { get; set; }
            public long RobuxBalance { get; set; }
            public long TicketsBalance { get; set; }
            public string ThumbnailUrl { get; set; }
            public bool IsAnyBuildersClubMember { get; set; }
        }
    }
}