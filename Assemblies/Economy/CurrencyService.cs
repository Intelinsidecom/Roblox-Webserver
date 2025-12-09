using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Economy
{
    /// <summary>
    /// Provides helpers for adjusting Robux or Tix balances.
    /// </summary>
    public sealed class CurrencyService
    {
        public enum CurrencyKind { Robux, Tix }

        /// <summary>
        /// Adds (or subtracts) Robux/Tix to a user balance in a single atomic statement.
        /// </summary>
        /// <param name="connectionString">Database connection string.</param>
        /// <param name="userId">User id.</param>
        /// <param name="delta">Positive or negative change.</param>
        /// <param name="currency">Currency kind.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public async Task<bool> AdjustBalanceAsync(string connectionString, long userId, long delta, CurrencyKind currency, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                return false;
            string column = currency == CurrencyKind.Robux ? "robux_balance" : "tix_balance";
            using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            using var tx = conn.BeginTransaction();
            string sql = $"update users set {column} = {column} + @d where user_id = @id";
            using var cmd = new NpgsqlCommand(sql, conn, tx);
            cmd.Parameters.AddWithValue("d", delta);
            cmd.Parameters.AddWithValue("id", userId);
            int affected = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            tx.Commit();
            return affected == 1;
        }
    }
}
