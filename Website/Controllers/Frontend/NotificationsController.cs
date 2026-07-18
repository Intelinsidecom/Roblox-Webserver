using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Website.Controllers.Frontend;

[ApiController]
public class NotificationsController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly NotificationService _notificationService;

    public NotificationsController(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        var connStr = DatabaseUtilities.GetConnectionString(configuration);
        _notificationService = new NotificationService(connStr);
    }

    private long? GetCurrentUserId()
    {
        var claim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(claim) || !long.TryParse(claim, out var userId) || userId <= 0)
            return null;
        return userId;
    }

    [Authorize]
    [HttpGet("notification-stream/notification-stream-data")]
    public IActionResult GetInitializeData()
    {
        var baseUrl = _configuration["PublicBaseUrl"]
                   ?? _configuration["BaseUrl"]
                   ?? $"{Request.Scheme}://{Request.Host}";

        var response = new
        {
            NotificationDomain = baseUrl,
            CurrentUserId = GetCurrentUserId() ?? 0,
            InApp = false,
            IsUserOnPhone = false,
            InAndroidApp = false,
            IniOSApp = false,
            InUWPApp = false,
            BannerDismissTimeSpan = 4000,
            SignalRDisconnectionResponseInMilliseconds = 5000,
            IsChatDisabledByPrivacySetting = false,
            AllowedNotificationSourceTypes = new[]
            {
                "Test", "FriendRequestReceived", "FriendRequestAccepted",
                "PartyInviteReceived", "PartyMemberJoined", "ChatNewMessage",
                "PrivateMessageReceived", "UserAddedToPrivateServerWhiteList",
                "ConversationUniverseChanged", "TeamCreateInvite", "GameUpdate",
                "DeveloperMetricsAvailable", "CommentOnAsset", "AssetPurchased",
                "AssetFavorited"
            }
        };

        return Content(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = null
        }), "application/json");
    }

    [Authorize]
    [HttpGet("v2/stream-notifications/unread-count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var count = await _notificationService.GetUnreadCountAsync(userId.Value, cancellationToken).ConfigureAwait(false);

        return Content(JsonSerializer.Serialize(new { unreadNotifications = count }, new JsonSerializerOptions
        {
            PropertyNamingPolicy = null
        }), "application/json");
    }

    [Authorize]
    [HttpGet("v2/stream-notifications/get-recent")]
    public async Task<IActionResult> GetRecentNotifications(
        [FromQuery(Name = "startIndex")] int startIndex = 0,
        [FromQuery(Name = "maxRows")] int maxRows = 10,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var notifications = await _notificationService.GetRecentNotificationsAsync(
            userId.Value, startIndex, maxRows, cancellationToken).ConfigureAwait(false);

        var response = notifications.Select(n => new
        {
            id = n.Id,
            notificationSourceType = n.NotificationSourceType,
            eventCount = 1,
            isInteracted = n.IsInteracted,
            metadataCollection = new[]
            {
                new
                {
                    SenderUserId = n.SenderUserId?.ToString() ?? "",
                    SenderUserName = n.SenderUserName,
                    SubjectType = n.SubjectType,
                    SubjectId = n.SubjectId.ToString(),
                    SubjectName = n.SubjectName
                }
            },
            created = n.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
        }).ToList();

        return Content(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = null
        }), "application/json");
    }

    [Authorize]
    [HttpPost("v2/stream-notifications/clear-unread")]
    public async Task<IActionResult> ClearUnread(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        await _notificationService.ClearUnreadAsync(userId.Value, cancellationToken).ConfigureAwait(false);
        return Ok(new { });
    }

    [Authorize]
    [HttpPost("v2/stream-notifications/mark-interacted")]
    public async Task<IActionResult> MarkInteracted(
        [FromBody] MarkInteractedRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null || request.EventId <= 0)
            return BadRequest(new { error = "Invalid request" });

        await _notificationService.MarkInteractedAsync(request.EventId, cancellationToken).ConfigureAwait(false);
        return Ok(new { });
    }

    [Authorize]
    [HttpPost("v2/notifications/update-notification-settings")]
    public IActionResult UpdateNotificationSettings([FromBody] object settings)
    {
        return Ok(new { });
    }

    public class MarkInteractedRequest
    {
        [JsonPropertyName("eventId")]
        public long EventId { get; set; }
    }
}
