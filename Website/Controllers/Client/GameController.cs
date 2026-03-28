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

        public GameController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

    [HttpPost("game/report-stats")]
    public IActionResult ReportStats()
    {
        return Ok("success");
    }

    [HttpPost("/game/validate-machine")]
    public IActionResult ValidateMachine()
    {
        return Ok("success");
    }

    [HttpGet("/Game/ClientPresence.ashx")]
    public async Task<IActionResult> ClientPresence(long? userID, long? PlaceID)
    {
        if (!userID.HasValue || !PlaceID.HasValue)
        {
            return BadRequest("Missing userID or PlaceID parameter");
        }

        try
        {
            // Check if user exists in game_presence table using raw SQL
            var connection = _dbContext.Database.GetDbConnection();
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT placeid, jobid FROM game_presence WHERE uid = @uid";
            
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@uid";
            parameter.Value = userID.Value;
            command.Parameters.Add(parameter);
            
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                // User is in a game, return their current game info
                return Json(new { 
                    success = true,
                    inGame = true,
                    placeID = reader.GetInt64(0),
                    jobID = reader.GetString(1)
                });
            }
            else
            {
                // User hasn't joined any game
                return Json(new { 
                    success = false, 
                    message = "User not in game",
                    inGame = false
                });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
}