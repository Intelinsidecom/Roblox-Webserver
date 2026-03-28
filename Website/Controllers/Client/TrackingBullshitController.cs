using Microsoft.AspNetCore.Mvc;

namespace Website.Controllers.Client
{
    [ApiController]
    public class TrackingBullshitController : ControllerBase
    {
        [HttpPost("Game/MachineConfiguration.ashx")]
        public IActionResult MachineConfiguration()
        {
            return Ok("OK");
        }

        [HttpGet("Game/MachineConfiguration.ashx")]
        public IActionResult MachineConfigurationGet()
        {
            return Ok("OK");
        }

        [HttpPost("ephemeralcounters/v1.1/Counters/Increment")]
        public IActionResult IncrementCounter([FromQuery] string apiKey, [FromQuery] string counterName, [FromQuery] int amount = 1)
        {
            return Ok("OK");
        }

        [HttpPost("ephemeralcounters/v1.0/MultiIncrement")]
        public IActionResult MultiIncrement([FromQuery] string apiKey, [FromForm] string counterNamesCsv)
        {
            return Ok("OK");
        }
    }
}