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

            string hash;
            using (var sha = SHA256.Create())
            {
                var hashBytes = sha.ComputeHash(fileBytes);
                var sb = new StringBuilder(hashBytes.Length * 2);
                foreach (var b in hashBytes)
                    sb.Append(b.ToString("x2"));
                hash = sb.ToString();
            }

            var assetFolder = Path.Combine(cdnAssetsRoot, "asset");
            Directory.CreateDirectory(assetFolder);

            var extension = Path.GetExtension(originalFileName);
            var fileName = string.IsNullOrWhiteSpace(extension) ? hash : hash + extension;
            var fullPath = Path.Combine(assetFolder, fileName);

            using (var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            {
                await fs.WriteAsync(fileBytes, 0, fileBytes.Length, cancellationToken).ConfigureAwait(false);
            }

            string? thumbnailUrl = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(thumbnailsRoot) &&
                    !string.IsNullOrWhiteSpace(thumbnailBaseUrl) &&
                    !string.IsNullOrWhiteSpace(tshirtTemplatePath) &&
                    File.Exists(tshirtTemplatePath))
                {
                    Directory.CreateDirectory(thumbnailsRoot);

                    var thumbFileName = hash + "_tshirt.png";
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

                    var baseUrl = thumbnailBaseUrl.TrimEnd('/', '\\');
                    var relPath = ("thumbnails/" + thumbFileName).TrimStart('/', '\\');
                    thumbnailUrl = string.Concat(baseUrl, "/", relPath);
                }
            }
            catch
            {
                // Swallow thumbnail composition failures; asset upload should still succeed.
                thumbnailUrl = null;
            }

            var createParams = new AssetCreateParams
            {
                Name = name,
                AssetTypeId = 2,
                OwnerUserId = ownerUserId,
                ContentHash = hash,
                FileExtension = extension,
                ContentType = contentType,
                ThumbnailUrl = thumbnailUrl
            };

            var assetId = await _repository.CreateAssetAsync(connectionString, createParams, cancellationToken)
                .ConfigureAwait(false);
            await _userAssetsRepository.AddUserAssetAsync(connectionString, ownerUserId, assetId, cancellationToken)
                .ConfigureAwait(false);

            return assetId;
        }
    }
}
