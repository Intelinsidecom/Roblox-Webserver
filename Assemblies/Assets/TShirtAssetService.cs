using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace Assets
{
    public sealed class TShirtAssetService
    {
        private readonly AssetsRepository _repository = new AssetsRepository();
        private readonly UserAssetsRepository _userAssetsRepository = new UserAssetsRepository();

        public async Task<long> CreateTShirtAsync(
            string connectionString,
            long ownerUserId,
            string name,
            string originalFileName,
            string contentType,
            byte[] fileBytes,
            string cdnAssetsRoot,
            string thumbnailsRoot,
            string thumbnailBaseUrl,
            string tshirtTemplatePath,
            string baseUrl,
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

            // 1) ---------- Save uploaded PNG as image asset ------------------
            string pngHash;
            using (var sha = SHA256.Create())
            {
                var hashBytes = sha.ComputeHash(fileBytes);
                var sb = new StringBuilder(hashBytes.Length * 2);
                foreach (var b in hashBytes)
                    sb.Append(b.ToString("x2"));
                pngHash = sb.ToString();
            }

            var assetFolder = Path.Combine(cdnAssetsRoot, "asset");
            Directory.CreateDirectory(assetFolder);

            var pngExtension = Path.GetExtension(originalFileName);
            if (string.IsNullOrWhiteSpace(pngExtension))
                pngExtension = ".png";
            var pngFileName = pngHash + pngExtension;
            var pngFullPath = Path.Combine(assetFolder, pngFileName);

            using (var fs = new FileStream(pngFullPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            {
                await fs.WriteAsync(fileBytes, 0, fileBytes.Length, cancellationToken).ConfigureAwait(false);
            }

            var imageCreateParams = new AssetCreateParams
            {
                Name = name + " Image",
                AssetTypeId = 1, // Image / Decal
                OwnerUserId = ownerUserId,
                ContentHash = pngHash,
                FileExtension = pngExtension,
                ContentType = contentType,
                ThumbnailUrl = null
            };
            var imageAssetId = await _repository.CreateAssetAsync(connectionString, imageCreateParams, cancellationToken)
                .ConfigureAwait(false);
            await _userAssetsRepository.AddUserAssetAsync(connectionString, ownerUserId, imageAssetId, cancellationToken)
                .ConfigureAwait(false);

            // 2) ---------- Compose ShirtGraphic XML referencing image asset --------
            var graphicBaseUrl = !string.IsNullOrWhiteSpace(publicAssetBaseUrl) ? publicAssetBaseUrl : baseUrl;
            if (string.IsNullOrWhiteSpace(graphicBaseUrl))
                graphicBaseUrl = "http://localhost";

            var xml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<roblox xmlns:xmime=""http://www.w3.org/2005/05/xmlmime"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:noNamespaceSchemaLocation=""http://www.roblox.com/roblox.xsd"" version=""4"">
    <External>null</External>
    <External>nil</External>
    <Item class=""ShirtGraphic"">
        <Properties>
            <Content name=""Graphic"">rbxassetid://{imageAssetId}</Content>
            <string name=""Name"">{System.Security.SecurityElement.Escape(name)}</string>
            <bool name=""archivable"">true</bool>
        </Properties>
    </Item>
</roblox>";

            // Compute hash for XML file for storage uniqueness
            string xmlHash;
            using (var sha = SHA256.Create())
            {
                var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(xml));
                var sb = new StringBuilder(hashBytes.Length * 2);
                foreach (var b in hashBytes)
                    sb.Append(b.ToString("x2"));
                xmlHash = sb.ToString();
            }

            var xmlExtension = ".rbxmx";
            var xmlFileName = xmlHash + xmlExtension;
            var xmlFullPath = Path.Combine(assetFolder, xmlFileName);
            File.WriteAllText(xmlFullPath, xml, new UTF8Encoding(false));

            // 3) ---------- Generate thumbnail using template -------------------
            string? thumbnailUrl = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(thumbnailsRoot) &&
                    !string.IsNullOrWhiteSpace(thumbnailBaseUrl) &&
                    !string.IsNullOrWhiteSpace(tshirtTemplatePath) &&
                    File.Exists(tshirtTemplatePath))
                {
                    Directory.CreateDirectory(thumbnailsRoot);

                    var thumbFileName = xmlHash + "_tshirt.png";
                    var thumbPath = Path.Combine(thumbnailsRoot, thumbFileName);

                    using (var template = new Bitmap(tshirtTemplatePath))
                    using (var userImageStream = new MemoryStream(fileBytes))
                    using (var userImage = new Bitmap(userImageStream))
                    using (var composed = new Bitmap(template.Width, template.Height))
                    using (var g = Graphics.FromImage(composed))
                    {
                        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                        g.DrawImage(template, 0, 0, template.Width, template.Height);

                        var targetWidth = (int)(template.Width * 0.5);
                        var targetHeight = (int)(template.Height * 0.5);
                        var targetX = (template.Width - targetWidth) / 2;
                        var targetY = (template.Height - targetHeight) / 2;

                        g.DrawImage(userImage, new Rectangle(targetX, targetY, targetWidth, targetHeight));

                        composed.Save(thumbPath, ImageFormat.Png);
                    }

                    var thumbBase = thumbnailBaseUrl?.TrimEnd('/', '\\') ?? string.Empty;
                    var relPath = ("thumbnails/" + thumbFileName).TrimStart('/', '\\');
                    thumbnailUrl = string.IsNullOrEmpty(thumbBase) ? null : string.Concat(thumbBase, "/", relPath);
                }
            }
            catch
            {
                // Swallow thumbnail composition failures; asset upload should still succeed.
                thumbnailUrl = null;
            }

            var tshirtCreateParams = new AssetCreateParams
            {
                Name = name,
                AssetTypeId = 2, // T-Shirt
                OwnerUserId = ownerUserId,
                ContentHash = xmlHash,
                FileExtension = xmlExtension,
                ContentType = "application/xml",
                ThumbnailUrl = thumbnailUrl
            };

            var tshirtAssetId = await _repository.CreateAssetAsync(connectionString, tshirtCreateParams, cancellationToken)
                .ConfigureAwait(false);
            await _userAssetsRepository.AddUserAssetAsync(connectionString, ownerUserId, tshirtAssetId, cancellationToken)
                .ConfigureAwait(false);

            return tshirtAssetId;
        }
    }
}
