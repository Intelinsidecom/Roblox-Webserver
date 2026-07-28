using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Games;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RobloxWebserver.Data;
using Assets;
using System.IO;
using System.Net.Http;
using Website.Services;
using Npgsql;
using System.Security.Claims;

namespace Website.Controllers.Client
{
    [ApiController]
    public class GameController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly GamePresenceService _gamePresenceService;
        private readonly AuthenticationTicketService _ticketService;
        private readonly IConfiguration _configuration;
        private readonly AssetService _assetService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ScriptTemplateService _scriptTemplateService;

        public GameController(AppDbContext dbContext, GamePresenceService gamePresenceService, AuthenticationTicketService ticketService, IConfiguration configuration, IHttpClientFactory httpClientFactory, ScriptTemplateService scriptTemplateService)
        {
            _dbContext = dbContext;
            _gamePresenceService = gamePresenceService;
            _ticketService = ticketService ?? throw new ArgumentNullException(nameof(ticketService));
            _configuration = configuration;
            _assetService = new AssetService(configuration, httpClientFactory);
            _httpClientFactory = httpClientFactory;
            _scriptTemplateService = scriptTemplateService;
        }

        private bool ValidateArbiterToken(string token)
        {
            var expectedToken = _configuration["Arbiter:AccessKey"];
            return !string.IsNullOrEmpty(expectedToken) && token == expectedToken;
        }

        [HttpPost("game/report-stats")]
        public IActionResult ReportStats()
        {
            return Ok("success");
        }

        [HttpPost("/game/validate-machine")]
        public IActionResult ValidateMachine()
        {
            return Json(new { success = true });
        }

        [HttpPost("/game/report-event")]
        public IActionResult ReportEvent()
        {
            return Json(new { success = true });
        }

        [HttpGet("/Game/LuaWebService/HandleSocialRequest.ashx")]
        public IActionResult HandleSocialRequest(string method, int playerid, int? userid, int? groupid)
        {
            string response;
            
            switch (method?.ToLower())
            {
                case "isfriendswith":
                    // Always return false for friendship checks
                    response = "<Value Type=\"boolean\">false</Value>";
                    break;
                    
                case "isbestfriendswith":
                    // Always return false for best friendship checks
                    response = "<Value Type=\"boolean\">false</Value>";
                    break;
                    
                case "isingroup":
                    // Always return false for group membership checks
                    response = "<Value Type=\"boolean\">false</Value>";
                    break;
                    
                case "getgrouprank":
                    // Always return 0 for group rank (no rank)
                    response = "<Value Type=\"integer\">0</Value>";
                    break;
                    
                case "getgrouprole":
                    // Always return empty string for group role (no role)
                    response = "<Value Type=\"string\"></Value>";
                    break;
                    
                default:
                    return BadRequest($"Unknown method: {method}");
            }
            
            return Content(response, "application/xml");
        }

        [HttpGet("/Game/ClientPing.ashx")]
        public async Task<IActionResult> ClientPing(long? UserID, long? PlaceID)
        {
            if (!UserID.HasValue || !PlaceID.HasValue)
            {
                return BadRequest("Missing UserID or PlaceID parameter");
            }

            try
            {
                var presence = await _gamePresenceService.GetUserGamePresenceAsync(UserID.Value);
                
                if (presence != null && presence.PlaceId == PlaceID.Value)
                {
                    await _gamePresenceService.UpdatePlayerActivityAsync(UserID.Value, PlaceID.Value);
                    
                    return Json(new { 
                        success = true,
                        message = "Ping updated",
                        inGame = true,
                        placeID = presence.PlaceId,
                        jobID = presence.JobId
                    });
                }
                else if (presence != null && presence.PlaceId != PlaceID.Value)
                {
                    return Json(new { 
                        success = false, 
                        message = "Player is in a different place",
                        inGame = true,
                        currentPlaceID = presence.PlaceId,
                        requestedPlaceID = PlaceID.Value
                    });
                }
                else
                {
                    return Json(new { 
                        success = false, 
                        message = "Player not found in any game",
                        inGame = false
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("/Game/Joined")]
        public async Task<IActionResult> PlayerJoined([FromForm] long userId, [FromForm] long placeId, [FromForm] string jobId, [FromForm] string token)
        {
            if (!ValidateArbiterToken(token))
            {
                return Unauthorized("Invalid authentication token");
            }

            try
            {
                var existingPresence = await _gamePresenceService.GetUserGamePresenceAsync(userId);
                
                if (existingPresence != null)
                {
                    if (existingPresence.PlaceId == placeId && existingPresence.JobId == jobId)
                    {
                        await _gamePresenceService.UpdatePlayerActivityAsync(userId, placeId);
                        return Json(new { success = true, message = "Player activity updated" });
                    }
                    else
                    {
                        await _gamePresenceService.RemoveFromGameAsync(userId);
                    }
                }

                var ticket = await _ticketService.CreateGameTicketAsync(userId, placeId);
                if (ticket != null)
                {
                    await _gamePresenceService.RecordGameJoinAsync(userId, placeId, jobId, ticket.TicketToken);
                    return Json(new { success = true, message = "Player joined game" });
                }

                return Json(new { success = false, message = "Failed to create ticket" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameController.PlayerJoined] Error: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("/Game/Left")]
        public async Task<IActionResult> PlayerLeft([FromForm] long userId, [FromForm] long placeId, [FromForm] string jobId, [FromForm] string token)
        {
            if (!ValidateArbiterToken(token))
            {
                return Unauthorized("Invalid authentication token");
            }

            try
            {
                var presence = await _gamePresenceService.GetUserGamePresenceAsync(userId);
                
                if (presence != null && presence.PlaceId == placeId && presence.JobId == jobId)
                {
                    await _gamePresenceService.RemoveFromGameAsync(userId);
                    return Json(new { success = true, message = "Player left game" });
                }
                else if (presence != null)
                {
                    return Json(new { success = true, message = "Player is in a different game, not removing" });
                }
                
                return Json(new { success = true, message = "Player not in game" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameController.PlayerLeft] Error: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("/Game/GetCurrentUser.ashx")]
        public async Task<IActionResult> GetCurrentUser()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (long.TryParse(userIdClaim, out var userId) && userId > 0)
                    return Content(userId.ToString(), "text/plain");
            }

            var token = Request.Cookies[".ROBLOSECURITY"];
            if (string.IsNullOrEmpty(token))
                token = Request.Query["suggest"];
            if (string.IsNullOrEmpty(token))
                token = Request.Query[".ROBLOSECURITY"];
            if (string.IsNullOrEmpty(token))
                token = Request.Headers["X-Roblox-Auth"];

            if (!string.IsNullOrEmpty(token))
            {
                var tokenService = HttpContext.RequestServices.GetRequiredService<TokenService>();
                var userId = await tokenService.ValidateSessionAsync(token);
                if (userId.HasValue && userId.Value > 0)
                    return Content(userId.Value.ToString(), "text/plain");
            }

            return Content("0", "text/plain");
        }

        [HttpGet("/Game/ClientPresence.ashx")]
        public async Task<IActionResult> ClientPresence(long? userID, long? PlaceID, string? action, string? jobId)
        {
            if (!userID.HasValue || !PlaceID.HasValue)
            {
                return BadRequest("Missing userID or PlaceID parameter");
            }

            try
            {
                var existingPresence = await _gamePresenceService.GetUserGamePresenceAsync(userID.Value);
                if (existingPresence != null && existingPresence.PlaceId != PlaceID.Value)
                {
                    return Json(new { 
                        success = false,
                        kick = true,
                        message = "User already in another game",
                        currentPlaceID = existingPresence.PlaceId,
                        requestedPlaceID = PlaceID.Value,
                        reason = "CONFLICTING_GAME_SESSION"
                    });
                }

                if (action == "disconnect")
                {
                    await _gamePresenceService.RemoveFromGameAsync(userID.Value);
                    return Json(new { success = true, message = "Disconnected" });
                }
                else if (action == "ping" || string.IsNullOrEmpty(action))
                {
                    await _gamePresenceService.UpdatePlayerHeartbeatAsync(userID.Value);
                    
                    var presence = await _gamePresenceService.GetUserGamePresenceAsync(userID.Value);
                    
                    if (presence != null && !string.IsNullOrEmpty(jobId) && presence.JobId != jobId)
                    {
                        await _gamePresenceService.RecordGameJoinAsync(userID.Value, PlaceID.Value, jobId, "");
                        presence = await _gamePresenceService.GetUserGamePresenceAsync(userID.Value);
                    }
                    
                    if (presence == null)
                    {
                        return Json(new { 
                            success = false, 
                            message = "User not in game",
                            inGame = false,
                            needsCreation = true
                        });
                    }
                    
                    if (presence != null)
                    {
                        return Json(new { 
                            success = true,
                            inGame = true,
                            placeID = presence.PlaceId,
                            jobID = presence.JobId,
                            lastPing = presence.UpdatedAt,
                            playerCount = await _gamePresenceService.GetActivePlayerCountByPlaceAsync(PlaceID.Value)
                        });
                    }
                    else
                    {
                        return Json(new { 
                            success = false, 
                            message = "User not in game",
                            inGame = false
                        });
                    }
                }
                else if (action == "create")
                {
                    if (existingPresence != null)
                    {
                        if (existingPresence.PlaceId == PlaceID.Value && 
                            (!string.IsNullOrEmpty(jobId) && existingPresence.JobId == jobId))
                        {
                            return Json(new { 
                                success = true,
                                inGame = true,
                                placeID = existingPresence.PlaceId,
                                jobID = existingPresence.JobId,
                                lastPing = existingPresence.UpdatedAt,
                                playerCount = await _gamePresenceService.GetActivePlayerCountByPlaceAsync(PlaceID.Value),
                                message = "Already in game"
                            });
                        }
                        
                        await _gamePresenceService.RemoveFromGameAsync(userID.Value);
                    }
                    
                    var ticket = await _ticketService.CreateGameTicketAsync(userID.Value, PlaceID.Value);
                    if (ticket != null)
                    {
                        var actualJobId = !string.IsNullOrEmpty(jobId) ? jobId : Guid.NewGuid().ToString();
                        await _gamePresenceService.RecordGameJoinAsync(userID.Value, PlaceID.Value, actualJobId, ticket.TicketToken);
                        var presence = await _gamePresenceService.GetUserGamePresenceAsync(userID.Value);

                        if (presence != null)
                        {
                            return Json(new { 
                                success = true,
                                inGame = true,
                                placeID = presence.PlaceId,
                                jobID = presence.JobId,
                                lastPing = presence.UpdatedAt,
                                playerCount = await _gamePresenceService.GetActivePlayerCountByPlaceAsync(PlaceID.Value),
                                message = "Presence created successfully"
                            });
                        }
                    }
                    
                    return Json(new { 
                        success = false, 
                        message = "Failed to create presence",
                        inGame = false
                    });
                }
                else
                {
                    return BadRequest($"Unknown action: {action}");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
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

        private async Task<bool> UserOwnsPlaceAsync(long userId, long placeId)
        {
            var connectionString = _configuration.GetConnectionString("Default");
            if (string.IsNullOrEmpty(connectionString))
                return false;

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(@"
                select a.owner_user_id
                from assets a
                where a.asset_id = @placeId and a.is_place = true
                limit 1", conn);

            cmd.Parameters.AddWithValue("placeId", placeId);

            var result = await cmd.ExecuteScalarAsync();
            return result != null && (long)result == userId;
        }

        [HttpGet("/Game/edit.ashx")]
        public async Task EditPlace([FromQuery] long placeId, [FromQuery] string? upload = null, [FromQuery] bool testmode = false, [FromQuery] long universeId = 0)
        {
            if (placeId < 0)
            {
                Response.StatusCode = 400;
                return;
            }

            var userId = await GetCurrentUserIdAsync();
            if (userId == 0 || !await UserOwnsPlaceAsync(userId, placeId))
            {
                Response.StatusCode = 403;
                return;
            }

            var baseUrl = _configuration["PublicBaseUrl"]?.TrimEnd('/') ?? $"{Request.Scheme}://{Request.Host}";
            var placeIdStr = placeId > 0 ? placeId.ToString() : "0";

            var roblosecurityCookie = Request.Cookies[".ROBLOSECURITY"] ?? Request.Cookies["RBXSessionTracker"] ?? "";
            var luaSafeCookie = roblosecurityCookie.Replace("\\", "\\\\").Replace("\"", "\\\"");

            var dataBaseUrl = _configuration["DataBaseUrl"] ?? baseUrl.Replace("://www.", "://data.").Replace("://assetgame.", "://data.");
            var uploadUrl = $"{dataBaseUrl}/Data/Upload.ashx?assetid={placeId}";

            var replacement = $"%1% = {luaSafeCookie}; %2% = {placeIdStr}; %3% = {baseUrl}; %4% = {uploadUrl}; %5% = {universeId}";
            var script = _scriptTemplateService.Render("edit", replacement);

            var data = "\r\n" + script;
            await WriteSignedScript(data);
        }

        [HttpGet("/Game/visit.ashx")]
        public async Task VisitPlace([FromQuery] long placeId, [FromQuery] long userId = 0, [FromQuery] long universeId = 0, [FromQuery] int isPlaySolo = 0)
        {
            if (placeId < 0)
            {
                Response.StatusCode = 400;
                return;
            }

            var baseUrl = _configuration["PublicBaseUrl"]?.TrimEnd('/') ?? $"{Request.Scheme}://{Request.Host}";

            var roblosecurityCookie = Request.Cookies[".ROBLOSECURITY"] ?? Request.Cookies["RBXSessionTracker"] ?? "";
            var luaSafeCookie = roblosecurityCookie.Replace("\\", "\\\\").Replace("\"", "\\\"");

            var userName = $"Guest {userId}";
            var isUnder13 = "False";
            var membershipType = "None";
            var accountAge = "0";

            if (userId > 0)
            {
                try
                {
                    var connStr = _configuration.GetConnectionString("Default");
                    if (!string.IsNullOrWhiteSpace(connStr))
                    {
                        await using var conn = new NpgsqlConnection(connStr);
                        await conn.OpenAsync();
                        await using var cmd = new NpgsqlCommand(
                            "SELECT user_name, birthday, membership_status, account_age_days FROM users WHERE user_id = @uid", conn);
                        cmd.Parameters.AddWithValue("uid", userId);
                        await using var reader = await cmd.ExecuteReaderAsync();
                        if (await reader.ReadAsync())
                        {
                            userName = reader.IsDBNull(0) ? userName : reader.GetString(0);
                            accountAge = reader.IsDBNull(3) ? "0" : reader.GetInt32(3).ToString();

                            if (!reader.IsDBNull(1))
                            {
                                var birthday = reader.GetDateTime(1);
                                isUnder13 = birthday.AddYears(13) > DateTime.UtcNow ? "True" : "False";
                            }

                            var membership = reader.IsDBNull(2) ? 0 : reader.GetInt16(2);
                            membershipType = membership switch
                            {
                                1 => "BuildersClub",
                                2 => "TurboBuildersClub",
                                3 => "OutrageousBuildersClub",
                                _ => "None"
                            };
                        }
                    }
                }
                catch
                {
                    // fallback to guest defaults
                }
            }

            var isPlaySoloStr = isPlaySolo != 0 ? "true" : "false";
            var userAgent = Request.Headers["User-Agent"].FirstOrDefault() ?? "";
            var isInStudio = userAgent.Contains("Roblox", StringComparison.OrdinalIgnoreCase) ? "true" : "false";

            var placeIdStr = placeId > 0 ? placeId.ToString() : "0";
            var replacement = $"%1% = {luaSafeCookie}; %2% = {placeIdStr}; %3% = {baseUrl}; %4% = {universeId}; %5% = {userId}; %6% = {userName}; %7% = {isUnder13}; %8% = {membershipType}; %9% = {accountAge}; %10% = {isPlaySoloStr}; %11% = {isInStudio}";
            var script = _scriptTemplateService.Render("visit", replacement);

            var data = "\r\n" + script;
            await WriteSignedScript(data);
        }

        private async Task WriteSignedScript(string data)
        {
            var privateKeyPem = _configuration["RSA:PrivateKey"];
            if (string.IsNullOrWhiteSpace(privateKeyPem))
            {
                Response.ContentType = "text/plain";
                Response.Headers["X-Robots-Tag"] = "noindex";
                await Response.WriteAsync(data);
                return;
            }

            using (var rsa = System.Security.Cryptography.RSA.Create())
            {
                rsa.ImportFromPem(privateKeyPem);
                var dataBytes = System.Text.Encoding.UTF8.GetBytes(data);
                var sig = rsa.SignData(dataBytes, System.Security.Cryptography.HashAlgorithmName.SHA1, System.Security.Cryptography.RSASignaturePadding.Pkcs1);
                var sigB64 = Convert.ToBase64String(sig);
                Response.ContentType = "text/plain";
                Response.Headers["X-Robots-Tag"] = "noindex";
                await Response.WriteAsync("--rbxsig%" + sigB64 + "%" + data);
            }
        }
    }
}