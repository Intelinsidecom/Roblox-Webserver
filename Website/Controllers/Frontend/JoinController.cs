using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Games;
using System.Threading.Tasks;

namespace Website.Controllers.Frontend
{
    [ApiController]
    public class JoinController : Controller
    {
        private readonly AuthenticationTicketService _ticketService;
        private readonly GamePresenceService _gamePresenceService;

        public JoinController(AuthenticationTicketService ticketService, GamePresenceService gamePresenceService)
        {
            _ticketService = ticketService ?? throw new System.ArgumentNullException(nameof(ticketService));
            _gamePresenceService = gamePresenceService ?? throw new System.ArgumentNullException(nameof(gamePresenceService));
        }

        [HttpGet("game-auth/getauthticket")]
        public async Task<IActionResult> GetAuthTicket([FromQuery] long? placeId = null)
        {
            try
            {
                long userId = 1;
                
                var ticket = await _ticketService.CreateGeneralTicketAsync(userId);
                
                if (ticket == null)
                {
                    return StatusCode(500, "Failed to create authentication ticket");
                }

                Response.Headers["Content-Type"] = "text/plain";
                return Content(ticket.TicketToken, "text/plain");
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("client-status/set")]
        public async Task<IActionResult> SetClientStatus([FromQuery] string status, [FromQuery] long? placeId = null, [FromQuery] string? jobId = null, [FromQuery] string? ticketToken = null)
        {
            try
            {
                // Get the current user ID from session or authentication
                // For now, we'll use a default user ID (you should get this from actual authentication)
                long userId = 1;
                
                // Store the client status in session
                HttpContext.Session.SetString("ClientStatus", status ?? "Unknown");
                
                if (status == "Connected" && placeId.HasValue && !string.IsNullOrEmpty(jobId) && !string.IsNullOrEmpty(ticketToken))
                {
                    await _gamePresenceService.RecordGameJoinAsync(userId, placeId.Value, jobId, ticketToken);
                }
                else if (status == "Disconnected")
                {
                    await _gamePresenceService.RemoveFromGameAsync(userId);
                }
                
                await _gamePresenceService.UpdateUserClientStatusAsync(userId, status ?? "Unknown");
                
                return Ok(new { success = true, status = status });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("game/joined")]
        public async Task<IActionResult> GameJoined([FromBody] GameJoinRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.TicketToken))
                {
                    return BadRequest("Invalid request or missing ticket token");
                }

                var ticket = await _ticketService.ValidateTicketAsync(request.TicketToken);
                if (ticket == null)
                {
                    return Unauthorized("Invalid or expired ticket");
                }

                await _gamePresenceService.RecordGameJoinAsync(
                    ticket.UserId, 
                    request.PlaceId, 
                    request.JobId, 
                    request.TicketToken);

                return Ok(new { success = true, message = "Game join recorded" });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("game/left")]
        public async Task<IActionResult> GameLeft([FromBody] GameLeaveRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Invalid request");
                }

                long userId = request.UserId;
                if (!string.IsNullOrEmpty(request.TicketToken))
                {
                    var ticket = await _ticketService.ValidateTicketAsync(request.TicketToken);
                    if (ticket != null)
                    {
                        userId = ticket.UserId;
                    }
                }

                if (userId <= 0)
                {
                    return BadRequest("Invalid user identification");
                }

                await _gamePresenceService.RemoveFromGameAsync(userId);

                return Ok(new { success = true, message = "Game leave recorded" });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("game/presence/{userId}")]
        public async Task<IActionResult> GetUserPresence(long userId)
        {
            try
            {
                var presence = await _gamePresenceService.GetUserGamePresenceAsync(userId);
                
                if (presence == null)
                {
                    return Ok(new { 
                        success = false, 
                        message = "User not in game",
                        inGame = false
                    });
                }

                return Ok(new { 
                    success = true,
                    inGame = true,
                    placeId = presence.PlaceId,
                    jobId = presence.JobId,
                    joinedAt = presence.CreatedAt
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("client-status")]
        public IActionResult GetClientStatus()
        {
            try
            {
                var status = HttpContext.Session.GetString("ClientStatus") ?? "Unknown";
                
                Response.Headers["Content-Type"] = "text/plain";
                return Content(status, "text/plain");
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }

    public class GameJoinRequest
    {
        public long PlaceId { get; set; }
        public string JobId { get; set; } = string.Empty;
        public string TicketToken { get; set; } = string.Empty;
    }

    public class GameLeaveRequest
    {
        public long UserId { get; set; }
        public string? TicketToken { get; set; }
    }
}