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
            var items = GetDummyItems();
            return View("~/Views/Pages/Catalog.cshtml", items);
        }

        [HttpGet("browse.aspx")]
        public async System.Threading.Tasks.Task<IActionResult> Browse(
            [FromQuery(Name = "Category")] int? category,
            [FromQuery(Name = "Subcategory")] int? subcategory,
            [FromQuery(Name = "Keyword")] string? keyword,
            [FromQuery(Name = "PageNumber")] int pageNumber = 1,
            [FromQuery(Name = "SortType")] int sortType = 0,
            [FromQuery(Name = "SortAggregation")] int sortAggregation = 5)
        {
            // Keyword search takes precedence over other special cases.
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var connStr = _configuration.GetConnectionString("Default") ?? string.Empty;
                var html = await CatalogSearchHelper.BuildSearchHtmlAsync(connStr, keyword, category ?? 0, 42);

                ViewBag.Category = category ?? 0;
                ViewBag.Subcategory = subcategory ?? 0;
                ViewBag.PageNumber = 1;
                ViewBag.TotalPages = 1;
                ViewBag.TotalItems = 42;
                ViewBag.SortType = sortType;
                ViewBag.SortAggregation = sortAggregation;
                ViewBag.Keyword = keyword;
                ViewBag.CatalogItemsHtml = html;

                return View("~/Views/Pages/catalog/browse.aspx.cshtml");
            }

            // Special-case: All Categories -> show all asset types using database-backed HTML builder
            if (category == 1)
            {
                var connStr = _configuration.GetConnectionString("Default") ?? string.Empty;
                var html = await AllCatalogHelper.BuildAllAssetsHtmlAsync(connStr, 42);

                ViewBag.Category = category ?? 0;
                ViewBag.Subcategory = subcategory ?? 0;
                ViewBag.PageNumber = 1;
                ViewBag.TotalPages = 1;
                ViewBag.TotalItems = 42;
                ViewBag.SortType = sortType;
                ViewBag.SortAggregation = sortAggregation;
                ViewBag.CatalogItemsHtml = html;

                return View("~/Views/Pages/catalog/browse.aspx.cshtml");
            }

            if (category == 3 && subcategory == 13)
            {
                var connStr = _configuration.GetConnectionString("Default") ?? string.Empty;
                var html = await _catalogService.BuildTShirtCatalogHtmlAsync(connStr, 42);

                ViewBag.Category = category ?? 0;
                ViewBag.Subcategory = subcategory ?? 0;
                ViewBag.PageNumber = 1;
                ViewBag.TotalPages = 1;
                ViewBag.TotalItems = 42;
                ViewBag.SortType = sortType;
                ViewBag.SortAggregation = sortAggregation;
                ViewBag.CatalogItemsHtml = html;

                return View("~/Views/Pages/catalog/browse.aspx.cshtml");
            }

            const int pageSize = 42;

            var allItems = GetCatalogItems(category, subcategory);

            // Basic server-side sorting for demo purposes
            allItems = sortType switch
            {
                5 => allItems
                    .OrderByDescending(i => i.PriceRobux ?? 0)
                    .ThenByDescending(i => i.PriceTickets ?? 0)
                    .ToList(),
                4 => allItems
                    .OrderBy(i => i.PriceRobux ?? int.MaxValue)
                    .ThenBy(i => i.PriceTickets ?? int.MaxValue)
                    .ToList(),
                3 => allItems
                    .OrderByDescending(i => i.UpdatedText)
                    .ToList(),
                2 => allItems
                    .OrderByDescending(i => i.Sales ?? 0)
                    .ToList(),
                1 => allItems
                    .OrderByDescending(i => i.FavoritedCount ?? 0)
                    .ToList(),
                _ => allItems
            };

            var totalItems = allItems.Count;
            var totalPages = totalItems == 0
                ? 1
                : (int)Math.Ceiling(totalItems / (double)pageSize);

            if (pageNumber < 1)
            {
                pageNumber = 1;
            }
            if (pageNumber > totalPages)
            {
                pageNumber = totalPages;
            }

            ViewBag.PageNumber = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.Category = category ?? 0;
            ViewBag.Subcategory = subcategory ?? 0;
            ViewBag.TotalItems = totalItems;
            ViewBag.SortType = sortType;
            ViewBag.SortAggregation = sortAggregation;

            var items = allItems
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return View("~/Views/Pages/catalog/browse.aspx.cshtml", items);
        }

        private static List<CatalogItemViewModel> GetDummyItems(int pageNumber = 1)
        {
            return new List<CatalogItemViewModel>
            {
                new CatalogItemViewModel
                {
                    Id = pageNumber,
                    Name = $"Dummy Fedora (Page {pageNumber})",
                    CreatorName = "ROBLOX",
                    ImageUrl = "/images/RobloxLogo.png",
                    OriginalPriceRobux = 500,
                    PriceRobux = 1250 + (pageNumber - 1) * 50,
                    IsLimited = true,
                    IsLimitedUnique = false,
                    IsNew = pageNumber == 1,
                    UpdatedText = "2 days ago",
                    Sales = 1000 * pageNumber,
                    FavoritedCount = 200 + 10 * pageNumber
                }
            };
        }

        private static List<CatalogItemViewModel> GetCatalogItems(int? category, int? subcategory)
        {
            // Clothing (3) + T-Shirts (13) should return a full list of T-Shirts.
            if (category == 3 && subcategory == 13)
            {
                var tShirts = new List<CatalogItemViewModel>();

                for (var i = 1; i <= 200; i++)
                {
                    tShirts.Add(new CatalogItemViewModel
                    {
                        Id = 1000 + i,
                        Name = $"Classic T-Shirt #{i}",
                        CreatorName = "ROBLOX",
                        ImageUrl = "/images/RobloxLogo.png",
                        PriceRobux = 5 + (i % 5),
                        OriginalPriceRobux = null,
                        IsLimited = false,
                        IsLimitedUnique = false,
                        IsNew = i <= 10,
                        UpdatedText = "recently",
                        Sales = 50 * i,
                        FavoritedCount = 10 * i
                    });
                }

                return tShirts;
            }

            // Fallback: show a small generic list if another category is requested.
            return new List<CatalogItemViewModel>
            {
                new CatalogItemViewModel
                {
                    Id = 1,
                    Name = "Sample Catalog Item",
                    CreatorName = "ROBLOX",
                    ImageUrl = "/images/RobloxLogo.png",
                    OriginalPriceRobux = 500,
                    PriceRobux = 1000,
                    IsLimited = true,
                    IsLimitedUnique = false,
                    IsNew = true,
                    UpdatedText = "2 days ago",
                    Sales = 1000,
                    FavoritedCount = 200
                }
            };
        }
    }
}
