using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Common;

public class NotificationService
{
    private readonly string _connectionString;

    public NotificationService(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<long> CreateNotificationAsync(
        long userId,
        string sourceType,
        long? senderUserId = null,
        string senderUserName = "",
        string subjectType = "",
        long subjectId = 0,
        string subjectName = "",
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO notifications (user_id, notification_source_type, sender_user_id, sender_user_name, subject_type, subject_id, subject_name)
            VALUES (@userId, @sourceType, @senderUserId, @senderUserName, @subjectType, @subjectId, @subjectName)
            RETURNING id";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@userId", userId);
        cmd.Parameters.AddWithValue("@sourceType", sourceType);
        cmd.Parameters.AddWithValue("@senderUserId", (object?)senderUserId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@senderUserName", senderUserName);
        cmd.Parameters.AddWithValue("@subjectType", subjectType);
        cmd.Parameters.AddWithValue("@subjectId", subjectId);
        cmd.Parameters.AddWithValue("@subjectName", subjectName);

        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return Convert.ToInt64(result);
    }

    public async Task<int> GetUnreadCountAsync(long userId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT COUNT(*) FROM notifications WHERE user_id = @userId AND is_read = FALSE";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@userId", userId);

        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return Convert.ToInt32(result);
    }

    public async Task<List<NotificationData>> GetRecentNotificationsAsync(long userId, int startIndex, int maxRows, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, user_id, notification_source_type, sender_user_id, sender_user_name,
                   subject_type, subject_id, subject_name, is_read, is_interacted, created_at
            FROM notifications
            WHERE user_id = @userId
            ORDER BY created_at DESC
            LIMIT @maxRows OFFSET @startIndex";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@userId", userId);
        cmd.Parameters.AddWithValue("@maxRows", maxRows);
        cmd.Parameters.AddWithValue("@startIndex", startIndex);

        var notifications = new List<NotificationData>();
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            notifications.Add(new NotificationData
            {
                Id = reader.GetInt64(0),
                UserId = reader.GetInt64(1),
                NotificationSourceType = reader.GetString(2),
                SenderUserId = reader.IsDBNull(3) ? null : reader.GetInt64(3),
                SenderUserName = reader.IsDBNull(4) ? "" : reader.GetString(4),
                SubjectType = reader.IsDBNull(5) ? "" : reader.GetString(5),
                SubjectId = reader.IsDBNull(6) ? 0 : reader.GetInt64(6),
                SubjectName = reader.IsDBNull(7) ? "" : reader.GetString(7),
                IsRead = reader.GetBoolean(8),
                IsInteracted = reader.GetBoolean(9),
                CreatedAt = reader.GetDateTime(10)
            });
        }

        return notifications;
    }

    public async Task ClearUnreadAsync(long userId, CancellationToken cancellationToken = default)
    {
        const string sql = "UPDATE notifications SET is_read = TRUE WHERE user_id = @userId AND is_read = FALSE";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@userId", userId);
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task MarkInteractedAsync(long notificationId, CancellationToken cancellationToken = default)
    {
        const string sql = "UPDATE notifications SET is_interacted = TRUE WHERE id = @id";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", notificationId);
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public class NotificationData
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("userId")]
        public long UserId { get; set; }

        [JsonPropertyName("notificationSourceType")]
        public string NotificationSourceType { get; set; } = string.Empty;

        [JsonPropertyName("senderUserId")]
        public long? SenderUserId { get; set; }

        [JsonPropertyName("senderUserName")]
        public string SenderUserName { get; set; } = string.Empty;

        [JsonPropertyName("subjectType")]
        public string SubjectType { get; set; } = string.Empty;

        [JsonPropertyName("subjectId")]
        public long SubjectId { get; set; }

        [JsonPropertyName("subjectName")]
        public string SubjectName { get; set; } = string.Empty;

        [JsonPropertyName("isRead")]
        public bool IsRead { get; set; }

        [JsonPropertyName("isInteracted")]
        public bool IsInteracted { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}
