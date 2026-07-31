using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Users
{
    public class FeedEntry
    {
        public long FeedEntryId { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; } = "";
        public string HeadshotUrl { get; set; } = "";
        public string Message { get; set; } = "";
        public short FeedType { get; set; }
        public long? GroupId { get; set; }
        public string? GroupName { get; set; }
        public string? GroupThumbUrl { get; set; }
        public long? PosterUserId { get; set; }
        public string? PosterUserName { get; set; }
        public string? PosterHeadshotUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public static partial class UserQueries
    {
        public static async Task<long> InsertFeedEntryAsync(string connectionString, long userId, string message, short feedType = 0, long? groupId = null, long? posterUserId = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            var recipientIds = new List<long> { userId };
            using (var friendCmd = new NpgsqlCommand(@"
                select friend_user_id
                from user_friends
                where user_id = @userId", conn))
            {
                friendCmd.Parameters.AddWithValue("userId", userId);
                await using var friendReader = await friendCmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                while (await friendReader.ReadAsync(cancellationToken).ConfigureAwait(false))
                    recipientIds.Add(friendReader.GetInt64(0));
            }

            var authorId = posterUserId ?? userId;
            long insertedId = 0;

            foreach (var recipientId in recipientIds.Distinct())
            {
                using var cmd = new NpgsqlCommand(@"
                    INSERT INTO feed_entries (user_id, message, feed_type, group_id, poster_user_id)
                    VALUES (@userId, @message, @feedType, @groupId, @posterUserId)
                    RETURNING feed_entry_id", conn);

                cmd.Parameters.AddWithValue("userId", recipientId);
                cmd.Parameters.AddWithValue("message", message);
                cmd.Parameters.AddWithValue("feedType", feedType);
                cmd.Parameters.AddWithValue("groupId", groupId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("posterUserId", authorId);

                var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                if (insertedId == 0 && result != null && result is not DBNull)
                    insertedId = Convert.ToInt64(result);
            }

            return insertedId;
        }

        public static async Task<List<FeedEntry>> GetFeedEntriesForUserAsync(string connectionString, long currentUserId, int limit = 20, CancellationToken cancellationToken = default)
        {
            var results = new List<FeedEntry>();
            if (string.IsNullOrWhiteSpace(connectionString) || currentUserId <= 0)
                return results;

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var cmd = new NpgsqlCommand(@"
                SELECT fe.feed_entry_id, fe.user_id, u.user_name,
                       coalesce(u.headshot_thumbnail_url, '') as headshot_url,
                       fe.message, fe.feed_type, fe.group_id,
                       '' as group_name,
                       '' as group_thumb_url,
                fe.poster_user_id,
                coalesce(pu.user_name, '') as poster_user_name,
                coalesce(pu.headshot_thumbnail_url, '') as poster_headshot_url,
                fe.created_at
                FROM feed_entries fe
                JOIN users u ON u.user_id = fe.user_id
                LEFT JOIN users pu ON pu.user_id = fe.poster_user_id
                WHERE fe.user_id = @currentUserId
                ORDER BY fe.created_at DESC
                LIMIT @limit", conn);

            cmd.Parameters.AddWithValue("currentUserId", currentUserId);
            cmd.Parameters.AddWithValue("limit", limit);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                results.Add(new FeedEntry
                {
                    FeedEntryId = reader.GetInt64(0),
                    UserId = reader.GetInt64(1),
                    UserName = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    HeadshotUrl = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    Message = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    FeedType = reader.GetInt16(5),
                    GroupId = reader.IsDBNull(6) ? (long?)null : reader.GetInt64(6),
                    GroupName = null,
                    GroupThumbUrl = null,
                    PosterUserId = reader.IsDBNull(9) ? (long?)null : reader.GetInt64(9),
                    PosterUserName = reader.IsDBNull(10) ? null : reader.GetString(10),
                    PosterHeadshotUrl = reader.IsDBNull(11) ? null : reader.GetString(11),
                    CreatedAt = reader.GetDateTime(12)
                });
            }

            return results;
        }
    }
}
