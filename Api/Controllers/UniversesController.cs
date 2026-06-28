using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api.Data;
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
        private readonly AppDbContext _dbContext;
        private readonly CurrentUserService _currentUserService;

        public UniversesController(AppDbContext dbContext, CurrentUserService currentUserService)
        {
            _dbContext = dbContext;
            _currentUserService = currentUserService;
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


        [HttpGet("{universeId}/cloudeditenabled")]
        public async Task<IActionResult> CloudEditEnabled(long universeId)
        {
            // Just not yet
            return Ok(new { enabled = false });
        }

        [HttpGet("get-info")]
        public async Task GetUniverseInfo([FromQuery] long? universeId, [FromQuery] long? placeId)
        {
            if ((universeId == null || universeId <= 0) && (placeId == null || placeId <= 0))
            {
                Response.StatusCode = 400;
                await Response.WriteAsync("{\"error\":\"Invalid universe ID\"}");
                return;
            }

            try
            {
                var connection = _dbContext.Database.GetDbConnection();
                await connection.OpenAsync();

                if (placeId > 0 && (universeId == null || universeId <= 0))
                {
                    using var resolveCmd = connection.CreateCommand();
                    resolveCmd.CommandText = @"
                        SELECT u.universe_id
                        FROM universes u
                        WHERE @placeId = ANY(u.place_ids)
                        LIMIT 1";
                    var p = resolveCmd.CreateParameter();
                    p.ParameterName = "@placeId";
                    p.Value = placeId;
                    resolveCmd.Parameters.Add(p);

                    var result = await resolveCmd.ExecuteScalarAsync();
                    if (result == null || result == DBNull.Value)
                    {
                        Response.StatusCode = 404;
                        await Response.WriteAsync("{\"error\":\"Universe not found\"}");
                        return;
                    }
                    universeId = (long)result;
                }

                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT 
                        u.name,
                        COALESCE(u.root_place_id, 0) as root_place_id,
                        u.creator_user_id,
                        u.created_at,
                        u.Studio_Access_To_APIs
                    FROM universes u
                    WHERE u.universe_id = @universeId";

                var param = command.CreateParameter();
                param.ParameterName = "@universeId";
                param.Value = universeId;
                command.Parameters.Add(param);

                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var name = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    var rootPlace = reader.IsDBNull(1) ? 0L : reader.GetInt64(1);
                    var creatorId = reader.IsDBNull(2) ? 0L : reader.GetInt64(2);
                    var studioAccess = !reader.IsDBNull(4) && reader.GetBoolean(4);

                    var json = $"{{\"Name\":{System.Text.Json.JsonSerializer.Serialize(name)},\"Description\":\"\",\"RootPlace\":{rootPlace},\"StudioAccessToApisAllowed\":{(studioAccess ? "true" : "false")},\"CurrentUserHasEditPermissions\":true,\"UniverseAvatarType\":1}}";
                    Response.ContentType = "application/json; charset=utf-8";
                    await Response.WriteAsync(json);
                    return;
                }

                Response.StatusCode = 404;
                await Response.WriteAsync("{\"error\":\"Universe not found\"}");
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                await Response.WriteAsync($"{{\"error\":\"Internal server error: {System.Text.Json.JsonSerializer.Serialize(ex.Message)}\"}}");
            }
        }

        [HttpGet("get-aliases")]
        public async Task GetAliases([FromQuery] long universeId, [FromQuery] int page = 1)
        {
            Response.ContentType = "application/json; charset=utf-8";
            await Response.WriteAsync("{\"FinalPage\":true,\"Aliases\":[],\"PageSize\":50}");
        }

        [HttpGet("get-universe-places")]
        public async Task GetUniversePlaces([FromQuery] long universeId, [FromQuery] int page = 1)
        {
            if (universeId <= 0)
            {
                Response.StatusCode = 400;
                await Response.WriteAsync("{\"error\":\"Invalid universe ID\"}");
                return;
            }

            try
            {
                var connection = _dbContext.Database.GetDbConnection();
                await connection.OpenAsync();

                long rootPlaceId;
                List<long> placeIds;

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT COALESCE(root_place_id, 0), COALESCE(place_ids, ARRAY[]::bigint[])
                        FROM universes WHERE universe_id = @uid";
                    var p = cmd.CreateParameter();
                    p.ParameterName = "@uid";
                    p.Value = universeId;
                    cmd.Parameters.Add(p);

                    using var reader = await cmd.ExecuteReaderAsync();
                    if (!await reader.ReadAsync())
                    {
                        Response.StatusCode = 404;
                        await Response.WriteAsync("{\"error\":\"Universe not found\"}");
                        return;
                    }

                    rootPlaceId = reader.IsDBNull(0) ? 0L : reader.GetInt64(0);
                    placeIds = ((long[])reader.GetValue(1)).ToList();
                }

                if (rootPlaceId > 0 && !placeIds.Contains(rootPlaceId))
                    placeIds.Insert(0, rootPlaceId);

                var places = new List<string>();
                if (placeIds.Count > 0)
                {
                    using var cmd2 = connection.CreateCommand();
                    var inParams = string.Join(",", placeIds.Select((_, i) => "@p" + i));
                    cmd2.CommandText = $"SELECT asset_id, name FROM assets WHERE asset_id IN ({inParams})";
                    for (int i = 0; i < placeIds.Count; i++)
                    {
                        var pp = cmd2.CreateParameter();
                        pp.ParameterName = "@p" + i;
                        pp.Value = placeIds[i];
                        cmd2.Parameters.Add(pp);
                    }

                    using var r2 = await cmd2.ExecuteReaderAsync();
                    while (await r2.ReadAsync())
                    {
                        var pid = r2.GetInt64(0);
                        var pname = r2.IsDBNull(1) ? "" : r2.GetString(1);
                        places.Add($"{{\"PlaceId\":{pid},\"Name\":{System.Text.Json.JsonSerializer.Serialize(pname)}}}");
                    }
                }

                var json = $"{{\"FinalPage\":true,\"RootPlace\":{rootPlaceId},\"Places\":[{string.Join(",", places)}],\"PageSize\":{places.Count}}}";
                Response.ContentType = "application/json; charset=utf-8";
                await Response.WriteAsync(json);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                await Response.WriteAsync($"{{\"error\":\"Internal server error: {System.Text.Json.JsonSerializer.Serialize(ex.Message)}\"}}");
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

