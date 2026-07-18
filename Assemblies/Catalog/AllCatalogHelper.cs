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
    /// ("All Categories" in browse.aspx). Now supports all catalog tabs:
    /// Collectibles (2), Clothing (3), Body Parts (4), Gear (5),
    /// Models (6), Plugins (7), Decals (8), Audio (9), Meshes (10).
    /// </summary>
    public static class AllCatalogHelper
    {
        /// <summary>
        /// Maps a catalog Category + Subcategory to a list of asset_type_id values.
        /// Returns null when no filter should be applied.
        /// </summary>
        public static int[]? GetAssetTypeIdsForCategory(int category, int? subcategory)
        {
            switch (category)
            {
                case 2: // Collectibles
                    if (!subcategory.HasValue || subcategory.Value == 2)
                        return new[] { 8, 16, 19, 24, 27, 41, 42, 43 };
                    switch (subcategory.Value)
                    {
                        case 9:  return new[] { 8, 41, 42, 43 }; // Hats
                        case 10: return new[] { 16 };            // Faces
                        case 5:  return new[] { 19, 24, 27 };    // Gear
                        default: return new[] { 8, 16, 19, 24, 27, 41, 42, 43 };
                    }

                case 3: // Clothing
                    if (!subcategory.HasValue || subcategory.Value == 3)
                        return new[] { 2, 11, 12, 32 };
                    switch (subcategory.Value)
                    {
                        case 12: return new[] { 11 };            // Shirts
                        case 13: return new[] { 2 };             // T-Shirts
                        case 14: return new[] { 12 };            // Pants
                        case 9:  return new[] { 8, 41, 42, 43 }; // Hats
                        case 11: return new[] { 32 };            // Packages
                        default: return new[] { 2, 11, 12, 32 };
                    }

                case 4: // Body Parts
                    if (!subcategory.HasValue || subcategory.Value == 4)
                        return new[] { 16, 17, 32 };
                    switch (subcategory.Value)
                    {
                        case 15: return new[] { 17 };            // Heads
                        case 10: return new[] { 16 };            // Faces
                        case 11: return new[] { 32 };            // Packages
                        default: return new[] { 16, 17, 32 };
                    }

                case 5: // Gear
                    return new[] { 19, 24, 27 };

                case 6: // Models
                    return new[] { 10 };

                case 7: // Plugins
                    return new[] { 38 };

                case 8: // Decals
                    return new[] { 13 };

                case 9: // Audio
                    return new[] { 3 };

                case 10: // Meshes
                    return new[] { 4 };

                default:
                    return null;
            }
        }

        public static async Task<string> BuildAllAssetsHtmlAsync(string connectionString, int maxCount = 42, int? category = null, int? subcategory = null, IReadOnlyCollection<int>? genres = null, ICatalogItemRenderer? renderer = null)
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
       a.price,
       a.price_in_tix,
       a.asset_type_id,
       a.limited_unique,
       a.limited_quantity
from assets a
join users u on u.user_id = a.owner_user_id
where coalesce(a.asset_image, false) = false
  and (a.on_sale = true or a.is_copying_allowed = true)
  and (a.is_place = false or a.is_place is null)";

                if (category.HasValue)
                {
                    var typeIds = GetAssetTypeIdsForCategory(category.Value, subcategory);
                    if (typeIds != null && typeIds.Length > 0)
                    {
                        var placeholders = string.Join(", ", typeIds.Select((_, i) => $"@type{i}"));
                        sql += $"\n  and a.asset_type_id in ({placeholders})";
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

                    if (category.HasValue)
                    {
                        var typeIds = GetAssetTypeIdsForCategory(category.Value, subcategory);
                        if (typeIds != null)
                        {
                            for (int i = 0; i < typeIds.Length; i++)
                            {
                                cmd.Parameters.AddWithValue($"type{i}", typeIds[i]);
                            }
                        }
                    }

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
                            var priceTickets = reader.IsDBNull(9) ? (long?)null : reader.GetInt64(9);
                            var assetTypeId = reader.IsDBNull(10) ? 0 : reader.GetInt32(10);
                            var isLimitedUnique = !reader.IsDBNull(11) && reader.GetBoolean(11);
                            var limitedQuantity = reader.IsDBNull(12) ? (long?)null : reader.GetInt64(12);

                            if (isLimitedUnique || limitedQuantity.HasValue)
                            {
                                Console.WriteLine($"[LIMITED] Asset {id} ({name}): IsLimitedUnique={isLimitedUnique}, LimitedQuantity={limitedQuantity}, AssetTypeId={assetTypeId}");
                            }

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
                                Sales = 0,
                                FavoritedCount = 0,
                                IsNew = AssetHelpers.IsNew(createdAt),
                                UpdatedText = AssetHelpers.GetFriendlyUpdatedText(lastUpdated),
                                IsLimitedUnique = isLimitedUnique,
                                IsLimited = limitedQuantity.HasValue
                            });
                        }
                    }
                }
            }

            var assetsRepo = new Assets.AssetsRepository();
            foreach (var item in items)
            {
                try
                {
                    var favCount = await assetsRepo.GetFavoriteCountAsync(connectionString, item.Id).ConfigureAwait(false);
                    item.FavoritedCount = favCount;
                }
                catch
                {
                    item.FavoritedCount = null;
                }
            }

            var page = new CatalogPageResult
            {
                Items = items,
                TotalItems = items.Count
            };

            Console.WriteLine($"[LIMITED] AllCatalogHelper: {items.Count} total items, {items.Count(i => i.IsLimited || i.IsLimitedUnique)} limited");

            var service = new CatalogService(new CatalogRepository(), renderer);
            return service.BuildCatalogHtml(page);
        }
    }
}
