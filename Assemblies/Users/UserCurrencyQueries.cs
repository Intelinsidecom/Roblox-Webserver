using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Users
{
    public static partial class UserQueries
    {
        /// <summary>
        /// Gets the Robux or Tix balance for a user by id.
        /// </summary>
        /// <param name="connectionString">Connection string to the Postgres database.</param>
        /// <param name="userId">User id.</param>
        /// <param name="currency">"robux" or "tix" (case-insensitive).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Balance amount, or 0 if user not found.</returns>
        public static async Task<long> GetCurrencyByIdAsync(string connectionString, long userId, string currency, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                return 0;
            currency = currency?.ToLowerInvariant();
            string column = currency switch
            {
                "robux" => "robux_balance",
                "tix" or "tickets" => "tix_balance",
                _ => throw new ArgumentException("currency must be 'robux' or 'tix'", nameof(currency))
            };

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            using var cmd = new NpgsqlCommand($"select coalesce({column},0) from users where user_id=@id", conn);
            cmd.Parameters.AddWithValue("id", userId);
            var obj = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            return obj is long l ? l : (obj is int i ? i : 0);
        }

        /// <summary>
        /// Convenience wrapper: get Robux balance by username.
        /// </summary>
        public static async Task<long> GetCurrencyByUserNameAsync(string connectionString, string userName, string currency, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("userName is required", nameof(userName));
            currency = currency?.ToLowerInvariant();
            string column = currency switch
            {
                "robux" => "robux_balance",
                "tix" or "tickets" => "tix_balance",
                _ => throw new ArgumentException("currency must be 'robux' or 'tix'", nameof(currency))
            };

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            using var cmd = new NpgsqlCommand($"select coalesce({column},0) from users where user_name=@name", conn);
            cmd.Parameters.AddWithValue("name", userName);
            var obj = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            return obj is long l ? l : (obj is int i ? i : 0);
        }
    }
}
