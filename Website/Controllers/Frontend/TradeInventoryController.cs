using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Website.Controllers;

[ApiController]
[Route("v1/users/{userId}/assets/collectibles")]
public class TradeInventoryController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public TradeInventoryController(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    [HttpGet]
    public async Task<IActionResult> GetCollectibles(
        long userId,
        [FromQuery] string? cursor,
        [FromQuery] int? assetType,
        [FromQuery] int limit = 25,
        [FromQuery] string? sortOrder = "Desc",
        CancellationToken cancellationToken = default)
    {
        if (limit <= 0 || limit > 100)
            limit = 25;

        var isDesc = string.IsNullOrWhiteSpace(sortOrder) ||
                     sortOrder.Equals("Desc", StringComparison.OrdinalIgnoreCase);

        long? cursorAssetId = null;
        if (!string.IsNullOrWhiteSpace(cursor) && long.TryParse(cursor, out var parsedCursor) && parsedCursor > 0)
        {
            cursorAssetId = parsedCursor;
        }

        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr))
        {
            return new JsonResult(new { data = new List<object>(), nextPageCursor = (string?)null });
        }

        try
        {
            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            var userCheck = new NpgsqlCommand("SELECT 1 FROM users WHERE user_id = @userId", conn);
            userCheck.Parameters.AddWithValue("userId", userId);
            var userExists = await userCheck.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            if (userExists == null)
            {
                return new JsonResult(new
                {
                    data = new List<object>(),
                    nextPageCursor = (string?)null
                });
            }

            var sql = @"SELECT ua.user_asset_id,
       a.asset_id,
       a.name,
       a.recent_average_price,
       a.price,
       aser.serial_number,
       a.limited_quantity,
       u.membership_status
FROM user_assets ua
JOIN assets a ON a.asset_id = ua.asset_id
JOIN users u ON u.user_id = ua.user_id
LEFT JOIN asset_serials aser ON aser.asset_id = ua.asset_id AND aser.owner_user_id = ua.user_id
WHERE ua.user_id = @userId";

            if (assetType.HasValue)
            {
                sql += " AND a.asset_type_id = @assetType";
            }

            if (cursorAssetId.HasValue)
            {
                sql += isDesc ? " AND a.asset_id < @cursorAssetId" : " AND a.asset_id > @cursorAssetId";
            }

            sql += isDesc ? " ORDER BY a.asset_id DESC" : " ORDER BY a.asset_id ASC";
            sql += " LIMIT @limit";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("userId", userId);
            cmd.Parameters.AddWithValue("limit", limit + 1);
            if (assetType.HasValue)
                cmd.Parameters.AddWithValue("assetType", assetType.Value);
            if (cursorAssetId.HasValue)
                cmd.Parameters.AddWithValue("cursorAssetId", cursorAssetId.Value);

            var items = new List<object>();
            long? nextCursor = null;

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var userAssetId = reader.GetInt64(0);
                var assetId = reader.GetInt64(1);
                var name = reader.IsDBNull(2) ? "Unnamed" : reader.GetString(2);
                var recentAveragePrice = reader.IsDBNull(3) ? 0 : reader.GetInt64(3);
                var price = reader.IsDBNull(4) ? 0 : reader.GetInt64(4);
                var serialNumber = reader.IsDBNull(5) ? (long?)null : reader.GetInt64(5);
                var limitedQuantity = reader.IsDBNull(6) ? (long?)null : reader.GetInt64(6);
                var membershipStatus = reader.IsDBNull(7) ? 0 : reader.GetInt16(7);

                if (items.Count >= limit)
                {
                    nextCursor = assetId;
                    break;
                }

                items.Add(new Dictionary<string, object?>
                {
                    ["userAssetId"] = userAssetId,
                    ["name"] = name,
                    ["assetId"] = assetId,
                    ["recentAveragePrice"] = recentAveragePrice,
                    ["originalPrice"] = price,
                    ["serialNumber"] = serialNumber,
                    ["assetStock"] = limitedQuantity,
                    ["buildersClubMembershipType"] = membershipStatus
                });
            }

            return new JsonResult(new
            {
                data = items,
                nextPageCursor = nextCursor?.ToString()
            });
        }
        catch (Exception)
        {
            return new JsonResult(new
            {
                data = new List<object>(),
                nextPageCursor = (string?)null,
                errors = new[] { new { code = 0, message = "Error loading inventory" } }
            });
        }
    }
}