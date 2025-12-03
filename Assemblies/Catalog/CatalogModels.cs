namespace RobloxWebserver.Assemblies.Catalog
{
    public class CatalogItem
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CreatorName { get; set; } = string.Empty;
        public long? CreatorId { get; set; }
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

    public class CatalogPageResult
    {
        public IReadOnlyList<CatalogItem> Items { get; set; } = Array.Empty<CatalogItem>();
        public int TotalItems { get; set; }
    }
}
