using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Assets;
using RobloxWebserver.Assemblies.Catalog;
using Users;

namespace RobloxWebserver.Controllers
{
    [Route("catalog")]
    public class CatalogController : Controller
    {
        private readonly ICatalogService _catalogService;
        private readonly IConfiguration _configuration;
        private readonly AssetMetadataRepository _assetMetadataRepository;
        private readonly UserAssetsRepository _userAssetsRepository = new UserAssetsRepository();
        private readonly AssetsRepository _assetsRepository = new AssetsRepository();

        public CatalogController(ICatalogService catalogService, IConfiguration configuration)
        {
            _catalogService = catalogService;
            _configuration = configuration;
            _assetMetadataRepository = new AssetMetadataRepository();
        }

        public class CatalogItemViewModel
        {
            public long Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string CreatorName { get; set; } = string.Empty;
            public long CreatorId { get; set; }
            public string ImageUrl { get; set; } = string.Empty;

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
        }

        [HttpGet("{id:long}/{itemName}")]
        public async Task<IActionResult> Item(long id, string itemName)
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

            var expectedSlug = ToSlug(asset.Name);
            if (!string.Equals(itemName, expectedSlug, StringComparison.OrdinalIgnoreCase))
            {
                return NotFound();
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
            var userIdClaim = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(userIdClaim) && long.TryParse(userIdClaim, out var currentUserId) && currentUserId > 0)
            {
                try
                {
                    userRobux = await UserQueries.GetCurrencyByIdAsync(connectionString, currentUserId, "robux").ConfigureAwait(false);
                    isOwned = await _userAssetsRepository.UserOwnsAssetAsync(connectionString, currentUserId, asset.AssetId).ConfigureAwait(false);
                    isFavorited = await _assetsRepository.UserHasFavoritedAsync(connectionString, currentUserId, asset.AssetId).ConfigureAwait(false);
                }
                catch
                {
                    userRobux = 0;
                    isOwned = false;
                    isFavorited = false;
                }
            }

            var model = new CatalogItemViewModel
            {
                Id = asset.AssetId,
                Name = asset.Name,
                CreatorName = string.IsNullOrWhiteSpace(creatorName) ? "ROBLOX" : creatorName,
                CreatorId = asset.OwnerUserId,
                ImageUrl = string.IsNullOrWhiteSpace(asset.ThumbnailUrl) ? "/images/RobloxLogo.png" : asset.ThumbnailUrl,
                AssetTypeLabel = AssetTypeNames.GetTypeName(asset.AssetTypeId),
                PriceRobux = asset.OnSale ? (int?)Math.Min(asset.Price, int.MaxValue) : null,
                PriceTickets = null,
                OriginalPriceRobux = null,
                IsLimited = false,
                IsLimitedUnique = false,
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
                IsFavorited = isFavorited
            };

            return View("~/Views/Pages/catalog/{id}/{ItemName}.cshtml", model);
        }

        private static string ToSlug(string name)
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
        public IActionResult Index()
        {
            return RedirectToAction("Browse");
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
                var html = await CatalogSearchHelper.BuildSearchHtmlAsync(connStr, keyword, category ?? 0, effectiveGenres, 42);

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
            var allHtml = await AllCatalogHelper.BuildAllAssetsHtmlAsync(connectionString, 42, category, subcategory, effectiveGenres);

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
    }
}
