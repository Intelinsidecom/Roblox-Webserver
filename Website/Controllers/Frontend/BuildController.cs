using Microsoft.AspNetCore.Mvc;

namespace RobloxWebserver.Controllers
{
    // Minimal stub endpoints required by the front-end Build/Develop JavaScript.
    [ApiController]
    [Route("build")]
    public class BuildController : Controller
    {
        /// <summary>
        /// Legacy endpoint used by UniverseLoader.js.  Returns a very small HTML fragment so the
        /// loader considers the request successful.  Replace with real implementation once the
        /// universes feature is backed by database data.
        /// </summary>
        [HttpGet("universes")]
        public IActionResult Universes()
        {
            const string html = @"<table class='item-table' data-item-id='1' data-rootplace-id='1' data-type='universes'>
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
</table>";
            return Content(html, "text/html");
        }
    }
}
