using System;
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
        
        public string MembershipText
        {
            get
            {
                return "Free";
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
                            robux_balance, tix_balance
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
                TixBalance = reader.IsDBNull(9) ? 0 : reader.GetInt64(9)
            };
        }
    }
}