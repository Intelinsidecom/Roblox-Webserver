using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using Thumbnails;
using Assets;

namespace Website.Controllers;

[ApiController]
public class AssetThumbnail3DController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IThumbnailService _thumbnailService;

    public AssetThumbnail3DController(IConfiguration configuration, IThumbnailService thumbnailService)
    {
        _configuration = configuration;
        _thumbnailService = thumbnailService;
    }

    [HttpGet("asset-thumbnail-3d/json")]
    public async Task<IActionResult> Index([FromQuery] long assetId, [FromQuery] int? width, [FromQuery] int? height)
    {
        if (assetId <= 0)
            return BadRequest(new { error = "assetId is required" });

        var connStr = _configuration.GetConnectionString("Default");
        if (!string.IsNullOrWhiteSpace(connStr))
        {
            var repo = new AssetMetadataRepository();
            var asset = await repo.GetAssetByIdAsync(connStr, assetId);
            if (asset == null)
                return NotFound(new { error = "Asset not found" });
        }

        var w = width.GetValueOrDefault(420);
        var h = height.GetValueOrDefault(420);
        var sizeSegment = $"{w}x{h}";

        var metadataUrl = Url.Action(
                              action: nameof(GetMetadata),
                              controller: "AssetThumbnail3D",
                              values: new { assetId = assetId, size = sizeSegment }
                          )
                          ?? $"/asset-thumbnail-3d/metadata/asset/{assetId}/{sizeSegment}";

        return Ok(new Asset3DStatusResponse
        {
            Final = true,
            Url = metadataUrl
        });
    }

    [HttpGet("asset-thumbnail-3d/metadata/asset/{assetId:long}/{size}")]
    public async Task<IActionResult> GetMetadata(long assetId, string size)
    {
        if (assetId <= 0)
            return BadRequest(new { error = "assetId is required" });

        var connStr = _configuration.GetConnectionString("Default");
        int assetTypeId = 0;
        if (!string.IsNullOrWhiteSpace(connStr))
        {
            var repo = new AssetMetadataRepository();
            var asset = await repo.GetAssetByIdAsync(connStr, assetId);
            if (asset == null)
                return NotFound(new { error = "Asset not found" });
            assetTypeId = asset.AssetTypeId;
        }

        var w = 420;
        var h = 420;
        if (!string.IsNullOrWhiteSpace(size))
        {
            var parts = size.ToLowerInvariant().Split('x');
            if (parts.Length == 2)
            {
                if (int.TryParse(parts[0], out var pw) && pw > 0) w = pw;
                if (int.TryParse(parts[1], out var ph) && ph > 0) h = ph;
            }
        }

        // Check DB cache first
        if (!string.IsNullOrWhiteSpace(connStr))
        {
            try
            {
                var cacheRepo = new Asset3DThumbnailCacheRepository();
                var (found, entry) = await cacheRepo.TryGetAsync(connStr!, assetId, default);
                if (found && entry != null)
                {
                    var cdnBase = _configuration["Thumbnails:ThumbnailUrl"] ?? "https://cdn.freblx.xyz/";
                    var objRel = $"3DAsset/{entry.ModelHash}/{entry.ObjFileName}";
                    var mtlRel = $"3DAsset/{entry.ModelHash}/{entry.MtlFileName}";

                    var objUrl = CombineCdnUrl(cdnBase, objRel);
                    var mtlUrl = CombineCdnUrl(cdnBase, mtlRel);

                    var aabb = GetAabbFromObjFile(entry.ModelHash, entry.ObjFileName);
                    return Ok(new { obj = objUrl, mtl = mtlUrl, aabb = aabb, camera = GetCameraForHash(entry.ModelHash) });
                }
            }
            catch
            {
            }
        }

        // Cache miss — render via Arbiter (use Model.lua for model assets)
        Avatar3DCacheResult cached;
        if (assetTypeId == 10)
        {
            cached = await _thumbnailService.RenderModel3DAndCacheAsync(assetId, w, h, force: true);
        }
        else
        {
            cached = await _thumbnailService.RenderAsset3DAndCacheAsync(assetId, w, h, force: true);
        }
        var cdnBaseFinal = _configuration["Thumbnails:ThumbnailUrl"] ?? "https://cdn.freblx.xyz/";
        var objRelative = $"3DAsset/{cached.Hash}/{cached.ObjFileName}";
        var mtlRelative = $"3DAsset/{cached.Hash}/{cached.MtlFileName}";
        var objUrlFinal = CombineCdnUrl(cdnBaseFinal, objRelative);
        var mtlUrlFinal = CombineCdnUrl(cdnBaseFinal, mtlRelative);

        // Cache in DB
        if (!string.IsNullOrWhiteSpace(connStr))
        {
            try
            {
                var cacheRepo = new Asset3DThumbnailCacheRepository();
                await cacheRepo.UpsertAsync(connStr!, assetId, cached.Hash, cached.ObjFileName, cached.MtlFileName, w, h, default);
            }
            catch
            {
            }
        }

        var aabbFinal = !string.IsNullOrWhiteSpace(cached.DirectoryPath)
            ? ReadAabbFromObjFile(cached.DirectoryPath, cached.ObjFileName)
            : GetAabbFromObjFile(cached.Hash, cached.ObjFileName);
        return Ok(new { obj = objUrlFinal, mtl = mtlUrlFinal, aabb = aabbFinal, camera = ParseCameraJson(cached.CameraJson) ?? DefaultCamera() });
    }

    private object? GetAabbFromObjFile(string modelHash, string objFileName)
    {
        var baseDir = _configuration["Thumbnails:Asset3DDirectory"];
        if (string.IsNullOrWhiteSpace(baseDir))
        {
            var avatar3dDir = _configuration["Thumbnails:Avatar3DDirectory"];
            baseDir = !string.IsNullOrWhiteSpace(avatar3dDir)
                ? Path.Combine(avatar3dDir!, "..", "3DAsset")
                : null;
        }
        if (string.IsNullOrWhiteSpace(baseDir))
            return DefaultAabb();
        var dir = Path.Combine(baseDir, modelHash);
        return ReadAabbFromObjFile(dir, objFileName);
    }

    private object? ReadAabbFromObjFile(string directoryPath, string objFileName)
    {
        var objPath = Path.Combine(directoryPath, objFileName);
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

    private object DefaultAabb()
    {
        return new
        {
            min = new { x = -1f, y = 0f, z = -1f },
            max = new { x = 1f, y = 3f, z = 1f }
        };
    }

    private object DefaultCamera()
    {
        return new
        {
            position = new { x = 0f, y = 2f, z = 4f },
            direction = new { x = 0f, y = 0.5f, z = 4f }
        };
    }

    private object? GetCameraForHash(string modelHash)
    {
        var baseDir = _configuration["Thumbnails:Asset3DDirectory"];
        if (string.IsNullOrWhiteSpace(baseDir))
        {
            var avatar3dDir = _configuration["Thumbnails:Avatar3DDirectory"];
            if (!string.IsNullOrWhiteSpace(avatar3dDir))
                baseDir = Path.Combine(avatar3dDir!, "..", "3DAsset");
        }
        if (string.IsNullOrWhiteSpace(baseDir))
            return DefaultCamera();
        var cameraPath = Path.Combine(baseDir, modelHash, "camera.json");
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
            using var doc = JsonDocument.Parse(cameraJson);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
                return null;
            if (!root.TryGetProperty("position", out var posEl) || posEl.ValueKind != JsonValueKind.Object)
                return null;
            if (!root.TryGetProperty("direction", out var dirEl) || dirEl.ValueKind != JsonValueKind.Object)
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

    private static float? TryGetFloat(JsonElement el, string property)
    {
        if (!el.TryGetProperty(property, out var val))
            return null;
        if (val.ValueKind == JsonValueKind.Number && val.TryGetSingle(out var f))
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

    public sealed class Asset3DStatusResponse
    {
        [JsonPropertyName("Final")]
        public bool Final { get; set; }

        [JsonPropertyName("Url")]
        public string Url { get; set; } = string.Empty;
    }
}
