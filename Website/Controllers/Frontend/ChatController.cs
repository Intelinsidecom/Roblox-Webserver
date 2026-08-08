using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Chat;
using Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Website.Hubs;
using Website.Services;

namespace Website.Controllers.Frontend;

[ApiController]
public class ChatController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ChatRepository _chat;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly TypingTracker _typingTracker;
    private readonly PresenceTracker _presenceTracker;

    public ChatController(
        IConfiguration configuration,
        IHubContext<NotificationHub> hubContext,
        TypingTracker typingTracker,
        PresenceTracker presenceTracker)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _typingTracker = typingTracker ?? throw new ArgumentNullException(nameof(typingTracker));
        _presenceTracker = presenceTracker ?? throw new ArgumentNullException(nameof(presenceTracker));
        _chat = new ChatRepository(DatabaseUtilities.GetConnectionString(configuration));
    }

    private long? GetCurrentUserId()
    {
        var claim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(claim) || !long.TryParse(claim, out var userId) || userId <= 0)
            return null;
        return userId;
    }

    private static string Serialize(object obj)
    {
        return JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
    }

    private static string SerializePascal(object? obj)
    {
        return JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
    }

    private string GetBaseUrl()
    {
        return _configuration["PublicBaseUrl"]
            ?? _configuration["BaseUrl"]
            ?? $"{Request.Scheme}://{Request.Host}";
    }

    private async Task NotifyConversationParticipants(
        long conversationId,
        long excludeUserId,
        string methodName,
        object payload,
        CancellationToken cancellationToken = default)
    {
        var participantIds = await _chat.GetConversationParticipantIdsAsync(conversationId, cancellationToken)
            .ConfigureAwait(false);

        foreach (var pid in participantIds)
        {
            if (pid != excludeUserId)
            {
                await _hubContext.Clients
                    .Group($"user_{pid}")
                    .SendAsync("notification", methodName,
                        JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = null }),
                        0, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }

    [Authorize]
    [HttpGet("v2/metadata")]
    [HttpGet("v1.0/metadata")]
    public async Task<IActionResult> GetMetadata(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var metadata = await _chat.GetMetadataAsync(cancellationToken).ConfigureAwait(false);
        return Content(Serialize(metadata), "application/json");
    }

    [Authorize]
    [HttpGet("v2/get-unread-conversation-count")]
    [HttpGet("v1.0/get-unread-conversation-count")]
    public async Task<IActionResult> GetUnreadConversationCount(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        try
        {
            var count = await _chat.GetUnreadConversationCountAsync(userId.Value, cancellationToken).ConfigureAwait(false);
            return Content(Serialize(new { count }), "application/json");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatController] GetUnreadConversationCount error: {ex.Message}");
            return Content(Serialize(new { count = 0 }), "application/json");
        }
    }

    [Authorize]
    [HttpGet("v2/get-user-conversations")]
    [HttpGet("v1.0/get-user-conversations")]
    public async Task<IActionResult> GetUserConversations(
        [FromQuery(Name = "pageNumber")] int pageNumber = 1,
        [FromQuery(Name = "pageSize")] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        try
        {
            var conversations = await _chat.GetUserConversationsAsync(userId.Value, pageNumber, pageSize, cancellationToken)
                .ConfigureAwait(false);

            var result = new List<object>();
            foreach (var c in conversations)
                result.Add(await FormatConversationResponse(c, userId.Value, cancellationToken).ConfigureAwait(false));
            return Content(Serialize(result), "application/json");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatController] GetUserConversations error: {ex.Message}");
            return Content(Serialize(new object[] { }), "application/json");
        }
    }

    [Authorize]
    [HttpGet("v2/get-conversations")]
    [HttpGet("v1.0/get-conversations")]
    public async Task<IActionResult> GetConversations(
        [FromQuery(Name = "conversationIds")] string? conversationIdsStr = null,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(conversationIdsStr))
            return Content(Serialize(new object[] { }), "application/json");

        try
        {
            var ids = conversationIdsStr.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => long.TryParse(s.Trim(), out var id) ? id : 0)
                .Where(id => id > 0)
                .ToArray();

            var conversations = await _chat.GetConversationsAsync(ids, cancellationToken).ConfigureAwait(false);
            var result = new List<object>();
            foreach (var c in conversations)
                result.Add(await FormatConversationResponse(c, userId.Value, cancellationToken).ConfigureAwait(false));
            return Content(Serialize(result), "application/json");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatController] GetConversations error: {ex.Message}");
            return Content(Serialize(new object[] { }), "application/json");
        }
    }

    [Authorize]
    [HttpGet("v2/get-messages")]
    [HttpGet("v1.0/get-messages")]
    public async Task<IActionResult> GetMessages(
        [FromQuery(Name = "conversationId")] long conversationId = 0,
        [FromQuery(Name = "exclusiveStartMessageId")] long exclusiveStartMessageId = 0,
        [FromQuery(Name = "pageSize")] int pageSize = 30,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        if (conversationId <= 0)
            return Content(Serialize(new object[] { }), "application/json");

        try
        {
            var isParticipant = await _chat.IsUserInConversationAsync(conversationId, userId.Value, cancellationToken)
                .ConfigureAwait(false);
            if (!isParticipant)
                return Forbid();

            var messages = await _chat.GetMessagesAsync(conversationId, exclusiveStartMessageId, pageSize, cancellationToken)
                .ConfigureAwait(false);

            var lastReadIds = await _chat.GetLastReadMessageIdsAsync(userId.Value, new[] { conversationId }, cancellationToken)
                .ConfigureAwait(false);
            var lastReadMessageId = lastReadIds.TryGetValue(conversationId, out var lrid) ? lrid : 0;

            var data = messages.Select(m => FormatMessageResponse(m, lastReadMessageId)).ToList();
            return Content(Serialize(data), "application/json");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatController] GetMessages error: {ex.Message}");
            return Content(Serialize(new object[] { }), "application/json");
        }
    }

    [Authorize]
    [HttpGet("v2/multi-get-latest-messages")]
    [HttpGet("v1.0/multi-get-latest-messages")]
    public async Task<IActionResult> MultiGetLatestMessages(
        [FromQuery(Name = "conversationIds")] string? conversationIdsStr = null,
        [FromQuery(Name = "pageSize")] int pageSize = 1,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(conversationIdsStr))
            return Content(Serialize(new object[] { }), "application/json");

        try
        {
            var ids = conversationIdsStr.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => long.TryParse(s.Trim(), out var id) ? id : 0)
                .Where(id => id > 0)
                .ToArray();

            var results = await _chat.MultiGetLatestMessagesAsync(ids, pageSize, cancellationToken)
                .ConfigureAwait(false);

            var lastReadIds = await _chat.GetLastReadMessageIdsAsync(userId.Value, ids, cancellationToken)
                .ConfigureAwait(false);

            var response = results.Select(r =>
            {
                var lrmid = lastReadIds.TryGetValue(r.ConversationId, out var lrid) ? lrid : 0;
                return new
                {
                    conversationId = r.ConversationId,
                    chatMessages = r.Messages.Select(m => FormatMessageResponse(m, lrmid)).ToList()
                };
            }).ToList();

            return Content(Serialize(response), "application/json");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatController] MultiGetLatestMessages error: {ex.Message}");
            return Content(Serialize(new object[] { }), "application/json");
        }
    }

    [Authorize]
    [HttpPost("v2/send-message")]
    [HttpPost("v1.0/send-message")]
    public async Task<IActionResult> SendMessage(
        [FromBody] SendMessageRequest? request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        if (request == null || request.ConversationId <= 0 || string.IsNullOrEmpty(request.Message))
            return BadRequest(new { });

        try
        {
            var isParticipant = await _chat.IsUserInConversationAsync(request.ConversationId, userId.Value, cancellationToken)
                .ConfigureAwait(false);
            if (!isParticipant)
                return Forbid();

            var message = await _chat.SendMessageAsync(
                request.ConversationId, userId.Value, request.Message, "PlainText", cancellationToken)
                .ConfigureAwait(false);

            var senderName = await _chat.GetUserNameAsync(userId.Value, cancellationToken).ConfigureAwait(false) ?? "";

            var messagePayload = FormatMessageResponse(message, message.Id);

            await NotifyConversationParticipants(
                request.ConversationId,
                userId.Value,
                "ChatNotifications",
                new { Type = "NewMessage", ConversationId = request.ConversationId },
                cancellationToken).ConfigureAwait(false);

            await _hubContext.Clients
                .Group($"user_{userId.Value}")
                .SendAsync("notification", "ChatNotifications",
                    JsonSerializer.Serialize(new { Type = "NewMessageBySelf", ConversationId = request.ConversationId },
                        new JsonSerializerOptions { PropertyNamingPolicy = null }),
                    0, cancellationToken)
                .ConfigureAwait(false);

            var sentStr = message.SentAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            return Content(Serialize(new { resultType = "Success", messageId = message.Id, sent = sentStr, messageType = message.MessageType, content = message.Content, pieces = new[] { new { type = 1, text = message.Content } }, filteredForReceivers = false }), "application/json");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatController] SendMessage error: {ex.Message}");
            return StatusCode(500, new { });
        }
    }

    [Authorize]
    [HttpPost("v2/mark-as-read")]
    [HttpPost("v1.0/mark-as-read")]
    public async Task<IActionResult> MarkAsRead(
        [FromBody] MarkAsReadRequest? request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        if (request == null || request.ConversationId <= 0)
            return Ok(new { });

        try
        {
            await _chat.MarkAsReadAsync(request.ConversationId, userId.Value, request.EndMessageId, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatController] MarkAsRead error: {ex.Message}");
        }

        return Ok(new { });
    }

    [Authorize]
    [HttpPost("v2/mark-as-seen")]
    [HttpPost("v1.0/mark-as-seen")]
    public async Task<IActionResult> MarkAsSeen(
        [FromBody] MarkAsSeenRequest? request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        if (request?.ConversationsToMarkSeen == null || request.ConversationsToMarkSeen.Length == 0)
            return Ok(new { });

        try
        {
            var convIds = request.ConversationsToMarkSeen.Select(x => x.ConversationId).ToArray();
            await _chat.MarkAsSeenAsync(userId.Value, convIds, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatController] MarkAsSeen error: {ex.Message}");
        }

        return Ok(new { });
    }

    [Authorize]
    [HttpPost("v2/start-one-to-one-conversation")]
    [HttpPost("v1.0/start-one-to-one-conversation")]
    public async Task<IActionResult> StartOneToOneConversation(
        [FromBody] StartOneToOneRequest? request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        if (request == null || request.ParticipantUserId <= 0)
            return BadRequest(new { });

        if (request.ParticipantUserId == userId.Value)
            return BadRequest(new { });

        try
        {
            var participantExists = await _chat.GetUserNameAsync(request.ParticipantUserId, cancellationToken)
                .ConfigureAwait(false);
            if (participantExists == null)
                return NotFound(new { });

            var conversation = await _chat.StartOneToOneConversationAsync(
                userId.Value, request.ParticipantUserId, cancellationToken).ConfigureAwait(false);

            var response = await FormatConversationResponse(conversation, userId.Value, cancellationToken).ConfigureAwait(false);

            await NotifyConversationParticipants(
                conversation.Id,
                userId.Value,
                "ChatNotifications",
                new { Type = "NewConversation", ConversationId = conversation.Id },
                cancellationToken).ConfigureAwait(false);

            return Content(Serialize(new { conversation = response }), "application/json");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatController] StartOneToOneConversation error: {ex.Message}");
            return StatusCode(500, new { });
        }
    }

    [Authorize]
    [HttpPost("v2/start-group-conversation")]
    [HttpPost("v1.0/start-group-conversation")]
    public async Task<IActionResult> StartGroupConversation(
        [FromBody] StartGroupRequest? request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        if (request?.ParticipantUserIds == null || request.ParticipantUserIds.Length == 0)
            return BadRequest(new { });

        try
        {
            var allParticipants = request.ParticipantUserIds
                .Where(id => id != userId.Value)
                .Distinct()
                .ToArray();

            var conversation = await _chat.StartGroupConversationAsync(
                userId.Value, allParticipants, request.Title, cancellationToken).ConfigureAwait(false);

            var response = await FormatConversationResponse(conversation, userId.Value, cancellationToken).ConfigureAwait(false);

            foreach (var pid in allParticipants)
            {
                await _hubContext.Clients
                    .Group($"user_{pid}")
                    .SendAsync("notification", "ChatNotifications",
                        JsonSerializer.Serialize(new { Type = "NewConversation", ConversationId = conversation.Id },
                            new JsonSerializerOptions { PropertyNamingPolicy = null }),
                        0, cancellationToken)
                    .ConfigureAwait(false);
            }

            return Content(Serialize(new { resultType = "Success", conversation = response }), "application/json");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatController] StartGroupConversation error: {ex.Message}");
            return StatusCode(500, new { });
        }
    }

    [Authorize]
    [HttpPost("v2/add-to-conversation")]
    [HttpPost("v1.0/add-to-conversation")]
    public async Task<IActionResult> AddToConversation(
        [FromBody] AddToConversationRequest? request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        if (request == null || request.ParticipantUserIds == null || request.ParticipantUserIds.Length == 0 || request.ConversationId <= 0)
            return BadRequest(new { });

        try
        {
            var isParticipant = await _chat.IsUserInConversationAsync(request.ConversationId, userId.Value, cancellationToken)
                .ConfigureAwait(false);
            if (!isParticipant)
                return Forbid();

            await _chat.AddToConversationAsync(request.ConversationId, request.ParticipantUserIds, cancellationToken)
                .ConfigureAwait(false);

            await NotifyConversationParticipants(
                request.ConversationId,
                userId.Value,
                "ChatNotifications",
                new { Type = "ParticipantAdded", ConversationId = request.ConversationId, ParticipantUserIds = request.ParticipantUserIds },
                cancellationToken).ConfigureAwait(false);

            foreach (var pid in request.ParticipantUserIds)
            {
                await _hubContext.Clients
                    .Group($"user_{pid}")
                    .SendAsync("notification", "ChatNotifications",
                        JsonSerializer.Serialize(new { Type = "AddedToConversation", ConversationId = request.ConversationId },
                            new JsonSerializerOptions { PropertyNamingPolicy = null }),
                        0, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatController] AddToConversation error: {ex.Message}");
        }

        return Content(Serialize(new { resultType = "Success", conversationId = request?.ConversationId }), "application/json");
    }

    [Authorize]
    [HttpPost("v2/remove-from-conversation")]
    [HttpPost("v1.0/remove-from-conversation")]
    public async Task<IActionResult> RemoveFromConversation(
        [FromBody] RemoveFromConversationRequest? request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        if (request == null || request.ConversationId <= 0)
            return BadRequest(new { });

        try
        {
            var isParticipant = await _chat.IsUserInConversationAsync(request.ConversationId, userId.Value, cancellationToken)
                .ConfigureAwait(false);
            if (!isParticipant)
                return Forbid();

            var targetUserId = request.ParticipantUserId > 0 ? request.ParticipantUserId : userId.Value;

            await _chat.RemoveFromConversationAsync(request.ConversationId, targetUserId, cancellationToken)
                .ConfigureAwait(false);

            await _hubContext.Clients
                .Group($"user_{targetUserId}")
                .SendAsync("notification", "ChatNotifications",
                    JsonSerializer.Serialize(new { Type = "RemovedFromConversation", ConversationId = request.ConversationId },
                        new JsonSerializerOptions { PropertyNamingPolicy = null }),
                    0, cancellationToken)
                .ConfigureAwait(false);

            await NotifyConversationParticipants(
                request.ConversationId,
                targetUserId,
                "ChatNotifications",
                new { Type = "ParticipantLeft", ConversationId = request.ConversationId, ParticipantUserId = targetUserId },
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatController] RemoveFromConversation error: {ex.Message}");
        }

        return Ok(new { });
    }

    [Authorize]
    [HttpPost("v2/rename-group-conversation")]
    [HttpPost("v1.0/rename-group-conversation")]
    public async Task<IActionResult> RenameGroupConversation(
        [FromBody] RenameGroupRequest? request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        if (request == null || request.ConversationId <= 0 || string.IsNullOrWhiteSpace(request.NewTitle))
            return BadRequest(new { });

        try
        {
            var success = await _chat.RenameGroupConversationAsync(
                request.ConversationId, userId.Value, request.NewTitle, cancellationToken).ConfigureAwait(false);

            if (!success)
                return Forbid();

            await NotifyConversationParticipants(
                request.ConversationId,
                userId.Value,
                "ChatNotifications",
                new { Type = "ConversationTitleChanged", ConversationId = request.ConversationId, ActorTargetId = userId.Value, Title = request.NewTitle },
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatController] RenameGroupConversation error: {ex.Message}");
        }

        return Content(Serialize(new { resultType = "Success", conversationTitle = request?.NewTitle ?? "" }), "application/json");
    }

    [Authorize]
    [HttpPost("v2/update-user-typing-status")]
    [HttpPost("v1.0/update-user-typing-status")]
    public async Task<IActionResult> UpdateUserTypingStatus(
        [FromBody] TypingStatusRequest? request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        if (request == null || request.ConversationId <= 0)
            return BadRequest(new { });

        _typingTracker.UpdateTypingStatus(request.ConversationId, userId.Value, request.IsTyping);

        if (request.IsTyping)
        {
            await NotifyConversationParticipants(
                request.ConversationId,
                userId.Value,
                "ChatNotifications",
                new { Type = "ParticipantTyping", ConversationId = request.ConversationId, UserId = userId.Value, IsTyping = true },
                cancellationToken).ConfigureAwait(false);
        }

        return Ok(new { });
    }

    [Authorize]
    [HttpPost("v2/set-conversation-universe")]
    [HttpPost("v1.0/set-conversation-universe")]
    public async Task<IActionResult> SetConversationUniverse(
        [FromBody] SetUniverseRequest? request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        if (request == null || request.ConversationId <= 0 || request.UniverseId <= 0)
            return BadRequest(new { });

        try
        {
            var isParticipant = await _chat.IsUserInConversationAsync(request.ConversationId, userId.Value, cancellationToken)
                .ConfigureAwait(false);
            if (!isParticipant)
                return Forbid();

            await _chat.SetConversationUniverseAsync(request.ConversationId, userId.Value, request.UniverseId, cancellationToken)
                .ConfigureAwait(false);

            await NotifyConversationParticipants(
                request.ConversationId,
                userId.Value,
                "ChatNotifications",
                new { Type = "ConversationUniverseChanged", ConversationId = request.ConversationId, ActorTargetId = userId.Value, UniverseId = request.UniverseId },
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatController] SetConversationUniverse error: {ex.Message}");
        }

        return Ok(new { });
    }

    [Authorize]
    [HttpPost("v2/reset-conversation-universe")]
    [HttpPost("v1.0/reset-conversation-universe")]
    public async Task<IActionResult> ResetConversationUniverse(
        [FromBody] ResetUniverseRequest? request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        if (request == null || request.ConversationId <= 0)
            return BadRequest(new { });

        try
        {
            var isParticipant = await _chat.IsUserInConversationAsync(request.ConversationId, userId.Value, cancellationToken)
                .ConfigureAwait(false);
            if (!isParticipant)
                return Forbid();

            await _chat.ResetConversationUniverseAsync(request.ConversationId, userId.Value, cancellationToken)
                .ConfigureAwait(false);

            await NotifyConversationParticipants(
                request.ConversationId,
                userId.Value,
                "ChatNotifications",
                new { Type = "ConversationUniverseChanged", ConversationId = request.ConversationId, ActorTargetId = userId.Value, UniverseId = (long?)null },
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatController] ResetConversationUniverse error: {ex.Message}");
        }

        return Ok(new { });
    }

    #region Unread Endpoints

    [Authorize]
    [HttpGet("v1.0/get-unread-conversations")]
    public async Task<IActionResult> GetUnreadConversations(
        [FromQuery(Name = "pageNumber")] int pageNumber = 1,
        [FromQuery(Name = "pageSize")] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        try
        {
            var conversations = await _chat.GetUnreadConversationsAsync(userId.Value, pageNumber, pageSize, cancellationToken)
                .ConfigureAwait(false);

            var result = new List<object>();
            foreach (var c in conversations)
                result.Add(await FormatConversationForClient(c, cancellationToken).ConfigureAwait(false));

            return Content(SerializePascal(result), "application/json");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatController] GetUnreadConversations error: {ex.Message}");
            return Content(SerializePascal(new object[] { }), "application/json");
        }
    }

    [Authorize]
    [HttpGet("v1.0/get-unread-messages")]
    public async Task<IActionResult> GetUnreadMessages(
        [FromQuery(Name = "pageNumber")] int pageNumber = 1,
        [FromQuery(Name = "pageSize")] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        try
        {
            var messages = await _chat.GetUnreadMessagesAsync(userId.Value, pageNumber, pageSize, cancellationToken)
                .ConfigureAwait(false);

            var result = messages.Select(m => new
            {
                Id = m.Id,
                ConversationId = m.ConversationId,
                SenderTargetId = m.SenderTargetId,
                SenderName = m.SenderName,
                MessageType = m.MessageType,
                Content = m.Content,
                Sent = m.SentAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            }).ToList();

            return Content(SerializePascal(result), "application/json");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatController] GetUnreadMessages error: {ex.Message}");
            return Content(SerializePascal(new object[] { }), "application/json");
        }
    }

    #endregion

    #region Party Endpoints

    [Authorize]
    [HttpPost("v1.0/party/create")]
    public async Task<IActionResult> PartyCreate(
        [FromBody] CreatePartyRequest? request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        if (request == null || request.ConversationId <= 0)
            return BadRequest(new { });

        try
        {
            var isParticipant = await _chat.IsUserInConversationAsync(request.ConversationId, userId.Value, cancellationToken)
                .ConfigureAwait(false);
            if (!isParticipant)
                return Forbid();

            var party = await _chat.CreatePartyAsync(
                request.ConversationId, userId.Value, request.InvitedUserIds ?? Array.Empty<long>(), cancellationToken)
                .ConfigureAwait(false);

            if (request.InvitedUserIds != null)
            {
                foreach (var pid in request.InvitedUserIds.Distinct())
                {
                    if (pid == userId.Value) continue;
                    await _hubContext.Clients
                        .Group($"user_{pid}")
                        .SendAsync("notification", "PartyNotifications",
                            JsonSerializer.Serialize(new { Type = "InvitedToParty", PartyId = party.Id },
                                new JsonSerializerOptions { PropertyNamingPolicy = null }),
                            0, cancellationToken)
                        .ConfigureAwait(false);
                }
            }

            return Content(SerializePascal(FormatPartyResponse(party)), "application/json");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatController] PartyCreate error: {ex.Message}");
            return StatusCode(500, new { });
        }
    }

    [Authorize]
    [HttpPost("v1.0/party/invite")]
    public async Task<IActionResult> PartyInvite(
        [FromBody] PartyInviteRequest? request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        if (request == null || request.PartyId <= 0 || request.InvitedUserId <= 0)
            return BadRequest(new { });

        try
        {
            var isLeader = await _chat.IsPartyLeaderAsync(request.PartyId, userId.Value, cancellationToken)
                .ConfigureAwait(false);
            if (!isLeader)
                return Forbid();

            await _chat.InviteToPartyAsync(request.PartyId, request.InvitedUserId, cancellationToken)
                .ConfigureAwait(false);

            await _hubContext.Clients
                .Group($"user_{request.InvitedUserId}")
                .SendAsync("notification", "PartyNotifications",
                    JsonSerializer.Serialize(new { Type = "InvitedToParty", PartyId = request.PartyId },
                        new JsonSerializerOptions { PropertyNamingPolicy = null }),
                    0, cancellationToken)
                .ConfigureAwait(false);

            return Ok(new { });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatController] PartyInvite error: {ex.Message}");
            return StatusCode(500, new { });
        }
    }

    [Authorize]
    [HttpPost("v1.0/party/leave")]
    public async Task<IActionResult> PartyLeave(
        [FromBody] PartyIdRequest? request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        if (request == null || request.PartyId <= 0)
            return BadRequest(new { });

        try
        {
            var isMember = await _chat.IsPartyMemberAsync(request.PartyId, userId.Value, cancellationToken)
                .ConfigureAwait(false);
            if (!isMember)
                return Forbid();

            await _chat.LeavePartyAsync(request.PartyId, userId.Value, cancellationToken).ConfigureAwait(false);

            await _hubContext.Clients
                .Group($"user_{userId.Value}")
                .SendAsync("notification", "PartyNotifications",
                    JsonSerializer.Serialize(new { Type = "ILeftParty", PartyId = request.PartyId },
                        new JsonSerializerOptions { PropertyNamingPolicy = null }),
                    0, cancellationToken)
                .ConfigureAwait(false);

            return Ok(new { });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatController] PartyLeave error: {ex.Message}");
            return StatusCode(500, new { });
        }
    }

    [Authorize]
    [HttpPost("v1.0/party/join")]
    public async Task<IActionResult> PartyJoin(
        [FromBody] PartyIdRequest? request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        if (request == null || request.PartyId <= 0)
            return BadRequest(new { });

        try
        {
            var isMember = await _chat.IsPartyMemberAsync(request.PartyId, userId.Value, cancellationToken)
                .ConfigureAwait(false);
            if (!isMember)
                return NotFound(new { });

            var joined = await _chat.JoinPartyAsync(request.PartyId, userId.Value, cancellationToken).ConfigureAwait(false);

            await _hubContext.Clients
                .Group($"user_{userId.Value}")
                .SendAsync("notification", "PartyNotifications",
                    JsonSerializer.Serialize(new { Type = "IJoinedParty", PartyId = request.PartyId },
                        new JsonSerializerOptions { PropertyNamingPolicy = null }),
                    0, cancellationToken)
                .ConfigureAwait(false);

            return Content(SerializePascal(new { Success = joined }), "application/json");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatController] PartyJoin error: {ex.Message}");
            return StatusCode(500, new { });
        }
    }

    [Authorize]
    [HttpGet("v1.0/party/get-invites")]
    public async Task<IActionResult> GetInvitedParties(
        [FromQuery(Name = "pageNumber")] int pageNumber = 1,
        [FromQuery(Name = "pageSize")] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        try
        {
            var parties = await _chat.GetInvitedPartiesAsync(userId.Value, pageNumber, pageSize, cancellationToken)
                .ConfigureAwait(false);
            var result = parties.Select(FormatPartyResponse).ToList();
            return Content(SerializePascal(result), "application/json");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatController] GetInvitedParties error: {ex.Message}");
            return Content(SerializePascal(new object[] { }), "application/json");
        }
    }

    [Authorize]
    [HttpGet("v1.0/party/get-current")]
    public async Task<IActionResult> GetCurrentParty(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        try
        {
            var party = await _chat.GetCurrentPartyAsync(userId.Value, cancellationToken).ConfigureAwait(false);
            return Content(SerializePascal(party != null ? FormatPartyResponse(party) : null), "application/json");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatController] GetCurrentParty error: {ex.Message}");
            return Content(SerializePascal(null), "application/json");
        }
    }

    [Authorize]
    [HttpPost("v1.0/party/remove-from-party")]
    public async Task<IActionResult> RemoveFromParty(
        [FromBody] RemoveFromPartyRequest? request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        if (request == null || request.PartyId <= 0 || request.UserId <= 0)
            return BadRequest(new { });

        try
        {
            var isLeader = await _chat.IsPartyLeaderAsync(request.PartyId, userId.Value, cancellationToken)
                .ConfigureAwait(false);
            if (!isLeader)
                return Forbid();

            await _chat.RemoveFromPartyAsync(request.PartyId, request.UserId, cancellationToken).ConfigureAwait(false);

            await _hubContext.Clients
                .Group($"user_{request.UserId}")
                .SendAsync("notification", "PartyNotifications",
                    JsonSerializer.Serialize(new { Type = "PartyUserLeft", PartyId = request.PartyId },
                        new JsonSerializerOptions { PropertyNamingPolicy = null }),
                    0, cancellationToken)
                .ConfigureAwait(false);

            return Ok(new { });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatController] RemoveFromParty error: {ex.Message}");
            return StatusCode(500, new { });
        }
    }

    [Authorize]
    [HttpGet("v1.0/party/get-parties-for-conversations")]
    public async Task<IActionResult> GetPartiesForConversations(
        [FromQuery(Name = "conversationIds")] string? conversationIdsStr = null,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(conversationIdsStr))
            return Content(SerializePascal(new object[] { }), "application/json");

        try
        {
            var ids = conversationIdsStr.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => long.TryParse(s.Trim(), out var id) ? id : 0)
                .Where(id => id > 0)
                .ToArray();

            var parties = await _chat.GetPartiesForConversationsAsync(ids, cancellationToken).ConfigureAwait(false);
            var result = parties.Select(FormatPartyResponse).ToList();
            return Content(SerializePascal(result), "application/json");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatController] GetPartiesForConversations error: {ex.Message}");
            return Content(SerializePascal(new object[] { }), "application/json");
        }
    }

    #endregion

    #region Response Formatters

    private async Task<object> FormatConversationResponse(ConversationResult conv, long currentUserId, CancellationToken cancellationToken = default)
    {
        var participantIds = conv.Participants.Select(p => p.UserId).Distinct().ToArray();
        var userNames = await _chat.GetMultipleUserNamesAsync(participantIds, cancellationToken).ConfigureAwait(false);

        var participants = conv.Participants.Select(p => new
        {
            targetId = p.UserId,
            userId = p.UserId,
            name = userNames.TryGetValue(p.UserId, out var uname) ? uname : "",
            role = p.Role
        }).ToList();

        var initiator = conv.Participants.FirstOrDefault();
        var initiatorName = initiator != null && userNames.TryGetValue(initiator.UserId, out var iname) ? iname : "";

        var title = conv.Title;
        if (string.IsNullOrWhiteSpace(title) && conv.ConversationType == "OneToOneConversation")
        {
            var otherParticipant = conv.Participants.FirstOrDefault(p => p.UserId != currentUserId);
            if (otherParticipant != null && userNames.TryGetValue(otherParticipant.UserId, out var otherName))
                title = otherName;
        }

        return new
        {
            id = conv.Id,
            title,
            InitiationSource = "Manual",
            participants,
            hasUnreadMessages = conv.HasUnreadMessages,
            type = conv.ConversationType,
            conversationType = conv.ConversationType,
            conversationUniverse = conv.UniverseId.HasValue ? new { universeId = conv.UniverseId.Value } : null,
            conversationTitle = title,
            initiator = initiator != null ? new { targetId = initiator.UserId, userId = initiator.UserId, name = initiatorName } : null,
            convId = conv.Id.ToString(),
            creatorUserId = conv.CreatorUserId,
            created = conv.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            lastUpdated = conv.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            conversationScreenUrl = ""
        };
    }

    private object FormatMessageResponse(ChatMessageResult msg, long lastReadMessageId = 0)
    {
        var eventBased = msg.EventType != null ? new
            {
                type = msg.EventType,
                rawContent = msg.Content
            } : new { type = "", rawContent = "" };

        return new
        {
            id = msg.Id,
            senderType = "User",
            sent = msg.SentAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            read = msg.Id <= lastReadMessageId,
            messageType = msg.MessageType,
            msg = msg.Content,
            senderTargetId = msg.SenderTargetId,
            content = msg.Content,
            eventBased,
            filteredForReceivers = false,
            isSystemMessage = false,
            messageSent = true,
            messageBlocked = false,
            senderUsername = msg.SenderName
        };
    }

    private async Task<object> FormatConversationForClient(ConversationResult conv, CancellationToken cancellationToken = default)
    {
        var participantIds = conv.Participants.Select(p => p.UserId).Distinct().ToArray();
        var userNames = await _chat.GetMultipleUserNamesAsync(participantIds, cancellationToken).ConfigureAwait(false);

        var participants = conv.Participants.Select(p => new
        {
            Id = p.UserId,
            Username = userNames.TryGetValue(p.UserId, out var uname) ? uname : "",
            UserPresenceType = 0
        }).ToList();

        return new
        {
            Id = conv.Id,
            Title = conv.Title,
            ConversationType = conv.ConversationType,
            ParticipantUsers = participants,
            IsGroupChat = conv.ConversationType == "MultiUserConversation",
            HasUnreadMessages = conv.HasUnreadMessages,
            ChatMessages = new object[] { }
        };
    }

    private object FormatPartyResponse(PartyResult party)
    {
        return new
        {
            Id = party.Id,
            ConversationId = party.ConversationId,
            LeaderUser = new { Id = party.LeaderUserId, Name = party.LeaderName },
            MemberUsers = party.Members.Select(m => new
            {
                Id = m.UserId,
                Name = m.Name,
                MemberStatus = m.MemberStatus
            }).ToList(),
            GameId = party.GameId,
            GamePlaceId = party.GamePlaceId
        };
    }

    #endregion

    #region Request DTOs

    public class SendMessageRequest
    {
        [JsonPropertyName("conversationId")]
        public long ConversationId { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }

    public class MarkAsReadRequest
    {
        [JsonPropertyName("conversationId")]
        public long ConversationId { get; set; }

        [JsonPropertyName("endMessageId")]
        public long EndMessageId { get; set; }
    }

    public class MarkAsSeenRequest
    {
        [JsonPropertyName("conversationsToMarkSeen")]
        public ConversationSeenEntry[]? ConversationsToMarkSeen { get; set; }
    }

    public class ConversationSeenEntry
    {
        [JsonPropertyName("conversationId")]
        public long ConversationId { get; set; }
    }

    public class StartOneToOneRequest
    {
        [JsonPropertyName("participantUserId")]
        public long ParticipantUserId { get; set; }
    }

    public class StartGroupRequest
    {
        [JsonPropertyName("participantUserIds")]
        public long[]? ParticipantUserIds { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }
    }

    public class AddToConversationRequest
    {
        [JsonPropertyName("participantUserIds")]
        public long[]? ParticipantUserIds { get; set; }

        [JsonPropertyName("conversationId")]
        public long ConversationId { get; set; }
    }

    public class RemoveFromConversationRequest
    {
        [JsonPropertyName("participantUserId")]
        public long ParticipantUserId { get; set; }

        [JsonPropertyName("conversationId")]
        public long ConversationId { get; set; }
    }

    public class RenameGroupRequest
    {
        [JsonPropertyName("conversationId")]
        public long ConversationId { get; set; }

        [JsonPropertyName("newTitle")]
        public string? NewTitle { get; set; }
    }

    public class TypingStatusRequest
    {
        [JsonPropertyName("conversationId")]
        public long ConversationId { get; set; }

        [JsonPropertyName("isTyping")]
        public bool IsTyping { get; set; }
    }

    public class SetUniverseRequest
    {
        [JsonPropertyName("conversationId")]
        public long ConversationId { get; set; }

        [JsonPropertyName("universeId")]
        public long UniverseId { get; set; }
    }

    public class ResetUniverseRequest
    {
        [JsonPropertyName("conversationId")]
        public long ConversationId { get; set; }
    }

    public class CreatePartyRequest
    {
        [JsonPropertyName("conversationId")]
        public long ConversationId { get; set; }

        [JsonPropertyName("invitedUserIds")]
        public long[]? InvitedUserIds { get; set; }
    }

    public class PartyInviteRequest
    {
        [JsonPropertyName("partyId")]
        public long PartyId { get; set; }

        [JsonPropertyName("invitedUserId")]
        public long InvitedUserId { get; set; }
    }

    public class PartyIdRequest
    {
        [JsonPropertyName("partyId")]
        public long PartyId { get; set; }
    }

    public class RemoveFromPartyRequest
    {
        [JsonPropertyName("partyId")]
        public long PartyId { get; set; }

        [JsonPropertyName("userId")]
        public long UserId { get; set; }
    }

    #endregion
}
