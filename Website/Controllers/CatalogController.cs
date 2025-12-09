using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RobloxWebserver.Assemblies.Catalog;

namespace RobloxWebserver.Controllers
{
    [Route("catalog")]
    public class CatalogController : Controller
    {
        private readonly ICatalogService _catalogService;
        private readonly IConfiguration _configuration;

        public CatalogController(ICatalogService catalogService, IConfiguration configuration)
        {
            _catalogService = catalogService;
            _configuration = configuration;
        }

        public class CatalogItemViewModel
        {
            public long Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string CreatorName { get; set; } = string.Empty;
            public string ImageUrl { get; set; } = string.Empty;

            public int? PriceRobux { get; set; }
            public int? PriceTickets { get; set; }
            public int? OriginalPriceRobux { get; set; }

            public bool IsLimited { get; set; }
            public bool IsLimitedUnique { get; set; }
            public bool IsNew { get; set; }

            public string UpdatedText { get; set; } = string.Empty;
            public int? Sales { get; set; }
            public int? FavoritedCount { get; set; }
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
