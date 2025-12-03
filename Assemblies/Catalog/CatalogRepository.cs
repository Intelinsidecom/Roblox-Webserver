using System.Threading.Tasks;

namespace RobloxWebserver.Assemblies.Catalog
{
    public sealed class CatalogRepository : ICatalogRepository
    {
        public Task<CatalogPageResult> GetItemsAsync(
            int category,
            int? subcategory,
            int pageNumber,
            int pageSize)
        {
            // Placeholder implementation for now; main T-shirt path uses direct SQL in CatalogService.
            var empty = new CatalogPageResult
            {
                Items = System.Array.Empty<CatalogItem>(),
                TotalItems = 0
            };

            return Task.FromResult(empty);
        }
    }
}
