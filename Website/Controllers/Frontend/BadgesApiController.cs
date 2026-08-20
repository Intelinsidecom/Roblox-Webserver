using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Games;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Website.Controllers.Frontend;

[ApiController]
public class BadgesApiController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public BadgesApiController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet("badges/list-badges-for-place/json")]
    public async Task<IActionResult> ListBadgesForPlace([FromQuery] long placeId, CancellationToken cancellationToken = default)
    {
        if (placeId <= 0)
            return Ok(new { GameBadges = Array.Empty<object>() });

        try
        {
            var connStr = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
                return Ok(new { GameBadges = Array.Empty<object>() });

            var badges = await GamesRepository.GetBadgesForPlaceAsync(connStr, placeId, cancellationToken);

            if (badges.Count == 0)
                return Ok(new { GameBadges = Array.Empty<object>() });

            long visitCount = 0;
            var universeId = await GamesRepository.GetUniverseIdFromPlaceIdAsync(connStr, placeId, cancellationToken);
            if (universeId.HasValue)
            {
                var universe = await GamesRepository.GetUniverseAsync(connStr, universeId.Value, cancellationToken);
                if (universe != null)
                    visitCount = universe.VisitCount;
            }

            var badgeIds = new List<long>(badges.Count);
            foreach (var b in badges)
                badgeIds.Add(b.AssetId);

            var stats = await GamesRepository.GetBadgeStatsAsync(connStr, badgeIds, cancellationToken,
                subtractCreator: _configuration.GetValue<bool>("Badge:SubtractCreatorFromTotalWon"));

            var result = new object[badges.Count];
            for (int i = 0; i < badges.Count; i++)
            {
                var b = badges[i];
                long totalWon = 0;
                long wonYesterday = 0;
                if (stats.TryGetValue(b.AssetId, out var s))
                {
                    totalWon = s.TotalWon;
                    wonYesterday = s.WonYesterday;
                }

                var (rarityPercent, rarityName) = GamesRepository.GetRarity(totalWon, visitCount);

                result[i] = new
                {
                    BadgeAssetId = b.AssetId,
                    Name = b.Name,
                    Description = b.Description ?? "",
                    ImageUrl = b.ThumbnailUrl ?? "",
                    IsOwned = false,
                    Rarity = rarityPercent,
                    RarityName = rarityName,
                    TotalAwarded = totalWon,
                    TotalAwardedYesterday = wonYesterday,
                    Created = b.CreatedAt.ToString("o"),
                    Updated = b.CreatedAt.ToString("o"),
                    BadgeSeoUrl = "",
                    CreatorId = b.CreatorUserId,
                    IsImageUrlFinal = true,
                };
            }

            return Ok(new { GameBadges = result });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] ListBadgesForPlace: {ex.Message}");
            return Ok(new { GameBadges = Array.Empty<object>() });
        }
    }
}
