using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Games;
using Npgsql;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Website.Controllers.Client
{


    /// <summary>
    /// Attempt at getting the place joining to work. copied from void revival. im just testing stuff locally
    /// </summary>
    [ApiController]
    [Route("Game")]
    public class PlaceLauncherController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AuthenticationTicketService _ticketService;
        private readonly GamePresenceService _gamePresenceService;
        private readonly string _privateKey;
        private readonly string _publicKey;

        public PlaceLauncherController(IConfiguration configuration, AuthenticationTicketService ticketService, GamePresenceService gamePresenceService)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _ticketService = ticketService ?? throw new ArgumentNullException(nameof(ticketService));
            _gamePresenceService = gamePresenceService ?? throw new ArgumentNullException(nameof(gamePresenceService));
            _privateKey = _configuration["RSA:PrivateKey"] ?? throw new InvalidOperationException("RSA:PrivateKey not found in configuration");
            _publicKey = _configuration["RSA:PublicKey"] ?? throw new InvalidOperationException("RSA:PublicKey not found in configuration");
        }

        [HttpGet("PlaceLauncher.ashx")]
        public async Task<IActionResult> PlaceLauncher([FromQuery] string request, [FromQuery] long? placeId, [FromQuery] int? serverPort, [FromQuery] string? jobid)
        {
            if (placeId == null)
            {
                return Ok(new PlaceLauncherResponse { status = 8, message = "Place does not exist" });
            }

            try
            {
                return request switch
                {
                    "RequestGame" or "RequestGameJob" or "RequestFollowUser" => await HandleRequestGame(placeId.Value, request, jobid, serverPort),
                    "AuthenticateTicket" => await HandleAuthenticateTicket(),
                    "LogJoinClick" => Ok(new { status = 1, message = "Logged" }),
                    _ => BadRequest(new { status = 0, message = "Invalid request type" })
                };
            }
            catch (Exception ex)
            {
                return Ok(new PlaceLauncherResponse 
                { 
                    status = 4, 
                    message = $"Unknown Error: {ex.Message}" 
                });
            }
        }

        private async Task<IActionResult> HandleRequestGame(long placeId, string requestType, string? providedJobId, int? providedServerPort)
        {
            long userId = 1; 

            if (!await UserCanAccessPlaceAsync(userId, placeId))
            {
                return Ok(new PlaceLauncherResponse { status = 3, message = "Access denied" });
            }

            var ticket = await _ticketService.CreateGameTicketAsync(userId, placeId);
            var jobId = !string.IsNullOrEmpty(providedJobId) ? providedJobId : Guid.NewGuid().ToString();
            var port = providedServerPort ?? 53640;
            var baseUrl = _configuration["PublicBaseUrl"]?.TrimEnd('/');
            int gamestatus = 2; // Success
            var joinScriptUrl = $"{baseUrl}/game/join.ashx?serverPort={port}&gameid={placeId}&jobid={jobId}";
            var authenticationUrl = $"{baseUrl}/Login/Negotiate.ashx";

            return Ok(new PlaceLauncherResponse
            {
                jobId = jobId,
                status = gamestatus,
                joinScriptUrl = joinScriptUrl,
                authenticationUrl = authenticationUrl,
                authenticationTicket = ticket.TicketToken,
                message = null
            });
        }

        [HttpGet("join.ashx")]
        [Produces("text/plain")]
        public async Task<IActionResult> Join([FromQuery] long gameid, [FromQuery] int serverPort, [FromQuery] string jobid)
        {
            try
            {
                if (gameid <= 0 || serverPort <= 0 || string.IsNullOrEmpty(jobid))
                {
                    return BadRequest("Invalid parameters");
                }

                var arbiterHost = _configuration["Arbiter:Host"] ?? "localhost";
                var arbiterPort = _configuration["Arbiter:Port"] ?? "5000";
                var arbiterUrl = $"http://{arbiterHost}:{arbiterPort}";
                
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(5);
                
                try
                {
                    var response = await httpClient.GetAsync($"{arbiterUrl}/api/gameservers/{jobid}/status");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        
                        var serverInfoOptions = new JsonSerializerOptions 
                        { 
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                        };
                        
                        var serverInfoDict = JsonSerializer.Deserialize<Dictionary<string, object>>(json, serverInfoOptions);
                        
                        if (serverInfoDict != null && serverInfoDict.TryGetValue("port", out var portObj) && 
                            int.TryParse(portObj.ToString(), out var actualPort))
                        {
                            if (actualPort != serverPort)
                            {
                                serverPort = actualPort;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Error getting server port from Arbiter: {ex.Message}");
                }

                long userId = 1;
                var userName = "Admin";
                var displayName = "Admin";
                var membership = "None"; // or "BuildersClub", etc.
                var accountAge = 365;
                var baseUrl = _configuration["PublicBaseUrl"]?.TrimEnd('/') + "/";
                var machineAddress = "127.0.0.1";
                var localhostMode = _configuration.GetValue<bool>("LocalhostMode", true);
                
                if (localhostMode)
                {
                    machineAddress = "127.0.0.1";
                }
                else
                {
                    machineAddress = _configuration["MachineAddress"] ?? "127.0.0.1";
                }

                var ticket = await _ticketService.CreateGameTicketAsync(userId, gameid);
                // Player count logging moved to ClientPresence.ashx - only record ticket usage here
                await _ticketService.MarkTicketAsUsedAsync(ticket.TicketToken);

                var joinScript = new JoinScriptData
                {
                    MachineAddress = machineAddress,
                    ServerPort = serverPort,
                    ServerConnections = new[] { new ServerConnection { Address = machineAddress, Port = serverPort } },
                    PingUrl = $"{baseUrl}Game/ClientPresence.ashx?PlaceID={gameid}&userID={userId}&jobId={jobid}",
                    UserName = userName,
                    DisplayName = displayName,
                    UserId = userId,
                    RobloxLocale = "en_us",
                    GameLocale = "en_us",
                    CharacterAppearance = $"{baseUrl}Asset/CharacterFetch.ashx?userId={userId}",
                    ClientTicket = ticket.TicketToken,
                    GameId = jobid,
                    PlaceId = gameid,
                    UniverseId = gameid,
                    CreatorId = 1,
                    CreatorTypeEnum = "User",
                    MembershipType = membership,
                    AccountAge = accountAge,
                    BaseUrl = baseUrl
                };

                var options = new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = null,
                    WriteIndented = false,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                var data = JsonSerializer.Serialize(joinScript, options);

                var signature = SignData("\r\n" + data);
                var result = $"--rbxsig%{signature}%\r\n{data}";

                return Content(result, "text/plain");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Join script error: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private async Task<IActionResult> HandleAuthenticateTicket()
        {
            var suggest = Request.Query["suggest"].ToString();
            if (string.IsNullOrEmpty(suggest))
            {
                return Unauthorized();
            }

            var ticket = await _ticketService.ValidateTicketAsync(suggest);
            if (ticket == null)
            {
                return Unauthorized();
            }

            Response.Cookies.Append(".ROBLOSECURITY", ticket.TicketToken, new Microsoft.AspNetCore.Http.CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(30),
                Path = "/",
                HttpOnly = true
            });

            return Content(ticket.TicketToken, "text/plain");
        }

        private string SignData(string content)
        {
            using (var rsa = RSA.Create())
            {
                rsa.ImportFromPem(_privateKey);
                var data = Encoding.UTF8.GetBytes(content);
                var signature = rsa.SignData(data, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
                return Convert.ToBase64String(signature);
            }
        }

        private string GenerateAuthTicket(long userId, string userName, string jobId)
        {
            var dateStr = DateTime.Now.ToString("M/d/yyyy h:mm:ss tt");
            return $"{userId};{userName};{jobId};{dateStr}";
        }

        private async Task<bool> UserCanAccessPlaceAsync(long userId, long placeId)
        {
            var connectionString = _configuration.GetConnectionString("Default");
            if (string.IsNullOrEmpty(connectionString))
                return false;

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(@"
                select a.access_type, u.creator_user_id 
                from assets a
                left join universes u on @placeId = ANY(u.place_ids)
                where a.asset_id = @placeId and a.is_place = true
                limit 1", conn);

            cmd.Parameters.AddWithValue("placeId", placeId);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var accessType = reader.IsDBNull(0) ? 1 : reader.GetInt32(0);
                var creatorUserId = reader.IsDBNull(1) ? 0 : reader.GetInt64(1);

                if (creatorUserId == userId)
                    return true;

                return accessType switch
                {
                    1 => true,
                    2 => await AreFriendsAsync(userId, creatorUserId),
                    3 => false,
                    _ => false
                };
            }

            return false;
        }

        private async Task<bool> AreFriendsAsync(long userId1, long userId2)
        {
            var connectionString = _configuration.GetConnectionString("Default");
            if (string.IsNullOrEmpty(connectionString))
                return false;

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(@"
                select 1 from user_friends 
                where user_id = @user1 and friend_user_id = @user2
                union
                select 1 from user_friends 
                where user_id = @user2 and friend_user_id = @user1
                limit 1", conn);

            cmd.Parameters.AddWithValue("user1", userId1);
            cmd.Parameters.AddWithValue("user2", userId2);

            var result = await cmd.ExecuteScalarAsync();
            return result != null;
        }

        public class PlaceLauncherResponse
        {
            [JsonPropertyName("jobId")]
            public string? jobId { get; set; }
            [JsonPropertyName("status")]
            public int status { get; set; }
            [JsonPropertyName("joinScriptUrl")]
            public string? joinScriptUrl { get; set; }
            [JsonPropertyName("authenticationUrl")]
            public string? authenticationUrl { get; set; }
            [JsonPropertyName("authenticationTicket")]
            public string? authenticationTicket { get; set; }
            [JsonPropertyName("message")]
            public string? message { get; set; }
        }

        public class JoinScriptData
        {
            public int ClientPort { get; set; } = 0;
            public string MachineAddress { get; set; } = "";
            public int ServerPort { get; set; }
            public ServerConnection[] ServerConnections { get; set; } = Array.Empty<ServerConnection>();
            public string PingUrl { get; set; } = "";
            public int PingInterval { get; set; } = 30;
            public string UserName { get; set; } = "";
            public string DisplayName { get; set; } = "";
            public bool SeleniumTestMode { get; set; } = false;
            public long UserId { get; set; }
            public string RobloxLocale { get; set; } = "en_us";
            public string GameLocale { get; set; } = "en_us";
            public bool SuperSafeChat { get; set; } = false;
            public string CharacterAppearance { get; set; } = "";
            public string ClientTicket { get; set; } = "";
            public string GameId { get; set; } = "";
            public long PlaceId { get; set; }
            public string MeasurementUrl { get; set; } = "";
            public string WaitingForCharacterGuid { get; set; } = "26eb3e21-aa80-475b-a777-b43c3ea5f7d2";
            public string BaseUrl { get; set; } = "";
            public string ChatStyle { get; set; } = "ClassicAndBubble";
            public string VendorId { get; set; } = "0";
            public string ScreenShotInfo { get; set; } = "";
            public string VideoInfo { get; set; } = "";
            public long CreatorId { get; set; }
            public string CreatorTypeEnum { get; set; } = "User";
            public string MembershipType { get; set; } = "None";
            public int AccountAge { get; set; } = 0;
            public string CookieStoreFirstTimePlayKey { get; set; } = "rbx_evt_ftp";
            public string CookieStoreFiveMinutePlayKey { get; set; } = "rbx_evt_fmp";
            public bool CookieStoreEnabled { get; set; } = true;
            public bool IsRobloxPlace { get; set; } = false;
            public bool GenerateTeleportJoin { get; set; } = false;
            public bool IsUnknownOrUnder13 { get; set; } = false;
            public string SessionId { get; set; } = "";
            public int DataCenterId { get; set; } = 69420;
            public long UniverseId { get; set; }
            public long BrowserTrackerId { get; set; } = 0;
            public bool UsePortraitMode { get; set; } = false;
            public long FollowUserId { get; set; } = 0;
        }

        public class ServerConnection
        {
            public string Address { get; set; } = "";
            public int Port { get; set; }
        }

        public class ArbiterGameServerInfo
        {
            [JsonPropertyName("gameId")]
            public string GameId { get; set; } = string.Empty;

            [JsonPropertyName("placeId")]
            public int PlaceId { get; set; }

            [JsonPropertyName("port")]
            public int Port { get; set; }

            [JsonPropertyName("maxPlayers")]
            public int MaxPlayers { get; set; }

            [JsonPropertyName("playerCount")]
            public int PlayerCount { get; set; }

            [JsonPropertyName("status")]
            public string Status { get; set; } = string.Empty;

            [JsonPropertyName("startTime")]
            public DateTime StartTime { get; set; }

            [JsonPropertyName("expiration")]
            public DateTime Expiration { get; set; }

            [JsonPropertyName("baseUrl")]
            public string BaseUrl { get; set; } = string.Empty;

            [JsonPropertyName("privateServerId")]
            public string PrivateServerId { get; set; } = string.Empty;

            [JsonPropertyName("lastActivityTime")]
            public DateTime LastActivityTime { get; set; }
        }
    }
}
