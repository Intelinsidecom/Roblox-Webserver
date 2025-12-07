using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using Assets;

namespace RobloxWebserver.Assemblies.Catalog
{
    public class CatalogService : ICatalogService
    {
        private readonly ICatalogRepository _repository;

        public CatalogService(ICatalogRepository repository)
        {
            _repository = repository;
        }

        public Task<CatalogPageResult> GetItemsAsync(
            int category,
            int? subcategory,
            int pageNumber,
            int pageSize)
        {
            return _repository.GetItemsAsync(category, subcategory, pageNumber, pageSize);
        }

        public string BuildCatalogHtml(CatalogPageResult pageResult)
        {
            var sb = new StringBuilder();

            foreach (var item in pageResult.Items)
            {
                sb.Append("<div class=\"CatalogItemOuter SmallOuter\">");
                sb.Append("<div class=\"SmallCatalogItemView SmallView\">");
                sb.Append("<div class=\"CatalogItemInner SmallInner\">");
                sb.Append("<div class=\"roblox-item-image image-small\" data-item-id=\"" + item.Id + "\" data-image-size=\"small\">");
                sb.Append("<div class=\"item-image-wrapper\"><a href=\"#\">");
                sb.Append("<img class=\"original-image\" alt=\"" + System.Net.WebUtility.HtmlEncode(item.Name) + "\" title=\"" + System.Net.WebUtility.HtmlEncode(item.Name) + "\" src=\"" + item.ImageUrl + "\" />");
                if (item.IsNew)
                {
                    sb.Append("<img src=\"/images/NewItem.png\" alt=\"New\" />");
                }
                sb.Append("</a></div></div>");

                sb.Append("<div id=\"textDisplay\">");
                sb.Append("<div class=\"CatalogItemName notranslate\"><a class=\"name notranslate\" href=\"#\" title=\"" + System.Net.WebUtility.HtmlEncode(item.Name) + "\">");
                sb.Append(System.Net.WebUtility.HtmlEncode(item.Name));
                sb.Append("</a></div>");

                // Price block: show Robux icon-style span with 0 if no price is set
                var priceRobux = item.PriceRobux ?? 0;
                sb.Append("<div class=\"robux-price\"><span class=\"robux notranslate\">");
                sb.Append(priceRobux);
                sb.Append("</span></div>");

                // Hover content: creator, updated, sales, favorited (all safe defaults)
                var creator = string.IsNullOrWhiteSpace(item.CreatorName) ? "ROBLOX" : item.CreatorName;
                var updated = string.IsNullOrWhiteSpace(item.UpdatedText) ? "recently" : item.UpdatedText;
                var sales = item.Sales ?? 0;
                var favorited = item.FavoritedCount ?? 0;

                sb.Append("<div class=\"CatalogHoverContent\" style=\"text-align:left;\">");
                sb.Append("<div><span class=\"CatalogItemInfoLabel\">Creator:</span> <span class=\"HoverInfo notranslate\">");
                if (item.CreatorId.HasValue && item.CreatorId.Value > 0)
                {
                    sb.Append("<a href=\"/users/");
                    sb.Append(item.CreatorId.Value);
                    sb.Append("/profile\">");
                    sb.Append(System.Net.WebUtility.HtmlEncode(creator));
                    sb.Append("</a>");
                }
                else
                {
                    sb.Append(System.Net.WebUtility.HtmlEncode(creator));
                }
                sb.Append("</span></div>");

                sb.Append("<div><span class=\"CatalogItemInfoLabel\">Updated:</span> <span class=\"HoverInfo\">");
                sb.Append(System.Net.WebUtility.HtmlEncode(updated));
                sb.Append("</span></div>");

                sb.Append("<div><span class=\"CatalogItemInfoLabel\">Sales:</span> <span class=\"HoverInfo notranslate\">");
                sb.Append(sales);
                sb.Append("</span></div>");

                sb.Append("<div><span class=\"CatalogItemInfoLabel\">Favorited:</span> <span class=\"HoverInfo\">");
                sb.Append(favorited);
                sb.Append(" times</span></div>");

                sb.Append("</div>"); // end CatalogHoverContent

                sb.Append("</div>"); // end textDisplay

                sb.Append("</div></div></div>");
            }

            return sb.ToString();
        }

        public async Task<string> BuildTShirtCatalogHtmlAsync(string connectionString, int maxCount = 42)
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
where a.asset_type_id = 2
  and coalesce(a.asset_image, false) = false
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
                            var creatorName = reader.IsDBNull(4) ? "" : reader.GetString(4);
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

            return BuildCatalogHtml(page);
        }
    }
}
