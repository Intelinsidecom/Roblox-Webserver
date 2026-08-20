using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Avatar;

namespace Api.Controllers
{
    [ApiController]
    [Route("appearance")]
    public class AppearanceController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly Api.Services.CurrentUserService _currentUserService;

        public AppearanceController(IConfiguration configuration, Api.Services.CurrentUserService currentUserService)
        {
            _configuration = configuration;
            _currentUserService = currentUserService;
        }

        private string GetConnStr() => _configuration.GetConnectionString("Default") ?? "";

        [HttpGet("get-my-user-outfits")]
        public async Task<IActionResult> GetMyUserOutfits()
        {
            long userId = await _currentUserService.GetUserIdAsync();
            if (userId <= 0)
                return StatusCode(403);

            try
            {
                var connStr = GetConnStr();
                var repo = new AvatarWornAssetsRepository();
                var assetIds = await repo.GetWornAssetIdsAsync(connStr, userId);

                var items = new object[assetIds.Length];
                for (int i = 0; i < assetIds.Length; i++)
                {
                    items[i] = new { assetId = assetIds[i] };
                }

                return Content(JsonSerializer.Serialize(items, new JsonSerializerOptions { PropertyNamingPolicy = null }), "application/json");
            }
            catch (Exception ex)
            {
                return Content("[]", "application/json");
            }
        }

        [HttpPost("wear-user-outfit")]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data", "application/json")]
        public async Task<IActionResult> WearUserOutfit()
        {
            long userId = await _currentUserService.GetUserIdAsync();
            if (userId <= 0)
                return StatusCode(403);

            try
            {
                long outfitId = 0;

                if (Request.HasFormContentType)
                {
                    long.TryParse(Request.Form["outfitId"].FirstOrDefault(), out outfitId);
                }
                else
                {
                    using var body = await JsonDocument.ParseAsync(Request.Body);
                    if (body.RootElement.TryGetProperty("outfitId", out var oid))
                        oid.TryGetInt64(out outfitId);
                }

                if (outfitId <= 0)
                    return BadRequest(new { success = false, error = "Invalid outfitId" });

                var connStr = GetConnStr();
                var outfitsRepo = new OutfitsRepository();
                var outfit = await outfitsRepo.GetOutfitDetailsAsync(connStr, outfitId);

                if (outfit == null)
                    return NotFound(new { success = false, error = "Outfit not found" });

                var wornRepo = new AvatarWornAssetsRepository();
                await wornRepo.SetWornAssetIdsAsync(connStr, userId, outfit.AssetIds);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpPost("set-clothing")]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data", "application/json")]
        public async Task<IActionResult> SetClothing()
        {
            long userId = await _currentUserService.GetUserIdAsync();
            if (userId <= 0)
                return StatusCode(403);

            try
            {
                long assetId = 0;

                if (Request.HasFormContentType)
                {
                    long.TryParse(Request.Form["assetId"].FirstOrDefault(), out assetId);
                }
                else
                {
                    using var body = await JsonDocument.ParseAsync(Request.Body);
                    if (body.RootElement.TryGetProperty("assetId", out var aid))
                        aid.TryGetInt64(out assetId);
                }

                if (assetId <= 0)
                    return BadRequest(new { success = false, error = "Invalid assetId" });

                var connStr = GetConnStr();
                var repo = new AvatarWornAssetsRepository();
                var result = await repo.WearAssetAsync(connStr, userId, assetId);

                // Persist the updated worn assets
                await repo.SetWornAssetIdsAsync(connStr, userId, result.AssetIds);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
    }
}
