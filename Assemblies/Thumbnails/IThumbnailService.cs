using System.Threading;
using System.Threading.Tasks;

namespace Thumbnails;

public interface IThumbnailService
{
    Task<ThumbnailSaveResult> SaveBase64PngAsync(string base64, string? overrideOutputDirectory = null, CancellationToken cancellationToken = default);
    Task<ThumbnailSaveResult> RenderAvatarAsync(string type, long userId, int? x = null, int? y = null, CancellationToken cancellationToken = default);
    Task<string> RenderAvatar3DBase64Async(long userId, int? x = null, int? y = null, CancellationToken cancellationToken = default);
    Task<Avatar3DCacheResult> RenderAvatar3DAndCacheAsync(long userId, int? x = null, int? y = null, bool force = false, CancellationToken cancellationToken = default);
    Task<ThumbnailSaveResult> RenderPlaceAsync(long placeId, int? x = null, int? y = null, CancellationToken cancellationToken = default);
    Task<ThumbnailSaveResult> RenderPlaceAsync(long placeId, int? x, int? y, string? connectionString, CancellationToken cancellationToken = default);
    Task<ThumbnailSaveResult> RenderPlaceAsync(long placeId, int? x, int? y, string? connectionString, string? placeAssetHash, CancellationToken cancellationToken = default);
    Task<bool> HasCustomIconAsync(long placeId, string connectionString, CancellationToken cancellationToken = default);

    Task<string> RenderAsset3DBase64Async(long assetId, int? x = null, int? y = null, CancellationToken cancellationToken = default);
    Task<Avatar3DCacheResult> RenderAsset3DAndCacheAsync(long assetId, int? x = null, int? y = null, bool force = false, CancellationToken cancellationToken = default);

    Task<string> RenderModel3DBase64Async(long assetId, int? x = null, int? y = null, CancellationToken cancellationToken = default);
    Task<Avatar3DCacheResult> RenderModel3DAndCacheAsync(long assetId, int? x = null, int? y = null, bool force = false, CancellationToken cancellationToken = default);
}
