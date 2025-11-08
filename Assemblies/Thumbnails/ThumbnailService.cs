using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;

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

        var fileName = hash + ".png";
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

    public async Task<string> RenderAvatarAsync(string type, long userId, int? x = null, int? y = null, CancellationToken cancellationToken = default)
    {
        var arbiterUrl = _configuration?["Thumbnails:ArbiterUrl"] ?? "http://localhost:5000";
        var uploadUrl = _configuration?["Thumbnails:UploadUrl"] ?? "http://localhost:5077/api/thumbnails/upload";
        var accessKey = _configuration?["Upload:AccessKey"] ?? string.Empty;

        var qb = new StringBuilder();
        qb.Append("type=").Append(Uri.EscapeDataString(type ?? "headshot"));
        qb.Append("&userId=").Append(Uri.EscapeDataString(userId.ToString()));
        if (x.HasValue) qb.Append("&x=").Append(x.Value);
        if (y.HasValue) qb.Append("&y=").Append(y.Value);

        var requestUri = arbiterUrl.TrimEnd('/') + "/renderavatar?" + qb.ToString();

        using var http = new HttpClient();
        using var req = new HttpRequestMessage(HttpMethod.Get, requestUri);
        using var resp = await http.SendAsync(req, cancellationToken).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);

        using var doc = JsonDocument.Parse(json);

        if (doc.RootElement.ValueKind == JsonValueKind.Array)
        {
            var len = doc.RootElement.GetArrayLength();
            if (len == 0)
                throw new InvalidOperationException("Unexpected response from Arbiter. Raw: " + Trunc(json));

            for (int i = len - 1; i >= 0; i--)
            {
                var el = doc.RootElement[i];
                string? val = null;
                if (el.ValueKind == JsonValueKind.Object)
                {
                    if (el.TryGetProperty("value", out var vEl) && vEl.ValueKind == JsonValueKind.String)
                        val = vEl.GetString();
                }
                else if (el.ValueKind == JsonValueKind.String)
                {
                    val = el.GetString();
                }
                if (string.IsNullOrWhiteSpace(val))
                    continue;
                try
                {
                    using var innerTry = JsonDocument.Parse(val!);
                    var hashTry = innerTry.RootElement.TryGetProperty("hash", out var hElTry) ? hElTry.GetString() : null;
                    if (!string.IsNullOrWhiteSpace(hashTry))
                        return hashTry!;
                }
                catch (JsonException)
                {
                }
            }
            throw new InvalidOperationException("Unexpected response from Arbiter. Raw: " + Trunc(json));
        }
        else if (doc.RootElement.ValueKind == JsonValueKind.Object)
        {
            if (doc.RootElement.TryGetProperty("hash", out var hEl) && hEl.ValueKind == JsonValueKind.String)
            {
                var direct = hEl.GetString();
                if (!string.IsNullOrWhiteSpace(direct))
                    return direct!;
            }
            if (doc.RootElement.TryGetProperty("value", out var vEl) && vEl.ValueKind == JsonValueKind.String)
            {
                var val = vEl.GetString();
                if (!string.IsNullOrWhiteSpace(val))
                {
                    try
                    {
                        using var inner = JsonDocument.Parse(val!);
                        var hash = inner.RootElement.TryGetProperty("hash", out var hEl2) ? hEl2.GetString() : null;
                        if (!string.IsNullOrWhiteSpace(hash))
                            return hash!;
                    }
                    catch (JsonException)
                    {
                    }
                }
            }
            throw new InvalidOperationException("Unexpected response from Arbiter. Raw: " + Trunc(json));
        }
        else if (doc.RootElement.ValueKind == JsonValueKind.String)
        {
            var val = doc.RootElement.GetString();
            if (!string.IsNullOrWhiteSpace(val))
            {
                using var inner = JsonDocument.Parse(val!);
                var hash = inner.RootElement.TryGetProperty("hash", out var hEl) ? hEl.GetString() : null;
                if (!string.IsNullOrWhiteSpace(hash))
                    return hash!;
            }
            throw new InvalidOperationException("Unexpected response from Arbiter. Raw(inner): " + Trunc(val ?? string.Empty));
        }
        else
        {
            throw new InvalidOperationException("Unexpected response from Arbiter. Raw: " + Trunc(json));
        }
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
}

