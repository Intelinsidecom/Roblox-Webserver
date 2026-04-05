using Microsoft.AspNetCore.Mvc;
using Games;
using System.Threading.Tasks;

namespace Website.Controllers.Frontend
{
    [ApiController]
    [Route("games")]
    public class GameController : ControllerBase
    {
        private readonly GamePresenceService _gamePresenceService;

        public GameController(GamePresenceService gamePresenceService)
        {
            _gamePresenceService = gamePresenceService;
        }

        /// <summary>
        /// Gets the number of players in a specific game server (by job ID)
        /// </summary>
        [HttpGet("players/count")]
        public async Task<IActionResult> GetPlayerCountByJobId([FromQuery] string jobId)
        {
            if (string.IsNullOrEmpty(jobId))
            {
                return BadRequest("Missing jobId parameter");
            }

            try
            {
                var playerCount = await _gamePresenceService.GetPlayerCountByJobIdAsync(jobId);
                
                return Ok(new { 
                    success = true,
                    jobId = jobId,
                    playerCount = playerCount
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the number of players in a specific place
        /// </summary>
        [HttpGet("place/{placeId}/players/count")]
        public async Task<IActionResult> GetPlayerCountByPlaceId(long placeId)
        {
            if (placeId <= 0)
            {
                return BadRequest("Invalid placeId");
            }

            try
            {
                var players = await _gamePresenceService.GetPlayersInPlaceAsync(placeId);
                var playerCount = players.Count;
                
                return Ok(new { 
                    success = true,
                    placeId = placeId,
                    playerCount = playerCount
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}