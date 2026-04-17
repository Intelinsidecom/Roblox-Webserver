using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System;
using System.Text;

namespace Website.Controllers.Client
{
    /// <summary>
    /// Callback translator service - forwards game server callbacks from website to Arbiter
    /// </summary>
    [ApiController]
    [Route("gs/callback")]
    public class CallbackTranslatorController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CallbackTranslatorController> _logger;

        public CallbackTranslatorController(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<CallbackTranslatorController> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> ForwardCallback()
        {
            try
            {
                var form = await Request.ReadFormAsync();
                
                if (!form.TryGetValue("CallbackToken", out var tokenValue) || 
                    string.IsNullOrWhiteSpace(tokenValue))
                {
                    return BadRequest(new { error = "CallbackToken is required" });
                }
                
                var token = tokenValue.ToString();
                var arbiterUrl = _configuration["Arbiter:Url"] ?? "http://localhost:5000";
                var arbiterCallbackUrl = $"{arbiterUrl}/gs/callback";
                var content = new FormUrlEncodedContent(form.Select(kv => new KeyValuePair<string, string>(kv.Key, kv.Value)));
                var response = await _httpClient.PostAsync(arbiterCallbackUrl, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return Content("OK", "text/plain");
                }
                else
                {
                    return StatusCode((int)response.StatusCode, new { error = "Failed to forward to Arbiter" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }
}
