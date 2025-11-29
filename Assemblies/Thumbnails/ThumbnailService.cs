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

namespace Thumbnails;

public sealed class ThumbnailService : IThumbnailService
{
    private readonly IConfiguration? _configuration;

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

        var requestUri = arbiterUrl.TrimEnd('/') + "/renderavatar?" + qb.ToString();

        using var http = new HttpClient();
        using var req = new HttpRequestMessage(HttpMethod.Get, requestUri);
        using var resp = await http.SendAsync(req, cancellationToken).ConfigureAwait(false);

        var json = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
        if (!resp.IsSuccessStatusCode)
        {
            var statusCode = (int)resp.StatusCode;
            var reason = resp.ReasonPhrase ?? string.Empty;
            throw new HttpRequestException($"Arbiter /renderavatar returned {statusCode} {reason}. Body: {Trunc(json)}");
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

    public async Task<Avatar3DCacheResult> RenderAvatar3DAndCacheAsync(long userId, int? x = null, int? y = null, CancellationToken cancellationToken = default)
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

    
        if (File.Exists(mapPath))
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
                        return new Avatar3DCacheResult
                        {
                            Hash = existingHash,
                            DirectoryPath = existingDir,
                            ObjFileName = Path.GetFileName(objFiles[0]),
                            MtlFileName = Path.GetFileName(mtlFiles[0]),
                            AlreadyExisted = true
                        };
                    }
                }
            }
        }

        var base64 = await RenderAvatar3DBase64Async(userId, x, y, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(base64))
            throw new InvalidOperationException("Base64 payload for 3D avatar was empty.");

        var commaIdx = base64.IndexOf(',');
        if (commaIdx >= 0)
            base64 = base64.Substring(commaIdx + 1);

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(base64);
        }
        catch (FormatException)
        {
            bytes = Encoding.UTF8.GetBytes(base64);
        }

        var hash = ComputeSha256(bytes);

        var dir = Path.Combine(baseDir!, hash);
        Directory.CreateDirectory(dir);

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
                AlreadyExisted = true
            };
        }

        string objFileName = "avatar.obj";
        string mtlFileName = "avatar.mtl";


        if (IsZip(bytes))
        {
            using var ms = new MemoryStream(bytes);
            using var zip = new ZipArchive(ms, ZipArchiveMode.Read);
            foreach (var entry in zip.Entries)
            {
                if (string.IsNullOrEmpty(entry.Name))
                    continue;

                var targetPath = Path.Combine(dir, entry.Name);
                using var zs = entry.Open();
                using var fs = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None);
                zs.CopyTo(fs);

                var ext = Path.GetExtension(entry.Name).ToLowerInvariant();
                if (ext == ".obj")
                {
                    objFileName = entry.Name;
                }
                else if (ext == ".mtl")
                {
                    mtlFileName = entry.Name;
                }
            }

            // Ensure we have at least a basic MTL file
            var mtlPath = Path.Combine(dir, mtlFileName);
            if (!File.Exists(mtlPath))
            {
                File.WriteAllText(mtlPath, "newmtl default\nKd 1.000000 1.000000 1.000000\n");
            }
        }
        else
        {
            // Fallback: assume the payload itself is an OBJ file
            var objPath = Path.Combine(dir, objFileName);
            File.WriteAllBytes(objPath, bytes);

            // Create a simple default MTL file so loaders expecting MTL still work
            var mtlPath = Path.Combine(dir, mtlFileName);
            if (!File.Exists(mtlPath))
            {
                File.WriteAllText(mtlPath, "newmtl default\nKd 1.000000 1.000000 1.000000\n");
            }
        }

        return new Avatar3DCacheResult
        {
            Hash = hash,
            DirectoryPath = dir,
            ObjFileName = objFileName,
            MtlFileName = mtlFileName,
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

