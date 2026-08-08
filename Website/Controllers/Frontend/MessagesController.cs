using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Users;
using Website.Hubs;
using Website.Services;

namespace Website.Controllers.Frontend;

[ApiController]
public class MessagesController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly PrivateMessageQueries _messageQueries;
    private readonly IHubContext<NotificationHub> _hubContext;

    public MessagesController(IConfiguration configuration, IHubContext<NotificationHub> hubContext)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        var connStr = DatabaseUtilities.GetConnectionString(configuration);
        _messageQueries = new PrivateMessageQueries(connStr);
    }

    private long? GetCurrentUserId()
    {
        var claim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(claim) || !long.TryParse(claim, out var userId) || userId <= 0)
            return null;
        return userId;
    }

    private string GetBaseUrl()
    {
        return _configuration["PublicBaseUrl"]
            ?? _configuration["BaseUrl"]
            ?? $"{Request.Scheme}://{Request.Host}";
    }

    private static string GetUserHeadshotUrl(long userId)
    {
        return $"/headshot-thumbnail/image?userId={userId}&width=48&height=48";
    }

    private static (string subject, string body) FormatNotificationContent(NotificationService.NotificationData n)
    {
        return n.NotificationSourceType switch
        {
            "PrivateMessageReceived" => (
                "sent you a message",
                $"<p><a href=\"/my/messages#!/inbox?page=1&amp;conversationId={n.SubjectId}\">{WebUtility.HtmlEncode(n.SubjectName)}</a></p>"
            ),
            "FriendRequestReceived" => (
                "wants to be your friend",
                ""
            ),
            "FriendRequestAccepted" => (
                "accepted your friend request",
                ""
            ),
            "GameUpdate" => (
                $"{n.SubjectName}",
                ""
            ),
            "CommentOnAsset" => (
                $"commented on {n.SubjectName}",
                ""
            ),
            "AssetPurchased" => (
                $"purchased {n.SubjectName}",
                ""
            ),
            "AssetFavorited" => (
                $"favorited {n.SubjectName}",
                ""
            ),
            "TradeRequestReceived" => (
                "sent you a trade request",
                $"<p><a href=\"/my/money#!/TradeItems\">View Trade</a></p>"
            ),
            _ => (
                n.SubjectName,
                ""
            )
        };
    }

    private static string Serialize(object obj)
    {
        return JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            PropertyNamingPolicy = null
        });
    }

    [Authorize]
    [HttpGet("messages/api/get-messages")]
    public async Task<IActionResult> GetMessages(
        [FromQuery(Name = "messageTab")] int messageTab = 0,
        [FromQuery(Name = "pageNumber")] int pageNumber = 0,
        [FromQuery(Name = "pageSize")] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var result = await _messageQueries.GetMessagesAsync(userId.Value, messageTab, pageNumber, pageSize, cancellationToken)
            .ConfigureAwait(false);

        var baseUrl = GetBaseUrl();
        var response = new
        {
            result.PageNumber,
            result.TotalPages,
            result.TotalCollectionSize,
            Collection = result.Collection.Select(m => new
            {
                m.Id,
                m.Subject,
                m.Body,
                Sender = new
                {
                    UserId = m.SenderId,
                    UserName = m.SenderUserName
                },
                SenderAbsoluteUrl = $"{baseUrl}/users/{m.SenderId}/profile",
                SenderThumbnail = new
                {
                    Url = GetUserHeadshotUrl(m.SenderId),
                    Final = false,
                    RetryUrl = $"/thumbnail/avatar-headshot?userId={m.SenderId}&width=48&height=48"
                },
                Recipient = new
                {
                    UserId = m.RecipientId,
                    UserName = m.RecipientUserName
                },
                RecipientAbsoluteUrl = $"{baseUrl}/users/{m.RecipientId}/profile",
                RecipientThumbnail = new
                {
                    Url = GetUserHeadshotUrl(m.RecipientId),
                    Final = false,
                    RetryUrl = $"/thumbnail/avatar-headshot?userId={m.RecipientId}&width=48&height=48"
                },
                m.IsRead,
                m.IsSystemMessage,
                IsReportAbuseDisplayed = false,
                AbuseReportAbsoluteUrl = "",
                Created = m.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
            }).ToList()
        };

        return Content(Serialize(response), "application/json");
    }

    [Authorize]
    [HttpGet("notifications/api/get-notifications")]
    public async Task<IActionResult> GetNotifications(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var notificationService = new NotificationService(DatabaseUtilities.GetConnectionString(_configuration));
        var notifications = await notificationService.GetRecentNotificationsAsync(userId.Value, 0, 100, cancellationToken)
            .ConfigureAwait(false);

        var baseUrl = GetBaseUrl();
        var response = new
        {
            PageNumber = 0,
            TotalPages = 1,
            TotalCollectionSize = notifications.Count,
            Collection = notifications.Select(n =>
            {
                var (subject, body) = FormatNotificationContent(n);
                return new
                {
                    Id = n.Id,
                    Subject = subject,
                    Body = body,
                    Sender = new
                    {
                        UserId = n.SenderUserId ?? 0,
                        UserName = n.SenderUserName
                    },
                    SenderAbsoluteUrl = n.SenderUserId.HasValue ? $"{baseUrl}/users/{n.SenderUserId}/profile" : "",
                    SenderThumbnail = new
                    {
                        Url = n.SenderUserId.HasValue ? GetUserHeadshotUrl(n.SenderUserId.Value) : "",
                        Final = false,
                        RetryUrl = n.SenderUserId.HasValue ? $"/thumbnail/avatar-headshot?userId={n.SenderUserId}&width=48&height=48" : ""
                    },
                    Recipient = new
                    {
                        UserId = userId.Value,
                        UserName = ""
                    },
                    RecipientAbsoluteUrl = "",
                    RecipientThumbnail = new
                    {
                        Url = GetUserHeadshotUrl(userId.Value),
                        Final = true
                    },
                    IsRead = n.IsRead,
                    IsSystemMessage = false,
                    IsReportAbuseDisplayed = false,
                    AbuseReportAbsoluteUrl = "",
                    Created = n.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
                };
            }).ToList()
        };

        await notificationService.ClearUnreadAsync(userId.Value, cancellationToken)
            .ConfigureAwait(false);

        return Content(Serialize(response), "application/json");
    }

    [Authorize]
    [HttpPost("messages/api/mark-messages-read")]
    public async Task<IActionResult> MarkRead([FromBody] MarkRequest request, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        if (request?.MessageIds == null || request.MessageIds.Length == 0)
            return Ok(new { });

        await _messageQueries.MarkReadAsync(request.MessageIds, userId.Value, cancellationToken)
            .ConfigureAwait(false);

        return Ok(new { });
    }

    [Authorize]
    [HttpPost("messages/api/mark-messages-unread")]
    public async Task<IActionResult> MarkUnread([FromBody] MarkRequest request, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        if (request?.MessageIds == null || request.MessageIds.Length == 0)
            return Ok(new { });

        await _messageQueries.MarkUnreadAsync(request.MessageIds, userId.Value, cancellationToken)
            .ConfigureAwait(false);

        return Ok(new { });
    }

    [Authorize]
    [HttpPost("messages/api/archive-messages")]
    public async Task<IActionResult> ArchiveMessages([FromBody] MarkRequest request, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        if (request?.MessageIds == null || request.MessageIds.Length == 0)
            return Ok(new { });

        await _messageQueries.ArchiveMessagesAsync(request.MessageIds, userId.Value, cancellationToken)
            .ConfigureAwait(false);

        return Ok(new { });
    }

    [Authorize]
    [HttpPost("messages/api/unarchive-messages")]
    public async Task<IActionResult> UnarchiveMessages([FromBody] MarkRequest request, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        if (request?.MessageIds == null || request.MessageIds.Length == 0)
            return Ok(new { });

        await _messageQueries.UnarchiveMessagesAsync(request.MessageIds, userId.Value, cancellationToken)
            .ConfigureAwait(false);

        return Ok(new { });
    }

    [Authorize]
    [HttpPost("messages/api/send-message")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        if (request == null || request.RecipientId <= 0)
            return Content(Serialize(new { success = false, message = "Invalid recipient" }), "application/json");

        if (string.IsNullOrWhiteSpace(request.Body))
            return Content(Serialize(new { success = false, message = "Message body is required" }), "application/json");

        return await SendPrivateMessageAsync(
            userId.Value,
            request.RecipientId,
            request.Subject ?? "",
            request.Body,
            request.ReplyMessageId > 0 ? request.ReplyMessageId : null,
            request.IncludePreviousMessage,
            cancellationToken).ConfigureAwait(false);
    }

    [Authorize]
    [HttpPost("messages/send")]
    public async Task<IActionResult> Send([FromBody] ComposeSendRequest request, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        if (request == null || request.To <= 0)
            return StatusCode(400);

        if (string.IsNullOrWhiteSpace(request.Body))
            return StatusCode(400);

        var recipientExists = await _messageQueries.UserExistsAsync(request.To, cancellationToken)
            .ConfigureAwait(false);
        if (!recipientExists)
            return StatusCode(400);

        return await SendPrivateMessageAsync(
            userId.Value,
            request.To,
            request.Subject ?? "",
            request.Body,
            null,
            false,
            cancellationToken).ConfigureAwait(false);
    }

    private async Task<IActionResult> SendPrivateMessageAsync(
        long senderId,
        long recipientId,
        string subject,
        string body,
        long? replyMessageId,
        bool includePreviousMessage,
        CancellationToken cancellationToken)
    {
        var recipientExists = await _messageQueries.UserExistsAsync(recipientId, cancellationToken)
            .ConfigureAwait(false);
        if (!recipientExists)
            return Content(Serialize(new { success = false, message = "Recipient not found" }), "application/json");

        var canMessage = await UserQueries.CanMessageUserAsync(
            DatabaseUtilities.GetConnectionString(_configuration), senderId, recipientId, cancellationToken)
            .ConfigureAwait(false);
        if (!canMessage)
            return Content(Serialize(new { success = false, message = "You cannot message this user due to their privacy settings" }), "application/json");

        try
        {
            var messageId = await _messageQueries.SendMessageAsync(
                senderId,
                recipientId,
                subject,
                body,
                replyMessageId,
                includePreviousMessage,
                cancellationToken).ConfigureAwait(false);

            var senderName = await _messageQueries.GetUserNameAsync(senderId, cancellationToken)
                .ConfigureAwait(false);

            var notificationService = new NotificationService(DatabaseUtilities.GetConnectionString(_configuration));
            await notificationService.CreateNotificationAsync(
                recipientId,
                "PrivateMessageReceived",
                senderId,
                senderName,
                "Message",
                messageId,
                subject,
                cancellationToken).ConfigureAwait(false);

            await NotificationBroadcaster.BroadcastNewNotification(_hubContext, recipientId, cancellationToken)
                .ConfigureAwait(false);

            var unreadCount = await _messageQueries.GetUnreadCountAsync(recipientId, cancellationToken)
                .ConfigureAwait(false);
            await _hubContext.Clients
                .Group($"user_{recipientId}")
                .SendAsync("notification", "MessagesCountChanged",
                    JsonSerializer.Serialize(new { unreadMessages = unreadCount }),
                    0, cancellationToken).ConfigureAwait(false);

            return Content(Serialize(new { success = true, message = "" }), "application/json");
        }
        catch (Exception ex)
        {
            return Content(Serialize(new { success = false, message = ex.Message }), "application/json");
        }
    }

    [Authorize]
    [HttpGet("messages/api/get-message/{id:long}")]
    public async Task<IActionResult> GetMessageById(long id, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var message = await _messageQueries.GetMessageByIdAsync(id, userId.Value, cancellationToken)
            .ConfigureAwait(false);

        if (message == null)
            return NotFound();

        var baseUrl = GetBaseUrl();
        var response = new
        {
            message.Id,
            message.Subject,
            message.Body,
            Sender = new
            {
                UserId = message.SenderId,
                UserName = message.SenderUserName
            },
            SenderAbsoluteUrl = $"{baseUrl}/users/{message.SenderId}/profile",
            SenderThumbnail = new
            {
                Url = GetUserHeadshotUrl(message.SenderId),
                Final = false,
                RetryUrl = $"/thumbnail/avatar-headshot?userId={message.SenderId}&width=48&height=48"
            },
            Recipient = new
            {
                UserId = message.RecipientId,
                UserName = message.RecipientUserName
            },
            RecipientAbsoluteUrl = $"{baseUrl}/users/{message.RecipientId}/profile",
            RecipientThumbnail = new
            {
                Url = GetUserHeadshotUrl(message.RecipientId),
                Final = false,
                RetryUrl = $"/thumbnail/avatar-headshot?userId={message.RecipientId}&width=48&height=48"
            },
            message.IsRead,
            message.IsSystemMessage,
            IsReportAbuseDisplayed = false,
            AbuseReportAbsoluteUrl = "",
            Created = message.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };

        return Content(Serialize(response), "application/json");
    }

    [Authorize]
    [HttpGet("messages/api/unread-messages-summary")]
    public async Task<IActionResult> GetUnreadSummary(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var count = await _messageQueries.GetUnreadCountAsync(userId.Value, cancellationToken)
            .ConfigureAwait(false);

        return Content(Serialize(new { count = count }), "application/json");
    }

    public class MarkRequest
    {
        [System.Text.Json.Serialization.JsonPropertyName("messageIds")]
        public long[]? MessageIds { get; set; }
    }

    public class SendMessageRequest
    {
        [System.Text.Json.Serialization.JsonPropertyName("subject")]
        public string? Subject { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("body")]
        public string? Body { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("recipientId")]
        public long RecipientId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("replyMessageId")]
        public long ReplyMessageId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("includePreviousMessage")]
        public bool IncludePreviousMessage { get; set; }
    }

    public class ComposeSendRequest
    {
        [System.Text.Json.Serialization.JsonPropertyName("subject")]
        public string? Subject { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("body")]
        public string? Body { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("to")]
        public long To { get; set; }
    }
}
