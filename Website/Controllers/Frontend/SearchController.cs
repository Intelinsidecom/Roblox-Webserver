using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Users;
using Website.Hubs;
using Website.Services;

namespace Website.Controllers;

public class SearchController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly IHubContext<NotificationHub> _hubContext;

    public SearchController(IConfiguration configuration, IHubContext<NotificationHub> hubContext)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _hubContext = hubContext;
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

    [HttpPost("search/users/add-friend")]
    public async Task<IActionResult> AddFriend(
        [FromBody] AddFriendRequest? request,
        CancellationToken cancellationToken = default)
    {
        var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        long currentUserId = 0;
        if (!string.IsNullOrWhiteSpace(currentUserIdStr))
            long.TryParse(currentUserIdStr, out currentUserId);

        if (currentUserId <= 0)
            return Json(new { success = false, message = "Not authenticated" });

        if (request?.targetUserID == null || request.targetUserID <= 0)
            return Json(new { success = false, message = "Invalid target" });

        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr))
            return Json(new { success = false, message = "Server error" });

        var result = await UserQueries.SendFriendRequestAsync(
            connStr, currentUserId, request.targetUserID.Value, cancellationToken).ConfigureAwait(false);

        if (result != null && result.TryGetValue("success", out var s) && s is true)
        {
            var senderName = await UserQueries.GetUserNameByIdAsync(connStr, currentUserId, cancellationToken).ConfigureAwait(false) ?? "";
            var svc = new NotificationService(connStr);
            await svc.CreateNotificationAsync(
                request.targetUserID.Value,
                "FriendRequestReceived",
                currentUserId,
                senderName,
                "User",
                currentUserId,
                senderName,
                cancellationToken).ConfigureAwait(false);
            await NotificationBroadcaster.BroadcastNewNotification(
                _hubContext, request.targetUserID.Value, cancellationToken).ConfigureAwait(false);
        }

        return Json(result ?? new Dictionary<string, object?> { ["success"] = false });
    }

    [HttpPost("search/users/accept-friend")]
    public async Task<IActionResult> AcceptFriend(
        [FromBody] AcceptFriendRequest? request,
        CancellationToken cancellationToken = default)
    {
        var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        long currentUserId = 0;
        if (!string.IsNullOrWhiteSpace(currentUserIdStr))
            long.TryParse(currentUserIdStr, out currentUserId);

        if (currentUserId <= 0)
            return Json(new { success = false, message = "Not authenticated" });

        if (request?.targetUserID == null || request.invitationID == null)
            return Json(new { success = false, message = "Invalid parameters" });

        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr))
            return Json(new { success = false, message = "Server error" });

        var result = await UserQueries.AcceptFriendRequestAsync(
            connStr, request.invitationID.Value, request.targetUserID.Value, currentUserId, cancellationToken).ConfigureAwait(false);

        if (result != null && result.TryGetValue("success", out var s) && s is true)
        {
            var accepterName = await UserQueries.GetUserNameByIdAsync(connStr, currentUserId, cancellationToken).ConfigureAwait(false) ?? "";
            var svc = new NotificationService(connStr);
            await svc.CreateNotificationAsync(
                request.targetUserID.Value,
                "FriendRequestAccepted",
                currentUserId,
                accepterName,
                "User",
                currentUserId,
                accepterName,
                cancellationToken).ConfigureAwait(false);
            await NotificationBroadcaster.BroadcastNewNotification(
                _hubContext, request.targetUserID.Value, cancellationToken).ConfigureAwait(false);
        }

        return Json(result);
    }

    [HttpGet("search/users/relation-and-presence")]
    public async Task<IActionResult> RelationAndPresence(
        [FromQuery] long[]? userIds,
        CancellationToken cancellationToken = default)
    {
        var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        long currentUserId = 0;
        if (!string.IsNullOrWhiteSpace(currentUserIdStr))
            long.TryParse(currentUserIdStr, out currentUserId);

        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr) || userIds == null || userIds.Length == 0)
            return Json(new { PlayerPresences = Array.Empty<object>(), PlayerRelationships = Array.Empty<object>() });

        var userIdsList = userIds.Where(u => u > 0).Distinct().ToList();

        var (presences, relationships) = await UserQueries.GetRelationAndPresenceAsync(
            connStr, currentUserId, userIdsList, cancellationToken).ConfigureAwait(false);

        return Json(new { PlayerPresences = presences, PlayerRelationships = relationships });
    }

    [HttpGet("search/users/avatars")]
    public async Task<IActionResult> Avatars(
        [FromQuery] long[]? userIds,
        [FromQuery] bool isHeadshot = false,
        CancellationToken cancellationToken = default)
    {
        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr) || userIds == null || userIds.Length == 0)
            return Json(new { PlayerAvatars = Array.Empty<object>() });

        var urls = await Thumbnails.ThumbnailQueries.GetUserHeadshotUrlsAsync(
            connStr, userIds, cancellationToken).ConfigureAwait(false);

        var avatars = new List<Dictionary<string, object?>>();
        foreach (var userId in userIds)
        {
            var url = urls.TryGetValue(userId, out var u) ? u : null;
            avatars.Add(new Dictionary<string, object?>
            {
                ["UserId"] = userId,
                ["Thumbnail"] = new Dictionary<string, object?>
                {
                    ["Url"] = url ?? $"/headshot-thumbnail/image?userId={userId}&width=420&height=420&format=png"
                }
            });
        }

        return Json(new { PlayerAvatars = avatars });
    }

    public class AddFriendRequest
    {
        public long? targetUserID { get; set; }
    }

    public class AcceptFriendRequest
    {
        public long? targetUserID { get; set; }
        public long? invitationID { get; set; }
    }
}
