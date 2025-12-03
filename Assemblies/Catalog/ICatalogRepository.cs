using System.Threading.Tasks;

namespace RobloxWebserver.Assemblies.Catalog
{
    public interface ICatalogRepository
    {
        Task<CatalogPageResult> GetItemsAsync(
            int category,
            int? subcategory,
            int pageNumber,
            int pageSize);
    }
}
