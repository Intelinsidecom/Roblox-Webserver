using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace Assets
{
    public sealed class DecalAssetService
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

        public async Task<long> CreateDecalAsync(
            string connectionString,
            long ownerUserId,
            string name,
            string originalFileName,
            string contentType,
            byte[] fileBytes,
            string cdnAssetsRoot,
            string thumbnailsRoot,
            string thumbnailBaseUrl,
            string? publicAssetBaseUrl,
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

            // 1) ---------- Save uploaded PNG as backing image asset (type 1) ----------
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
                AssetTypeId = 1, // Image backing the Decal
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
            // Intentionally DO NOT add the backing image asset to user_assets; ownership should be on the Decal asset only.

            // 2) ---------- Compose Decal XML .rbxm referencing the image asset ----------
            var decalBaseUrl = !string.IsNullOrWhiteSpace(publicAssetBaseUrl) ? publicAssetBaseUrl : null;
            if (string.IsNullOrWhiteSpace(decalBaseUrl))
                decalBaseUrl = "http://localhost";

            var xml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<roblox xmlns:xmime=""http://www.w3.org/2005/05/xmlmime"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:noNamespaceSchemaLocation=""http://www.roblox.com/roblox.xsd"" version=""4"">
    <External>null</External>
    <External>nil</External>
    <Item class=""Decal"" referent=""RBX0"">
        <Properties>
            <Content name=""Texture""><url>{decalBaseUrl.TrimEnd('/', '\\')}/asset/?id={imageAssetId}</url></Content>
            <string name=""Name"">{System.Security.SecurityElement.Escape(name)}</string>
            <bool name=""archivable"">true</bool>
        </Properties>
    </Item>
</roblox>";

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

            // 3) ---------- Generate thumbnail from uploaded image ----------
            string? lowUrl = null;
            string? highResUrl = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(thumbnailsRoot) && !string.IsNullOrWhiteSpace(thumbnailBaseUrl))
                {
                    string thumbHash;
                    using (var sha = SHA256.Create())
                    {
                        var hBytes = sha.ComputeHash(pngBytes);
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

                    var thumbBase = thumbnailBaseUrl.TrimEnd('/', '\\');
                    var lowRelPath = ("thumbnails/" + lowFileName).TrimStart('/', '\\');
                    var highResRelPath = ("thumbnails/" + highResFileName).TrimStart('/', '\\');

                    const int highSize = 420;
                    const int lowSize = 110;

                    using (var ms = new MemoryStream(pngBytes))
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

                    lowUrl = string.IsNullOrEmpty(thumbBase) ? null : string.Concat(thumbBase, "/", lowRelPath);
                    highResUrl = string.IsNullOrEmpty(thumbBase) ? null : string.Concat(thumbBase, "/", highResRelPath);
                }
            }
            catch
            {
                // Swallow thumbnail failures; asset upload should still succeed.
            }

            // 4) ---------- Create Decal asset record (asset_type_id = 13) ----------
            var decalCreateParams = new AssetCreateParams
            {
                Name = name,
                AssetTypeId = 13, // Decal
                OwnerUserId = ownerUserId,
                ContentHash = xmlHash,
                FileExtension = xmlExtension,
                ContentType = "application/xml",
                ThumbnailUrl = lowUrl,
                HighResThumbnailUrl = highResUrl,
                Description = "Decal",
                IsCopyingAllowed = true
            };

            var decalAssetId = await _repository.CreateAssetAsync(connectionString, decalCreateParams, cancellationToken)
                .ConfigureAwait(false);
            await _userAssetsRepository.AddUserAssetAsync(connectionString, ownerUserId, decalAssetId, cancellationToken)
                .ConfigureAwait(false);

            // Mark the image asset as being the image for this Decal and link it.
            await _repository.UpdateAssetImageLinkAsync(connectionString, imageAssetId, true, decalAssetId, cancellationToken)
                .ConfigureAwait(false);

            return decalAssetId;
        }
    }
}
