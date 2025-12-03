using System.Threading.Tasks;

namespace RobloxWebserver.Assemblies.Catalog
{
    public interface ICatalogService
    {
        Task<CatalogPageResult> GetItemsAsync(
            int category,
            int? subcategory,
            int pageNumber,
            int pageSize);

        string BuildCatalogHtml(CatalogPageResult pageResult);

        Task<string> BuildTShirtCatalogHtmlAsync(string connectionString, int maxCount = 42);
    }
}
