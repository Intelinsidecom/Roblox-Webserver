using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Common;
using Games;

namespace Api.Controllers
{
    /// <summary>
    /// Endpoints for the universal points system, called by the game server's PointsService.
    ///   GET  /points/get-point-balance?userId=X[&placeId=Y]  -> { pointBalance }
    ///   POST /points/award-points?placeId=P&userId=U&amount=A -> { success, pointsAwarded, userGameBalance, userBalance }
    /// Balance reads are open; awards require the game-server access key header.
    /// </summary>
    [ApiController]
    [Route("points")]
    public class PointsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public PointsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("get-point-balance")]
        public async Task<IActionResult> GetPointBalance([FromQuery] long userId, [FromQuery] long? placeId = null)
        {
            try
            {
                if (userId <= 0)
                    return BadRequest(new { error = "Invalid userId" });

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);

                if (placeId.HasValue && placeId.Value > 0)
                {
                    var universeId = await GamesRepository.GetUniverseIdFromPlaceIdAsync(connectionString, placeId.Value);
                    if (!universeId.HasValue)
                        return Ok(new { pointBalance = 0 });

                    var universeBalance = await Points.GetUniversePointsAsync(connectionString, userId, universeId.Value);
                    return Ok(new { pointBalance = universeBalance });
                }

                var totalBalance = await Points.GetUserTotalPointsAsync(connectionString, userId);
                return Ok(new { pointBalance = totalBalance });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpPost("award-points")]
        public async Task<IActionResult> AwardPoints([FromQuery] long placeId, [FromQuery] long userId, [FromQuery] long amount)
        {
            try
            {
                var accessKeyHeader = Request.Headers["Accesskey"].FirstOrDefault();
                var expectedKey = _configuration["Arbiter:AccessKey"];
                if (string.IsNullOrWhiteSpace(expectedKey) ||
                    !string.Equals(accessKeyHeader, expectedKey, StringComparison.Ordinal))
                {
                    return Unauthorized(new { success = false, message = "Unauthorized" });
                }

                if (placeId <= 0)
                    return BadRequest(new { success = false, message = "Invalid placeId" });
                if (userId <= 0)
                    return BadRequest(new { success = false, message = "Invalid userId" });
                if (amount <= 0)
                    return BadRequest(new { success = false, message = "Amount must be a positive integer" });

                var maxAwardPerCall = 1000000;
                if (int.TryParse(_configuration["Points:MaxAwardPerCall"], out var configuredMax) && configuredMax > 0)
                    maxAwardPerCall = configuredMax;
                if (amount > maxAwardPerCall)
                    return BadRequest(new { success = false, message = $"Amount cannot exceed {maxAwardPerCall} per call" });

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);

                var universeId = await GamesRepository.GetUniverseIdFromPlaceIdAsync(connectionString, placeId);
                if (!universeId.HasValue)
                    return Ok(new { success = false, message = "Place not found" });

                var result = await Points.AwardPointsAsync(connectionString, userId, universeId.Value, amount);
                if (!result.Success)
                    return Ok(new { success = false, message = result.Error });

                return Ok(new
                {
                    success = true,
                    pointsAwarded = result.PointsAwarded,
                    userGameBalance = result.UserGameBalance,
                    userBalance = result.UserTotalBalance
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }
    }
}
