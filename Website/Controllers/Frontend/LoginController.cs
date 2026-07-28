using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Collections.Generic;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Common;
using System.Net;
using NpgsqlTypes;
using Games;

namespace Website.Controllers
{
    [ApiController]
    [Route("login")]
    public class LoginController : ControllerBase
    {
        public sealed class LoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        [HttpPost("v1")]
        [Consumes("application/json", "application/x-www-form-urlencoded", "multipart/form-data", "text/plain")]
        public async Task<IActionResult> LoginV1([FromServices] IConfiguration config, [FromServices] TokenService tokenService)
        {
            var username = string.Empty;
            var password = string.Empty;

            if (Request.HasFormContentType)
            {
                var f = Request.Form;
                username = FirstNonEmpty(f["username"], f["UserName"], f["login-username"], f["LoginUsername"]);
                password = FirstNonEmpty(f["password"], f["Password"], f["login-password"], f["LoginPassword"]);
            }

            if (string.IsNullOrWhiteSpace(username))
                username = FirstNonEmpty(Request.Query["username"], Request.Query["UserName"]);
            if (string.IsNullOrWhiteSpace(password))
                password = FirstNonEmpty(Request.Query["password"], Request.Query["Password"]);

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                // Try JSON
                try
                {
                    if ((Request.ContentType ?? string.Empty).Contains("application/json", StringComparison.OrdinalIgnoreCase))
                    {
                        Request.EnableBuffering();
                        Request.Body.Position = 0;
                        using var reader = new StreamReader(Request.Body);
                        var body = await reader.ReadToEndAsync();
                        Request.Body.Position = 0;
                        if (!string.IsNullOrWhiteSpace(body))
                        {
                            var dto = JsonSerializer.Deserialize<LoginRequest>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            username = dto?.Username ?? username;
                            password = dto?.Password ?? password;
                        }
                    }
                }
                catch { /* ignore */ }
            }

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return BadRequest(new { errors = new[] { new { code = 1, message = "Invalid request" } } });

            var connString = config.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connString))
                return StatusCode(500, new { errors = new[] { new { code = 5, message = "Database is not configured" } } });

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
            catch (Exception ex)
            {
                return StatusCode(500, new { errors = new[] { new { code = 7, message = "Login failed" } }, detail = ex.Message });
            }

            if (userId <= 0 || string.IsNullOrEmpty(storedPassword) || !PasswordHasher.VerifyPassword(password, storedPassword))
            {
                return StatusCode(403, new { errors = new[] { new { code = 1, message = "Incorrect username or password. Please try again" } } });
            }

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            string token;
            try
            {
                token = await tokenService.CreateSessionAsync(userId, ip);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { errors = new[] { new { code = 8, message = "Session create failed" } }, detail = ex.Message });
            }
            var expires = DateTimeOffset.UtcNow.AddYears(1);
            var rawDomain = config["Auth:CookieDomain"];
            var cookieDomain = string.IsNullOrWhiteSpace(rawDomain) ? null
                : rawDomain.StartsWith(".") ? rawDomain : "." + rawDomain;

            void SetLoginCookie(string name, string value)
            {
                Response.Cookies.Append(name, value, new CookieOptions
                {
                    HttpOnly = false,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Expires = expires,
                    Path = "/",
                    Domain = cookieDomain
                });
            }

            SetLoginCookie(".ROBLOSECURITY", token);

            return Ok(new { userId });
        }

        [HttpPost("/v2/login")]
        [Consumes("application/json", "application/x-www-form-urlencoded")]
        public async Task<IActionResult> LoginV2([FromServices] IConfiguration config, [FromServices] TokenService tokenService)
        {
            string username = null;
            string password = null;

            if (Request.HasFormContentType)
            {
                var f = Request.Form;
                username = f["username"].FirstOrDefault() ?? f["cvalue"].FirstOrDefault();
                password = f["password"].FirstOrDefault();
            }
            else if ((Request.ContentType ?? "").Contains("application/json", StringComparison.OrdinalIgnoreCase))
            {
                Request.EnableBuffering();
                Request.Body.Position = 0;
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();
                Request.Body.Position = 0;
                if (!string.IsNullOrWhiteSpace(body))
                {
                    using var jsonDoc = JsonDocument.Parse(body);
                    var root = jsonDoc.RootElement;

                    if (root.TryGetProperty("ctype", out var ctype) && root.TryGetProperty("cvalue", out var cvalue))
                    {
                        username = cvalue.GetString();
                        if (string.IsNullOrWhiteSpace(username))
                            username = root.TryGetProperty("username", out var u) ? u.GetString() : null;
                    }
                    else
                    {
                        var dto = JsonSerializer.Deserialize<LoginRequest>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        username = dto?.Username;
                    }
                    password = root.TryGetProperty("password", out var p) ? p.GetString() : null;
                }
            }

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return BadRequest(new { errors = new[] { new { code = 1, message = "Invalid request" } } });

            var connString = config.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connString))
                return StatusCode(500, new { errors = new[] { new { code = 5, message = "Database is not configured" } } });

            long userId = 0;
            string storedPassword = null;
            string userName = null;
            short membershipStatus = 0;
            string countryCode = null;
            DateTime? birthday = null;
            string moderationStatus = null;
            try
            {
                await using var conn = new NpgsqlConnection(connString);
                await conn.OpenAsync();
                await using var cmd = new NpgsqlCommand(@"
                    select user_id, user_name, password, membership_status, country_iso, birthday, moderation_status
                    from users where lower(user_name) = lower(@u) limit 1", conn);
                cmd.Parameters.AddWithValue("u", username);
                await using var rdr = await cmd.ExecuteReaderAsync();
                if (await rdr.ReadAsync())
                {
                    userId = rdr.IsDBNull(0) ? 0 : rdr.GetInt64(0);
                    userName = rdr.IsDBNull(1) ? null : rdr.GetString(1);
                    storedPassword = rdr.IsDBNull(2) ? null : rdr.GetString(2);
                    membershipStatus = rdr.IsDBNull(3) ? (short)0 : rdr.GetInt16(3);
                    countryCode = rdr.IsDBNull(4) ? null : rdr.GetString(4);
                    birthday = rdr.IsDBNull(5) ? null : rdr.GetDateTime(5);
                    moderationStatus = rdr.IsDBNull(6) ? null : rdr.GetString(6);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { errors = new[] { new { code = 7, message = "Login failed" } }, detail = ex.Message });
            }

            if (userId <= 0 || string.IsNullOrEmpty(storedPassword) || !PasswordHasher.VerifyPassword(password, storedPassword))
                return StatusCode(403, new { errors = new[] { new { code = 1, message = "Incorrect username or password. Please try again" } } });

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            string token;
            try
            {
                token = await tokenService.CreateSessionAsync(userId, ip);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { errors = new[] { new { code = 8, message = "Session create failed" } }, detail = ex.Message });
            }

            var expires = DateTimeOffset.UtcNow.AddDays(14);
            var rawDomain = config["Auth:CookieDomain"];
            var cookieDomain = string.IsNullOrWhiteSpace(rawDomain) ? null
                : rawDomain.StartsWith(".") ? rawDomain : "." + rawDomain;

            void SetLoginCookie(string name, string value)
            {
                Response.Cookies.Append(name, value, new CookieOptions
                {
                    HttpOnly = false,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Expires = expires,
                    Path = "/",
                    Domain = cookieDomain
                });
            }

            SetLoginCookie(".ROBLOSECURITY", token);

            var displayName = userName ?? "Unknown";
            var isUnder13 = birthday.HasValue && birthday.Value.AddYears(13) > DateTime.UtcNow;
            var isBanned = moderationStatus == "banned" || moderationStatus == "suspended";

            return Ok(new Dictionary<string, object>
            {
                ["membershipType"] = (int)membershipStatus,
                ["username"] = displayName,
                ["name"] = displayName,
                ["isUnder13"] = isUnder13,
                ["countryCode"] = countryCode ?? "",
                ["userId"] = userId,
                ["id"] = userId,
                ["displayName"] = displayName,
                ["user"] = new Dictionary<string, object>
                {
                    ["id"] = userId,
                    ["name"] = displayName,
                    ["displayName"] = displayName
                },
                ["isBanned"] = isBanned
            });
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
    }
}

