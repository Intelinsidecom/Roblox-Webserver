using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Games;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Api.Data;

namespace Website.Controllers.Client
{
    [ApiController]
    public class GameController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly GamePresenceService _gamePresenceService;
        private readonly AuthenticationTicketService _ticketService;
        private readonly IConfiguration _configuration;

        public GameController(AppDbContext dbContext, GamePresenceService gamePresenceService, AuthenticationTicketService ticketService, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _gamePresenceService = gamePresenceService;
            _ticketService = ticketService ?? throw new ArgumentNullException(nameof(ticketService));
            _configuration = configuration;
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
    }
}