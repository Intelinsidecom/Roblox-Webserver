using System;
using System.Threading;
using System.Threading.Tasks;
using Assets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace RobloxWebserver.Controllers
{
    [Route("plugins")]
    public class PluginInstallController : Controller
    {
        private readonly IConfiguration _configuration;

        public PluginInstallController(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        [HttpPost("{assetId}/install")]
        [Authorize]
        public async Task<IActionResult> Install(long assetId, CancellationToken cancellationToken = default)
        {
            if (assetId <= 0)
                return BadRequest(new { error = "Invalid assetId" });

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !long.TryParse(userIdClaim, out var userId) || userId <= 0)
                return Unauthorized();

            var connectionString = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connectionString))
                return StatusCode(500, new { error = "Database not configured" });

            try
            {
                var metadataRepo = new AssetMetadataRepository();
                var asset = await metadataRepo.GetAssetByIdAsync(connectionString, assetId, cancellationToken).ConfigureAwait(false);
                if (asset == null)
                    return NotFound(new { error = "Plugin not found" });

                if (asset.AssetTypeId != 38)
                    return BadRequest(new { error = "Asset is not a plugin" });

                var repo = new UserAssetsRepository();
                var owns = await repo.UserOwnsAssetAsync(connectionString, userId, assetId, cancellationToken).ConfigureAwait(false);
                if (!owns)
                {
                    var isFree = !asset.OnSale || (asset.Price <= 0 && asset.PriceTickets <= 0);
                    if (!isFree)
                        return StatusCode(402, new { error = "Plugin requires purchase" });

                    await repo.AddUserAssetAsync(connectionString, userId, assetId, cancellationToken).ConfigureAwait(false);
                }

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Install plugin assetId={assetId}: {ex}");
                return StatusCode(500, new { error = "Failed to process install request" });
            }
        }
    }
}
