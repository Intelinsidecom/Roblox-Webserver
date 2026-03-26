using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Common;

namespace Games
{
    /// <summary>
    /// Service for managing authentication tickets for Windows client game launches
    /// </summary>
    public class AuthenticationTicketService
    {
        private readonly string _connectionString;
        private readonly string _baseUrl;
        private readonly string _publicBaseUrl;

        public AuthenticationTicketService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Default") 
                ?? throw new ArgumentNullException("Database connection string not configured");
            _baseUrl = configuration["BaseUrl"] 
                ?? throw new ArgumentNullException("BaseUrl not configured");
            _publicBaseUrl = configuration["PublicBaseUrl"] 
                ?? throw new ArgumentNullException("PublicBaseUrl not configured");
        }

        /// <summary>
        /// Creates an authentication ticket for a user to join a specific place
        /// </summary>
        public async Task<AuthenticationTicket> CreateGameTicketAsync(long userId, long placeId, long? universeId = null, string? browserTrackerId = null)
        {
            var ticketToken = GenerateSecureToken();
            var authenticationUrl = $"{_publicBaseUrl}/Game/PlaceLauncher.ashx?request=AuthenticateTicket";
            var joinScriptUrl = $"{_publicBaseUrl}/Game/PlaceLauncher.ashx?request=RequestGame&placeId={placeId}&isPartyLeader=false&gender=&isTeleport=true";
            
            var expiresAt = DateTimeOffset.UtcNow.AddMinutes(15); // Tickets expire in 15 minutes

            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(@"
                insert into authentication_tickets 
                (ticket_token, user_id, place_id, universe_id, authentication_url, join_script_url, browser_tracker_id, expires_at)
                values (@token, @user_id, @place_id, @universe_id, @auth_url, @join_url, @tracker_id, @expires_at)
                returning ticket_id, created_at, expires_at", conn);

            cmd.Parameters.AddWithValue("token", ticketToken);
            cmd.Parameters.AddWithValue("user_id", userId);
            cmd.Parameters.AddWithValue("place_id", placeId);
            cmd.Parameters.AddWithValue("universe_id", universeId.HasValue ? (object)universeId.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("auth_url", authenticationUrl);
            cmd.Parameters.AddWithValue("join_url", joinScriptUrl);
            cmd.Parameters.AddWithValue("tracker_id", browserTrackerId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("expires_at", expiresAt.UtcDateTime);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new AuthenticationTicket
                {
                    TicketId = reader.GetInt64(0),
                    TicketToken = ticketToken,
                    UserId = userId,
                    PlaceId = placeId,
                    UniverseId = universeId,
                    AuthenticationUrl = authenticationUrl,
                    JoinScriptUrl = joinScriptUrl,
                    BrowserTrackerId = browserTrackerId,
                    CreatedAt = reader.GetDateTime(1),
                    ExpiresAt = reader.GetDateTime(2),
                    IsActive = true,
                    TicketType = "game_session"
                };
            }

            throw new InvalidOperationException("Failed to create authentication ticket");
        }

        /// <summary>
        /// Validates and retrieves an authentication ticket
        /// </summary>
        public async Task<AuthenticationTicket?> ValidateTicketAsync(string ticketToken)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(@"
                select ticket_id, ticket_token, user_id, place_id, universe_id, 
                       authentication_url, join_script_url, browser_tracker_id,
                       created_at, expires_at, used_at, client_ip, client_user_agent,
                       is_active, ticket_type
                from authentication_tickets 
                where ticket_token = @token and is_active = true and expires_at > now() at time zone 'utc'
                limit 1", conn);

            cmd.Parameters.AddWithValue("token", ticketToken);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new AuthenticationTicket
                {
                    TicketId = reader.GetInt64(0),
                    TicketToken = reader.GetString(1),
                    UserId = reader.GetInt64(2),
                    PlaceId = reader.IsDBNull(3) ? null : (long?)reader.GetInt64(3),
                    UniverseId = reader.IsDBNull(4) ? null : (long?)reader.GetInt64(4),
                    AuthenticationUrl = reader.GetString(5),
                    JoinScriptUrl = reader.GetString(6),
                    BrowserTrackerId = reader.IsDBNull(7) ? null : reader.GetString(7),
                    CreatedAt = reader.GetDateTime(8),
                    ExpiresAt = reader.GetDateTime(9),
                    UsedAt = reader.IsDBNull(10) ? null : (DateTime?)reader.GetDateTime(10),
                    ClientIp = reader.IsDBNull(11) ? null : reader.GetString(11),
                    ClientUserAgent = reader.IsDBNull(12) ? null : reader.GetString(12),
                    IsActive = reader.GetBoolean(13),
                    TicketType = reader.GetString(14)
                };
            }

            return null;
        }

        /// <summary>
        /// Marks a ticket as used (for tracking and cleanup)
        /// </summary>
        public async Task MarkTicketAsUsedAsync(string ticketToken, string? clientIp = null, string? clientUserAgent = null)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(@"
                update authentication_tickets 
                set used_at = now() at time zone 'utc',
                    client_ip = @client_ip,
                    client_user_agent = @user_agent,
                    is_active = false
                where ticket_token = @token", conn);

            cmd.Parameters.AddWithValue("token", ticketToken);
            cmd.Parameters.AddWithValue("client_ip", clientIp ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("user_agent", clientUserAgent ?? (object)DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Renews an authentication ticket (extends expiration time)
        /// </summary>
        public async Task<AuthenticationTicket?> RenewTicketAsync(string ticketToken)
        {
            var newExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15);

            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(@"
                update authentication_tickets 
                set expires_at = @expires_at
                where ticket_token = @token and is_active = true and expires_at > now() at time zone 'utc'
                returning ticket_id, user_id, place_id, universe_id, expires_at", conn);

            cmd.Parameters.AddWithValue("token", ticketToken);
            cmd.Parameters.AddWithValue("expires_at", newExpiresAt.UtcDateTime);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var existingTicket = await ValidateTicketAsync(ticketToken);
                if (existingTicket != null)
                {
                    existingTicket.ExpiresAt = newExpiresAt.UtcDateTime;
                    return existingTicket;
                }
            }

            return null;
        }

        /// <summary>
        /// Cleans up expired tickets
        /// </summary>
        public async Task CleanupExpiredTicketsAsync()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(@"
                update authentication_tickets 
                set is_active = false 
                where is_active = true and expires_at < now() at time zone 'utc'", conn);

            await cmd.ExecuteNonQueryAsync();
        }

        private static string GenerateSecureToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var tokenBytes = new byte[32];
            rng.GetBytes(tokenBytes);
            return Convert.ToBase64String(tokenBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }
    }

    public class AuthenticationTicket
    {
        public long TicketId { get; set; }
        public string TicketToken { get; set; } = string.Empty;
        public long UserId { get; set; }
        public long? PlaceId { get; set; }
        public long? UniverseId { get; set; }
        public string AuthenticationUrl { get; set; } = string.Empty;
        public string JoinScriptUrl { get; set; } = string.Empty;
        public string? BrowserTrackerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? UsedAt { get; set; }
        public string? ClientIp { get; set; }
        public string? ClientUserAgent { get; set; }
        public bool IsActive { get; set; }
        public string TicketType { get; set; } = string.Empty;
    }
}
