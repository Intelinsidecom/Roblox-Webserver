using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Common;

namespace Games
{
    /// <summary>
    /// Service for tracking user game presence and managing game join states
    /// </summary>
    public class GamePresenceService
    {
        private readonly string _connectionString;
        private readonly AuthenticationTicketService _ticketService;
        private readonly IConfiguration _configuration;

        public GamePresenceService(IConfiguration configuration, AuthenticationTicketService ticketService)
        {
            _connectionString = configuration.GetConnectionString("Default") 
                ?? throw new ArgumentNullException("Database connection string not configured");
            _ticketService = ticketService ?? throw new ArgumentNullException(nameof(ticketService));
            _configuration = configuration;
        }

        /// <summary>
        /// Records when a user successfully joins a game
        /// </summary>
        public async Task RecordGameJoinAsync(long userId, long placeId, string jobId, string ticketToken)
        {
            try
            {
                await _ticketService.MarkTicketAsUsedAsync(ticketToken);
                await InsertOrUpdateGamePresenceAsync(userId, placeId, jobId);
                await UpdateUserClientStatusAsync(userId, "Connected");
                
                await PlayerCountTracking.UpdateUserGameStatusAsync(userId, placeId, true, _configuration);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to record game join for user {userId}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Removes user from game presence (when they leave/disconnect)
        /// </summary>
        public async Task RemoveFromGameAsync(long userId)
        {
            try
            {
                long placeId = 0;
                
                using (var getConnection = new NpgsqlConnection(_connectionString))
                {
                    await getConnection.OpenAsync();
                    using var getCmd = new NpgsqlCommand("SELECT placeid FROM game_presence WHERE uid = @uid", getConnection);
                    getCmd.Parameters.AddWithValue("uid", userId);
                    var result = await getCmd.ExecuteScalarAsync();
                    if (result != null)
                    {
                        placeId = Convert.ToInt64(result);
                    }
                }

                using var deleteConn = new NpgsqlConnection(_connectionString);
                await deleteConn.OpenAsync();

                using var deleteCmd = new NpgsqlCommand("DELETE FROM game_presence WHERE uid = @uid", deleteConn);
                deleteCmd.Parameters.AddWithValue("uid", userId);
                await deleteCmd.ExecuteNonQueryAsync();

                await UpdateUserClientStatusAsync(userId, "Disconnected");
                
                await PlayerCountTracking.UpdateUserGameStatusAsync(userId, null, false, _configuration);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to remove user {userId} from game: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets the current game presence for a user
        /// </summary>
        public async Task<GamePresenceInfo?> GetUserGamePresenceAsync(long userId)
        {
            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new NpgsqlCommand(@"
                    SELECT placeid, jobid, created_at, updated_at 
                    FROM game_presence 
                    WHERE uid = @uid", conn);

                cmd.Parameters.AddWithValue("uid", userId);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var jobId = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                    return new GamePresenceInfo
                    {
                        UserId = userId,
                        PlaceId = reader.GetInt64(0),
                        JobId = jobId,
                        CreatedAt = reader.GetDateTime(2),
                        UpdatedAt = reader.GetDateTime(3),
                        IsInGame = !string.IsNullOrEmpty(jobId)
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get game presence for user {userId}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates user's client status (for the /client-status polling mechanism)
        /// </summary>
        public async Task UpdateUserClientStatusAsync(long userId, string status)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_connectionString) || userId <= 0)
                    return;

                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new NpgsqlCommand(@"
                    UPDATE users SET last_activity = NOW()
                    WHERE user_id = @userId", conn);
                cmd.Parameters.AddWithValue("userId", userId);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update client status for user {userId}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets all players currently in a specific place
        /// </summary>
        public async Task<System.Collections.Generic.List<GamePresenceInfo>> GetPlayersInPlaceAsync(long placeId)
        {
            var players = new System.Collections.Generic.List<GamePresenceInfo>();

            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new NpgsqlCommand(@"
                    SELECT uid, placeid, jobid, created_at, updated_at 
                    FROM game_presence 
                    WHERE placeid = @placeId
                    ORDER BY created_at DESC", conn);

                cmd.Parameters.AddWithValue("placeId", placeId);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var jobId = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                    players.Add(new GamePresenceInfo
                    {
                        UserId = reader.GetInt64(0),
                        PlaceId = reader.GetInt64(1),
                        JobId = jobId,
                        CreatedAt = reader.GetDateTime(3),
                        UpdatedAt = reader.GetDateTime(4),
                        IsInGame = !string.IsNullOrEmpty(jobId)
                    });
                }

                return players;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get players in place {placeId}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Cleans up stale game presence entries (older than specified timeout)
        /// Properly decrements player counts for removed users
        /// </summary>
        public async Task CleanupStalePresenceAsync(TimeSpan maxAge)
        {
            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                var staleEntries = new List<(long userId, long placeId)>();
                const string selectSql = @"
                    SELECT uid, placeid FROM game_presence 
                    WHERE updated_at < NOW() - INTERVAL '1 second' * @seconds";
                
                using (var selectCmd = new NpgsqlCommand(selectSql, conn))
                {
                    selectCmd.Parameters.AddWithValue("seconds", (long)maxAge.TotalSeconds);
                    using var reader = await selectCmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        staleEntries.Add((reader.GetInt64(0), reader.GetInt64(1)));
                    }
                }

                if (staleEntries.Count == 0)
                    return;


                const string deleteSql = @"
                    DELETE FROM game_presence 
                    WHERE updated_at < NOW() - INTERVAL '1 second' * @seconds";
                using (var deleteCmd = new NpgsqlCommand(deleteSql, conn))
                {
                    deleteCmd.Parameters.AddWithValue("seconds", (long)maxAge.TotalSeconds);
                    await deleteCmd.ExecuteNonQueryAsync();
                }

                foreach (var (userId, placeId) in staleEntries)
                {
                    await PlayerCountTracking.UpdateUserGameStatusAsync(userId, null, false, _configuration);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to cleanup stale game presence: {ex.Message}", ex);
            }
        }

        private async Task InsertOrUpdateGamePresenceAsync(long userId, long placeId, string jobId)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(@"
                INSERT INTO game_presence (uid, placeid, jobid, created_at, updated_at, last_ping)
                VALUES (@uid, @placeid, @jobid, NOW(), NOW(), NOW())
                ON CONFLICT (uid) 
                DO UPDATE SET 
                    placeid = EXCLUDED.placeid,
                    jobid = EXCLUDED.jobid,
                    updated_at = NOW(),
                    last_ping = EXCLUDED.last_ping", conn);

            cmd.Parameters.AddWithValue("uid", userId);
            cmd.Parameters.AddWithValue("placeid", placeId);
            cmd.Parameters.AddWithValue("jobid", jobId);

            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Logs a player as joined to a game (convenience method)
        /// </summary>
        public async Task LogPlayerAsync(long userId, long placeId, string jobId)
        {
            try
            {
                var ticket = await _ticketService.CreateGeneralTicketAsync(userId);
                
                if (ticket != null)
                {
                    await RecordGameJoinAsync(userId, placeId, jobId, ticket.TicketToken);
                }
                else
                {
                    throw new InvalidOperationException($"Failed to create ticket for user {userId}");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to log player {userId} join to place {placeId}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates a player's last activity timestamp (for keep-alive functionality)
        /// </summary>
        public async Task UpdatePlayerActivityAsync(long userId, long placeId)
        {
            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new NpgsqlCommand("UPDATE game_presence SET updated_at = NOW(), last_ping = NOW() WHERE uid = @uid AND placeid = @placeid", conn);
                cmd.Parameters.AddWithValue("uid", userId);
                cmd.Parameters.AddWithValue("placeid", placeId);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update player activity for user {userId}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates player heartbeat (called by ClientPresence endpoint)
        /// </summary>
        public async Task UpdatePlayerHeartbeatAsync(long userId)
        {
            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new NpgsqlCommand("UPDATE game_presence SET last_ping = NOW() WHERE uid = @uid", conn);
                cmd.Parameters.AddWithValue("uid", userId);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update player heartbeat for user {userId}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets the number of players in a specific game server (by job ID) - only counts active players with recent pings (30 sec timeout)
        /// </summary>
        public async Task<int> GetPlayerCountByJobIdAsync(string jobId)
        {
            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM game_presence WHERE jobid = @jobid AND jobid != '' AND (last_ping + INTERVAL '30 seconds') > NOW()", conn);
                cmd.Parameters.AddWithValue("jobid", jobId);
                
                var count = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(count ?? 0);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get player count for job {jobId}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets the number of players in a place - only counts active players with recent pings (30 sec timeout)
        /// </summary>
        public async Task<int> GetActivePlayerCountByPlaceAsync(long placeId)
        {
            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM game_presence WHERE placeid = @placeId AND (last_ping + INTERVAL '30 seconds') > NOW()", conn);
                cmd.Parameters.AddWithValue("placeId", placeId);
                
                var count = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(count ?? 0);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get active player count for place {placeId}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets a paginated list of players in a place with user info (usernames).
        /// Uses LIMIT/OFFSET for efficient large result sets.
        /// </summary>
        public async Task<(int TotalCount, List<GamePresenceWithUser> Players)> GetPlayersInPlacePaginatedAsync(
            long placeId, int startRow = 0, int maxRows = 10)
        {
            var players = new List<GamePresenceWithUser>();

            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                // Get total count first
                using var countCmd = new NpgsqlCommand(@"
                    SELECT COUNT(*) FROM game_presence 
                    WHERE placeid = @placeId 
                    AND (last_ping + INTERVAL '30 seconds') > NOW()", conn);
                countCmd.Parameters.AddWithValue("placeId", placeId);
                var totalCount = Convert.ToInt32(await countCmd.ExecuteScalarAsync() ?? 0);

                // Get paginated results with user info joined
                using var cmd = new NpgsqlCommand(@"
                    SELECT gp.uid, gp.placeid, gp.jobid, gp.created_at, gp.updated_at,
                           u.user_name
                    FROM game_presence gp
                    LEFT JOIN users u ON u.user_id = gp.uid
                    WHERE gp.placeid = @placeId
                    AND (gp.last_ping + INTERVAL '30 seconds') > NOW()
                    ORDER BY gp.created_at DESC
                    LIMIT @limit OFFSET @offset", conn);

                cmd.Parameters.AddWithValue("placeId", placeId);
                cmd.Parameters.AddWithValue("limit", maxRows);
                cmd.Parameters.AddWithValue("offset", startRow);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var jobId = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                    players.Add(new GamePresenceWithUser
                    {
                        UserId = reader.GetInt64(0),
                        PlaceId = reader.GetInt64(1),
                        JobId = jobId,
                        CreatedAt = reader.GetDateTime(3),
                        UpdatedAt = reader.GetDateTime(4),
                        UserName = reader.IsDBNull(5) ? "" : reader.GetString(5)
                    });
                }

                return (totalCount, players);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get paginated players for place {placeId}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Returns which of the given friends are currently in the specified place, and which server (jobid) they are in.
        /// </summary>
        public static async Task<List<FriendServerEntry>> GetFriendsInPlaceAsync(
            string connectionString, long placeId, List<long> friendIds,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString) || friendIds == null || friendIds.Count == 0)
                return new List<FriendServerEntry>();

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var cmd = new NpgsqlCommand(@"
                SELECT gp.uid, gp.jobid
                FROM game_presence gp
                WHERE gp.placeid = @placeId
                  AND gp.uid = ANY(@friendIds)
                  AND (gp.last_ping + INTERVAL '30 seconds') > NOW()
                ORDER BY gp.updated_at DESC", conn);

            cmd.Parameters.AddWithValue("placeId", placeId);
            cmd.Parameters.AddWithValue("friendIds", friendIds.ToArray());

            var results = new List<FriendServerEntry>();
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                results.Add(new FriendServerEntry
                {
                    UserId = reader.GetInt64(0),
                    JobId = reader.GetString(1)
                });
            }

            return results;
        }
    }

    public class GamePresenceInfo
    {
        public long UserId { get; set; }
        public long PlaceId { get; set; }
        public string JobId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsInGame { get; set; }
    }

    public class GamePresenceWithUser
    {
        public long UserId { get; set; }
        public long PlaceId { get; set; }
        public string JobId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string UserName { get; set; } = string.Empty;
    }

    public class FriendServerEntry
    {
        public long UserId { get; set; }
        public string JobId { get; set; } = string.Empty;
    }
}
