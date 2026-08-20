using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Games;

namespace Api.Controllers
{
    [ApiController]
    public class GamePlayersController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly GamePresenceService _gamePresenceService;

        public GamePlayersController(IConfiguration configuration, GamePresenceService gamePresenceService)
        {
            _configuration = configuration;
            _gamePresenceService = gamePresenceService;
        }

        [HttpGet("game/players/{placeId}")]
        public async Task<IActionResult> GetPlayersInPlace(
            long placeId,
            [FromQuery] int startRow = 0,
            [FromQuery] int maxRows = 10)
        {
            if (placeId <= 0)
            {
                return BadRequest(new { error = "Invalid place ID" });
            }

            if (maxRows > 100)
                maxRows = 100;
            if (maxRows <= 0)
                maxRows = 10;

            try
            {
                var (totalCount, players) = await _gamePresenceService.GetPlayersInPlacePaginatedAsync(
                    placeId, startRow, maxRows);

                var playerData = players.Select(p => new
                {
                    userId = p.UserId,
                    userName = p.UserName,
                    placeId = p.PlaceId,
                    jobId = p.JobId,
                    joinTime = p.CreatedAt,
                    lastUpdate = p.UpdatedAt
                }).ToList();

                return Ok(new
                {
                    totalPlayerCount = totalCount,
                    startRow = startRow,
                    maxRows = maxRows,
                    finalPage = (startRow + maxRows) >= totalCount,
                    players = playerData
                });
            }
            catch
            {
                return Ok(new
                {
                    totalPlayerCount = 0,
                    startRow = startRow,
                    maxRows = maxRows,
                    finalPage = true,
                    players = new object[] { }
                });
            }
        }

        [HttpGet("game/is-playable")]
        public async Task<IActionResult> IsPlayable(
            [FromQuery] long placeId,
            [FromServices] IConfiguration config)
        {
            if (placeId <= 0)
                return Ok(new { isPlayable = false });

            try
            {
                var connStr = config.GetConnectionString("Default");
                if (string.IsNullOrWhiteSpace(connStr))
                    return Ok(new { isPlayable = true });

                var playable = await GamesRepository.ValidatePlaceJoinAsync(connStr, placeId);
                return Ok(new { isPlayable = playable });
            }
            catch
            {
                return Ok(new { isPlayable = true });
            }
        }
    }
}
