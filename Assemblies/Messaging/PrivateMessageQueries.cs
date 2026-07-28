using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Messaging;

public class PrivateMessageQueries
{
    private readonly string _connectionString;

    public PrivateMessageQueries(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<PrivateMessageResult> GetMessagesAsync(long userId, int messageTab, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        string whereClause = messageTab switch
        {
            0 => "pm.recipient_id = @userId AND pm.is_archived = FALSE",
            1 => "pm.sender_id = @userId",
            3 => "pm.recipient_id = @userId AND pm.is_archived = TRUE",
            _ => "pm.recipient_id = @userId AND pm.is_archived = FALSE"
        };

        string orderClause = messageTab switch
        {
            1 => "pm.created_at DESC",
            _ => "pm.created_at DESC"
        };

        var countSql = $@"
            SELECT COUNT(*)
            FROM private_messages pm
            WHERE {whereClause}";

        var dataSql = $@"
            SELECT pm.id, pm.sender_id, pm.recipient_id, pm.subject, pm.body,
                   pm.is_read, pm.is_archived, pm.is_system_message, pm.reply_to_id, pm.created_at,
                   su.user_name AS sender_name, su.headshot_thumbnail_url AS sender_thumbnail,
                   ru.user_name AS recipient_name, ru.headshot_thumbnail_url AS recipient_thumbnail
            FROM private_messages pm
            LEFT JOIN users su ON su.user_id = pm.sender_id
            LEFT JOIN users ru ON ru.user_id = pm.recipient_id
            WHERE {whereClause}
            ORDER BY {orderClause}
            LIMIT @pageSize OFFSET @offset";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        long totalCount;
        using (var countCmd = new NpgsqlCommand(countSql, conn))
        {
            countCmd.Parameters.AddWithValue("@userId", userId);
            totalCount = Convert.ToInt64(await countCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false));
        }

        int totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling((double)totalCount / pageSize);
        var collection = new List<PrivateMessage>();

        using (var dataCmd = new NpgsqlCommand(dataSql, conn))
        {
            dataCmd.Parameters.AddWithValue("@userId", userId);
            dataCmd.Parameters.AddWithValue("@pageSize", pageSize);
            dataCmd.Parameters.AddWithValue("@offset", pageNumber * pageSize);

            using var reader = await dataCmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                collection.Add(new PrivateMessage
                {
                    Id = reader.GetInt64(0),
                    SenderId = reader.GetInt64(1),
                    RecipientId = reader.GetInt64(2),
                    Subject = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    Body = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    IsRead = reader.GetBoolean(5),
                    IsArchived = reader.GetBoolean(6),
                    IsSystemMessage = reader.GetBoolean(7),
                    ReplyToId = reader.IsDBNull(8) ? null : reader.GetInt64(8),
                    CreatedAt = reader.GetDateTime(9),
                    SenderUserName = reader.IsDBNull(10) ? "" : reader.GetString(10),
                    SenderThumbnail = reader.IsDBNull(11) ? "" : reader.GetString(11),
                    RecipientUserName = reader.IsDBNull(12) ? "" : reader.GetString(12),
                    RecipientThumbnail = reader.IsDBNull(13) ? "" : reader.GetString(13)
                });
            }
        }

        return new PrivateMessageResult
        {
            PageNumber = pageNumber,
            TotalPages = totalPages,
            TotalCollectionSize = totalCount,
            Collection = collection
        };
    }

    public async Task<PrivateMessage?> GetMessageByIdAsync(long messageId, long userId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT pm.id, pm.sender_id, pm.recipient_id, pm.subject, pm.body,
                   pm.is_read, pm.is_archived, pm.is_system_message, pm.reply_to_id, pm.created_at,
                   su.user_name AS sender_name, su.headshot_thumbnail_url AS sender_thumbnail,
                   ru.user_name AS recipient_name, ru.headshot_thumbnail_url AS recipient_thumbnail
            FROM private_messages pm
            LEFT JOIN users su ON su.user_id = pm.sender_id
            LEFT JOIN users ru ON ru.user_id = pm.recipient_id
            WHERE pm.id = @messageId
              AND (pm.sender_id = @userId OR pm.recipient_id = @userId)";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@messageId", messageId);
        cmd.Parameters.AddWithValue("@userId", userId);

        using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            return null;

        return new PrivateMessage
        {
            Id = reader.GetInt64(0),
            SenderId = reader.GetInt64(1),
            RecipientId = reader.GetInt64(2),
            Subject = reader.IsDBNull(3) ? "" : reader.GetString(3),
            Body = reader.IsDBNull(4) ? "" : reader.GetString(4),
            IsRead = reader.GetBoolean(5),
            IsArchived = reader.GetBoolean(6),
            IsSystemMessage = reader.GetBoolean(7),
            ReplyToId = reader.IsDBNull(8) ? null : reader.GetInt64(8),
            CreatedAt = reader.GetDateTime(9),
            SenderUserName = reader.IsDBNull(10) ? "" : reader.GetString(10),
            SenderThumbnail = reader.IsDBNull(11) ? "" : reader.GetString(11),
            RecipientUserName = reader.IsDBNull(12) ? "" : reader.GetString(12),
            RecipientThumbnail = reader.IsDBNull(13) ? "" : reader.GetString(13)
        };
    }

    public async Task<long> SendMessageAsync(long senderId, long recipientId, string subject, string body, long? replyToId, bool includePreviousMessage, CancellationToken cancellationToken = default)
    {
        string finalBody = body;
        string finalSubject = subject;

        if (replyToId.HasValue && includePreviousMessage)
        {
            var previousMessage = await GetMessageByIdAsync(replyToId.Value, senderId, cancellationToken).ConfigureAwait(false);
            if (previousMessage != null)
            {
                finalBody = body + "\n\n---------- Original Message ----------\nFrom: " +
                    previousMessage.SenderUserName + "\nSubject: " + previousMessage.Subject +
                    "\n\n" + previousMessage.Body;
                if (string.IsNullOrWhiteSpace(subject) && !string.IsNullOrWhiteSpace(previousMessage.Subject))
                    finalSubject = previousMessage.Subject;
            }
        }

        const string sql = @"
            INSERT INTO private_messages (sender_id, recipient_id, subject, body, reply_to_id)
            VALUES (@senderId, @recipientId, @subject, @body, @replyToId)
            RETURNING id";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@senderId", senderId);
        cmd.Parameters.AddWithValue("@recipientId", recipientId);
        cmd.Parameters.AddWithValue("@subject", finalSubject);
        cmd.Parameters.AddWithValue("@body", finalBody);
        cmd.Parameters.AddWithValue("@replyToId", (object?)replyToId ?? DBNull.Value);

        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return Convert.ToInt64(result);
    }

    public async Task MarkReadAsync(long[] messageIds, long userId, CancellationToken cancellationToken = default)
    {
        if (messageIds.Length == 0) return;

        var parameters = new List<string>();
        for (int i = 0; i < messageIds.Length; i++)
        {
            parameters.Add($"@id{i}");
        }

        var sql = $@"
            UPDATE private_messages
            SET is_read = TRUE
            WHERE id IN ({string.Join(",", parameters)})
              AND recipient_id = @userId";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var cmd = new NpgsqlCommand(sql, conn);
        for (int i = 0; i < messageIds.Length; i++)
        {
            cmd.Parameters.AddWithValue($"@id{i}", messageIds[i]);
        }
        cmd.Parameters.AddWithValue("@userId", userId);

        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task MarkUnreadAsync(long[] messageIds, long userId, CancellationToken cancellationToken = default)
    {
        if (messageIds.Length == 0) return;

        var parameters = new List<string>();
        for (int i = 0; i < messageIds.Length; i++)
        {
            parameters.Add($"@id{i}");
        }

        var sql = $@"
            UPDATE private_messages
            SET is_read = FALSE
            WHERE id IN ({string.Join(",", parameters)})
              AND recipient_id = @userId";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var cmd = new NpgsqlCommand(sql, conn);
        for (int i = 0; i < messageIds.Length; i++)
        {
            cmd.Parameters.AddWithValue($"@id{i}", messageIds[i]);
        }
        cmd.Parameters.AddWithValue("@userId", userId);

        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task ArchiveMessagesAsync(long[] messageIds, long userId, CancellationToken cancellationToken = default)
    {
        if (messageIds.Length == 0) return;

        var parameters = new List<string>();
        for (int i = 0; i < messageIds.Length; i++)
        {
            parameters.Add($"@id{i}");
        }

        var sql = $@"
            UPDATE private_messages
            SET is_archived = TRUE
            WHERE id IN ({string.Join(",", parameters)})
              AND recipient_id = @userId";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var cmd = new NpgsqlCommand(sql, conn);
        for (int i = 0; i < messageIds.Length; i++)
        {
            cmd.Parameters.AddWithValue($"@id{i}", messageIds[i]);
        }
        cmd.Parameters.AddWithValue("@userId", userId);

        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UnarchiveMessagesAsync(long[] messageIds, long userId, CancellationToken cancellationToken = default)
    {
        if (messageIds.Length == 0) return;

        var parameters = new List<string>();
        for (int i = 0; i < messageIds.Length; i++)
        {
            parameters.Add($"@id{i}");
        }

        var sql = $@"
            UPDATE private_messages
            SET is_archived = FALSE
            WHERE id IN ({string.Join(",", parameters)})
              AND recipient_id = @userId";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var cmd = new NpgsqlCommand(sql, conn);
        for (int i = 0; i < messageIds.Length; i++)
        {
            cmd.Parameters.AddWithValue($"@id{i}", messageIds[i]);
        }
        cmd.Parameters.AddWithValue("@userId", userId);

        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<int> GetUnreadCountAsync(long userId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT COUNT(*)
            FROM private_messages
            WHERE recipient_id = @userId
              AND is_read = FALSE
              AND is_archived = FALSE";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@userId", userId);

        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return Convert.ToInt32(result);
    }

    public async Task<bool> UserExistsAsync(long userId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT 1 FROM users WHERE user_id = @userId";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@userId", userId);

        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return result != null;
    }

    public async Task<string> GetUserNameAsync(long userId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT user_name FROM users WHERE user_id = @userId";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@userId", userId);

        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return result?.ToString() ?? "";
    }
}

public class PrivateMessage
{
    public long Id { get; set; }
    public long SenderId { get; set; }
    public long RecipientId { get; set; }
    public string Subject { get; set; } = "";
    public string Body { get; set; } = "";
    public bool IsRead { get; set; }
    public bool IsArchived { get; set; }
    public bool IsSystemMessage { get; set; }
    public long? ReplyToId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string SenderUserName { get; set; } = "";
    public string SenderThumbnail { get; set; } = "";
    public string RecipientUserName { get; set; } = "";
    public string RecipientThumbnail { get; set; } = "";
}

public class PrivateMessageResult
{
    public int PageNumber { get; set; }
    public int TotalPages { get; set; }
    public long TotalCollectionSize { get; set; }
    public List<PrivateMessage> Collection { get; set; } = new();
}
