using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Chat;

public class ChatRepository
{
    private readonly string _connectionString;

    public ChatRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<ChatMetadata> GetMetadataAsync(CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(new ChatMetadata
        {
            IsChatEnabledByPrivacySetting = 1,
            ChatGroupUpSellEnabled = false,
            ChatButtonFriendDiscoveryEnabled = false,
            ChatDisabledByPrivacySettingType = 0,
            EnableRecipientsList = true,
            MaxGroupParticipants = 25,
            MaxConversationTitleLength = 100,
            ConversationTitleMinLength = 1,
            ChatInputMaxLength = 1000,
            EnableSignalRTransportRestriction = false,
            TypingIndicatorDelayInMilliseconds = 3000,
            ReconnectWaitTimeInMilliseconds = 5000,
            MaxConversationSnippetTitleLength = 48,
            LanguageForPrivacySettingUnavailable = "Chat is unavailable.",
            PartyChromeDisplayTimeStampInterval = 30000,
            NumberOfMembersForPartyChrome = 25,
            SignalRDisconnectionResponseInMilliseconds = 5000,
            TypingInChatFromSenderThrottleMs = 5000,
            TypingInChatForReceiverExpirationMs = 10000,
            SenderTypesForUnknownMessageTypeError = new List<string> { "Unknown" },
            RelativeValueToRecordUiPerformance = 1.0,
            IsUsingCacheToLoadFriendsInfoEnabled = false,
            CachedDataFromLocalStorageExpirationMS = 600000,
            IsInvalidMessageTypeFallbackEnabled = true,
            IsRespectingMessageTypeEnabled = true,
            ValidMessageTypesWhiteList = new List<string> { "PlainText", "Link", "EventBased" },
            ShouldRespectConversationHasUnreadMessageToMarkAsRead = true
        });
    }

    public async Task<int> GetUnreadConversationCountAsync(long userId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT COUNT(DISTINCT c.id)
            FROM conversations c
            INNER JOIN conversation_participants cp ON cp.conversation_id = c.id AND cp.user_id = @userId
            INNER JOIN chat_messages cm ON cm.conversation_id = c.id
              AND cm.id > cp.last_read_message_id
              AND cm.sender_id != @userId
              AND cm.is_deleted = FALSE
            WHERE c.is_deleted = FALSE";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@userId", userId);
        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return Convert.ToInt32(result);
    }

    public async Task<List<ConversationResult>> GetUserConversationsAsync(long userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT c.id, c.title, c.conversation_type, c.creator_user_id, c.universe_id, c.created_at,
                   cp.last_read_message_id,
                   (SELECT json_agg(json_build_object(
                       'userId', cp2.user_id,
                       'role', cp2.role
                   ))
                   FROM conversation_participants cp2
                   WHERE cp2.conversation_id = c.id) AS participants_json,
                   EXISTS(
                       SELECT 1 FROM chat_messages cm
                       WHERE cm.conversation_id = c.id
                         AND cm.id > cp.last_read_message_id
                         AND cm.sender_id != @userId
                         AND cm.is_deleted = FALSE
                   ) AS has_unread
            FROM conversations c
            INNER JOIN conversation_participants cp ON cp.conversation_id = c.id AND cp.user_id = @userId
            WHERE c.is_deleted = FALSE
            ORDER BY c.created_at DESC
            LIMIT @pageSize OFFSET @offset";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@userId", userId);
        cmd.Parameters.AddWithValue("@pageSize", pageSize);
        cmd.Parameters.AddWithValue("@offset", (pageNumber - 1) * pageSize);

        var results = new List<ConversationResult>();
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var participantsJson = reader.IsDBNull(7) ? "[]" : reader.GetString(7);
            var participants = JsonSerializer.Deserialize<List<ParticipantDto>>(participantsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

            results.Add(new ConversationResult
            {
                Id = reader.GetInt64(0),
                Title = reader.IsDBNull(1) ? "" : reader.GetString(1),
                ConversationType = reader.GetString(2),
                CreatorUserId = reader.GetInt64(3),
                UniverseId = reader.IsDBNull(4) ? null : reader.GetInt64(4),
                CreatedAt = reader.GetDateTime(5),
                LastReadMessageId = reader.IsDBNull(6) ? 0 : reader.GetInt64(6),
                Participants = participants,
                HasUnreadMessages = !reader.IsDBNull(8) && reader.GetBoolean(8)
            });
        }

        return results;
    }

    public async Task<List<ConversationResult>> GetConversationsAsync(long[] conversationIds, CancellationToken cancellationToken = default)
    {
        if (conversationIds.Length == 0) return new List<ConversationResult>();

        var parameters = new List<string>();
        for (int i = 0; i < conversationIds.Length; i++)
            parameters.Add($"@id{i}");

        var sql = $@"
            SELECT c.id, c.title, c.conversation_type, c.creator_user_id, c.universe_id, c.created_at,
                   0,
                   (SELECT json_agg(json_build_object(
                       'userId', cp2.user_id,
                       'role', cp2.role
                   ))
                   FROM conversation_participants cp2
                   WHERE cp2.conversation_id = c.id) AS participants_json
            FROM conversations c
            WHERE c.id IN ({string.Join(",", parameters)}) AND c.is_deleted = FALSE";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var cmd = new NpgsqlCommand(sql, conn);
        for (int i = 0; i < conversationIds.Length; i++)
            cmd.Parameters.AddWithValue($"@id{i}", conversationIds[i]);

        var results = new List<ConversationResult>();
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var participantsJson = reader.IsDBNull(7) ? "[]" : reader.GetString(7);
            var participants = JsonSerializer.Deserialize<List<ParticipantDto>>(participantsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

            results.Add(new ConversationResult
            {
                Id = reader.GetInt64(0),
                Title = reader.IsDBNull(1) ? "" : reader.GetString(1),
                ConversationType = reader.GetString(2),
                CreatorUserId = reader.GetInt64(3),
                UniverseId = reader.IsDBNull(4) ? null : reader.GetInt64(4),
                CreatedAt = reader.GetDateTime(5),
                LastReadMessageId = reader.IsDBNull(6) ? 0 : reader.GetInt64(6),
                Participants = participants
            });
        }

        return results;
    }

    public async Task<List<ChatMessageResult>> GetMessagesAsync(long conversationId, long exclusiveStartMessageId, int pageSize, CancellationToken cancellationToken = default)
    {
        string whereClause = exclusiveStartMessageId > 0
            ? "WHERE cm.conversation_id = @conversationId AND cm.id < @exclusiveStartMessageId AND cm.is_deleted = FALSE"
            : "WHERE cm.conversation_id = @conversationId AND cm.is_deleted = FALSE";

        var sql = $@"
            SELECT cm.id, cm.conversation_id, cm.sender_id, cm.message_type, cm.content,
                   cm.event_type, cm.event_metadata, cm.sent_at,
                   u.user_name
            FROM chat_messages cm
            LEFT JOIN users u ON u.user_id = cm.sender_id
            {whereClause}
            ORDER BY cm.id DESC
            LIMIT @pageSize";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@conversationId", conversationId);
        cmd.Parameters.AddWithValue("@pageSize", pageSize);
        if (exclusiveStartMessageId > 0)
            cmd.Parameters.AddWithValue("@exclusiveStartMessageId", exclusiveStartMessageId);

        var messages = new List<ChatMessageResult>();
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            messages.Add(new ChatMessageResult
            {
                Id = reader.GetInt64(0),
                ConversationId = reader.GetInt64(1),
                SenderTargetId = reader.GetInt64(2),
                MessageType = reader.GetString(3),
                Content = reader.IsDBNull(4) ? "" : reader.GetString(4),
                EventType = reader.IsDBNull(5) ? null : reader.GetString(5),
                EventMetadata = reader.IsDBNull(6) ? null : reader.GetString(6),
                SentAt = reader.GetDateTime(7),
                SenderName = reader.IsDBNull(8) ? "" : reader.GetString(8)
            });
        }

        return messages;
    }

    public async Task<List<LatestMessagesResult>> MultiGetLatestMessagesAsync(long[] conversationIds, int pageSize, CancellationToken cancellationToken = default)
    {
        if (conversationIds.Length == 0) return new List<LatestMessagesResult>();

        var results = new List<LatestMessagesResult>();
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        foreach (var convId in conversationIds)
        {
            const string sql = @"
                SELECT cm.id, cm.conversation_id, cm.sender_id, cm.message_type, cm.content,
                       cm.event_type, cm.event_metadata, cm.sent_at, u.user_name
                FROM chat_messages cm
                LEFT JOIN users u ON u.user_id = cm.sender_id
                WHERE cm.conversation_id = @conversationId AND cm.is_deleted = FALSE
                ORDER BY cm.id DESC
                LIMIT @pageSize";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@conversationId", convId);
            cmd.Parameters.AddWithValue("@pageSize", pageSize);

            var messages = new List<ChatMessageResult>();
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                messages.Add(new ChatMessageResult
                {
                    Id = reader.GetInt64(0),
                    ConversationId = reader.GetInt64(1),
                SenderTargetId = reader.GetInt64(2),
                    MessageType = reader.GetString(3),
                    Content = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    EventType = reader.IsDBNull(5) ? null : reader.GetString(5),
                    EventMetadata = reader.IsDBNull(6) ? null : reader.GetString(6),
                    SentAt = reader.GetDateTime(7),
                    SenderName = reader.IsDBNull(8) ? "" : reader.GetString(8)
                });
            }

            messages.Reverse();
            results.Add(new LatestMessagesResult
            {
                ConversationId = convId,
                Messages = messages
            });
        }

        return results;
    }

    public async Task<ChatMessageResult> SendMessageAsync(long conversationId, long senderId, string content, string messageType = "PlainText", CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO chat_messages (conversation_id, sender_id, message_type, content)
            VALUES (@conversationId, @senderId, @messageType, @content)
            RETURNING id, sent_at";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@conversationId", conversationId);
        cmd.Parameters.AddWithValue("@senderId", senderId);
        cmd.Parameters.AddWithValue("@messageType", messageType);
        cmd.Parameters.AddWithValue("@content", content);

        using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return new ChatMessageResult
            {
                Id = reader.GetInt64(0),
                ConversationId = conversationId,
                SenderTargetId = senderId,
                MessageType = messageType,
                Content = content,
                SentAt = reader.GetDateTime(1)
            };
        }

        throw new InvalidOperationException("Failed to send message");
    }

    public async Task<long> SendEventMessageAsync(long conversationId, long senderId, string eventType, string metadataJson, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO chat_messages (conversation_id, sender_id, message_type, content, event_type, event_metadata)
            VALUES (@conversationId, @senderId, 'EventBased', '', @eventType, @metadata::jsonb)
            RETURNING id";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@conversationId", conversationId);
        cmd.Parameters.AddWithValue("@senderId", senderId);
        cmd.Parameters.AddWithValue("@eventType", eventType);
        cmd.Parameters.AddWithValue("@metadata", metadataJson);

        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return Convert.ToInt64(result);
    }

    public async Task<Dictionary<long, long>> GetLastReadMessageIdsAsync(long userId, long[] conversationIds, CancellationToken cancellationToken = default)
    {
        if (conversationIds.Length == 0) return new Dictionary<long, long>();

        var parameters = new List<string>();
        for (int i = 0; i < conversationIds.Length; i++)
            parameters.Add($"@cid{i}");

        var sql = $@"
            SELECT conversation_id, last_read_message_id
            FROM conversation_participants
            WHERE user_id = @userId AND conversation_id IN ({string.Join(",", parameters)})";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@userId", userId);
        for (int i = 0; i < conversationIds.Length; i++)
            cmd.Parameters.AddWithValue($"@cid{i}", conversationIds[i]);

        var result = new Dictionary<long, long>();
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            result[reader.GetInt64(0)] = reader.IsDBNull(1) ? 0 : reader.GetInt64(1);

        return result;
    }

    public async Task MarkAsReadAsync(long conversationId, long userId, long endMessageId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE conversation_participants
            SET last_read_message_id = GREATEST(last_read_message_id, @endMessageId)
            WHERE conversation_id = @conversationId AND user_id = @userId
              AND last_read_message_id < @endMessageId";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@conversationId", conversationId);
        cmd.Parameters.AddWithValue("@userId", userId);
        cmd.Parameters.AddWithValue("@endMessageId", endMessageId);
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task MarkAsSeenAsync(long userId, long[] conversationIds, CancellationToken cancellationToken = default)
    {
        if (conversationIds.Length == 0) return;

        var parameters = new List<string>();
        for (int i = 0; i < conversationIds.Length; i++)
            parameters.Add($"@convId{i}");

        var sql = $@"
            UPDATE conversation_participants
            SET last_seen_at = NOW()
            WHERE user_id = @userId AND conversation_id IN ({string.Join(",", parameters)})";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@userId", userId);
        for (int i = 0; i < conversationIds.Length; i++)
            cmd.Parameters.AddWithValue($"@convId{i}", conversationIds[i]);

        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<ConversationResult?> GetConversationAsync(long conversationId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT c.id, c.title, c.conversation_type, c.creator_user_id, c.universe_id, c.created_at,
                   (SELECT json_agg(json_build_object(
                       'userId', cp.user_id,
                       'role', cp.role
                   ))
                   FROM conversation_participants cp
                   WHERE cp.conversation_id = c.id) AS participants_json
            FROM conversations c
            WHERE c.id = @conversationId AND c.is_deleted = FALSE";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@conversationId", conversationId);

        using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var participantsJson = reader.IsDBNull(6) ? "[]" : reader.GetString(6);
            var participants = JsonSerializer.Deserialize<List<ParticipantDto>>(participantsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

            return new ConversationResult
            {
                Id = reader.GetInt64(0),
                Title = reader.IsDBNull(1) ? "" : reader.GetString(1),
                ConversationType = reader.GetString(2),
                CreatorUserId = reader.GetInt64(3),
                UniverseId = reader.IsDBNull(4) ? null : reader.GetInt64(4),
                CreatedAt = reader.GetDateTime(5),
                Participants = participants
            };
        }

        return null;
    }

    public async Task<bool> IsUserInConversationAsync(long conversationId, long userId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 1 FROM conversation_participants
            WHERE conversation_id = @conversationId AND user_id = @userId";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@conversationId", conversationId);
        cmd.Parameters.AddWithValue("@userId", userId);

        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return result != null;
    }

    public async Task<List<long>> GetConversationParticipantIdsAsync(long conversationId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT user_id FROM conversation_participants
            WHERE conversation_id = @conversationId
            ORDER BY user_id";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@conversationId", conversationId);

        var userIds = new List<long>();
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            userIds.Add(reader.GetInt64(0));
        }

        return userIds;
    }

    public async Task<ConversationResult> StartOneToOneConversationAsync(long userId, long participantUserId, CancellationToken cancellationToken = default)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        // Check for existing 1:1 conversation
        const string checkSql = @"
            SELECT cp1.conversation_id
            FROM conversation_participants cp1
            INNER JOIN conversation_participants cp2 ON cp2.conversation_id = cp1.conversation_id
            INNER JOIN conversations c ON c.id = cp1.conversation_id
            WHERE cp1.user_id = @userId AND cp2.user_id = @participantUserId
              AND c.conversation_type = 'OneToOneConversation' AND c.is_deleted = FALSE
            LIMIT 1";

        using (var checkCmd = new NpgsqlCommand(checkSql, conn))
        {
            checkCmd.Parameters.AddWithValue("@userId", userId);
            checkCmd.Parameters.AddWithValue("@participantUserId", participantUserId);
            var existingId = await checkCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            if (existingId != null)
            {
                return (await GetConversationAsync(Convert.ToInt64(existingId), cancellationToken).ConfigureAwait(false))!;
            }
        }

        // Create new conversation
        const string createSql = @"
            INSERT INTO conversations (conversation_type, creator_user_id)
            VALUES ('OneToOneConversation', @userId)
            RETURNING id, created_at";

        long convId;
        DateTime createdAt;
        using (var createCmd = new NpgsqlCommand(createSql, conn))
        {
            createCmd.Parameters.AddWithValue("@userId", userId);
            using var r = await createCmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            await r.ReadAsync(cancellationToken).ConfigureAwait(false);
            convId = r.GetInt64(0);
            createdAt = r.GetDateTime(1);
        }

        // Add both participants
        const string addParticipantSql = "INSERT INTO conversation_participants (conversation_id, user_id) VALUES (@convId, @userId)";

        using (var p1 = new NpgsqlCommand(addParticipantSql, conn))
        {
            p1.Parameters.AddWithValue("@convId", convId);
            p1.Parameters.AddWithValue("@userId", userId);
            await p1.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        using (var p2 = new NpgsqlCommand(addParticipantSql, conn))
        {
            p2.Parameters.AddWithValue("@convId", convId);
            p2.Parameters.AddWithValue("@userId", participantUserId);
            await p2.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        return new ConversationResult
        {
            Id = convId,
            ConversationType = "OneToOneConversation",
            CreatorUserId = userId,
            CreatedAt = createdAt,
            Participants = new List<ParticipantDto>
            {
                new() { UserId = userId, Role = "Member" },
                new() { UserId = participantUserId, Role = "Member" }
            }
        };
    }

    public async Task<ConversationResult> StartGroupConversationAsync(long userId, long[] participantUserIds, string? title, CancellationToken cancellationToken = default)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string createSql = @"
            INSERT INTO conversations (title, conversation_type, creator_user_id)
            VALUES (@title, 'MultiUserConversation', @userId)
            RETURNING id, created_at";

        long convId;
        DateTime createdAt;
        using (var createCmd = new NpgsqlCommand(createSql, conn))
        {
            createCmd.Parameters.AddWithValue("@title", (object?)title ?? DBNull.Value);
            createCmd.Parameters.AddWithValue("@userId", userId);
            using var r = await createCmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            await r.ReadAsync(cancellationToken).ConfigureAwait(false);
            convId = r.GetInt64(0);
            createdAt = r.GetDateTime(1);
        }

        // Add creator
        const string addSql = "INSERT INTO conversation_participants (conversation_id, user_id) VALUES (@convId, @userId)";
        using (var p = new NpgsqlCommand(addSql, conn))
        {
            p.Parameters.AddWithValue("@convId", convId);
            p.Parameters.AddWithValue("@userId", userId);
            await p.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        // Add other participants
        foreach (var pid in participantUserIds)
        {
            if (pid == userId) continue;
            using (var p = new NpgsqlCommand(addSql, conn))
            {
                p.Parameters.AddWithValue("@convId", convId);
                p.Parameters.AddWithValue("@userId", pid);
                await p.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        var participants = new List<ParticipantDto> { new() { UserId = userId, Role = "Member" } };
        foreach (var pid in participantUserIds)
        {
            if (pid != userId)
                participants.Add(new ParticipantDto { UserId = pid, Role = "Member" });
        }

        return new ConversationResult
        {
            Id = convId,
            Title = title ?? "",
            ConversationType = "MultiUserConversation",
            CreatorUserId = userId,
            CreatedAt = createdAt,
            Participants = participants
        };
    }

    public async Task<bool> AddToConversationAsync(long conversationId, long[] participantUserIds, CancellationToken cancellationToken = default)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"
            INSERT INTO conversation_participants (conversation_id, user_id)
            VALUES (@convId, @userId)
            ON CONFLICT (conversation_id, user_id) DO NOTHING";

        foreach (var userId in participantUserIds)
        {
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@convId", conversationId);
            cmd.Parameters.AddWithValue("@userId", userId);
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        return true;
    }

    public async Task<bool> RemoveFromConversationAsync(long conversationId, long userId, CancellationToken cancellationToken = default)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string removeSql = "DELETE FROM conversation_participants WHERE conversation_id = @convId AND user_id = @userId";
        using (var cmd = new NpgsqlCommand(removeSql, conn))
        {
            cmd.Parameters.AddWithValue("@convId", conversationId);
            cmd.Parameters.AddWithValue("@userId", userId);
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        // Check remaining participants
        const string countSql = "SELECT COUNT(*) FROM conversation_participants WHERE conversation_id = @convId";
        using (var countCmd = new NpgsqlCommand(countSql, conn))
        {
            countCmd.Parameters.AddWithValue("@convId", conversationId);
            var count = Convert.ToInt64(await countCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false));
            if (count <= 0)
            {
                const string deleteSql = "UPDATE conversations SET is_deleted = TRUE WHERE id = @convId";
                using var deleteCmd = new NpgsqlCommand(deleteSql, conn);
                deleteCmd.Parameters.AddWithValue("@convId", conversationId);
                await deleteCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        return true;
    }

    public async Task<bool> RenameGroupConversationAsync(long conversationId, long userId, string newTitle, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE conversations SET title = @newTitle
            WHERE id = @conversationId AND creator_user_id = @userId AND is_deleted = FALSE
              AND conversation_type = 'MultiUserConversation'";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@newTitle", newTitle);
        cmd.Parameters.AddWithValue("@conversationId", conversationId);
        cmd.Parameters.AddWithValue("@userId", userId);

        var rowsAffected = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        return rowsAffected > 0;
    }

    public async Task<bool> SetConversationUniverseAsync(long conversationId, long userId, long universeId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE conversations SET universe_id = @universeId
            WHERE id = @conversationId AND is_deleted = FALSE";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@universeId", universeId);
        cmd.Parameters.AddWithValue("@conversationId", conversationId);

        var rows = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        if (rows > 0)
        {
            var metadata = JsonSerializer.Serialize(new
            {
                setConversationUniverse = new { actorUserId = userId, universeId }
            });
            await SendEventMessageAsync(conversationId, userId, "SetConversationUniverse", metadata, cancellationToken).ConfigureAwait(false);
        }

        return rows > 0;
    }

    public async Task<bool> ResetConversationUniverseAsync(long conversationId, long userId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE conversations SET universe_id = NULL
            WHERE id = @conversationId AND is_deleted = FALSE";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@conversationId", conversationId);

        var rows = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        if (rows > 0)
        {
            var metadata = JsonSerializer.Serialize(new
            {
                resetConversationUniverse = new { actorUserId = userId }
            });
            await SendEventMessageAsync(conversationId, userId, "ResetConversationUniverse", metadata, cancellationToken).ConfigureAwait(false);
        }

        return rows > 0;
    }

    public async Task<string?> GetUserNameAsync(long userId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT user_name FROM users WHERE user_id = @userId";
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@userId", userId);
        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return result?.ToString();
    }

    public async Task<Dictionary<long, string>> GetMultipleUserNamesAsync(long[] userIds, CancellationToken cancellationToken = default)
    {
        if (userIds.Length == 0) return new Dictionary<long, string>();

        var parameters = new List<string>();
        for (int i = 0; i < userIds.Length; i++)
            parameters.Add($"@uid{i}");

        var sql = $@"SELECT user_id, user_name FROM users WHERE user_id IN ({string.Join(",", parameters)})";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var cmd = new NpgsqlCommand(sql, conn);
        for (int i = 0; i < userIds.Length; i++)
            cmd.Parameters.AddWithValue($"@uid{i}", userIds[i]);

        var names = new Dictionary<long, string>();
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            names[reader.GetInt64(0)] = reader.GetString(1);
        }

        return names;
    }

    public async Task<long> GetMaxMessageIdAsync(long conversationId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT COALESCE(MAX(id), 0) FROM chat_messages
            WHERE conversation_id = @conversationId AND is_deleted = FALSE";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@conversationId", conversationId);
        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return Convert.ToInt64(result);
    }
}

public class ChatMetadata
{
    public int IsChatEnabledByPrivacySetting { get; set; }
    public bool ChatGroupUpSellEnabled { get; set; }
    public bool ChatButtonFriendDiscoveryEnabled { get; set; }
    public int ChatDisabledByPrivacySettingType { get; set; }
    public bool EnableRecipientsList { get; set; }
    public int MaxGroupParticipants { get; set; }
    public int MaxConversationTitleLength { get; set; }
    public int ConversationTitleMinLength { get; set; }
    public int ChatInputMaxLength { get; set; }
    public bool EnableSignalRTransportRestriction { get; set; }
    public int TypingIndicatorDelayInMilliseconds { get; set; }
    public int ReconnectWaitTimeInMilliseconds { get; set; }
    public int MaxConversationSnippetTitleLength { get; set; }
    public string LanguageForPrivacySettingUnavailable { get; set; } = "Chat is unavailable.";
    public int PartyChromeDisplayTimeStampInterval { get; set; } = 30000;
    public int NumberOfMembersForPartyChrome { get; set; } = 25;
    public int SignalRDisconnectionResponseInMilliseconds { get; set; } = 5000;
    public int TypingInChatFromSenderThrottleMs { get; set; } = 5000;
    public int TypingInChatForReceiverExpirationMs { get; set; } = 10000;
    public List<string> SenderTypesForUnknownMessageTypeError { get; set; } = new() { "Unknown" };
    public double RelativeValueToRecordUiPerformance { get; set; } = 1.0;
    public bool IsUsingCacheToLoadFriendsInfoEnabled { get; set; } = true;
    public long CachedDataFromLocalStorageExpirationMS { get; set; } = 600000;
    public bool IsInvalidMessageTypeFallbackEnabled { get; set; } = true;
    public bool IsRespectingMessageTypeEnabled { get; set; } = true;
    public List<string> ValidMessageTypesWhiteList { get; set; } = new() { "PlainText", "Link", "EventBased" };
    public bool ShouldRespectConversationHasUnreadMessageToMarkAsRead { get; set; } = true;
}

public class ConversationResult
{
    public long Id { get; set; }
    public string Title { get; set; } = "";
    public string ConversationType { get; set; } = "";
    public long CreatorUserId { get; set; }
    public long? UniverseId { get; set; }
    public DateTime CreatedAt { get; set; }
    public long LastReadMessageId { get; set; }
    public bool HasUnreadMessages { get; set; }
    public List<ParticipantDto> Participants { get; set; } = new();
}

public class ParticipantDto
{
    public long UserId { get; set; }
    public string Role { get; set; } = "Member";
}

public class ChatMessageResult
{
    public long Id { get; set; }
    public long ConversationId { get; set; }
    public long SenderTargetId { get; set; }
    public string SenderType { get; set; } = "User";
    public string MessageType { get; set; } = "PlainText";
    public string Content { get; set; } = "";
    public string? EventType { get; set; }
    public string? EventMetadata { get; set; }
    public DateTime SentAt { get; set; }
    public string SenderName { get; set; } = "";
}

public class LatestMessagesResult
{
    public long ConversationId { get; set; }
    public List<ChatMessageResult> Messages { get; set; } = new();
}
