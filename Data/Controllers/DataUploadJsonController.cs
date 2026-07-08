using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Assets;
using Games;
using Microsoft.AspNetCore.Mvc;

namespace Data.Controllers;

[ApiController]
[Route("data/upload/json")]
public class DataUploadJsonController : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null
    };

    [HttpPost]
    public async Task<IActionResult> Upload(
        [FromQuery] int assetTypeId,
        [FromQuery] string name,
        [FromQuery] string? description = null,
        [FromQuery] int? groupId = null,
        [FromServices] IConfiguration configuration = null!,
        [FromServices] TokenService tokenService = null!,
        [FromServices] AssetService assetService = null!)
    {
        var token = Request.Cookies[".ROBLOSECURITY"];
        if (string.IsNullOrWhiteSpace(token))
            return Unauthorized("Authentication required.");

        var userId = await tokenService.ValidateSessionAsync(token);
        if (userId == null || userId.Value <= 0)
            return Unauthorized("Invalid session.");

        var connStr = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr))
            return new JsonResult(new { Success = false, Message = "Database connection string not configured." }, JsonOptions);

        if (string.IsNullOrWhiteSpace(name))
            return new JsonResult(new { Success = false, Message = "Name is required." }, JsonOptions);

        if (assetTypeId == 0)
            return new JsonResult(new { Success = false, Message = "Invalid asset type." }, JsonOptions);

        var assetsDirectory = configuration["Assets:Directory"];
        if (string.IsNullOrWhiteSpace(assetsDirectory))
            return new JsonResult(new { Success = false, Message = "Assets directory not configured." }, JsonOptions);

        byte[] fileBytes;
        await using (var memory = new MemoryStream())
        {
            await Request.Body.CopyToAsync(memory);
            fileBytes = memory.ToArray();
        }

        if (fileBytes.Length == 0)
            return new JsonResult(new { Success = false, Message = "No file data provided." }, JsonOptions);

        try
        {
            var ownerUserId = userId.Value;
            
            if (assetTypeId == 13)
            {
                var thumbnailUrl = configuration["Thumbnails:ThumbnailUrl"];
                var thumbnailsRoot = configuration["Thumbnails:OutputDirectory"];
                var publicBaseUrl = configuration["Website:PublicBaseUrl"];

                var decalService = new Assets.DecalAssetService();
                var decalAssetId = await decalService.CreateDecalAsync(
                    connStr,
                    ownerUserId,
                    name,
                    null!,
                    "image/png",
                    fileBytes,
                    assetsDirectory,
                    thumbnailsRoot ?? Path.Combine(assetsDirectory, "thumbnails"),
                    thumbnailUrl ?? "http://localhost/thumbnails",
                    publicBaseUrl,
                    CancellationToken.None
                ).ConfigureAwait(false);

                return new JsonResult(new { Success = true, BackingAssetId = decalAssetId }, JsonOptions);
            }

            string contentHash;
            using (var sha = SHA256.Create())
            {
                var hashBytes = sha.ComputeHash(fileBytes);
                var sb = new StringBuilder(hashBytes.Length * 2);
                foreach (var b in hashBytes)
                    sb.Append(b.ToString("x2"));
                contentHash = sb.ToString();
            }

            var assetFolder = Path.Combine(assetsDirectory, "asset");
            Directory.CreateDirectory(assetFolder);

            var ext = assetTypeId;
            var fileName = contentHash + ext;
            var fullPath = Path.Combine(assetFolder, fileName);

            await System.IO.File.WriteAllBytesAsync(fullPath, fileBytes);

            var createParams = new AssetCreateParams
            {
                Name = name,
                AssetTypeId = assetTypeId,
                OwnerUserId = ownerUserId,
                ContentHash = contentHash,
                FileExtension = ext.ToString(),
                ContentType = assetTypeId.ToString(),
                Description = description,
                IsCopyingAllowed = true
            };

            var repo = new AssetsRepository();
            var assetId = await repo.CreateAssetAsync(connStr, createParams);

            var userAssetsRepo = new UserAssetsRepository();
            await userAssetsRepo.AddUserAssetAsync(connStr, ownerUserId, assetId);

            _ = Task.Run(async () => await assetService.RenderAssetThumbnailAsync(assetId).ConfigureAwait(false));

            return new JsonResult(new { Success = true, BackingAssetId = assetId }, JsonOptions);
        }
        catch (Exception ex)
        {
            return new JsonResult(new { Success = false, Message = ex.Message }, JsonOptions);
        }
    }
}
