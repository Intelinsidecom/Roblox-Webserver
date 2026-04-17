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
using System.Net.Http;
using System.Security.Claims;
using System.Linq;
using System.Net;

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
        private readonly TokenService _tokenService;
        private readonly GamePresenceService _gamePresenceService;
        private readonly string _privateKey;
        private readonly string _publicKey;

        public PlaceLauncherController(IConfiguration configuration, AuthenticationTicketService ticketService, TokenService tokenService, GamePresenceService gamePresenceService)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _ticketService = ticketService ?? throw new ArgumentNullException(nameof(ticketService));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _gamePresenceService = gamePresenceService ?? throw new ArgumentNullException(nameof(gamePresenceService));
            _privateKey = _configuration["RSA:PrivateKey"] ?? throw new InvalidOperationException("RSA:PrivateKey not found in configuration");
            _publicKey = _configuration["RSA:PublicKey"] ?? throw new InvalidOperationException("RSA:PublicKey not found in configuration");
        }

        [HttpGet("PlaceLauncher.ashx")]
        [HttpPost("PlaceLauncher.ashx")]
        public async Task<IActionResult> PlaceLauncher([FromQuery] string request, [FromQuery] long? placeId, [FromQuery] int? serverPort, [FromQuery] string? jobid, [FromQuery] bool? guest)
        {
            var requestId = Guid.NewGuid().ToString("N")[..8];

            if (guest == true)
            {
                HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
            }
            
            if (placeId == null)
            {
                return Ok(new PlaceLauncherResponse { status = 8, message = "Place does not exist" });
            }

            try
            {
                return request switch
                {
                    "RequestGame" or "RequestGameJob" or "RequestFollowUser" => await HandleRequestGame(placeId.Value, request, jobid, serverPort, requestId),
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

        private async Task<ArbiterGameServerInfo?> FindAvailableServerForPlaceAsync(long placeId)
        {
            try
            {
                var arbiterHost = _configuration["Arbiter:Host"] ?? "localhost";
                var arbiterPort = _configuration["Arbiter:Port"] ?? "5000";
                var arbiterUrl = $"http://{arbiterHost}:{arbiterPort}";
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(5);
                var response = await httpClient.GetAsync($"{arbiterUrl}/api/gameservers/by-place/{placeId}");
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }
                
                var json = await response.Content.ReadAsStringAsync();
                var serverInfoOptions = new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                };
                
                var apiResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(json, serverInfoOptions);
                if (apiResponse == null || !apiResponse.TryGetValue("servers", out var serversObj))
                {
                    return null;
                }
                
                var serversJson = JsonSerializer.Serialize(serversObj);
                var servers = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(serversJson, serverInfoOptions);
                if (servers == null || servers.Count == 0)
                {
                    return null;
                }
                
                foreach (var serverDict in servers)
                {
                    if (serverDict.TryGetValue("playerCount", out var pcObj) && 
                        serverDict.TryGetValue("maxPlayers", out var mpObj) &&
                        int.TryParse(pcObj.ToString(), out var playerCount) &&
                        int.TryParse(mpObj.ToString(), out var maxPlayers) &&
                        playerCount < maxPlayers)
                    {
                        var gameId = serverDict["gameId"].ToString();
                        var port = int.Parse(serverDict["port"].ToString());

                        return new ArbiterGameServerInfo
                        {
                            GameId = gameId,
                            PlaceId = (int)placeId,
                            Port = port,
                            MaxPlayers = maxPlayers,
                            PlayerCount = playerCount,
                            Status = serverDict["status"].ToString()
                        };
                    }
                }
                
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private async Task<long> GetCurrentUserIdAsync()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var claimVal = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(claimVal) && long.TryParse(claimVal, out var userId) && userId > 0)
                {
                    return userId;
                }
            }

            return 0;
        }

        private async Task<(string userName, string displayName)> GetUserInfoAsync(long userId)
        {
            var connString = _configuration.GetConnectionString("Default");
            using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand("select user_name from users where user_id = @userId", conn);
            cmd.Parameters.AddWithValue("userId", userId);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var userName = reader.GetString(0);
                return (userName, userName);
            }
            
            return ("Unknown", "Unknown");
        }

        private async Task<IActionResult> HandleRequestGame(long placeId, string requestType, string? providedJobId, int? providedServerPort, string requestId)
        {
            long userId = await GetCurrentUserIdAsync();
            if (userId > 0 && !await UserCanAccessPlaceAsync(userId, placeId))
            {
                return Ok(new PlaceLauncherResponse { status = 3, message = "Access denied" });
            }

            string gameId;
            int serverPort;
            
            if (string.IsNullOrEmpty(providedJobId))
            {
                var existingServer = await FindAvailableServerForPlaceAsync(placeId);
                if (existingServer != null)
                {
                    gameId = existingServer.GameId;
                    serverPort = existingServer.Port;
                }
                else
                {
                    try
                    {
                        var connectionString = _configuration.GetConnectionString("Default");
                        var maxPlayers = await GameCreationService.GetPlaceMaxPlayersAsync(placeId, connectionString);                        
                        var (jobId, port) = await GameCreationService.CreateGameServerAsync((int)placeId, maxPlayers, _configuration, connectionString);
                        gameId = jobId;
                        serverPort = port;
                    }
                    catch (Exception ex)
                    {
                        return Ok(new PlaceLauncherResponse { status = 4, message = $"Error creating game server: {ex.Message}" });
                    }
                }
            }
            else
            {
                gameId = providedJobId;
                serverPort = providedServerPort ?? 53640;
            }

            string ticketToken = "";
            string guestToken = "";
            if (userId != 0)
            {
                ticketToken = await _tokenService.CreateGameTicketAsync(userId, placeId, gameId);
            }
            else
            {
                var randomSuffix = new Random().Next(1000, 9999);
                guestToken = $"guest-Guest{randomSuffix}";
            }
                        var baseUrl = _configuration["PublicBaseUrl"]?.TrimEnd('/');
            int gamestatus = 2; // Success
            var joinScriptUrl = $"{baseUrl}/game/join.ashx?serverPort={serverPort}&gameid={placeId}&jobid={gameId}";
            var authenticationUrl = $"{baseUrl}/Login/Negotiate.ashx";

            return Ok(new PlaceLauncherResponse
            {
                jobId = gameId,
                status = gamestatus,
                joinScriptUrl = joinScriptUrl,
                authenticationUrl = authenticationUrl,
                authenticationTicket = ticketToken ?? guestToken, // guest token for guests
                message = null
            });
        }

        [HttpGet("join.ashx")]
        [Produces("text/plain")]
        public async Task<IActionResult> Join([FromQuery] long gameid, [FromQuery] int serverPort, [FromQuery] string jobid, [FromQuery] bool? guest)
        {
            try
            {
                if (gameid <= 0 || serverPort <= 0 || string.IsNullOrEmpty(jobid))
                {
                    return BadRequest("Invalid parameters");
                }
                
                if (guest == true)
                {
                    HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
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

                long userId;
                string userName, displayName;

                if (User.Identity?.IsAuthenticated == true)
                {
                    var claimVal = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
                    if (!string.IsNullOrEmpty(claimVal) && long.TryParse(claimVal, out userId) && userId > 0)
                    {
                        var userInfo = await GetUserInfoAsync(userId);
                        userName = userInfo.userName;
                        displayName = userInfo.displayName;
                    }
                    else
                    {
                        userId = 0;
                        var randomSuffix = new Random().Next(1000, 9999);
                        userName = $"Guest{randomSuffix}";
                        displayName = userName;
                    }
                }
                else
                {
                    userId = 0;
                    var randomSuffix = new Random().Next(1000, 9999);
                    userName = $"Guest{randomSuffix}";
                    displayName = userName;
                }
                
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

                var clientTicket = userId != 0 
                    ? ""
                    : $"guest-{userName}";
                
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
                    ClientTicket = clientTicket,
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

            var ticket = await _tokenService.ValidateGameTicketAsync(suggest);
            if (ticket == null)
            {
                return Unauthorized();
            }

            await _tokenService.MarkTicketUsedAsync(suggest);
            var sessionToken = await _tokenService.CreateSessionAsync(ticket.UserId, HttpContext.Connection.RemoteIpAddress?.ToString());
            var cookieOptions = new Microsoft.AspNetCore.Http.CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                Path = "/",
                HttpOnly = true,
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax
            };
            
            var allowInsecure = _configuration.GetValue<bool>("Auth:AllowInsecureCookies");
            var cookieDomain = _configuration["Auth:CookieDomain"];
            cookieOptions.Secure = Request.IsHttps && !allowInsecure;
            if (!string.IsNullOrWhiteSpace(cookieDomain))
                cookieOptions.Domain = cookieDomain;

            Response.Cookies.Append(".ROBLOSECURITY", sessionToken, cookieOptions);

            return Content(sessionToken, "text/plain");
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

