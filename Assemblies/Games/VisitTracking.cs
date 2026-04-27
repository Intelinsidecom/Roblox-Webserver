using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Games
{
    /// <summary>
    /// Static methods for tracking universe visits
    /// </summary>
    public static class VisitTracking
    {
        private static readonly ConcurrentDictionary<long, DateTime> _lastVisitTime = new ConcurrentDictionary<long, DateTime>();
        private static readonly TimeSpan _cooldownPeriod = TimeSpan.FromSeconds(20);
        /// <summary>
        /// Records a user visit to a universe.
        /// Increments the universe visit count and adds/moves the universe to the user's visited_universes array (at the front).
        /// </summary>
        public static async Task RecordVisitAsync(long userId, long universeId, IConfiguration configuration)
        {
            if (userId <= 0 || universeId <= 0)
                return;

            var now = DateTime.UtcNow;
            if (_lastVisitTime.TryGetValue(userId, out var lastVisit))
            {
                if (now - lastVisit < _cooldownPeriod)
                    return;
            }

            var connectionString = configuration.GetConnectionString("Default");
            if (string.IsNullOrEmpty(connectionString))
                return;

            try
            {
                using var conn = new NpgsqlConnection(connectionString);
                await conn.OpenAsync();

                using var transaction = conn.BeginTransaction();
                
                try
                {
                    const string incrementUniverseSql = @"
                        UPDATE universes 
                        SET visit_count = visit_count + 1 
                        WHERE universe_id = @universeId";
                    
                    using (var cmd = new NpgsqlCommand(incrementUniverseSql, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("universeId", universeId);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    const string updateUserSql = @"
                        UPDATE users 
                        SET visited_universes = array_prepend(
                            @universeId, 
                            array_remove(visited_universes, @universeId)
                        )
                        WHERE user_id = @userId";
                    
                    using (var cmd = new NpgsqlCommand(updateUserSql, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("universeId", universeId);
                        cmd.Parameters.AddWithValue("userId", userId);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    transaction.Commit();
                    _lastVisitTime[userId] = DateTime.UtcNow;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VisitTracking] Error recording visit for user {userId}, universe {universeId}: {ex.Message}");
            }
        }
        
        public static async Task<bool> HasVisitedAsync(long userId, long universeId, string connectionString)
        {
            if (userId <= 0 || universeId <= 0 || string.IsNullOrEmpty(connectionString))
                return false;

            try
            {
                using var conn = new NpgsqlConnection(connectionString);
                await conn.OpenAsync();

                using var cmd = new NpgsqlCommand("SELECT 1 FROM users WHERE user_id = @userId AND @universeId = ANY(visited_universes)", conn);
                cmd.Parameters.AddWithValue("userId", userId);
                cmd.Parameters.AddWithValue("universeId", universeId);
                
                var result = await cmd.ExecuteScalarAsync();
                return result != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VisitTracking] Error checking visit for user {userId}, universe {universeId}: {ex.Message}");
                return false;
            }
        }
    }
}
