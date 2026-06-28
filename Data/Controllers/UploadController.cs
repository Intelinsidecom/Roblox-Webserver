using Games;
using Microsoft.AspNetCore.Mvc;

namespace Data.Controllers;

[ApiController]
[Route("Data/[controller].ashx")]
public class UploadController : ControllerBase
{
    [HttpPut]
    [HttpPost]
    public async Task<IActionResult> Upload(
        [FromQuery] long assetid,
        [FromServices] IConfiguration configuration,
        [FromServices] TokenService tokenService)
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

        var ownerId = await GamesRepository.GetAssetOwnerAsync(connStr, assetid);
        if (ownerId == null)
            return NotFound($"Asset {assetid} not found.");

        if (ownerId.Value != userId.Value)
            return Forbid("You do not own this asset.");

        var assetsDirectory = configuration["Assets:Directory"];
        if (string.IsNullOrWhiteSpace(assetsDirectory))
            return Problem("Assets directory not configured.");

        byte[] fileBytes;
        await using (var memory = new MemoryStream())
        {
            await Request.Body.CopyToAsync(memory);
            fileBytes = memory.ToArray();
        }

        var (success, error) = await GamesRepository.ReplacePlaceAssetAsync(
            connStr, assetsDirectory, assetid, fileBytes);

        if (!success)
            return NotFound(error ?? $"Asset {assetid} not found or is not a place.");

        return Ok($"Place {assetid} uploaded successfully.");
    }
}
