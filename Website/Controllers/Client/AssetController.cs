using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Net.Http;
using Assets;
using Users;
using Website.Services;

namespace Website.Controllers
{
    [ApiController]
    [Route("asset")]
    public class AssetController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AssetService _assetService;
        private readonly XMLTemplateService _templateService;

        public AssetController(IConfiguration configuration, IHttpClientFactory httpClientFactory, XMLTemplateService templateService)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _assetService = new AssetService(configuration, httpClientFactory);
            _templateService = templateService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsset(
            [FromQuery] long? id,
            [FromQuery] long? universeId,
            [FromQuery] string? assetName,
            [FromQuery(Name = "serverplaceid")] long? serverPlaceId = null,
            [FromQuery(Name = "ApiKey")] string? apiKey = null,
            [FromQuery(Name = "skipSigningScripts")] string? skipSigningScripts = null)
        {
            if (universeId.HasValue && !string.IsNullOrWhiteSpace(assetName))
            {
                long actualUniverseId = universeId.Value;
                if (actualUniverseId == 0)
                {
                    long? placeId = null;
                    string[] placeIdHeaders = { "Roblox-Place-Id", "X-Roblox-Place-Id", "Place-Id" };
                    foreach (var headerName in placeIdHeaders)
                    {
                        var placeIdHeader = Request.Headers[headerName].FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(placeIdHeader) && long.TryParse(placeIdHeader, out var headerPlaceId))
                        {
                            placeId = headerPlaceId;
                            break;
                        }
                    }

                    if (!placeId.HasValue && serverPlaceId.HasValue && serverPlaceId.Value > 0)
                    {
                        placeId = serverPlaceId.Value;
                    }

                    if (placeId.HasValue)
                    {
                        actualUniverseId = await GetUniverseIdFromPlaceId(placeId.Value);
                    }
                }

                if (actualUniverseId > 0)
                {
                    return await GetAssetByUniverseAlias(actualUniverseId, assetName);
                }

                return BadRequest(new { error = "Could not determine universe ID" });
            }

            if (!id.HasValue || id.Value <= 0)
                return BadRequest(new { error = "id is required" });

            var connStr = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
                return StatusCode(500, "Database connection string is not configured.");

            var metadataRepo = new AssetMetadataRepository();
            var asset = await metadataRepo.GetAssetByIdAsync(connStr, id.Value);

            var accessKeyHeader = Request.Headers["Accesskey"].FirstOrDefault();
            bool bypassAccessCheck = (!string.IsNullOrWhiteSpace(apiKey) &&
                string.Equals(apiKey, _configuration["Arbiter:AccessKey"], StringComparison.Ordinal)) ||
                (!string.IsNullOrWhiteSpace(accessKeyHeader) &&
                string.Equals(accessKeyHeader, _configuration["Arbiter:AccessKey"], StringComparison.Ordinal));

            var debugUserIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            bool isPlaceType = asset != null && (asset.IsPlace || asset.AssetTypeId == 9 || asset.AssetTypeId == 3);
            if (!bypassAccessCheck && isPlaceType && !asset.IsCopyingAllowed)
            {
                long? currentUserId = null;
                var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrWhiteSpace(userIdClaim) && long.TryParse(userIdClaim, out var parsedId))
                    currentUserId = parsedId;

                if (!currentUserId.HasValue || currentUserId.Value != asset.OwnerUserId)
                    return StatusCode(403, new { error = "This asset is copylocked and cannot be accessed." });
            }

            string? hash = asset?.ContentHash;
            string? ext = asset?.FileExtension;
            string? contentType = asset?.ContentType;
            int? assetTypeId = asset?.AssetTypeId;

            if (assetTypeId == 13 && !string.Equals(ext, ".rbxm", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(hash))
            {
                var scheme = Request.Scheme ?? "http";
                var host = Request.Host.HasValue ? Request.Host.Value : "localhost";
                var imageUrl = $"{scheme}://{host}/asset/decal-image/{id.Value}";

                var xml = _templateService.Render("decal.xml", new Dictionary<string, string>
                {
                    ["ImageUrl"] = System.Security.SecurityElement.Escape(imageUrl),
                    ["Name"] = System.Security.SecurityElement.Escape(asset?.Name ?? "Decal")
                });
                return Content(xml, "application/xml", Encoding.UTF8);
            }

            if (!string.IsNullOrWhiteSpace(hash))
            {
                var assetsRoot = _configuration["Assets:Directory"];
                if (!string.IsNullOrWhiteSpace(assetsRoot))
                {
                    var fullPath = _assetService.GetAssetFilePath(hash, ext);
                    if (!string.IsNullOrEmpty(fullPath) && System.IO.File.Exists(fullPath))
                    {
                        var ct = !string.IsNullOrWhiteSpace(contentType) && contentType.Contains('/') ? contentType : "application/octet-stream";
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

        private async Task<IActionResult> GetAssetByUniverseAlias(long universeId, string assetName)
        {
            var connStr = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
                return StatusCode(500, "Database connection string is not configured.");

            try
            {
                var aliasesJson = await Games.GamesRepository.GetUniverseAliasesAsync(connStr, universeId);
                if (aliasesJson == null)
                    return NotFound(new { error = "Universe not found" });
                if (string.IsNullOrWhiteSpace(aliasesJson) || aliasesJson == "[]")
                    return NotFound(new { error = "No aliases found" });

                using var doc = System.Text.Json.JsonDocument.Parse(aliasesJson);
                var aliases = doc.RootElement;

                long? targetAssetId = null;
                foreach (var alias in aliases.EnumerateArray())
                {
                    if (alias.TryGetProperty("Name", out var nameProp) &&
                        nameProp.GetString() == assetName)
                    {
                        if (alias.TryGetProperty("TargetId", out var targetIdProp) &&
                            long.TryParse(targetIdProp.GetString(), out var targetId))
                        {
                            targetAssetId = targetId;
                            break;
                        }
                    }
                }

                if (!targetAssetId.HasValue)
                    return NotFound(new { error = "Alias not found" });

                var metadataRepo = new AssetMetadataRepository();
                var asset = await metadataRepo.GetAssetByIdAsync(connStr, targetAssetId.Value);

                if (asset == null)
                    return NotFound(new { error = "Asset not found for alias" });

                var hash = asset.ContentHash;
                var ext = asset.FileExtension;
                var contentType = asset.ContentType;

                if (!string.IsNullOrWhiteSpace(hash))
                {
                    var assetsRoot = _configuration["Assets:Directory"];
                    if (!string.IsNullOrWhiteSpace(assetsRoot))
                    {
                        var fullPath = _assetService.GetAssetFilePath(hash, ext);
                        if (!string.IsNullOrEmpty(fullPath) && System.IO.File.Exists(fullPath))
                        {
                        var ct = !string.IsNullOrWhiteSpace(contentType) && contentType.Contains('/') ? contentType : "application/octet-stream";
                            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
                            return File(stream, ct);
                        }
                    }
                }

                if (_assetService.IsRobloxAssetDeliveryEnabled())
                {
                    var result2 = await _assetService.TryFetchFromRobloxAssetDeliveryAsync(targetAssetId.Value, contentType);
                    if (result2.Stream != null)
                    {
                        return File(result2.Stream, result2.ContentType);
                    }
                    if (string.IsNullOrWhiteSpace(hash))
                        return NotFound(new { error = "Asset not found" });
                    return NotFound(new { error = result2.Error });
                }

                if (string.IsNullOrWhiteSpace(hash))
                    return NotFound(new { error = "Asset not found" });
                return NotFound(new { error = "Asset file not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private async Task<long> GetUniverseIdFromPlaceId(long placeId)
        {
            var connStr = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
                return 0;

            try
            {
                var universeId = await Games.GamesRepository.GetUniverseIdFromPlaceIdAsync(connStr, placeId);
                return universeId ?? 0;
            }
            catch
            {
                // Ignore errors
            }

            return 0;
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

                var xml = _templateService.Render("bodycolors.xml", new Dictionary<string, string>
                {
                    ["HeadColorId"] = bodyColors.HeadColorId.ToString(),
                    ["LeftArmColorId"] = bodyColors.LeftArmColorId.ToString(),
                    ["LeftLegColorId"] = bodyColors.LeftLegColorId.ToString(),
                    ["RightArmColorId"] = bodyColors.RightArmColorId.ToString(),
                    ["RightLegColorId"] = bodyColors.RightLegColorId.ToString(),
                    ["TorsoColorId"] = bodyColors.TorsoColorId.ToString()
                });

                return Content(xml, "application/xml", Encoding.UTF8);
            }
            catch (Exception ex)
            {
                var xml = _templateService.Render("bodycolors.xml", new Dictionary<string, string>
                {
                    ["HeadColorId"] = "1",
                    ["LeftArmColorId"] = "1",
                    ["LeftLegColorId"] = "1",
                    ["RightArmColorId"] = "1",
                    ["RightLegColorId"] = "1",
                    ["TorsoColorId"] = "1"
                });

                return Content(xml, "application/xml", Encoding.UTF8);
            }
        }

        [HttpGet("id")]
        public IActionResult AssetById()
        {
            return Content(string.Empty, "text/plain");
        }

        // GET /Asset/decal-image/{id}
        // Serves the raw image bytes for old-style decals (used by on-the-fly Decal XML wrapper)
        [HttpGet("decal-image/{id}")]
        public async Task<IActionResult> GetDecalImage(long id)
        {
            var connStr = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
                return StatusCode(500, "Database connection string is not configured.");

            var metadataRepo = new AssetMetadataRepository();
            var asset = await metadataRepo.GetAssetByIdAsync(connStr, id);

            if (asset == null || asset.AssetTypeId != 13)
                return NotFound(new { error = "Asset not found" });

            var hash = asset.ContentHash;
            var ext = asset.FileExtension;

            var fullPath = _assetService.GetAssetFilePath(hash, ext);
            if (string.IsNullOrEmpty(fullPath) || !System.IO.File.Exists(fullPath))
                return NotFound(new { error = "Asset file not found" });

            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
            return File(stream, "image/png");
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
                var ownerUserId = await AssetsRepository.GetAssetCreatorIdAsync(connStr, assetId);

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