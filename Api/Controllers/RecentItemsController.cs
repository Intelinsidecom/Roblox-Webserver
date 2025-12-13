using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("v1/recent-items")]
    public class RecentItemsController : ControllerBase
    {
        [HttpGet("{type}/list")]
        public async Task<IActionResult> GetRecentItems(string type, [FromServices] IConfiguration config)
        {
            long userId = 0;
            var claimVal = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(claimVal))
                long.TryParse(claimVal, out userId);

            if (userId <= 0)
            {
                return StatusCode(403);
            }

            var connString = config.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connString))
                return StatusCode(500);

            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();

            // For now we only support the "All" type, which returns all clothing assets
            // in the user's inventory that are relevant to the avatar editor (T-Shirts and Pants).
            // Later this can be extended to support other list types.
            string sql = @"select a.asset_id,
       a.name,
       a.asset_type_id,
       a.thumbnail_url
from user_assets ua
join assets a on a.asset_id = ua.asset_id
where ua.user_id = @user_id
  and a.asset_type_id in (2, 12)
order by ua.created_at desc
limit 50;";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("user_id", userId);

            var items = new List<object>();
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var assetId = reader.GetInt64(0);
                    var name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                    var assetTypeId = reader.GetInt32(2);
                    var thumbnailUrl = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);

                    items.Add(new
                    {
                        id = assetId,
                        name,
                        assetTypeId,
                        thumbnailUrl
                    });
                }
            }

            var payload = new
            {
                data = items
            };

            return Ok(payload);
        }
    }
}
