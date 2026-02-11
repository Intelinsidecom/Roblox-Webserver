using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Games;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Thumbnails;
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
        private readonly IThumbnailService _thumbnailService;

        public GamesController(IConfiguration configuration, IThumbnailService thumbnailService)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _thumbnailService = thumbnailService ?? throw new ArgumentNullException(nameof(thumbnailService));
        }

        /// <summary>
        /// Handles the "Create New Game" button from /develop. For now this immediately creates
        /// a universe and root place row in the database and then redirects user to a
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
                    _thumbnailService,
                    _configuration,
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

        /// <summary>
        /// Returns HTML game cards for the games page. This endpoint is called via AJAX
        /// to load more games dynamically as users scroll or interact with filters.
        /// </summary>
        [HttpGet("moreresultscached")]
        [AllowAnonymous]
        public IActionResult MoreResultsCached(
            int SortFilter = 1,
            int TimeFilter = 0,
            int RegionFilter = 183,
            int GenreID = 1,
            int GameFilter = 1,
            int MinBCLevel = 0,
            int StartRows = 0,
            int MaxRows = 14,
            bool IsUserLoggedIn = false,
            int NumberOfRowsToOccupy = 1,
            int NumberOfColumns = 4,
            bool IsInHorizontalScrollMode = false,
            int DeviceTypeId = 1,
            int AdSpan = 0,
            int AdAlignment = 0,
            int v = 2,
            int PersonalizedUniverseId = 0,
            bool useFakeResults = false,
            bool IsGamesThumbnailAsyncEnabled = false)
        {
            // Generate sample game data for testing
            var sampleGames = GenerateSampleGames(SortFilter, GenreID, StartRows, MaxRows);

            // Build HTML response
            var html = BuildGamesHtml(sampleGames, StartRows);

            return Content(html, "text/html");
        }

        private List<SampleGame> GenerateSampleGames(int sortFilter, int genreId, int startRow, int maxRows)
        {
            var games = new List<SampleGame>();
            var random = new Random();

            // Sample game data covering different genres
            var gameTemplates = new[]
            {
                new { Name = "Epic Adventure Quest", Creator = "GameMaster123", Genre = "Adventure", Playing = random.Next(100, 5000), UpVotes = random.Next(1000, 50000), DownVotes = random.Next(100, 2000) },
                new { Name = "Tycoon Empire", Creator = "BuilderPro", Genre = "Building", Playing = random.Next(50, 2000), UpVotes = random.Next(500, 25000), DownVotes = random.Next(50, 1000) },
                new { Name = "Battle Arena", Creator = "FighterFan", Genre = "Fighting", Playing = random.Next(200, 8000), UpVotes = random.Next(2000, 80000), DownVotes = random.Next(200, 4000) },
                new { Name = "Space Station Survival", Creator = "SciFiMaker", Genre = "Sci-Fi", Playing = random.Next(100, 3000), UpVotes = random.Next(1500, 45000), DownVotes = random.Next(150, 3000) },
                new { Name = "Medieval Kingdom", Creator = "HistoryBuff", Genre = "Medieval", Playing = random.Next(80, 2500), UpVotes = random.Next(800, 30000), DownVotes = random.Next(80, 1600) },
                new { Name = "Speed Racing Championship", Creator = "RacerX", Genre = "Sports", Playing = random.Next(150, 4000), UpVotes = random.Next(1200, 35000), DownVotes = random.Next(120, 2400) },
                new { Name = "City Life Roleplay", Creator = "CityBuilder", Genre = "Town and City", Playing = random.Next(300, 6000), UpVotes = random.Next(2500, 60000), DownVotes = random.Next(250, 5000) },
                new { Name = "Western Outlaw", Creator = "CowboyKid", Genre = "Western", Playing = random.Next(60, 1500), UpVotes = random.Next(600, 20000), DownVotes = random.Next(60, 1200) },
                new { Name = "Navy Battles", Creator = "SailorMan", Genre = "Naval", Playing = random.Next(90, 2200), UpVotes = random.Next(900, 28000), DownVotes = random.Next(90, 1800) },
                new { Name = "FPS Combat Zone", Creator = "ShooterPro", Genre = "FPS", Playing = random.Next(400, 10000), UpVotes = random.Next(3000, 90000), DownVotes = random.Next(300, 6000) },
                new { Name = "Horror Mansion", Creator = "ScaryGames", Genre = "Horror", Playing = random.Next(120, 3500), UpVotes = random.Next(1800, 42000), DownVotes = random.Next(180, 3600) },
                new { Name = "Comedy Club", Creator = "FunnyGuy", Genre = "Comedy", Playing = random.Next(40, 1200), UpVotes = random.Next(400, 15000), DownVotes = random.Next(40, 800) },
                new { Name = "Military Base", Creator = "ArmyStrong", Genre = "Military", Playing = random.Next(180, 4500), UpVotes = random.Next(1600, 48000), DownVotes = random.Next(160, 3200) },
                new { Name = "RPG Quest", Creator = "FantasyFan", Genre = "RPG", Playing = random.Next(250, 7000), UpVotes = random.Next(2200, 65000), DownVotes = random.Next(220, 4400) }
            };

            // Generate games based on requested count
            for (int i = 0; i < maxRows; i++)
            {
                var template = gameTemplates[random.Next(gameTemplates.Length)];
                var position = startRow + i;
                var placeId = 1000000 + position; // Generate fake place IDs

                games.Add(new SampleGame
                {
                    Name = template.Name,
                    Creator = template.Creator,
                    Playing = template.Playing,
                    UpVotes = template.UpVotes,
                    DownVotes = template.DownVotes,
                    PlaceId = placeId,
                    Position = position,
                    ThumbnailUrl = $"https://t{random.Next(0, 7)}.rbxcdn.com/{GenerateRandomString(32)}"
                });
            }

            return games;
        }

        private string BuildGamesHtml(List<SampleGame> games, int startRow)
        {
            var html = "<div class=\"hidden-item hidden\" id=keyword></div>";

            foreach (var game in games)
            {
                html += $@"
<li class=""list-item game-card"">
    <div class=game-card-container>
        <a href=""/games/refer?SortFilter=8&TimeFilter=0&GenreFilter=1&RegionFilter=183&PlaceId={game.PlaceId}&Position={game.Position}&PageType=Games"" class=game-card-link>
            <div class=game-card-thumb-container>
                <img class=game-card-thumb src={game.ThumbnailUrl} alt=""{game.Name}"" thumbnail=""{{&#34;Final&#34;:true,&#34;Url&#34;:&#34;{game.ThumbnailUrl}&#34;,&#34;RetryUrl&#34;:null,&#34;UserId&#34;:0,&#34;EndpointType&#34;:&#34;Avatar&#34;}}"" image-retry>
            </div>
            <div class=""text-overflow game-card-name"" title=""{game.Name}"" ng-non-bindable>{game.Name}</div>
            <div class=game-card-name-secondary>{game.Playing} Playing</div>
            <div class=game-card-experimental>
                <span class=icon-experimental-gray2></span>
                <span class=experimental-label-long>Experimental Mode</span>
                <span class=experimental-label-short>Experimental</span>
            </div>
            <div class=game-card-vote>
                <div class=vote-bar data-voting-processed=false>
                    <div class=vote-thumbs-up>
                        <span class=icon-like-gray-16x16></span>
                    </div>
                    <div class=vote-container data-upvotes={game.UpVotes} data-downvotes={game.DownVotes}>
                        <div id=vote-background class=vote-background></div>
                        <div class=vote-percentage></div>
                        <div class=vote-mask>
                            <div class=""segment seg-1""></div>
                            <div class=""segment seg-2""></div>
                            <div class=""segment seg-3""></div>
                            <div class=""segment seg-4""></div>
                        </div>
                    </div>
                    <div class=vote-thumbs-down>
                        <span class=icon-dislike-gray-16x16></span>
                    </div>
                </div>
                <div class=vote-counts>
                    <div id=vote-down-count class=vote-down-count>{game.DownVotes:N0}</div>
                    <div id=vote-up-count class=vote-up-count>{game.UpVotes:N0}</div>
                </div>
            </div>
        </a>
        <div class=game-card-footer>
            <div class=creator>
                <span class=""text-label xsmall text-overflow""> By <a class=text-link href=""/users/{game.PlaceId}/profile"" ng-non-bindable>{game.Creator}</a> </span>
            </div>
            <div class=game-card-experimental>
                <span class=icon-experimental-gray2></span>
                <span class=experimental-label-long>Experimental Mode</span>
            </div>
        </div>
    </div>
</li>";
            }

            return html;
        }

        private string GenerateRandomString(int length)
        {
            const string chars = "abcdef0123456789";
            var random = new Random();
            var result = new char[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = chars[random.Next(chars.Length)];
            }
            return new string(result);
        }

        private class SampleGame
        {
            public string Name { get; set; } = string.Empty;
            public string Creator { get; set; } = string.Empty;
            public int Playing { get; set; }
            public int UpVotes { get; set; }
            public int DownVotes { get; set; }
            public long PlaceId { get; set; }
            public int Position { get; set; }
            public string ThumbnailUrl { get; set; } = string.Empty;
        }
    }
}
