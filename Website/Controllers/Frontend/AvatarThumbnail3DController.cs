using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Users;
using Thumbnails;
using Avatar;

namespace Website.Controllers;

[ApiController]
public class AvatarThumbnail3DController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IThumbnailService _thumbnailService;

    public AvatarThumbnail3DController(IConfiguration configuration, IThumbnailService thumbnailService)
    {
        _configuration = configuration;
        _thumbnailService = thumbnailService;
    }

    // GET /avatar-thumbnail-3d/user-avatar?userId=1&width=277&height=277
    [HttpGet("avatar-thumbnail-3d/user-avatar")]
    public async Task<IActionResult> UserAvatar([FromQuery] long userId, [FromQuery] int? width, [FromQuery] int? height)
    {
        if (userId <= 0)
            return BadRequest(new { error = "userId is required" });

        var connStr = _configuration.GetConnectionString("Default");
        if (!string.IsNullOrWhiteSpace(connStr))
        {
            var exists = await UserQueries.UserExistsAsync(connStr, userId);
            if (!exists)
                return NotFound(new { error = "User not found" });
        }

        var w = width.GetValueOrDefault(352);
        var h = height.GetValueOrDefault(352);
        var sizeSegment = $"{w}x{h}";

        var metadataUrl = Url.Action(
                              action: nameof(GetMetadata),
                              controller: "AvatarThumbnail3D",
                              values: new { userId = userId, size = sizeSegment }
                          )
                          ?? $"/avatar-thumbnail-3d/metadata/user/{userId}/{sizeSegment}";

        var response = new Avatar3DStatusResponse
        {
            Final = true,
            Url = metadataUrl
        };

        return Ok(response);
    }

    // GET /avatar-thumbnail-3d/metadata/user/{userId}/{size}
    [HttpGet("avatar-thumbnail-3d/metadata/user/{userId:long}/{size}")]
    public async Task<IActionResult> GetMetadata(long userId, string size)
    {
        if (userId <= 0)
            return BadRequest(new { error = "userId is required" });

        var connStr = _configuration.GetConnectionString("Default");
        if (!string.IsNullOrWhiteSpace(connStr))
        {
            var exists = await UserQueries.UserExistsAsync(connStr, userId);
            if (!exists)
                return NotFound(new { error = "User not found" });
        }

        // Parse requested size segment (e.g., "352x352")
        var w = 352;
        var h = 352;
        if (!string.IsNullOrWhiteSpace(size))
        {
            var parts = size.ToLowerInvariant().Split('x');
            if (parts.Length == 2)
            {
                if (int.TryParse(parts[0], out var pw) && pw > 0) w = pw;
                if (int.TryParse(parts[1], out var ph) && ph > 0) h = ph;
            }
        }

        string? configHash = null;

        // If a database connection is available, try a DB-backed 3D avatar cache based on avatar configuration hash.
        if (!string.IsNullOrWhiteSpace(connStr))
        {
            var configBuilder = new AvatarRenderConfigBuilder();
            var config = await configBuilder
                .BuildAvatarRenderConfigAsync(connStr!, userId, "avatar3d", w, h)
                .ConfigureAwait(false);

            configHash = config.configHash;

            // Global 3D cache lookup by configuration hash (best-effort).
            try
            {
                var cacheRepo = new Avatar3DThumbnailCacheRepository();
                var (found, entry) = await cacheRepo.TryGetAsync(connStr!, configHash!, default);
                if (found && entry != null)
                {
                    var cdnBaseCached = _configuration["Thumbnails:ThumbnailUrl"] ?? "https://cdn.freblx.xyz/";
                    var objRelCached = $"3DAvatar/{entry.ModelHash}/{entry.ObjFileName}";
                    var mtlRelCached = $"3DAvatar/{entry.ModelHash}/{entry.MtlFileName}";

                    var objUrlCached = CombineCdnUrl(cdnBaseCached, objRelCached);
                    var mtlUrlCached = CombineCdnUrl(cdnBaseCached, mtlRelCached);

                    return Ok(new { obj = objUrlCached, mtl = mtlUrlCached });
                }
            }
            catch
            {
                // Cache is best-effort; fall back to rendering on errors.
            }
        }

        // Cache miss or database not configured: render 3D avatar and cache models on disk.
        var cached = await _thumbnailService.RenderAvatar3DAndCacheAsync(userId, w, h);

        // Build CDN URLs for OBJ/MTL under 3DAvatar/{hash}/
        var cdnBase = _configuration["Thumbnails:ThumbnailUrl"] ?? "https://cdn.freblx.xyz/";
        var objRelative = $"3DAvatar/{cached.Hash}/{cached.ObjFileName}";
        var mtlRelative = $"3DAvatar/{cached.Hash}/{cached.MtlFileName}";

        var objUrl = CombineCdnUrl(cdnBase, objRelative);
        var mtlUrl = CombineCdnUrl(cdnBase, mtlRelative);

        // Persist mapping into avatar_3d_cache when DB is available (best-effort).
        if (!string.IsNullOrWhiteSpace(connStr) && !string.IsNullOrWhiteSpace(configHash))
        {
            try
            {
                var cacheRepo = new Avatar3DThumbnailCacheRepository();
                await cacheRepo.UpsertAsync(connStr!, configHash!, cached.Hash, cached.ObjFileName, cached.MtlFileName, w, h, default);
            }
            catch
            {
            }
        }

        // ThreeDeeThumbnails.js expects { obj, mtl } JSON from metadata URL
        return Ok(new { obj = objUrl, mtl = mtlUrl });
    }

    private static string CombineCdnUrl(string baseUrl, string relative)
    {
        if (string.IsNullOrEmpty(baseUrl)) return relative;
        if (string.IsNullOrEmpty(relative)) return baseUrl;
        var trimmedBase = baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/";
        return trimmedBase + relative.TrimStart('/');
    }

    public sealed class Avatar3DStatusResponse
    {
        [JsonPropertyName("Final")]
        public bool Final { get; set; }

        [JsonPropertyName("Url")]
        public string Url { get; set; } = string.Empty;
    }
}
