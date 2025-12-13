using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Users;

namespace Website.Controllers
{
    [ApiController]
    [Route("asset")]
    public class AssetController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AssetController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET /Asset?id={assetId}
        // Looks up the asset record and streams the underlying file from the CDN Assets directory.
        [HttpGet]
        // Accept optional serverplaceid param (ignored) so /asset/?id=123&serverplaceid=X works
        public async Task<IActionResult> GetAsset([FromQuery] long? id, [FromQuery(Name = "serverplaceid")] long? serverPlaceId = null)
        {
            if (!id.HasValue || id.Value <= 0)
                return BadRequest(new { error = "id is required" });

            var connStr = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
                return StatusCode(500, "Database connection string is not configured.");

            string? hash = null;
            string? ext = null;
            string? contentType = null;

            try
            {
                await using var conn = new NpgsqlConnection(connStr);
                await conn.OpenAsync().ConfigureAwait(false);

                const string sql = @"select content_hash, file_extension, content_type from assets where asset_id = @id";
                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("id", id.Value);

                await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
                if (await reader.ReadAsync().ConfigureAwait(false))
                {
                    hash = reader.IsDBNull(0) ? null : reader.GetString(0);
                    ext = reader.IsDBNull(1) ? null : reader.GetString(1);
                    contentType = reader.IsDBNull(2) ? null : reader.GetString(2);
                }
            }
            catch
            {
                return StatusCode(500, "Failed to query asset record.");
            }

            if (string.IsNullOrWhiteSpace(hash))
                return NotFound(new { error = "Asset not found" });

            var assetsRoot = _configuration["Assets:Directory"];
            if (string.IsNullOrWhiteSpace(assetsRoot))
                return StatusCode(500, "Assets directory is not configured.");

            var assetFolder = Path.Combine(assetsRoot, "asset");
            var fileName = string.IsNullOrWhiteSpace(ext) ? hash : hash + ext;
            var fullPath = Path.Combine(assetFolder, fileName);

            if (!System.IO.File.Exists(fullPath))
                return NotFound(new { error = "Asset file not found" });

            var ct = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType;
            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
            return File(stream, ct);
        }

        // GET /Asset/characterfetch.ashx?player={id}
        [HttpGet("characterfetch.ashx")]
        public async Task<IActionResult> CharacterFetchAshx([FromQuery] string? userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest(new { error = "userId is required" });
            return await CharacterFetchInternal(userId);
        }

        private async Task<IActionResult> CharacterFetchInternal(string? userId)
        {
            var pid = string.IsNullOrWhiteSpace(userId) ? "0" : userId;
            var scheme = string.IsNullOrEmpty(Request.Scheme) ? "http" : Request.Scheme;
            var host = Request.Host.HasValue ? Request.Host.Value : "localhost";
            var baseUrl = $"{scheme}://{host}";

            long.TryParse(pid, out var uid);

            var wornAssetIds = new System.Collections.Generic.List<long>();
            var connStr = _configuration.GetConnectionString("Default");
            if (!string.IsNullOrWhiteSpace(connStr) && uid > 0)
            {
                try
                {
                    await using var conn = new NpgsqlConnection(connStr);
                    await conn.OpenAsync().ConfigureAwait(false);

                    const string sql = @"select awa.asset_id
from avatar_worn_assets awa
join assets a on a.asset_id = awa.asset_id
where awa.user_id = @uid
order by awa.asset_id";

                    await using var cmd = new NpgsqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("uid", uid);

                    await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var assetId = reader.GetInt64(0);
                        wornAssetIds.Add(assetId);
                    }
                }
                catch
                {
                    // Ignore DB errors and fall back to hardcoded asset below
                }
            }

            var bodyColorsUrl = $"{baseUrl}/asset/bodycolors.ashx?userId={pid}";
            var urls = new System.Collections.Generic.List<string> { bodyColorsUrl };

            foreach (var assetId in wornAssetIds)
            {
                urls.Add($"{baseUrl}/asset/?id={assetId}");
            }

            // Response format (semicolon-separated URLs), per request:
            // http://your.url.here/Asset/bodycolors.ashx;http://your.url.here/Asset/?id=TSHIRT;http://your.url.here/Asset/?id=PANTS
            var body = string.Join(';', urls);

            return Content(body, "text/plain");
        }

        [HttpGet("BodyColors.ashx")]
        public async Task<IActionResult> BodyColors([FromQuery] long? userId)
        {
            if (!userId.HasValue || userId.Value <= 0)
                return BadRequest(new { error = "userId is required" });

            int head = 1, leftArm = 1, leftLeg = 1, rightArm = 1, rightLeg = 1, torso = 1;
            var uid = userId.Value;
            var connStr = _configuration.GetConnectionString("Default");

            if (!string.IsNullOrWhiteSpace(connStr))
            {
                try
                {
                    var exists = await UserQueries.UserExistsAsync(connStr, uid);
                    if (!exists)
                        return NotFound(new { error = "User not found" });

                    await using var conn = new NpgsqlConnection(connStr);
                    await conn.OpenAsync();
                    const string sql = @"select head_color, left_arm_color, left_leg_color, right_arm_color, right_leg_color, torso_color
                                          from bodycolors where user_id = @uid";
                    await using var cmd = new NpgsqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("uid", uid);
                    await using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        head = reader.IsDBNull(0) ? 1 : reader.GetInt32(0);
                        leftArm = reader.IsDBNull(1) ? 1 : reader.GetInt32(1);
                        leftLeg = reader.IsDBNull(2) ? 1 : reader.GetInt32(2);
                        rightArm = reader.IsDBNull(3) ? 1 : reader.GetInt32(3);
                        rightLeg = reader.IsDBNull(4) ? 1 : reader.GetInt32(4);
                        torso = reader.IsDBNull(5) ? 1 : reader.GetInt32(5);
                    }
                }
                catch
                {
                    // Ignore DB errors and fall back to defaults
                }
            }

            var xml = $"""
<roblox xmlns:xmime="http://www.w3.org/2005/05/xmlmime" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="http://www.roblox.com/roblox.xsd" version="4">
    <External>null</External>
    <External>nil</External>
    <Item class="BodyColors"> 
        <Properties>
            <int name="HeadColor">{head}</int>
            <int name="LeftArmColor">{leftArm}</int>
            <int name="LeftLegColor">{leftLeg}</int>
            <string name="Name">Body Colors</string>
            <int name="RightArmColor">{rightArm}</int>
            <int name="RightLegColor">{rightLeg}</int>
            <int name="TorsoColor">{torso}</int>
            <bool name="archivable">true</bool>
        </Properties>
    </Item>
</roblox>
""";

            return Content(xml, "application/xml", Encoding.UTF8);
        }

        [HttpGet("id")]
        public IActionResult AssetById()
        {
            return Content(string.Empty, "text/plain");
        }
    }
}

