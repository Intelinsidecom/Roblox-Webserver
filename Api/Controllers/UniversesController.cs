using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api.Data;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("universes")]
    public class UniversesController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public UniversesController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Get universe containing a specific place
        /// GET /universes/get-universe-containing-place?placeId={placeId}
        /// </summary>
        [HttpGet("get-universe-containing-place")]
        public async Task<IActionResult> GetUniverseContainingPlace([FromQuery] long placeId)
        {
            if (placeId <= 0)
            {
                return BadRequest(new { error = "Invalid place ID" });
            }

            try
            {
                var connection = _dbContext.Database.GetDbConnection();
                await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT u.universe_id
                    FROM universes u
                    WHERE @placeId = ANY(u.place_ids)
                    LIMIT 1";

                var parameter = command.CreateParameter();
                parameter.ParameterName = "@placeId";
                parameter.Value = placeId;
                command.Parameters.Add(parameter);

                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var universeId = reader.GetInt64(0);
                    return Ok(new { UniverseId = universeId });
                }
                else
                {
                    return Ok(new { UniverseId = 0 });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Validate if a player can join a place
        /// GET /universes/validate-place-join
        /// Returns simple "true"/"false" string for 2016 server compatibility
        /// </summary>
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
                var connection = _dbContext.Database.GetDbConnection();
                await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT a.asset_id, a.asset_type_id
                    FROM assets a
                    WHERE a.asset_id = @placeId
                    LIMIT 1";

                var parameter = command.CreateParameter();
                parameter.ParameterName = "@placeId";
                parameter.Value = placeId;
                command.Parameters.Add(parameter);

                using var reader = await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    return "false";
                }

                var assetId = reader.GetInt64(0);
                var assetTypeId = reader.GetInt32(1);
                if (assetTypeId != 9)
                {
                    return "false";
                }
                return "true";
            }
            catch (Exception)
            {
                return "false";
            }
        }

        /// <summary>
        /// Get player place instance
        /// GET /universes/get-player-place-instance?currentPlaceId={placeId}&userId={userId}
        /// </summary>
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
                var connection = _dbContext.Database.GetDbConnection();
                await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT gp.placeid, gp.jobid, gp.updated_at
                    FROM game_presence gp
                    WHERE gp.userid = @userId
                    LIMIT 1";

                var parameter = command.CreateParameter();
                parameter.ParameterName = "@userId";
                parameter.Value = userId;
                command.Parameters.Add(parameter);

                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var placeId = reader.GetInt64(0);
                    var jobId = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    var updatedAt = reader.GetDateTime(2);

                    return Ok(new
                    {
                        placeId = placeId,
                        jobId = jobId,
                        updatedAt = updatedAt
                    });
                }
                else
                {
                    return Ok(new { });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
            }
        }
    }
}
