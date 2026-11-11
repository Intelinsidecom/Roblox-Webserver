using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Games;
using Common;

namespace Api.Controllers
{
    /// <summary>
    /// API endpoints for game/place info
    /// </summary>
    [ApiController]
    [Route("game")]
    public class GamesController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public GamesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// POST /game/load-place-info
        /// Returns place info for the given place ID (passed via the roblox-place-id header).
        /// Mirrors the Roblox game/load-place-info endpoint.
        /// </summary>
        [HttpPost("load-place-info")]
        public async Task<IActionResult> LoadPlaceInfo()
        {
            var placeIdHeader = Request.Headers["roblox-place-id"].FirstOrDefault();
            if (!long.TryParse(placeIdHeader, out long placeId) || placeId <= 0)
            {
                return BadRequest(new { error = "Invalid or missing roblox-place-id header" });
            }

            try
            {
                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);

                // Look up the asset owner (creator) for this place
                var ownerUserId = await GamesRepository.GetAssetOwnerAsync(connectionString, placeId);
                if (ownerUserId == null)
                {
                    return NotFound(new { error = "Place not found" });
                }

                return Ok(new
                {
                    CreatorId = ownerUserId.Value,
                    CreatorType = "User",
                    PlaceVersion = placeId,
                    GameId = placeId,
                    IsRobloxPlace = ownerUserId.Value == 1
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// GET /game/validate-place-join?originPlaceId=&destinationPlaceId=
        /// Validates if a player can teleport between two places.
        /// Mirrors the Roblox game/validate-place-join endpoint.
        /// </summary>
        [HttpGet("validate-place-join")]
        public async Task<string> ValidatePlaceJoin(
            [FromQuery] long originPlaceId,
            [FromQuery] long destinationPlaceId)
        {
            // Both zero means no specific places — treat as valid
            if (originPlaceId == 0 && destinationPlaceId == 0)
            {
                return "true";
            }

            if (originPlaceId <= 0 || destinationPlaceId <= 0)
            {
                return "false";
            }

            try
            {
                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);

                // Validate the destination place exists and is a valid place (asset_type_id == 9)
                var isValid = await GamesRepository.ValidatePlaceJoinAsync(connectionString, destinationPlaceId);
                return isValid ? "true" : "false";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] ValidatePlaceJoin origin={originPlaceId} destination={destinationPlaceId}: {ex}");
                return "false";
            }
        }
    }
}
