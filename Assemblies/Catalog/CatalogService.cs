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
        private readonly ICatalogItemRenderer? _renderer;

        public CatalogService(ICatalogRepository repository, ICatalogItemRenderer? renderer = null)
        {
            _repository = repository;
            _renderer = renderer;
        }

        public Task<CatalogPageResult> GetItemsAsync(
            int category,
            int? subcategory,
            int pageNumber,
            int pageSize)
        {
            return _repository.GetItemsAsync(category, subcategory, pageNumber, pageSize);
        }

        public string BuildCatalogItemHtml(CatalogItem item, string size = "small")
        {
            if (item.IsLimitedUnique || item.IsLimited)
            {
                Console.WriteLine($"[LIMITED] BuildCatalogItemHtml: Asset {item.Id} ({item.Name}), IsLimitedUnique={item.IsLimitedUnique}, IsLimited={item.IsLimited}, Renderer={(_renderer != null ? _renderer.GetType().Name : "null")}");
            }

            if (_renderer != null)
            {
                return _renderer.RenderItem(item, size);
            }

            var outerClass = size == "large" ? "BigOuter" : "SmallOuter";
            var viewClass = size == "large" ? "BigView" : "SmallView";
            var innerClass = size == "large" ? "BigInner" : "SmallInner";
            var imageClass = size == "large" ? "image-large" : "image-small";
            var imageSize = size == "large" ? "large" : "small";
            var isAudio = item.AssetTypeId == 3;

            var slug = ToSlug(item.Name);
            var encodedName = System.Net.WebUtility.HtmlEncode(item.Name);
            var imageUrl = item.ImageUrl;
            if (isAudio && string.IsNullOrWhiteSpace(imageUrl))
                imageUrl = "/images/audio.png";

            var sb = new StringBuilder();

            sb.Append("<div class=\"CatalogItemOuter " + outerClass + "\">");
            sb.Append("<div class=\"SmallCatalogItemView " + viewClass + "\">");
            sb.Append("<div class=\"CatalogItemInner " + innerClass + "\">");
            sb.Append("<div class=\"roblox-item-image " + imageClass + "\" data-item-id=\"" + item.Id + "\" data-image-size=\"" + imageSize + "\">");

            var wrapperStyle = isAudio ? " style=\"position:relative;\"" : "";
            sb.Append("<div class=\"item-image-wrapper\"" + wrapperStyle + "><a href=\"/catalog/" + item.Id + "/" + slug + "\">");
            sb.Append("<img class=\"original-image\" alt=\"" + encodedName + "\" title=\"" + encodedName + "\" src=\"" + imageUrl + "\" />");

            if (item.IsLimitedUnique)
            {
                sb.Append("<img src=\"/images/LimitedUniqueItems.png\" alt=\"Limited Unique\" class=\"limited-overlay\" />");
            }
            else if (item.IsLimited)
            {
                sb.Append("<img src=\"/images/LimitedItems.png\" alt=\"Limited\" class=\"limited-overlay\" />");
            }

            if (item.IsNew)
            {
                sb.Append("<img src=\"/images/NewItem.png\" alt=\"New\" />");
            }

            sb.Append("</a>");

            if (isAudio)
            {
                sb.Append("<div class=\"MediaPlayerControls\">");
                sb.Append("<div class=\"MediaPlayerIcon icon-play\" data-mediathumb-url=\"/asset/?id=" + item.Id + "\" data-jplayer-version=\"2.9.2\"></div>");
                sb.Append("</div>");
            }

            sb.Append("</div></div>");
            sb.Append("<div id=\"textDisplay\">");
            sb.Append("<div class=\"CatalogItemName notranslate\"><a class=\"name notranslate\" href=\"/catalog/" + item.Id + "/" + slug + "\" title=\"" + encodedName + "\">");
            sb.Append(encodedName);
            sb.Append("</a></div>");

            if (item.PriceRobux.HasValue || item.PriceTickets.HasValue)
            {
                if (item.PriceRobux.HasValue)
                {
                    if (item.OriginalPriceRobux.HasValue && item.OriginalPriceRobux != item.PriceRobux)
                    {
                        sb.Append("<div class=\"robux-price\"><span class=\"SalesText\">was </span><span class=\"robux notranslate\">");
                        sb.Append(item.OriginalPriceRobux.Value);
                        sb.Append("</span></div>");
                        sb.Append("<div id=\"PrivateSales\"><span class=\"SalesText\">now </span><span class=\"robux notranslate\">");
                        if (item.PriceRobux.Value == 0)
                            sb.Append("Free");
                        else
                            sb.Append(item.PriceRobux.Value);
                        sb.Append("</span></div>");
                    }
                    else
                    {
                        sb.Append("<div class=\"robux-price\"><span class=\"robux notranslate\">");
                        if (item.PriceRobux.Value == 0)
                            sb.Append("Free");
                        else
                            sb.Append(item.PriceRobux.Value);
                        sb.Append("</span></div>");
                    }
                }
                else if (item.PriceTickets.HasValue)
                {
                    sb.Append("<div class=\"tickets-price\"><span class=\"tickets notranslate\">");
                    sb.Append(item.PriceTickets.Value);
                    sb.Append("</span></div>");
                }
            }

            var creator = string.IsNullOrWhiteSpace(item.CreatorName) ? "ROBLOX" : item.CreatorName;
            var updated = string.IsNullOrWhiteSpace(item.UpdatedText) ? "recently" : item.UpdatedText;
            var sales = item.Sales ?? 0;
            var favorited = item.FavoritedCount ?? 0;

            sb.Append("<div class=\"CatalogHoverContent\">");
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

            sb.Append("</div>");

            sb.Append("</div>");
            sb.Append("</div></div></div>");

            return sb.ToString();
        }

        public string BuildCatalogHtml(CatalogPageResult pageResult)
        {
            var sb = new StringBuilder();
            foreach (var item in pageResult.Items)
            {
                sb.Append(BuildCatalogItemHtml(item, "small"));
            }
            return sb.ToString();
        }

        public string BuildBigCatalogHtml(CatalogPageResult pageResult)
        {
            var sb = new StringBuilder();
            foreach (var item in pageResult.Items)
            {
                sb.Append(BuildCatalogItemHtml(item, "large"));
            }
            return sb.ToString();
        }

        private string BuildCatalogHtmlInternal(CatalogPageResult pageResult, string outerClass, string viewClass, string innerClass, string imageClass, string imageSize)
        {
            var sb = new StringBuilder();
            var size = imageSize == "large" ? "large" : "small";
            foreach (var item in pageResult.Items)
            {
                sb.Append(BuildCatalogItemHtml(item, size));
            }
            return sb.ToString();
        }

        private static string ToSlug(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            name = name.Trim().ToLowerInvariant();

            var chars = new StringBuilder(name.Length);
            bool lastWasHyphen = false;

            foreach (var ch in name)
            {
                if ((ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9'))
                {
                    chars.Append(ch);
                    lastWasHyphen = false;
                }
                else if (ch == ' ' || ch == '-' || ch == '_' || ch == '.')
                {
                    if (!lastWasHyphen)
                    {
                        chars.Append('-');
                        lastWasHyphen = true;
                    }
                }
            }

            var result = chars.ToString().Trim('-');
            return string.IsNullOrEmpty(result) ? string.Empty : result;
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
                                AssetTypeId = 2,
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
