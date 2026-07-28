using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Website.Controllers;

[ApiController]
[Route("v1/recent-items")]
public class RecentItemsController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public RecentItemsController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [Authorize]
    [HttpGet("{type}/list")]
    public async Task<IActionResult> GetRecentItems(string type)
    {
        long userId = 0;
        var claimVal = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(claimVal))
            long.TryParse(claimVal, out userId);

        if (userId <= 0)
        {
            return StatusCode(403);
        }

        var connString = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connString))
            return StatusCode(500);

        await using var conn = new NpgsqlConnection(connString);
        await conn.OpenAsync();

        string sql = @"select a.asset_id,
       a.name,
       a.asset_type_id,
       a.thumbnail_url
from user_assets ua
join assets a on a.asset_id = ua.asset_id
where ua.user_id = @user_id
  and a.asset_type_id in (2, 11, 12)
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

                var assetTypeName = assetTypeId switch
                {
                    2  => "T-Shirt",
                    8  => "Hat",
                    11 => "Shirt",
                    12 => "Pants",
                    17 => "Head",
                    18 => "Face",
                    19 => "Gear",
                    27 => "Torso",
                    28 => "Right Arm",
                    29 => "Left Arm",
                    30 => "Left Leg",
                    31 => "Right Leg",
                    41 => "Hair Accessory",
                    42 => "Face Accessory",
                    43 => "Neck Accessory",
                    44 => "Shoulder Accessory",
                    45 => "Front Accessory",
                    46 => "Back Accessory",
                    47 => "Waist Accessory",
                    _  => "Asset"
                };

                items.Add(new
                {
                    id = assetId,
                    name,
                    type = "Asset",
                    assetType = new { id = assetTypeId, name = assetTypeName },
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
