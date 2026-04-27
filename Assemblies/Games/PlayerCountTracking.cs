using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Games
{
    /// <summary>
    /// Static methods for tracking player counts across places and universes
    /// </summary>
    public static class PlayerCountTracking
    {
        /// <summary>
        /// Gets the total player count for a universe by summing all place player counts
        /// </summary>
        public static async Task<int> GetUniversePlayerCountAsync(long universeId, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Default");
            if (string.IsNullOrEmpty(connectionString))
                return 0;

            try
            {
                using var conn = new NpgsqlConnection(connectionString);
                await conn.OpenAsync();

                const string sql = @"
                    SELECT COALESCE(SUM(a.player_count), 0)
                    FROM assets a
                    INNER JOIN universes u ON a.asset_id = ANY(u.place_ids)
                    WHERE u.universe_id = @universeId AND a.is_place = true";

                using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("universeId", universeId);

                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result ?? 0);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Updates user's in-game status and current place
        /// </summary>
        public static async Task UpdateUserGameStatusAsync(long userId, long? placeId, bool inGame, IConfiguration configuration)
        {
            if (userId <= 0)
                return;

            var connectionString = configuration.GetConnectionString("Default");
            if (string.IsNullOrEmpty(connectionString))
                return;

            try
            {
                using var conn = new NpgsqlConnection(connectionString);
                await conn.OpenAsync();

                const string sql = @"
                    UPDATE users 
                    SET in_game = @inGame, 
                        current_place_id = @placeId
                    WHERE user_id = @userId";

                using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("userId", userId);
                cmd.Parameters.AddWithValue("inGame", inGame);
                cmd.Parameters.AddWithValue("placeId", placeId.HasValue ? (object)placeId.Value : DBNull.Value);

                await cmd.ExecuteNonQueryAsync();
            }
            catch
            {
            }
        }

        /// <summary>
        /// Recalculates player count for a place from active game_presence entries
        /// </summary>
        public static async Task RecalculatePlacePlayerCountAsync(long placeId, IConfiguration configuration)
        {
            if (placeId <= 0)
                return;

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
                    const string countSql = @"
                        SELECT COUNT(*) 
                        FROM game_presence 
                        WHERE placeid = @placeId 
                        AND (last_ping + INTERVAL '30 seconds') > NOW()";

                    int playerCount;
                    using (var cmd = new NpgsqlCommand(countSql, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("placeId", placeId);
                        var result = await cmd.ExecuteScalarAsync();
                        playerCount = Convert.ToInt32(result ?? 0);
                    }

                    const string updateSql = @"
                        UPDATE assets 
                        SET player_count = @playerCount 
                        WHERE asset_id = @placeId AND is_place = true";

                    using (var cmd = new NpgsqlCommand(updateSql, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("playerCount", playerCount);
                        cmd.Parameters.AddWithValue("placeId", placeId);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch
            {
                // Silent fail
            }
        }

        /// <summary>
        /// Gets the player count for a specific place
        /// </summary>
        public static async Task<int> GetPlacePlayerCountAsync(long placeId, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Default");
            if (string.IsNullOrEmpty(connectionString))
                return 0;

            try
            {
                using var conn = new NpgsqlConnection(connectionString);
                await conn.OpenAsync();

                const string sql = "SELECT player_count FROM assets WHERE asset_id = @placeId AND is_place = true";

                using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("placeId", placeId);

                var result = await cmd.ExecuteScalarAsync();
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch
            {
                return 0;
            }
        }
    }
}
