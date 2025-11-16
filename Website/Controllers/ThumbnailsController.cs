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
using System.Text;

namespace Website.Controllers;

[ApiController]
public class ThumbnailsController : ControllerBase
{
    private readonly IThumbnailService _thumbnailService;
    private readonly IConfiguration _configuration;

    public ThumbnailsController(IThumbnailService thumbnailService, IConfiguration configuration)
    {
        _thumbnailService = thumbnailService;
        _configuration = configuration;
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
            if (!string.IsNullOrWhiteSpace(connStr) && renderType == "headshot")
            {
                await using var conn = new NpgsqlConnection(connStr);
                await conn.OpenAsync(cancellationToken);
                await using var cmd = new NpgsqlCommand("update users set headshot_url = @u where user_id = @id", conn);
                cmd.Parameters.AddWithValue("u", fullUrl);
                cmd.Parameters.AddWithValue("id", targetUserId);
                await cmd.ExecuteNonQueryAsync(cancellationToken);
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
        await using (var conn = new NpgsqlConnection(connStr))
        {
            await conn.OpenAsync(cancellationToken);
            await using var cmd = new NpgsqlCommand("update users set headshot_url = @u where user_id = @id", conn);
            cmd.Parameters.AddWithValue("u", fullUrl);
            cmd.Parameters.AddWithValue("id", userId);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
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
        var avatarRepository = new AvatarRepository();
        var avatarStateFull = await avatarRepository.GetAvatarAsync(connStr, userId, cancellationToken);

        var wornIds = avatarStateFull.Assets ?? Array.Empty<Avatar.AvatarAssetState>();
        var wornAssetIds = wornIds
            .Select(a => a.id)
            .OrderBy(id => id)
            .ToArray();

        var configObject = new
        {
            renderType = "avatar",
            width = targetWidth,
            height = targetHeight,
            bodyColors = new
            {
                head = avatarStateFull.BodyColors.headColorId,
                torso = avatarStateFull.BodyColors.torsoColorId,
                rightArm = avatarStateFull.BodyColors.rightArmColorId,
                leftArm = avatarStateFull.BodyColors.leftArmColorId,
                rightLeg = avatarStateFull.BodyColors.rightLegColorId,
                leftLeg = avatarStateFull.BodyColors.leftLegColorId
            },
            wornAssetIds = wornAssetIds
        };

        var json = JsonSerializer.Serialize(configObject);
        string configHash;
        using (var sha = SHA256.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(json);
            var digest = sha.ComputeHash(bytes);
            var hashSb = new StringBuilder(digest.Length * 2);
            foreach (var b in digest)
                hashSb.Append(b.ToString("x2"));
            configHash = hashSb.ToString();
        }

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

        // Persist the URL as the user's headshot_url for compatibility.
        try
        {
            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync(cancellationToken);
            await using var cmd = new NpgsqlCommand("update users set headshot_url = @u where user_id = @id", conn);
            cmd.Parameters.AddWithValue("u", fullUrl);
            cmd.Parameters.AddWithValue("id", userId);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
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

    // GET /asset-thumbnail/image
    [HttpGet("asset-thumbnail/image")]
    public IActionResult Asset([FromQuery] long assetId, [FromQuery] int? width, [FromQuery] int? height, [FromQuery] string? format)
        => NotFound(new { error = "asset thumbnails not implemented" });

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
}
