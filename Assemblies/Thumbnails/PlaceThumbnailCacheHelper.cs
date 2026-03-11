using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Npgsql;

namespace Thumbnails
{
    /// <summary>
    /// Helper class for managing place thumbnail caching
    /// </summary>
    public static class PlaceThumbnailCacheHelper
    {
        private static readonly PlaceThumbnailCacheRepository _cacheRepository = new();

        /// <summary>
        /// Gets or generates a place thumbnail with caching support
        /// </summary>
        /// <param name="thumbnailService">Thumbnail service instance</param>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="placeId">Place ID</param>
        /// <param name="placeAssetHash">Hash of the place asset content</param>
        /// <param name="baseUrl">Base URL for CDN</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Tuple containing (iconUrl, iconHash, thumbnailHash)</returns>
        public static async Task<(string iconUrl, string iconHash, string thumbnailHash)> GetOrGeneratePlaceThumbnailAsync(
            IThumbnailService thumbnailService,
            string connectionString,
            long placeId,
            string placeAssetHash,
            string baseUrl,
            CancellationToken cancellationToken = default)
        {
            if (thumbnailService == null)
                throw new ArgumentNullException(nameof(thumbnailService));
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string is required", nameof(connectionString));
            if (placeId <= 0)
                throw new ArgumentOutOfRangeException(nameof(placeId));
            if (string.IsNullOrWhiteSpace(placeAssetHash))
                throw new ArgumentException("Place asset hash is required", nameof(placeAssetHash));
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentException("Base URL is required", nameof(baseUrl));

            // First, try to get cached thumbnails for this place asset hash (defaulting to 256x256 for icons)
            var (found, cachedIconHash, cachedThumbnailHash) = await _cacheRepository.TryGetAsync(
                connectionString, placeAssetHash, 256, 256, cancellationToken);

            if (found && !string.IsNullOrWhiteSpace(cachedIconHash) && !string.IsNullOrWhiteSpace(cachedThumbnailHash))
            {
                // Found cached entry - build URLs from cached hashes
                var iconUrl = CDNUtilities.GenerateThumbnailUrl(baseUrl, $"{cachedIconHash}.png");
                var thumbnailUrl = CDNUtilities.GenerateThumbnailUrl(baseUrl, $"{cachedThumbnailHash}.png");
                
                return (iconUrl, cachedIconHash, cachedThumbnailHash);
            }

            // Cache miss - need to generate new thumbnails

            try
            {
                // Render new thumbnails with caching
                var renderedResult = await thumbnailService.RenderPlaceAsync(placeId, x: 256, y: 256, connectionString: connectionString, placeAssetHash: placeAssetHash, cancellationToken: cancellationToken);
                var renderedResultHighRes = await thumbnailService.RenderPlaceAsync(placeId, x: 1024, y: 1024, connectionString: connectionString, placeAssetHash: placeAssetHash, cancellationToken: cancellationToken);
                
                // Copy to CDN as before
                var cdnThumbnailsPath = CDNUtilities.GetCDNThumbnailsPath();
                
                var sourcePath = Path.Combine(renderedResult.FullPath);
                var sourcePathHighRes = Path.Combine(renderedResultHighRes.FullPath);
                var cdnThumbnailPath = Path.Combine(cdnThumbnailsPath, renderedResult.FileName);
                var cdnThumbnailPathHighRes = Path.Combine(cdnThumbnailsPath, renderedResultHighRes.FileName);
                
                bool thumbnailCopied = CDNUtilities.SafeFileCopy(sourcePath, cdnThumbnailPath);
                bool highResThumbnailCopied = CDNUtilities.SafeFileCopy(sourcePathHighRes, cdnThumbnailPathHighRes);
                
                if (!thumbnailCopied && !highResThumbnailCopied)
                {
                    throw new InvalidOperationException("Failed to copy any thumbnail files to CDN location");
                }

                // Generate URLs
                var iconUrl = thumbnailCopied ? CDNUtilities.GenerateThumbnailUrl(baseUrl, renderedResult.FileName) : null;
                var iconUrlHighRes = highResThumbnailCopied ? CDNUtilities.GenerateThumbnailUrl(baseUrl, renderedResultHighRes.FileName) : null;

                if (string.IsNullOrWhiteSpace(iconUrl))
                {
                    throw new InvalidOperationException("Failed to generate icon URL");
                }

                // Cache the 256x256 result as icon, and 1024x1024 as thumbnail
                await _cacheRepository.UpsertAsync(
                    connectionString, 
                    placeAssetHash, 
                    renderedResult.Hash, 
                    renderedResultHighRes.Hash,
                    256, 
                    256, 
                    cancellationToken);

                // Also cache the high resolution version separately
                await _cacheRepository.UpsertAsync(
                    connectionString, 
                    placeAssetHash, 
                    renderedResultHighRes.Hash, 
                    renderedResultHighRes.Hash,
                    1024, 
                    1024, 
                    cancellationToken);

                
                return (iconUrl, renderedResult.Hash, renderedResultHighRes.Hash);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Invalidates cache for a specific place asset hash
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="placeAssetHash">Hash of the place asset</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task InvalidateCacheAsync(
            string connectionString,
            string placeAssetHash,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string is required", nameof(connectionString));
            if (string.IsNullOrWhiteSpace(placeAssetHash))
                throw new ArgumentException("Place asset hash is required", nameof(placeAssetHash));

            await _cacheRepository.DeleteAsync(connectionString, placeAssetHash, cancellationToken);
        }

        /// <summary>
        /// Clears all place thumbnail cache entries
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task ClearAllCacheAsync(
            string connectionString,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string is required", nameof(connectionString));

            await _cacheRepository.ClearAllAsync(connectionString, cancellationToken);
        }
    }
}
