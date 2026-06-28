using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Games;
using System;
using System.Threading.Tasks;

namespace Website.Controllers.Client
{
    [ApiController]
    public class PlacesController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public PlacesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("/places/{placeId:long}/settings")]
        public async Task<IActionResult> GetPlaceSettings(long placeId)
        {
            if (placeId <= 0)
                return BadRequest(new { error = "Invalid place ID" });

            var connectionString = _configuration.GetConnectionString("Default");
            if (string.IsNullOrEmpty(connectionString))
                return StatusCode(500, new { error = "Database not configured" });

            try
            {
                var settings = await PlacesHandler.GetPlaceSettingsAsync(connectionString, placeId);
                if (settings == null)
                    return NotFound(new { error = "Place not found" });

                return Ok(new
                {
                    Creator = new
                    {
                        Id = settings.OwnerId,
                        CreatorType = settings.CreatorType,
                        CreatorTargetId = settings.CreatorTargetId,
                        Name = settings.OwnerName
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
            }
        }
    }
}
