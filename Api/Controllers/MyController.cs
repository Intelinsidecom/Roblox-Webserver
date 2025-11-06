using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using Npgsql;
using Users;
using System;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("my")]
    public class MyController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly BalancesRepository _balances = new BalancesRepository();

        public MyController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance()
        {
            long userId = 0;
            var claimVal = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(claimVal))
                long.TryParse(claimVal, out userId);

            if (userId <= 0)
            {
                var cookie = Request.Cookies[".ROBLOSECURITY"];
                if (!string.IsNullOrWhiteSpace(cookie))
                {
                    try
                    {
                        var connString = _config.GetConnectionString("Default");
                        await using var conn = new NpgsqlConnection(connString);
                        await conn.OpenAsync();
                        await using var cmd = new NpgsqlCommand("select user_id from sessions where token = @t and (expires_at is null or expires_at > now() at time zone 'utc')", conn);
                        cmd.Parameters.AddWithValue("t", cookie);
                        var obj = await cmd.ExecuteScalarAsync();
                        if (obj is long uid) userId = uid;
                        else if (obj is int iid) userId = iid;
                        else if (obj != null)
                        {
                            try { userId = Convert.ToInt64(obj); } catch { userId = 0; }
                        }
                    }
                    catch { }
                }
            }

            if (userId <= 0)
                return StatusCode(403);

            var connStr = _config.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
                return StatusCode(500);

            var ub = await _balances.GetUserBalanceAsync(connStr, userId);
            return Ok(new { robux = ub.Robux, tickets = ub.Tickets });
        }
    }
}
