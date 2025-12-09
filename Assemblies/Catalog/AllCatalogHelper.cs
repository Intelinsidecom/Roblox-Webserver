using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using Assets;

namespace RobloxWebserver.Assemblies.Catalog
{
    /// <summary>
    /// Helper for building catalog HTML that includes all asset types
    /// ("All Categories" in browse.aspx).
    /// </summary>
    public static class AllCatalogHelper
    {
        public static async Task<string> BuildAllAssetsHtmlAsync(string connectionString, int maxCount = 42, int? category = null, int? subcategory = null, IReadOnlyCollection<int>? genres = null)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));

            if (maxCount <= 0)
                maxCount = 42;

            var items = new List<CatalogItem>();

            using (var conn = new NpgsqlConnection(connectionString))
            {
                await conn.OpenAsync().ConfigureAwait(false);

                var sql = @"select a.asset_id,
       a.name,
       a.thumbnail_url,
       a.owner_user_id,
       u.user_name,
       a.last_updated,
       a.created_at,
       a.on_sale,
       a.price
from assets a
join users u on u.user_id = a.owner_user_id
where coalesce(a.asset_image, false) = false
  and a.on_sale = true";

                // Apply simple category-specific rules similar to CatalogSearchHelper
                if (category.HasValue)
                {
                    if (category.Value == 3)
                    {
                        // Clothing: restrict to T-Shirts for now (asset_type_id = 2)
                        sql += "\n  and a.asset_type_id = 2";
                    }
                    else if (category.Value == 4)
                    {
                        // Body Parts: explicitly exclude T-Shirts
                        sql += "\n  and a.asset_type_id <> 2";
                    }
                }

                if (genres != null && genres.Count > 0)
                {
                    sql += "\n  and a.genre = any(@genres)";
                }

                sql += "\norder by a.asset_id desc\nlimit @limit";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("limit", maxCount);

                    if (genres != null && genres.Count > 0)
                    {
                        cmd.Parameters.AddWithValue("genres", genres.ToArray());
                    }

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            var id = reader.GetInt64(0);
                            var name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                            var thumb = reader.IsDBNull(2) ? null : reader.GetString(2);
                            var ownerUserId = reader.GetInt64(3);
                            var creatorName = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
                            var lastUpdated = reader.IsDBNull(5)
                                ? DateTimeOffset.UtcNow
                                : reader.GetFieldValue<DateTimeOffset>(5);
                            var createdAt = reader.IsDBNull(6)
                                ? lastUpdated
                                : reader.GetFieldValue<DateTimeOffset>(6);
                            var price = reader.IsDBNull(8) ? (long?)null : reader.GetInt64(8);

                            items.Add(new CatalogItem
                            {
                                Id = id,
                                Name = name,
                                CreatorName = string.IsNullOrWhiteSpace(creatorName) ? "ROBLOX" : creatorName,
                                CreatorId = ownerUserId,
                                ImageUrl = string.IsNullOrWhiteSpace(thumb) ? "/images/RobloxLogo.png" : thumb,
                                PriceRobux = price.HasValue ? (int?)price.Value : null,
                                Sales = 0,
                                FavoritedCount = 0,
                                IsNew = AssetHelpers.IsNew(createdAt),
                                UpdatedText = AssetHelpers.GetFriendlyUpdatedText(lastUpdated)
                            });
                        }
                    }
                }
            }

            var page = new CatalogPageResult
            {
                Items = items,
                TotalItems = items.Count
            };

            // We can safely construct a temporary CatalogService here because it only
            // needs the HTML-building logic; the repository is unused for this path.
            var service = new CatalogService(new CatalogRepository());
            return service.BuildCatalogHtml(page);
        }
    }
}
