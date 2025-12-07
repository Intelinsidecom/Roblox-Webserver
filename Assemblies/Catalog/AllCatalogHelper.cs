using System;
using System.Collections.Generic;
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
        public static async Task<string> BuildAllAssetsHtmlAsync(string connectionString, int maxCount = 42)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));

            if (maxCount <= 0)
                maxCount = 42;

            var items = new List<CatalogItem>();

            using (var conn = new NpgsqlConnection(connectionString))
            {
                await conn.OpenAsync().ConfigureAwait(false);

                const string sql = @"select a.asset_id,
       a.name,
       a.thumbnail_url,
       a.owner_user_id,
       u.user_name,
       a.last_updated,
       a.created_at
from assets a
join users u on u.user_id = a.owner_user_id
where coalesce(a.asset_image, false) = false
order by a.asset_id desc
limit @limit";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("limit", maxCount);

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

                            items.Add(new CatalogItem
                            {
                                Id = id,
                                Name = name,
                                CreatorName = string.IsNullOrWhiteSpace(creatorName) ? "ROBLOX" : creatorName,
                                CreatorId = ownerUserId,
                                ImageUrl = string.IsNullOrWhiteSpace(thumb) ? "/images/RobloxLogo.png" : thumb,
                                PriceRobux = 0,
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
