using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Security.Claims;

namespace Api.Controllers
{
    [ApiController]
    public class UsersController : ControllerBase
    {
        // GET /users/account-info
        [HttpGet("users/account-info")]
        public IActionResult GetAccountInfo([FromServices] IConfiguration config)
        {
            // Prefer user id from claims (set by Website middleware from sessions)
            long userId = 0;
            var claimVal = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(claimVal))
                long.TryParse(claimVal, out userId);

            // Fallback: if no claims (e.g., direct call bypassing middleware), try to map cookie -> user id via sessions
            if (userId <= 0)
            {
                var cookie = Request.Cookies[".ROBLOSECURITY"];
                if (!string.IsNullOrWhiteSpace(cookie))
                {
                    try
                    {
                        var connString = config.GetConnectionString("Default");
                        using var conn = new NpgsqlConnection(connString);
                        conn.Open();
                        using var cmd = new NpgsqlCommand("select user_id from sessions where token = @t and (expires_at is null or expires_at > now() at time zone 'utc')", conn);
                        cmd.Parameters.AddWithValue("t", cookie);
                        var obj = cmd.ExecuteScalar();
                        if (obj is long uid)
                            userId = uid;
                    }
                    catch { }
                }
            }

            if (userId <= 0)
                return StatusCode(403);

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
                    return StatusCode(403);
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

        [HttpGet("users/get-studio-experiment-enrollments")]
        public IActionResult GetStudioExperimentEnrollments([FromQuery] bool firstStudioVisit, [FromQuery] string browserTrackerId)
        {
            var response = new
            {
                experiments = new object[0], // Empty array - no active experiments
                browserTrackerId = string.IsNullOrEmpty(browserTrackerId) 
                    ? $"rbx_tracker_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}_{Guid.NewGuid():N}" 
                    : browserTrackerId
            };

            return Ok(response);
        }

        [HttpGet("users/get-experiment-enrollments")]
        public IActionResult GetExperimentEnrollments([FromQuery] string browserTrackerId)
        {
            var response = new
            {
                experiments = new object[0],
                browserTrackerId = string.IsNullOrEmpty(browserTrackerId) 
                    ? $"rbx_tracker_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}_{Guid.NewGuid():N}" 
                    : browserTrackerId
            };

            return Ok(response);
        }

        [HttpGet("userblock/getblockedusers")]
        public IActionResult GetBlockedUsers([FromQuery] long userId, [FromQuery] int page = 1)
        {
            // Theres no blocking functionality yet
            var response = new
            {
                success = true,
                userList = new object[0]
            };

            return Ok(response);
        }
    }
}
