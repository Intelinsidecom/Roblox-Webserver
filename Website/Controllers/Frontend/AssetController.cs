using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using Npgsql;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Net.Http;
using Assets;
using Users;

namespace Website.Controllers
{
    [ApiController]
    [Route("asset")]
    public class AssetController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AssetService _assetService;

        public AssetController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _assetService = new AssetService(configuration, httpClientFactory);
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
            var metadata = await _assetService.GetAssetMetadataAsync(id.Value);
            hash = metadata.Hash;
            ext = metadata.Extension;
            contentType = metadata.ContentType;

            if (!string.IsNullOrWhiteSpace(hash))
            {
                var assetsRoot = _configuration["Assets:Directory"];
                if (!string.IsNullOrWhiteSpace(assetsRoot))
                {
                    var fullPath = _assetService.GetAssetFilePath(hash, ext);
                    if (!string.IsNullOrEmpty(fullPath) && System.IO.File.Exists(fullPath))
                    {
                        var ct = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType;
                        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
                        return File(stream, ct);
                    }
                }
            }

            if (_assetService.IsRobloxAssetDeliveryEnabled())
            {
                var result = await _assetService.TryFetchFromRobloxAssetDeliveryAsync(id.Value, contentType);
                if (result.Stream != null)
                {
                    return File(result.Stream, result.ContentType);
                }
                if (string.IsNullOrWhiteSpace(hash))
                    return NotFound(new { error = "Asset not found" });
                return NotFound(new { error = result.Error });
            }

            if (string.IsNullOrWhiteSpace(hash))
                return NotFound(new { error = "Asset not found" });
            return NotFound(new { error = "Asset file not found" });
        }

        // GET /Asset/characterfetch.ashx?player={id}
        [HttpGet("characterfetch.ashx")]
        public async Task<IActionResult> CharacterFetchAshx([FromQuery] string? userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest(new { error = "userId is required" });
            
            var scheme = string.IsNullOrEmpty(Request.Scheme) ? "http" : Request.Scheme;
            var host = Request.Host.HasValue ? Request.Host.Value : "localhost";
            
            var body = await _assetService.GetCharacterFetchAsync(userId, scheme, host);
            return Content(body, "text/plain");
        }

        [HttpGet("BodyColors.ashx")]
        public async Task<IActionResult> BodyColors([FromQuery] long? userId)
        {
            if (!userId.HasValue || userId.Value <= 0)
                return BadRequest(new { error = "userId is required" });

            var uid = userId.Value;
            var connStr = _configuration.GetConnectionString("Default");

            if (string.IsNullOrWhiteSpace(connStr))
                return BadRequest(new { error = "Database not configured" });

            try
            {
                var exists = await UserQueries.UserExistsAsync(connStr, uid);
                if (!exists)
                    return NotFound(new { error = "User not found" });

                var repo = new Users.BodyColorsRepository();
                var bodyColors = await repo.GetBodyColorsAsync(connStr, uid);

                var xml = $"""
<roblox xmlns:xmime="http://www.w3.org/2005/05/xmlmime" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="http://www.roblox.com/roblox.xsd" version="4">
    <External>null</External>
    <External>nil</External>
    <Item class="BodyColors"> 
        <Properties>
            <int name="HeadColor">{bodyColors.HeadColorId}</int>
            <int name="LeftArmColor">{bodyColors.LeftArmColorId}</int>
            <int name="LeftLegColor">{bodyColors.LeftLegColorId}</int>
            <string name="Name">Body Colors</string>
            <int name="RightArmColor">{bodyColors.RightArmColorId}</int>
            <int name="RightLegColor">{bodyColors.RightLegColorId}</int>
            <int name="TorsoColor">{bodyColors.TorsoColorId}</int>
            <bool name="archivable">true</bool>
        </Properties>
    </Item>
</roblox>
""";

                return Content(xml, "application/xml", Encoding.UTF8);
            }
            catch (Exception ex)
            {
                // Return default body colors on any error
                var defaultXml = $"""
<roblox xmlns:xmime="http://www.w3.org/2005/05/xmlmime" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="http://www.roblox.com/roblox.xsd" version="4">
    <External>null</External>
    <External>nil</External>
    <Item class="BodyColors"> 
        <Properties>
            <int name="HeadColor">1</int>
            <int name="LeftArmColor">1</int>
            <int name="LeftLegColor">1</int>
            <string name="Name">Body Colors</string>
            <int name="RightArmColor">1</int>
            <int name="RightLegColor">1</int>
            <int name="TorsoColor">1</int>
            <bool name="archivable">true</bool>
        </Properties>
    </Item>
</roblox>
""";

                return Content(defaultXml, "application/xml", Encoding.UTF8);
            }
        }

        [HttpGet("id")]
        public IActionResult AssetById()
        {
            return Content(string.Empty, "text/plain");
        }

        [Authorize]
        [HttpPost("delete-from-inventory")]
        public async Task<IActionResult> DeleteFromInventory([FromForm] long assetId)
        {
            var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !long.TryParse(userIdClaim, out var userId) || userId <= 0)
                return Unauthorized(new { isValid = false, success = false, error = "Authentication required" });

            if (assetId <= 0)
                return BadRequest(new { isValid = false, success = false, error = "Invalid assetId" });

            var connStr = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
                return StatusCode(500, new { isValid = false, success = false, error = "Database connection string is not configured." });

            try
            {
                // First, ensure the asset exists and check if the current user is the creator.
                long? ownerUserId = null;
                await using (var conn = new NpgsqlConnection(connStr))
                {
                    await conn.OpenAsync().ConfigureAwait(false);

                    const string getOwnerSql = @"select owner_user_id from assets where asset_id = @asset_id";
                    await using var ownerCmd = new NpgsqlCommand(getOwnerSql, conn);
                    ownerCmd.Parameters.AddWithValue("asset_id", assetId);

                    var result = await ownerCmd.ExecuteScalarAsync().ConfigureAwait(false);
                    if (result != null && result != DBNull.Value)
                    {
                        ownerUserId = (long)result;
                    }
                }

                if (!ownerUserId.HasValue)
                {
                    return Ok(new { isValid = false, success = false, error = "Asset not found." });
                }

                if (ownerUserId.Value == userId)
                {
                    return Ok(new { isValid = false, success = false, error = "You cannot remove an asset you created from your inventory." });
                }

                var repo = new UserAssetsRepository();

                var owns = await repo.UserOwnsAssetAsync(connStr, userId, assetId).ConfigureAwait(false);
                if (!owns)
                {
                    return Ok(new { isValid = false, success = false, error = "You do not own this item." });
                }

                await repo.RemoveUserAssetAsync(connStr, userId, assetId).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return StatusCode(500, new { isValid = false, success = false, error = "Failed to remove asset from inventory" });
            }

            return Ok(new { isValid = true, success = true });
        }

    }
}