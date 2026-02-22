using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Npgsql;
using Control_Panel;

namespace ControlPanel.Functions
{
    public class UserSearchResult
    {
        public long Id { get; set; }
        public string Username { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastActivity { get; set; }
        public bool IsPremium { get; set; }
        public string MembershipType { get; set; }
    }

    public class UserSearchService
    {
        private readonly string connectionString;

        public UserSearchService(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));
            
            this.connectionString = connectionString;
        }

        public async Task<List<UserSearchResult>> SearchUsersByUsernameAsync(string username, int limit = 50)
        {
            var results = new List<UserSearchResult>();

            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string query = @"
                        SELECT user_id, user_name, description_bio, user_created, last_activity, premium_member, subscription_type
                        FROM users 
                        WHERE LOWER(user_name) LIKE LOWER(@username) 
                        ORDER BY user_name
                        LIMIT @limit";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", $"%{username}%");
                        command.Parameters.AddWithValue("@limit", limit);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                results.Add(new UserSearchResult
                                {
                                    Id = reader.GetInt64(0),
                                    Username = reader.GetString(1),
                                    Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                    CreatedAt = reader.GetDateTime(3),
                                    LastActivity = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                                    IsPremium = reader.GetBoolean(5),
                                    MembershipType = reader.IsDBNull(6) ? "None" : reader.GetString(6)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleWindow.Instance?.WriteError($"Error searching users by username: {ex.Message}");
                throw;
            }

            return results;
        }
    }
}
