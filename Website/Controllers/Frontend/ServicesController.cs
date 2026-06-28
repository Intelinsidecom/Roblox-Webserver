using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Games;
using Common;

namespace RobloxWebserver.Controllers
{
    public class ServicesController : Controller
    {
        private sealed class LoginArgs
        {
            public string username { get; set; }
            public string password { get; set; }
            public string captchaEnabled { get; set; }
            public string recaptchaChallenge { get; set; }
            public string recaptchaResponse { get; set; }
        }

        [Route("Services/Secure/LoginService.asmx/ValidateLogin")]
        [HttpPost]
        public async Task<IActionResult> ValidateLogin([FromServices] IConfiguration config, [FromServices] TokenService tokenService)
        {
            string username = null, password = null;

            if (Request.HasFormContentType)
            {
                var f = Request.Form;
                username = f["username"].FirstOrDefault() ?? f["UserName"].FirstOrDefault();
                password = f["password"].FirstOrDefault() ?? f["Password"].FirstOrDefault();
            }

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                try
                {
                    if ((Request.ContentType ?? "").Contains("application/json", StringComparison.OrdinalIgnoreCase))
                    {
                        Request.EnableBuffering();
                        Request.Body.Position = 0;
                        using var reader = new System.IO.StreamReader(Request.Body);
                        var body = await reader.ReadToEndAsync();
                        Request.Body.Position = 0;
                        if (!string.IsNullOrWhiteSpace(body))
                        {
                            var dto = JsonSerializer.Deserialize<LoginArgs>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            username = dto?.username ?? username;
                            password = dto?.password ?? password;
                        }
                    }
                }
                catch { }
            }

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return Json(new { d = new { IsValid = false, ErrorCode = "3", Message = "Invalid request" } });

            var connString = config.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connString))
                return Json(new { d = new { IsValid = false, ErrorCode = "3", Message = "Service unavailable" } });

            long userId = 0;
            string storedPassword = null;
            try
            {
                await using var conn = new NpgsqlConnection(connString);
                await conn.OpenAsync();
                await using var cmd = new NpgsqlCommand("select user_id, password from users where lower(user_name) = lower(@u) limit 1", conn);
                cmd.Parameters.AddWithValue("u", username);
                await using var rdr = await cmd.ExecuteReaderAsync();
                if (await rdr.ReadAsync())
                {
                    userId = rdr.IsDBNull(0) ? 0 : rdr.GetInt64(0);
                    storedPassword = rdr.IsDBNull(1) ? null : rdr.GetString(1);
                }
            }
            catch
            {
                return Json(new { d = new { IsValid = false, ErrorCode = "3", Message = "Login failed" } });
            }

            if (userId <= 0 || string.IsNullOrEmpty(storedPassword) || !PasswordHasher.VerifyPassword(password, storedPassword))
                return Json(new { d = new { IsValid = false, ErrorCode = "7", Message = "Invalid username or password" } });

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            string token;
            try
            {
                token = await tokenService.CreateSessionAsync(userId, ip);
            }
            catch
            {
                return Json(new { d = new { IsValid = false, ErrorCode = "3", Message = "Session create failed" } });
            }

            var expires = DateTimeOffset.UtcNow.AddYears(1);
            var isHttps = Request.IsHttps;
            var allowInsecure = config.GetValue<bool>("Auth:AllowInsecureCookies");
            var cookieDomain = config["Auth:CookieDomain"];
            Response.Cookies.Append(
                ".ROBLOSECURITY",
                token,
                new CookieOptions
                {
                    HttpOnly = false,
                    Secure = isHttps && !allowInsecure,
                    SameSite = SameSiteMode.Unspecified,
                    Expires = expires,
                    Path = "/",
                    Domain = string.IsNullOrWhiteSpace(cookieDomain) ? null : cookieDomain
                }
            );

            return Json(new { d = new { IsValid = true } });
        }
    }
}
