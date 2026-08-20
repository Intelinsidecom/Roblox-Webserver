using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Games;

/// <summary>
/// Database access for the universal points system.
/// Backs PointsService:GetPointBalance / GetGamePointBalance / AwardPoints.
/// Total balance lives on users.total_points; per-universe balance lives in user_universe_points.
/// </summary>
public static class Points
{
    private const long MaxBalance = int.MaxValue; // client casts every returned value to a 32-bit int

    public static async Task<long> GetUserTotalPointsAsync(string connectionString, long userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("connectionString is required", nameof(connectionString));
        if (userId <= 0)
            throw new ArgumentOutOfRangeException(nameof(userId));

        using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"SELECT total_points FROM users WHERE user_id = @userId";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("userId", userId);

        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        if (result == null || result == DBNull.Value)
            return 0;

        return Convert.ToInt64(result);
    }

    public static async Task<long> GetUniversePointsAsync(string connectionString, long userId, long universeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("connectionString is required", nameof(connectionString));
        if (userId <= 0)
            throw new ArgumentOutOfRangeException(nameof(userId));
        if (universeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(universeId));

        using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"SELECT points FROM user_universe_points WHERE user_id = @userId AND universe_id = @universeId";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("userId", userId);
        cmd.Parameters.AddWithValue("universeId", universeId);

        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        if (result == null || result == DBNull.Value)
            return 0;

        return Convert.ToInt64(result);
    }

    /// <summary>
    /// Awards points to a user within a universe, updating both the per-universe and the global total balance.
    /// Balances are clamped to int.MaxValue to stay within the client's 32-bit range.
    /// </summary>
    public static async Task<(bool Success, long PointsAwarded, long UserGameBalance, long UserTotalBalance, string? Error)> AwardPointsAsync(
        string connectionString, long userId, long universeId, long amount, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return (false, 0, 0, 0, "Connection string is required");
        if (userId <= 0)
            return (false, 0, 0, 0, "Invalid userId");
        if (universeId <= 0)
            return (false, 0, 0, 0, "Invalid universeId");
        if (amount <= 0)
            return (false, 0, 0, 0, "Amount must be a positive integer");

        using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        long oldTotal;
        long oldUniverse;
        using (var tx = conn.BeginTransaction())
        {
            try
            {
                using (var selectCmd = new NpgsqlCommand(@"SELECT total_points FROM users WHERE user_id = @userId", conn, tx))
                {
                    selectCmd.Parameters.AddWithValue("userId", userId);
                    var totalResult = await selectCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                    if (totalResult == null || totalResult == DBNull.Value)
                    {
                        tx.Rollback();
                        return (false, 0, 0, 0, "User not found");
                    }
                    oldTotal = Convert.ToInt64(totalResult);
                }

                using (var selectCmd = new NpgsqlCommand(@"SELECT points FROM user_universe_points WHERE user_id = @userId AND universe_id = @universeId", conn, tx))
                {
                    selectCmd.Parameters.AddWithValue("userId", userId);
                    selectCmd.Parameters.AddWithValue("universeId", universeId);
                    var universeResult = await selectCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                    oldUniverse = (universeResult == null || universeResult == DBNull.Value) ? 0 : Convert.ToInt64(universeResult);
                }

                var newUniverse = ClampToMax(oldUniverse + amount);
                var newTotal = ClampToMax(oldTotal + amount);
                var awarded = newUniverse - oldUniverse;

                using (var updateCmd = new NpgsqlCommand(@"UPDATE users SET total_points = @newTotal WHERE user_id = @userId", conn, tx))
                {
                    updateCmd.Parameters.AddWithValue("newTotal", newTotal);
                    updateCmd.Parameters.AddWithValue("userId", userId);
                    await updateCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                }

                using (var upsertCmd = new NpgsqlCommand(@"
                    INSERT INTO user_universe_points (user_id, universe_id, points)
                    VALUES (@userId, @universeId, @newUniverse)
                    ON CONFLICT (user_id, universe_id)
                    DO UPDATE SET points = EXCLUDED.points", conn, tx))
                {
                    upsertCmd.Parameters.AddWithValue("userId", userId);
                    upsertCmd.Parameters.AddWithValue("universeId", universeId);
                    upsertCmd.Parameters.AddWithValue("newUniverse", newUniverse);
                    await upsertCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                }

                tx.Commit();
                return (true, awarded, newUniverse, newTotal, null);
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
    }

    private static long ClampToMax(long value)
    {
        return value > MaxBalance ? MaxBalance : value;
    }
}
