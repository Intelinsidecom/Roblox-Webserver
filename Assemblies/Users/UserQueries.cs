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
    }
}
