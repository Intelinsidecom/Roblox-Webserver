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

        public DevelopController(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Legacy endpoint expected by BuildPage.js.  Returns an HTML fragment that is injected
        /// into the #MyCreationsTab .items-container element via jQuery .load().
        /// The real Roblox implementation is quite elaborate; for now we return a very small
        /// static fragment so that the page no longer appears empty and the JavaScript callbacks
        /// succeed.  Replace with real data once the games database is implemented.
        /// </summary>
        [HttpGet("games-list")]
        public IActionResult GamesList()
        {
            const string html = @"<a href='https://web.roblox.com/places/create' id='CreatePlace' class='create-new-button btn-medium btn-primary'>Create New Game</a>
<table class='section-header'>
    <tr>
        <td class='content-title'>
            <div>
                <h2 class='header-text'>Games</h2>
                <span class='aside-text' data-active-count='1' data-max-active-count='200'></span>
                <label class='checkbox-label active-only-checkbox'>
                    <input type='checkbox' />Show Public Only
                </label>
            </div>
        </td>
    </tr>
</table>
<div class='items-container-inner'>
    <table class='item-table' data-item-id='1' data-rootplace-id='1' data-type='universes'>
        <tr>
            <td class='image-col universe-image-col' style='text-align:center;'>
                <a href='/universes/configure?id=1' class='game-image'>
                    <img src='https://t7.rbxcdn.com/33d7dc233c34b71e4cb3cfe0b98f92f4' alt='My First Game' />
                </a>
            </td>
            <td class='universe-name-col'>
                <a class='title notranslate' href='/universes/configure?id=1'>My First Game</a>
                <table class='details-table'>
                    <tr>
                        <td class='item-universe'>
                            <span>Start Place:</span>
                            <a class='title notranslate start-place-url' href='/games/1/My-First-Game'>My First Game</a>
                        </td>
                    </tr>
                    <tr class='activate-cell'>
                        <td>
                            <a class='place-active' href='/universes/configure?id=1'>Public</a>
                        </td>
                    </tr>
                </table>
            </td>
            <td class='edit-col'>
                <a class='roblox-edit-button btn-control btn-control-large' href='javascript:'>Edit</a>
            </td>
            <td class='menu-col'>
                <div class='gear-button-wrapper'>
                    <a href='#' class='gear-button'></a>
                </div>
            </td>
        </tr>
    </table>
    <div class='separator'></div>
</div>";
            // Return as text/html so jQuery .load() inserts raw DOM into the empty items-container
            return Content(html, "text/html");
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
            // Treat assetTypeId 0 as universes/games; return the rich Upload New {assetName} fragment
            if (assetTypeId == 0)
            {
                const string assetName = "Games";

                var html = $@"<a href='#' class='create-new-button btn-medium btn-primary'>Upload New {assetName}</a>
<table class='section-header'>
    <tr>
        <td class='content-title'>
            <div>
                <h2 class='header-text'>{assetName}</h2>
                <label class='checkbox-label active-only-checkbox'>
                    <input type='checkbox'>Show Public Only
                </label>
            </div>
        </td>
    </tr>
</table>
<div class='items-container'>
    <div class='no-assets-text'>You have no {assetName} yet. Upload or create one to see it here!</div>
    <div class='separator'></div>
</div>";
                return Content(html, "text/html");
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
join assets t on t.asset_id = ua_t.asset_id and t.asset_type_id = 2
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

                                sb.Append(@"            <table class='item-table' data-item-id='");
                                sb.Append(assetId);
                                sb.Append(@"' data-type='tshirts'>");
                                sb.Append(@"                <tr>");
                                sb.Append(@"                    <td class='image-col universe-image-col' style='text-align:center'>");
                                sb.Append(@"                        <a href='#' class='game-image'> <img src='");
                                sb.Append(System.Net.WebUtility.HtmlEncode(thumbUrl ?? "https://t7.rbxcdn.com/6bfa4d3e4d38a70d2f5b493987fe29c4"));
                                sb.Append(@"' alt='T-Shirt'> </a>");
                                sb.Append(@"                    </td>");
                                sb.Append(@"                    <td class='universe-name-col'>");
                                sb.Append(@"                        <a class='title notranslate' href='#'>");
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
    }
}
