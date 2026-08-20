using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using Users;
using System;
using System.Threading.Tasks;
using Games;

namespace Api.Controllers
{
    [ApiController]
    [Route("my")]
    public class MyController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly BalancesRepository _balances = new BalancesRepository();
        private readonly TokenService _tokenService;

        public MyController(IConfiguration config, TokenService tokenService)
        {
            _config = config;
            _tokenService = tokenService;
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance()
        {
            long userId = await GetAuthenticatedUserIdAsync();
            if (userId <= 0)
                return StatusCode(403);

            var connStr = _config.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
                return StatusCode(500);

            var ub = await _balances.GetUserBalanceAsync(connStr, userId);
            return Ok(new { robux = ub.Robux, tickets = ub.Tickets });
        }

        [HttpGet("currency/balance")]
        public async Task<IActionResult> GetCurrencyBalance()
        {
            long userId = await GetAuthenticatedUserIdAsync();
            if (userId <= 0)
                return StatusCode(403);

            var connStr = _config.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
                return StatusCode(500);

            var ub = await _balances.GetUserBalanceAsync(connStr, userId);
            return Ok(new { robux = ub.Robux, tickets = ub.Tickets });
        }

        [HttpGet("economy-status")]
        public IActionResult GetEconomyStatus()
        {
            return Ok(new { isMarketplaceEnabled = true });
        }

        [HttpGet("platform-currency-budget")]
        public async Task<IActionResult> GetPlatformCurrencyBudget()
        {
            long userId = await GetAuthenticatedUserIdAsync();
            if (userId <= 0)
                return StatusCode(403);

            return Ok(0);
        }

        private async Task<long> GetAuthenticatedUserIdAsync()
        {
            var claimVal = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(claimVal) && long.TryParse(claimVal, out var userId) && userId > 0)
                return userId;

            var cookie = Request.Cookies[".ROBLOSECURITY"];
            if (string.IsNullOrWhiteSpace(cookie))
                return 0;

            var id = await _tokenService.ValidateSessionAsync(cookie);
            return id ?? 0;
        }
    }
}
