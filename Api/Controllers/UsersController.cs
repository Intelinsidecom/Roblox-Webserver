using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Api.Controllers
{
    [ApiController]
    [Route("users")]
    public class UsersController : ControllerBase
    {
        // GET /users/account-info
        [HttpGet("account-info")]
        public IActionResult GetAccountInfo([FromServices] IConfiguration config)
        {
            var cookie = Request.Cookies[".ROBLOSECURITY"];
            if (string.IsNullOrWhiteSpace(cookie))
                return BadRequest();

            long userId = 0;
            var idx = cookie.IndexOf("UserId=", StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                var val = cookie.Substring(idx + 7);
                var sep = val.IndexOf(';');
                if (sep >= 0) val = val.Substring(0, sep);
                long.TryParse(val, out userId);
            }
            if (userId <= 0)
                return BadRequest();

            string? username = null;
            string? email = null;
            string? password = null;

            try
            {
                var connString = config.GetConnectionString("Default");
                if (string.IsNullOrWhiteSpace(connString))
                    return StatusCode(500);

                using var conn = new NpgsqlConnection(connString);
                conn.Open();
                using var cmd = new NpgsqlCommand("select user_name, password, email from users where user_id = @id limit 1", conn);
                cmd.Parameters.AddWithValue("id", userId);
                using var reader = cmd.ExecuteReader();
                if (!reader.Read())
                {
                    return BadRequest();
                }
                username = reader.IsDBNull(0) ? null : reader.GetString(0);
                password = reader.IsDBNull(1) ? null : reader.GetString(1);
                email = reader.IsDBNull(2) ? null : reader.GetString(2);
            }
            catch
            {
            }

            var payload = new
            {
                username = username ?? string.Empty,
                hasPasswordSet = !string.IsNullOrEmpty(password),
                email = email ?? string.Empty
            };

            return Ok(payload);
        }
    }
}
