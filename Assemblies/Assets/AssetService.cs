using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Assets
{
    public class AssetService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public AssetService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Gets the character fetch response for a user, returning semicolon-separated URLs for worn assets
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="scheme">The request scheme (http/https)</param>
        /// <param name="host">The request host</param>
        /// <returns>Semicolon-separated URLs for character assets</returns>
        public async Task<string> GetCharacterFetchAsync(string userId, string scheme, string host)
        {
            var pid = string.IsNullOrWhiteSpace(userId) ? "0" : userId;
            var baseUrl = $"{scheme}://{host}";

            long.TryParse(pid, out var uid);

            var wornAssetIds = new List<long>();
            var connStr = _configuration.GetConnectionString("Default");
            if (!string.IsNullOrWhiteSpace(connStr) && uid > 0)
            {
                try
                {
                    await using var conn = new NpgsqlConnection(connStr);
                    await conn.OpenAsync().ConfigureAwait(false);

                    const string sql = @"select awa.asset_id
from avatar_worn_assets awa
join assets a on a.asset_id = awa.asset_id
where awa.user_id = @uid
order by awa.asset_id";

                    using var cmd = new NpgsqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("uid", uid);

                    await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var assetId = reader.GetInt64(0);
                        wornAssetIds.Add(assetId);
                    }
                }
                catch
                {
                    // Ignore DB errors and fall back to empty list
                }
            }

            var bodyColorsUrl = $"{baseUrl}/asset/bodycolors.ashx?userId={pid}";
            var urls = new List<string> { bodyColorsUrl };

            foreach (var assetId in wornAssetIds)
            {
                urls.Add($"{baseUrl}/asset/?id={assetId}");
            }

            // Response format (semicolon-separated URLs), per request:
            // http://your.url.here/Asset/bodycolors.ashx;http://your.url.here/Asset/?id=TSHIRT;http://your.url.here/Asset/?id=PANTS
            return string.Join(";", urls);
        }

        /// <summary>
        /// Attempts to fetch an asset from Roblox's asset delivery service
        /// </summary>
        /// <param name="assetId">The asset ID to fetch</param>
        /// <param name="contentType">The expected content type</param>
        /// <returns>File stream result if successful, null if failed</returns>
        public async Task<(Stream? Stream, string ContentType, string? Error)> TryFetchFromRobloxAssetDeliveryAsync(long assetId, string? contentType)
        {
            try
            {
                var baseUrl = _configuration["RobloxAssetDelivery:BaseUrl"] ?? "https://assetdelivery.roblox.com";
                var client = _httpClientFactory.CreateClient();
                
                var response = await client.GetAsync($"{baseUrl}/v1/asset/?id={assetId}");
                if (!response.IsSuccessStatusCode)
                {
                    return (null, string.Empty, "Asset not found locally or on Roblox asset delivery");
                }

                var stream = await response.Content.ReadAsStreamAsync();
                var ct = string.IsNullOrWhiteSpace(contentType) 
                    ? response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream" 
                    : contentType;
                
                return (stream, ct, null);
            }
            catch (Exception)
            {
                return (null, string.Empty, "Failed to fetch asset from Roblox asset delivery");
            }
        }

        /// <summary>
        /// Gets asset metadata including content hash, file extension, and content type
        /// </summary>
        /// <param name="assetId">The asset ID</param>
        /// <returns>Asset metadata or null if not found</returns>
        public async Task<(string? Hash, string? Extension, string? ContentType)> GetAssetMetadataAsync(long assetId)
        {
            var connStr = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
                return (null, null, null);

            try
            {
                await using var conn = new NpgsqlConnection(connStr);
                await conn.OpenAsync().ConfigureAwait(false);

                const string sql = @"select content_hash, file_extension, content_type from assets where asset_id = @id";
                using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("id", assetId);

                await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
                if (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var hash = reader.IsDBNull(0) ? null : reader.GetString(0);
                    var ext = reader.IsDBNull(1) ? null : reader.GetString(1);
                    var contentType = reader.IsDBNull(2) ? null : reader.GetString(2);
                    return (hash, ext, contentType);
                }
            }
            catch
            {
                // Return null on any error
            }

            return (null, null, null);
        }

        /// <summary>
        /// Gets the full file path for an asset
        /// </summary>
        /// <param name="hash">The content hash</param>
        /// <param name="extension">The file extension</param>
        /// <returns>Full file path or null if assets directory not configured</returns>
        public string? GetAssetFilePath(string? hash, string? extension)
        {
            if (string.IsNullOrWhiteSpace(hash))
                return null;

            var assetsRoot = _configuration["Assets:Directory"];
            if (string.IsNullOrWhiteSpace(assetsRoot))
                return null;

            var assetFolder = Path.Combine(assetsRoot, "asset");
            var fileName = string.IsNullOrWhiteSpace(extension) ? hash : hash + extension;
            return Path.Combine(assetFolder, fileName);
        }


        public bool IsRobloxAssetDeliveryEnabled()
        {
            var configValue = _configuration["RobloxAssetDelivery:Enabled"];
            return bool.TryParse(configValue, out var enabled) && enabled;
        }

        /// <summary>
        /// Renders a thumbnail for an asset via Arbiter and updates the database with the thumbnail URLs
        /// </summary>
        /// <param name="assetId">The asset ID to render</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if thumbnail was successfully rendered and updated, false otherwise</returns>
        public async Task<bool> RenderAssetThumbnailAsync(long assetId, CancellationToken cancellationToken = default)
        {
            var connStr = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
            {
                Console.WriteLine("[AssetService] Database connection string not configured");
                return false;
            }

            int assetTypeId;
            try
            {
                await using var conn = new NpgsqlConnection(connStr);
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

                const string sql = "SELECT asset_type_id FROM assets WHERE asset_id = @id";
                using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("id", assetId);

                var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                if (result == null || result == DBNull.Value)
                {
                    Console.WriteLine($"[AssetService] Asset {assetId} not found");
                    return false;
                }
                assetTypeId = Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                return false;
            }

            var thumbnailsRoot = _configuration["Thumbnails:OutputDirectory"];
            var thumbnailBaseUrl = _configuration["Thumbnails:ThumbnailUrl"];

            if (string.IsNullOrWhiteSpace(thumbnailsRoot) || string.IsNullOrWhiteSpace(thumbnailBaseUrl))
            {
                return false;
            }

            if (assetTypeId == 13)
            {
                return await RenderDecalThumbnailAsync(assetId, connStr, thumbnailsRoot, thumbnailBaseUrl, cancellationToken).ConfigureAwait(false);
            }

            var arbiterUrl = _configuration["Thumbnails:ArbiterUrl"] ?? "http://localhost:5000";

            string arbiterEndpoint = GetArbiterEndpointForAssetType(assetTypeId);
            var requestUri = $"{arbiterUrl.TrimEnd('/')}{arbiterEndpoint}?assetId={assetId}&x=420&y=420";

            try
            {
                using var http = new HttpClient();
                using var response = await http.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);
                
                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                string? renderedDataUri = ExtractThumbnailDataUri(json);
                if (string.IsNullOrWhiteSpace(renderedDataUri))
                {
                    return false;
                }

                var (lowUrl, highResUrl) = await SaveThumbnailsAsync(renderedDataUri, thumbnailsRoot, thumbnailBaseUrl, cancellationToken).ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(lowUrl) || string.IsNullOrWhiteSpace(highResUrl))
                {
                    return false;
                }

                var repo = new AssetsRepository();
                await repo.UpdateAssetThumbnailsAsync(connStr, assetId, lowUrl, highResUrl, cancellationToken).ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AssetService] Failed to render thumbnail for asset {assetId}: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> RenderDecalThumbnailAsync(long assetId, string connStr, string thumbnailsRoot, string thumbnailBaseUrl, CancellationToken cancellationToken)
        {
            var (hash, ext, contentType) = await GetAssetMetadataAsync(assetId);
            if (string.IsNullOrWhiteSpace(hash))
            {
                Console.WriteLine($"[AssetService] Decal {assetId}: no content hash found");
                return false;
            }

            byte[] imageBytes;
            bool isNewStyle = string.Equals(ext, ".rbxm", StringComparison.OrdinalIgnoreCase) ||
                              string.Equals(contentType, "application/xml", StringComparison.OrdinalIgnoreCase);

            if (isNewStyle)
            {
                // New-style decal: find the backing image (type 1) via asset_link
                long? imageAssetId = null;
                try
                {
                    await using var conn = new NpgsqlConnection(connStr);
                    await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

                    const string sql = @"SELECT asset_id FROM assets WHERE asset_link = @decalId AND asset_image = true LIMIT 1";
                    using var cmd = new NpgsqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("decalId", assetId);
                    var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                    if (result != null && result != DBNull.Value)
                        imageAssetId = Convert.ToInt64(result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[AssetService] Decal {assetId}: failed to find backing image: {ex.Message}");
                }

                if (imageAssetId.HasValue)
                {
                    var (imgHash, imgExt, _) = await GetAssetMetadataAsync(imageAssetId.Value);
                    if (!string.IsNullOrWhiteSpace(imgHash))
                    {
                        var imgPath = GetAssetFilePath(imgHash, imgExt);
                        if (!string.IsNullOrWhiteSpace(imgPath) && System.IO.File.Exists(imgPath))
                        {
                            try
                            {
                                imageBytes = await Task.Run(() => System.IO.File.ReadAllBytes(imgPath), cancellationToken).ConfigureAwait(false);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[AssetService] Decal {assetId}: failed to read backing image: {ex.Message}");
                                return false;
                            }
                        }
                        else
                        {
                            Console.WriteLine($"[AssetService] Decal {assetId}: backing image file not found at {imgPath}");
                            return false;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[AssetService] Decal {assetId}: backing image has no content hash");
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine($"[AssetService] Decal {assetId}: no backing image found via asset_link");
                    return false;
                }
            }
            else
            {
                // Old-style decal: raw image stored directly as {hash}{assetTypeId}
                var filePath = GetAssetFilePath(hash, ext);
                if (string.IsNullOrWhiteSpace(filePath) || !System.IO.File.Exists(filePath))
                {
                    Console.WriteLine($"[AssetService] Decal {assetId}: asset file not found at {filePath}");
                    return false;
                }

                try
                {
                    imageBytes = await Task.Run(() => System.IO.File.ReadAllBytes(filePath), cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[AssetService] Decal {assetId}: failed to read asset file: {ex.Message}");
                    return false;
                }
            }

            string thumbHash;
            using (var sha = SHA256.Create())
            {
                var hBytes = sha.ComputeHash(imageBytes);
                var sbh = new StringBuilder(hBytes.Length * 2);
                foreach (var b in hBytes)
                    sbh.Append(b.ToString("x2"));
                thumbHash = sbh.ToString();
            }

            Directory.CreateDirectory(thumbnailsRoot);

            var highResFileName = thumbHash + "_highres.png";
            var highResPath = Path.Combine(thumbnailsRoot, highResFileName);
            var lowFileName = thumbHash + ".png";
            var lowPath = Path.Combine(thumbnailsRoot, lowFileName);

            const int highSize = 420;
            const int lowSize = 110;

            try
            {
                await Task.Run(() =>
                {
                    using (var ms = new MemoryStream(imageBytes))
                    using (var original = new Bitmap(ms))
                    {
                        using (var highBmp = new Bitmap(highSize, highSize))
                        using (var gHigh = Graphics.FromImage(highBmp))
                        {
                            gHigh.Clear(Color.White);
                            var ratio = Math.Min((float)highSize / original.Width, (float)highSize / original.Height);
                            var newWidth = (int)(original.Width * ratio);
                            var newHeight = (int)(original.Height * ratio);
                            var offsetX = (highSize - newWidth) / 2;
                            var offsetY = (highSize - newHeight) / 2;
                            gHigh.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                            gHigh.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                            gHigh.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            gHigh.DrawImage(original, offsetX, offsetY, newWidth, newHeight);
                            highBmp.Save(highResPath, ImageFormat.Png);
                        }

                        using (var lowBmp = new Bitmap(lowSize, lowSize))
                        using (var gLow = Graphics.FromImage(lowBmp))
                        {
                            gLow.Clear(Color.White);
                            var ratio = Math.Min((float)lowSize / original.Width, (float)lowSize / original.Height);
                            var newWidth = (int)(original.Width * ratio);
                            var newHeight = (int)(original.Height * ratio);
                            var offsetX = (lowSize - newWidth) / 2;
                            var offsetY = (lowSize - newHeight) / 2;
                            gLow.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                            gLow.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                            gLow.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            gLow.DrawImage(original, offsetX, offsetY, newWidth, newHeight);
                            lowBmp.Save(lowPath, ImageFormat.Png);
                        }
                    }
                }, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AssetService] Decal {assetId}: failed to process thumbnail: {ex.Message}");
                return false;
            }

            var thumbBase = thumbnailBaseUrl.TrimEnd('/', '\\');
            var lowRelPath = ("thumbnails/" + lowFileName).TrimStart('/', '\\');
            var highResRelPath = ("thumbnails/" + highResFileName).TrimStart('/', '\\');

            var lowUrl = string.IsNullOrEmpty(thumbBase) ? null : string.Concat(thumbBase, "/", lowRelPath);
            var highResUrl = string.IsNullOrEmpty(thumbBase) ? null : string.Concat(thumbBase, "/", highResRelPath);

            if (string.IsNullOrWhiteSpace(lowUrl) || string.IsNullOrWhiteSpace(highResUrl))
                return false;

            var repo = new AssetsRepository();
            await repo.UpdateAssetThumbnailsAsync(connStr, assetId, lowUrl, highResUrl, cancellationToken).ConfigureAwait(false);
            Console.WriteLine($"[AssetService] Decal {assetId}: thumbnail saved (low={lowUrl}, high={highResUrl})");
            return true;
        }

        private string GetArbiterEndpointForAssetType(int assetTypeId)
        {
            return assetTypeId switch
            {
                4 => "/rendermesh",            // Mesh
                9 => "/rendergame",           // Place
                10 => "/rendermodel",           // Model
                11 => "/renderavatarasset",     // Shirt
                12 => "/renderavatarasset",     // Pants
                2 => "/renderavatarasset",      // T-Shirt
                _ => "/renderavatarasset"       // Default
            };
        }

        private string? ExtractThumbnailDataUri(string json)
        {
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.ValueKind == JsonValueKind.Object)
            {
                if (doc.RootElement.TryGetProperty("thumbnailUrl", out var tEl) && tEl.ValueKind == JsonValueKind.String)
                {
                    return tEl.GetString();
                }
            }
            else if (doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0)
            {
                var first = doc.RootElement[0];
                if (first.ValueKind == JsonValueKind.Object && first.TryGetProperty("value", out var vEl) && vEl.ValueKind == JsonValueKind.String)
                {
                    return vEl.GetString();
                }
            }

            return null;
        }

        private async Task<(string? LowUrl, string? HighResUrl)> SaveThumbnailsAsync(
            string dataUri,
            string thumbnailsRoot,
            string thumbnailBaseUrl,
            CancellationToken cancellationToken)
        {
            var commaIdx = dataUri.IndexOf(',');
            var base64Part = commaIdx >= 0 ? dataUri.Substring(commaIdx + 1) : dataUri;
            var thumbBytes = Convert.FromBase64String(base64Part);

            string thumbHash;
            using (var sha = SHA256.Create())
            {
                var hBytes = sha.ComputeHash(thumbBytes);
                var sbh = new StringBuilder(hBytes.Length * 2);
                foreach (var b in hBytes)
                    sbh.Append(b.ToString("x2"));
                thumbHash = sbh.ToString();
            }

            Directory.CreateDirectory(thumbnailsRoot);

            var highResFileName = thumbHash + "_highres.png";
            var highResPath = Path.Combine(thumbnailsRoot, highResFileName);

            var lowFileName = thumbHash + ".png";
            var lowPath = Path.Combine(thumbnailsRoot, lowFileName);

            await Task.Run(() =>
            {
                using (var ms = new MemoryStream(thumbBytes))
                using (var original = new Bitmap(ms))
                {
                    original.Save(highResPath, ImageFormat.Png);

                    const int lowSize = 110;
                    using (var lowBmp = new Bitmap(lowSize, lowSize))
                    using (var g = Graphics.FromImage(lowBmp))
                    {
                        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                        g.DrawImage(original, 0, 0, lowSize, lowSize);
                        lowBmp.Save(lowPath, ImageFormat.Png);
                    }
                }
            }, cancellationToken).ConfigureAwait(false);

            var thumbBase = thumbnailBaseUrl.TrimEnd('/', '\\');
            var lowRelPath = ("thumbnails/" + lowFileName).TrimStart('/', '\\');
            var highResRelPath = ("thumbnails/" + highResFileName).TrimStart('/', '\\');

            var lowUrl = string.IsNullOrEmpty(thumbBase) ? null : string.Concat(thumbBase, "/", lowRelPath);
            var highResUrl = string.IsNullOrEmpty(thumbBase) ? null : string.Concat(thumbBase, "/", highResRelPath);

            return (lowUrl, highResUrl);
        }
    }
}
