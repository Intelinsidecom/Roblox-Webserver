using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Games;

namespace RobloxWebserver.Controllers
{
    // Minimal stub endpoints required by the front-end Build/Develop JavaScript.
    [ApiController]
    [Route("build")]
    [Authorize]
    public class BuildController : Controller
    {
        private readonly IConfiguration _configuration;

        public BuildController(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public string? ConnectionString => _configuration.GetConnectionString("Default");

        /// <summary>
        /// Legacy endpoint used by UniverseLoader.js. Returns HTML table of user's universes.
        /// </summary>
        [HttpGet("universes")]
        public async Task<IActionResult> Universes(int startRow = 0, bool activeOnly = false, long? groupId = null, CancellationToken cancellationToken = default)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !long.TryParse(userIdClaim, out var userId) || userId <= 0)
                return Content(string.Empty, "text/html");

            var connStr = ConnectionString;
            if (string.IsNullOrWhiteSpace(connStr))
                return Content(string.Empty, "text/html");

            try
            {
                var universes = await GameListingService
                    .GetUniversesForUserAsync(connStr, userId, cancellationToken)
                    .ConfigureAwait(false);

                var html = new System.Text.StringBuilder();
                
                foreach (var u in universes)
                {
                    if (activeOnly && u.PrivacyLevel != 1) continue;

                    var statusText = u.PrivacyLevel switch
                    {
                        2 => "Friends",
                        3 => "Private",
                        _ => "Public",
                    };

                    var privacyClass = u.PrivacyLevel == 1 ? "place-active" : "place-inactive";

                    html.AppendLine(@"<table class='item-table' data-item-id='" + u.UniverseId + @"' data-rootplace-id='" + u.RootPlaceId + @"' data-type='universes'>
<tr>
    <td class='image-col universe-image-col' style='text-align:center;'>
        <a href='/universes/configure?id=" + u.UniverseId + @"' class='game-image'>
            <img src='" + u.ThumbnailUrl + @"' alt='" + u.UniverseName + @"' />
        </a>
    </td>
    <td class='universe-name-col'>
        <a class='title notranslate' href='/universes/configure?id=" + u.UniverseId + @"'>" + u.UniverseName + @"</a>
        <table class='details-table'>
            <tr>
                <td class='item-universe'>
                    <span>Start Place:</span>
                    <a class='title notranslate start-place-url' href='/games/" + u.RootPlaceId + @"/" + System.Web.HttpUtility.UrlEncode(u.PlaceName) + @"'>" + u.PlaceName + @"</a>
                </td>
            </tr>
            <tr class='activate-cell'>
                <td>
                    <a class='" + privacyClass + @"' href='/universes/configure?id=" + u.UniverseId + @"'>" + statusText + @"</a>
                </td>
            </tr>
        </table>
    </td>
    <td class='edit-col'>
        <a class='roblox-edit-button btn-control btn-control-large' href='javascript:editGameInStudio(" + u.RootPlaceId + @", " + u.UniverseId + @", true)'>Edit</a>
    </td>
    <td class='menu-col'>
        <div class='gear-button-wrapper'>
            <a href='#' class='gear-button'></a>
        </div>
    </td>
</tr>
</table>
<div class='separator'></div>");
                }

                return Content(html.ToString(), "text/html");
            }
            catch
            {
                return Content(string.Empty, "text/html");
            }
        }
    }
}
