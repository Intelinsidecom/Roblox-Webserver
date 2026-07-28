using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Users;

namespace Website.Controllers.Frontend;

[ApiController]
public class FriendsApiController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public FriendsApiController(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    private static string Serialize(object obj)
    {
        return JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
    }

    [Authorize]
    [HttpGet("v1/users/{userId:long}/friends")]
    public async Task<IActionResult> GetFriends(
        long userId,
        [FromQuery(Name = "limit")] int limit = 100,
        [FromQuery(Name = "sortOrder")] string? sortOrder = null,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
            return Content(Serialize(new { data = new object[] { } }), "application/json");

        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr))
            return Content(Serialize(new { data = new object[] { } }), "application/json");

        try
        {
            var friends = await UserQueries.GetFriendListAsync(
                connStr, userId, 0, 0, Math.Min(limit, 200), "AllFriends", cancellationToken)
                .ConfigureAwait(false);

            var data = friends.Select(f => new
            {
                isOnline = f.TryGetValue("IsOnline", out var online) && online is true,
                friendFrequentScore = 0,
                friendFrequentRank = 0,
                id = f.TryGetValue("UserId", out var uid) ? uid : 0,
                name = f.TryGetValue("Username", out var uname) ? uname : "",
                Username = f.TryGetValue("Username", out var uname2) ? uname2 : "",
                displayName = f.TryGetValue("DisplayName", out var dname) ? dname : "",
                externalAppDisplayName = (string?)null,
                isDeleted = false,
                friendFrequentScoreRank = 0,
                displayNameReasonType = "None"
            }).ToList();

            return Content(Serialize(new { data }), "application/json");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FriendsApiController] GetFriends error: {ex.Message}");
            return Content(Serialize(new { data = new object[] { } }), "application/json");
        }
    }
}
