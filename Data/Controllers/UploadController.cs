using Assets;
using Common;
using Games;
using Microsoft.AspNetCore.Mvc;
using Thumbnails;

namespace Data.Controllers;

[ApiController]
[Route("Data/[controller].ashx")]
public class UploadController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Upload(
        [FromQuery] long assetid,
        [FromServices] IConfiguration configuration,
        [FromServices] TokenService tokenService,
        [FromServices] AssetService assetService,
        [FromQuery] string type = "Place",
        CancellationToken cancellationToken = default)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, HttpContext.RequestAborted);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(90));
        var ct = timeoutCts.Token;

        try
        {
            var token = Request.Cookies[".ROBLOSECURITY"];
            if (string.IsNullOrWhiteSpace(token))
                return Unauthorized("Authentication required.");

            var userId = await tokenService.ValidateSessionAsync(token);
            if (userId == null || userId.Value <= 0)
                return Unauthorized("Invalid session.");

            var connStr = configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
                return Problem("Database connection string not configured.");

            var ownerId = await GamesRepository.GetAssetOwnerAsync(connStr, assetid, ct);
            if (ownerId == null)
                return NotFound($"Asset {assetid} not found.");

            if (ownerId.Value != userId.Value)
                return Forbid("You do not own this asset.");

            var assetsDirectory = configuration["Assets:Directory"];
            if (string.IsNullOrWhiteSpace(assetsDirectory))
                return Problem("Assets directory not configured.");

            byte[] fileBytes;
            var contentEncoding = Request.Headers["Content-Encoding"].FirstOrDefault()?.ToLowerInvariant();
            if (contentEncoding == "gzip")
            {
                await using var compressed = new MemoryStream();
                await Request.Body.CopyToAsync(compressed, ct);
                compressed.Seek(0, SeekOrigin.Begin);
                await using var decompressed = new MemoryStream();
                await using var gzipStream = new System.IO.Compression.GZipStream(compressed, System.IO.Compression.CompressionMode.Decompress);
                await gzipStream.CopyToAsync(decompressed, ct);
                fileBytes = decompressed.ToArray();
            }
            else
            {
                await using (var memory = new MemoryStream())
                {
                    await Request.Body.CopyToAsync(memory, ct);
                    fileBytes = memory.ToArray();
                }
            }

            if (string.Equals(type, "Model", StringComparison.OrdinalIgnoreCase))
            {
                var (isValid, validationError) = AssetValidationHelper.ValidateModelContent(fileBytes);
                if (!isValid)
                    return BadRequest(validationError ?? "Invalid model file.");

                var newHash = HashingUtilities.GenerateFileHash(fileBytes);

                var assetFolder = Path.Combine(assetsDirectory, "asset");
                Directory.CreateDirectory(assetFolder);
                var fileName = newHash + ".rbxm";
                var filePath = Path.Combine(assetFolder, fileName);

                try
                {
                    await System.IO.File.WriteAllBytesAsync(filePath, fileBytes, ct);
                }
                catch (Exception ex)
                {
                    return Problem($"Failed to save file: {ex.Message}");
                }

                var repo = new AssetsRepository();
                await repo.UpdateAssetModelContentAsync(connStr, assetid, newHash, ct);

                return Content(assetid.ToString(), "text/plain");
            }

            if (string.Equals(type, "Plugin", StringComparison.OrdinalIgnoreCase))
            {
                var (isValid, validationError) = AssetValidationHelper.ValidatePluginContent(fileBytes);
                if (!isValid)
                    return BadRequest(validationError ?? "Invalid plugin file.");

                var newHash = HashingUtilities.GenerateFileHash(fileBytes);

                var assetFolder = Path.Combine(assetsDirectory, "asset");
                Directory.CreateDirectory(assetFolder);
                var fileName = newHash + ".rbxm";
                var filePath = Path.Combine(assetFolder, fileName);

                try
                {
                    await System.IO.File.WriteAllBytesAsync(filePath, fileBytes, ct);
                }
                catch (Exception ex)
                {
                    return Problem($"Failed to save file: {ex.Message}");
                }

                var repo = new AssetsRepository();
                await repo.UpdateAssetModelContentAsync(connStr, assetid, newHash, ct);

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await assetService.RenderAssetThumbnailAsync(assetid).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ThumbnailGen ERROR] Plugin {assetid}: {ex.Message}");
                    }
                });

                return Content(assetid.ToString(), "text/plain");
            }

            var (success, error) = await GamesRepository.ReplacePlaceAssetAsync(
                connStr, assetsDirectory, assetid, fileBytes, ct, assetService: assetService);

            if (!success)
                return NotFound(error ?? $"Asset {assetid} not found or is not a place.");

            _ = Task.Run(async () =>
            {
                try
                {
                    var thumbnailService = new ThumbnailService(configuration);
                    var baseUrl = configuration["Thumbnails:ThumbnailUrl"];
                    if (string.IsNullOrWhiteSpace(baseUrl)) return;

                    var metaRepo = new AssetMetadataRepository();
                    var assetRecord = await metaRepo.GetAssetByIdAsync(connStr, assetid);
                    if (assetRecord == null) return;
                    var contentHash = assetRecord.ContentHash;
                    if (string.IsNullOrWhiteSpace(contentHash)) return;

                    await PlaceThumbnail.GeneratePlaceThumbnailAsync(
                        thumbnailService, connStr, assetid, contentHash, baseUrl,
                        placeName: assetRecord.Name, cancellationToken: CancellationToken.None);

                    await PlaceThumbnail.GenerateAutoGeneratedThumbnailAsync(
                        thumbnailService, connStr, assetid, contentHash, baseUrl,
                        CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ThumbnailGen ERROR] Place {assetid}: {ex.GetType().Name}: {ex.Message}");
                }
            });

            return Content(assetid.ToString(), "text/plain");
        }
        catch (OperationCanceledException) when (!HttpContext.RequestAborted.IsCancellationRequested)
        {
            return Problem("Upload timed out after 90 seconds.");
        }
        catch (Exception ex)
        {
            return Problem($"Upload failed: {ex.Message}");
        }
    }
}
