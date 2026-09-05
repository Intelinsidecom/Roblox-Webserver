using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Linq;
using System.Net.Http;
using System.IO;
using Avatar;

namespace Website.Controllers;

[ApiController]
[Route("v1/outfits")]
public class OutfitsV1Controller : ControllerBase
{
    private readonly IConfiguration _configuration;

    public OutfitsV1Controller(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private string? GetConnStr()
    {
        return _configuration.GetConnectionString("Default");
    }

    private long? GetAuthenticatedUserId()
    {
        var idStr = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(idStr) || !long.TryParse(idStr, out var userId) || userId <= 0)
            return null;
        return userId;
    }

    [HttpGet("/v1/users/{userId:long}/outfits")]
    public async Task<IActionResult> GetOutfits(long userId, [FromQuery] int itemsPerPage = 50, [FromQuery] int page = 1, CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
            return BadRequest(new { error = "Invalid userId" });

        var connStr = GetConnStr();
        if (string.IsNullOrWhiteSpace(connStr))
            return Problem("Database not configured");

        var repo = new OutfitsRepository();
        var (items, total) = await repo.GetOutfitsAsync(connStr, userId, itemsPerPage, page, cancellationToken).ConfigureAwait(false);

        var data = items.Select(o => new
        {
            id = o.Id,
            name = o.Name,
            thumbnail = o.ThumbnailUrl,
            created = o.CreatedAt.ToString("o")
        }).ToArray();

        return Ok(new
        {
            data,
            total,
            filteredCount = data.Length
        });
    }

    [HttpGet("{outfitId:long}/details")]
    public async Task<IActionResult> GetOutfitDetails(long outfitId, CancellationToken cancellationToken = default)
    {
        if (outfitId <= 0)
            return BadRequest(new { error = "Invalid outfitId" });

        var connStr = GetConnStr();
        if (string.IsNullOrWhiteSpace(connStr))
            return Problem("Database not configured");

        var repo = new OutfitsRepository();
        var outfit = await repo.GetOutfitDetailsAsync(connStr, outfitId, cancellationToken).ConfigureAwait(false);

        if (outfit == null)
            return NotFound(new { error = "Outfit not found" });

        return Ok(new
        {
            id = outfit.Id,
            name = outfit.Name,
            bodyColors = outfit.BodyColors == null ? null : new
            {
                headColorId = outfit.BodyColors.HeadColorId,
                torsoColorId = outfit.BodyColors.TorsoColorId,
                rightArmColorId = outfit.BodyColors.RightArmColorId,
                leftArmColorId = outfit.BodyColors.LeftArmColorId,
                rightLegColorId = outfit.BodyColors.RightLegColorId,
                leftLegColorId = outfit.BodyColors.LeftLegColorId
            },
            assetIds = outfit.AssetIds,
            created = outfit.CreatedAt.ToString("o"),
            updated = outfit.UpdatedAt.ToString("o")
        });
    }

    [Authorize]
    [HttpPost("create")]
    public async Task<IActionResult> CreateOutfit([FromBody] CreateOutfitModel model, CancellationToken cancellationToken = default)
    {
        var currentUserId = GetAuthenticatedUserId();
        if (currentUserId == null)
            return Unauthorized(new { error = "Authentication required" });

        if (string.IsNullOrWhiteSpace(model.name))
            return BadRequest(new { error = "Name is required" });

        var connStr = GetConnStr();
        if (string.IsNullOrWhiteSpace(connStr))
            return Problem("Database not configured");

        var repo = new OutfitsRepository();

        var count = await repo.GetOutfitCountAsync(connStr, currentUserId.Value, cancellationToken).ConfigureAwait(false);
        if (count >= OutfitsRepository.MaxOutfitsPerUser)
        {
            return BadRequest(new
            {
                errors = new[] { new { code = 1, message = "You have reached the maximum number of outfits" } }
            });
        }

        var bodyColors = new OutfitBodyColors
        {
            HeadColorId = model.bodyColors?.headColorId ?? 1,
            TorsoColorId = model.bodyColors?.torsoColorId ?? 1,
            RightArmColorId = model.bodyColors?.rightArmColorId ?? 1,
            LeftArmColorId = model.bodyColors?.leftArmColorId ?? 1,
            RightLegColorId = model.bodyColors?.rightLegColorId ?? 1,
            LeftLegColorId = model.bodyColors?.leftLegColorId ?? 1
        };

        var assetIds = model.assetIds ?? Array.Empty<long>();
        string? outfitThumbnailUrl = null;
        try
        {
            outfitThumbnailUrl = await Thumbnails.ThumbnailQueries.GetUserThumbnailUrlAsync(connStr, currentUserId.Value, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
        }

        var outfit = await repo.CreateOutfitAsync(connStr, currentUserId.Value, model.name, bodyColors, assetIds, outfitThumbnailUrl, cancellationToken).ConfigureAwait(false);

        return Ok(new
        {
            id = outfit.Id,
            name = outfit.Name,
            created = outfit.CreatedAt.ToString("o")
        });
    }

    [Authorize]
    [HttpPost("{outfitId:long}/wear")]
    public async Task<IActionResult> WearOutfit(long outfitId, CancellationToken cancellationToken = default)
    {
        var currentUserId = GetAuthenticatedUserId();
        if (currentUserId == null)
            return Unauthorized(new { error = "Authentication required" });

        var connStr = GetConnStr();
        if (string.IsNullOrWhiteSpace(connStr))
            return Problem("Database not configured");

        var repo = new OutfitsRepository();
        var outfit = await repo.GetOutfitDetailsAsync(connStr, outfitId, cancellationToken).ConfigureAwait(false);

        if (outfit == null)
            return NotFound(new { error = "Outfit not found" });

        var invalidAssetIds = Array.Empty<long>();

        if (outfit.AssetIds.Length > 0)
        {
            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string ownedSql = @"select asset_id from assets where asset_id = any(@ids) and owner_user_id = @uid";
            using var ownedCmd = new NpgsqlCommand(ownedSql, conn);
            ownedCmd.Parameters.AddWithValue("ids", outfit.AssetIds);
            ownedCmd.Parameters.AddWithValue("uid", currentUserId.Value);

            var ownedIds = new System.Collections.Generic.List<long>();
            await using var reader = await ownedCmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                ownedIds.Add(reader.GetInt64(0));
            }

            invalidAssetIds = outfit.AssetIds.Where(id => !ownedIds.Contains(id)).ToArray();
        }

        if (outfit.BodyColors != null)
        {
            var bodyColorsRepo = new BodyColorsRepository();
            await bodyColorsRepo.SetBodyColorsAsync(connStr, currentUserId.Value, new BodyColorsRepository.BodyColors
            {
                HeadColorId = outfit.BodyColors.HeadColorId,
                TorsoColorId = outfit.BodyColors.TorsoColorId,
                RightArmColorId = outfit.BodyColors.RightArmColorId,
                LeftArmColorId = outfit.BodyColors.LeftArmColorId,
                RightLegColorId = outfit.BodyColors.RightLegColorId,
                LeftLegColorId = outfit.BodyColors.LeftLegColorId
            }, cancellationToken).ConfigureAwait(false);
        }

        var wornRepo = new AvatarWornAssetsRepository();
        var validAssetIds = outfit.AssetIds.Where(id => !invalidAssetIds.Contains(id)).ToArray();
        await wornRepo.SetWornAssetIdsAsync(connStr, currentUserId.Value, validAssetIds, cancellationToken).ConfigureAwait(false);
        await AvatarStateHasher.RecomputeAndStoreAvatarHashAsync(connStr, currentUserId.Value, cancellationToken).ConfigureAwait(false);

        return Ok(new
        {
            success = true,
            invalidAssetIds = invalidAssetIds.Length > 0 ? invalidAssetIds : null
        });
    }

    [Authorize]
    [HttpPost("{outfitId:long}/delete")]
    public async Task<IActionResult> DeleteOutfit(long outfitId, CancellationToken cancellationToken = default)
    {
        var currentUserId = GetAuthenticatedUserId();
        if (currentUserId == null)
            return Unauthorized(new { error = "Authentication required" });

        var connStr = GetConnStr();
        if (string.IsNullOrWhiteSpace(connStr))
            return Problem("Database not configured");

        var repo = new OutfitsRepository();
        var deleted = await repo.DeleteOutfitAsync(connStr, outfitId, currentUserId.Value, cancellationToken).ConfigureAwait(false);

        if (!deleted)
            return NotFound(new { error = "Outfit not found" });

        return Ok(new { success = true });
    }

    [Authorize]
    [HttpPatch("{outfitId:long}")]
    public async Task<IActionResult> PatchOutfit(long outfitId, [FromBody] PatchOutfitModel model, CancellationToken cancellationToken = default)
    {
        var currentUserId = GetAuthenticatedUserId();
        if (currentUserId == null)
            return Unauthorized(new { error = "Authentication required" });

        if (outfitId <= 0)
            return BadRequest(new { error = "Invalid outfitId" });

        var connStr = GetConnStr();
        if (string.IsNullOrWhiteSpace(connStr))
            return Problem("Database not configured");

        var repo = new OutfitsRepository();

        OutfitBodyColors? bodyColors = null;
        if (model.bodyColors != null)
        {
            bodyColors = new OutfitBodyColors
            {
                HeadColorId = model.bodyColors.headColorId,
                TorsoColorId = model.bodyColors.torsoColorId,
                RightArmColorId = model.bodyColors.rightArmColorId,
                LeftArmColorId = model.bodyColors.leftArmColorId,
                RightLegColorId = model.bodyColors.rightLegColorId,
                LeftLegColorId = model.bodyColors.leftLegColorId
            };
        }

        var updated = await repo.PatchOutfitAsync(connStr, outfitId, currentUserId.Value, model.name, bodyColors, model.assetIds, null, cancellationToken).ConfigureAwait(false);

        if (!updated)
            return NotFound(new { error = "Outfit not found" });

        return Ok(new { success = true });
    }

    [AllowAnonymous]
    [HttpGet("/outfits/download")]
    public async Task<IActionResult> DownloadOutfit([FromQuery] long userOutfitId, CancellationToken cancellationToken = default)
    {
        if (userOutfitId <= 0)
            return BadRequest("Invalid userOutfitId");

        var connStr = GetConnStr();
        if (string.IsNullOrWhiteSpace(connStr))
            return Problem("Database not configured");

        var repo = new OutfitsRepository();
        var outfit = await repo.GetOutfitDetailsAsync(connStr, userOutfitId, cancellationToken).ConfigureAwait(false);
        if (outfit == null)
            return NotFound("Outfit not found");

        var thumbnailUrl = outfit.ThumbnailUrl;
        if (string.IsNullOrWhiteSpace(thumbnailUrl))
        {
            thumbnailUrl = await Thumbnails.ThumbnailQueries.GetUserThumbnailUrlAsync(connStr, outfit.UserId, cancellationToken).ConfigureAwait(false);
        }

        if (string.IsNullOrWhiteSpace(thumbnailUrl))
            return NotFound("No thumbnail available");

        if (!thumbnailUrl.StartsWith("http://") && !thumbnailUrl.StartsWith("https://"))
            thumbnailUrl = "https://" + thumbnailUrl;

        try
        {
            using var http = new HttpClient { Timeout = Common.HttpClientDefaults.Timeout };
            var imageBytes = await http.GetByteArrayAsync(thumbnailUrl).ConfigureAwait(false);
            return File(imageBytes, "image/png", $"Outfit-{userOutfitId}.png");
        }
        catch
        {
            return NotFound("Failed to fetch thumbnail");
        }
    }

    public sealed class CreateOutfitModel
    {
        public string name { get; set; } = string.Empty;
        public BodyColorsModel? bodyColors { get; set; }
        public long[]? assetIds { get; set; }
    }

    public sealed class PatchOutfitModel
    {
        public string? name { get; set; }
        public BodyColorsModel? bodyColors { get; set; }
        public long[]? assetIds { get; set; }
    }

    public sealed class BodyColorsModel
    {
        public int headColorId { get; set; }
        public int torsoColorId { get; set; }
        public int rightArmColorId { get; set; }
        public int leftArmColorId { get; set; }
        public int rightLegColorId { get; set; }
        public int leftLegColorId { get; set; }
    }
}
