using Microsoft.AspNetCore.Mvc;
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

        public GameController(AppDbContext dbContext, GamePresenceService gamePresenceService, AuthenticationTicketService ticketService)
        {
            _dbContext = dbContext;
            _gamePresenceService = gamePresenceService;
            _ticketService = ticketService ?? throw new ArgumentNullException(nameof(ticketService));
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