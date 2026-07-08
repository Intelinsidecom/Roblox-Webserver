using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Website.Services;

namespace RobloxWebserver.Controllers;

[ApiController]
[Route("develop")]
[Authorize]
public class DevelopTabController : Controller
{
    private readonly Website.Services.DevelopTabService _tabService;

    public DevelopTabController(Website.Services.DevelopTabService tabService)
    {
        _tabService = tabService ?? throw new ArgumentNullException(nameof(tabService));
    }

    public static string ResolveViewName(string? viewParam, string? pageParam)
    {
        if (string.IsNullOrEmpty(viewParam) &&
            string.Equals(pageParam, "universes", StringComparison.OrdinalIgnoreCase))
        {
            return "Games";
        }

        if (string.IsNullOrEmpty(viewParam) && string.IsNullOrEmpty(pageParam))
        {
            return "T-Shirts";
        }

        if (string.IsNullOrEmpty(viewParam))
        {
            return "Games";
        }

        return viewParam switch
        {
            "9" => "Places",
            "10" => "Models",
            "13" => "Decals",
            "21" => "Badges",
            "3" => "Audio",
            "24" => "Animations",
            "40" => "Meshes",
            "2" => "T-Shirts",
            "11" => "Shirts",
            "12" => "Pants",
            "38" => "Plugins",
            _ => "Games",
        };
    }

    [HttpGet("tab/{view}")]
    public async Task<IActionResult> Tab(string view,
        [FromQuery] bool showPublicOnly = false,
        [FromQuery] int? category = null,
        [FromQuery] int? sortType = null,
        [FromQuery] string[]? genres = null,
        [FromQuery] int pageNumber = 1,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(view) && char.IsLower(view[0]))
            view = char.ToUpperInvariant(view[0]) + view[1..];

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !long.TryParse(userIdClaim, out var userId) || userId <= 0)
        {
            return Unauthorized();
        }

        var nameClaim = User.FindFirst(ClaimTypes.Name)?.Value;

        var genreList = genres?
            .SelectMany(g => g.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(s => int.TryParse(s, out var id) ? id : (int?)null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var vm = await _tabService.BuildAsync(userId, nameClaim, view, showPublicOnly,
            groupId: null, category: category, sortType: sortType,
            genres: genreList, pageNumber: pageNumber, cancellationToken: cancellationToken).ConfigureAwait(false);

        return PartialView($"~/Views/Develop/Tabs/{view}.cshtml", vm);
    }

    [HttpGet("asset-list/{assetTypeId:int}")]
    public async Task<IActionResult> AssetList(int assetTypeId, CancellationToken cancellationToken)
    {
        var viewName = assetTypeId switch
        {
            0 => "Games",
            2 => "T-Shirts",
            11 => "Shirts",
            12 => "Pants",
            _ => ResolveViewName(assetTypeId.ToString(), null),
        };

        return await Tab(viewName, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    [HttpGet("games-list")]
    public Task<IActionResult> GamesList(CancellationToken cancellationToken)
        => Tab("Games", cancellationToken: cancellationToken);

    [HttpGet("groups/games-list")]
    public Task<IActionResult> GroupGamesList(CancellationToken cancellationToken)
        => Tab("Games", cancellationToken: cancellationToken);
}
