using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    public class CountersController : ControllerBase
    {
        [HttpPost]
        [HttpGet]
        [Route("v1.1/Counters/Increment")]
        public IActionResult Increment(
            [FromQuery(Name = "apiKey")] string? apiKey,
            [FromQuery(Name = "counterName")] string? counterName,
            [FromQuery(Name = "amount")] string? amount)
        {
            return NoContent();
        }
    }
}
