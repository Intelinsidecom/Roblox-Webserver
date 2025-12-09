using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Users
{
    public sealed class BalancesRepository
    {
        public sealed class UserBalance
        {
            public long Robux { get; set; }
            public long Tickets { get; set; }
        }

        public async Task<UserBalance> GetUserBalanceAsync(string connectionString, long userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                return new UserBalance { Robux = 0, Tickets = 0 };
            using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var cmd = new NpgsqlCommand("select coalesce(robux_balance, 0), coalesce(tix_balance, 0) from users where user_id = @id", conn);
            cmd.Parameters.AddWithValue("id", userId);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            long robux = 0;
            long tickets = 0;
            if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                if (!reader.IsDBNull(0)) robux = reader.GetInt64(0);
                if (!reader.IsDBNull(1)) tickets = reader.GetInt64(1);
            }

            return new UserBalance { Robux = robux, Tickets = tickets };
        }
    }
}
