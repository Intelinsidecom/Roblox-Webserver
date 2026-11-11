using System;
using System.Net.Http;
using System.Threading.Tasks;
using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Website.Controllers.Client
{
    [ApiController]
    public class GameInstanceHeartbeatController : ControllerBase
    {
        private static readonly TimeSpan ForwardTimeout = TimeSpan.FromSeconds(5);

        private readonly IConfiguration _configuration;
        private readonly ILogger<GameInstanceHeartbeatController> _logger;

        public GameInstanceHeartbeatController(
            IConfiguration configuration,
            ILogger<GameInstanceHeartbeatController> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("v2/CreateOrUpdate")]
        [HttpPost("v2/CreateOrUpdate")]
        public async Task<IActionResult> CreateOrUpdate()
        {
            var gameId = Request.Query["gameId"].ToString();

            try
            {
                var arbiterUrl = _configuration["ArbiterUrl"];
                if (string.IsNullOrWhiteSpace(arbiterUrl))
                {
                    var arbiterHost = _configuration["Arbiter:Host"] ?? "localhost";
                    var arbiterPort = _configuration["Arbiter:Port"] ?? "5000";
                    arbiterUrl = $"http://{arbiterHost}:{arbiterPort}";
                }

                var target = $"{arbiterUrl.TrimEnd('/')}/api/gameservers/heartbeat{Request.QueryString.Value}";

                using var httpClient = HttpClientDefaults.CreateLocalClient();
                httpClient.Timeout = ForwardTimeout;

                using var response = await httpClient.GetAsync(target);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Arbiter heartbeat forward returned {StatusCode} for game {GameId}",
                        (int)response.StatusCode, gameId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to forward RCC heartbeat for game {GameId}", gameId);
            }

            return Ok(new { received = true });
        }
    }
}
