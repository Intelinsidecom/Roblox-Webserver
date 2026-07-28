using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Assets;
using Npgsql;
using System.Security.Claims;
using System.Threading.Tasks;
using Games;
using Users;
using Api.Services;

namespace Api.Controllers
{
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly CurrentUserService _currentUserService;

        public UsersController(CurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

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
        public async Task<IActionResult> GetBlockedUsers([FromQuery] long userId, [FromQuery] int page = 1, [FromServices] IConfiguration config = null)
        {
            var connString = config.GetConnectionString("Default");
            var result = await Users.UserQueries.GetBlockedUsersAsync(connString, userId).ConfigureAwait(false);
            return Ok(result);
        }

        public class MultiFollowingExistsRequest
        {
            public long userId { get; set; }
            public long[] otherUserIds { get; set; }
        }

        [HttpPost("user/multi-following-exists")]
        public async Task<IActionResult> MultiFollowingExists([FromBody] MultiFollowingExistsRequest request, [FromServices] IConfiguration config)
        {
            if (request == null || request.userId <= 0 || request.otherUserIds == null || request.otherUserIds.Length == 0)
                return Ok(new { FollowingDetails = new object[0] });

            var connString = config.GetConnectionString("Default");
            var result = await Users.UserQueries.GetMultiFollowingExistsAsync(connString, request.userId, request.otherUserIds).ConfigureAwait(false);
            return Ok(result);
        }

        [HttpGet("user/get-friendship-count")]
        public async Task<IActionResult> GetFriendshipCount([FromQuery] long userId, [FromServices] IConfiguration config)
        {
            var currentUserId = await _currentUserService.GetUserIdAsync();
            if (userId <= 0)
                userId = currentUserId;

            if (userId <= 0)
                return Ok(new { success = true, count = 0, message = "" });

            var connString = config.GetConnectionString("Default");
            var count = await Users.UserQueries.GetFriendCountAsync(connString, userId).ConfigureAwait(false);
            return Ok(new { success = true, count = count, message = "" });
        }

        [HttpPost("user/request-friendship")]
        public async Task<IActionResult> RequestFriendship([FromQuery] long recipientUserId, [FromServices] IConfiguration config)
        {
            var senderId = await _currentUserService.GetUserIdAsync();
            if (senderId <= 0)
                return Ok(new { success = false });

            if (recipientUserId <= 0)
                return Ok(new { success = false });

            var connString = config.GetConnectionString("Default");
            var result = await Users.UserQueries.SendFriendRequestAsync(connString, senderId, recipientUserId).ConfigureAwait(false);
            return Ok(new { success = true });
        }

        [HttpPost("user/decline-friend-request")]
        public async Task<IActionResult> DeclineFriendRequest([FromQuery] long requesterUserId, [FromServices] IConfiguration config)
        {
            var currentUserId = await _currentUserService.GetUserIdAsync();
            if (currentUserId <= 0)
                return Ok(new { success = false });

            if (requesterUserId <= 0)
                return Ok(new { success = false });

            var connString = config.GetConnectionString("Default");
            var result = await Users.UserQueries.RevokeFriendshipAsync(connString, currentUserId, requesterUserId).ConfigureAwait(false);
            return Ok(new { success = true });
        }

        [HttpGet("users/{userId:long}/friends")]
        public async Task<IActionResult> GetFriendsForClient(
            [FromRoute] long userId,
            [FromServices] IConfiguration config,
            [FromQuery] int page = 1)
        {
            if (userId <= 0)
                return Ok(Array.Empty<object>());

            var connStr = config.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
                return Ok(Array.Empty<object>());

            try
            {
                const int pageSize = 100;
                var friends = await UserQueries.GetFriendListAsync(
                    connStr, userId, 0, 0, 1000, "AllFriends").ConfigureAwait(false);

                var start = (page - 1) * pageSize;
                if (start >= friends.Count)
                    return Content("[]", "application/json");

                var pageItems = friends.Skip(start).Take(pageSize).Select(f => new
                {
                    Username = f.TryGetValue("Username", out var uname) ? uname : "",
                    Id = f.TryGetValue("UserId", out var uid) ? uid : 0
                }).ToList();

                return Content(System.Text.Json.JsonSerializer.Serialize(pageItems), "application/json");
            }
            catch
            {
                return Content("[]", "application/json");
            }
        }

        [HttpGet("user/following-exists")]
        public async Task<IActionResult> FollowingExists([FromQuery] long userId, [FromQuery] long followerUserId, [FromServices] IConfiguration config)
        {
            if (userId <= 0 || followerUserId <= 0)
                return Ok(new { success = true, isFollowing = false });

            var connString = config.GetConnectionString("Default");
            var isFollowing = await Users.UserQueries.IsFollowingAsync(connString, followerUserId, userId).ConfigureAwait(false);
            return Ok(new { success = true, isFollowing = isFollowing });
        }

        [HttpGet("my/friendsonline")]
        public async Task<IActionResult> GetFriendsOnline([FromServices] IConfiguration config)
        {
            var currentUserId = await _currentUserService.GetUserIdAsync();
            if (currentUserId <= 0)
                return Ok(Array.Empty<long>());

            var connString = config.GetConnectionString("Default");
            var onlineFriendIds = await Users.UserQueries.GetOnlineFriendsAsync(connString, currentUserId).ConfigureAwait(false);
            return Ok(onlineFriendIds);
        }

        [HttpPost("user/follow")]
        public async Task<IActionResult> FollowGameClient([FromForm] long followedUserId, [FromServices] IConfiguration config)
        {
            var currentUserId = await _currentUserService.GetUserIdAsync();
            if (currentUserId <= 0)
                return Ok(new { success = false });

            if (followedUserId <= 0)
                return Ok(new { success = false });

            var connString = config.GetConnectionString("Default");
            var result = await Users.UserQueries.FollowUserAsync(connString, currentUserId, followedUserId).ConfigureAwait(false);
            return Ok(new { success = true });
        }

        [HttpPost("user/unfollow")]
        public async Task<IActionResult> UnfollowGameClient([FromForm] long followedUserId, [FromServices] IConfiguration config)
        {
            var currentUserId = await _currentUserService.GetUserIdAsync();
            if (currentUserId <= 0)
                return Ok(new { success = false });

            if (followedUserId <= 0)
                return Ok(new { success = false });

            var connString = config.GetConnectionString("Default");
            var result = await Users.UserQueries.UnfollowUserAsync(connString, currentUserId, followedUserId).ConfigureAwait(false);
            return Ok(new { success = true });
        }
    }
}
