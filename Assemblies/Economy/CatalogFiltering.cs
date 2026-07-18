using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using Assets;
using RobloxWebserver.Assemblies.Catalog;

namespace RobloxWebserver.Assemblies.Economy
{
    public static class CatalogFiltering
    {
        private static readonly int[] NonCatalogTypes = { 3, 4, 10, 13, 38 };

        public static async Task<string> BuildPopularItemsHtmlAsync(string connectionString, int maxCount = 42, bool excludeNonCatalogTypes = true, ICatalogItemRenderer? renderer = null)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));

            if (maxCount <= 0)
                maxCount = 42;

            var items = await FetchRankedItemsAsync(connectionString, maxCount, excludeNonCatalogTypes);

            var page = new CatalogPageResult
            {
                Items = items,
                TotalItems = items.Count
            };

            var service = new CatalogService(new CatalogRepository(), renderer);
            return service.BuildCatalogHtml(page);
        }

        public static async Task<string> BuildFeaturedItemsHtmlAsync(string connectionString, ICatalogItemRenderer? renderer = null)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));

            var items = await FetchFeaturedItemsAsync(connectionString);

            var page = new CatalogPageResult
            {
                Items = items,
                TotalItems = items.Count
            };

            var service = new CatalogService(new CatalogRepository(), renderer);
            return service.BuildBigCatalogHtml(page);
        }

        private static async Task<List<CatalogItem>> FetchRankedItemsAsync(string connectionString, int maxCount, bool excludeNonCatalogTypes)
        {
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
       a.price,
       a.price_in_tix,
       coalesce(a.sales, 0) as sales_count,
       coalesce(jsonb_array_length(a.favorites), 0) as fav_count,
       a.asset_type_id,
       a.limited_unique,
       a.limited_quantity
from assets a
join users u on u.user_id = a.owner_user_id
where coalesce(a.asset_image, false) = false
  and (a.on_sale = true or a.is_copying_allowed = true)
  and (a.is_place = false or a.is_place is null)";

                if (excludeNonCatalogTypes)
                {
                    sql += "\n  and a.asset_type_id <> all(@excluded_types)";
                }

                sql += @"
order by (coalesce(a.sales, 0) * 2 + coalesce(jsonb_array_length(a.favorites), 0) * 1) desc,
         a.asset_id desc
limit @limit";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("limit", maxCount);

                    if (excludeNonCatalogTypes)
                    {
                        cmd.Parameters.AddWithValue("excluded_types", NonCatalogTypes);
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
                            var price = reader.IsDBNull(7) ? (long?)null : reader.GetInt64(7);
                            var priceTickets = reader.IsDBNull(8) ? (long?)null : reader.GetInt64(8);
                            var sales = reader.IsDBNull(9) ? (int?)null : reader.GetInt32(9);
                            var favCount = reader.IsDBNull(10) ? (int?)null : reader.GetInt32(10);
                            var assetTypeId = reader.IsDBNull(11) ? 0 : reader.GetInt32(11);
                            var isLimitedUnique = !reader.IsDBNull(12) && reader.GetBoolean(12);
                            var limitedQuantity = reader.IsDBNull(13) ? (long?)null : reader.GetInt64(13);

                            items.Add(new CatalogItem
                            {
                                Id = id,
                                Name = name,
                                CreatorName = string.IsNullOrWhiteSpace(creatorName) ? "ROBLOX" : creatorName,
                                CreatorId = ownerUserId,
                                ImageUrl = string.IsNullOrWhiteSpace(thumb) ? (assetTypeId == 3 ? "/images/audio.png" : "/images/RobloxLogo.png") : thumb,
                                AssetTypeId = assetTypeId,
                                PriceRobux = price.HasValue ? (int?)price.Value : null,
                                PriceTickets = priceTickets.HasValue ? (int?)priceTickets.Value : null,
                                Sales = sales,
                                FavoritedCount = favCount,
                                IsNew = AssetHelpers.IsNew(createdAt),
                                UpdatedText = AssetHelpers.GetFriendlyUpdatedText(lastUpdated),
                                IsLimitedUnique = isLimitedUnique,
                                IsLimited = limitedQuantity.HasValue
                            });
                        }
                    }
                }
            }

            return items;
        }

        private static async Task<List<CatalogItem>> FetchFeaturedItemsAsync(string connectionString)
        {
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
       a.created_at,
       a.price,
       a.price_in_tix,
       coalesce(a.sales, 0) as sales_count,
       coalesce(jsonb_array_length(a.favorites), 0) as fav_count,
       a.asset_type_id,
       a.limited_unique,
       a.limited_quantity
from assets a
join users u on u.user_id = a.owner_user_id
where a.featured_rank > 0
  and coalesce(a.asset_image, false) = false
  and (a.on_sale = true or a.is_copying_allowed = true)
  and (a.is_place = false or a.is_place is null)
order by a.featured_rank asc
limit 4";

                using (var cmd = new NpgsqlCommand(sql, conn))
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
                        var price = reader.IsDBNull(7) ? (long?)null : reader.GetInt64(7);
                        var priceTickets = reader.IsDBNull(8) ? (long?)null : reader.GetInt64(8);
                        var sales = reader.IsDBNull(9) ? (int?)null : reader.GetInt32(9);
                        var favCount = reader.IsDBNull(10) ? (int?)null : reader.GetInt32(10);
                        var assetTypeId = reader.IsDBNull(11) ? 0 : reader.GetInt32(11);
                        var isLimitedUnique = !reader.IsDBNull(12) && reader.GetBoolean(12);
                        var limitedQuantity = reader.IsDBNull(13) ? (long?)null : reader.GetInt64(13);

                        items.Add(new CatalogItem
                        {
                            Id = id,
                            Name = name,
                            CreatorName = string.IsNullOrWhiteSpace(creatorName) ? "ROBLOX" : creatorName,
                            CreatorId = ownerUserId,
                            ImageUrl = string.IsNullOrWhiteSpace(thumb) ? "/images/RobloxLogo.png" : thumb,
                            AssetTypeId = assetTypeId,
                            PriceRobux = price.HasValue ? (int?)price.Value : null,
                            PriceTickets = priceTickets.HasValue ? (int?)priceTickets.Value : null,
                            Sales = sales,
                            FavoritedCount = favCount,
                            IsNew = AssetHelpers.IsNew(createdAt),
                            UpdatedText = AssetHelpers.GetFriendlyUpdatedText(lastUpdated),
                            IsLimitedUnique = isLimitedUnique,
                            IsLimited = limitedQuantity.HasValue
                        });
                    }
                }
            }

            return items;
        }
    }
}
