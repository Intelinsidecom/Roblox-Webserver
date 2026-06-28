using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Assets;
using Npgsql;
using System.Security.Claims;
using System.Threading.Tasks;
using Games;
using Api.Services;

namespace Api.Controllers
{
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly CurrentUserService _currentUserService;

        [HttpGet("users/account-info")]
        public IActionResult GetAccountInfo([FromServices] IConfiguration config)
        {
            long userId = _currentUserService.GetUserIdAsync().GetAwaiter().GetResult();

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

        [HttpGet("users/authenticated")]
        public async Task<IActionResult> GetAuthenticatedUser([FromServices] IConfiguration config)
        {
            var userId = await _currentUserService.GetUserIdAsync();
            if (userId <= 0)
                return Unauthorized();

            string username;
            try
            {
                var connString = config.GetConnectionString("Default");
                if (string.IsNullOrWhiteSpace(connString))
                    return StatusCode(500);

                await using var conn = new NpgsqlConnection(connString);
                await conn.OpenAsync();
                await using var cmd = new NpgsqlCommand("select user_name from users where user_id = @id limit 1", conn);
                cmd.Parameters.AddWithValue("id", userId);
                var obj = await cmd.ExecuteScalarAsync();
                if (obj is not string name || string.IsNullOrWhiteSpace(name))
                    return Unauthorized();

                username = name;
            }
            catch
            {
                return Unauthorized();
            }

            return Ok(new
            {
                id = userId,
                name = username,
                displayName = username,
                isStaff = false
            });
        }

        [HttpGet("users/{userId:long}")]
        public async Task<IActionResult> GetUserById([FromRoute] long userId, [FromServices] IConfiguration config)
        {
            if (userId <= 0)
                return NotFound();

            var connString = config.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connString))
                return StatusCode(500);

            string username = string.Empty;

            try
            {
                await using var conn = new NpgsqlConnection(connString);
                await conn.OpenAsync();
                await using var cmd = new NpgsqlCommand(@"
                    select user_name
                    from users
                    where user_id = @id
                    limit 1", conn);
                cmd.Parameters.AddWithValue("id", userId);

                await using var reader = await cmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                    return NotFound();

                username = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
            }
            catch
            {
                return StatusCode(500);
            }

            return Ok(new
            {
                Id = userId,
                Username = username
            });
        }

        [HttpGet("users/{userId:long}/canmanage/{placeId:long}")]
        public async Task<IActionResult> CanManage(long userId, long placeId, [FromServices] IConfiguration config)
        {
            if (userId <= 0 || placeId <= 0)
                return Ok(new { Success = false, CanManage = false });

            var connString = config.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connString))
                return StatusCode(500);

            var creatorId = await AssetsRepository.GetAssetCreatorIdAsync(connString, placeId);
            var canManage = creatorId.HasValue && creatorId.Value == userId;

            return Ok(new
            {
                Success = canManage,
                CanManage = canManage
            });
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
