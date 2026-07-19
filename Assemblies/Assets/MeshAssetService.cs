using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.Configuration;

namespace Assets
{
    public sealed class MeshAssetService
    {
        private readonly AssetsRepository _repository = new AssetsRepository();
        private readonly UserAssetsRepository _userAssetsRepository = new UserAssetsRepository();
        private readonly AssetService _assetService;

        public MeshAssetService(IConfiguration? configuration = null)
        {
            _assetService = new AssetService(configuration ?? new ConfigurationBuilder().Build(), null);
        }

        public async Task<long> CreateMeshAsync(
            string connectionString,
            long ownerUserId,
            string name,
            byte[] fileBytes,
            string cdnAssetsRoot,
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

            var (isValid, errorMessage) = AssetValidationHelper.ValidateMeshContent(fileBytes, bypassLimits);
            if (!isValid)
                throw new ArgumentException(errorMessage ?? "Mesh validation failed.");

            string contentHash;
            using (var sha = SHA256.Create())
            {
                var hashBytes = sha.ComputeHash(fileBytes);
                var sb = new StringBuilder(hashBytes.Length * 2);
                foreach (var b in hashBytes)
                    sb.Append(b.ToString("x2"));
                contentHash = sb.ToString();
            }

            var assetFolder = Path.Combine(cdnAssetsRoot, "asset");
            Directory.CreateDirectory(assetFolder);

            var fileExtension = ".mesh";
            var fileName = contentHash + fileExtension;
            var fullPath = Path.Combine(assetFolder, fileName);

            await Task.Run(() => File.WriteAllBytes(fullPath, fileBytes), cancellationToken)
                .ConfigureAwait(false);

            var createParams = new AssetCreateParams
            {
                Name = name,
                AssetTypeId = 4,
                OwnerUserId = ownerUserId,
                ContentHash = contentHash,
                FileExtension = fileExtension,
                ContentType = "application/octet-stream",
                ThumbnailUrl = null,
                HighResThumbnailUrl = null,
                Description = "Mesh",
                IsCopyingAllowed = true
            };

            var meshAssetId = await _repository.CreateAssetAsync(connectionString, createParams, cancellationToken)
                .ConfigureAwait(false);

            await _userAssetsRepository.AddUserAssetAsync(connectionString, ownerUserId, meshAssetId, cancellationToken)
                .ConfigureAwait(false);

            try
            {
                _ = Task.Run(async () => await _assetService.RenderAssetThumbnailAsync(meshAssetId, cancellationToken).ConfigureAwait(false));

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MeshAssetService] Failed to contact Arbiter for asset {meshAssetId}: {ex.Message}");
            }

            return meshAssetId;
        }
    }
}
