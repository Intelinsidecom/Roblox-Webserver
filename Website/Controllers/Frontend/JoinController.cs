using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Games;
using System.Threading.Tasks;
using System.Security.Claims;

namespace Website.Controllers.Frontend
{
    [ApiController]
    public class JoinController : Controller
    {
        private readonly AuthenticationTicketService _ticketService;
        private readonly TokenService _tokenService;
        private readonly GamePresenceService _gamePresenceService;

        public JoinController(AuthenticationTicketService ticketService, TokenService tokenService, GamePresenceService gamePresenceService)
        {
            _ticketService = ticketService ?? throw new System.ArgumentNullException(nameof(ticketService));
            _tokenService = tokenService ?? throw new System.ArgumentNullException(nameof(tokenService));
            _gamePresenceService = gamePresenceService ?? throw new System.ArgumentNullException(nameof(gamePresenceService));
        }

        [HttpGet("game-auth/getauthticket")]
        public async Task<IActionResult> GetAuthTicket([FromQuery] long? placeId = null)
        {
            try
            {
                var claimVal = User.FindFirstValue(ClaimTypes.NameIdentifier);
                long userId;
                bool isGuest = false;
                
                if (string.IsNullOrEmpty(claimVal) || !long.TryParse(claimVal, out userId) || userId <= 0)
                {
                    userId = 0;
                    isGuest = true;
                }
                
                var ticketToken = await _tokenService.CreateGameTicketAsync(userId, placeId ?? 0, null);
                
                if (string.IsNullOrEmpty(ticketToken))
                {
                    return StatusCode(500, "Failed to create authentication ticket");
                }

                Response.Headers["Content-Type"] = "text/plain";
                return Content(ticketToken, "text/plain");
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

                long userId;
                var claimVal = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(claimVal) && long.TryParse(claimVal, out userId))
                {
                }
                else
                {
                    userId = 0;
                }
                
                HttpContext.Session.SetString("ClientStatus", status ?? "Unknown");
                
                if (status == "Connected" && placeId.HasValue && !string.IsNullOrEmpty(jobId) && !string.IsNullOrEmpty(ticketToken))
                {
                    await _gamePresenceService.RecordGameJoinAsync(userId, placeId.Value, jobId, ticketToken);
                    
                    var config = HttpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
                    var connString = config.GetConnectionString("Default");
                    var universeId = await GamesRepository.GetUniverseIdFromPlaceIdAsync(connString, placeId.Value);
                    if (universeId.HasValue)
                    {
                        await VisitTracking.RecordVisitAsync(userId, universeId.Value, config);
                    }
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
