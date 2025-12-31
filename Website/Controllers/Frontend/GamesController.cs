using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Games;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Users;

namespace RobloxWebserver.Controllers
{
    /// <summary>
    /// Minimal endpoints backing game/universe creation from the legacy /develop page.
    /// This currently creates a universe and a single root place asset, then redirects
    /// to a placeholder configure page for that universe.
    /// </summary>
    [ApiController]
    [Route("games")]
    [Authorize]
    public sealed class GamesController : Controller
    {
        private readonly IConfiguration _configuration;

        public GamesController(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Handles the "Create New Game" button from /develop. For now this immediately creates
        /// a universe and root place row in the database and then redirects the user to a
        /// not-yet-implemented universe configuration page.
        /// </summary>
        [HttpGet("create")]
        public async Task<IActionResult> Create(CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !long.TryParse(userIdClaim, out var userId) || userId <= 0)
            {
                return Redirect("/");
            }

            var connStr = _configuration.GetConnectionString("Default");
            var assetsRoot = _configuration["Assets:Directory"];
            var starterPlacePath = _configuration["Games:StarterPlacePath"];
            var enableCooldownRaw = _configuration["Games:EnableCreationCooldown"];
            var enableCooldown = true;
            if (!string.IsNullOrWhiteSpace(enableCooldownRaw) && bool.TryParse(enableCooldownRaw, out var parsedCooldown))
            {
                enableCooldown = parsedCooldown;
            }
            UniverseInfo universe;
            try
            {
                var creatorUserName = await UserQueries.GetUserNameByIdAsync(connStr, userId, cancellationToken)
                    .ConfigureAwait(false) ?? User.Identity?.Name ?? string.Empty;

                universe = await GameCreationService.CreateUniverseWithRootPlaceAsync(
                    connStr,
                    userId,
                    creatorUserName,
                    assetsRoot,
                    starterPlacePath,
                    enableCooldown,
                    cancellationToken);
            }
            catch
            {
                // If anything fails, fall back to the develop page for now.
                return Redirect("/develop?Page=universes");
            }

            // In real Roblox this would go to /universes/configure?id={universeId}.
            // That endpoint does not exist yet in this clone, so we redirect back
            // to the universes view where the new row can eventually appear once
            // listing is wired up.
            return Redirect("/develop?Page=universes");
        }
    }
}
