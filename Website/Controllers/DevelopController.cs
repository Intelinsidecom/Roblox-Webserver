using Microsoft.AspNetCore.Mvc;

namespace RobloxWebserver.Controllers
{
    // Handles endpoints used by the legacy /develop page JavaScript
    [ApiController]
    [Route("develop")]
    public class DevelopController : Controller
    {
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
        /// Returns a simple placeholder fragment for now based on asset type id.
        /// </summary>
        [HttpGet("asset-list/{assetTypeId:int}")]
        public IActionResult AssetList(int assetTypeId)
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

            // For all other asset types, just return simple text
            return Content("hello", "text/html");
        }
    }
}
