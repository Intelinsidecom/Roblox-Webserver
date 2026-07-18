using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;
using System.IO.Compression;
using Npgsql;
using Assets;

namespace Thumbnails;

public sealed class ThumbnailService : IThumbnailService
{
    private readonly IConfiguration? _configuration;
    private readonly PlaceThumbnailCacheRepository _cacheRepository = new();

    public const string PrimaryConfigKey = "Thumbnails:OutputDirectory";
    public const string LegacyConfigKey = "ThumbnailOutputDirectory";

    public ThumbnailService(IConfiguration? configuration = null)
    {
        _configuration = configuration;
    }

    public async Task<ThumbnailSaveResult> SaveBase64PngAsync(string base64, string? overrideOutputDirectory = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(base64))
            throw new ArgumentException("Base64 input is required", nameof(base64));

        // Remove potential data URI prefix
        var commaIdx = base64.IndexOf(',');
        if (commaIdx >= 0)
            base64 = base64.Substring(commaIdx + 1);

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(base64);
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("Invalid base64 string provided", nameof(base64), ex);
        }

        // Determine image format via magic bytes
        // PNG: 89 50 4E 47 0D 0A 1A 0A; JPEG: FF D8 ... FF D9
        string ext = IsPng(bytes) ? ".png" : (IsJpeg(bytes) ? ".jpg" : ".png");

        // Compute SHA256 hash of bytes
        string hash;
        using (var sha = SHA256.Create())
        {
            var digest = sha.ComputeHash(bytes);
            var sb = new StringBuilder(digest.Length * 2);
            foreach (var b in digest)
                sb.Append(b.ToString("x2"));
            hash = sb.ToString();
        }

        var outputDir = ResolveOutputDirectory(overrideOutputDirectory);
        Directory.CreateDirectory(outputDir);

        var fileName = hash + ext;
        var fullPath = Path.Combine(outputDir, fileName);

        if (File.Exists(fullPath))
        {
            return new ThumbnailSaveResult
            {
                Hash = hash,
                FileName = fileName,
                FullPath = fullPath,
                AlreadyExisted = true
            };
        }

        // Write file atomically using the bytes as provided (assumed PNG)
        var tempPath = fullPath + ".tmp";
        using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
        {
            await fs.WriteAsync(bytes, 0, bytes.Length, cancellationToken).ConfigureAwait(false);
        }
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
        File.Move(tempPath, fullPath);

        return new ThumbnailSaveResult
        {
            Hash = hash,
            FileName = fileName,
            FullPath = fullPath,
            AlreadyExisted = false
        };
    }

    public async Task<ThumbnailSaveResult> RenderAvatarAsync(string type, long userId, int? x = null, int? y = null, CancellationToken cancellationToken = default)
    {
        var arbiterUrl = _configuration?["Thumbnails:ArbiterUrl"] ?? "http://localhost:5000";

        var qb = new StringBuilder();
        qb.Append("type=").Append(Uri.EscapeDataString(type ?? "headshot"));
        qb.Append("&userId=").Append(Uri.EscapeDataString(userId.ToString()));
        if (x.HasValue) qb.Append("&x=").Append(x.Value);
        if (y.HasValue) qb.Append("&y=").Append(y.Value);
        // If a Website base URL is configured, pass it explicitly so Arbiter doesn't infer its own host
        var websiteBase = _configuration?["Thumbnails:WebsiteBaseUrl"];
        if (!string.IsNullOrWhiteSpace(websiteBase))
        {
            qb.Append("&baseUrl=").Append(Uri.EscapeDataString(websiteBase));
        }

        // Headshots go through the dedicated /renderheadshot endpoint, others use /renderavatar
        var route = string.Equals(type, "headshot", StringComparison.OrdinalIgnoreCase)
            ? "/renderheadshot?"
            : "/renderavatar?";

        var requestUri = arbiterUrl.TrimEnd('/') + route + qb.ToString();

        using var http = new HttpClient();
        using var req = new HttpRequestMessage(HttpMethod.Get, requestUri);
        using var resp = await http.SendAsync(req, cancellationToken).ConfigureAwait(false);

        var json = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
        if (!resp.IsSuccessStatusCode)
        {
            var statusCode = (int)resp.StatusCode;
            var reason = resp.ReasonPhrase ?? string.Empty;
            throw new HttpRequestException($"Arbiter {route.TrimEnd('?')} returned {statusCode} {reason}. Body: {Trunc(json)}");
        }

        using var doc = JsonDocument.Parse(json);

        // Extract base64 PNG from Arbiter response. Expected shapes:
        // - Array of { type: "string", value: "<base64>" }
        // - Object with { value: "<base64>" }
        // - Raw string "<base64>"
        string? base64 = null;

        if (doc.RootElement.ValueKind == JsonValueKind.Array)
        {
            var len = doc.RootElement.GetArrayLength();
            if (len == 0)
                throw new InvalidOperationException("Unexpected response from Arbiter. Raw: " + Trunc(json));

            // Walk from end to start to get the last value
            for (int i = len - 1; i >= 0; i--)
            {
                var el = doc.RootElement[i];
                if (el.ValueKind == JsonValueKind.Object && el.TryGetProperty("value", out var vEl) && vEl.ValueKind == JsonValueKind.String)
                {
                    base64 = vEl.GetString();
                    if (!string.IsNullOrWhiteSpace(base64)) break;
                }
                else if (el.ValueKind == JsonValueKind.String)
                {
                    base64 = el.GetString();
                    if (!string.IsNullOrWhiteSpace(base64)) break;
                }
            }
        }
        else if (doc.RootElement.ValueKind == JsonValueKind.Object)
        {
            if (doc.RootElement.TryGetProperty("value", out var vEl) && vEl.ValueKind == JsonValueKind.String)
            {
                base64 = vEl.GetString();
            }
        }
        else if (doc.RootElement.ValueKind == JsonValueKind.String)
        {
            base64 = doc.RootElement.GetString();
        }

        if (string.IsNullOrWhiteSpace(base64))
            throw new InvalidOperationException("Could not extract base64 PNG from Arbiter response. Raw: " + Trunc(json));

        var save = await SaveBase64PngAsync(base64!, null, cancellationToken).ConfigureAwait(false);
        return save;
    }

    public async Task<Avatar3DCacheResult> RenderAvatar3DAndCacheAsync(long userId, int? x = null, int? y = null, bool force = false, CancellationToken cancellationToken = default)
    {
        // Determine 3D avatar output root directory
        var baseDir = _configuration?["Thumbnails:Avatar3DDirectory"];
        if (string.IsNullOrWhiteSpace(baseDir))
        {
            baseDir = @"C:\\Users\\Intel\\Documents\\GitHub\\RobloxWebserver\\CDN\\Assets\\3DAvatar";
        }

        var w = x.GetValueOrDefault(0);
        var h = y.GetValueOrDefault(0);
        var cacheKey = $"{userId}_{w}x{h}";
        var mapsDir = Path.Combine(baseDir!, "maps");
        Directory.CreateDirectory(mapsDir);
        var mapPath = Path.Combine(mapsDir, cacheKey + ".txt");

        if (!force && File.Exists(mapPath))
        {
            var existingHash = File.ReadAllText(mapPath).Trim();
            if (!string.IsNullOrWhiteSpace(existingHash))
            {
                var existingDir = Path.Combine(baseDir!, existingHash);
                if (Directory.Exists(existingDir))
                {
                    var objFiles = Directory.GetFiles(existingDir, "*.obj");
                    var mtlFiles = Directory.GetFiles(existingDir, "*.mtl");
                    if (objFiles.Length > 0 && mtlFiles.Length > 0)
                    {
                        var cameraFile = Path.Combine(existingDir, "camera.json");
                        var cameraJson = File.Exists(cameraFile) ? File.ReadAllText(cameraFile).Trim() : null;
                        return new Avatar3DCacheResult
                        {
                            Hash = existingHash,
                            DirectoryPath = existingDir,
                            ObjFileName = Path.GetFileName(objFiles[0]),
                            MtlFileName = Path.GetFileName(mtlFiles[0]),
                            AlreadyExisted = true,
                            CameraJson = cameraJson
                        };
                    }
                }
            }
        }

        var base64 = await RenderAvatar3DBase64Async(userId, x, y, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(base64))
            throw new InvalidOperationException("Base64 payload for 3D avatar was empty.");
        byte[] bytes;
        string? rccCameraJson = null;
        if (base64.TrimStart().StartsWith("{"))
        {
            var jsonStr = base64;
            bytes = Encoding.UTF8.GetBytes(jsonStr);
            var hash = ComputeSha256(bytes);
            var dir = Path.Combine(baseDir!, hash);
            Directory.CreateDirectory(dir);

            try
            {
                using var previewDoc = JsonDocument.Parse(jsonStr);
                if (previewDoc.RootElement.TryGetProperty("camera", out var cameraEl) && cameraEl.ValueKind == JsonValueKind.Object)
                {
                    rccCameraJson = cameraEl.GetRawText();
                }
            }
            catch (Exception ex) { Console.WriteLine($"[ERROR] Parse RCC camera JSON: {ex}"); }

            var existingObjFiles = Directory.GetFiles(dir, "*.obj");
            var existingMtlFiles = Directory.GetFiles(dir, "*.mtl");
            if (existingObjFiles.Length > 0 && existingMtlFiles.Length > 0)
            {
                File.WriteAllText(mapPath, hash);
                return new Avatar3DCacheResult
                {
                    Hash = hash,
                    DirectoryPath = dir,
                    ObjFileName = Path.GetFileName(existingObjFiles[0]),
                    MtlFileName = Path.GetFileName(existingMtlFiles[0]),
                    AlreadyExisted = true,
                    CameraJson = rccCameraJson
                };
            }

            try
            {
                using var jDoc = JsonDocument.Parse(jsonStr);
                if (jDoc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    string objFileName = "avatar.obj";
                    string mtlFileName = "avatar.mtl";
                    bool extracted = false;

                    if (jDoc.RootElement.TryGetProperty("camera", out var cameraEl) && cameraEl.ValueKind == JsonValueKind.Object)
                    {
                        rccCameraJson = cameraEl.GetRawText();
                    }

                    JsonElement filesEl;
                    bool hasFilesWrapper = jDoc.RootElement.TryGetProperty("files", out filesEl) && filesEl.ValueKind == JsonValueKind.Object;

                    var props = hasFilesWrapper
                        ? filesEl.EnumerateObject()
                        : jDoc.RootElement.EnumerateObject();

                    foreach (var prop in props)
                    {
                        if (prop.Value.ValueKind != JsonValueKind.Object)
                            continue;
                        if (!prop.Value.TryGetProperty("content", out var contentEl) || contentEl.ValueKind != JsonValueKind.String)
                            continue;
                        if (!hasFilesWrapper && (string.Equals(prop.Name, "camera", StringComparison.OrdinalIgnoreCase) || string.Equals(prop.Name, "AABB", StringComparison.OrdinalIgnoreCase)))
                            continue;

                        var fileName = prop.Name;
                        var fileBase64 = contentEl.GetString()!;
                        byte[] fileBytes;
                        try { fileBytes = Convert.FromBase64String(fileBase64); }
                        catch { continue; }

                        var targetPath = Path.Combine(dir, fileName);
                        Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
                        File.WriteAllBytes(targetPath, fileBytes);
                        extracted = true;

                        var ext = Path.GetExtension(fileName).ToLowerInvariant();
                        if (ext == ".obj") objFileName = fileName;
                        else if (ext == ".mtl") mtlFileName = fileName;
                    }

                    if (extracted)
                    {
                        var mtlPath = Path.Combine(dir, mtlFileName);
                        if (File.Exists(mtlPath))
                        {
                            var cdnBase = _configuration?["Thumbnails:ThumbnailUrl"] ?? "https://cdn.freblx.xyz/";
                            cdnBase = cdnBase.TrimEnd('/') + "/";
                            var cdnPrefix = cdnBase + "3DAvatar/" + hash + "/";

                            var mtlContent = File.ReadAllText(mtlPath);
                            var lines = mtlContent.Split('\n');
                            var rewritten = lines.Select(l => {
                                var trimmed = l.TrimStart();
                                if (trimmed.StartsWith("map_Kd", StringComparison.OrdinalIgnoreCase) || trimmed.StartsWith("map_kd", StringComparison.OrdinalIgnoreCase))
                                {
                                    var parts = l.Split(new char[] { ' ', '\t' }, 2, StringSplitOptions.RemoveEmptyEntries);
                                    if (parts.Length == 2 && !parts[1].Contains("://"))
                                        return parts[0] + " " + cdnPrefix + parts[1].Trim();
                                }
                                return l;
                            });
                            File.WriteAllText(mtlPath, string.Join("\n", rewritten));
                        }
                        else
                        {
                            File.WriteAllText(mtlPath, "newmtl default\nKd 1.000000 1.000000 1.000000\n");
                        }

                        if (!string.IsNullOrWhiteSpace(rccCameraJson))
                        {
                            File.WriteAllText(Path.Combine(dir, "camera.json"), rccCameraJson);
                        }
                        File.WriteAllText(mapPath, hash);
                        return new Avatar3DCacheResult
                        {
                            Hash = hash,
                            DirectoryPath = dir,
                            ObjFileName = objFileName,
                            MtlFileName = mtlFileName,
                            AlreadyExisted = false,
                            CameraJson = rccCameraJson
                        };
                    }
                }
            }
            catch
            {
            }
        }

        var commaIdx = -1;
        var base64Trimmed = base64.TrimStart();
        if (base64Trimmed.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            commaIdx = base64.IndexOf(',');
            if (commaIdx >= 0)
                base64 = base64.Substring(commaIdx + 1);
        }

        try
        {
            bytes = Convert.FromBase64String(base64);
        }
        catch (FormatException)
        {
            bytes = Encoding.UTF8.GetBytes(base64);
        }

        var hash2 = ComputeSha256(bytes);

        var dir2 = Path.Combine(baseDir!, hash2);
        Directory.CreateDirectory(dir2);

        var existingObjFiles2 = Directory.GetFiles(dir2, "*.obj");
        var existingMtlFiles2 = Directory.GetFiles(dir2, "*.mtl");
        if (existingObjFiles2.Length > 0 && existingMtlFiles2.Length > 0)
        {
            File.WriteAllText(mapPath, hash2);
            return new Avatar3DCacheResult
            {
                Hash = hash2,
                DirectoryPath = dir2,
                ObjFileName = Path.GetFileName(existingObjFiles2[0]),
                MtlFileName = Path.GetFileName(existingMtlFiles2[0]),
                AlreadyExisted = true
            };
        }

        string objFileName2 = "avatar.obj";
        string mtlFileName2 = "avatar.mtl";

        if (IsZip(bytes))
        {
            using var ms = new MemoryStream(bytes);
            using var zip = new ZipArchive(ms, ZipArchiveMode.Read);
            foreach (var entry in zip.Entries)
            {
                if (string.IsNullOrEmpty(entry.Name))
                    continue;

                var targetPath = Path.Combine(dir2, entry.Name);
                using var zs = entry.Open();
                using var fs = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None);
                zs.CopyTo(fs);

                var ext = Path.GetExtension(entry.Name).ToLowerInvariant();
                if (ext == ".obj")
                    objFileName2 = entry.Name;
                else if (ext == ".mtl")
                    mtlFileName2 = entry.Name;
            }

            var mtlPath = Path.Combine(dir2, mtlFileName2);
            if (!File.Exists(mtlPath))
                File.WriteAllText(mtlPath, "newmtl default\nKd 1.000000 1.000000 1.000000\n");
        }
        else
        {
            var objPath = Path.Combine(dir2, objFileName2);
            File.WriteAllBytes(objPath, bytes);

            var mtlPath = Path.Combine(dir2, mtlFileName2);
            if (!File.Exists(mtlPath))
                File.WriteAllText(mtlPath, "newmtl default\nKd 1.000000 1.000000 1.000000\n");
        }

        return new Avatar3DCacheResult
        {
            Hash = hash2,
            DirectoryPath = dir2,
            ObjFileName = objFileName2,
            MtlFileName = mtlFileName2,
            AlreadyExisted = false
        };
    }

    public async Task<string> RenderAvatar3DBase64Async(long userId, int? x = null, int? y = null, CancellationToken cancellationToken = default)
    {
        var arbiterUrl = _configuration?["Thumbnails:ArbiterUrl"] ?? "http://localhost:5000";

        var qb = new StringBuilder();
        qb.Append("type=").Append(Uri.EscapeDataString("avatar"));
        qb.Append("&userId=").Append(Uri.EscapeDataString(userId.ToString()));
        if (x.HasValue) qb.Append("&x=").Append(x.Value);
        if (y.HasValue) qb.Append("&y=").Append(y.Value);
        var websiteBase = _configuration?["Thumbnails:WebsiteBaseUrl"];
        if (!string.IsNullOrWhiteSpace(websiteBase))
        {
            qb.Append("&baseUrl=").Append(Uri.EscapeDataString(websiteBase));
        }

        var requestUri = arbiterUrl.TrimEnd('/') + "/renderavatar3d?" + qb.ToString();

        using var http = new HttpClient();
        using var req = new HttpRequestMessage(HttpMethod.Get, requestUri);
        using var resp = await http.SendAsync(req, cancellationToken).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);

        using var doc = JsonDocument.Parse(json);

        string? base64 = null;

        if (doc.RootElement.ValueKind == JsonValueKind.Array)
        {
            var len = doc.RootElement.GetArrayLength();
            if (len == 0)
                throw new InvalidOperationException("Unexpected response from Arbiter. Raw: " + Trunc(json));

            for (int i = len - 1; i >= 0; i--)
            {
                var el = doc.RootElement[i];
                if (el.ValueKind == JsonValueKind.Object && el.TryGetProperty("value", out var vEl) && vEl.ValueKind == JsonValueKind.String)
                {
                    base64 = vEl.GetString();
                    if (!string.IsNullOrWhiteSpace(base64)) break;
                }
                else if (el.ValueKind == JsonValueKind.String)
                {
                    base64 = el.GetString();
                    if (!string.IsNullOrWhiteSpace(base64)) break;
                }
            }
        }
        else if (doc.RootElement.ValueKind == JsonValueKind.Object)
        {
            if (doc.RootElement.TryGetProperty("value", out var vEl) && vEl.ValueKind == JsonValueKind.String)
            {
                base64 = vEl.GetString();
            }
        }
        else if (doc.RootElement.ValueKind == JsonValueKind.String)
        {
            base64 = doc.RootElement.GetString();
        }

        if (string.IsNullOrWhiteSpace(base64))
            throw new InvalidOperationException("Could not extract base64 PNG from Arbiter response. Raw: " + Trunc(json));

        return base64!;
    }

    public async Task<ThumbnailSaveResult> RenderPlaceAsync(long placeId, int? x = null, int? y = null, CancellationToken cancellationToken = default)
    {
        return await RenderPlaceAsync(placeId, x, y, null, null, cancellationToken);
    }

    public async Task<ThumbnailSaveResult> RenderPlaceAsync(long placeId, int? x, int? y, string? connectionString, CancellationToken cancellationToken = default)
    {
        return await RenderPlaceAsync(placeId, x, y, connectionString, null, cancellationToken);
    }

    public async Task<ThumbnailSaveResult> RenderPlaceAsync(long placeId, int? x, int? y, string? connectionString, string? placeAssetHash, CancellationToken cancellationToken = default)
    {
        // If connection string and place asset hash are provided, try to check cache first
        if (!string.IsNullOrWhiteSpace(connectionString) && !string.IsNullOrWhiteSpace(placeAssetHash))
        {
            try
            {
                // Check cache for existing thumbnails with specific dimensions
                var (found, cachedIconHash, cachedThumbnailHash) = await _cacheRepository.TryGetAsync(
                    connectionString, placeAssetHash, x, y, cancellationToken);

                if (found && !string.IsNullOrWhiteSpace(cachedIconHash))
                {
                    // Found cached entry - return cached result
                    var outputDir = ResolveOutputDirectory(null);
                    var fileName = cachedIconHash + ".png";
                    var fullPath = Path.Combine(outputDir, fileName);
                    
                    // Check if the cached file actually exists on disk
                    if (File.Exists(fullPath))
                    {
                        return new ThumbnailSaveResult
                        {
                            Hash = cachedIconHash,
                            FileName = fileName,
                            FullPath = fullPath,
                            AlreadyExisted = true
                        };
                    }
                }
            }
            catch
            {
                // Continue with normal rendering if cache check fails
            }
        }

        // Cache miss or no connection string provided - render new thumbnail
        var arbiterUrl = _configuration?["Thumbnails:ArbiterUrl"] ?? "http://localhost:5000";

        var qb = new StringBuilder();
        qb.Append("placeId=").Append(Uri.EscapeDataString(placeId.ToString()));
        if (x.HasValue) qb.Append("&x=").Append(x.Value);
        if (y.HasValue) qb.Append("&y=").Append(y.Value);
        // If a Website base URL is configured, pass it explicitly so Arbiter doesn't infer its own host
        var websiteBase = _configuration?["Thumbnails:WebsiteBaseUrl"];
        if (!string.IsNullOrWhiteSpace(websiteBase))
        {
            qb.Append("&baseUrl=").Append(Uri.EscapeDataString(websiteBase));
        }

        var requestUri = arbiterUrl.TrimEnd('/') + "/rendergame?" + qb.ToString();

        using var http = new HttpClient();
        using var req = new HttpRequestMessage(HttpMethod.Get, requestUri);
        using var resp = await http.SendAsync(req, cancellationToken).ConfigureAwait(false);

        var json = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
        if (!resp.IsSuccessStatusCode)
        {
            var statusCode = (int)resp.StatusCode;
            var reason = resp.ReasonPhrase ?? string.Empty;
            throw new HttpRequestException($"Arbiter /rendergame returned {statusCode} {reason}. Body: {Trunc(json)}");
        }

        using var doc = JsonDocument.Parse(json);

        // Extract base64 PNG from Arbiter response. Expected shapes:
        // - Array of { type: "string", value: "<base64>" }
        // - Object with { value: "<base64>" }
        // - Raw string "<base64>"
        string? base64 = null;

        if (doc.RootElement.ValueKind == JsonValueKind.Array)
        {
            var len = doc.RootElement.GetArrayLength();
            if (len == 0)
                throw new InvalidOperationException("Unexpected response from Arbiter. Raw: " + Trunc(json));

            // Walk from end to start to get the last value
            for (int i = len - 1; i >= 0; i--)
            {
                var el = doc.RootElement[i];
                if (el.ValueKind == JsonValueKind.Object && el.TryGetProperty("value", out var vEl) && vEl.ValueKind == JsonValueKind.String)
                {
                    base64 = vEl.GetString();
                    if (!string.IsNullOrWhiteSpace(base64)) break;
                }
                else if (el.ValueKind == JsonValueKind.String)
                {
                    base64 = el.GetString();
                    if (!string.IsNullOrWhiteSpace(base64)) break;
                }
            }
        }
        else if (doc.RootElement.ValueKind == JsonValueKind.Object)
        {
            if (doc.RootElement.TryGetProperty("value", out var vEl) && vEl.ValueKind == JsonValueKind.String)
            {
                base64 = vEl.GetString();
            }
        }
        else if (doc.RootElement.ValueKind == JsonValueKind.String)
        {
            base64 = doc.RootElement.GetString();
        }

        if (string.IsNullOrWhiteSpace(base64))
            throw new InvalidOperationException("Could not extract base64 PNG from Arbiter response. Raw: " + Trunc(json));

        var saveResult = await SaveBase64PngAsync(base64!, null, cancellationToken).ConfigureAwait(false);

        // If we have a connection string and place asset hash, cache the result
        if (!string.IsNullOrWhiteSpace(connectionString) && !string.IsNullOrWhiteSpace(placeAssetHash) && !saveResult.AlreadyExisted)
        {
            try
            {
                // Cache the generated thumbnail with dimensions
                await _cacheRepository.UpsertAsync(
                    connectionString, 
                    placeAssetHash, 
                    saveResult.Hash, 
                    saveResult.Hash, // Using same hash for both icon and thumbnail for now
                    x, 
                    y, 
                    cancellationToken);
            }
            catch
            {
                // Don't fail the whole operation if caching fails
            }
        }

        return saveResult;
    }

    public async Task<string> RenderAsset3DBase64Async(long assetId, int? x = null, int? y = null, CancellationToken cancellationToken = default)
    {
        var arbiterUrl = _configuration?["Thumbnails:ArbiterUrl"] ?? "http://localhost:5000";

        var qb = new StringBuilder();
        qb.Append("assetId=").Append(Uri.EscapeDataString(assetId.ToString()));
        if (x.HasValue) qb.Append("&x=").Append(x.Value);
        if (y.HasValue) qb.Append("&y=").Append(y.Value);
        var websiteBase = _configuration?["Thumbnails:WebsiteBaseUrl"];
        if (!string.IsNullOrWhiteSpace(websiteBase))
        {
            qb.Append("&baseUrl=").Append(Uri.EscapeDataString(websiteBase));
        }

        var requestUri = arbiterUrl.TrimEnd('/') + "/renderasset3d?" + qb.ToString();

        using var http = new HttpClient();
        using var req = new HttpRequestMessage(HttpMethod.Get, requestUri);
        using var resp = await http.SendAsync(req, cancellationToken).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);

        using var doc = JsonDocument.Parse(json);

        string? base64 = null;

        if (doc.RootElement.ValueKind == JsonValueKind.Array)
        {
            var len = doc.RootElement.GetArrayLength();
            if (len == 0)
                throw new InvalidOperationException("Unexpected response from Arbiter. Raw: " + Trunc(json));

            for (int i = len - 1; i >= 0; i--)
            {
                var el = doc.RootElement[i];
                if (el.ValueKind == JsonValueKind.Object && el.TryGetProperty("value", out var vEl) && vEl.ValueKind == JsonValueKind.String)
                {
                    base64 = vEl.GetString();
                    if (!string.IsNullOrWhiteSpace(base64)) break;
                }
                else if (el.ValueKind == JsonValueKind.String)
                {
                    base64 = el.GetString();
                    if (!string.IsNullOrWhiteSpace(base64)) break;
                }
            }
        }
        else if (doc.RootElement.ValueKind == JsonValueKind.Object)
        {
            if (doc.RootElement.TryGetProperty("value", out var vEl) && vEl.ValueKind == JsonValueKind.String)
            {
                base64 = vEl.GetString();
            }
        }
        else if (doc.RootElement.ValueKind == JsonValueKind.String)
        {
            base64 = doc.RootElement.GetString();
        }

        if (string.IsNullOrWhiteSpace(base64))
            throw new InvalidOperationException("Could not extract base64 from Arbiter response. Raw: " + Trunc(json));

        return base64!;
    }

    public async Task<Avatar3DCacheResult> RenderAsset3DAndCacheAsync(long assetId, int? x = null, int? y = null, bool force = false, CancellationToken cancellationToken = default)
    {
        // Determine 3D asset output root directory (shared with avatar3d but under 3DAsset subfolder)
        var baseDir = _configuration?["Thumbnails:Asset3DDirectory"];
        if (string.IsNullOrWhiteSpace(baseDir))
        {
            var avatar3dDir = _configuration?["Thumbnails:Avatar3DDirectory"];
            if (!string.IsNullOrWhiteSpace(avatar3dDir))
            {
                baseDir = Path.Combine(avatar3dDir!, "..", "3DAsset");
            }
            else
            {
                baseDir = @"C:\Users\Intel\Documents\GitHub\RobloxWebserver\CDN\Assets\3DAsset";
            }
        }

        var w = x.GetValueOrDefault(420);
        var h = y.GetValueOrDefault(420);
        var mapPath = Path.Combine(baseDir!, assetId.ToString() + ".txt");

        if (!force && File.Exists(mapPath))
        {
            var existingHash = File.ReadAllText(mapPath).Trim();
            if (!string.IsNullOrWhiteSpace(existingHash))
            {
                var existingDir = Path.Combine(baseDir!, existingHash);
                if (Directory.Exists(existingDir))
                {
                    var objFiles = Directory.GetFiles(existingDir, "*.obj");
                    var mtlFiles = Directory.GetFiles(existingDir, "*.mtl");
                    if (objFiles.Length > 0 && mtlFiles.Length > 0)
                    {
                        var cameraFile = Path.Combine(existingDir, "camera.json");
                        var cameraJson = File.Exists(cameraFile) ? File.ReadAllText(cameraFile).Trim() : null;
                        return new Avatar3DCacheResult
                        {
                            Hash = existingHash,
                            DirectoryPath = existingDir,
                            ObjFileName = Path.GetFileName(objFiles[0]),
                            MtlFileName = Path.GetFileName(mtlFiles[0]),
                            AlreadyExisted = true,
                            CameraJson = cameraJson
                        };
                    }
                }
            }
        }

        var base64 = await RenderAsset3DBase64Async(assetId, x, y, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(base64))
            throw new InvalidOperationException("Base64 payload for 3D asset was empty.");

        byte[] bytes;
        string? rccCameraJson = null;
        if (base64.TrimStart().StartsWith("{"))
        {
            var jsonStr = base64;
            bytes = Encoding.UTF8.GetBytes(jsonStr);
            var hash = ComputeSha256(bytes);
            var dir = Path.Combine(baseDir!, hash);
            Directory.CreateDirectory(dir);

            try
            {
                using var previewDoc = JsonDocument.Parse(jsonStr);
                if (previewDoc.RootElement.TryGetProperty("camera", out var cameraEl) && cameraEl.ValueKind == JsonValueKind.Object)
                {
                    rccCameraJson = cameraEl.GetRawText();
                }
            }
            catch (Exception ex) { Console.WriteLine($"[ERROR] Parse RCC camera JSON: {ex}"); }

            var existingObjFiles = Directory.GetFiles(dir, "*.obj");
            var existingMtlFiles = Directory.GetFiles(dir, "*.mtl");
            if (existingObjFiles.Length > 0 && existingMtlFiles.Length > 0)
            {
                File.WriteAllText(mapPath, hash);
                return new Avatar3DCacheResult
                {
                    Hash = hash,
                    DirectoryPath = dir,
                    ObjFileName = Path.GetFileName(existingObjFiles[0]),
                    MtlFileName = Path.GetFileName(existingMtlFiles[0]),
                    AlreadyExisted = true,
                    CameraJson = rccCameraJson
                };
            }

            try
            {
                using var jDoc = JsonDocument.Parse(jsonStr);
                if (jDoc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    string objFileName = "asset.obj";
                    string mtlFileName = "asset.mtl";
                    bool extracted = false;

                    if (jDoc.RootElement.TryGetProperty("camera", out var cameraEl) && cameraEl.ValueKind == JsonValueKind.Object)
                    {
                        rccCameraJson = cameraEl.GetRawText();
                    }

                    JsonElement filesEl;
                    bool hasFilesWrapper = jDoc.RootElement.TryGetProperty("files", out filesEl) && filesEl.ValueKind == JsonValueKind.Object;

                    var props = hasFilesWrapper
                        ? filesEl.EnumerateObject()
                        : jDoc.RootElement.EnumerateObject();

                    foreach (var prop in props)
                    {
                        if (prop.Value.ValueKind != JsonValueKind.Object)
                            continue;
                        if (!prop.Value.TryGetProperty("content", out var contentEl) || contentEl.ValueKind != JsonValueKind.String)
                            continue;
                        if (!hasFilesWrapper && (string.Equals(prop.Name, "camera", StringComparison.OrdinalIgnoreCase) || string.Equals(prop.Name, "AABB", StringComparison.OrdinalIgnoreCase)))
                            continue;

                        var fileName = prop.Name;
                        var fileBase64 = contentEl.GetString()!;
                        byte[] fileBytes;
                        try { fileBytes = Convert.FromBase64String(fileBase64); }
                        catch { continue; }

                        var targetPath = Path.Combine(dir, fileName);
                        Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
                        File.WriteAllBytes(targetPath, fileBytes);
                        extracted = true;

                        var ext = Path.GetExtension(fileName).ToLowerInvariant();
                        if (ext == ".obj") objFileName = fileName;
                        else if (ext == ".mtl") mtlFileName = fileName;
                    }

                    if (extracted)
                    {
                        var mtlPath = Path.Combine(dir, mtlFileName);
                        if (File.Exists(mtlPath))
                        {
                            var cdnBase = _configuration?["Thumbnails:ThumbnailUrl"] ?? "https://cdn.freblx.xyz/";
                            cdnBase = cdnBase.TrimEnd('/') + "/";
                            var cdnPrefix = cdnBase + "3DAsset/" + hash + "/";

                            var mtlContent = File.ReadAllText(mtlPath);
                            var lines = mtlContent.Split('\n');
                            var rewritten = lines.Select(l => {
                                var trimmed = l.TrimStart();
                                if (trimmed.StartsWith("map_Kd", StringComparison.OrdinalIgnoreCase) || trimmed.StartsWith("map_kd", StringComparison.OrdinalIgnoreCase))
                                {
                                    var parts = l.Split(new char[] { ' ', '\t' }, 2, StringSplitOptions.RemoveEmptyEntries);
                                    if (parts.Length == 2 && !parts[1].Contains("://"))
                                        return parts[0] + " " + cdnPrefix + parts[1].Trim();
                                }
                                return l;
                            });
                            File.WriteAllText(mtlPath, string.Join("\n", rewritten));
                        }
                        else
                        {
                            File.WriteAllText(mtlPath, "newmtl default\nKd 1.000000 1.000000 1.000000\n");
                        }

                        if (!string.IsNullOrWhiteSpace(rccCameraJson))
                        {
                            File.WriteAllText(Path.Combine(dir, "camera.json"), rccCameraJson);
                        }
                        File.WriteAllText(mapPath, hash);
                        return new Avatar3DCacheResult
                        {
                            Hash = hash,
                            DirectoryPath = dir,
                            ObjFileName = objFileName,
                            MtlFileName = mtlFileName,
                            AlreadyExisted = false,
                            CameraJson = rccCameraJson
                        };
                    }
                }
            }
            catch
            {
            }
        }

        var commaIdx = -1;
        var base64Trimmed = base64.TrimStart();
        if (base64Trimmed.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            commaIdx = base64.IndexOf(',');
            if (commaIdx >= 0)
                base64 = base64.Substring(commaIdx + 1);
        }

        try
        {
            bytes = Convert.FromBase64String(base64);
        }
        catch (FormatException)
        {
            bytes = Encoding.UTF8.GetBytes(base64);
        }

        var hash2 = ComputeSha256(bytes);

        var dir2 = Path.Combine(baseDir!, hash2);
        Directory.CreateDirectory(dir2);

        var existingObjFiles2 = Directory.GetFiles(dir2, "*.obj");
        var existingMtlFiles2 = Directory.GetFiles(dir2, "*.mtl");
        if (existingObjFiles2.Length > 0 && existingMtlFiles2.Length > 0)
        {
            File.WriteAllText(mapPath, hash2);
            return new Avatar3DCacheResult
            {
                Hash = hash2,
                DirectoryPath = dir2,
                ObjFileName = Path.GetFileName(existingObjFiles2[0]),
                MtlFileName = Path.GetFileName(existingMtlFiles2[0]),
                AlreadyExisted = true
            };
        }

        string objFileName2 = "asset.obj";
        string mtlFileName2 = "asset.mtl";

        if (IsZip(bytes))
        {
            using var ms = new MemoryStream(bytes);
            using var zip = new ZipArchive(ms, ZipArchiveMode.Read);
            foreach (var entry in zip.Entries)
            {
                if (string.IsNullOrEmpty(entry.Name))
                    continue;

                var targetPath = Path.Combine(dir2, entry.Name);
                using var zs = entry.Open();
                using var fs = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None);
                zs.CopyTo(fs);

                var ext = Path.GetExtension(entry.Name).ToLowerInvariant();
                if (ext == ".obj")
                    objFileName2 = entry.Name;
                else if (ext == ".mtl")
                    mtlFileName2 = entry.Name;
            }

            var mtlPath = Path.Combine(dir2, mtlFileName2);
            if (!File.Exists(mtlPath))
                File.WriteAllText(mtlPath, "newmtl default\nKd 1.000000 1.000000 1.000000\n");
        }
        else
        {
            var objPath = Path.Combine(dir2, objFileName2);
            File.WriteAllBytes(objPath, bytes);

            var mtlPath = Path.Combine(dir2, mtlFileName2);
            if (!File.Exists(mtlPath))
                File.WriteAllText(mtlPath, "newmtl default\nKd 1.000000 1.000000 1.000000\n");
        }

        File.WriteAllText(mapPath, hash2);
        return new Avatar3DCacheResult
        {
            Hash = hash2,
            DirectoryPath = dir2,
            ObjFileName = objFileName2,
            MtlFileName = mtlFileName2,
            AlreadyExisted = false
        };
    }

    public async Task<string> RenderModel3DBase64Async(long assetId, int? x = null, int? y = null, CancellationToken cancellationToken = default)
    {
        var arbiterUrl = _configuration?["Thumbnails:ArbiterUrl"] ?? "http://localhost:5000";

        var qb = new StringBuilder();
        qb.Append("assetId=").Append(Uri.EscapeDataString(assetId.ToString()));
        if (x.HasValue) qb.Append("&x=").Append(x.Value);
        if (y.HasValue) qb.Append("&y=").Append(y.Value);
        var websiteBase = _configuration?["Thumbnails:WebsiteBaseUrl"];
        if (!string.IsNullOrWhiteSpace(websiteBase))
        {
            qb.Append("&baseUrl=").Append(Uri.EscapeDataString(websiteBase));
        }

        var requestUri = arbiterUrl.TrimEnd('/') + "/rendermodel3d?" + qb.ToString();

        using var http = new HttpClient();
        using var req = new HttpRequestMessage(HttpMethod.Get, requestUri);
        using var resp = await http.SendAsync(req, cancellationToken).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);

        using var doc = JsonDocument.Parse(json);

        string? base64 = null;

        if (doc.RootElement.ValueKind == JsonValueKind.Array)
        {
            var len = doc.RootElement.GetArrayLength();
            if (len == 0)
                throw new InvalidOperationException("Unexpected response from Arbiter. Raw: " + Trunc(json));

            for (int i = len - 1; i >= 0; i--)
            {
                var el = doc.RootElement[i];
                if (el.ValueKind == JsonValueKind.Object && el.TryGetProperty("value", out var vEl) && vEl.ValueKind == JsonValueKind.String)
                {
                    base64 = vEl.GetString();
                    if (!string.IsNullOrWhiteSpace(base64)) break;
                }
                else if (el.ValueKind == JsonValueKind.String)
                {
                    base64 = el.GetString();
                    if (!string.IsNullOrWhiteSpace(base64)) break;
                }
            }
        }
        else if (doc.RootElement.ValueKind == JsonValueKind.Object)
        {
            if (doc.RootElement.TryGetProperty("value", out var vEl) && vEl.ValueKind == JsonValueKind.String)
            {
                base64 = vEl.GetString();
            }
        }
        else if (doc.RootElement.ValueKind == JsonValueKind.String)
        {
            base64 = doc.RootElement.GetString();
        }

        if (string.IsNullOrWhiteSpace(base64))
            throw new InvalidOperationException("Could not extract base64 from Arbiter response. Raw: " + Trunc(json));

        return base64!;
    }

    public async Task<Avatar3DCacheResult> RenderModel3DAndCacheAsync(long assetId, int? x = null, int? y = null, bool force = false, CancellationToken cancellationToken = default)
    {
        var baseDir = _configuration?["Thumbnails:Asset3DDirectory"];
        if (string.IsNullOrWhiteSpace(baseDir))
        {
            var avatar3dDir = _configuration?["Thumbnails:Avatar3DDirectory"];
            if (!string.IsNullOrWhiteSpace(avatar3dDir))
            {
                baseDir = Path.Combine(avatar3dDir!, "..", "3DAsset");
            }
            else
            {
                baseDir = @"C:\Users\Intel\Documents\GitHub\RobloxWebserver\CDN\Assets\3DAsset";
            }
        }

        var w = x.GetValueOrDefault(420);
        var h = y.GetValueOrDefault(420);
        var mapPath = Path.Combine(baseDir!, assetId.ToString() + ".txt");

        if (!force && File.Exists(mapPath))
        {
            var existingHash = File.ReadAllText(mapPath).Trim();
            if (!string.IsNullOrWhiteSpace(existingHash))
            {
                var existingDir = Path.Combine(baseDir!, existingHash);
                if (Directory.Exists(existingDir))
                {
                    var objFiles = Directory.GetFiles(existingDir, "*.obj");
                    var mtlFiles = Directory.GetFiles(existingDir, "*.mtl");
                    if (objFiles.Length > 0 && mtlFiles.Length > 0)
                    {
                        var cameraFile = Path.Combine(existingDir, "camera.json");
                        var cameraJson = File.Exists(cameraFile) ? File.ReadAllText(cameraFile).Trim() : null;
                        return new Avatar3DCacheResult
                        {
                            Hash = existingHash,
                            DirectoryPath = existingDir,
                            ObjFileName = Path.GetFileName(objFiles[0]),
                            MtlFileName = Path.GetFileName(mtlFiles[0]),
                            AlreadyExisted = true,
                            CameraJson = cameraJson
                        };
                    }
                }
            }
        }

        var base64 = await RenderModel3DBase64Async(assetId, x, y, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(base64))
            throw new InvalidOperationException("Base64 payload for 3D model was empty.");

        byte[] bytes;
        string? rccCameraJson = null;
        if (base64.TrimStart().StartsWith("{"))
        {
            var jsonStr = base64;
            bytes = Encoding.UTF8.GetBytes(jsonStr);
            var hash = ComputeSha256(bytes);
            var dir = Path.Combine(baseDir!, hash);
            Directory.CreateDirectory(dir);

            try
            {
                using var previewDoc = JsonDocument.Parse(jsonStr);
                if (previewDoc.RootElement.TryGetProperty("camera", out var cameraEl) && cameraEl.ValueKind == JsonValueKind.Object)
                {
                    rccCameraJson = cameraEl.GetRawText();
                }
            }
            catch (Exception ex) { Console.WriteLine($"[ERROR] Parse RCC camera JSON: {ex}"); }

            var existingObjFiles = Directory.GetFiles(dir, "*.obj");
            var existingMtlFiles = Directory.GetFiles(dir, "*.mtl");
            if (existingObjFiles.Length > 0 && existingMtlFiles.Length > 0)
            {
                File.WriteAllText(mapPath, hash);
                return new Avatar3DCacheResult
                {
                    Hash = hash,
                    DirectoryPath = dir,
                    ObjFileName = Path.GetFileName(existingObjFiles[0]),
                    MtlFileName = Path.GetFileName(existingMtlFiles[0]),
                    AlreadyExisted = true,
                    CameraJson = rccCameraJson
                };
            }

            try
            {
                using var jDoc = JsonDocument.Parse(jsonStr);
                if (jDoc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    string objFileName = "model.obj";
                    string mtlFileName = "model.mtl";
                    bool extracted = false;

                    if (jDoc.RootElement.TryGetProperty("camera", out var cameraEl) && cameraEl.ValueKind == JsonValueKind.Object)
                    {
                        rccCameraJson = cameraEl.GetRawText();
                    }

                    JsonElement filesEl;
                    bool hasFilesWrapper = jDoc.RootElement.TryGetProperty("files", out filesEl) && filesEl.ValueKind == JsonValueKind.Object;

                    var props = hasFilesWrapper
                        ? filesEl.EnumerateObject()
                        : jDoc.RootElement.EnumerateObject();

                    foreach (var prop in props)
                    {
                        if (prop.Value.ValueKind != JsonValueKind.Object)
                            continue;
                        if (!prop.Value.TryGetProperty("content", out var contentEl) || contentEl.ValueKind != JsonValueKind.String)
                            continue;
                        if (!hasFilesWrapper && (string.Equals(prop.Name, "camera", StringComparison.OrdinalIgnoreCase) || string.Equals(prop.Name, "AABB", StringComparison.OrdinalIgnoreCase)))
                            continue;

                        var fileName = prop.Name;
                        var fileBase64 = contentEl.GetString()!;
                        byte[] fileBytes;
                        try { fileBytes = Convert.FromBase64String(fileBase64); }
                        catch { continue; }

                        var targetPath = Path.Combine(dir, fileName);
                        Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
                        File.WriteAllBytes(targetPath, fileBytes);
                        extracted = true;

                        var ext = Path.GetExtension(fileName).ToLowerInvariant();
                        if (ext == ".obj") objFileName = fileName;
                        else if (ext == ".mtl") mtlFileName = fileName;
                    }

                    if (extracted)
                    {
                        var mtlPath = Path.Combine(dir, mtlFileName);
                        if (File.Exists(mtlPath))
                        {
                            var cdnBase = _configuration?["Thumbnails:ThumbnailUrl"] ?? "https://cdn.freblx.xyz/";
                            cdnBase = cdnBase.TrimEnd('/') + "/";
                            var cdnPrefix = cdnBase + "3DAsset/" + hash + "/";

                            var mtlContent = File.ReadAllText(mtlPath);
                            var lines = mtlContent.Split('\n');
                            var rewritten = lines.Select(l => {
                                var trimmed = l.TrimStart();
                                if (trimmed.StartsWith("map_Kd", StringComparison.OrdinalIgnoreCase) || trimmed.StartsWith("map_kd", StringComparison.OrdinalIgnoreCase))
                                {
                                    var parts = l.Split(new char[] { ' ', '\t' }, 2, StringSplitOptions.RemoveEmptyEntries);
                                    if (parts.Length == 2 && !parts[1].Contains("://"))
                                        return parts[0] + " " + cdnPrefix + parts[1].Trim();
                                }
                                return l;
                            });
                            File.WriteAllText(mtlPath, string.Join("\n", rewritten));
                        }
                        else
                        {
                            File.WriteAllText(mtlPath, "newmtl default\nKd 1.000000 1.000000 1.000000\n");
                        }

                        if (!string.IsNullOrWhiteSpace(rccCameraJson))
                        {
                            File.WriteAllText(Path.Combine(dir, "camera.json"), rccCameraJson);
                        }
                        File.WriteAllText(mapPath, hash);
                        return new Avatar3DCacheResult
                        {
                            Hash = hash,
                            DirectoryPath = dir,
                            ObjFileName = objFileName,
                            MtlFileName = mtlFileName,
                            AlreadyExisted = false,
                            CameraJson = rccCameraJson
                        };
                    }
                }
            }
            catch
            {
            }
        }

        var commaIdx = -1;
        var base64Trimmed = base64.TrimStart();
        if (base64Trimmed.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            commaIdx = base64.IndexOf(',');
            if (commaIdx >= 0)
                base64 = base64.Substring(commaIdx + 1);
        }

        try
        {
            bytes = Convert.FromBase64String(base64);
        }
        catch (FormatException)
        {
            bytes = Encoding.UTF8.GetBytes(base64);
        }

        var hash2 = ComputeSha256(bytes);

        var dir2 = Path.Combine(baseDir!, hash2);
        Directory.CreateDirectory(dir2);

        var existingObjFiles2 = Directory.GetFiles(dir2, "*.obj");
        var existingMtlFiles2 = Directory.GetFiles(dir2, "*.mtl");
        if (existingObjFiles2.Length > 0 && existingMtlFiles2.Length > 0)
        {
            File.WriteAllText(mapPath, hash2);
            return new Avatar3DCacheResult
            {
                Hash = hash2,
                DirectoryPath = dir2,
                ObjFileName = Path.GetFileName(existingObjFiles2[0]),
                MtlFileName = Path.GetFileName(existingMtlFiles2[0]),
                AlreadyExisted = true
            };
        }

        string objFileName2 = "model.obj";
        string mtlFileName2 = "model.mtl";

        if (IsZip(bytes))
        {
            using var ms = new MemoryStream(bytes);
            using var zip = new ZipArchive(ms, ZipArchiveMode.Read);
            foreach (var entry in zip.Entries)
            {
                if (string.IsNullOrEmpty(entry.Name))
                    continue;

                var targetPath = Path.Combine(dir2, entry.Name);
                using var zs = entry.Open();
                using var fs = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None);
                zs.CopyTo(fs);

                var ext = Path.GetExtension(entry.Name).ToLowerInvariant();
                if (ext == ".obj")
                    objFileName2 = entry.Name;
                else if (ext == ".mtl")
                    mtlFileName2 = entry.Name;
            }

            var mtlPath = Path.Combine(dir2, mtlFileName2);
            if (!File.Exists(mtlPath))
                File.WriteAllText(mtlPath, "newmtl default\nKd 1.000000 1.000000 1.000000\n");
        }
        else
        {
            var objPath = Path.Combine(dir2, objFileName2);
            File.WriteAllBytes(objPath, bytes);

            var mtlPath = Path.Combine(dir2, mtlFileName2);
            if (!File.Exists(mtlPath))
                File.WriteAllText(mtlPath, "newmtl default\nKd 1.000000 1.000000 1.000000\n");
        }

        File.WriteAllText(mapPath, hash2);
        return new Avatar3DCacheResult
        {
            Hash = hash2,
            DirectoryPath = dir2,
            ObjFileName = objFileName2,
            MtlFileName = mtlFileName2,
            AlreadyExisted = false
        };
    }

    /// <summary>
    /// Check if a place has a custom icon set
    /// </summary>
    public async Task<bool> HasCustomIconAsync(long placeId, string connectionString, CancellationToken cancellationToken = default)
    {
        using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken);
        
        const string sql = @"SELECT custom_icon 
                               FROM assets 
                               WHERE asset_id = @placeId AND is_place = true";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("placeId", placeId);
        
        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        if (result == null || result == DBNull.Value)
            return false;
            
        return Convert.ToBoolean(result);
    }

    private string ResolveOutputDirectory(string? overrideOutputDirectory)
    {
        if (!string.IsNullOrWhiteSpace(overrideOutputDirectory))
            return overrideOutputDirectory!;

        var fromConfig = _configuration?[PrimaryConfigKey] ?? _configuration?[LegacyConfigKey];
        if (!string.IsNullOrWhiteSpace(fromConfig))
            return fromConfig!;

        // Fallback: ./thumbnails relative to current process
        return Path.Combine(AppContext.BaseDirectory, "thumbnails");
    }

    private static string Trunc(string s, int max = 1000)
    {
        if (s == null) return string.Empty;
        return s.Length <= max ? s : s.Substring(0, max);
    }

    private static bool IsPng(ReadOnlySpan<byte> bytes)
    {
        // 8-byte PNG signature
        if (bytes.Length < 8) return false;
        return bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47 &&
               bytes[4] == 0x0D && bytes[5] == 0x0A && bytes[6] == 0x1A && bytes[7] == 0x0A;
    }

    private static bool IsJpeg(ReadOnlySpan<byte> bytes)
    {
        // JPEG starts with FF D8 and ends with FF D9 (we just check start)
        if (bytes.Length < 2) return false;
        return bytes[0] == 0xFF && bytes[1] == 0xD8;
    }

    private static bool IsZip(ReadOnlySpan<byte> bytes)
    {
        // ZIP files start with 'PK' 0x03 0x04
        if (bytes.Length < 4) return false;
        return bytes[0] == 0x50 && bytes[1] == 0x4B && bytes[2] == 0x03 && bytes[3] == 0x04;
    }

    private static string ComputeSha256(byte[] bytes)
    {
        using var sha = SHA256.Create();
        var digest = sha.ComputeHash(bytes);
        var sb = new StringBuilder(digest.Length * 2);
        foreach (var b in digest)
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}

