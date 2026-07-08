using Assets;
using Games;
using Microsoft.AspNetCore.Mvc;

namespace Data.Controllers;

[ApiController]
[Route("ide/publish")]
public class IDEPublishController : ControllerBase
{
    [HttpPost("UploadNewMesh")]
    public async Task<IActionResult> UploadNewMesh(
        [FromQuery] string name,
        [FromQuery] int? genreTypeId = null,
        [FromQuery] string? description = null,
        [FromQuery] bool? allowComments = null,
        [FromServices] IConfiguration configuration = null!,
        [FromServices] TokenService tokenService = null!)
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

        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("Name is required.");

        if (genreTypeId.HasValue && genreTypeId.Value == 0)
            return BadRequest("Invalid genre type id.");

        byte[] fileBytes;
        await using (var memory = new MemoryStream())
        {
            await Request.Body.CopyToAsync(memory);
            fileBytes = memory.ToArray();
        }

        if (fileBytes.Length == 0)
            return BadRequest("No file data provided.");

        var assetsDirectory = configuration["Assets:Directory"];
        if (string.IsNullOrWhiteSpace(assetsDirectory))
            return Problem("Assets directory not configured.");

        try
        {
            var service = new MeshAssetService(configuration);
            var assetId = await service.CreateMeshAsync(
                connStr,
                userId.Value,
                name,
                fileBytes,
                assetsDirectory);

            if (genreTypeId.HasValue)
            {
                var repo = new AssetsRepository();
                await repo.UpdateAssetGenreAsync(connStr, assetId, genreTypeId.Value);
            }

            if (allowComments.HasValue)
            {
                var repo = new AssetsRepository();
                await repo.UpdateAssetAllowCommentsAsync(connStr, assetId, allowComments.Value);
            }

            return Content(assetId.ToString(), "text/plain");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return Problem($"Mesh upload failed: {ex.Message}");
        }
    }
}
