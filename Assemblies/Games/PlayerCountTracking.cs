using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Games
{
    public static class PlayerCountTracking
    {
        public static async Task<int> GetUniversePlayerCountAsync(long universeId, IConfiguration configuration)
        {
            var arbiterUrl = configuration["ArbiterUrl"] ?? "http://localhost:5000";
            var connectionString = configuration.GetConnectionString("Default");

            try
            {
                var result = await GamesQueries.GetLivePlayerCountForUniverseAsync(
                    universeId, connectionString, arbiterUrl, false).ConfigureAwait(false);
                return result.TotalPlayers;
            }
            catch
            {
                return 0;
            }
        }

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

        public static async Task<int> GetPlacePlayerCountAsync(long placeId, IConfiguration configuration)
        {
            var arbiterUrl = configuration["ArbiterUrl"] ?? "http://localhost:5000";

            try
            {
                var result = await GamesQueries.GetLivePlayerCountForPlaceAsync(
                    (int)placeId, arbiterUrl, false).ConfigureAwait(false);
                return result.TotalPlayers;
            }
            catch
            {
                return 0;
            }
        }
    }
}
