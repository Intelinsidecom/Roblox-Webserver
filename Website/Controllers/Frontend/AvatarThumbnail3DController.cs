using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
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

    [HttpGet("avatar-thumbnail-3d/user-avatar")]
    [HttpGet("avatar-thumbnail-3d/json")]
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

        if (!string.IsNullOrWhiteSpace(connStr))
        {
            var configBuilder = new AvatarRenderConfigBuilder();
            var config = await configBuilder
                .BuildAvatarRenderConfigAsync(connStr!, userId, "avatar3d", w, h)
                .ConfigureAwait(false);

            configHash = config.configHash;

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

                    var aabbCached = GetAabbFromObjFile(entry.ModelHash, entry.ObjFileName);
                    return Ok(new { obj = objUrlCached, mtl = mtlUrlCached, aabb = aabbCached, camera = GetCameraForHash(entry.ModelHash) });
                }
            }
            catch
            {
            }
        }

        var cached = await _thumbnailService.RenderAvatar3DAndCacheAsync(userId, w, h, force: true);
        var cdnBase = _configuration["Thumbnails:ThumbnailUrl"] ?? "https://cdn.freblx.xyz/";
        var objRelative = $"3DAvatar/{cached.Hash}/{cached.ObjFileName}";
        var mtlRelative = $"3DAvatar/{cached.Hash}/{cached.MtlFileName}";
        var objUrl = CombineCdnUrl(cdnBase, objRelative);
        var mtlUrl = CombineCdnUrl(cdnBase, mtlRelative);

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

        var aabb = !string.IsNullOrWhiteSpace(cached.DirectoryPath)
            ? ReadAabbFromObjFile(cached.DirectoryPath, cached.ObjFileName)
            : GetAabbFromObjFile(cached.Hash, cached.ObjFileName);
        return Ok(new { obj = objUrl, mtl = mtlUrl, aabb = aabb, camera = ParseCameraJson(cached.CameraJson) ?? DefaultCamera() });
    }

    private object GetCameraForHash(string modelHash)
    {
        var baseDir = _configuration["Thumbnails:Avatar3DDirectory"];
        if (string.IsNullOrWhiteSpace(baseDir))
        {
            baseDir = @"C:\Users\Intel\Documents\GitHub\Roblox-Webserver\CDN\Assets\3DAvatar";
        }
        var cameraPath = System.IO.Path.Combine(baseDir, modelHash, "camera.json");
        if (System.IO.File.Exists(cameraPath))
        {
            try
            {
                var cameraJson = System.IO.File.ReadAllText(cameraPath);
                var cameraObj = ParseCameraJson(cameraJson);
                if (cameraObj != null)
                    return cameraObj;
            }
            catch
            {
            }
        }
        return DefaultCamera();
    }

    private static object? ParseCameraJson(string? cameraJson)
    {
        if (string.IsNullOrWhiteSpace(cameraJson))
            return null;
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(cameraJson);
            var root = doc.RootElement;
            if (root.ValueKind != System.Text.Json.JsonValueKind.Object)
                return null;
            if (!root.TryGetProperty("position", out var posEl) || posEl.ValueKind != System.Text.Json.JsonValueKind.Object)
                return null;
            if (!root.TryGetProperty("direction", out var dirEl) || dirEl.ValueKind != System.Text.Json.JsonValueKind.Object)
                return null;

            float? px = TryGetFloat(posEl, "x");
            float? py = TryGetFloat(posEl, "y");
            float? pz = TryGetFloat(posEl, "z");
            float? dx = TryGetFloat(dirEl, "x");
            float? dy = TryGetFloat(dirEl, "y");
            float? dz = TryGetFloat(dirEl, "z");
            if (px == null || py == null || pz == null || dx == null || dy == null || dz == null)
                return null;

            return new
            {
                position = new { x = px.Value, y = py.Value, z = pz.Value },
                direction = new { x = dx.Value, y = dy.Value, z = dz.Value }
            };
        }
        catch
        {
            return null;
        }
    }

    private static float? TryGetFloat(System.Text.Json.JsonElement el, string property)
    {
        if (!el.TryGetProperty(property, out var val))
            return null;
        if (val.ValueKind == System.Text.Json.JsonValueKind.Number && val.TryGetSingle(out var f))
            return f;
        return null;
    }

    private static string CombineCdnUrl(string baseUrl, string relative)
    {
        if (string.IsNullOrEmpty(baseUrl)) return relative;
        if (string.IsNullOrEmpty(relative)) return baseUrl;
        var trimmedBase = baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/";
        return trimmedBase + relative.TrimStart('/');
    }

    private object? GetAabbFromObjFile(string modelHash, string objFileName)
    {
        var baseDir = _configuration["Thumbnails:Avatar3DDirectory"];
        if (string.IsNullOrWhiteSpace(baseDir))
            return DefaultAabb();
        var dir = System.IO.Path.Combine(baseDir, modelHash);
        return ReadAabbFromObjFile(dir, objFileName);
    }

    private static object? ReadAabbFromObjFile(string directoryPath, string objFileName)
    {
        var objPath = System.IO.Path.Combine(directoryPath, objFileName);
        if (!System.IO.File.Exists(objPath))
            return DefaultAabb();

        try
        {
            var lines = System.IO.File.ReadLines(objPath);
            float minX = float.MaxValue, minY = float.MaxValue, minZ = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue, maxZ = float.MinValue;
            bool hasVertex = false;

            foreach (var line in lines)
            {
                if (line.Length < 3 || line[0] != 'v' || line[1] != ' ')
                    continue;

                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 4)
                    continue;

                if (float.TryParse(parts[1], out var x) &&
                    float.TryParse(parts[2], out var y) &&
                    float.TryParse(parts[3], out var z))
                {
                    if (x < minX) minX = x;
                    if (y < minY) minY = y;
                    if (z < minZ) minZ = z;
                    if (x > maxX) maxX = x;
                    if (y > maxY) maxY = y;
                    if (z > maxZ) maxZ = z;
                    hasVertex = true;
                }
            }

            if (!hasVertex)
                return DefaultAabb();

            return new
            {
                min = new { x = minX, y = minY, z = minZ },
                max = new { x = maxX, y = maxY, z = maxZ }
            };
        }
        catch
        {
            return DefaultAabb();
        }
    }

    private static object DefaultAabb()
    {
        return new
        {
            min = new { x = -1f, y = 0f, z = -1f },
            max = new { x = 1f, y = 3f, z = 1f }
        };
    }

    private static object DefaultCamera()
    {
        return new
        {
            position = new { x = 0f, y = 2f, z = 4f },
            direction = new { x = 0f, y = 0.5f, z = 4f }
        };
    }

    public sealed class Avatar3DStatusResponse
    {
        [JsonPropertyName("Final")]
        public bool Final { get; set; }

        [JsonPropertyName("Url")]
        public string Url { get; set; } = string.Empty;
    }
}
