using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Games;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Website.Controllers.Frontend;

[ApiController]
public class GamesApiController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public GamesApiController(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    private static string Serialize(object obj)
    {
        return JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
    }

    [Authorize]
    [HttpGet("v1/games/sorts")]
    public IActionResult GetSorts(
        [FromQuery(Name = "model.gameSortsContext")] string? gameSortsContext = null,
        CancellationToken cancellationToken = default)
    {
        var sorts = new object[]
        {
            new { token = "TopRated", name = "Top Rated", tokenExpiry = "" },
            new { token = "Popular", name = "Popular", tokenExpiry = "" },
            new { token = "RecentlyUpdated", name = "Recently Updated", tokenExpiry = "" },
            new { token = "TopEarning", name = "Top Earning", tokenExpiry = "" },
            new { token = "TopGrossing", name = "Top Grossing", tokenExpiry = "" },
            new { token = "Recommended", name = "Recommended", tokenExpiry = "" }
        };

        return Content(Serialize(new { sorts }), "application/json");
    }

    [Authorize]
    [HttpGet("v1/games/list")]
    public async Task<IActionResult> GetGameList(
        [FromQuery(Name = "sortToken")] string? sortToken = null,
        [FromQuery(Name = "maxRows")] int maxRows = 10,
        [FromQuery(Name = "startRow")] int startRow = 0,
        CancellationToken cancellationToken = default)
    {
        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr))
            return Content(Serialize(new { data = new object[] { } }), "application/json");

        List<long> universeIds;

        switch (sortToken)
        {
            case "TopRated":
                universeIds = await GamesQueries.GetTopRatedUniverseIdsAsync(
                    Math.Min(maxRows + startRow, 200), connStr, cancellationToken).ConfigureAwait(false);
                break;
            case "Popular":
            default:
                universeIds = await GamesQueries.GetPopularUniverseIdsAsync(
                    Math.Min(maxRows + startRow, 200), connStr, cancellationToken).ConfigureAwait(false);
                break;
        }

        universeIds = universeIds.Skip(startRow).Take(maxRows).ToList();

        var games = await GamesQueries.GetGameEntriesByUniverseIdsAsync(
            universeIds, connStr, cancellationToken).ConfigureAwait(false);

        var data = games.Select(g => new
        {
            id = g.UniverseId,
            creatorType = "User",
            creatorTargetId = g.CreatorUserId,
            creatorName = g.CreatorName,
            totalUpVotes = g.UpVotes,
            totalDownVotes = g.DownVotes,
            universeId = g.UniverseId,
            name = g.Name,
            placeId = g.PlaceId,
            imageToken = "",
            playing = 0,
            playerCount = 0,
            imageId = (long)0,
            isSponsored = false,
            totalBanners = 0,
            minimumAge = 0,
            minimumMembershipLevel = 0,
            price = 0,
            isPublicUniverse = true,
            genre = (object?)null
        }).ToList();

        return Content(Serialize(new { data }), "application/json");
    }

    [Authorize]
    [HttpGet("v1/games/multiget-place-details")]
    public async Task<IActionResult> MultiGetPlaceDetails(
        [FromQuery(Name = "placeIds")] string? placeIdsStr = null,
        CancellationToken cancellationToken = default)
    {
        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr) || string.IsNullOrWhiteSpace(placeIdsStr))
            return Content(Serialize(new { data = new object[] { } }), "application/json");

        var placeIds = placeIdsStr.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => long.TryParse(s.Trim(), out var id) ? id : 0)
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        var games = new List<GamesQueries.GameEntry>();
        foreach (var placeId in placeIds)
        {
            try
            {
                var universeId = await GamesRepository.GetUniverseIdFromPlaceIdAsync(connStr, placeId, cancellationToken)
                    .ConfigureAwait(false);
                if (universeId.HasValue)
                {
                    var entries = await GamesQueries.GetGameEntriesByUniverseIdsAsync(
                        new List<long> { universeId.Value }, connStr, cancellationToken).ConfigureAwait(false);
                    games.AddRange(entries);
                }
            }
            catch { }
        }

        var data = games.Select(g => new
        {
            placeId = g.PlaceId,
            universeId = g.UniverseId,
            name = g.Name,
            imageToken = g.IconUrl,
            isPlayable = true,
            reasonProhibited = "",
            universeRootPlaceId = g.PlaceId,
            price = 0,
            creatorName = g.CreatorName,
            creatorType = "User",
            creatorTargetId = g.CreatorUserId,
            creatorId = g.CreatorUserId
        }).ToList();

        return Content(Serialize(new { data }), "application/json");
    }

    [Authorize]
    [HttpGet("v1/games")]
    public async Task<IActionResult> GetGames(
        [FromQuery(Name = "universeIds")] string? universeIdsStr = null,
        CancellationToken cancellationToken = default)
    {
        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr) || string.IsNullOrWhiteSpace(universeIdsStr))
            return Content(Serialize(new { data = new object[] { } }), "application/json");

        var universeIds = universeIdsStr.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => long.TryParse(s.Trim(), out var id) ? id : 0)
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        var games = await GamesQueries.GetGameEntriesByUniverseIdsAsync(
            universeIds, connStr, cancellationToken).ConfigureAwait(false);

        var data = games.Select(g => new
        {
            id = g.UniverseId,
            creatorType = "User",
            creatorTargetId = g.CreatorUserId,
            creatorName = g.CreatorName,
            totalUpVotes = g.UpVotes,
            totalDownVotes = g.DownVotes,
            universeId = g.UniverseId,
            name = g.Name,
            placeId = g.PlaceId,
            imageToken = "",
            playing = 0,
            playerCount = 0,
            imageId = (long)0,
            isSponsored = false,
            totalBanners = 0,
            minimumAge = 0,
            minimumMembershipLevel = 0,
            price = 0,
            isPublicUniverse = true,
            genre = (object?)null
        }).ToList();

        return Content(Serialize(new { data }), "application/json");
    }

    [Authorize]
    [HttpGet("v1/games/multiget-playability-status")]
    public IActionResult GetPlayabilityStatus(
        [FromQuery(Name = "universeIds")] string? universeIdsStr = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(universeIdsStr))
            return Content(Serialize(new { data = new object[] { } }), "application/json");

        var universeIds = universeIdsStr.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => long.TryParse(s.Trim(), out var id) ? id : 0)
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        var data = universeIds.Select(uid => new
        {
            universeId = uid,
            isPlayable = true,
            reasonProhibited = "",
            placeId = (long)0
        }).ToList();

        return Content(Serialize(new { data }), "application/json");
    }

    [Authorize]
    [HttpGet("v1/games/icons")]
    public IActionResult GetGameIcons(
        [FromQuery(Name = "size")] string? size = null,
        [FromQuery(Name = "format")] string? format = null,
        [FromQuery(Name = "imageTokens")] string? imageTokens = null,
        [FromQuery(Name = "universeIds")] string? universeIdsStr = null,
        CancellationToken cancellationToken = default)
    {
        return GetGameIconsInternal(imageTokens, universeIdsStr);
    }

    [Authorize]
    [HttpGet("v1/games/game-thumbnails")]
    public IActionResult GetGameThumbnailsByTokens(
        [FromQuery(Name = "imageTokens")] string? imageTokens = null,
        [FromQuery(Name = "size")] string? size = null,
        [FromQuery(Name = "format")] string? format = null,
        CancellationToken cancellationToken = default)
    {
        return GetGameIconsInternal(imageTokens, null);
    }

    private IActionResult GetGameIconsInternal(string? imageTokens, string? universeIdsStr)
    {
        var connStr = _configuration.GetConnectionString("Default");
        var data = new List<object>();

        if (!string.IsNullOrWhiteSpace(imageTokens))
        {
            var tokens = imageTokens.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .ToArray();

            foreach (var token in tokens)
            {
                data.Add(new
                {
                    targetId = (long)0,
                    state = "Completed",
                    imageUrl = token.StartsWith("/") ? token : $"/images/{token}",
                    version = ""
                });
            }
        }

        if (!string.IsNullOrWhiteSpace(universeIdsStr) && !string.IsNullOrWhiteSpace(connStr))
        {
            var universeIds = universeIdsStr.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => long.TryParse(s.Trim(), out var id) ? id : 0)
                .Where(id => id > 0)
                .Distinct()
                .Take(100)
                .ToList();

            foreach (var uid in universeIds)
            {
                try
                {
                    var iconUrl = $"/Thumbs/Asset.ashx?x=150&y=150&asset={uid}";
                    data.Add(new
                    {
                        targetId = uid,
                        state = "Completed",
                        imageUrl = iconUrl,
                        version = ""
                    });
                }
                catch
                {
                    data.Add(new
                    {
                        targetId = uid,
                        state = "Completed",
                        imageUrl = "/images/DefaultProfile.png",
                        version = ""
                    });
                }
            }
        }

        return Content(Serialize(new { data }), "application/json");
    }
}
