using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TagLib;

namespace Assets
{
    public sealed class AudioAssetService
    {
        private static readonly string[] AllowedExtensions = { ".mp3", ".ogg", ".wav" };
        private const long MaxFileSize = 8 * 1024 * 1024; // 8 MB
        private static readonly TimeSpan MaxDuration = TimeSpan.FromMinutes(2);

        private readonly AssetsRepository _repository = new AssetsRepository();
        private readonly UserAssetsRepository _userAssetsRepository = new UserAssetsRepository();

        public async Task<long> CreateAudioAsync(
            string connectionString,
            long ownerUserId,
            string name,
            byte[] fileBytes,
            string cdnAssetsRoot,
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

            ValidateAudioFile(fileBytes);

            string contentHash;
            using (var sha = SHA256.Create())
            {
                var hashBytes = sha.ComputeHash(fileBytes);
                var sb = new StringBuilder(hashBytes.Length * 2);
                foreach (var b in hashBytes)
                    sb.Append(b.ToString("x2"));
                contentHash = sb.ToString();
            }

            var extension = GetFileExtension(fileBytes);
            var assetFolder = Path.Combine(cdnAssetsRoot, "asset");
            Directory.CreateDirectory(assetFolder);

            var fileName = contentHash + extension;
            var fullPath = Path.Combine(assetFolder, fileName);

            await Task.Run(() => System.IO.File.WriteAllBytes(fullPath, fileBytes), cancellationToken)
                .ConfigureAwait(false);

            var createParams = new AssetCreateParams
            {
                Name = name,
                AssetTypeId = 3,
                OwnerUserId = ownerUserId,
                ContentHash = contentHash,
                FileExtension = extension,
                ContentType = GetContentType(extension),
                ThumbnailUrl = null,
                HighResThumbnailUrl = null,
                Description = "Audio"
            };

            var audioAssetId = await _repository.CreateAssetAsync(connectionString, createParams, cancellationToken)
                .ConfigureAwait(false);

            await _userAssetsRepository.AddUserAssetAsync(connectionString, ownerUserId, audioAssetId, cancellationToken)
                .ConfigureAwait(false);

            return audioAssetId;
        }

        private static void ValidateAudioFile(byte[] fileBytes)
        {
            if (fileBytes.Length > MaxFileSize)
                throw new ArgumentException($"Audio file exceeds maximum size of 8.0 MB (actual: {fileBytes.Length / (1024.0 * 1024.0):F1} MB).");

            var extension = GetFileExtension(fileBytes);

            var tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + extension);
            try
            {
                System.IO.File.WriteAllBytes(tempPath, fileBytes);

                using var file = TagLib.File.Create(tempPath);

                if (file.Properties.Duration > MaxDuration)
                    throw new ArgumentException($"Audio duration exceeds maximum of 2 minutes (actual: {file.Properties.Duration:mm\\:ss}).");
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Audio file validation failed: {ex.Message}");
            }
            finally
            {
                if (System.IO.File.Exists(tempPath))
                    System.IO.File.Delete(tempPath);
            }
        }

        private static string GetFileExtension(byte[] fileBytes)
        {
            if (fileBytes.Length < 4)
                throw new ArgumentException("File is too small to be a valid audio file.");

            // Check magic bytes
            if (fileBytes[0] == 0x49 && fileBytes[1] == 0x44 && fileBytes[2] == 0x33)
                return ".mp3";
            if (fileBytes[0] == 0xFF && (fileBytes[1] & 0xE0) == 0xE0)
                return ".mp3";
            if (fileBytes[0] == 0x4F && fileBytes[1] == 0x67 && fileBytes[2] == 0x67 && fileBytes[3] == 0x53)
                return ".ogg";
            if (fileBytes[0] == 0x52 && fileBytes[1] == 0x49 && fileBytes[2] == 0x46 && fileBytes[3] == 0x46)
                return ".wav";

            throw new ArgumentException("Unsupported audio format. Only MP3, OGG, and WAV files are allowed.");
        }

        private static string GetContentType(string extension)
        {
            return extension switch
            {
                ".mp3" => "audio/mpeg",
                ".ogg" => "audio/ogg",
                ".wav" => "audio/wav",
                _ => "application/octet-stream"
            };
        }
    }
}
