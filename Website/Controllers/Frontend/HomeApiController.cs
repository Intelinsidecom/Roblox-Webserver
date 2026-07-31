using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Games;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Users;

namespace Website.Controllers.Frontend;

[ApiController]
public class HomeApiController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public HomeApiController(IConfiguration configuration)
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
    [HttpGet("/home/recently-visited-places")]
    public async Task<IActionResult> RecentlyVisitedPlaces(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var connStr = _configuration.GetConnectionString("Default");
        if (userId <= 0 || string.IsNullOrWhiteSpace(connStr))
            return Content(Serialize(new { Data = new { GameDisplayModels = Array.Empty<object>() } }), "application/json");

        try
        {
            var universeIds = await GamesQueries.GetRecentlyVisitedUniverseIdsAsync(userId, connStr, cancellationToken).ConfigureAwait(false);
            return await BuildGamesResponse(universeIds, connStr, cancellationToken);
        }
        catch
        {
            return Content(Serialize(new { Data = new { GameDisplayModels = Array.Empty<object>() } }), "application/json");
        }
    }

    [Authorize]
    [HttpGet("/home/friend-activity")]
    public async Task<IActionResult> FriendActivity(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var connStr = _configuration.GetConnectionString("Default");
        if (userId <= 0 || string.IsNullOrWhiteSpace(connStr))
            return Content(Serialize(new { Data = new { GameDisplayModels = Array.Empty<object>() } }), "application/json");

        try
        {
            var universeIds = await GamesQueries.GetFriendActivityUniverseIdsAsync(userId, connStr, 10, cancellationToken).ConfigureAwait(false);
            return await BuildGamesResponse(universeIds, connStr, cancellationToken);
        }
        catch
        {
            return Content(Serialize(new { Data = new { GameDisplayModels = Array.Empty<object>() } }), "application/json");
        }
    }

    [Authorize]
    [HttpGet("/user/favorites/places")]
    public async Task<IActionResult> FavoritePlaces(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var connStr = _configuration.GetConnectionString("Default");
        if (userId <= 0 || string.IsNullOrWhiteSpace(connStr))
            return Content(Serialize(new { Data = new { GameDisplayModels = Array.Empty<object>() } }), "application/json");

        try
        {
            var universeIds = await GamesQueries.GetFavoritePlaceUniverseIdsAsync(userId, connStr, cancellationToken).ConfigureAwait(false);
            return await BuildGamesResponse(universeIds, connStr, cancellationToken);
        }
        catch
        {
            return Content(Serialize(new { Data = new { GameDisplayModels = Array.Empty<object>() } }), "application/json");
        }
    }

    [Authorize]
    [HttpPost("/home/updatestatus")]
    public async Task<IActionResult> UpdateStatus([FromForm] string? status, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
            return new JsonResult(new { success = false, message = "Not authenticated" });

        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr))
            return new JsonResult(new { success = false, message = "Database not configured" });

        try
        {
            var statusText = status ?? "";
            statusText = statusText.Replace("{", "").Replace("}", "");
            if (statusText.Length > 254)
                statusText = statusText.Substring(0, 254);

            var ok = await UserQueries.UpdateUserStatusTextAsync(connStr, userId, statusText, cancellationToken).ConfigureAwait(false);
            if (!ok)
                return new JsonResult(new { success = false, message = "Failed to update status" });

            if (!string.IsNullOrWhiteSpace(statusText))
            {
                await UserQueries.InsertFeedEntryAsync(connStr, userId, statusText, 0, null, null, cancellationToken).ConfigureAwait(false);
            }

            return new JsonResult(new { success = true, message = statusText });
        }
        catch
        {
            return new JsonResult(new { success = false, message = "An error occurred" });
        }
    }

    private async Task<IActionResult> BuildGamesResponse(List<long> universeIds, string connStr, CancellationToken cancellationToken)
    {
        if (universeIds.Count == 0)
            return Content(Serialize(new { Data = new { GameDisplayModels = Array.Empty<object>() } }), "application/json");

        var games = await GamesQueries.GetGameEntriesByUniverseIdsAsync(universeIds, connStr, cancellationToken).ConfigureAwait(false);

        var arbiterUrl = _configuration["ArbiterUrl"] ?? "http://localhost:5000";
        var liveCounts = await GamesQueries.GetLivePlayerCountsForUniverseIdsAsync(
            games.Select(g => g.UniverseId).Distinct().ToList(), connStr, arbiterUrl, cancellationToken).ConfigureAwait(false);

        var orderedGames = universeIds
            .Select(id => games.FirstOrDefault(g => g.UniverseId == id))
            .Where(g => g != null)
            .ToList();

        var models = orderedGames.Select(g => new
        {
            HasErrorOcurred = false,
            PlayerCount = liveCounts.TryGetValue(g.UniverseId, out var pc) ? pc : g.Playing,
            TotalUpVotes = g.UpVotes,
            TotalDownVotes = g.DownVotes,
            Name = g.Name,
            Thumbnail = new
            {
                Url = g.IconUrl,
                Final = !string.IsNullOrEmpty(g.IconUrl) && !g.IconUrl.Contains("blocked.png"),
                RetryUrl = (string?)null,
                UserId = 0L,
                EndpointType = "Avatar"
            },
            CreatorName = g.CreatorName,
            CreatorAbsoluteUrl = $"/users/{g.CreatorUserId}/profile",
            GameDetailReferralUrl = $"/games/{g.PlaceId}",
            UseDataSrc = false,
            DeleteUniverseId = g.UniverseId
        }).ToList();

        return Content(Serialize(new
        {
            Data = new
            {
                GameDisplayModels = models,
                ShowSmallGameIcon = false,
                LabelCreatorByJs = "By {creatorLink}",
                LabelPlayingPhraseJs = "{playerCount} Playing"
            }
        }), "application/json");
    }

    private long GetCurrentUserId()
    {
        var idClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(idClaim))
            return 0;
        if (long.TryParse(idClaim, out var id))
            return id;
        return 0;
    }
}
