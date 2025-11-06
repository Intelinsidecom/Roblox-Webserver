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

            using var cmd = new NpgsqlCommand("select coalesce(robux_balance, 0) from users where user_id = @id", conn);
            cmd.Parameters.AddWithValue("id", userId);
            var obj = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            long robux = 0;
            if (obj != null && obj != DBNull.Value)
            {
                try { robux = Convert.ToInt64(obj); } catch { robux = 0; }
            }

            // Tickets currency no longer exists; return 0 to satisfy legacy clients
            return new UserBalance { Robux = robux, Tickets = 0 };
        }
    }
}
