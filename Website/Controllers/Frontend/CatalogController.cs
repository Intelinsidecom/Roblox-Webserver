using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Assets;
using RobloxWebserver.Assemblies.Catalog;
using RobloxWebserver.Assemblies.Economy;
using Users;
using Avatar;
using Website.Services;

namespace RobloxWebserver.Controllers
{
    [Route("catalog")]
    public class CatalogController : Controller
    {
        private readonly ICatalogService _catalogService;
        private readonly IConfiguration _configuration;
        private readonly ICatalogItemRenderer _catalogItemRenderer;
        private readonly AssetMetadataRepository _assetMetadataRepository;
        private readonly UserAssetsRepository _userAssetsRepository = new UserAssetsRepository();
        private readonly AssetsRepository _assetsRepository = new AssetsRepository();
        private readonly AvatarWornAssetsRepository _avatarWornAssetsRepository = new AvatarWornAssetsRepository();
        private readonly DevelopTabService _developTabService;

        public CatalogController(ICatalogService catalogService, IConfiguration configuration, DevelopTabService developTabService, ICatalogItemRenderer catalogItemRenderer)
        {
            _catalogService = catalogService;
            _configuration = configuration;
            _catalogItemRenderer = catalogItemRenderer;
            _assetMetadataRepository = new AssetMetadataRepository();
            _developTabService = developTabService ?? throw new ArgumentNullException(nameof(developTabService));
        }

        public class CatalogItemViewModel
        {
            public long Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string CreatorName { get; set; } = string.Empty;
            public long CreatorId { get; set; }
            public string ImageUrl { get; set; } = string.Empty;

            public int AssetTypeId { get; set; }
            public string AssetTypeLabel { get; set; } = string.Empty;

            public int? PriceRobux { get; set; }
            public int? PriceTickets { get; set; }
            public int? OriginalPriceRobux { get; set; }

            public bool IsLimited { get; set; }
            public bool IsLimitedUnique { get; set; }
            public bool IsNew { get; set; }

            public string UpdatedText { get; set; } = string.Empty;
            public int? Sales { get; set; }
            public int? FavoritedCount { get; set; }
            public string Description { get; set; } = string.Empty;
            public int GenreId { get; set; }
            public string GenreLabel { get; set; } = string.Empty;
            public long UserRobuxBalance { get; set; }
            public bool AllowComments { get; set; }
            public bool IsOwned { get; set; }
            public bool IsFavorited { get; set; }
            public bool IsWorn { get; set; }
            public bool IsOnSale { get; set; }
            public long ItemVersionId { get; set; }

            public long? LimitedQuantity { get; set; }
            public long? LimitedRemaining { get; set; }
            public DateTime? LimitedUntil { get; set; }
            public long RecentAveragePrice { get; set; }
            public long? OwnedSerialNumber { get; set; }
            public int ResellerCount { get; set; }
            public long BestResalePrice { get; set; }
            public int UserMembershipLevel { get; set; }
        }

        [HttpGet("{id:long}")]
        [HttpGet("{id:long}/{itemName}")]
        public async Task<IActionResult> Item(long id, string? itemName, CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            var connectionString = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return StatusCode(500, "Database connection string is not configured.");
            }

            var asset = await _assetMetadataRepository.GetAssetByIdAsync(connectionString, id).ConfigureAwait(false);
            if (asset == null)
            {
                return NotFound();
            }

            var expectedSlug = ToSlug(asset.Name);
            if (!string.Equals(itemName ?? "", expectedSlug, StringComparison.OrdinalIgnoreCase))
            {
                return RedirectPermanentPreserveMethod("/catalog/" + id + "/" + expectedSlug);
            }

            int? favoritedCount = null;
            try
            {
                var count = await _assetsRepository.GetFavoriteCountAsync(connectionString, asset.AssetId).ConfigureAwait(false);
                favoritedCount = count;
            }
            catch
            {
                favoritedCount = null;
            }

            var creatorName = string.Empty;
            if (asset.OwnerUserId > 0)
            {
                var name = await UserQueries.GetUserNameByIdAsync(connectionString, asset.OwnerUserId).ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    creatorName = name;
                }
            }

            long userRobux = 0;
            bool isOwned = false;
            bool isFavorited = false;
            bool isWorn = false;
            int userMembershipLevel = 0;
            long currentUserId = 0;
            var userIdClaim = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(userIdClaim) && long.TryParse(userIdClaim, out var parsedUserId) && parsedUserId > 0)
            {
                currentUserId = parsedUserId;
                try
                {
                    userRobux = await UserQueries.GetCurrencyByIdAsync(connectionString, currentUserId, "robux").ConfigureAwait(false);
                    isOwned = await _userAssetsRepository.UserOwnsAssetAsync(connectionString, currentUserId, asset.AssetId).ConfigureAwait(false);
                    isFavorited = await _assetsRepository.UserHasFavoritedAsync(connectionString, currentUserId, asset.AssetId).ConfigureAwait(false);

                    var profileData = await UserQueries.GetUserProfileDataAsync(connectionString, currentUserId).ConfigureAwait(false);
                    if (profileData != null)
                    {
                        var membershipStatus = profileData.GetValueOrDefault("membershipStatus") as short? ?? 0;
                        userMembershipLevel = membershipStatus switch
                        {
                            1 => 1,
                            2 => 2,
                            3 => 11,
                            _ => 0
                        };
                    }

                    if (isOwned)
                    {
                        var wornIds = await _avatarWornAssetsRepository.GetWornAssetIdsAsync(connectionString, currentUserId).ConfigureAwait(false);
                        isWorn = wornIds != null && System.Array.IndexOf(wornIds, asset.AssetId) >= 0;
                    }
                }
                catch
                {
                    userRobux = 0;
                    isOwned = false;
                    isFavorited = false;
                }
            }

            var primaryThumb = string.IsNullOrWhiteSpace(asset.HighResThumbnailUrl)
                ? asset.ThumbnailUrl
                : asset.HighResThumbnailUrl;

            long itemVersionId = 0;
            try
            {
                await using var conn = new NpgsqlConnection(connectionString);
                await conn.OpenAsync().ConfigureAwait(false);
                await using var cmd = new NpgsqlCommand("select extract(epoch from last_updated)::bigint from assets where asset_id = @id", conn);
                cmd.Parameters.AddWithValue("id", id);
                var result = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                itemVersionId = result is long l ? l : 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Item({id}) fetch last_updated: {ex}");
                itemVersionId = 0;
            }

            var isLimited = asset.LimitedUnique || asset.LimitedQuantity.HasValue;
            long bestResalePrice = 0;
            int resellerCount = 0;
            long? ownedSerial = null;

            if (isLimited)
            {
                try
                {
                    var limitedService = new Economy.LimitedItemService();
                    var limitedData = await limitedService.GetLimitedDataAsync(connectionString, asset.AssetId, cancellationToken).ConfigureAwait(false);
                    if (limitedData != null)
                    {
                        asset.LimitedQuantity = limitedData.Quantity;
                        asset.LimitedRemaining = limitedData.Remaining;
                        asset.LimitedUntil = limitedData.Until;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Item({id}) fetch limited data: {ex}");
                }

                try
                {
                    var resaleService = new Economy.ResaleListingService(new Economy.MarketplaceFeeService(_configuration));
                    await using var conn2 = new NpgsqlConnection(connectionString);
                    await conn2.OpenAsync(cancellationToken).ConfigureAwait(false);

                    var cheapest = await resaleService.GetCheapestListingAsync(conn2, asset.AssetId, cancellationToken).ConfigureAwait(false);
                    if (cheapest != null)
                        bestResalePrice = cheapest.Price;

                    resellerCount = await resaleService.GetResellerCountAsync(conn2, asset.AssetId, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Item({id}) fetch resale listings: {ex}");
                }

                if (currentUserId > 0)
                {
                    try
                    {
                        var limitedService2 = new Economy.LimitedItemService();
                        ownedSerial = await limitedService2.GetUserSerialAsync(connectionString, asset.AssetId, currentUserId, cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] Item({id}) fetch user serial: {ex}");
                    }
                }
            }

            int? displayPrice = null;
            if (isLimited && bestResalePrice > 0)
            {
                displayPrice = (int)Math.Min(bestResalePrice, int.MaxValue);
            }
            else if (asset.OnSale || asset.IsCopyingAllowed)
            {
                displayPrice = asset.IsCopyingAllowed ? 0 : (int?)Math.Min(asset.Price, int.MaxValue);
            }

            var model = new CatalogItemViewModel
            {
                Id = asset.AssetId,
                Name = asset.Name,
                CreatorName = string.IsNullOrWhiteSpace(creatorName) ? "ROBLOX" : creatorName,
                CreatorId = asset.OwnerUserId,
                ImageUrl = string.IsNullOrWhiteSpace(primaryThumb) ? (_configuration["DefaultThumbnailUrl"] ?? "/images/default.png") : primaryThumb,
                AssetTypeId = asset.AssetTypeId,
                AssetTypeLabel = AssetTypeNames.GetTypeName(asset.AssetTypeId),
                PriceRobux = displayPrice,
                PriceTickets = (asset.OnSale || asset.IsCopyingAllowed) ? (int?)(asset.IsCopyingAllowed ? 0 : Math.Min(asset.PriceTickets, int.MaxValue)) : null,
                OriginalPriceRobux = (int?)Math.Min(asset.Price, int.MaxValue),
                IsLimited = isLimited,
                IsLimitedUnique = asset.LimitedUnique,
                IsNew = false,
                UpdatedText = string.Empty,
                Sales = null,
                FavoritedCount = favoritedCount,
                Description = asset.Description ?? string.Empty,
                GenreId = asset.Genre,
                GenreLabel = AssetGenreNames.GetGenreLabel(asset.Genre),
                UserRobuxBalance = userRobux,
                AllowComments = asset.AllowComments,
                IsOwned = isOwned,
                IsFavorited = isFavorited,
                IsWorn = isWorn,
                IsOnSale = asset.OnSale || asset.IsCopyingAllowed,
                ItemVersionId = itemVersionId,
                LimitedQuantity = asset.LimitedQuantity,
                LimitedRemaining = asset.LimitedRemaining,
                LimitedUntil = asset.LimitedUntil,
                RecentAveragePrice = asset.RecentAveragePrice,
                OwnedSerialNumber = ownedSerial,
                ResellerCount = resellerCount,
                BestResalePrice = bestResalePrice,
                UserMembershipLevel = userMembershipLevel
            };

            return View("~/Views/Pages/catalog/{id}/{ItemName}.cshtml", model);
        }

        public static string ToSlug(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            name = name.Trim().ToLowerInvariant();

            var chars = new System.Text.StringBuilder(name.Length);
            bool lastWasHyphen = false;

            foreach (var ch in name)
            {
                if ((ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9'))
                {
                    chars.Append(ch);
                    lastWasHyphen = false;
                }
                else if (ch == ' ' || ch == '-' || ch == '_' || ch == '.')
                {
                    if (!lastWasHyphen)
                    {
                        chars.Append('-');
                        lastWasHyphen = true;
                    }
                }
            }

            var result = chars.ToString().Trim('-');
            return string.IsNullOrEmpty(result) ? string.Empty : result;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var connectionString = _configuration.GetConnectionString("Default") ?? string.Empty;
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return StatusCode(500, "Database connection string is not configured.");
            }

            var excludeNonCatalog = _configuration.GetValue<bool>("Catalog:ExcludeNonCatalogTypes");

            var featuredHtml = await CatalogFiltering.BuildFeaturedItemsHtmlAsync(connectionString, _catalogItemRenderer);
            var popularHtml = await CatalogFiltering.BuildPopularItemsHtmlAsync(connectionString, 42, excludeNonCatalog, _catalogItemRenderer);

            ViewBag.FeaturedItemsHtml = featuredHtml;
            ViewBag.PopularItemsHtml = popularHtml;

            return View("~/Views/Pages/Catalog.cshtml");
        }

        [HttpGet("browse.aspx")]
        public async System.Threading.Tasks.Task<IActionResult> Browse(
            [FromQuery(Name = "Category")] int? category,
            [FromQuery(Name = "Subcategory")] int? subcategory,
            [FromQuery(Name = "Genre")] int? genre,
            [FromQuery(Name = "Genres")] string[]? genres,
            [FromQuery(Name = "Keyword")] string? keyword,
            [FromQuery(Name = "PageNumber")] int pageNumber = 1,
            [FromQuery(Name = "SortType")] int sortType = 0,
            [FromQuery(Name = "SortAggregation")] int sortAggregation = 5)
        {
            var combinedGenres = new List<int>();

            // Always read raw Genres values directly from the query string to ensure
            // that the filter is applied even if model binding behaves unexpectedly.
            var rawGenreParams = Request.Query["Genres"];
            if (rawGenreParams.Count > 0)
            {
                foreach (var raw in rawGenreParams)
                {
                    if (string.IsNullOrWhiteSpace(raw))
                    {
                        continue;
                    }

                    if (!int.TryParse(raw, out var parsed))
                    {
                        continue;
                    }

                    if (parsed <= 0)
                    {
                        continue;
                    }

                    if (!combinedGenres.Contains(parsed))
                    {
                        combinedGenres.Add(parsed);
                    }
                }
            }

            if (genre.HasValue && genre.Value > 0 && !combinedGenres.Contains(genre.Value))
            {
                combinedGenres.Add(genre.Value);
            }

            var effectiveGenres = combinedGenres.Count > 0 ? combinedGenres : null;

            // Keyword search takes precedence over other special cases.
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var connStr = _configuration.GetConnectionString("Default") ?? string.Empty;
                var html = await CatalogSearchHelper.BuildSearchHtmlAsync(connStr, keyword, category ?? 0, effectiveGenres, 42, _catalogItemRenderer);

                ViewBag.Category = category ?? 0;
                ViewBag.Subcategory = subcategory ?? 0;
                ViewBag.Genres = effectiveGenres;
                ViewBag.Genre = effectiveGenres != null && effectiveGenres.Count > 0 ? effectiveGenres[0] : 0;
                ViewBag.PageNumber = 1;
                ViewBag.TotalPages = 1;
                ViewBag.TotalItems = 42;
                ViewBag.SortType = sortType;
                ViewBag.SortAggregation = sortAggregation;
                ViewBag.Keyword = keyword;
                ViewBag.CatalogItemsHtml = html;

                return View("~/Views/Pages/catalog/browse.aspx.cshtml");
            }

            // All non-keyword browse paths currently use a shared HTML builder that pulls
            // real items from the database and excludes generated image assets.
            // This ensures that all categories show real items instead of the previous
            // hard-coded example item list.

            var connectionString = _configuration.GetConnectionString("Default") ?? string.Empty;
            var allHtml = await AllCatalogHelper.BuildAllAssetsHtmlAsync(connectionString, 42, category, subcategory, effectiveGenres, _catalogItemRenderer);

            ViewBag.PageNumber = 1;
            ViewBag.TotalPages = 1;
            ViewBag.Category = category ?? 0;
            ViewBag.Subcategory = subcategory ?? 0;
            ViewBag.Genres = effectiveGenres;
            ViewBag.Genre = effectiveGenres != null && effectiveGenres.Count > 0 ? effectiveGenres[0] : 0;
            ViewBag.TotalItems = 42;
            ViewBag.SortType = sortType;
            ViewBag.SortAggregation = sortAggregation;
            ViewBag.CatalogItemsHtml = allHtml;

            return View("~/Views/Pages/catalog/browse.aspx.cshtml");
        }

        [HttpGet("contents")]
        [Authorize]
        public async Task<IActionResult> Contents(
            [FromQuery(Name = "SortType")] int sortType = 0,
            [FromQuery(Name = "Category")] int category = 0,
            [FromQuery(Name = "Genres")] string[]? genres = null,
            [FromQuery(Name = "Keyword")] string? keyword = null,
            [FromQuery(Name = "PageNumber")] int pageNumber = 1,
            CancellationToken cancellationToken = default)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !long.TryParse(userIdClaim, out var userId) || userId <= 0)
            {
                return Unauthorized();
            }

            var nameClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

            var genreList = genres?
                .SelectMany(g => g.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(s => int.TryParse(s, out var id) ? id : (int?)null)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            var vm = await _developTabService.BuildAsync(userId, nameClaim, "Library",
                category: category > 0 ? category : null,
                sortType: sortType,
                genres: genreList,
                keyword: keyword,
                pageNumber: pageNumber,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            return PartialView("~/Views/Develop/Tabs/Library.cshtml", vm);
        }
    }
}
