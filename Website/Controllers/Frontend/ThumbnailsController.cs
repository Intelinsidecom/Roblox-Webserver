using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Thumbnails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using Npgsql;
using Users;
using System.IO;
using Avatar;
using System.Security.Cryptography;
using Assets;
using System.Data;

namespace Website.Controllers;

[ApiController]
public class ThumbnailsController : ControllerBase
{
    private readonly IThumbnailService _thumbnailService;
    private readonly IConfiguration _configuration;
    private readonly AssetMetadataRepository _assetMetadataRepository;

    public ThumbnailsController(IThumbnailService thumbnailService, IConfiguration configuration)
    {
        _thumbnailService = thumbnailService;
        _configuration = configuration;
        _assetMetadataRepository = new AssetMetadataRepository();
    }

    private sealed class ItemThumbnailRequest
    {
        public string? imageSize { get; set; }
        public bool noClick { get; set; }
        public bool noOverlays { get; set; }
        public long assetId { get; set; }
    }

    private sealed class AvatarThumbnailRequest
    {
        public string? imageSize { get; set; }
        public bool noClick { get; set; }
        public bool noOverlays { get; set; }
        public long userId { get; set; }
        public long userOutfitId { get; set; }
        public string? name { get; set; }
    }

    // JSONP endpoint used by JS/modules/Widgets/ItemImage.js
    // GET /item-thumbnails?jsoncallback=foo&params=[{...}]
    [HttpGet("item-thumbnails")]
    public async Task<IActionResult> ItemThumbnails([FromQuery] string? jsoncallback, [FromQuery(Name = "params")] string? rawParams)
    {
        var results = new List<object?>();

        if (!string.IsNullOrWhiteSpace(rawParams))
        {
            try
            {
                using var doc = JsonDocument.Parse(rawParams);
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    var connStr = _configuration.GetConnectionString("Default");

                    foreach (var elem in doc.RootElement.EnumerateArray())
                    {
                        long assetId = 0;
                        if (elem.TryGetProperty("assetId", out var assetProp))
                        {
                            if (assetProp.ValueKind == JsonValueKind.Number)
                            {
                                assetId = assetProp.GetInt64();
                            }
                            else if (assetProp.ValueKind == JsonValueKind.String && long.TryParse(assetProp.GetString(), out var parsed))
                            {
                                assetId = parsed;
                            }
                        }

                        if (assetId <= 0)
                        {
                            results.Add(null);
                            continue;
                        }

                        string name = $"Item {assetId}";
                        string thumbUrl = "/images/RobloxLogo.png";

                        if (!string.IsNullOrWhiteSpace(connStr))
                        {
                            try
                            {
                                var asset = await _assetMetadataRepository.GetAssetByIdAsync(connStr, assetId).ConfigureAwait(false);
                                if (asset != null)
                                {
                                    if (!string.IsNullOrWhiteSpace(asset.Name))
                                    {
                                        name = asset.Name;
                                    }

                                    if (!string.IsNullOrWhiteSpace(asset.ThumbnailUrl))
                                    {
                                        thumbUrl = asset.ThumbnailUrl!;
                                    }
                                }
                            }
                            catch
                            {
                                // Fallback to defaults on DB error
                            }
                        }

                        var itemUrl = $"/catalog/{assetId}/";

                        results.Add(new
                        {
                            url = itemUrl,
                            name,
                            thumbnailUrl = thumbUrl,
                            thumbnailFinal = true
                        });
                    }
                }
            }
            catch
            {
                // Malformed JSON: return empty array
            }
        }

        var json = JsonSerializer.Serialize(results);

        if (string.IsNullOrWhiteSpace(jsoncallback))
        {
            return Content(json, "application/json");
        }

        var script = $"{jsoncallback}({json});";
        return Content(script, "application/javascript");
    }

    // JSONP endpoint used by JS/modules/Widgets/AvatarImage.js
    // GET /avatar-thumbnails?jsoncallback=foo&params=[{...}]
    [HttpGet("avatar-thumbnails")]
    public async Task<IActionResult> AvatarThumbnails([FromQuery] string? jsoncallback, [FromQuery(Name = "params")] string? rawParams)
    {
        var results = new List<object?>();

        if (!string.IsNullOrWhiteSpace(rawParams))
        {
            try
            {
                using var doc = JsonDocument.Parse(rawParams);
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    var connStr = _configuration.GetConnectionString("Default");
                    var pendingPlaceholder = _configuration["Thumbnails:PendingAvatarPlaceholderUrl"];
                    if (string.IsNullOrWhiteSpace(pendingPlaceholder))
                    {
                        pendingPlaceholder = "/images/RobloxLogo.png";
                    }

                    foreach (var elem in doc.RootElement.EnumerateArray())
                    {
                        long userId = 0;
                        string displayName = "Player";

                        if (elem.TryGetProperty("userId", out var userIdProp))
                        {
                            if (userIdProp.ValueKind == JsonValueKind.Number)
                            {
                                userId = userIdProp.GetInt64();
                            }
                            else if (userIdProp.ValueKind == JsonValueKind.String && long.TryParse(userIdProp.GetString(), out var parsed))
                            {
                                userId = parsed;
                            }
                        }

                        if (elem.TryGetProperty("name", out var nameProp) && nameProp.ValueKind == JsonValueKind.String)
                        {
                            var n = nameProp.GetString();
                            if (!string.IsNullOrWhiteSpace(n))
                            {
                                displayName = n!;
                            }
                        }

                        if (userId <= 0)
                        {
                            results.Add(null);
                            continue;
                        }

                        var profileUrl = $"/users/{userId}/profile";

                        string thumbUrl;
                        bool thumbnailFinal;

                        if (!string.IsNullOrWhiteSpace(connStr))
                        {
                            string? existingUrl = null;
                            try
                            {
                                // For avatar widgets we serve the user's headshot image.
                                existingUrl = await ThumbnailQueries.GetUserHeadshotUrlAsync(connStr!, userId).ConfigureAwait(false);
                            }
                            catch
                            {
                            }

                            if (!string.IsNullOrWhiteSpace(existingUrl))
                            {
                                // Normal path: use the stored headshot URL.
                                thumbUrl = existingUrl!;
                                thumbnailFinal = true;
                            }
                            else
                            {
                                // No stored headshot yet. Instead of leaving the avatar
                                // in a permanently pending/blocked state, fall back to the
                                // headshot-thumbnail endpoint, which will trigger Arbiter
                                // rendering and then redirect the browser to the final URL.
                                thumbUrl = $"/headshot-thumbnail/image?userId={userId}";
                                thumbnailFinal = true;
                            }
                        }
                        else
                        {
                            // Fallback when DB is not configured: use the existing
                            // headshot-thumbnail/image endpoint as a best-effort
                            // avatar representation.
                            thumbUrl = $"/headshot-thumbnail/image?userId={userId}";
                            thumbnailFinal = true;
                        }

                        results.Add(new
                        {
                            url = profileUrl,
                            name = displayName,
                            thumbnailUrl = thumbUrl,
                            thumbnailFinal,
                            bcOverlayUrl = (string?)null
                        });
                    }
                }
            }
            catch
            {
                // Malformed JSON: return empty array
            }
        }

        var json = JsonSerializer.Serialize(results);

        if (string.IsNullOrWhiteSpace(jsoncallback))
        {
            return Content(json, "application/json");
        }

        var script = $"{jsoncallback}({json});";
        return Content(script, "application/javascript");
    }

    // Legacy endpoint used by AjaxAvatarThumbnail.js
    // GET /thumbs/rawavatar.ashx?UserID=<id>&ThumbnailFormatID=<fmt>
    [HttpGet("thumbs/rawavatar.ashx")]
    public async Task<IActionResult> RawAvatar([FromQuery] long UserID, [FromQuery] int ThumbnailFormatID)
    {
        try
        {
            if (UserID <= 0)
                return BadRequest(new { error = "UserID is required" });
            var connStr = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
                return Content("ERROR: DB_NOT_CONFIGURED", "text/plain");

            var exists = await UserQueries.UserExistsAsync(connStr, UserID);
            if (!exists)
                return Content("ERROR: USER_NOT_FOUND", "text/plain");

            var url = await ThumbnailQueries.GetUserThumbnailUrlAsync(connStr, UserID);

            if (!string.IsNullOrWhiteSpace(url))
            {
                return Content(url!, "text/plain");
            }
            // Legacy polling contract: do not trigger rendering here
            return Content("PENDING", "text/plain");
        }
        catch (Exception ex)
        {
            return Content("ERROR: " + ex.Message, "text/plain");
        }
    }

    // Disabled duplicate: handled by AvatarV1Controller
    [NonAction]
    public async Task<IActionResult> RedrawThumbnail([FromQuery] string? type, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(type))
            return BadRequest(new { error = "type is required" });
        var renderType = type.Trim().ToLowerInvariant();

        var idStr = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(idStr) || !long.TryParse(idStr, out var targetUserId) || targetUserId <= 0)
            return Unauthorized(new { error = "Authentication required" });

        try
        {
            var save = await _thumbnailService.RenderAvatarAsync(renderType, targetUserId, cancellationToken: cancellationToken);
            var hash = save.Hash;
            var baseUrl = _configuration["Thumbnails:ThumbnailUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                var scheme = string.IsNullOrWhiteSpace(Request.Scheme) ? "http" : Request.Scheme;
                var host = Request.Host.HasValue ? Request.Host.Value : "localhost";
                baseUrl = $"{scheme}://{host}/";
            }
            var fullUrl = CombineUrl(baseUrl!, save.FileName);

            var connStr = _configuration.GetConnectionString("Default");
            if (!string.IsNullOrWhiteSpace(connStr))
            {
                if (string.Equals(renderType, "headshot", StringComparison.OrdinalIgnoreCase))
                {
                    await ThumbnailQueries.SetUserHeadshotUrlAsync(connStr!, targetUserId, fullUrl, cancellationToken);
                }
                else if (string.Equals(renderType, "avatar", StringComparison.OrdinalIgnoreCase))
                {
                    await ThumbnailQueries.SetUserThumbnailUrlAsync(connStr!, targetUserId, fullUrl, cancellationToken);
                }
                // "full" renders do not persist URLs.
            }

            return Ok(new { hash, thumbnail_url = fullUrl });
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    // GET /headshot-thumbnail/image
    [HttpGet("headshot-thumbnail/image")]
    public async Task<IActionResult> Headshot([FromQuery] long userId, [FromQuery] int? width, [FromQuery] int? height, [FromQuery] string? format, CancellationToken cancellationToken)
    {
        if (userId <= 0) return BadRequest(new { error = "userId is required" });
        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr))
            return Problem("Database not configured");

        var exists = await UserQueries.UserExistsAsync(connStr, userId, cancellationToken);
        if (!exists) return NotFound(new { error = "User not found" });

        var url = await ThumbnailQueries.GetUserHeadshotUrlAsync(connStr, userId, cancellationToken);
        if (!string.IsNullOrWhiteSpace(url))
            return Redirect(url);

        // If no headshot URL yet, render now and persist, then redirect
        var save = await _thumbnailService.RenderAvatarAsync("headshot", userId, cancellationToken: cancellationToken);
        var hash = save.Hash;
        var baseUrl = _configuration["Thumbnails:ThumbnailUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            var scheme = string.IsNullOrWhiteSpace(Request.Scheme) ? "http" : Request.Scheme;
            var host = Request.Host.HasValue ? Request.Host.Value : "localhost";
            baseUrl = $"{scheme}://{host}/";
        }
        var fullUrl = CombineUrl(baseUrl!, save.FileName);
        await ThumbnailQueries.SetUserHeadshotUrlAsync(connStr, userId, fullUrl, cancellationToken);
        return Redirect(fullUrl);
    }

    // GET /bust-thumbnail/image
    // Always re-renders the avatar via Arbiter using the requested width/height
    // and updates the user's headshot_url before redirecting to the CDN URL.
    [HttpGet("bust-thumbnail/image")]
    public async Task<IActionResult> Bust([FromQuery] long userId, [FromQuery] int? width, [FromQuery] int? height, [FromQuery] string? format, CancellationToken cancellationToken)
    {
        if (userId <= 0) return BadRequest(new { error = "userId is required" });

        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr))
            return Problem("Database not configured");

        var exists = await UserQueries.UserExistsAsync(connStr, userId, cancellationToken);
        if (!exists)
            return NotFound(new { error = "User not found" });

        // Resolve target size for Arbiter. Default to 420x420 if not specified.
        var targetWidth = width.GetValueOrDefault(420);
        var targetHeight = height.GetValueOrDefault(420);
        // Build a canonical avatar configuration JSON for global caching
        var configBuilder = new AvatarRenderConfigBuilder();
        var config = await configBuilder
            .BuildAvatarRenderConfigAsync(connStr, userId, "avatar", targetWidth, targetHeight, cancellationToken)
            .ConfigureAwait(false);

        var configHash = config.configHash;

        // Global cache lookup by configuration hash
        try
        {
            var cacheRepo = new AvatarThumbnailCacheRepository();
            var (found, fileName) = await cacheRepo.TryGetAsync(connStr, configHash, cancellationToken);
            if (found && !string.IsNullOrWhiteSpace(fileName))
            {
                var baseCachedUrl = GetCdnBaseUrl();
                var cachedFullUrl = CombineUrl(baseCachedUrl, fileName!);
                return Redirect(cachedFullUrl);
            }
        }
        catch
        {
            // Cache is best-effort; fall back to rendering on errors.
        }

        // Render a fresh avatar thumbnail directly at the requested size.
        var save = await _thumbnailService.RenderAvatarAsync("avatar", userId, targetWidth, targetHeight, cancellationToken);

        // Compose full CDN URL for the rendered file.
        var baseUrl = GetCdnBaseUrl();
        var fullUrl = CombineUrl(baseUrl, save.FileName);

        // Store in global avatar thumbnail cache (best-effort)
        try
        {
            var cacheRepo = new AvatarThumbnailCacheRepository();
            await cacheRepo.UpsertAsync(connStr, configHash, save.Hash, save.FileName, "avatar", targetWidth, targetHeight, cancellationToken);
        }
        catch
        {
        }

        // Optionally persist the avatar render URL without touching the headshot entry.
        try
        {
            await ThumbnailQueries.SetUserThumbnailUrlAsync(connStr, userId, fullUrl, cancellationToken);
        }
        catch
        {
            // Best-effort persistence; continue even if update fails.
        }

        return Redirect(fullUrl);
    }

    // GET /outfit-thumbnail/image
    [HttpGet("outfit-thumbnail/image")]
    public IActionResult Outfit([FromQuery] long userOutfitId, [FromQuery] int? width, [FromQuery] int? height, [FromQuery] string? format)
        => NotFound(new { error = "outfit thumbnails not implemented" });

    // GET /game-thumbnails/image
    [HttpGet("game-thumbnails/image")]
    public async Task<IActionResult> GameThumbnail([FromQuery] long assetId, [FromQuery] int? width, [FromQuery] int? height, [FromQuery] string? format, [FromQuery] bool ignoreAssetMedia = false, [FromQuery] bool returnAutoGenerated = false, CancellationToken cancellationToken = default)
    {
        if (assetId <= 0) return BadRequest(new { error = "assetId is required" });
        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr))
            return Problem("Database not configured");

        // Get asset information
        var asset = await _assetMetadataRepository.GetAssetByIdAsync(connStr, assetId, cancellationToken);
        if (asset == null)
            return NotFound(new { error = "Asset not found" });

        // Get auto-generated thumbnail from assets table first
        string? thumbnailUrl = null;
        
        try
        {
            // Check if there's already an auto-generated thumbnail URL in the assets table
            if (asset.PlaceAutoGeneratedThumbnail && !string.IsNullOrWhiteSpace(asset.PlaceGeneratedThumbnailUrl))
            {
                thumbnailUrl = asset.PlaceGeneratedThumbnailUrl;
            }
        }
        catch (Exception ex)
        {
        }

        // If the place already has a custom or video thumbnail selected, honour that choice and
        // avoid (re)generating an auto-generated thumbnail which would flip the database flag back
        // to auto-generated. When returnAutoGenerated=true, we should force auto-generated regardless.
        if (returnAutoGenerated && (asset.PlaceCustomThumbnail || asset.PlaceVideoThumbnail))
        {
            // When returnAutoGenerated=true, ignore the current selection and force auto-generated
            // Use the place's generated thumbnail URL instead of null
            thumbnailUrl = asset.PlaceGeneratedThumbnailUrl;
        }
        else if (string.IsNullOrWhiteSpace(thumbnailUrl) && returnAutoGenerated)
        {
            try
            {
                var baseUrl = GetCdnBaseUrl();
                await PlaceThumbnail.GeneratePlaceThumbnailAsync(
                    _thumbnailService,
                    connStr,
                    assetId,
                    asset.ContentHash ?? string.Empty,
                    baseUrl,
                    width,
                    height,
                    asset?.Name ?? string.Empty,
                    cancellationToken);

                // Reload asset to fetch the newly stored generated thumbnail URL
                var updatedAsset = await _assetMetadataRepository.GetAssetByIdAsync(connStr, assetId, cancellationToken);
                if (updatedAsset?.PlaceAutoGeneratedThumbnail == true && !string.IsNullOrWhiteSpace(updatedAsset.PlaceGeneratedThumbnailUrl))
                {
                    thumbnailUrl = updatedAsset.PlaceGeneratedThumbnailUrl;
                }
            }
            catch
            {
                // If generation fails, continue to fallback
            }
        }

        // If we have a thumbnail URL, redirect to it
        if (!string.IsNullOrWhiteSpace(thumbnailUrl))
        {
            return Redirect(thumbnailUrl);
        }

        // Fallback to placeholder image
        return Redirect("/images/RobloxLogo.png");
    }

    // GET /game-icons/json
    [HttpGet("game-icons/json")]
    public async Task<IActionResult> GameIconJson([FromQuery] long assetId, [FromQuery] int? width, [FromQuery] int? height, [FromQuery] string? format, [FromQuery] bool ignoreAssetMedia = false, [FromQuery] bool returnAutoGenerated = false, CancellationToken cancellationToken = default)
    {
        if (assetId <= 0) return BadRequest(new { error = "assetId is required" });
        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr))
            return Problem("Database not configured");

        // Get asset information
        var asset = await _assetMetadataRepository.GetAssetByIdAsync(connStr, assetId, cancellationToken);
        if (asset == null)
            return NotFound(new { error = "Asset not found" });

        // Determine which icon URL to return based on parameters.
        // Behavior:
        // - returnAutoGenerated = true  => use generated icon URL if present
        // - returnAutoGenerated = false => prefer custom icon URL, then thumbnail_url
        string? iconUrl = null;

        if (returnAutoGenerated)
        {
            if (!string.IsNullOrWhiteSpace(asset.PlaceGeneratedIconUrl))
            {
                iconUrl = asset.PlaceGeneratedIconUrl;
            }
        }
        else
        {
            // Prefer custom icon if it exists; otherwise fall back to thumbnail_url
            if (asset.CustomIcon && !string.IsNullOrWhiteSpace(asset.PlaceCustomIconUrl))
            {
                iconUrl = asset.PlaceCustomIconUrl;
            }
            else if (!string.IsNullOrWhiteSpace(asset.ThumbnailUrl))
            {
                iconUrl = asset.ThumbnailUrl;
            }
        }

        // Fallback to placeholder image if the requested type does not exist
        if (string.IsNullOrWhiteSpace(iconUrl))
        {
            iconUrl = "/images/RobloxLogo.png";
        }

        return Ok(new
        {
            url = iconUrl,
            assetId,
            hasCustomIcon = asset.CustomIcon && !string.IsNullOrWhiteSpace(asset.PlaceCustomIconUrl),
            hasGeneratedIcon = asset.GeneratedIcon && !string.IsNullOrWhiteSpace(asset.PlaceGeneratedIconUrl)
        });
    }

    // GET /game-icons/image
    [HttpGet("game-icons/image")]
    public async Task<IActionResult> GameIcon([FromQuery] long assetId, [FromQuery] int? width, [FromQuery] int? height, [FromQuery] string? format, [FromQuery] bool ignoreAssetMedia = false, [FromQuery] bool returnAutoGenerated = false, CancellationToken cancellationToken = default)
    {
        if (assetId <= 0) return BadRequest(new { error = "assetId is required" });
        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr))
            return Problem("Database not configured");

        // Get asset information
        var asset = await _assetMetadataRepository.GetAssetByIdAsync(connStr, assetId, cancellationToken);
        if (asset == null)
            return NotFound(new { error = "Asset not found" });

        // Determine which icon to return based on parameters.
        // Behavior:
        // - returnAutoGenerated = true  => use generated icon URL if present
        // - returnAutoGenerated = false => prefer custom icon URL, then thumbnail_url
        string? iconUrl = null;

        if (returnAutoGenerated)
        {
            if (!string.IsNullOrWhiteSpace(asset.PlaceGeneratedIconUrl))
            {
                iconUrl = asset.PlaceGeneratedIconUrl;
            }
            else
            {
                // No generated icon exists yet, generate one on-demand
                try
                {
                    var baseUrl = GetCdnBaseUrl();
                    await PlaceThumbnail.GeneratePlaceThumbnailAsync(_thumbnailService, connStr, assetId, asset.ContentHash ?? "", baseUrl, width, height, asset?.Name ?? "", cancellationToken);
                    
                    // Get the updated asset to retrieve the generated icon URL
                    var updatedAsset = await _assetMetadataRepository.GetAssetByIdAsync(connStr, assetId, cancellationToken);
                    if (!string.IsNullOrWhiteSpace(updatedAsset?.PlaceGeneratedIconUrl))
                    {
                        iconUrl = updatedAsset.PlaceGeneratedIconUrl;
                    }
                }
                catch
                {
                    // If generation fails, continue to fallback
                }
            }
        }
        else
        {
            // Prefer custom icon if it exists; otherwise fall back to thumbnail_url
            if (asset.CustomIcon && !string.IsNullOrWhiteSpace(asset.PlaceCustomIconUrl))
            {
                iconUrl = asset.PlaceCustomIconUrl;
            }
            else if (!string.IsNullOrWhiteSpace(asset.ThumbnailUrl))
            {
                iconUrl = asset.ThumbnailUrl;
            }
        }

        // If we have a URL, redirect to it
        if (!string.IsNullOrWhiteSpace(iconUrl))
        {
            return Redirect(iconUrl);
        }

        // Fallback to placeholder image
        return Redirect("/images/RobloxLogo.png");
    }

    private string GetCdnBaseUrl()
    {
        var baseUrl = _configuration["Thumbnails:ThumbnailUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            var scheme = string.IsNullOrWhiteSpace(Request.Scheme) ? "http" : Request.Scheme;
            var host = Request.Host.HasValue ? Request.Host.Value : "localhost";
            baseUrl = $"{scheme}://{host}/";
        }
        return baseUrl!;
    }

    private static string CombineUrl(string baseUrl, string relative)
    {
        if (string.IsNullOrEmpty(baseUrl)) return relative;
        if (string.IsNullOrEmpty(relative)) return baseUrl;
        var trimmedBase = baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/";
        return trimmedBase + relative.TrimStart('/');
    }

    public sealed class BodyColorsModel
    {
        public int headColorId { get; set; }
        public int torsoColorId { get; set; }
        public int rightArmColorId { get; set; }
        public int leftArmColorId { get; set; }
        public int rightLegColorId { get; set; }
        public int leftLegColorId { get; set; }
    }

    /// <summary>
    /// POST /Thumbs/AssetMedia/PlaceMediaItemSortHandler.ashx - Handle thumbnail sorting for places
    /// Legacy endpoint that matches original Roblox URL pattern
    /// </summary>
    [HttpGet("Thumbs/AssetMedia/PlaceMediaItemSortHandler.ashx")]
    public async Task<IActionResult> PlaceMediaItemSortHandler([FromQuery] string sort)
    {
        try
        {
            // Get current user ID from claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
            {
                return Ok(new { success = false, message = "User not authenticated" });
            }

            if (string.IsNullOrWhiteSpace(sort))
            {
                return Ok(new { success = false, message = "Sort parameter is required" });
            }

            // Parse comma-separated thumbnail IDs
            var thumbnailIds = sort.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => id.Trim())
                .Where(id => long.TryParse(id, out _))
                .Select(id => long.Parse(id))
                .ToList();

            if (thumbnailIds.Count == 0)
            {
                return Ok(new { success = false, message = "No valid thumbnail IDs provided" });
            }

            var connectionString = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return Ok(new { success = false, message = "Database not configured" });
            }

            // Verify user owns the place that contains these thumbnails
            var (placeId, isValid) = await PlaceThumbnail.GetThumbnailPlaceAndOwnershipAsync(
                connectionString, thumbnailIds[0], currentUserId);

            if (!isValid)
            {
                return Ok(new { success = false, message = "Access denied" });
            }

            // Update sort orders for all thumbnails
            var success = await PlaceThumbnail.UpdateThumbnailSortOrderAsync(
                connectionString, placeId, thumbnailIds);

            if (success)
            {
                return Ok(new { 
                    success = true, 
                    message = "Thumbnail order updated successfully",
                    sortedCount = thumbnailIds.Count
                });
            }
            else
            {
                return Ok(new { success = false, message = "Failed to update thumbnail order" });
            }
        }
        catch (Exception ex)
        {
            return Ok(new { success = false, message = "An error occurred while updating thumbnail order" });
        }
    }

    /// <summary>
    /// POST /thumbnail/set-asset-media-sort-order - Modern endpoint for thumbnail sorting
    /// </summary>
    [HttpPost("thumbnail/set-asset-media-sort-order")]
    public async Task<IActionResult> SetAssetMediaSortOrder([FromForm] string sort)
    {
        // This endpoint uses the same logic as the legacy one
        return await PlaceMediaItemSortHandler(sort);
    }
}
