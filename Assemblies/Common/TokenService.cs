using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching;
using Npgsql;

namespace Games
{
    /// <summary>
    /// Unified token service handling both session tokens (.ROBLOSECURITY) and game tickets.
    /// Game tickets are stored in memory cache with automatic expiration.
    /// Session tokens are stored in database with long-term persistence.
    /// </summary>
    public class TokenService
    {
        private readonly string _connectionString;
        private readonly IMemoryCache _memoryCache;
        private readonly MemoryCacheEntryOptions _ticketCacheOptions;

        public TokenService(IConfiguration configuration, IMemoryCache memoryCache)
        {
            _connectionString = configuration.GetConnectionString("Default") 
                ?? throw new ArgumentNullException("Database connection string not configured");
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _ticketCacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(15))
                .SetPriority(CacheItemPriority.Normal);
        }

        /// <summary>
        /// Creates a persistent session token for web authentication.
        /// These tokens are stored in the database and last for 1 year.
        /// </summary>
        public async Task<string> CreateSessionAsync(long userId, string? ipAddress = null)
        {
            var token = GenerateToken("sess_");
            var expiresAt = DateTimeOffset.UtcNow.AddYears(1);

            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(@"
                INSERT INTO sessions (token, user_id, created_at, expires_at, last_ip)
                VALUES (@token, @user_id, NOW(), @expires_at, @ip)
                ON CONFLICT (token) DO UPDATE SET
                    user_id = EXCLUDED.user_id,
                    expires_at = EXCLUDED.expires_at,
                    last_ip = EXCLUDED.last_ip", conn);

            cmd.Parameters.AddWithValue("token", token);
            cmd.Parameters.AddWithValue("user_id", userId);
            cmd.Parameters.AddWithValue("expires_at", expiresAt.UtcDateTime);
            cmd.Parameters.AddWithValue("ip", ipAddress != null ? (object)IPAddress.Parse(ipAddress) : DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
            return token;
        }

        /// <summary>
        /// Validates a session token and returns the user ID if valid.
        /// Also extends the session expiration on successful validation.
        /// </summary>
        public async Task<long?> ValidateSessionAsync(string token)
        {
            if (string.IsNullOrEmpty(token) || !token.StartsWith("sess_"))
                return null;

            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(@"
                SELECT user_id FROM sessions 
                WHERE token = @token 
                AND (expires_at IS NULL OR expires_at > NOW() AT TIME ZONE 'utc')
                LIMIT 1", conn);
            
            cmd.Parameters.AddWithValue("token", token);
            var result = await cmd.ExecuteScalarAsync();
            
            if (result is long userId)
            {
                await ExtendSessionAsync(token);
                return userId;
            }

            return null;
        }

        /// <summary>
        /// Extends session expiration by updating last activity.
        /// </summary>
        public async Task ExtendSessionAsync(string token)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(@"
                UPDATE sessions 
                SET expires_at = NOW() AT TIME ZONE 'utc' + INTERVAL '1 year'
                WHERE token = @token", conn);
            
            cmd.Parameters.AddWithValue("token", token);
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Revokes a session token immediately.
        /// </summary>
        public async Task RevokeSessionAsync(string token)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(@"
                DELETE FROM sessions WHERE token = @token", conn);
            
            cmd.Parameters.AddWithValue("token", token);
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Creates a short-lived game ticket stored in memory cache.
        /// These tickets are for single-use game joins and expire after 15 minutes.
        /// </summary>
        public async Task<string> CreateGameTicketAsync(long userId, long placeId, string? jobId = null)
        {
            var token = GenerateToken("game_");
            var ticket = new GameTicket
            {
                Token = token,
                UserId = userId,
                PlaceId = placeId,
                JobId = jobId,
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15),
                IsUsed = false
            };

            _memoryCache.Set(token, ticket, _ticketCacheOptions);

            _ = Task.Run(async () =>
            {
                try
                {
                    using var conn = new NpgsqlConnection(_connectionString);
                    await conn.OpenAsync();
                    using var cmd = new NpgsqlCommand(@"
                        INSERT INTO authentication_tickets 
                        (ticket_token, user_id, place_id, created_at, expires_at, is_active, memory_cached, cache_expires_at, ticket_type)
                        VALUES (@token, @user_id, @place_id, NOW(), NOW() + INTERVAL '15 minutes', true, true, NOW() + INTERVAL '15 minutes', 'game_session')
                        ON CONFLICT (ticket_token) DO NOTHING", conn);
                    
                    cmd.Parameters.AddWithValue("token", token);
                    cmd.Parameters.AddWithValue("user_id", userId);
                    cmd.Parameters.AddWithValue("place_id", placeId);
                    await cmd.ExecuteNonQueryAsync();
                }
                catch {}
            });

            return token;
        }

        /// <summary>
        /// Validates a game ticket from memory cache.
        /// Returns null if ticket is expired or not found.
        /// Tickets remain valid for 15 minutes regardless of use count.
        /// </summary>
        public Task<GameTicket?> ValidateGameTicketAsync(string token)
        {
            if (string.IsNullOrEmpty(token) || !token.StartsWith("game_"))
                return Task.FromResult<GameTicket?>(null);

            if (_memoryCache.TryGetValue(token, out GameTicket? ticket))
            {
                if (ticket != null && ticket.ExpiresAt > DateTimeOffset.UtcNow)
                {
                    return Task.FromResult<GameTicket?>(ticket);
                }
            }

            return Task.FromResult<GameTicket?>(null);
        }

        /// <summary>
        /// Marks a game ticket as used in the database shadow but keeps it in memory cache.
        /// The ticket will expire naturally after 15 minutes of inactivity from the memory cache.
        /// </summary>
        public async Task MarkTicketUsedAsync(string token)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    using var conn = new NpgsqlConnection(_connectionString);
                    await conn.OpenAsync();
                    using var cmd = new NpgsqlCommand(@"
                        UPDATE authentication_tickets 
                        SET is_active = false, used_at = NOW()
                        WHERE ticket_token = @token", conn);
                    
                    cmd.Parameters.AddWithValue("token", token);
                    await cmd.ExecuteNonQueryAsync();
                }
                catch {}
            });

            await Task.CompletedTask;
        }

        /// <summary>
        /// Cleans up expired game tickets from the database shadow table.
        /// This should be called periodically by a background service.
        /// </summary>
        public async Task CleanupExpiredTicketsAsync()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand(@"
                DELETE FROM authentication_tickets 
                WHERE memory_cached = true 
                AND cache_expires_at < NOW() - INTERVAL '1 hour'", conn);

            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Cleans up inactive sessions that haven't been used in over 1 year.
        /// </summary>
        public async Task CleanupInactiveSessionsAsync()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(@"
                DELETE FROM sessions 
                WHERE expires_at < NOW() - INTERVAL '30 days'", conn);

            await cmd.ExecuteNonQueryAsync();
        }

        private static string GenerateToken(string prefix)
        {
            using var rng = RandomNumberGenerator.Create();
            var tokenBytes = new byte[32];
            rng.GetBytes(tokenBytes);
            var base64 = Convert.ToBase64String(tokenBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
            return prefix + base64;
        }
    }

    public class GameTicket
    {
        public string Token { get; set; } = string.Empty;
        public long UserId { get; set; }
        public long PlaceId { get; set; }
        public string? JobId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
    }
}

