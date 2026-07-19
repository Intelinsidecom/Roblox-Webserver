using System.Text.Json.Serialization;
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
using Common;

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

    [HttpGet("item-thumbnails")]
    [Authorize]
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
                                existingUrl = await ThumbnailQueries.GetUserHeadshotUrlAsync(connStr!, userId).ConfigureAwait(false);
                            }
                            catch
                            {
                            }

                            if (!string.IsNullOrWhiteSpace(existingUrl))
                            {
                                thumbUrl = existingUrl!;
                                thumbnailFinal = true;
                            }
                            else
                            {
                                thumbUrl = $"/headshot-thumbnail/image?userId={userId}";
                                thumbnailFinal = true;
                            }
                        }
                        else
                        {
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
    [Authorize]
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
            return Content("PENDING", "text/plain");
        }
        catch (Exception ex)
        {
            return Content("ERROR: " + ex.Message, "text/plain");
        }
    }

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
        var targetWidth = width.GetValueOrDefault(420);
        var targetHeight = height.GetValueOrDefault(420);
        var configBuilder = new AvatarRenderConfigBuilder();
        var config = await configBuilder
            .BuildAvatarRenderConfigAsync(connStr, userId, "avatar", targetWidth, targetHeight, cancellationToken)
            .ConfigureAwait(false);

        var configHash = config.configHash;

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
        }

        var save = await _thumbnailService.RenderAvatarAsync("avatar", userId, targetWidth, targetHeight, cancellationToken);
        var baseUrl = GetCdnBaseUrl();
        var fullUrl = CombineUrl(baseUrl, save.FileName);

        try
        {
            var cacheRepo = new AvatarThumbnailCacheRepository();
            await cacheRepo.UpsertAsync(connStr, configHash, save.Hash, save.FileName, "avatar", targetWidth, targetHeight, cancellationToken);
        }
        catch
        {
        }

        try
        {
            await ThumbnailQueries.SetUserThumbnailUrlAsync(connStr, userId, fullUrl, cancellationToken);
        }
        catch
        {
            // continue even if update fails.
        }

        return Redirect(fullUrl);
    }

    [HttpGet("outfit-thumbnail/json")]
    public async Task<IActionResult> OutfitThumbnailJson([FromQuery] long userOutfitId, [FromQuery] int? width, [FromQuery] int? height, [FromQuery] string? format, CancellationToken cancellationToken)
    {
        if (userOutfitId <= 0)
            return BadRequest(new { error = "userOutfitId is required" });

        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr))
            return Ok(new GameThumbnailJsonResponse { Url = "/images/RobloxLogo.png", Final = true });

        var repo = new OutfitsRepository();
        var outfit = await repo.GetOutfitDetailsAsync(connStr, userOutfitId, cancellationToken).ConfigureAwait(false);

        string? thumbnailUrl = null;

        if (outfit != null)
        {
            if (!string.IsNullOrWhiteSpace(outfit.ThumbnailUrl))
            {
                thumbnailUrl = outfit.ThumbnailUrl;
            }
            else
            {
                thumbnailUrl = await ThumbnailQueries.GetUserThumbnailUrlAsync(connStr, outfit.UserId, cancellationToken).ConfigureAwait(false);
            }
        }

        if (string.IsNullOrWhiteSpace(thumbnailUrl))
        {
            thumbnailUrl = _configuration["DefaultThumbnailUrl"] ?? "/images/default.png";
        }
        else if (!thumbnailUrl.StartsWith("http://") && !thumbnailUrl.StartsWith("https://") && !thumbnailUrl.StartsWith("/"))
        {
            thumbnailUrl = "https://" + thumbnailUrl;
        }

        var response = new GameThumbnailJsonResponse
        {
            Url = thumbnailUrl,
            Final = true
        };

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = null };
        var json = JsonSerializer.Serialize(response, jsonOptions);
        return Content(json, "application/json");
    }

    // GET /outfit-thumbnail/image
    [HttpGet("outfit-thumbnail/image")]
    [Authorize]
    public IActionResult Outfit([FromQuery] long userOutfitId, [FromQuery] int? width, [FromQuery] int? height, [FromQuery] string? format)
        => NotFound(new { error = "outfit thumbnails not implemented" });

    // GET /control-panel/avatar
    // Endpoint for Control Panel to fetch avatar images without authentication
    [HttpGet("control-panel/avatar")]
    public async Task<IActionResult> ControlPanelAvatar([FromQuery] long userId, [FromQuery] int? width, [FromQuery] int? height, [FromQuery] string? format, CancellationToken cancellationToken)
    {
        if (userId <= 0) return BadRequest(new { error = "userId is required" });

        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr))
            return Problem("Database not configured");

        var exists = await UserQueries.UserExistsAsync(connStr, userId, cancellationToken);
        if (!exists)
            return NotFound(new { error = "User not found" });
        
        var targetWidth = width.GetValueOrDefault(420);
        var targetHeight = height.GetValueOrDefault(420);
        var configBuilder = new AvatarRenderConfigBuilder();
        var config = await configBuilder
            .BuildAvatarRenderConfigAsync(connStr, userId, "avatar", targetWidth, targetHeight, cancellationToken)
            .ConfigureAwait(false);

        var configHash = config.configHash;

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
        }

        var save = await _thumbnailService.RenderAvatarAsync("avatar", userId, targetWidth, targetHeight, cancellationToken);
        var baseUrl = GetCdnBaseUrl();
        var fullUrl = CombineUrl(baseUrl, save.FileName);

        try
        {
            var cacheRepo = new AvatarThumbnailCacheRepository();
            await cacheRepo.UpsertAsync(connStr, configHash, save.Hash, save.FileName, "avatar", targetWidth, targetHeight, cancellationToken);
        }
        catch
        {
        }

        return Redirect(fullUrl);
    }

    [HttpGet("game-thumbnails/json")]
    [HttpGet("asset-thumbnail/json")]
    public async Task<IActionResult> GameThumbnailJson([FromQuery] long assetId, [FromQuery] int? width, [FromQuery] int? height, [FromQuery] string? format, [FromQuery] bool ignoreAssetMedia = false, [FromQuery] bool returnAutoGenerated = false, CancellationToken cancellationToken = default)
    {
        if (assetId <= 0) return BadRequest(new { error = "assetId is required" });
        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr))
            return Problem("Database not configured");

        var asset = await _assetMetadataRepository.GetAssetByIdAsync(connStr, assetId, cancellationToken);
        if (asset == null)
            return NotFound(new { error = "Asset not found" });

        string? thumbnailUrl = null;
        
        if (returnAutoGenerated)
        {
            thumbnailUrl = asset.PlaceGeneratedThumbnailUrl;
        }
        else if (asset.PlaceCustomThumbnail)
        {
            var placeThumbnailUrl = await ThumbnailQueries.GetFirstPlaceThumbnailUrlAsync(connStr, assetId, cancellationToken);
            thumbnailUrl = placeThumbnailUrl ?? asset.ThumbnailUrl;
        }
        else
        {
            // Check the asset's thumbnail mode flags to determine which thumbnail to use
            if (asset.IsPlace)
            {
                if (asset.PlaceAutoGeneratedThumbnail)
                {
                    thumbnailUrl = asset.PlaceGeneratedThumbnailUrl;
                }
                else if (asset.PlaceCustomThumbnail && !string.IsNullOrWhiteSpace(asset.PlaceCustomIconUrl))
                {
                    var placeThumbnailUrl = await ThumbnailQueries.GetFirstPlaceThumbnailUrlAsync(connStr, assetId, cancellationToken);
                    thumbnailUrl = placeThumbnailUrl ?? asset.ThumbnailUrl;
                }
                else if (asset.PlaceVideoThumbnail && !string.IsNullOrWhiteSpace(asset.PlaceThumbnailVideo))
                {
                    // For video thumbnails, we might need to handle differently, but for now fall back to thumbnail_url
                    thumbnailUrl = asset.ThumbnailUrl;
                }
                else
                {
                    thumbnailUrl = asset.ThumbnailUrl;
                }
            }
            else
            {
                thumbnailUrl = asset.ThumbnailUrl;
            }
        }

        if (string.IsNullOrWhiteSpace(thumbnailUrl) && returnAutoGenerated)
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

        var isFinal = true;
        if (string.IsNullOrWhiteSpace(thumbnailUrl))
        {
            thumbnailUrl = _configuration["DefaultThumbnailUrl"] ?? "/images/default.png";
            if (!asset.IsPlace)
                isFinal = false;
        }
        else if (!thumbnailUrl.StartsWith("http://") && !thumbnailUrl.StartsWith("https://") && !thumbnailUrl.StartsWith("/"))
        {
            thumbnailUrl = "https://" + thumbnailUrl;
        }

        var response = new GameThumbnailJsonResponse
        {
            Url = thumbnailUrl,
            Final = isFinal
        };
        
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null
        };
        
        var json = JsonSerializer.Serialize(response, jsonOptions);
        return Content(json, "application/json");
    }

    public sealed class GameThumbnailJsonResponse
    {
        [JsonPropertyName("Url")]
        public string Url { get; set; } = "";
        
        [JsonPropertyName("Final")]
        public bool Final { get; set; }
    }

    [HttpGet("avatar-thumbnail/json")]
    public async Task<IActionResult> AvatarThumbnailJson([FromQuery] long userId, [FromQuery] int? width, [FromQuery] int? height, [FromQuery] string? format, CancellationToken cancellationToken)
    {
        if (userId <= 0)
            return BadRequest(new { error = "userId is required" });

        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr))
            return Ok(new GameThumbnailJsonResponse { Url = "/images/RobloxLogo.png", Final = true });

        var exists = await UserQueries.UserExistsAsync(connStr, userId, cancellationToken);
        if (!exists)
            return NotFound(new { error = "User not found" });

        var targetWidth = width.GetValueOrDefault(352);
        var targetHeight = height.GetValueOrDefault(352);

        var configBuilder = new AvatarRenderConfigBuilder();
        var config = await configBuilder
            .BuildAvatarRenderConfigAsync(connStr, userId, "avatar", targetWidth, targetHeight, cancellationToken)
            .ConfigureAwait(false);

        var configHash = config.configHash;

        try
        {
            var cacheRepo = new AvatarThumbnailCacheRepository();
            var (found, fileName) = await cacheRepo.TryGetAsync(connStr, configHash, cancellationToken);
            if (found && !string.IsNullOrWhiteSpace(fileName))
            {
                var baseCachedUrl = GetCdnBaseUrl();
                var cachedFullUrl = CombineUrl(baseCachedUrl, fileName!);
                var cb = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                return Ok(new GameThumbnailJsonResponse { Url = $"{cachedFullUrl}?_={cb}", Final = true });
            }
        }
        catch
        {
        }

        // Cache miss — render new avatar
        try
        {
            var save = await _thumbnailService.RenderAvatarAsync("avatar", userId, targetWidth, targetHeight, cancellationToken);
            var baseUrl = GetCdnBaseUrl();
            var fullUrl = CombineUrl(baseUrl, save.FileName);

            // Save to hash-based cache
            try
            {
                var cacheRepo = new AvatarThumbnailCacheRepository();
                await cacheRepo.UpsertAsync(connStr, configHash, save.Hash, save.FileName, "avatar", targetWidth, targetHeight, cancellationToken);
            }
            catch
            {
            }

            try
            {
                await ThumbnailQueries.SetUserThumbnailUrlAsync(connStr, userId, fullUrl, cancellationToken);
            }
            catch
            {
            }

            var cb2 = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return Ok(new GameThumbnailJsonResponse { Url = $"{fullUrl}?_={cb2}", Final = true });
        }
        catch
        {
            return Ok(new GameThumbnailJsonResponse { Url = "/images/RobloxLogo.png", Final = true });
        }
    }

    // GET /game-thumbnails/image
    [HttpGet("game-thumbnails/image")]
    public async Task<IActionResult> GameThumbnail([FromQuery] long assetId, [FromQuery] int? width, [FromQuery] int? height, [FromQuery] string? format, [FromQuery] bool ignoreAssetMedia = false, [FromQuery] bool returnAutoGenerated = false, CancellationToken cancellationToken = default)
    {
        if (assetId <= 0) return BadRequest(new { error = "assetId is required" });
        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr))
            return Problem("Database not configured");

        var asset = await _assetMetadataRepository.GetAssetByIdAsync(connStr, assetId, cancellationToken);
        if (asset == null)
            return NotFound(new { error = "Asset not found" });

        string? thumbnailUrl = null;
        
        if (returnAutoGenerated)
        {
            thumbnailUrl = asset.PlaceGeneratedThumbnailUrl;
        }
        else if (asset.PlaceCustomThumbnail)
        {
            var placeThumbnailUrl = await ThumbnailQueries.GetFirstPlaceThumbnailUrlAsync(connStr, assetId, cancellationToken);
            thumbnailUrl = placeThumbnailUrl ?? asset.ThumbnailUrl;
        }
        else
        {
            if (asset.IsPlace)
            {
                thumbnailUrl = asset.ThumbnailUrl;
            }
            else
            {
                thumbnailUrl = asset.ThumbnailUrl;
            }
        }

        if (string.IsNullOrWhiteSpace(thumbnailUrl) && returnAutoGenerated)
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

        if (!string.IsNullOrWhiteSpace(thumbnailUrl))
        {
            if (width.HasValue && height.HasValue && width > 0 && height > 0)
            {
                try
                {
                    var hash = CDNUtilities.ExtractHashFromUrl(thumbnailUrl);
                    if (!string.IsNullOrWhiteSpace(hash))
                    {
                        var resizedImageBytes = CDNUtilities.ResizePlaceThumbnailFromCDN(hash, width.Value, height.Value, format);
                        if (resizedImageBytes != null)
                        {
                            var contentType = CDNUtilities.GetContentType(format);
                            return File(resizedImageBytes, contentType);
                        }
                    }
                }
                catch
                {
                    // If resizing fails, fall back to original redirect
                }
            }
            
            if (Request.Headers.ContainsKey("X-Requested-With") || 
                Request.Headers.ContainsKey("Authorization") ||
                Request.Query.ContainsKey("_"))
            {
                try
                {
                    var hash = CDNUtilities.ExtractHashFromUrl(thumbnailUrl);
                    if (!string.IsNullOrWhiteSpace(hash))
                    {
                        // Try to fetch and serve the image directly
                        var imageBytes = CDNUtilities.GetPlaceThumbnailFromCDN(hash);
                        if (imageBytes != null)
                        {
                            var contentType = CDNUtilities.GetContentType(format) ?? "image/png";
                            return File(imageBytes, contentType);
                        }
                    }
                }
                catch
                {
                    // If direct fetch fails, fall back to redirect
                }
            }
            
            return Redirect(thumbnailUrl);
        }
        
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

        var asset = await _assetMetadataRepository.GetAssetByIdAsync(connStr, assetId, cancellationToken);
        if (asset == null)
            return NotFound(new { error = "Asset not found" });

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
            if (asset.CustomIcon && !string.IsNullOrWhiteSpace(asset.PlaceCustomIconUrl))
            {
                iconUrl = asset.PlaceCustomIconUrl;
            }
            else if (!string.IsNullOrWhiteSpace(asset.ThumbnailUrl))
            {
                iconUrl = asset.ThumbnailUrl;
            }
        }

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

    [HttpGet("Thumbs/Avatar.ashx")]
    public async Task<IActionResult> AvatarLegacy([FromQuery] int x, [FromQuery] int y, [FromQuery] long userId, CancellationToken cancellationToken)
    {
        if (userId <= 0)
            return NotFound();

        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr))
            return NotFound();

        var exists = await UserQueries.UserExistsAsync(connStr, userId, cancellationToken);
        if (!exists)
            return NotFound();

        var url = await ThumbnailQueries.GetUserThumbnailUrlAsync(connStr, userId, cancellationToken);

        if (!string.IsNullOrWhiteSpace(url))
        {
            var fileName = Path.GetFileName(new Uri(url).AbsolutePath);
            var outputDir = _configuration["Thumbnails:OutputDirectory"];
            if (!string.IsNullOrWhiteSpace(outputDir))
            {
                var filePath = Path.Combine(outputDir, fileName);
                if (System.IO.File.Exists(filePath))
                    return Redirect(url);
            }
        }

        var save = await _thumbnailService.RenderAvatarAsync("avatar", userId, cancellationToken: cancellationToken);
        var baseUrl = GetCdnBaseUrl();
        var fullUrl = CombineUrl(baseUrl, save.FileName);

        await ThumbnailQueries.SetUserThumbnailUrlAsync(connStr, userId, fullUrl, cancellationToken);

        return Redirect(fullUrl);
    }

    // GET /game-icons/image
    [HttpGet("game-icons/image")]
    [Authorize]
    public async Task<IActionResult> GameIcon([FromQuery] long assetId, [FromQuery] int? width, [FromQuery] int? height, [FromQuery] string? format, [FromQuery] bool ignoreAssetMedia = false, [FromQuery] bool returnAutoGenerated = false, CancellationToken cancellationToken = default)
    {
        if (assetId <= 0) return BadRequest(new { error = "assetId is required" });
        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr))
            return Problem("Database not configured");

        var asset = await _assetMetadataRepository.GetAssetByIdAsync(connStr, assetId, cancellationToken);
        if (asset == null)
            return NotFound(new { error = "Asset not found" });

        string? iconUrl = null;

        if (returnAutoGenerated)
        {
            if (!string.IsNullOrWhiteSpace(asset.PlaceGeneratedIconUrl))
            {
                iconUrl = asset.PlaceGeneratedIconUrl;
            }
            else
            {
                try
                {
                    var baseUrl = GetCdnBaseUrl();
                    await PlaceThumbnail.GeneratePlaceThumbnailAsync(_thumbnailService, connStr, assetId, asset.ContentHash ?? "", baseUrl, width, height, asset?.Name ?? "", cancellationToken);
                    var updatedAsset = await _assetMetadataRepository.GetAssetByIdAsync(connStr, assetId, cancellationToken);
                    if (!string.IsNullOrWhiteSpace(updatedAsset?.PlaceGeneratedIconUrl))
                    {
                        iconUrl = updatedAsset.PlaceGeneratedIconUrl;
                    }
                }
                catch
                {
                }
            }
        }
        else
        {
            if (asset.CustomIcon && !string.IsNullOrWhiteSpace(asset.PlaceCustomIconUrl))
            {
                iconUrl = asset.PlaceCustomIconUrl;
            }
            else if (!string.IsNullOrWhiteSpace(asset.ThumbnailUrl))
            {
                iconUrl = asset.ThumbnailUrl;
            }
        }

        if (!string.IsNullOrWhiteSpace(iconUrl))
        {
            return Redirect(iconUrl);
        }

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
    [Authorize]
    public async Task<IActionResult> PlaceMediaItemSortHandler([FromQuery] string sort)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
            {
                return Ok(new { success = false, message = "User not authenticated" });
            }

            if (string.IsNullOrWhiteSpace(sort))
            {
                return Ok(new { success = false, message = "Sort parameter is required" });
            }

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

            var (placeId, isValid) = await PlaceThumbnail.GetThumbnailPlaceAndOwnershipAsync(
                connectionString, thumbnailIds[0], currentUserId);

            if (!isValid)
            {
                return Ok(new { success = false, message = "Access denied" });
            }

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
    [Authorize]
    public async Task<IActionResult> SetAssetMediaSortOrder([FromForm] string sort)
    {
        return await PlaceMediaItemSortHandler(sort);
    }
}
