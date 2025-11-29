using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Website.Controllers;

[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public UsersController(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Cursor-based inventory listing used by the avatar editor.
    /// Example: /users/inventory/list-json?assetTypeId=2&cursor=&itemsPerPage=50&sortOrder=Desc
    /// </summary>
    [Authorize]
    [HttpGet("inventory/list-json")]
    public async Task<IActionResult> GetInventory(
        [FromQuery] int assetTypeId,
        [FromQuery] string? cursor,
        [FromQuery] int itemsPerPage = 50,
        [FromQuery] string? sortOrder = "Desc",
        CancellationToken cancellationToken = default)
    {
        var idStr = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(idStr) || !long.TryParse(idStr, out var userId) || userId <= 0)
            return Unauthorized(new { error = "Authentication required" });

        if (itemsPerPage <= 0 || itemsPerPage > 100)
            itemsPerPage = 50;

        var isDesc = string.IsNullOrWhiteSpace(sortOrder) ||
                     sortOrder.Equals("Desc", StringComparison.OrdinalIgnoreCase);

        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr))
        {
            var errorData = new System.Collections.Generic.Dictionary<string, object?>
            {
                ["Items"] = new System.Collections.Generic.List<object>(),
                ["nextPageCursor"] = null
            };

            var errorResponse = new System.Collections.Generic.Dictionary<string, object?>
            {
                ["isValid"] = false,
                ["Data"] = "Database connection string is not configured."
            };

            return new JsonResult(errorResponse);
        }

        long? lastAssetId = null;
        if (!string.IsNullOrWhiteSpace(cursor) && long.TryParse(cursor, out var parsedCursor) && parsedCursor > 0)
        {
            lastAssetId = parsedCursor;
        }

        try
        {
            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            var sql = @"select a.asset_id,
       a.name,
       a.thumbnail_url
from user_assets ua
join assets a on a.asset_id = ua.asset_id
where ua.user_id = @uid
  and a.asset_type_id = @assetTypeId";

            if (lastAssetId.HasValue)
            {
                if (isDesc)
                {
                    sql += " and a.asset_id < @cursorAssetId";
                }
                else
                {
                    sql += " and a.asset_id > @cursorAssetId";
                }
            }

            sql += isDesc ? " order by a.asset_id desc" : " order by a.asset_id asc";
            sql += " limit @limit";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("uid", userId);
            cmd.Parameters.AddWithValue("assetTypeId", assetTypeId);
            cmd.Parameters.AddWithValue("limit", itemsPerPage + 1); // fetch one extra to detect next page
            if (lastAssetId.HasValue)
            {
                cmd.Parameters.AddWithValue("cursorAssetId", lastAssetId.Value);
            }

            var items = new List<object>();
            long? nextCursor = null;

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var assetId = reader.GetInt64(0);
                var name = reader.IsDBNull(1) ? "Unnamed" : reader.GetString(1);
                var thumbUrl = reader.IsDBNull(2) ? null : reader.GetString(2);

                if (items.Count >= itemsPerPage)
                {
                    // This row exists only to indicate there is another page.
                    nextCursor = assetId;
                    break;
                }

                var itemObject = new System.Collections.Generic.Dictionary<string, object?>
                {
                    ["Item"] = new System.Collections.Generic.Dictionary<string, object?>
                    {
                        ["AssetId"] = assetId,
                        ["Name"] = name,
                        ["AbsoluteUrl"] = $"/catalog/{assetId}/item"
                    },
                    ["Thumbnail"] = thumbUrl == null
                        ? null
                        : new System.Collections.Generic.Dictionary<string, object?>
                        {
                            ["Url"] = thumbUrl
                        },
                    ["UserItem"] = new System.Collections.Generic.Dictionary<string, object?>
                    {
                        ["IsRentalExpired"] = false
                    }
                };

                items.Add(itemObject);
            }

            var dataObject = new System.Collections.Generic.Dictionary<string, object?>
            {
                ["Items"] = items,
                ["items"] = items,
                ["nextPageCursor"] = nextCursor?.ToString()
            };

            var response = new System.Collections.Generic.Dictionary<string, object?>
            {
                ["isValid"] = true,
                ["Data"] = dataObject,
                ["data"] = dataObject
            };

            return new JsonResult(response);
        }
        catch (Exception ex)
        {
            var errorResponse = new System.Collections.Generic.Dictionary<string, object?>
            {
                ["isValid"] = false,
                ["Data"] = ex.Message,
                ["data"] = ex.Message
            };

            return new JsonResult(errorResponse);
        }
    }
}
