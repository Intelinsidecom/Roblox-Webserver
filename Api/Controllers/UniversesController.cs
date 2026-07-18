using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Threading.Tasks;
using Games;
using Common;
using Api.Services;

namespace Api.Controllers
{
    [ApiController]
    [Route("universes")]
    public class UniversesController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly CurrentUserService _currentUserService;
        private readonly GamePresenceService _gamePresenceService;

        public UniversesController(IConfiguration configuration, CurrentUserService currentUserService, GamePresenceService gamePresenceService)
        {
            _configuration = configuration;
            _currentUserService = currentUserService;
            _gamePresenceService = gamePresenceService;
        }

        [HttpGet("get-universe-containing-place")]
        public async Task<IActionResult> GetUniverseContainingPlace([FromQuery] long placeId)
        {
            if (placeId <= 0)
            {
                return BadRequest(new { error = "Invalid place ID" });
            }

            try
            {
                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var universeId = await GamesRepository.GetUniverseIdFromPlaceIdAsync(connectionString, placeId);
                return Ok(new { UniverseId = universeId ?? 0 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
            }
        }


        [HttpGet("{universeId}/cloudeditenabled")]
        public async Task<IActionResult> CloudEditEnabled(long universeId)
        {
            if (universeId <= 0)
            {
                return BadRequest(new { error = "Invalid universe ID" });
            }

            try
            {
                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var universe = await GamesRepository.GetUniverseAsync(connectionString, universeId);
                return Ok(new { enabled = false /*universe != null*/ });
            }
            catch
            {
                return Ok(new { enabled = false });
            }
        }

        [HttpPost("{universeId}/enablecloudedit")]
        public async Task<IActionResult> EnableCloudEdit(long universeId)
        {
            return Ok(new { success = true });
        }

        [HttpPost("{universeId}/disablecloudedit")]
        public async Task<IActionResult> DisableCloudEdit(long universeId)
        {
            return Ok(new { success = true });
        }

        [HttpGet("get-info")]
        public async Task<IActionResult> GetUniverseInfo([FromQuery] long? universeId, [FromQuery] long? placeId)
        {
            if ((universeId == null || universeId <= 0) && (placeId == null || placeId <= 0))
            {
                return BadRequest(new { error = "Invalid universe ID" });
            }

            try
            {
                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);

                if (placeId > 0 && (universeId == null || universeId <= 0))
                {
                    var resolved = await GamesRepository.GetUniverseIdFromPlaceIdAsync(connectionString, placeId.Value);
                    if (resolved == null)
                    {
                        return NotFound(new { error = "Universe not found" });
                    }
                    universeId = resolved.Value;
                }

                var universe = await GamesRepository.GetUniverseAsync(connectionString, universeId!.Value);
                if (universe == null)
                {
                    return NotFound(new { error = "Universe not found" });
                }

                return Ok(new
                {
                    Name = universe.Name,
                    Description = "",
                    RootPlace = universe.RootPlaceId,
                    StudioAccessToApisAllowed = universe.Studio_Access_To_APIs,
                    CurrentUserHasEditPermissions = true,
                    UniverseAvatarType = 1
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpPost("create-alias-v2")]
        public async Task<IActionResult> CreateAliasV2(
            [FromQuery] long universeId,
            [FromBody] CreateAliasV2Request body)
        {
            if (universeId <= 0)
            {
                return BadRequest(new { Success = false, Message = "Invalid universe ID" });
            }

            if (body == null || string.IsNullOrWhiteSpace(body.Name) || string.IsNullOrWhiteSpace(body.Type) || string.IsNullOrWhiteSpace(body.TargetId))
            {
                return BadRequest(new { Success = false, Message = "Missing required fields: Name, Type, TargetId" });
            }

            try
            {
                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);

                var universe = await GamesRepository.GetUniverseAsync(connectionString, universeId);
                if (universe == null)
                {
                    return NotFound(new { Success = false, Message = "Universe not found" });
                }

                var aliasJson = JsonSerializer.Serialize(new
                {
                    body.Name,
                    body.Type,
                    body.TargetId
                });

                await AliasHandler.AppendAliasAsync(connectionString, universeId, aliasJson);

                return Ok(new { Success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpGet("get-aliases")]
        public async Task<IActionResult> GetAliases([FromQuery] long universeId, [FromQuery] int page = 1)
        {
            if (universeId <= 0)
            {
                return BadRequest(new { error = "Invalid universe ID" });
            }

            try
            {
                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var aliasesJson = await GamesRepository.GetUniverseAliasesAsync(connectionString, universeId);
                var aliases = string.IsNullOrEmpty(aliasesJson) ? new object[] { } : JsonSerializer.Deserialize<object>(aliasesJson)!;

                return Ok(new
                {
                    FinalPage = true,
                    Aliases = aliases,
                    PageSize = 50
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpGet("get-universe-places")]
        public async Task<IActionResult> GetUniversePlaces([FromQuery] long universeId, [FromQuery] int page = 1)
        {
            if (universeId <= 0)
            {
                return BadRequest(new { error = "Invalid universe ID" });
            }

            try
            {
                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);

                var universe = await GamesRepository.GetUniverseAsync(connectionString, universeId);
                if (universe == null)
                {
                    return NotFound(new { error = "Universe not found" });
                }

                var placeIds = await GamesQueries.GetUniversePlaceIdsAsync(universeId, connectionString);
                var places = await GamesQueries.GetPlacesByIdsAsync(placeIds, connectionString);

                return Ok(new
                {
                    FinalPage = true,
                    RootPlace = universe.RootPlaceId,
                    Places = places.Select(p => new { PlaceId = p.PlaceId, Name = p.Name }),
                    PageSize = places.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpGet("validate-place-join")]
        public async Task<string> ValidatePlaceJoin(
            [FromQuery] long? originPlaceId,
            [FromQuery] long? destinationPlaceId)
        {
            var placeId = destinationPlaceId ?? originPlaceId;
            
            if (placeId == null || placeId <= 0)
            {
                return "false";
            }

            try
            {
                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var isValid = await GamesRepository.ValidatePlaceJoinAsync(connectionString, placeId.Value);
                return isValid ? "true" : "false";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] ValidatePlaceJoin placeId={placeId}: {ex}");
                return "false";
            }
        }

        [HttpGet("get-player-place-instance")]
        public async Task<IActionResult> GetPlayerPlaceInstance(
            [FromQuery] long currentPlaceId,
            [FromQuery] long userId)
        {
            if (userId <= 0)
            {
                return BadRequest(new { error = "Invalid user ID" });
            }

            try
            {
                var authUserId = await _currentUserService.GetUserIdAsync();
                if (authUserId <= 0 || authUserId != userId)
                {
                    return Unauthorized(new { error = "You can only query your own session" });
                }

                var presence = await _gamePresenceService.GetUserGamePresenceAsync(userId);
                if (presence == null)
                {
                    return Ok(new { });
                }

                return Ok(new
                {
                    placeId = presence.PlaceId,
                    jobId = presence.JobId,
                    updatedAt = presence.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
            }
        }

    }

    public class CreateAliasV2Request
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string TargetId { get; set; } = "";
    }
}
