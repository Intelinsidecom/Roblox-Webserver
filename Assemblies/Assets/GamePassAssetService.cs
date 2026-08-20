using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Assets
{
    public sealed class GamePassAssetService
    {
        private const int PassSize = 512;
        private const int HighResThumbSize = 420;
        private const int LowThumbSize = 110;

        private readonly AssetsRepository _repository = new AssetsRepository();
        private readonly UserAssetsRepository _userAssetsRepository = new UserAssetsRepository();

        /// <summary>
        /// Creates a persisted game pass asset (asset_type_id = 34) owned by the
        /// specified user and linked to the universe whose root place matches
        /// targetPlaceId. The image is center-cropped, then circle-masked with
        /// transparent corners before being stored.
        /// </summary>
        public async Task<long> CreateGamePassAsync(
            string connectionString,
            long ownerUserId,
            string name,
            string description,
            byte[] imageBytes,
            long targetPlaceId,
            string cdnAssetsRoot,
            string thumbnailsRoot,
            string thumbnailBaseUrl,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (ownerUserId <= 0)
                throw new ArgumentOutOfRangeException(nameof(ownerUserId));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("name is required", nameof(name));
            if (imageBytes == null || imageBytes.Length == 0)
                throw new ArgumentException("imageBytes is required", nameof(imageBytes));
            if (targetPlaceId <= 0)
                throw new ArgumentException("targetPlaceId is required", nameof(targetPlaceId));
            if (string.IsNullOrWhiteSpace(cdnAssetsRoot))
                throw new ArgumentException("cdnAssetsRoot is required", nameof(cdnAssetsRoot));

            // 1) ---------- Resolve the owning universe for the target place ----------
            var belongsToUniverse = await ResolveUniverseIdAsync(connectionString, ownerUserId, targetPlaceId, cancellationToken)
                .ConfigureAwait(false);
            if (belongsToUniverse <= 0)
                throw new InvalidOperationException("You cannot manage this place");

            // 2) ---------- Process image: square crop + circle mask ----------
            var circularBytes = ApplyCircleMask(imageBytes, PassSize);

            // 3) ---------- Persist the PNG backing image ----------
            var pngHash = ComputeSha256Hex(circularBytes);

            var assetFolder = Path.Combine(cdnAssetsRoot, "asset");
            Directory.CreateDirectory(assetFolder);

            var pngExtension = ".png";
            var pngFullPath = Path.Combine(assetFolder, pngHash + pngExtension);

            using (var fs = new FileStream(pngFullPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            {
                await fs.WriteAsync(circularBytes, 0, circularBytes.Length, cancellationToken).ConfigureAwait(false);
            }

            // 4) ---------- Generate thumbnails from the circular image ----------
            string? lowUrl = null;
            string? highResUrl = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(thumbnailsRoot) && !string.IsNullOrWhiteSpace(thumbnailBaseUrl))
                {
                    Directory.CreateDirectory(thumbnailsRoot);

                    var highResFileName = pngHash + "_highres.png";
                    var lowFileName = pngHash + ".png";
                    var highResPath = Path.Combine(thumbnailsRoot, highResFileName);
                    var lowPath = Path.Combine(thumbnailsRoot, lowFileName);

                    var thumbBase = thumbnailBaseUrl.TrimEnd('/', '\\');
                    var lowRelPath = ("thumbnails/" + lowFileName).TrimStart('/', '\\');
                    var highResRelPath = ("thumbnails/" + highResFileName).TrimStart('/', '\\');

                    using (var ms = new MemoryStream(circularBytes))
                    using (var original = new Bitmap(ms))
                    {
                        using (var highBmp = new Bitmap(HighResThumbSize, HighResThumbSize, PixelFormat.Format32bppArgb))
                        using (var gHigh = Graphics.FromImage(highBmp))
                        {
                            gHigh.Clear(Color.Transparent);
                            gHigh.CompositingQuality = CompositingQuality.HighQuality;
                            gHigh.SmoothingMode = SmoothingMode.HighQuality;
                            gHigh.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            gHigh.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            gHigh.DrawImage(original, 0, 0, HighResThumbSize, HighResThumbSize);
                            highBmp.Save(highResPath, ImageFormat.Png);
                        }

                        using (var lowBmp = new Bitmap(LowThumbSize, LowThumbSize, PixelFormat.Format32bppArgb))
                        using (var gLow = Graphics.FromImage(lowBmp))
                        {
                            gLow.Clear(Color.Transparent);
                            gLow.CompositingQuality = CompositingQuality.HighQuality;
                            gLow.SmoothingMode = SmoothingMode.HighQuality;
                            gLow.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            gLow.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            gLow.DrawImage(original, 0, 0, LowThumbSize, LowThumbSize);
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

            // 5) ---------- Create the game pass asset record (type 34) ----------
            var createParams = new AssetCreateParams
            {
                Name = name,
                AssetTypeId = 34, // Game Pass
                OwnerUserId = ownerUserId,
                ContentHash = pngHash,
                FileExtension = pngExtension,
                ContentType = "image/png",
                ThumbnailUrl = lowUrl,
                HighResThumbnailUrl = highResUrl,
                Description = string.IsNullOrWhiteSpace(description) ? "Game Pass" : description,
                BelongsToUniverse = belongsToUniverse,
            };

            var assetId = await _repository.CreateAssetAsync(connectionString, createParams, cancellationToken)
                .ConfigureAwait(false);

            // Game passes are stored off-sale with price 1 by default.
            await _repository.UpdateAssetSaleAsync(connectionString, assetId, onSale: false, price: 1, cancellationToken)
                .ConfigureAwait(false);

            await _userAssetsRepository.AddUserAssetAsync(connectionString, ownerUserId, assetId, cancellationToken)
                .ConfigureAwait(false);

            return assetId;
        }

        /// <summary>
        /// Returns the universe_id whose root_place_id equals targetPlaceId and that
        /// is owned by the given user, or 0 if no such universe exists.
        /// </summary>
        private static async Task<long> ResolveUniverseIdAsync(
            string connectionString,
            long ownerUserId,
            long targetPlaceId,
            CancellationToken cancellationToken)
        {
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = @"select u.universe_id
from universes u
where u.root_place_id = @place_id
  and u.creator_user_id = @owner_user_id
limit 1;";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("place_id", targetPlaceId);
            cmd.Parameters.AddWithValue("owner_user_id", ownerUserId);

            var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            if (result == null || result == DBNull.Value)
                return 0;

            return Convert.ToInt64(result);
        }

        /// <summary>
        /// Masks an image into a circle: the source is center-cropped to a square,
        /// scaled to the given size, then everything outside the inscribed circle
        /// becomes transparent.
        /// </summary>
        private static byte[] ApplyCircleMask(byte[] imageBytes, int size)
        {
            using var input = new MemoryStream(imageBytes);
            using var source = new Bitmap(input);

            var srcRatio = (float)source.Width / source.Height;
            var targetRatio = 1f;

            int srcX, srcY, srcW, srcH;
            if (srcRatio > targetRatio)
            {
                srcH = source.Height;
                srcW = (int)(source.Height * targetRatio);
                srcX = (source.Width - srcW) / 2;
                srcY = 0;
            }
            else
            {
                srcW = source.Width;
                srcH = (int)(source.Width / targetRatio);
                srcX = 0;
                srcY = (source.Height - srcH) / 2;
            }

            var output = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(output))
            {
                g.Clear(Color.Transparent);
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                var srcRect = new Rectangle(srcX, srcY, srcW, srcH);
                var destRect = new Rectangle(0, 0, size, size);

                using (var path = new GraphicsPath())
                {
                    path.AddEllipse(0, 0, size, size);
                    g.SetClip(path);
                    g.DrawImage(source, destRect, srcRect, GraphicsUnit.Pixel);
                }
            }

            using var outputStream = new MemoryStream();
            output.Save(outputStream, ImageFormat.Png);
            return outputStream.ToArray();
        }

        private static string ComputeSha256Hex(byte[] bytes)
        {
            using var sha = SHA256.Create();
            var hashBytes = sha.ComputeHash(bytes);
            var sb = new StringBuilder(hashBytes.Length * 2);
            foreach (var b in hashBytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
