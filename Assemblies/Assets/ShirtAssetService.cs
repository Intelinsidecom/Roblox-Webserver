using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Http;
using System.Text.Json;

namespace Assets
{
    public sealed class ShirtAssetService
    {
        private readonly AssetsRepository _repository = new AssetsRepository();
        private readonly UserAssetsRepository _userAssetsRepository = new UserAssetsRepository();

        private static byte[] ConvertToPng(byte[] originalBytes)
        {
            using (var input = new MemoryStream(originalBytes))
            using (var image = new Bitmap(input))
            using (var output = new MemoryStream())
            {
                image.Save(output, ImageFormat.Png);
                return output.ToArray();
            }
        }

        public async Task<long> CreateShirtAsync(
            string connectionString,
            long ownerUserId,
            string name,
            string originalFileName,
            string contentType,
            byte[] fileBytes,
            string cdnAssetsRoot,
            string baseUrl,
            string thumbnailsRoot,
            string thumbnailBaseUrl,
            string? publicAssetBaseUrl,
            string? arbiterBaseUrl,
            bool bypassLimits = false,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (ownerUserId <= 0)
                throw new ArgumentOutOfRangeException(nameof(ownerUserId));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("name is required", nameof(name));
            if (fileBytes == null || fileBytes.Length == 0)
                throw new ArgumentException("fileBytes is required", nameof(fileBytes));
            if (string.IsNullOrWhiteSpace(cdnAssetsRoot))
                throw new ArgumentException("cdnAssetsRoot is required", nameof(cdnAssetsRoot));
            if (string.IsNullOrWhiteSpace(thumbnailsRoot))
                throw new ArgumentException("thumbnailsRoot is required", nameof(thumbnailsRoot));

            try
            {
                using (var input = new MemoryStream(fileBytes))
                using (var image = new Bitmap(input))
                {
                    if (!bypassLimits && (image.Width != 585 || image.Height != 559))
                        throw new ArgumentException("Shirt image must be exactly 585x559 pixels.", nameof(fileBytes));
                }
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Invalid image file.", ex);
            }

            var pngBytes = ConvertToPng(fileBytes);

            string pngHash;
            using (var sha = SHA256.Create())
            {
                var hashBytes = sha.ComputeHash(pngBytes);
                var sb = new StringBuilder(hashBytes.Length * 2);
                foreach (var b in hashBytes)
                    sb.Append(b.ToString("x2"));
                pngHash = sb.ToString();
            }

            var assetFolder = Path.Combine(cdnAssetsRoot, "asset");
            Directory.CreateDirectory(assetFolder);

            var pngExtension = ".png";
            var pngFileName = pngHash + pngExtension;
            var pngFullPath = Path.Combine(assetFolder, pngFileName);

            using (var fs = new FileStream(pngFullPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            {
                await fs.WriteAsync(pngBytes, 0, pngBytes.Length, cancellationToken).ConfigureAwait(false);
            }

            var imageCreateParams = new AssetCreateParams
            {
                Name = name + " Image",
                AssetTypeId = 1, // Image / Decal backing the Shirt
                OwnerUserId = ownerUserId,
                ContentHash = pngHash,
                FileExtension = pngExtension,
                ContentType = "image/png",
                ThumbnailUrl = null,
                AssetImage = false,
                AssetLink = null
            };

            var imageAssetId = await _repository.CreateAssetAsync(connectionString, imageCreateParams, cancellationToken)
                .ConfigureAwait(false);
            // Intentionally DO NOT add the backing image asset to user_assets; ownership should be on the Shirt asset only.

            var graphicBaseUrl = !string.IsNullOrWhiteSpace(publicAssetBaseUrl) ? publicAssetBaseUrl : baseUrl;
            if (string.IsNullOrWhiteSpace(graphicBaseUrl))
                graphicBaseUrl = "http://localhost";

            var xml = $"<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                      "<roblox xmlns:xmime=\"http://www.w3.org/2005/05/xmlmime\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"http://www.roblox.com/roblox.xsd\" version=\"4\">\n" +
                      "    <External>null</External>\n" +
                      "    <External>nil</External>\n" +
                      "    <Item class=\"Shirt\" referent=\"RBX0\">\n" +
                      "        <Properties>\n" +
                      $"            <Content name=\"ShirtTemplate\"><url>{graphicBaseUrl.TrimEnd('/', '\\')}/asset/?id={imageAssetId}</url></Content>\n" +
                      $"            <string name=\"Name\">{System.Security.SecurityElement.Escape(name)}</string>\n" +
                      "            <bool name=\"archivable\">true</bool>\n" +
                      "        </Properties>\n" +
                      "    </Item>\n" +
                      "</roblox>";

            string xmlHash;
            using (var sha = SHA256.Create())
            {
                var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(xml));
                var sb = new StringBuilder(hashBytes.Length * 2);
                foreach (var b in hashBytes)
                    sb.Append(b.ToString("x2"));
                xmlHash = sb.ToString();
            }

            var xmlExtension = ".rbxm";
            var xmlFileName = xmlHash + xmlExtension;
            var xmlFullPath = Path.Combine(assetFolder, xmlFileName);
            File.WriteAllText(xmlFullPath, xml, new UTF8Encoding(false));

            var shirtCreateParams = new AssetCreateParams
            {
                Name = name,
                AssetTypeId = 11, // Shirt
                OwnerUserId = ownerUserId,
                ContentHash = xmlHash,
                FileExtension = xmlExtension,
                ContentType = "application/xml",
                ThumbnailUrl = null,
                HighResThumbnailUrl = null,
                Description = "Shirt"
            };

            var shirtAssetId = await _repository.CreateAssetAsync(connectionString, shirtCreateParams, cancellationToken)
                .ConfigureAwait(false);
            await _userAssetsRepository.AddUserAssetAsync(connectionString, ownerUserId, shirtAssetId, cancellationToken)
                .ConfigureAwait(false);

            await _repository.UpdateAssetImageLinkAsync(connectionString, imageAssetId, true, shirtAssetId, cancellationToken)
                .ConfigureAwait(false);

            try
            {
                using var http = new HttpClient { Timeout = Common.HttpClientDefaults.Timeout };
                var baseUrlToUse = string.IsNullOrWhiteSpace(arbiterBaseUrl)
                    ? "http://localhost:5000"
                    : arbiterBaseUrl!.TrimEnd('/', '\\');

                var requestUri = $"{baseUrlToUse}/renderavatarasset?assetId={shirtAssetId}";

                using var response = await http.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    using var doc = JsonDocument.Parse(json);

                    string? renderedDataUri = null;

                    if (doc.RootElement.ValueKind == JsonValueKind.Object)
                    {
                        if (doc.RootElement.TryGetProperty("thumbnailUrl", out var tEl) && tEl.ValueKind == JsonValueKind.String)
                        {
                            renderedDataUri = tEl.GetString();
                        }
                    }
                    else if (doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0)
                    {
                        var first = doc.RootElement[0];
                        if (first.ValueKind == JsonValueKind.Object && first.TryGetProperty("value", out var vEl) && vEl.ValueKind == JsonValueKind.String)
                        {
                            renderedDataUri = vEl.GetString();
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(renderedDataUri))
                    {
                        try
                        {
                            var commaIdx = renderedDataUri.IndexOf(',');
                            var base64Part = commaIdx >= 0 ? renderedDataUri.Substring(commaIdx + 1) : renderedDataUri;
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

                            var lowFileName = thumbHash + "_lowquality.png";
                            var lowPath = Path.Combine(thumbnailsRoot, lowFileName);

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

                            var thumbBase = thumbnailBaseUrl.TrimEnd('/', '\\');
                            var lowRelPath = ("thumbnails/" + lowFileName).TrimStart('/', '\\');
                            var highResRelPath = ("thumbnails/" + highResFileName).TrimStart('/', '\\');

                            var lowUrl = string.IsNullOrEmpty(thumbBase) ? null : string.Concat(thumbBase, "/", lowRelPath);
                            var highResUrl = string.IsNullOrEmpty(thumbBase) ? null : string.Concat(thumbBase, "/", highResRelPath);

                            await _repository.UpdateAssetThumbnailsAsync(connectionString, shirtAssetId, lowUrl, highResUrl, cancellationToken)
                                .ConfigureAwait(false);
                            Console.WriteLine($"[ShirtAssetService] Thumbnails stored for asset {shirtAssetId}: low={lowUrl}, high={highResUrl}");
                        }
                        catch (Exception exThumb)
                        {
                            Console.WriteLine($"[ShirtAssetService] Failed to process Arbiter thumbnail for asset {shirtAssetId}: {exThumb.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[ShirtAssetService] Arbiter returned no usable thumbnail for asset {shirtAssetId}.");
                    }
                }
                else
                {
                    Console.WriteLine($"[ShirtAssetService] Arbiter HTTP {response.StatusCode} for {requestUri}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ShirtAssetService] Failed to contact Arbiter for asset {shirtAssetId}: {ex.Message}");
            }

            return shirtAssetId;
        }
    }
}
