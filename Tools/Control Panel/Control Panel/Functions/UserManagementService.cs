using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using Npgsql;
using Control_Panel;
using Thumbnails;
using Users;
using Common;

namespace ControlPanel.Functions
{
    public class UserData
    {
        public long UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? LastActivity { get; set; }
        public string AvatarThumbnailUrl { get; set; }
        public string HeadshotThumbnailUrl { get; set; }
        public long RobuxBalance { get; set; }
        public long TixBalance { get; set; }
        public bool EmailVerified { get; set; }
        public string Facebook { get; set; }
        public string Twitter { get; set; }
        public string GooglePlus { get; set; }
        public string YouTube { get; set; }
        public string Twitch { get; set; }
        public short SocialNetworksVisibility { get; set; }

        public string StatusText
        {
            get
            {
                if (LastActivity.HasValue && LastActivity.Value > DateTime.UtcNow.AddDays(-7))
                    return "Active";
                
                if (LastActivity.HasValue && LastActivity.Value > DateTime.UtcNow.AddDays(-30))
                    return "Inactive";
                
                return "Offline";
            }
        }
        
        public string CreatedDateFormatted => CreatedAt?.ToString("yyyy-MM-dd HH:mm") ?? "Unknown";
        public string LastActivityFormatted => LastActivity?.ToString("yyyy-MM-dd HH:mm") ?? "Never";
        public string RobuxBalanceFormatted => $"{RobuxBalance:N0} Robux";
        public string TixBalanceFormatted => $"{TixBalance:N0} Tix";
        
        public string GenderText
        {
            get
            {
                return Gender?.ToLowerInvariant() switch
                {
                    "male" => "Male",
                    "female" => "Female",
                    "none" => "Unknown",
                    _ => "Unknown"
                };
            }
        }
        
        public string MembershipType { get; set; }

        public string MembershipText
        {
            get
            {
                return MembershipType switch
                {
                    "BuildersClub" => "Builders Club",
                    "TurboBuildersClub" => "Turbo Builders Club",
                    "OutrageousBuildersClub" => "Outrageous Builders Club",
                    _ => "Free"
                };
            }
        }
    }

    public class UserManagementService
    {
        private readonly string connectionString;
        private readonly UserSearchService userSearchService;

        public UserManagementService(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));
            
            this.connectionString = connectionString;
            this.userSearchService = new UserSearchService(connectionString);
        }

        public async Task<UserData> GetUserByIdAsync(long userId)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string query = @"
                        SELECT 
                            user_id, user_name, email, gender, user_created, last_activity,
                            avatar_thumbnail_url, headshot_thumbnail_url,
                            robux_balance, tix_balance,
                            email_verified,
                            social_facebook_url, social_twitter_url, social_googleplus_url,
                            social_youtube_url, social_twitch_url, social_networks_visibility
                        FROM users 
                        WHERE user_id = @userId";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@userId", userId);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var userData = MapReaderToUserData((NpgsqlDataReader)reader);
                                
                                try
                                {
                                    var avatarUrl = await ThumbnailQueries.GetUserThumbnailUrlAsync(connectionString, userId);
                                    var headshotUrl = await ThumbnailQueries.GetUserHeadshotUrlAsync(connectionString, userId);
                                    userData.AvatarThumbnailUrl = !string.IsNullOrEmpty(avatarUrl) ? avatarUrl : userData.AvatarThumbnailUrl;
                                    userData.HeadshotThumbnailUrl = !string.IsNullOrEmpty(headshotUrl) ? headshotUrl : userData.HeadshotThumbnailUrl;
                                }
                                catch (Exception thumbEx)
                                {
                                    ConsoleWindow.Instance?.WriteError($"[Thumbnail Debug] Failed to get dynamic thumbnails: {thumbEx.Message}");
                                }

                                try
                                {
                                    userData.MembershipType = await UserQueries.GetMembershipTypeAsync(connectionString, userId);
                                }
                                catch (Exception memEx)
                                {
                                    ConsoleWindow.Instance?.WriteError($"[Membership Debug] Failed to get membership type: {memEx.Message}");
                                    userData.MembershipType = "None";
                                }
                                
                                return userData;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleWindow.Instance?.WriteError($"Error getting user data for ID {userId}: {ex.Message}");
                ConsoleWindow.Instance?.WriteError($"[SQL Debug] Exception: {ex.StackTrace}");
                throw;
            }

            return null;
        }

        public async Task<bool> UpdateUserFieldAsync(long userId, string fieldName, object value)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    var validFields = new[]
                    {
                        "user_name", "email", "gender"
                    };

                    if (!Array.Exists(validFields, field => field == fieldName))
                    {
                        ConsoleWindow.Instance?.WriteError($"Invalid field name: {fieldName}");
                        return false;
                    }

                    string query = $"UPDATE users SET {fieldName} = @value WHERE user_id = @userId";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@value", value);
                        command.Parameters.AddWithValue("@userId", userId);

                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleWindow.Instance?.WriteError($"Error updating user field {fieldName} for user {userId}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ResetUserPasswordAsync(long userId, string newPassword)
        {
            try
            {
                return await Users.UserQueries.UpdateUserPasswordAsync(connectionString, userId, newPassword);
            }
            catch (Exception ex)
            {
                ConsoleWindow.Instance?.WriteError($"Error resetting password for user {userId}: {ex.Message}");
                return false;
            }
        }

        public async Task<(long userId, bool success, string errorMessage)> CreateUserAsync(string username, string password, string gender)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    long newUserId;
                    using (var cmd = new NpgsqlCommand("select coalesce(max(user_id), 0) + 1 from users", connection))
                    {
                        var result = await cmd.ExecuteScalarAsync();
                        newUserId = Convert.ToInt64(result);
                    }

                    string normalizedGender = gender?.ToLowerInvariant() switch
                    {
                        "male" => "male",
                        "female" => "female",
                        "unknown" => "none",
                        _ => "none"
                    };

                    var createParams = new UserCreateParams
                    {
                        UserId = newUserId,
                        UserName = username.Trim(),
                        Password = PasswordHasher.HashPassword(password),
                        Gender = normalizedGender,
                        ModerationStatus = "ok"
                    };

                    var repository = new UsersRepository();
                    await repository.CreateUserAsync(connectionString, createParams, failIfExists: true);

                    return (newUserId, true, null);
                }
            }
            catch (Exception ex)
            {
                ConsoleWindow.Instance?.WriteError($"Error creating user: {ex.Message}");
                return (0, false, ex.Message);
            }
        }

        public async Task<bool> SetMembershipAsync(long userId, short membershipStatus)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using var command = new NpgsqlCommand(
                        "UPDATE users SET membership_status = @status WHERE user_id = @userId", connection);
                    command.Parameters.AddWithValue("@status", membershipStatus);
                    command.Parameters.AddWithValue("@userId", userId);
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                ConsoleWindow.Instance?.WriteError($"Error setting membership for user {userId}: {ex.Message}");
                return false;
            }
        }

        public async Task<List<long>> GetAllUserIdsAsync()
        {
            var userIds = new List<long>();
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using var command = new NpgsqlCommand("SELECT user_id FROM users", connection);
                    using var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                        userIds.Add(reader.GetInt64(0));
                }
            }
            catch (Exception ex)
            {
                ConsoleWindow.Instance?.WriteError($"Error loading user ids: {ex.Message}");
            }
            return userIds;
        }

        public async Task<string> GetUserDescriptionAsync(long userId)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using var command = new NpgsqlCommand(
                        "SELECT description_bio FROM users WHERE user_id = @userId", connection);
                    command.Parameters.AddWithValue("@userId", userId);
                    var result = await command.ExecuteScalarAsync();
                    return result == null || result == DBNull.Value ? string.Empty : result.ToString();
                }
            }
            catch (Exception ex)
            {
                ConsoleWindow.Instance?.WriteError($"Error getting description for user {userId}: {ex.Message}");
                return string.Empty;
            }
        }

        public async Task<bool> UpdateUserDescriptionAsync(long userId, string description)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using var command = new NpgsqlCommand(
                        "UPDATE users SET description_bio = @description WHERE user_id = @userId", connection);
                    command.Parameters.AddWithValue("@description",
                        string.IsNullOrEmpty(description) ? (object)DBNull.Value : description);
                    command.Parameters.AddWithValue("@userId", userId);
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                ConsoleWindow.Instance?.WriteError($"Error updating description for user {userId}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetEmailVerifiedAsync(long userId, bool verified)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using var command = new NpgsqlCommand(
                        "UPDATE users SET email_verified = @verified WHERE user_id = @userId", connection);
                    command.Parameters.AddWithValue("@verified", verified);
                    command.Parameters.AddWithValue("@userId", userId);
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                ConsoleWindow.Instance?.WriteError($"Error setting email verified for user {userId}: {ex.Message}");
                return false;
            }
        }

        public async Task<string> GetGenderAsync(long userId)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using var command = new NpgsqlCommand(
                        "SELECT gender::text FROM users WHERE user_id = @userId", connection);
                    command.Parameters.AddWithValue("@userId", userId);
                    var result = await command.ExecuteScalarAsync();
                    return result == null || result == DBNull.Value ? "none" : result.ToString();
                }
            }
            catch (Exception ex)
            {
                ConsoleWindow.Instance?.WriteError($"Error getting gender for user {userId}: {ex.Message}");
                return "none";
            }
        }

        public async Task<bool> UpdateGenderAsync(long userId, string gender)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using var command = new NpgsqlCommand(
                        "UPDATE users SET gender = cast(@gender as gender_enum) WHERE user_id = @userId", connection);
                    command.Parameters.AddWithValue("@gender", gender ?? "none");
                    command.Parameters.AddWithValue("@userId", userId);
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                ConsoleWindow.Instance?.WriteError($"Error updating gender for user {userId}: {ex.Message}");
                return false;
            }
        }

        public async Task<DateTime?> GetBirthdayAsync(long userId)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using var command = new NpgsqlCommand(
                        "SELECT birthday FROM users WHERE user_id = @userId", connection);
                    command.Parameters.AddWithValue("@userId", userId);
                    var result = await command.ExecuteScalarAsync();
                    if (result == null || result == DBNull.Value) return null;
                    return (DateTime)result;
                }
            }
            catch (Exception ex)
            {
                ConsoleWindow.Instance?.WriteError($"Error getting birthday for user {userId}: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateBirthdayAsync(long userId, DateTime? birthday)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using var command = new NpgsqlCommand(
                        "UPDATE users SET birthday = @birthday WHERE user_id = @userId", connection);
                    command.Parameters.AddWithValue("@birthday", (object)birthday ?? DBNull.Value);
                    command.Parameters.AddWithValue("@userId", userId);
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                ConsoleWindow.Instance?.WriteError($"Error updating birthday for user {userId}: {ex.Message}");
                return false;
            }
        }

        private UserData MapReaderToUserData(NpgsqlDataReader reader)
        {
            return new UserData
            {
                UserId = reader.GetInt64(0),
                Username = reader.GetString(1),
                Email = reader.IsDBNull(2) ? null : reader.GetString(2),
                Gender = reader.IsDBNull(3) ? "none" : reader.GetString(3),
                CreatedAt = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                LastActivity = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5),
                AvatarThumbnailUrl = reader.IsDBNull(6) ? null : reader.GetString(6),
                HeadshotThumbnailUrl = reader.IsDBNull(7) ? null : reader.GetString(7),
                RobuxBalance = reader.IsDBNull(8) ? 0 : reader.GetInt64(8),
                TixBalance = reader.IsDBNull(9) ? 0 : reader.GetInt64(9),
                EmailVerified = !reader.IsDBNull(10) && reader.GetBoolean(10),
                Facebook = reader.IsDBNull(11) ? null : reader.GetString(11),
                Twitter = reader.IsDBNull(12) ? null : reader.GetString(12),
                GooglePlus = reader.IsDBNull(13) ? null : reader.GetString(13),
                YouTube = reader.IsDBNull(14) ? null : reader.GetString(14),
                Twitch = reader.IsDBNull(15) ? null : reader.GetString(15),
                SocialNetworksVisibility = reader.IsDBNull(16) ? (short)6 : reader.GetInt16(16)
            };
        }
    }
}