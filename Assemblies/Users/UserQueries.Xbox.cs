using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Common;

namespace Users
{
    public sealed class XboxAccountCreateResult
    {
        public long UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public bool Created { get; set; }
    }

    public static partial class UserQueries
    {
        public static async Task<long?> GetUserIdByXuidAsync(string connectionString, string xuid, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (string.IsNullOrWhiteSpace(xuid))
                return null;

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var cmd = new NpgsqlCommand(
                "select user_id from users where xbox_xuid = @xuid and xbox_xuid <> '' limit 1", conn);
            cmd.Parameters.AddWithValue("xuid", xuid);

            var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            return result == null || result is DBNull ? null : Convert.ToInt64(result);
        }

        public static async Task<Dictionary<string, long>> GetUserIdByXuidsAsync(string connectionString, IReadOnlyCollection<string> xuids, CancellationToken cancellationToken = default)
        {
            var map = new Dictionary<string, long>();
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (xuids == null || xuids.Count == 0)
                return map;

            var cleaned = new List<string>();
            foreach (var x in xuids)
            {
                if (!string.IsNullOrWhiteSpace(x) && !cleaned.Contains(x))
                    cleaned.Add(x);
            }
            if (cleaned.Count == 0)
                return map;

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var cmd = new NpgsqlCommand(
                "select xbox_xuid, user_id from users where xbox_xuid = any(@xuids) and xbox_xuid <> ''", conn);
            cmd.Parameters.AddWithValue("xuids", cleaned.ToArray());

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var key = reader.IsDBNull(0) ? null : reader.GetString(0);
                if (!string.IsNullOrEmpty(key) && !map.ContainsKey(key))
                    map[key] = reader.GetInt64(1);
            }
            return map;
        }

        public static async Task<string?> GetUserXuidAsync(string connectionString, long userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                return null;

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var cmd = new NpgsqlCommand(
                "select xbox_xuid from users where user_id = @uid", conn);
            cmd.Parameters.AddWithValue("uid", userId);

            var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            var xuid = result == null || result is DBNull ? null : result.ToString();
            return string.IsNullOrWhiteSpace(xuid) ? null : xuid;
        }

        public static async Task<bool> IsXboxLinkedAsync(string connectionString, long userId, CancellationToken cancellationToken = default)
        {
            return await GetUserXuidAsync(connectionString, userId, cancellationToken).ConfigureAwait(false) != null;
        }

        public static async Task<bool> LinkXuidToUserAsync(string connectionString, long userId, string xuid, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId));
            if (string.IsNullOrWhiteSpace(xuid))
                throw new ArgumentException("xuid is required", nameof(xuid));

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var cmd = new NpgsqlCommand(
                "update users set xbox_xuid = @xuid, xbox_user = true where user_id = @uid", conn);
            cmd.Parameters.AddWithValue("xuid", xuid);
            cmd.Parameters.AddWithValue("uid", userId);

            try
            {
                var rows = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                return rows > 0;
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                // Another row already holds this xuid.
                return false;
            }
        }

        public static async Task<bool> UnlinkXuidAsync(string connectionString, string xuid, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (string.IsNullOrWhiteSpace(xuid))
                return false;

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var cmd = new NpgsqlCommand(
                "update users set xbox_xuid = null, xbox_user = false where xbox_xuid = @xuid and xbox_xuid <> ''", conn);
            cmd.Parameters.AddWithValue("xuid", xuid);

            var rows = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            return rows > 0;
        }

        public static async Task<bool> UnlinkXboxForUserAsync(string connectionString, long userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                return false;

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var cmd = new NpgsqlCommand(
                "update users set xbox_xuid = null, xbox_user = false where user_id = @uid", conn);
            cmd.Parameters.AddWithValue("uid", userId);

            var rows = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            return rows > 0;
        }

        public static async Task<bool> SetUserCredentialsAsync(string connectionString, long userId, string username, string hashedPassword, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId));
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("username is required", nameof(username));
            if (string.IsNullOrWhiteSpace(hashedPassword))
                throw new ArgumentException("hashedPassword is required", nameof(hashedPassword));

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            // Uniqueness of user_name is enforced app-level (no unique index on the
            // column), so guard with NOT EXISTS; a losing race updates 0 rows.
            using var cmd = new NpgsqlCommand(@"
update users
set user_name = @name, password = @pwd
where user_id = @uid
  and not exists (select 1 from users where lower(user_name) = lower(@name) and user_id <> @uid)", conn);
            cmd.Parameters.AddWithValue("name", username);
            cmd.Parameters.AddWithValue("pwd", hashedPassword);
            cmd.Parameters.AddWithValue("uid", userId);

            try
            {
                var rows = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                return rows > 0;
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                return false;
            }
        }

        public static async Task<XboxAccountCreateResult> GetOrCreateXboxAccountAsync(string connectionString, string xuid, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (string.IsNullOrWhiteSpace(xuid))
                throw new ArgumentException("xuid is required", nameof(xuid));

            var existingId = await GetUserIdByXuidAsync(connectionString, xuid, cancellationToken).ConfigureAwait(false);
            if (existingId.HasValue)
            {
                var existingName = await GetUserNameByIdAsync(connectionString, existingId.Value, cancellationToken).ConfigureAwait(false);
                return new XboxAccountCreateResult
                {
                    UserId = existingId.Value,
                    UserName = existingName ?? string.Empty,
                    Created = false
                };
            }

            var repo = new UsersRepository();

            var placeholder = await AllocatePlaceholderNameAsync(connectionString, xuid, cancellationToken).ConfigureAwait(false);

            long newUserId;
            await using (var conn = new NpgsqlConnection(connectionString))
            {
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
                using var cmd = new NpgsqlCommand("select coalesce(max(user_id), 0) + 1 from users", conn);
                newUserId = Convert.ToInt64(await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false));
            }

            await repo.CreateUserAsync(connectionString, new UserCreateParams
            {
                UserId = newUserId,
                UserName = placeholder,
                Password = null,
                Email = null
            }, failIfExists: true, cancellationToken).ConfigureAwait(false);

            if (!await LinkXuidToUserAsync(connectionString, newUserId, xuid, cancellationToken).ConfigureAwait(false))
            {
                // Lost a race with another connect for the same xuid; reuse that row.
                var racedId = await GetUserIdByXuidAsync(connectionString, xuid, cancellationToken).ConfigureAwait(false);
                if (racedId.HasValue)
                {
                    var racedName = await GetUserNameByIdAsync(connectionString, racedId.Value, cancellationToken).ConfigureAwait(false);
                    return new XboxAccountCreateResult
                    {
                        UserId = racedId.Value,
                        UserName = racedName ?? string.Empty,
                        Created = false
                    };
                }
                throw new InvalidOperationException($"Failed to link xuid to newly created user {newUserId}");
            }

            return new XboxAccountCreateResult
            {
                UserId = newUserId,
                UserName = placeholder,
                Created = true
            };
        }

        private static async Task<string> AllocatePlaceholderNameAsync(string connectionString, string xuid, CancellationToken cancellationToken)
        {
            var digits = new System.Text.StringBuilder();
            foreach (var c in xuid)
            {
                if (char.IsDigit(c))
                    digits.Append(c);
            }
            var suffix = digits.Length > 0 ? digits.ToString() : xuid;
            if (suffix.Length > 11)
                suffix = suffix.Substring(suffix.Length - 11);

            var baseName = "XboxUser_" + suffix;
            if (baseName.Length > 20)
                baseName = baseName.Substring(0, 20);

            var candidate = baseName;
            var attempt = 0;
            while (await UsernameExistsAsync(connectionString, candidate, cancellationToken).ConfigureAwait(false))
            {
                attempt++;
                candidate = baseName.Length > 18
                    ? baseName.Substring(0, 18) + attempt.ToString()
                    : baseName + "_" + attempt.ToString();
            }
            return candidate;
        }
    }
}
