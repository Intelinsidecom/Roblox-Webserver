using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("login")]
    public class LoginController : ControllerBase
    {
        private sealed class LoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        [HttpPost("v1")]
        [Consumes("application/json", "application/x-www-form-urlencoded", "multipart/form-data", "text/plain")]
        public async Task<IActionResult> LoginV1([FromServices] IConfiguration config)
        {
            // Parse input flexibly (form, query, or json)
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
            catch (Exception ex)
            {
                return StatusCode(500, new { errors = new[] { new { code = 7, message = "Login failed" } }, detail = ex.Message });
            }

            if (userId <= 0 || string.IsNullOrEmpty(storedPassword) || !string.Equals(storedPassword, password, StringComparison.Ordinal))
            {
                // Mirror legacy behavior: 403 on credential failure
                return StatusCode(403, new { message = "Credentials" });
            }

            // Set session cookie
            var cookieValue = $"UserId={userId}";
            Response.Cookies.Append(
                ".ROBLOSECURITY",
                cookieValue,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTimeOffset.UtcNow.AddYears(1)
                }
            );

            return Ok(new { userId });
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
