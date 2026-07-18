using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using Assets;

namespace RobloxWebserver.Assemblies.Catalog
{
    /// <summary>
    /// Minimal helper for building catalog HTML for keyword search results.
    /// Currently returns simple placeholder HTML so the site can run even
    /// without a full-text search implementation.
    /// </summary>
    public static class CatalogSearchHelper
    {
        public static async Task<string> BuildSearchHtmlAsync(string connectionString, string keyword, int category, IReadOnlyCollection<int>? genres, int maxCount, ICatalogItemRenderer? renderer = null)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));

            if (string.IsNullOrWhiteSpace(keyword))
                keyword = "Items";

            if (maxCount <= 0)
                maxCount = 42;

            var items = new List<CatalogItem>();

            using (var conn = new NpgsqlConnection(connectionString))
            {
                await conn.OpenAsync().ConfigureAwait(false);

                var sqlBuilder = new StringBuilder();
                sqlBuilder.Append(@"select a.asset_id,");
                sqlBuilder.Append("       a.name,");
                sqlBuilder.Append("       a.thumbnail_url,");
                sqlBuilder.Append("       a.owner_user_id,");
                sqlBuilder.Append("       u.user_name,");
                sqlBuilder.Append("       a.last_updated,");
                sqlBuilder.Append("       a.created_at,");
                sqlBuilder.Append("       a.on_sale,");
                sqlBuilder.Append("       a.price,");
                sqlBuilder.Append("       a.price_in_tix,");
                sqlBuilder.Append("       a.asset_type_id,");
                sqlBuilder.Append("       a.limited_unique,");
                sqlBuilder.Append("       a.limited_quantity ");
                sqlBuilder.Append("from assets a ");
                sqlBuilder.Append("join users u on u.user_id = a.owner_user_id ");
                sqlBuilder.Append("where coalesce(a.asset_image, false) = false ");
                sqlBuilder.Append("  and (a.on_sale = true or a.is_copying_allowed = true) ");
                sqlBuilder.Append("  and (a.is_place = false or a.is_place is null) ");
                sqlBuilder.Append("  and (a.thumbnail_url is null or a.thumbnail_url not ilike '%image%') ");
                sqlBuilder.Append("  and (a.name is null or a.name not ilike '%image%') ");
                sqlBuilder.Append("  and a.name ilike @keyword ");

                var typeIdsForCategory = AllCatalogHelper.GetAssetTypeIdsForCategory(category, null);
                if (typeIdsForCategory != null && typeIdsForCategory.Length > 0)
                {
                    var placeholders = string.Join(", ", typeIdsForCategory.Select((_, i) => $"@type{i}"));
                    sqlBuilder.Append($"  and a.asset_type_id in ({placeholders}) ");
                }

                if (genres != null && genres.Count > 0)
                {
                    sqlBuilder.Append("  and a.genre = any(@genres) ");
                }

                sqlBuilder.Append("order by a.asset_id desc ");
                sqlBuilder.Append("limit @limit");

                var sql = sqlBuilder.ToString();

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("keyword", "%" + keyword + "%");
                    cmd.Parameters.AddWithValue("limit", maxCount);

                    if (typeIdsForCategory != null)
                    {
                        for (int i = 0; i < typeIdsForCategory.Length; i++)
                        {
                            cmd.Parameters.AddWithValue($"type{i}", typeIdsForCategory[i]);
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

            var service = new CatalogService(new CatalogRepository(), renderer);
            return service.BuildCatalogHtml(page);
        }
    }

}
