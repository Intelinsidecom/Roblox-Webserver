using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Common;

namespace Users
{
    public static partial class UserQueries
    {
        public static async Task<bool> UserExistsAsync(string connectionString, long userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0) return false;

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            using var cmd = new NpgsqlCommand("select 1 from users where user_id = @id", conn);
            cmd.Parameters.AddWithValue("id", userId);
            var obj = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            return obj != null;
        }

        public static async Task<string?> GetCreatorOfIdAsync(string connectionString, long userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                return null;

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            using var cmd = new NpgsqlCommand("select user_name from users where user_id = @id", conn);
            cmd.Parameters.AddWithValue("id", userId);
            var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            return result == null || result is DBNull ? null : Convert.ToString(result);
        }

        public static Task<string?> GetUserNameByIdAsync(string connectionString, long userId, CancellationToken cancellationToken = default)
        {
            return GetCreatorOfIdAsync(connectionString, userId, cancellationToken);
        }

        public static async Task<bool> UsernameExistsAsync(string connectionString, string username, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (string.IsNullOrWhiteSpace(username))
                return false;

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            using var cmd = new NpgsqlCommand("select 1 from users where lower(user_name) = lower(@username)", conn);
            cmd.Parameters.AddWithValue("username", username);
            var obj = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            return obj != null;
        }

        public static async Task<Dictionary<string, object?>?> GetUserProfileDataAsync(string connectionString, long userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                return null;

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            using var cmd = new NpgsqlCommand(@"
                select user_name,
                       friends_count,
                       followers_count,
                       following_count,
                       subscription_type,
                       membership_status,
                       premium_member,
                       can_pm,
                       can_chat,
                       can_trade,
                       profile_visibility,
                       description_bio,
                       headshot_thumbnail_url,
                       avatar_thumbnail_url,
                       in_game,
                       current_place_id,
                       status_text,
                       profile_collectables,
                       user_created
                from users
                where user_id = @id", conn);
            cmd.Parameters.AddWithValue("id", userId);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                return null;

            return new Dictionary<string, object?>
            {
                ["userName"] = reader.IsDBNull(0) ? null : reader.GetString(0),
                ["friendsCount"] = reader.GetInt32(1),
                ["followersCount"] = reader.GetInt32(2),
                ["followingCount"] = reader.GetInt32(3),
                ["subscriptionType"] = reader.IsDBNull(4) ? null : reader.GetString(4),
                ["membershipStatus"] = reader.GetInt16(5),
                ["premiumMember"] = reader.GetBoolean(6),
                ["canPm"] = reader.GetBoolean(7),
                ["canChat"] = reader.GetBoolean(8),
                ["canTrade"] = reader.GetBoolean(9),
                ["profileVisibility"] = reader.IsDBNull(10) ? null : reader.GetString(10),
                ["descriptionBio"] = reader.IsDBNull(11) ? null : reader.GetString(11),
                ["headshotThumbnailUrl"] = reader.IsDBNull(12) ? null : reader.GetString(12),
                ["avatarThumbnailUrl"] = reader.IsDBNull(13) ? null : reader.GetString(13),
                ["inGame"] = reader.GetBoolean(14),
                ["currentPlaceId"] = reader.IsDBNull(15) ? (long?)null : reader.GetInt64(15),
                ["statusText"] = reader.IsDBNull(16) ? null : reader.GetString(16),
                ["profileCollectables"] = reader.IsDBNull(17) ? null : (int[]?)reader.GetValue(17),
                ["userCreated"] = reader.IsDBNull(18) ? null : reader.GetDateTime(18)
            };
        }

        public static async Task<List<Dictionary<string, object?>>> GetWornAssetDetailsAsync(string connectionString, long userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                return new List<Dictionary<string, object?>>();

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var cmd = new NpgsqlCommand(@"
                select a.asset_id, a.name, coalesce(a.thumbnail_url, '') as thumbnail_url
                from avatar_worn_assets w
                join assets a on a.asset_id = w.asset_id
                where w.user_id = @uid
                order by a.asset_type_id, a.name", conn);
            cmd.Parameters.AddWithValue("uid", userId);

            var results = new List<Dictionary<string, object?>>();
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                results.Add(new Dictionary<string, object?>
                {
                    ["assetId"] = reader.GetInt64(0),
                    ["name"] = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    ["thumbnailUrl"] = reader.IsDBNull(2) ? "" : reader.GetString(2)
                });
            }

            return results;
        }

        public static async Task<int> GetTotalPlaceVisitsAsync(string connectionString, long userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                return 0;

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var cmd = new NpgsqlCommand(@"
                select coalesce(sum(visit_count), 0)
                from universes
                where creator_user_id = @uid", conn);
            cmd.Parameters.AddWithValue("uid", userId);

            var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            if (result is int intVal) return intVal;
            if (result is long longVal) return (int)longVal;
            if (result is decimal decVal) return (int)decVal;
            return 0;
        }

        public static async Task<bool> UpdateUserStatusTextAsync(string connectionString, long userId, string? statusText, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                return false;

            try
            {
                await using var conn = new NpgsqlConnection(connectionString);
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

                using var cmd = new NpgsqlCommand(@"
                    UPDATE users
                    SET status_text = @statusText
                    WHERE user_id = @userId", conn);

                if (string.IsNullOrWhiteSpace(statusText))
                    cmd.Parameters.AddWithValue("statusText", DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("statusText", statusText);

                cmd.Parameters.AddWithValue("userId", userId);

                int rowsAffected = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                return rowsAffected > 0;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> UpdateUserPasswordAsync(string connectionString, long userId, string newPassword, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                return false;
            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("newPassword is required", nameof(newPassword));

            try
            {
                string hashedPassword = PasswordHasher.HashPassword(newPassword);

                await using var conn = new NpgsqlConnection(connectionString);
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
                
                using var cmd = new NpgsqlCommand(@"
                    UPDATE users 
                    SET password = @password, password_last_changed_at = @changedAt
                    WHERE user_id = @userId", conn);
                
                cmd.Parameters.AddWithValue("password", hashedPassword);
                cmd.Parameters.AddWithValue("changedAt", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("userId", userId);

                int rowsAffected = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating password for user {userId}: {ex.Message}");
                throw;
            }
        }
        public static async Task<bool> ToggleProfileCollectableAsync(string connectionString, long userId, long assetId, bool addToProfile)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0 || assetId <= 0)
                return false;

            try
            {
                await using var conn = new NpgsqlConnection(connectionString);
                await conn.OpenAsync().ConfigureAwait(false);

                int[] current;
                using (var readCmd = new NpgsqlCommand("select profile_collectables from users where user_id = @uid", conn))
                {
                    readCmd.Parameters.AddWithValue("uid", userId);
                    var result = await readCmd.ExecuteScalarAsync().ConfigureAwait(false);
                    current = result == null || result == DBNull.Value
                        ? Array.Empty<int>()
                        : (int[])result;
                }

                if (addToProfile)
                {
                    if (Array.IndexOf(current, (int)assetId) < 0)
                    {
                        var extended = new int[current.Length + 1];
                        current.CopyTo(extended, 0);
                        extended[extended.Length - 1] = (int)assetId;
                        current = extended;
                    }
                }
                else
                {
                    current = Array.FindAll(current, id => id != (int)assetId);
                }

                using (var writeCmd = new NpgsqlCommand("update users set profile_collectables = @arr where user_id = @uid", conn))
                {
                    writeCmd.Parameters.AddWithValue("arr", current);
                    writeCmd.Parameters.AddWithValue("uid", userId);
                    await writeCmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
