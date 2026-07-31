using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Users
{
    public static partial class UserQueries
    {
        public static async Task<List<Dictionary<string, object?>>> GetFriendListAsync(
            string connectionString, long userId, long currentUserId, long currentPage, int pageSize, string friendsType,
            CancellationToken cancellationToken = default)
        {
            var results = new List<Dictionary<string, object?>>();
            if (string.IsNullOrWhiteSpace(connectionString) || userId <= 0)
                return results;

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            var offset = currentPage;

            string sql;
            string orderClause = "order by u.user_name";

            // All queries return 9 columns:
            // 0: user_id, 1: user_name, 2: thumbnail_url, 3: last_activity,
            // 4: presence_universe_id, 5: in_game, 6: status_text,
            // 7: is_followed, 8: invitation_id (null for non-request types)

            switch (friendsType)
            {
                case "AllFriends":
                    sql = $@"
                        select u.user_id, u.user_name, coalesce(u.headshot_thumbnail_url, '') as thumbnail_url,
                               u.last_activity, u.presence_universe_id, u.in_game, u.status_text,
                               exists(
                                   select 1 from users us
                                   where us.user_id = @currentUserId
                                   and u.user_id = any(us.following)
                               ) as is_followed,
                               null::bigint as invitation_id
                        from user_friends uf
                        join users u on u.user_id = uf.friend_user_id
                        where uf.user_id = @userId
                        {orderClause}
                        offset @offset limit @limit";
                    break;

                case "Following":
                    sql = $@"
                        select u.user_id, u.user_name, coalesce(u.headshot_thumbnail_url, '') as thumbnail_url,
                               u.last_activity, u.presence_universe_id, u.in_game, u.status_text,
                               true as is_followed,
                               null::bigint as invitation_id
                        from users u
                        where u.user_id = any(
                            select unnest(following) from users where user_id = @userId
                        )
                        {orderClause}
                        offset @offset limit @limit";
                    break;

                case "Followers":
                    sql = $@"
                        select u.user_id, u.user_name, coalesce(u.headshot_thumbnail_url, '') as thumbnail_url,
                               u.last_activity, u.presence_universe_id, u.in_game, u.status_text,
                               exists(
                                   select 1 from users us
                                   where us.user_id = @currentUserId
                                   and u.user_id = any(us.following)
                               ) as is_followed,
                               null::bigint as invitation_id
                        from users u
                        where u.user_id = any(
                            select unnest(followers) from users where user_id = @userId
                        )
                        {orderClause}
                        offset @offset limit @limit";
                    break;

                case "FriendRequests":
                    sql = $@"
                        select u.user_id, u.user_name, coalesce(u.headshot_thumbnail_url, '') as thumbnail_url,
                               u.last_activity, u.presence_universe_id, u.in_game, u.status_text,
                               false as is_followed,
                               fr.id as invitation_id
                        from friend_requests fr
                        join users u on u.user_id = fr.sender_id
                        where fr.receiver_id = @userId and fr.status = 'pending'
                        order by fr.created_at desc
                        offset @offset limit @limit";
                    break;

                default:
                    return results;
            }

            using (var cmd = new NpgsqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("userId", userId);
                cmd.Parameters.AddWithValue("currentUserId", currentUserId);
                cmd.Parameters.AddWithValue("offset", offset);
                cmd.Parameters.AddWithValue("limit", pageSize);

                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    var friendUserId = reader.GetInt64(0);
                    var thumbnailUrl = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    var presenceUniverseId = reader.IsDBNull(4) ? (long?)null : reader.GetInt64(4);
                    var statusText = reader.IsDBNull(6) ? "" : reader.GetString(6);
                    var entry = new Dictionary<string, object?>
                    {
                        ["UserId"] = friendUserId,
                        ["Username"] = reader.IsDBNull(1) ? "" : reader.GetString(1),
                        ["DisplayName"] = reader.IsDBNull(1) ? "" : reader.GetString(1),
                        ["AvatarUri"] = thumbnailUrl,
                        ["Thumbnail"] = new Dictionary<string, object?>
                        {
                            ["Url"] = thumbnailUrl,
                            ["RetryUrl"] = $"/thumbnail/avatar-headshot?userId={friendUserId}",
                            ["Final"] = !string.IsNullOrEmpty(thumbnailUrl),
                            ["UserId"] = friendUserId,
                            ["EndpointType"] = "Avatar"
                        },
                        ["IsOnline"] = !reader.IsDBNull(3) &&
                            (DateTime.UtcNow - reader.GetDateTime(3)).TotalMinutes < 5,
                        ["IsDeleted"] = false,
                        ["IsFollowed"] = !reader.IsDBNull(7) && reader.GetBoolean(7),
                        ["IsFriend"] = friendsType == "AllFriends",
                        ["FriendStatus"] = friendsType == "AllFriends" ? "Friend"
                            : friendsType == "FriendRequests" ? "FriendRequestReceived"
                            : "NotFriend",
                        ["LastLocation"] = statusText,
                        ["StatusText"] = statusText,
                        ["InGame"] = !reader.IsDBNull(5) && reader.GetBoolean(5),
                        ["InStudio"] = false,
                        ["AbsolutePlaceURL"] = presenceUniverseId.HasValue
                            ? $"/games/{presenceUniverseId.Value}" : ""
                    };

                    if (friendsType == "FriendRequests" && !reader.IsDBNull(8))
                    {
                        entry["InvitationId"] = reader.GetInt64(8);
                    }

                    results.Add(entry);
                }
            }

            return results;
        }

        public static async Task<int> GetFriendListTotalCountAsync(
            string connectionString, long userId, string friendsType,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString) || userId <= 0)
                return 0;

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            string sql = friendsType switch
            {
                "AllFriends" => "select count(*) from user_friends where user_id = @userId",
                "Following" => "select coalesce(array_length((select following from users where user_id = @userId), 1), 0)",
                "Followers" => "select coalesce(array_length((select followers from users where user_id = @userId), 1), 0)",
                "FriendRequests" => "select count(*) from friend_requests where receiver_id = @userId and status = 'pending'",
                _ => "select 0"
            };

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("userId", userId);
            var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            return result == null || result is DBNull ? 0 : Convert.ToInt32(result);
        }

        public static async Task<Dictionary<string, object?>?> SendFriendRequestAsync(
            string connectionString, long senderId, long receiverId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString) || senderId <= 0 || receiverId <= 0)
                return new Dictionary<string, object?> { ["success"] = false, ["errorId"] = 3 };

            if (senderId == receiverId)
                return new Dictionary<string, object?> { ["success"] = false, ["errorId"] = 6 };

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            var receiverExists = await UserExistsAsync(connectionString, receiverId, cancellationToken).ConfigureAwait(false);
            if (!receiverExists)
                return new Dictionary<string, object?> { ["success"] = false, ["errorId"] = 10 };

            using (var checkCmd = new NpgsqlCommand(@"
                select id from friend_requests
                where sender_id = @receiver and receiver_id = @sender
                  and status = 'pending'
                limit 1", conn))
            {
                checkCmd.Parameters.AddWithValue("sender", senderId);
                checkCmd.Parameters.AddWithValue("receiver", receiverId);
                var reverseResult = await checkCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                if (reverseResult != null && reverseResult is long existingRequestId)
                {
                    using var acceptCmd = new NpgsqlCommand(@"
                        update friend_requests
                        set status = 'accepted', updated_at = now()
                        where id = @id", conn);
                    acceptCmd.Parameters.AddWithValue("id", existingRequestId);
                    await acceptCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

                    using var insertCmd = new NpgsqlCommand(@"
                        insert into user_friends (user_id, friend_user_id)
                        values (@sender, @receiver), (@receiver, @sender)
                        on conflict do nothing", conn);
                    insertCmd.Parameters.AddWithValue("sender", senderId);
                    insertCmd.Parameters.AddWithValue("receiver", receiverId);
                    await insertCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

                    using var updateCmd = new NpgsqlCommand(@"
                        update users set friends_count = friends_count + 1
                        where user_id in (@sender, @receiver)", conn);
                    updateCmd.Parameters.AddWithValue("sender", senderId);
                    updateCmd.Parameters.AddWithValue("receiver", receiverId);
                    await updateCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

                    return new Dictionary<string, object?> { ["success"] = true };
                }
            }

            using (var sameCheckCmd = new NpgsqlCommand(@"
                select 1 from friend_requests
                where sender_id = @sender and receiver_id = @receiver
                  and status = 'pending'
                limit 1", conn))
            {
                sameCheckCmd.Parameters.AddWithValue("sender", senderId);
                sameCheckCmd.Parameters.AddWithValue("receiver", receiverId);
                var sameResult = await sameCheckCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                if (sameResult != null)
                    return new Dictionary<string, object?> { ["success"] = false, ["errorId"] = 2 };
            }

            using (var friendCheck = new NpgsqlCommand(@"
                select 1 from user_friends
                where (user_id = @sender and friend_user_id = @receiver)
                   or (user_id = @receiver and friend_user_id = @sender)
                limit 1", conn))
            {
                friendCheck.Parameters.AddWithValue("sender", senderId);
                friendCheck.Parameters.AddWithValue("receiver", receiverId);
                var alreadyFriends = await friendCheck.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                if (alreadyFriends != null)
                    return new Dictionary<string, object?> { ["success"] = false, ["errorId"] = 2 };
            }

            using (var insertCmd = new NpgsqlCommand(@"
                insert into friend_requests (sender_id, receiver_id, status)
                values (@sender, @receiver, 'pending')", conn))
            {
                insertCmd.Parameters.AddWithValue("sender", senderId);
                insertCmd.Parameters.AddWithValue("receiver", receiverId);
                await insertCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            return new Dictionary<string, object?> { ["success"] = true };
        }

        public static async Task<Dictionary<string, object?>> AcceptFriendRequestAsync(
            string connectionString, long requestId, long senderId, long receiverId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                return new Dictionary<string, object?> { ["success"] = false, ["errorId"] = 3 };

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var cmd = new NpgsqlCommand(@"
                update friend_requests
                set status = 'accepted', updated_at = now()
                where sender_id = @sender and receiver_id = @receiver and status = 'pending'", conn);
            cmd.Parameters.AddWithValue("sender", senderId);
            cmd.Parameters.AddWithValue("receiver", receiverId);
            var rows = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            if (rows == 0)
            {
                using var fallbackCmd = new NpgsqlCommand(@"
                    update friend_requests
                    set status = 'accepted', updated_at = now()
                    where id = @id and status = 'pending'", conn);
                fallbackCmd.Parameters.AddWithValue("id", requestId);
                rows = await fallbackCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            if (rows == 0)
                return new Dictionary<string, object?> { ["success"] = false, ["errorId"] = 3 };

            using (var insertCmd = new NpgsqlCommand(@"
                insert into user_friends (user_id, friend_user_id)
                values (@sender, @receiver), (@receiver, @sender)
                on conflict do nothing", conn))
            {
                insertCmd.Parameters.AddWithValue("sender", senderId);
                insertCmd.Parameters.AddWithValue("receiver", receiverId);
                await insertCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            using (var updateCmd = new NpgsqlCommand(@"
                update users set friends_count = friends_count + 1
                where user_id in (@sender, @receiver)", conn))
            {
                updateCmd.Parameters.AddWithValue("sender", senderId);
                updateCmd.Parameters.AddWithValue("receiver", receiverId);
                await updateCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            return new Dictionary<string, object?> { ["success"] = true };
        }

        public static async Task<Dictionary<string, object?>> DeclineFriendRequestAsync(
            string connectionString, long requestId,
            CancellationToken cancellationToken = default,
            long senderId = 0, long receiverId = 0)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                return new Dictionary<string, object?> { ["success"] = false };

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var cmd = new NpgsqlCommand(@"
                update friend_requests
                set status = 'declined', updated_at = now()
                where id = @id and status = 'pending'", conn);
            cmd.Parameters.AddWithValue("id", requestId);
            var rows = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            if (rows == 0 && senderId > 0 && receiverId > 0)
            {
                using var fallbackCmd = new NpgsqlCommand(@"
                    update friend_requests
                    set status = 'declined', updated_at = now()
                    where sender_id = @sender and receiver_id = @receiver and status = 'pending'", conn);
                fallbackCmd.Parameters.AddWithValue("sender", senderId);
                fallbackCmd.Parameters.AddWithValue("receiver", receiverId);
                await fallbackCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            return new Dictionary<string, object?> { ["success"] = true };
        }

        public static async Task<Dictionary<string, object?>> DeclineAllFriendRequestsAsync(
            string connectionString, long receiverId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                return new Dictionary<string, object?> { ["success"] = false };

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var cmd = new NpgsqlCommand(@"
                update friend_requests
                set status = 'declined', updated_at = now()
                where receiver_id = @receiverId and status = 'pending'", conn);
            cmd.Parameters.AddWithValue("receiverId", receiverId);
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            return new Dictionary<string, object?> { ["success"] = true };
        }

        public static async Task<Dictionary<string, object?>> RemoveFriendshipAsync(
            string connectionString, long userId1, long userId2,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString) || userId1 <= 0 || userId2 <= 0)
                return new Dictionary<string, object?> { ["success"] = false };

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var cmd = new NpgsqlCommand(@"
                delete from user_friends
                where (user_id = @u1 and friend_user_id = @u2)
                   or (user_id = @u2 and friend_user_id = @u1)", conn);
            cmd.Parameters.AddWithValue("u1", userId1);
            cmd.Parameters.AddWithValue("u2", userId2);
            var deleted = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            if (deleted > 0)
            {
                using var updateCmd = new NpgsqlCommand(@"
                    update users set friends_count = greatest(friends_count - 1, 0)
                    where user_id in (@u1, @u2)", conn);
                updateCmd.Parameters.AddWithValue("u1", userId1);
                updateCmd.Parameters.AddWithValue("u2", userId2);
                await updateCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            return new Dictionary<string, object?> { ["success"] = true };
        }

        public static async Task<Dictionary<string, object?>> FollowUserAsync(
            string connectionString, long followerId, long targetUserId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString) || followerId <= 0 || targetUserId <= 0)
                return new Dictionary<string, object?> { ["success"] = false, ["message"] = "Invalid parameters" };

            if (followerId == targetUserId)
                return new Dictionary<string, object?> { ["success"] = false, ["message"] = "Cannot follow yourself" };

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            var targetExists = await UserExistsAsync(connectionString, targetUserId, cancellationToken).ConfigureAwait(false);
            if (!targetExists)
                return new Dictionary<string, object?> { ["success"] = false, ["message"] = "User not found" };

            using (var checkCmd = new NpgsqlCommand(@"
                select 1 from users
                where user_id = @followerId and @targetId = any(following)
                limit 1", conn))
            {
                checkCmd.Parameters.AddWithValue("followerId", followerId);
                checkCmd.Parameters.AddWithValue("targetId", (int)targetUserId);
                var alreadyFollowing = await checkCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                if (alreadyFollowing != null)
                    return new Dictionary<string, object?> { ["success"] = true };
            }

            using (var followCmd = new NpgsqlCommand(@"
                update users
                set following = array_append(following, @targetId::int),
                    following_count = following_count + 1
                where user_id = @followerId", conn))
            {
                followCmd.Parameters.AddWithValue("followerId", followerId);
                followCmd.Parameters.AddWithValue("targetId", (int)targetUserId);
                await followCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            using (var followerCmd = new NpgsqlCommand(@"
                update users
                set followers = array_append(followers, @followerId::int),
                    followers_count = followers_count + 1
                where user_id = @targetId", conn))
            {
                followerCmd.Parameters.AddWithValue("targetId", targetUserId);
                followerCmd.Parameters.AddWithValue("followerId", (int)followerId);
                await followerCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            return new Dictionary<string, object?> { ["success"] = true };
        }

        public static async Task<Dictionary<string, object?>> UnfollowUserAsync(
            string connectionString, long followerId, long targetUserId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString) || followerId <= 0 || targetUserId <= 0)
                return new Dictionary<string, object?> { ["success"] = false };

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            using (var unfollowCmd = new NpgsqlCommand(@"
                update users
                set following = array_remove(following, @targetId::int),
                    following_count = greatest(following_count - 1, 0)
                where user_id = @followerId", conn))
            {
                unfollowCmd.Parameters.AddWithValue("followerId", followerId);
                unfollowCmd.Parameters.AddWithValue("targetId", (int)targetUserId);
                await unfollowCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            using (var removeFollowerCmd = new NpgsqlCommand(@"
                update users
                set followers = array_remove(followers, @followerId::int),
                    followers_count = greatest(followers_count - 1, 0)
                where user_id = @targetId", conn))
            {
                removeFollowerCmd.Parameters.AddWithValue("targetId", targetUserId);
                removeFollowerCmd.Parameters.AddWithValue("followerId", (int)followerId);
                await removeFollowerCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            return new Dictionary<string, object?> { ["success"] = true };
        }

        public static async Task<bool> AreFriendsAsync(
            string connectionString, long userId1, long userId2,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString) || userId1 <= 0 || userId2 <= 0)
                return false;

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var cmd = new NpgsqlCommand(@"
                select 1 from user_friends
                where (user_id = @u1 and friend_user_id = @u2)
                   or (user_id = @u2 and friend_user_id = @u1)
                limit 1", conn);
            cmd.Parameters.AddWithValue("u1", userId1);
            cmd.Parameters.AddWithValue("u2", userId2);
            var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            return result != null;
        }

        public static async Task<bool> IsFollowingAsync(
            string connectionString, long followerId, long targetUserId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString) || followerId <= 0 || targetUserId <= 0)
                return false;

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var cmd = new NpgsqlCommand(@"
                select 1 from users
                where user_id = @followerId and @targetId = any(following)
                limit 1", conn);
            cmd.Parameters.AddWithValue("followerId", followerId);
            cmd.Parameters.AddWithValue("targetId", (int)targetUserId);
            var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            return result != null;
        }

        public static async Task<(bool hasPending, long requestId, bool isIncoming)> GetPendingFriendRequestAsync(
            string connectionString, long userId1, long userId2,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString) || userId1 <= 0 || userId2 <= 0)
                return (false, 0, false);

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var cmd = new NpgsqlCommand(@"
                select id, sender_id, receiver_id from friend_requests
                where ((sender_id = @u1 and receiver_id = @u2)
                   or (sender_id = @u2 and receiver_id = @u1))
                  and status = 'pending'
                limit 1", conn);
            cmd.Parameters.AddWithValue("u1", userId1);
            cmd.Parameters.AddWithValue("u2", userId2);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var requestId = reader.GetInt64(0);
                var senderId = reader.GetInt64(1);
                return (true, requestId, senderId == userId2);
            }

            return (false, 0, false);
        }

        public static async Task<bool> IsBlockedAsync(
            string connectionString, long currentUserId, long targetUserId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString) || currentUserId <= 0 || targetUserId <= 0)
                return false;

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var cmd = new NpgsqlCommand(@"
                select 1 from users
                where user_id = @currentUserId and @targetId = any(blocked)
                limit 1", conn);
            cmd.Parameters.AddWithValue("currentUserId", currentUserId);
            cmd.Parameters.AddWithValue("targetId", (int)targetUserId);
            var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            return result != null;
        }

        public static async Task<Dictionary<string, object?>> BlockUserAsync(
            string connectionString, long currentUserId, long targetUserId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString) || currentUserId <= 0 || targetUserId <= 0)
                return new Dictionary<string, object?> { ["success"] = false, ["error"] = "Invalid parameters" };

            if (currentUserId == targetUserId)
                return new Dictionary<string, object?> { ["success"] = false, ["error"] = "Cannot block yourself" };

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var cmd = new NpgsqlCommand(@"
                update users
                set blocked = array_append(blocked, @targetId::int),
                    blocked_users = blocked_users + 1
                where user_id = @currentUserId
                  and not (@targetId = any(blocked))", conn);
            cmd.Parameters.AddWithValue("currentUserId", currentUserId);
            cmd.Parameters.AddWithValue("targetId", (int)targetUserId);
            var rows = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            return new Dictionary<string, object?>
            {
                ["success"] = rows > 0,
                ["error"] = rows > 0 ? null : "User is already blocked"
            };
        }

        public static async Task<Dictionary<string, object?>> UnblockUserAsync(
            string connectionString, long currentUserId, long targetUserId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString) || currentUserId <= 0 || targetUserId <= 0)
                return new Dictionary<string, object?> { ["success"] = false, ["error"] = "Invalid parameters" };

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var cmd = new NpgsqlCommand(@"
                update users
                set blocked = array_remove(blocked, @targetId::int),
                    blocked_users = greatest(blocked_users - 1, 0)
                where user_id = @currentUserId
                  and (@targetId = any(blocked))", conn);
            cmd.Parameters.AddWithValue("currentUserId", currentUserId);
            cmd.Parameters.AddWithValue("targetId", (int)targetUserId);
            var rows = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            return new Dictionary<string, object?>
            {
                ["success"] = rows > 0,
                ["error"] = rows > 0 ? null : "User is not blocked"
            };
        }

        public static async Task<Dictionary<string, object>> GetBlockedUsersAsync(
            string connectionString, long userId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString) || userId <= 0)
                return new Dictionary<string, object> { ["success"] = true, ["userList"] = new Dictionary<string, bool>() };

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var cmd = new NpgsqlCommand(@"
                select blocked from users
                where user_id = @userId
                limit 1", conn);
            cmd.Parameters.AddWithValue("userId", userId);
            var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);

            var userList = new Dictionary<string, bool>();

            if (result is int[] blockedIds)
            {
                foreach (var id in blockedIds)
                {
                    userList[id.ToString()] = true;
                }
            }

            return new Dictionary<string, object>
            {
                ["success"] = true,
                ["userList"] = userList
            };
        }

        public static async Task<Dictionary<string, object>> GetMultiFollowingExistsAsync(
            string connectionString, long userId, long[] otherUserIds,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString) || userId <= 0 || otherUserIds == null || otherUserIds.Length == 0)
                return new Dictionary<string, object> { ["FollowingDetails"] = new object[0] };

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            HashSet<int> userFollowing = new();
            using (var cmd = new NpgsqlCommand(@"
                select following from users
                where user_id = @userId
                limit 1", conn))
            {
                cmd.Parameters.AddWithValue("userId", userId);
                var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                if (result is int[] arr)
                    userFollowing = new HashSet<int>(arr);
            }

            var otherUserIdsInt = otherUserIds.Select(id => (int)id).ToArray();
            HashSet<int> othersFollowingUser = new();
            using (var cmd = new NpgsqlCommand(@"
                select user_id from users
                where user_id = any(@otherUserIds)
                  and @userId = any(following)", conn))
            {
                cmd.Parameters.AddWithValue("otherUserIds", otherUserIdsInt);
                cmd.Parameters.AddWithValue("userId", (int)userId);
                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    othersFollowingUser.Add(reader.GetInt32(0));
                }
            }

            var details = new List<Dictionary<string, object>>();
            foreach (var otherId in otherUserIds)
            {
                var otherIdInt = (int)otherId;
                details.Add(new Dictionary<string, object>
                {
                    ["UserId1"] = userId,
                    ["UserId2"] = otherId,
                    ["User1FollowsUser2"] = userFollowing.Contains(otherIdInt),
                    ["User2FollowsUser1"] = othersFollowingUser.Contains(otherIdInt)
                });
            }

            return new Dictionary<string, object>
            {
                ["FollowingDetails"] = details
            };
        }

        public static async Task<int> GetFriendCountAsync(
            string connectionString, long userId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString) || userId <= 0)
                return 0;

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var cmd = new NpgsqlCommand(@"
                select friends_count from users
                where user_id = @userId
                limit 1", conn);
            cmd.Parameters.AddWithValue("userId", userId);
            var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            if (result == null || result is DBNull) return 0;
            return Convert.ToInt32(result);
        }

        public static async Task<int> GetPendingFriendRequestCountAsync(
            string connectionString, long userId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString) || userId <= 0)
                return 0;

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var cmd = new NpgsqlCommand(@"
                select count(*) from friend_requests
                where receiver_id = @userId and status = 'pending'", conn);
            cmd.Parameters.AddWithValue("userId", userId);
            var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            if (result == null || result is DBNull) return 0;
            return Convert.ToInt32(result);
        }

        public static async Task<Dictionary<string, object?>> RevokeFriendshipAsync(
            string connectionString, long currentUserId, long otherUserId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString) || currentUserId <= 0 || otherUserId <= 0)
                return new Dictionary<string, object?> { ["success"] = false };

            if (currentUserId == otherUserId)
                return new Dictionary<string, object?> { ["success"] = false };

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            using (var incomingCmd = new NpgsqlCommand(@"
                select id from friend_requests
                where sender_id = @otherUserId and receiver_id = @currentUserId and status = 'pending'
                limit 1", conn))
            {
                incomingCmd.Parameters.AddWithValue("currentUserId", currentUserId);
                incomingCmd.Parameters.AddWithValue("otherUserId", otherUserId);
                var incomingResult = await incomingCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                if (incomingResult != null && incomingResult is long requestId)
                {
                    await DeclineFriendRequestAsync(connectionString, requestId, cancellationToken).ConfigureAwait(false);
                    return new Dictionary<string, object?> { ["success"] = true };
                }
            }

            using (var outgoingCmd = new NpgsqlCommand(@"
                select id from friend_requests
                where sender_id = @currentUserId and receiver_id = @otherUserId and status = 'pending'
                limit 1", conn))
            {
                outgoingCmd.Parameters.AddWithValue("currentUserId", currentUserId);
                outgoingCmd.Parameters.AddWithValue("otherUserId", otherUserId);
                var outgoingResult = await outgoingCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                if (outgoingResult != null && outgoingResult is long requestId)
                {
                    await DeclineFriendRequestAsync(connectionString, requestId, cancellationToken).ConfigureAwait(false);
                    return new Dictionary<string, object?> { ["success"] = true };
                }
            }

            using (var friendsCmd = new NpgsqlCommand(@"
                select 1 from user_friends
                where (user_id = @currentUserId and friend_user_id = @otherUserId)
                   or (user_id = @otherUserId and friend_user_id = @currentUserId)
                limit 1", conn))
            {
                friendsCmd.Parameters.AddWithValue("currentUserId", currentUserId);
                friendsCmd.Parameters.AddWithValue("otherUserId", otherUserId);
                var areFriends = await friendsCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                if (areFriends != null)
                {
                    await RemoveFriendshipAsync(connectionString, currentUserId, otherUserId, cancellationToken).ConfigureAwait(false);
                    return new Dictionary<string, object?> { ["success"] = true };
                }
            }

            return new Dictionary<string, object?> { ["success"] = true };
        }

        public static async Task<List<long>> GetOnlineFriendsAsync(
            string connectionString, long userId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString) || userId <= 0)
                return new List<long>();

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var cmd = new NpgsqlCommand(@"
                select u.user_id
                from user_friends uf
                join users u on u.user_id = uf.friend_user_id
                where uf.user_id = @userId
                  and u.last_activity > now() - interval '5 minutes'", conn);
            cmd.Parameters.AddWithValue("userId", userId);

            var results = new List<long>();
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                results.Add(reader.GetInt64(0));
            }

            return results;
        }

        public static async Task<(List<Dictionary<string, object?>> Presences, List<Dictionary<string, object?>> Relationships)> GetRelationAndPresenceAsync(
            string connectionString, long currentUserId, List<long> userIds,
            CancellationToken cancellationToken = default)
        {
            var presences = new List<Dictionary<string, object?>>();
            var relationships = new List<Dictionary<string, object?>>();
            if (string.IsNullOrWhiteSpace(connectionString) || userIds == null || userIds.Count == 0)
                return (presences, relationships);

            var userIdsDistinct = userIds.Where(u => u > 0).Distinct().ToList();

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            using (var cmd = new NpgsqlCommand(@"
                select user_id, last_activity, in_game, status_text
                from users where user_id = any(@ids)", conn))
            {
                cmd.Parameters.AddWithValue("ids", userIdsDistinct);
                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    var uid = reader.GetInt64(0);
                    var lastActivity = reader.IsDBNull(1) ? (DateTime?)null : reader.GetDateTime(1);
                    var isOnline = lastActivity.HasValue && (DateTime.UtcNow - lastActivity.Value).TotalMinutes < 5;
                    presences.Add(new Dictionary<string, object?>
                    {
                        ["UserId"] = uid,
                        ["IsOnline"] = isOnline,
                        ["InGame"] = !reader.IsDBNull(2) && reader.GetBoolean(2),
                        ["InStudio"] = false,
                        ["LastLocation"] = reader.IsDBNull(3) ? "" : reader.GetString(3)
                    });
                }
            }

            foreach (var uid in userIdsDistinct)
            {
                if (uid == currentUserId)
                {
                    relationships.Add(new Dictionary<string, object?>
                    {
                        ["UserId"] = uid,
                        ["FriendshipStatus"] = "NoFriendship",
                        ["IsFollowed"] = false,
                        ["YourOwnResult"] = true
                    });
                    continue;
                }

                var friendshipStatus = "NoFriendship";
                bool isFollowed = false;

                using (var friendCmd = new NpgsqlCommand(@"
                    select 1 from user_friends
                    where (user_id = @u1 and friend_user_id = @u2)
                       or (user_id = @u2 and friend_user_id = @u1)
                    limit 1", conn))
                {
                    friendCmd.Parameters.AddWithValue("u1", currentUserId);
                    friendCmd.Parameters.AddWithValue("u2", uid);
                    var result = await friendCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                    if (result != null) friendshipStatus = "Friends";
                }

                if (friendshipStatus != "Friends")
                {
                    using (var reqCmd = new NpgsqlCommand(@"
                        select sender_id from friend_requests
                        where status = 'pending'
                          and ((sender_id = @u1 and receiver_id = @u2)
                            or (sender_id = @u2 and receiver_id = @u1))
                        limit 1", conn))
                    {
                        reqCmd.Parameters.AddWithValue("u1", currentUserId);
                        reqCmd.Parameters.AddWithValue("u2", uid);
                        var sender = await reqCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                        if (sender != null)
                        {
                            friendshipStatus = sender is long s && s == currentUserId
                                ? "PendingOnOtherUser"
                                : "PendingOnCurrentUser";
                        }
                    }
                }

                using (var followCmd = new NpgsqlCommand(@"
                    select 1 from users
                    where user_id = @currentUserId and @targetId = any(following)
                    limit 1", conn))
                {
                    followCmd.Parameters.AddWithValue("currentUserId", currentUserId);
                    followCmd.Parameters.AddWithValue("targetId", (int)uid);
                    var result = await followCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                    isFollowed = result != null;
                }

                relationships.Add(new Dictionary<string, object?>
                {
                    ["UserId"] = uid,
                    ["FriendshipStatus"] = friendshipStatus,
                    ["IsFollowed"] = isFollowed
                });
            }

            return (presences, relationships);
        }
    }
}
