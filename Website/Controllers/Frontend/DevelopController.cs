using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Assets;
using Npgsql;

namespace RobloxWebserver.Controllers
{
    // Handles endpoints used by the legacy /develop page JavaScript
    [ApiController]
    [Route("develop")]
    [Authorize]
    public class DevelopController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly TShirtAssetService _tshirtService = new TShirtAssetService();
        private readonly PantsAssetService _pantsService = new PantsAssetService();
        private readonly ShirtAssetService _shirtService = new ShirtAssetService();
        private readonly ShirtAssetsRepository _shirtAssetsRepository = new ShirtAssetsRepository();
        private readonly UserAssetsRepository _userAssetsRepository = new UserAssetsRepository();

        public DevelopController(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Legacy endpoint expected by BuildPage.js.  Returns an HTML fragment that is injected
        /// into the #MyCreationsTab .items-container element via jQuery .load().
        ///
        /// This implementation now queries the universes + assets tables to list the
        /// logged-in user's games, instead of returning a single static demo row.
        /// </summary>
        [HttpGet("games-list")]
        public async Task<IActionResult> GamesList(CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !long.TryParse(userIdClaim, out var userId) || userId <= 0)
            {
                return Unauthorized();
            }

            var sb = new StringBuilder();

            // Header + create button (mirrors the previous static fragment)
            sb.Append("<a href='/places/create' id='CreatePlace' class='create-new-button btn-medium btn-primary'>Create New Game</a>");
            sb.Append("<table class='section-header'>");
            sb.Append("    <tr>");
            sb.Append("        <td class='content-title'>");
            sb.Append("            <div>");
            sb.Append("                <h2 class='header-text'>Games</h2>");
            sb.Append("                <span class='aside-text' data-active-count='0' data-max-active-count='200'></span>");
            sb.Append("                <label class='checkbox-label active-only-checkbox'>");
            sb.Append("                    <input type='checkbox' />Show Public Only");
            sb.Append("                </label>");
            sb.Append("            </div>");
            sb.Append("        </td>");
            sb.Append("    </tr>");
            sb.Append("</table>");

            sb.Append("<div class='items-container-inner'>");

            try
            {
                var connStr = _configuration.GetConnectionString("Default");
                var universes = await Games.GameListingService.GetUniversesForUserAsync(connStr, userId, cancellationToken).ConfigureAwait(false);

                var hasAny = false;
                foreach (var universe in universes)
                {
                    hasAny = true;

                    var statusText = universe.PrivacyLevel switch
                    {
                        2 => "Friends",
                        3 => "Private",
                        _ => "Public"
                    };

                    // Use privacy-dependent CSS so the correct icon slice is shown:
                    // place-active (public) vs place-inactive (friends/private).
                    var privacyClass = universe.PrivacyLevel == 1 ? "place-active" : "place-inactive";

                    var placeSlug = CatalogController.ToSlug(universe.PlaceName);
                    var configureUrl = "/universes/configure?id=" + universe.UniverseId;
                    var startPlaceUrl = universe.RootPlaceId > 0
                        ? "/games/" + universe.RootPlaceId + "/" + placeSlug
                        : "#";

                    sb.Append("    <table class='item-table' data-item-id='");
                    sb.Append(universe.UniverseId);
                    sb.Append("' data-rootplace-id='");
                    sb.Append(universe.RootPlaceId);
                    sb.Append("' data-type='universes'>");
                    sb.Append("        <tr>");
                    sb.Append("            <td class='image-col universe-image-col' style='text-align:center;'>");
                    sb.Append("                <a href='");
                    sb.Append(configureUrl);
                    sb.Append("' class='game-image'>");
                    sb.Append("                    <img src='");
                    sb.Append(System.Net.WebUtility.HtmlEncode(universe.ThumbnailUrl ?? "/images/blocked.png"));
                    sb.Append("' alt='");
                    sb.Append(System.Net.WebUtility.HtmlEncode(universe.UniverseName));
                    sb.Append("' />");
                    sb.Append("                </a>");
                    sb.Append("            </td>");
                    sb.Append("            <td class='universe-name-col'>");
                    sb.Append("                <a class='title notranslate' href='");
                    sb.Append(configureUrl);
                    sb.Append("'>");
                    sb.Append(System.Net.WebUtility.HtmlEncode(universe.UniverseName));
                    sb.Append("</a>");
                    sb.Append("                <table class='details-table'>");
                    sb.Append("                    <tr>");
                    sb.Append("                        <td class='item-universe'>");
                    sb.Append("                            <span>Start Place:</span>");
                    sb.Append("                            <a class='title notranslate start-place-url' href='");
                    sb.Append(startPlaceUrl);
                    sb.Append("'>");
                    sb.Append(System.Net.WebUtility.HtmlEncode(universe.PlaceName));
                    sb.Append("</a>");
                    sb.Append("                        </td>");
                    sb.Append("                    </tr>");
                    sb.Append("                    <tr class='activate-cell'>");
                    sb.Append("                        <td>");
                    sb.Append("                            <a class='");
                    sb.Append(privacyClass);
                    sb.Append("' href='");
                    sb.Append(configureUrl);
                    sb.Append("'>");
                    sb.Append(System.Net.WebUtility.HtmlEncode(statusText));
                    sb.Append("</a>");
                    sb.Append("                        </td>");
                    sb.Append("                    </tr>");
                    sb.Append("                </table>");
                    sb.Append("            </td>");
                    sb.Append("            <td class='edit-col'>");
                    sb.Append("                <a class='roblox-edit-button btn-control btn-control-large' href='javascript:'>Edit</a>");
                    sb.Append("            </td>");
                    sb.Append("            <td class='menu-col'>");
                    sb.Append("                <div class='gear-button-wrapper'>");
                    sb.Append("                    <a href='#' class='gear-button'></a>");
                    sb.Append("                </div>");
                    sb.Append("            </td>");
                    sb.Append("        </tr>");
                    sb.Append("    </table>");
                    sb.Append("    <div class='separator'></div>");
                }

                if (!hasAny)
                {
                    sb.Append("    <div class='no-games-text'>You have no games yet. Click 'Create New Game' to get started.</div>");
                }
            }
            catch
            {
                sb.Append("    <div class='no-games-text'>We couldn't load your games right now.</div>");
            }

            sb.Append("</div>");

            // Return as text/html so jQuery .load() inserts raw DOM into the empty items-container
            return Content(sb.ToString(), "text/html");
        }
        /// <summary>
        /// Placeholder endpoint for group creations games list expected by legacy develop page.
        /// </summary>
        [HttpGet("groups/games-list")]
        public IActionResult GroupGamesList()
        {
            // TODO: Replace with real data once group database implemented
            const string html = @"<div class='items-container-inner'><div class='no-games-text'>This group has no games yet.</div></div>";
            return Content(html, "text/html");
        }

        /// <summary>
        /// Generic asset list endpoint used by various view tabs.
        /// Returns HTML fragments that are injected into the develop page via jQuery .load().
        /// </summary>
        [HttpGet("asset-list/{assetTypeId:int}")]
        public async Task<IActionResult> AssetList(int assetTypeId, CancellationToken cancellationToken)
        {
            // Treat assetTypeId 0 as universes/games and reuse the same HTML as the games list
            if (assetTypeId == 0)
            {
                // Delegate to the games list endpoint so the HTML fragment stays consistent.
                return await GamesList(cancellationToken);
            }

            // T-Shirt view for assetTypeId 2: show upload form and the current user's T-Shirt inventory
            if (assetTypeId == 2)
            {
                var sb = new StringBuilder();
                sb.Append(@"<div class='items-container-inner'>");
                sb.Append(@"    <h1 class='title'>Create a T-Shirt <span class='tip-text'>Don't know how? <a href='https://en.help.roblox.com/hc/en-us/articles/203313200' class='text-link'>Click here</a></span></h1>");
                sb.Append(@"    <form id='tshirt-upload-form' method='post' enctype='multipart/form-data' action='/develop/upload-tshirt'>");

                // Find your image row
                sb.Append(@"        <div class='form-outer'>");
                sb.Append(@"            <div class='form-inner label-column'>");
                sb.Append(@"                <span class='form-label'>Find your image:</span>");
                sb.Append(@"            </div>");
                sb.Append(@"            <div class='form-inner input-column'>");
                sb.Append(@"                <input type='file' id='tshirt-file' name='file' accept='image/*' required />");
                sb.Append(@"            </div>");
                sb.Append(@"        </div>");

                // T-Shirt name row
                sb.Append(@"        <div class='form-outer'>");
                sb.Append(@"            <div class='form-inner label-column'>");
                sb.Append(@"                <span class='form-label'>T-Shirt Name:</span>");
                sb.Append(@"            </div>");
                sb.Append(@"            <div class='form-inner input-column'>");
                sb.Append(@"                <input type='text' id='tshirt-name' name='name' class='text-box text-box-large' required />");
                sb.Append(@"            </div>");
                sb.Append(@"        </div>");

                // Upload button row
                sb.Append(@"        <div class='form-outer'>");
                sb.Append(@"            <div class='form-inner label-column'></div>");
                sb.Append(@"            <div class='form-inner input-column'>");
                sb.Append(@"                <button type='submit' class='btn-medium btn-primary'>Upload</button>");
                sb.Append(@"            </div>");
                sb.Append(@"        </div>");

                sb.Append(@"    </form>");

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrWhiteSpace(userIdClaim) && long.TryParse(userIdClaim, out var userId) && userId > 0)
                {
                    var connStr = _configuration.GetConnectionString("Default");
                    if (!string.IsNullOrWhiteSpace(connStr))
                    {
                        try
                        {
                            await using var conn = new NpgsqlConnection(connStr);
                            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

                            var sql = @"select t.asset_id as tshirt_asset_id,
       t.name,
       ua_t.created_at,
       t.thumbnail_url,
       i.asset_id as image_asset_id
from user_assets ua_t
join assets t on t.asset_id = ua_t.asset_id and t.asset_type_id = 2 and t.owner_user_id = @uid
left join assets i on i.owner_user_id = t.owner_user_id
                  and i.asset_type_id = 1
                  and i.name = t.name || ' Image'
where ua_t.user_id = @uid
order by ua_t.created_at desc, t.asset_id desc
limit 50;";

                            await using var cmd = new NpgsqlCommand(sql, conn);
                            cmd.Parameters.AddWithValue("uid", userId);

                            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

                            var hasAny = false;
                            sb.Append(@"    <div class='tshirt-inventory-list'>");
                            sb.Append(@"        <h3 class='header-text'>Your T-Shirts</h3>");
                            sb.Append(@"        <div class='items-container'>");

                            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                            {
                                hasAny = true;
                                var assetId = reader.GetInt64(0); // T-Shirt asset id (.rbxm)
                                var name = reader.IsDBNull(1) ? "Unnamed" : reader.GetString(1);
                                var createdAt = reader.GetDateTime(2);
                                var thumbUrl = reader.IsDBNull(3) ? null : reader.GetString(3);
                                var imageAssetId = reader.IsDBNull(4) ? (long?)null : reader.GetInt64(4); // Image asset id
                                var createdDateString = createdAt.ToString("M/d/yyyy");

                                // Use the same catalog item URL pattern: /catalog/{id}/{slug}
                                var slug = CatalogController.ToSlug(name);
                                var catalogUrl = "/catalog/" + assetId + "/" + slug;

                                sb.Append(@"            <table class='item-table' data-item-id='");
                                sb.Append(assetId);
                                sb.Append(@"' data-type='tshirts'>");
                                sb.Append(@"                <tr>");
                                sb.Append(@"                    <td class='image-col universe-image-col' style='text-align:center'>");
                                sb.Append(@"                        <a href='");
                                sb.Append(catalogUrl);
                                sb.Append(@"' class='game-image'> <img src='");
                                sb.Append(System.Net.WebUtility.HtmlEncode(thumbUrl ?? "https://t7.rbxcdn.com/6bfa4d3e4d38a70d2f5b493987fe29c4"));
                                sb.Append(@"' alt='T-Shirt'> </a>");
                                sb.Append(@"                    </td>");
                                sb.Append(@"                    <td class='universe-name-col'>");
                                sb.Append(@"                        <a class='title notranslate' href='");
                                sb.Append(catalogUrl);
                                sb.Append("'>");
                                sb.Append(System.Net.WebUtility.HtmlEncode(name));
                                sb.Append(@"</a>");
                                sb.Append(@"                        <table class='details-table'>");
                                sb.Append(@"                            <tr>");
                                sb.Append(@"                                <td class='item-universe'><span>Created:</span> ");
                                sb.Append(createdDateString);
                                sb.Append(@" (ID: ");
                                var idToShow = imageAssetId ?? assetId;
                                sb.Append(@"<a href='/asset/?id=");
                                sb.Append(idToShow);
                                sb.Append(@"'>");
                                sb.Append(idToShow);
                                sb.Append(@"</a>)</td>");
                                sb.Append(@"                            </tr>");
                                sb.Append(@"                        </table>");
                                sb.Append(@"                    </td>");
                                sb.Append(@"                    <td class='edit-col'></td>");
                                sb.Append(@"                    <td class='menu-col'>");
                                sb.Append(@"                        <div class='gear-button-wrapper'>");
                                sb.Append(@"                            <a href='#' class='gear-button'></a>");
                                sb.Append(@"                        </div>");
                                sb.Append(@"                    </td>");
                                sb.Append(@"                </tr>");
                                sb.Append(@"            </table>");
                                sb.Append(@"            <div class='separator'></div>");
                            }

                            if (!hasAny)
                            {
                                sb.Append(@"            <div class='no-assets-text'>You have no T-Shirts yet. Upload one to see it here!</div>");
                            }

                            sb.Append(@"        </div>");

                            // Add dropdown menu for T-shirt actions
                            sb.Append(@"        <div id='tshirt-dropdown-menu' style='display:none;'>");
                            sb.Append(@"            <a href='#' data-action='configure'>Configure</a>");
                            sb.Append(@"            <a href='#' data-action='advertise' class='divider-top'>Advertise</a>");
                            sb.Append(@"        </div>");

                            sb.Append(@"    </div>");
                        }
                        catch
                        {
                            sb.Append(@"    <div class='tshirt-inventory-list'>");
                            sb.Append(@"        <div class='no-assets-text'>We couldn't load your T-Shirts right now.</div>");
                            sb.Append(@"    </div>");
                        }
                    }
                }

                sb.Append(@"</div>");

                return Content(sb.ToString(), "text/html");
            }

            // Shirts view for assetTypeId 11: show upload form and the current user's Shirts inventory
            if (assetTypeId == 11)
            {
                var sb = new StringBuilder();
                sb.Append(@"<div class='items-container-inner'>");
                sb.Append(@"    <h1 class='title'>Create Shirt</h1>");
                sb.Append(@"    <h3 class='title'>Get the Template<a href=""/images/Template-Shirts.png"">here</a></h3>");
                sb.Append(@"    <form id='shirt-upload-form' method='post' enctype='multipart/form-data' action='/develop/upload-shirt'>");

                // Find your image row
                sb.Append(@"        <div class='form-outer'>");
                sb.Append(@"            <div class='form-inner label-column'>");
                sb.Append(@"                <span class='form-label'>Find your shirt image:</span>");
                sb.Append(@"            </div>");
                sb.Append(@"            <div class='form-inner input-column'>");
                sb.Append(@"                <input type='file' id='shirt-file' name='file' accept='image/*' required />");
                sb.Append(@"            </div>");
                sb.Append(@"        </div>");

                // Shirt name row
                sb.Append(@"        <div class='form-outer'>");
                sb.Append(@"            <div class='form-inner label-column'>");
                sb.Append(@"                <span class='form-label'>Shirt Name:</span>");
                sb.Append(@"            </div>");
                sb.Append(@"            <div class='form-inner input-column'>");
                sb.Append(@"                <input type='text' id='shirt-name' name='name' class='text-box text-box-large' required />");
                sb.Append(@"            </div>");
                sb.Append(@"        </div>");

                // Upload button row
                sb.Append(@"        <div class='form-outer'>");
                sb.Append(@"            <div class='form-inner label-column'></div>");
                sb.Append(@"            <div class='form-inner input-column'>");
                sb.Append(@"                <button type='submit' class='btn-medium btn-primary'>Upload</button>");
                sb.Append(@"                <div id='shirt-upload-error' class='text-error'></div>");
                sb.Append(@"            </div>");
                sb.Append(@"        </div>");

                sb.Append(@"    </form>");

                var userIdClaimShirt = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrWhiteSpace(userIdClaimShirt) && long.TryParse(userIdClaimShirt, out var userIdShirt) && userIdShirt > 0)
                {
                    var connStrShirt = _configuration.GetConnectionString("Default");
                    if (!string.IsNullOrWhiteSpace(connStrShirt))
                    {
                        try
                        {
                            var shirtItems = await _shirtAssetsRepository.GetUserShirtsWithImagesAsync(connStrShirt, userIdShirt, cancellationToken).ConfigureAwait(false);

                            var hasAnyShirts = false;
                            sb.Append(@"    <div class='shirts-inventory-list'>");
                            sb.Append(@"        <h3 class='header-text'>Your Shirts</h3>");
                            sb.Append(@"        <div class='items-container'>");

                            foreach (var item in shirtItems)
                            {
                                hasAnyShirts = true;
                                var assetId = item.AssetId;
                                var name = item.Name;
                                var createdDateString = item.CreatedAt.ToString("M/d/yyyy");
                                var thumbUrl = item.ThumbnailUrl;
                                var imageAssetId = item.ImageAssetId;

                                var slug = CatalogController.ToSlug(name);
                                var catalogUrl = "/catalog/" + assetId + "/" + slug;

                                sb.Append(@"            <table class='item-table' data-item-id='");
                                sb.Append(assetId);
                                sb.Append(@"' data-type='shirts'>");
                                sb.Append(@"                <tr>");
                                sb.Append(@"                    <td class='image-col universe-image-col' style='text-align:center'>");
                                sb.Append(@"                        <a href='");
                                sb.Append(catalogUrl);
                                sb.Append(@"' class='game-image'> <img src='");
                                sb.Append(System.Net.WebUtility.HtmlEncode(thumbUrl ?? "https://t7.rbxcdn.com/6bfa4d3e4d38a70d2f5b493987fe29c4"));
                                sb.Append(@"' alt='Shirt'> </a>");
                                sb.Append(@"                    </td>");
                                sb.Append(@"                    <td class='universe-name-col'>");
                                sb.Append(@"                        <a class='title notranslate' href='");
                                sb.Append(catalogUrl);
                                sb.Append(@"'>");
                                sb.Append(System.Net.WebUtility.HtmlEncode(name));
                                sb.Append(@"</a>");
                                sb.Append(@"                        <table class='details-table'>");
                                sb.Append(@"                            <tr>");
                                sb.Append(@"                                <td class='item-universe'><span>Created:</span> ");
                                sb.Append(createdDateString);
                                sb.Append(@" (ID: ");
                                var idToShowShirt = imageAssetId ?? assetId;
                                sb.Append(@"<a href='/asset/?id=");
                                sb.Append(idToShowShirt);
                                sb.Append(@"'>");
                                sb.Append(idToShowShirt);
                                sb.Append(@"</a>)</td>");
                                sb.Append(@"                            </tr>");
                                sb.Append(@"                        </table>");
                                sb.Append(@"                    </td>");
                                sb.Append(@"                    <td class='edit-col'></td>");
                                sb.Append(@"                    <td class='menu-col'>");
                                sb.Append(@"                        <div class='gear-button-wrapper'>");
                                sb.Append(@"                            <a href='#' class='gear-button'></a>");
                                sb.Append(@"                        </div>");
                                sb.Append(@"                    </td>");
                                sb.Append(@"                </tr>");
                                sb.Append(@"            </table>");
                                sb.Append(@"            <div class='separator'></div>");
                            }

                            if (!hasAnyShirts)
                            {
                                sb.Append(@"            <div class='no-assets-text'>You have no Shirts yet. Upload one to see it here!</div>");
                            }

                            sb.Append(@"        </div>");

                            // Reuse the same dropdown menu id as T-Shirts/Pants so legacy JS treats shirts as clothing
                            sb.Append(@"        <div id='tshirt-dropdown-menu' style='display:none;'>");
                            sb.Append(@"            <a href='#' data-action='configure'>Configure</a>");
                            sb.Append(@"            <a href='#' data-action='advertise' class='divider-top'>Advertise</a>");
                            sb.Append(@"        </div>");

                            sb.Append(@"    </div>");
                        }
                        catch
                        {
                            sb.Append(@"    <div class='shirts-inventory-list'>");
                            sb.Append(@"        <div class='no-assets-text'>We couldn't load your Shirts right now.</div>");
                            sb.Append(@"    </div>");
                        }
                    }
                }

                sb.Append(@"</div>");

                return Content(sb.ToString(), "text/html");
            }

            // Pants view for assetTypeId 12: show upload form and the current user's Pants inventory
            if (assetTypeId == 12)
            {
                var sb = new StringBuilder();
                sb.Append(@"<div class='items-container-inner'>");
                sb.Append(@"    <h1 class='title'>Create Pants</h1>");
                sb.Append(@"    <h3 class='title'>Get the Template<a href=""/images/Template-Pants.png"">here</a></h3>");
                sb.Append(@"    <form id='pants-upload-form' method='post' enctype='multipart/form-data' action='/develop/upload-pants'>");

                // Find your image row
                sb.Append(@"        <div class='form-outer'>");
                sb.Append(@"            <div class='form-inner label-column'>");
                sb.Append(@"                <span class='form-label'>Find your pants image:</span>");
                sb.Append(@"            </div>");
                sb.Append(@"            <div class='form-inner input-column'>");
                sb.Append(@"                <input type='file' id='pants-file' name='file' accept='image/*' required />");
                sb.Append(@"            </div>");
                sb.Append(@"        </div>");

                // Pants name row
                sb.Append(@"        <div class='form-outer'>");
                sb.Append(@"            <div class='form-inner label-column'>");
                sb.Append(@"                <span class='form-label'>Pants Name:</span>");
                sb.Append(@"            </div>");
                sb.Append(@"            <div class='form-inner input-column'>");
                sb.Append(@"                <input type='text' id='pants-name' name='name' class='text-box text-box-large' required />");
                sb.Append(@"            </div>");
                sb.Append(@"        </div>");

                // Upload button row
                sb.Append(@"        <div class='form-outer'>");
                sb.Append(@"            <div class='form-inner label-column'></div>");
                sb.Append(@"            <div class='form-inner input-column'>");
                sb.Append(@"                <button type='submit' class='btn-medium btn-primary'>Upload</button>");
                sb.Append(@"                <div id='pants-upload-error' class='text-error'></div>");
                sb.Append(@"            </div>");
                sb.Append(@"        </div>");

                sb.Append(@"    </form>");

                var userIdClaimPants = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrWhiteSpace(userIdClaimPants) && long.TryParse(userIdClaimPants, out var userIdPants) && userIdPants > 0)
                {
                    var connStrPants = _configuration.GetConnectionString("Default");
                    if (!string.IsNullOrWhiteSpace(connStrPants))
                    {
                        try
                        {
                            var pantsItems = await _userAssetsRepository.GetUserPantsWithImagesAsync(connStrPants, userIdPants, cancellationToken).ConfigureAwait(false);

                            var hasAnyPants = false;
                            sb.Append(@"    <div class='pants-inventory-list'>");
                            sb.Append(@"        <h3 class='header-text'>Your Pants</h3>");
                            sb.Append(@"        <div class='items-container'>");

                            foreach (var item in pantsItems)
                            {
                                hasAnyPants = true;
                                var assetId = item.AssetId;
                                var name = item.Name;
                                var createdDateString = item.CreatedAt.ToString("M/d/yyyy");
                                var thumbUrl = item.ThumbnailUrl;
                                var imageAssetId = item.ImageAssetId;

                                var slug = CatalogController.ToSlug(name);
                                var catalogUrl = "/catalog/" + assetId + "/" + slug;

                                sb.Append(@"            <table class='item-table' data-item-id='");
                                sb.Append(assetId);
                                sb.Append(@"' data-type='pants'>");
                                sb.Append(@"                <tr>");
                                sb.Append(@"                    <td class='image-col universe-image-col' style='text-align:center'>");
                                sb.Append(@"                        <a href='");
                                sb.Append(catalogUrl);
                                sb.Append(@"' class='game-image'> <img src='");
                                sb.Append(System.Net.WebUtility.HtmlEncode(thumbUrl ?? "https://t7.rbxcdn.com/6bfa4d3e4d38a70d2f5b493987fe29c4"));
                                sb.Append(@"' alt='Pants'> </a>");
                                sb.Append(@"                    </td>");
                                sb.Append(@"                    <td class='universe-name-col'>");
                                sb.Append(@"                        <a class='title notranslate' href='");
                                sb.Append(catalogUrl);
                                sb.Append(@"'>");
                                sb.Append(System.Net.WebUtility.HtmlEncode(name));
                                sb.Append(@"</a>");
                                sb.Append(@"                        <table class='details-table'>");
                                sb.Append(@"                            <tr>");
                                sb.Append(@"                                <td class='item-universe'><span>Created:</span> ");
                                sb.Append(createdDateString);
                                sb.Append(@" (ID: ");
                                var idToShowPants = imageAssetId ?? assetId;
                                sb.Append(@"<a href='/asset/?id=");
                                sb.Append(idToShowPants);
                                sb.Append(@"'>");
                                sb.Append(idToShowPants);
                                sb.Append(@"</a>)</td>");
                                sb.Append(@"                            </tr>");
                                sb.Append(@"                        </table>");
                                sb.Append(@"                    </td>");
                                sb.Append(@"                    <td class='edit-col'></td>");
                                sb.Append(@"                    <td class='menu-col'>");
                                sb.Append(@"                        <div class='gear-button-wrapper'>");
                                sb.Append(@"                            <a href='#' class='gear-button'></a>");
                                sb.Append(@"                        </div>");
                                sb.Append(@"                    </td>");
                                sb.Append(@"                </tr>");
                                sb.Append(@"            </table>");
                                sb.Append(@"            <div class='separator'></div>");
                            }

                            if (!hasAnyPants)
                            {
                                sb.Append(@"            <div class='no-assets-text'>You have no Pants yet. Upload one to see it here!</div>");
                            }

                            sb.Append(@"        </div>");

                            // Add dropdown menu for Pants actions (reuse clothing dropdown shared with T-Shirts)
                            sb.Append(@"        <div id='tshirt-dropdown-menu' style='display:none;'>");
                            sb.Append(@"            <a href='#' data-action='configure'>Configure</a>");
                            sb.Append(@"            <a href='#' data-action='advertise' class='divider-top'>Advertise</a>");
                            sb.Append(@"        </div>");

                            sb.Append(@"    </div>");
                        }
                        catch
                        {
                            sb.Append(@"    <div class='pants-inventory-list'>");
                            sb.Append(@"        <div class='no-assets-text'>We couldn't load your Pants right now.</div>");
                            sb.Append(@"    </div>");
                        }
                    }
                }

                sb.Append(@"</div>");

                return Content(sb.ToString(), "text/html");
            }

            // For all other asset types, just return simple text
            return Content("hello", "text/html");
        }

        [HttpPost("upload-tshirt")]
        public async Task<IActionResult> UploadTShirt([FromForm] string name, [FromForm] IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is required.");

            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Name is required.");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !long.TryParse(userIdClaim, out var userId) || userId <= 0)
                return Unauthorized("User must be logged in to upload assets.");

            var connStr = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
                return StatusCode(500, "Database connection string is not configured.");

            if (await _userAssetsRepository.HasUploadedClothingInLastHourAsync(connStr, userId, cancellationToken).ConfigureAwait(false))
                return BadRequest("You can only upload one shirt, pants, or T-Shirt per hour. Please try again later.");

            byte[] fileBytes;
            await using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
                fileBytes = ms.ToArray();
            }

            var assetsDirectory = _configuration["Assets:Directory"];
            if (string.IsNullOrWhiteSpace(assetsDirectory))
                return StatusCode(500, "Assets directory is not configured.");

            var thumbnailsRoot = _configuration["Thumbnails:OutputDirectory"];
            var thumbnailBaseUrl = _configuration["Thumbnails:ThumbnailUrl"];
            var tshirtTemplatePath = _configuration["Thumbnails:TshirtTemplatePath"];
            var tshirtTemplateHighResPath = _configuration["Thumbnails:TshirtTemplateHighResPath"];
            var publicAssetBaseUrl = _configuration["Assets:PublicBaseUrl"];

            try
            {
                var scheme = string.IsNullOrEmpty(Request.Scheme) ? "http" : Request.Scheme;
                var host = Request.Host.HasValue ? Request.Host.Value : "localhost";
                var baseUrl = $"{scheme}://{host}";

                _ = await _tshirtService.CreateTShirtAsync(
                    connStr,
                    userId,
                    name,
                    file.FileName,
                    file.ContentType,
                    fileBytes,
                    assetsDirectory,
                    thumbnailsRoot ?? string.Empty,
                    thumbnailBaseUrl ?? string.Empty,
                    tshirtTemplatePath ?? string.Empty,
                    tshirtTemplateHighResPath ?? string.Empty,
                    baseUrl,
                    publicAssetBaseUrl,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return StatusCode(500, "Failed to save asset record.");
            }

            return Redirect("/develop?view=2");
        }

        [HttpPost("upload-pants")]
        public async Task<IActionResult> UploadPants([FromForm] string name, [FromForm] IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is required.");

            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Name is required.");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !long.TryParse(userIdClaim, out var userId) || userId <= 0)
                return Unauthorized("User must be logged in to upload assets.");

            var connStr = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
                return StatusCode(500, "Database connection string is not configured.");

            byte[] fileBytes;
            await using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
                fileBytes = ms.ToArray();
            }

            var assetsDirectory = _configuration["Assets:Directory"];
            var thumbnailsRoot = _configuration["Thumbnails:OutputDirectory"];
            var thumbnailBaseUrl = _configuration["Thumbnails:ThumbnailUrl"];

            if (string.IsNullOrWhiteSpace(assetsDirectory))
                return StatusCode(500, "Assets directory is not configured.");
            if (string.IsNullOrWhiteSpace(thumbnailsRoot) || string.IsNullOrWhiteSpace(thumbnailBaseUrl))
                return StatusCode(500, "Thumbnail configuration is not configured.");

            try
            {
                var scheme = string.IsNullOrEmpty(Request.Scheme) ? "http" : Request.Scheme;
                var host = Request.Host.HasValue ? Request.Host.Value : "localhost";
                var baseUrl = $"{scheme}://{host}";
                var arbiterBaseUrl = _configuration["Arbiter:BaseUrl"];

                _ = await _pantsService.CreatePantsAsync(
                    connStr,
                    userId,
                    name,
                    file.FileName,
                    file.ContentType,
                    fileBytes,
                    assetsDirectory,
                    baseUrl,
                    thumbnailsRoot,
                    thumbnailBaseUrl ?? string.Empty,
                    _configuration["Assets:PublicBaseUrl"],
                    arbiterBaseUrl,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "Failed to save asset record.");
            }

            return Redirect("/develop?view=12");
        }

        [HttpPost("upload-shirt")]
        public async Task<IActionResult> UploadShirt([FromForm] string name, [FromForm] IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is required.");

            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Name is required.");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !long.TryParse(userIdClaim, out var userId) || userId <= 0)
                return Unauthorized("User must be logged in to upload assets.");

            var connStr = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
                return StatusCode(500, "Database connection string is not configured.");

            byte[] fileBytes;
            await using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
                fileBytes = ms.ToArray();
            }

            var assetsDirectory = _configuration["Assets:Directory"];
            var thumbnailsRoot = _configuration["Thumbnails:OutputDirectory"];
            var thumbnailBaseUrl = _configuration["Thumbnails:ThumbnailUrl"];

            if (string.IsNullOrWhiteSpace(assetsDirectory))
                return StatusCode(500, "Assets directory is not configured.");
            if (string.IsNullOrWhiteSpace(thumbnailsRoot) || string.IsNullOrWhiteSpace(thumbnailBaseUrl))
                return StatusCode(500, "Thumbnail configuration is not configured.");

            try
            {
                var scheme = string.IsNullOrEmpty(Request.Scheme) ? "http" : Request.Scheme;
                var host = Request.Host.HasValue ? Request.Host.Value : "localhost";
                var baseUrl = $"{scheme}://{host}";
                var arbiterBaseUrl = _configuration["Arbiter:BaseUrl"];

                _ = await _shirtService.CreateShirtAsync(
                    connStr,
                    userId,
                    name,
                    file.FileName,
                    file.ContentType,
                    fileBytes,
                    assetsDirectory,
                    baseUrl,
                    thumbnailsRoot,
                    thumbnailBaseUrl ?? string.Empty,
                    _configuration["Assets:PublicBaseUrl"],
                    arbiterBaseUrl,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "Failed to save asset record.");
            }

            return Redirect("/develop?view=11");
        }
    }
}
