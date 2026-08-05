using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Users
{
    /// <summary>
    /// Email-related data access (users.email, email_verified, and the
    /// email_verification_tokens table). Mirrors the style of UserQueries.
    /// </summary>
    public static class EmailQueries
    {
        /// <summary>
        /// Returns the email address currently stored for the user, or null if
        /// the user has no email set or does not exist.
        /// </summary>
        public static async Task<string?> GetEmailAsync(
            string connectionString,
            long userId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                return null;

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            using var cmd = new NpgsqlCommand(
                "select email from users where user_id = @uid", conn);
            cmd.Parameters.AddWithValue("uid", userId);
            var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            return result == null || result is DBNull ? null : Convert.ToString(result);
        }

        /// <summary>
        /// True if the user's email_verified flag is true. Returns false on any
        /// error or missing user.
        /// </summary>
        public static async Task<bool> IsEmailVerifiedAsync(
            string connectionString,
            long userId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString) || userId <= 0)
                return false;

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            using var cmd = new NpgsqlCommand(
                "select email_verified from users where user_id = @uid", conn);
            cmd.Parameters.AddWithValue("uid", userId);
            var v = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            return v != null && v != DBNull.Value && Convert.ToBoolean(v);
        }

        /// <summary>
        /// Updates users.email, clears email_verified, and refreshes
        /// password_last_changed_at (coalesce so an existing timestamp isn't
        /// blown away). Used by the change-email flow after the password has
        /// been verified by the caller.
        /// </summary>
        public static async Task UpdateEmailAsync(
            string connectionString,
            long userId,
            string newEmail,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId), "userId must be positive");
            if (string.IsNullOrWhiteSpace(newEmail))
                throw new ArgumentException("newEmail is required", nameof(newEmail));

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            using var cmd = new NpgsqlCommand(
                @"update users
                     set email = @e,
                         email_verified = false,
                         password_last_changed_at = coalesce(password_last_changed_at, now())
                   where user_id = @uid", conn);
            cmd.Parameters.AddWithValue("e", newEmail);
            cmd.Parameters.AddWithValue("uid", userId);
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Marks email_verified = true for the given user. Does NOT touch the
        /// email column; the caller is expected to have already persisted the
        /// new address via UpdateEmailAsync.
        /// </summary>
        public static async Task MarkEmailVerifiedAsync(
            string connectionString,
            long userId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString) || userId <= 0)
                return;

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            using var cmd = new NpgsqlCommand(
                "update users set email_verified = true where user_id = @uid", conn);
            cmd.Parameters.AddWithValue("uid", userId);
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates (or replaces) a verification token for a (userId, email)
        /// pair. The unique constraint on the table ensures there is at most
        /// one outstanding token per pair, so repeated "resend verification
        /// email" clicks rotate the token rather than insert duplicates.
        /// Returns the token that was written.
        /// </summary>
        public static async Task<string> UpsertVerificationTokenAsync(
            string connectionString,
            long userId,
            string email,
            string token,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId), "userId must be positive");
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("email is required", nameof(email));
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("token is required", nameof(token));

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            using var cmd = new NpgsqlCommand(
                @"INSERT INTO email_verification_tokens (token, user_id, email, expires_at)
                      VALUES (@token, @uid, @email, now() + interval '24 hours')
                      ON CONFLICT (user_id, email) DO UPDATE
                        SET token = EXCLUDED.token,
                            expires_at = EXCLUDED.expires_at", conn);
            cmd.Parameters.AddWithValue("token", token);
            cmd.Parameters.AddWithValue("uid", userId);
            cmd.Parameters.AddWithValue("email", email);
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            return token;
        }

        /// <summary>
        /// Looks up a pending verification token. Returns null if no row exists.
        /// </summary>
        public static async Task<VerificationTokenInfo?> GetVerificationTokenAsync(
            string connectionString,
            string token,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(token))
                return null;

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            using var cmd = new NpgsqlCommand(
                @"SELECT user_id, email, expires_at
                    FROM email_verification_tokens
                   WHERE token = @token", conn);
            cmd.Parameters.AddWithValue("token", token);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                return null;

            return new VerificationTokenInfo
            {
                UserId = reader.GetInt64(0),
                Email = reader.GetString(1),
                ExpiresAt = reader.GetDateTime(2)
            };
        }

        /// <summary>
        /// Deletes a single verification token row. Safe to call with a token
        /// that no longer exists (no-op in that case).
        /// </summary>
        public static async Task DeleteVerificationTokenAsync(
            string connectionString,
            string token,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(token))
                return;

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            using var cmd = new NpgsqlCommand(
                "DELETE FROM email_verification_tokens WHERE token = @token", conn);
            cmd.Parameters.AddWithValue("token", token);
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Lightweight DTO returned by GetVerificationTokenAsync. Keeping it a
    /// separate type lets callers read the fields without re-querying.
    /// </summary>
    public sealed class VerificationTokenInfo
    {
        public long UserId { get; set; }
        public string Email { get; set; } = "";
        public DateTime ExpiresAt { get; set; }
    }
}
