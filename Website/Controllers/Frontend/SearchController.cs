using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Users;

namespace Website.Controllers;

public class SearchController : Controller
{
    private readonly IConfiguration _configuration;

    public SearchController(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    [HttpGet("search/users/metadata")]
    public async Task<IActionResult> SearchUsersMetadata(
        [FromQuery] string? keyword,
        CancellationToken cancellationToken = default)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        long currentUserId = 0;
        if (!string.IsNullOrWhiteSpace(userIdStr))
            long.TryParse(userIdStr, out currentUserId);

        var isGuest = !User.Identity?.IsAuthenticated ?? true;

        return Json(new
        {
            Keyword = keyword ?? "",
            MaxRows = 12,
            CurrentUserId = currentUserId,
            InApp = false,
            InAndroidApp = false,
            IniOSApp = false,
            IsPhone = false,
            IsTablet = false,
            KeywordMinLength = 3,
            IsGuest = isGuest,
            LoadingImageUrl = "/images/4bed93c91f909002b1f17f05c0ce13d1.gif",
            FriendshipStatusValues = new[] { "NoFriendship", "Friends", "PendingOnCurrentUser", "PendingOnOtherUser" },
            Links = new
            {
                Search = "/search/users/search",
                Avatars = "/search/users/avatars",
                Friendship = "/search/users/friendship",
                Presence = "/search/users/presence",
                RelationAndPresence = "/search/users/relation-and-presence",
                AddFriend = "/search/users/add-friend",
                AcceptFriendRequest = "/search/users/accept-friend"
            }
        });
    }

    [HttpGet("search/users/search")]
    public async Task<IActionResult> SearchUsers(
        [FromQuery] string? keyword,
        [FromQuery] int startIndex = 0,
        [FromQuery] int maxRows = 12,
        CancellationToken cancellationToken = default)
    {
        var connectionString = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
            return Json(new { TotalResults = 0, UserSearchResults = Array.Empty<object>() });

        if (string.IsNullOrWhiteSpace(keyword))
            return Json(new { TotalResults = 0, UserSearchResults = Array.Empty<object>() });

        var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        long currentUserId = 0;
        if (!string.IsNullOrWhiteSpace(currentUserIdStr))
            long.TryParse(currentUserIdStr, out currentUserId);

        var totalResults = await UserQueries.CountUsersByKeywordAsync(connectionString, keyword, cancellationToken);
        var dbResults = await UserQueries.SearchUsersByKeywordAsync(connectionString, keyword, startIndex, maxRows, cancellationToken);

        var results = new List<Dictionary<string, object?>>();
        foreach (var dbResult in dbResults)
        {
            results.Add(new Dictionary<string, object?>
            {
                ["UserId"] = dbResult.UserId,
                ["Name"] = dbResult.Username,
                ["PreviousUserNamesCsv"] = "",
                ["IsOnline"] = false,
                ["InGame"] = false,
                ["InStudio"] = false,
                ["FriendshipStatus"] = currentUserId > 0 && currentUserId == dbResult.UserId ? "Friends" : "NoFriendship",
                ["IsFollowed"] = false,
                ["AllowedToFollowInGame"] = false,
                ["UserProfilePageUrl"] = $"/users/{dbResult.UserId}/profile",
                ["Thumbnail"] = new Dictionary<string, object?>
                {
                    ["Url"] = $"/headshot-thumbnail/image?userId={dbResult.UserId}&width=420&height=420&format=png"
                },
                ["LastLocation"] = "",
                ["AbsolutePlaceUrl"] = "",
                ["PrimaryGroup"] = "",
                ["PrimaryGroupUrl"] = ""
            });
        }

        return Json(new
        {
            TotalResults = (int)totalResults,
            UserSearchResults = results
        });
    }
}
