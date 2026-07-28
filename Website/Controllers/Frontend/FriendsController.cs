using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Users;
using Website.Hubs;
using Website.Services;

namespace Website.Controllers;

public class FriendsController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly IHubContext<NotificationHub> _hubContext;

    public FriendsController(IConfiguration configuration, IHubContext<NotificationHub> hubContext)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _hubContext = hubContext;
    }

    [HttpGet("system-feedback")]
    public IActionResult SystemFeedback()
    {
        return Content("<div class=alert-system-feedback><div class=\"alert alert-warning\"></div></div>", "text/html");
    }

    [HttpGet("users/{id}/friends")]
    public async Task<IActionResult> Index(long id)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        long currentUserId = 0;
        if (!string.IsNullOrWhiteSpace(userIdStr))
            long.TryParse(userIdStr, out currentUserId);

        var currentUserName = User.Identity?.Name ?? "Guest";
        var profileUserName = "User";

        var connStr = _configuration.GetConnectionString("Default");
        if (!string.IsNullOrWhiteSpace(connStr) && id > 0)
        {
            var name = await Users.UserQueries.GetUserNameByIdAsync(connStr, id).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(name))
                profileUserName = name;
        }

        ViewBag.CurrentUserId = currentUserId;
        ViewBag.CurrentUserName = currentUserName;
        ViewBag.ProfileUserId = id;
        ViewBag.ProfileUserName = profileUserName;
        ViewBag.IsOwnProfile = currentUserId > 0 && currentUserId == id;

        return View("~/Views/Pages/users/{id}/friends.cshtml");
    }

    [HttpGet("users/friends/list-json")]
    public async Task<IActionResult> GetFriendList(
        [FromQuery] long userId,
        [FromQuery] long currentPage = 0,
        [FromQuery] int pageSize = 18,
        [FromQuery] string? friendsType = "AllFriends",
        CancellationToken cancellationToken = default)
    {
        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr) || userId <= 0)
            return Json(new { Friends = Array.Empty<object>(), TotalFriends = 0, CurrentPage = 0 });

        var currentUserId = GetCurrentUserId();

        var validTypes = new[] { "AllFriends", "Following", "Followers", "FriendRequests" };
        if (!validTypes.Contains(friendsType))
            friendsType = "AllFriends";

        var friends = await UserQueries.GetFriendListAsync(
            connStr, userId, currentUserId, currentPage, pageSize, friendsType, cancellationToken).ConfigureAwait(false);

        var defaultThumb = _configuration["Thumbnails:DefaultThumbnailUrl"];
        if (string.IsNullOrWhiteSpace(defaultThumb)) defaultThumb = "/images/default.png";
        foreach (var f in friends)
        {
            if (f.TryGetValue("UserId", out var uidObj) && uidObj is long uid && uid > 0)
                f["AbsoluteURL"] = $"/users/{uid}/profile";

            if (f.TryGetValue("Thumbnail", out var thumbObj) && thumbObj is Dictionary<string, object?> thumb)
            {
                var url = thumb.GetValueOrDefault("Url") as string;
                if (string.IsNullOrEmpty(url))
                {
                    thumb["Url"] = defaultThumb;
                    var tuid = thumb.GetValueOrDefault("UserId") as long? ?? 0;
                    thumb["RetryUrl"] = $"/thumbnail/avatar-headshot?userId={tuid}";
                }
                f["AvatarUri"] = thumb["Url"];
            }
        }

        var totalFriends = await UserQueries.GetFriendListTotalCountAsync(
            connStr, userId, friendsType, cancellationToken).ConfigureAwait(false);

        return Json(new
        {
            Friends = friends,
            TotalFriends = totalFriends,
            CurrentPage = currentPage
        });
    }

    [Authorize]
    [HttpPost("api/friends/sendfriendrequest")]
    public async Task<IActionResult> SendFriendRequest(
        [FromBody] TargetUserRequest? request,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId <= 0)
            return Json(new { success = false, errorId = 3 });

        if (request?.targetUserId == null || request.targetUserId <= 0)
            return Json(new { success = false, errorId = 3 });

        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr))
            return Json(new { success = false, errorId = 3 });

        var result = await UserQueries.SendFriendRequestAsync(
            connStr, currentUserId, request.targetUserId.Value, cancellationToken).ConfigureAwait(false);

        if (result != null && result.TryGetValue("success", out var s) && s is true)
        {
            var senderName = await UserQueries.GetUserNameByIdAsync(connStr, currentUserId, cancellationToken).ConfigureAwait(false) ?? "";
            var svc = new NotificationService(connStr);
            await svc.CreateNotificationAsync(
                request.targetUserId.Value,
                "FriendRequestReceived",
                currentUserId,
                senderName,
                "User",
                currentUserId,
                senderName,
                cancellationToken).ConfigureAwait(false);
            await NotificationBroadcaster.BroadcastNewNotification(
                _hubContext, request.targetUserId.Value, cancellationToken).ConfigureAwait(false);
        }

        return Json(result!);
    }

    [Authorize]
    [HttpPost("api/friends/acceptfriendrequest")]
    public async Task<IActionResult> AcceptFriendRequest(
        [FromBody] AcceptDeclineRequest? request,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId <= 0 || request?.invitationID == null || request?.targetUserID == null)
            return Json(new { success = false, errorId = 3 });

        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr))
            return Json(new { success = false, errorId = 3 });

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

    [Authorize]
    [HttpPost("api/friends/declinefriendrequest")]
    public async Task<IActionResult> DeclineFriendRequest(
        [FromBody] AcceptDeclineRequest? request,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId <= 0 || request?.invitationID == null)
            return Json(new { success = false });

        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr))
            return Json(new { success = false });

        var targetUserId = request.targetUserID > 0 ? request.targetUserID.Value : 0;
        var result = await UserQueries.DeclineFriendRequestAsync(
            connStr, request.invitationID.Value, cancellationToken: cancellationToken, senderId: targetUserId, receiverId: currentUserId).ConfigureAwait(false);

        return Json(result);
    }

    [Authorize]
    [HttpPost("api/friends/declineallfriendrequests")]
    public async Task<IActionResult> DeclineAllFriendRequests(
        CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId <= 0)
            return Json(new { success = false });

        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr))
            return Json(new { success = false });

        var result = await UserQueries.DeclineAllFriendRequestsAsync(
            connStr, currentUserId, cancellationToken).ConfigureAwait(false);

        return Json(result);
    }

    [Authorize]
    [HttpPost("api/friends/removefriend")]
    public async Task<IActionResult> RemoveFriend(
        [FromBody] TargetUserRequest? request,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId <= 0 || request?.targetUserId == null || request.targetUserId <= 0)
            return Json(new { success = false });

        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr))
            return Json(new { success = false });

        var result = await UserQueries.RemoveFriendshipAsync(
            connStr, currentUserId, request.targetUserId.Value, cancellationToken).ConfigureAwait(false);

        return Json(result);
    }

    private long GetCurrentUserId()
    {
        var idClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(idClaim))
            return 0;
        if (long.TryParse(idClaim, out var id))
            return id;
        return 0;
    }

    public class TargetUserRequest
    {
        public long? targetUserId { get; set; }
    }

    public class AcceptDeclineRequest
    {
        public long? invitationID { get; set; }
        public long? targetUserID { get; set; }
    }
}
