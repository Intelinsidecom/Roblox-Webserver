using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Assets;
using Economy;
using Games;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Website.Services;

public sealed class DevelopTabService
{
    private readonly IConfiguration _configuration;
    private readonly ShirtAssetsRepository _shirtAssetsRepository = new();
    private readonly UserAssetsRepository _userAssetsRepository = new();

    public DevelopTabService(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public string? ConnectionString => _configuration.GetConnectionString("Default");

    private string DefaultThumbnailUrl => _configuration["Thumbnails:DefaultThumbnailUrl"] ?? "/images/default.png";
    private string AudioThumbnailUrl => _configuration["Thumbnails:AudioThumbnailUrl"] ?? "/images/audio.png";
    private string PluginThumbnailUrl => _configuration["Thumbnails:PluginThumbnailUrl"] ?? "/images/plugin.png";

    private string GetFallbackThumbnailUrl(int assetTypeId) => assetTypeId switch
    {
        3 => AudioThumbnailUrl,
        38 => PluginThumbnailUrl,
        _ => DefaultThumbnailUrl,
    };

    public async Task<Assemblies.Common.DevelopTabViewModel> BuildAsync(
        long userId,
        string? userName,
        string viewName,
        bool showPublicOnly = false,
        long? groupId = null,
        int? category = null,
        int? sortType = null,
        List<int>? genres = null,
        string? keyword = null,
        int pageNumber = 1,
        CancellationToken cancellationToken = default)
    {
        var vm = new Assemblies.Common.DevelopTabViewModel
        {
            ViewName = viewName,
            UserId = userId,
            UserName = userName,
            GroupId = groupId,
            SelectedCategory = category ?? 0,
            SelectedSortType = sortType ?? 0,
            SelectedGenres = genres ?? new List<int>(),
            Keyword = keyword,
            PageNumber = pageNumber,
        };

        switch (viewName)
        {
            case "Games":
                await PopulateGamesAsync(vm, showPublicOnly, cancellationToken).ConfigureAwait(false);
                break;
            case "Places":
                await PopulatePlacesAsync(vm, cancellationToken).ConfigureAwait(false);
                break;
            case "T-Shirts":
                await PopulateTShirtsAsync(vm, cancellationToken).ConfigureAwait(false);
                break;
            case "Shirts":
                await PopulateShirtsAsync(vm, cancellationToken).ConfigureAwait(false);
                break;
            case "Pants":
                await PopulatePantsAsync(vm, cancellationToken).ConfigureAwait(false);
                break;
            case "Models":
                await PopulateModelsAsync(vm, cancellationToken).ConfigureAwait(false);
                break;
            case "Meshes":
                await PopulateMeshesAsync(vm, cancellationToken).ConfigureAwait(false);
                break;
            case "Decals":
                await PopulateDecalsAsync(vm, cancellationToken).ConfigureAwait(false);
                break;
            case "Audio":
                await PopulateAudiosAsync(vm, cancellationToken).ConfigureAwait(false);
                break;
            case "Library":
                await PopulateLibraryAsync(vm, cancellationToken).ConfigureAwait(false);
                break;
            case "Plugins":
                await PopulatePluginsAsync(vm, cancellationToken).ConfigureAwait(false);
                break;
            case "GamePasses":
                await PopulateGamesAsync(vm, showPublicOnly: true, cancellationToken).ConfigureAwait(false);
                await PopulateGamePassesAsync(vm, cancellationToken).ConfigureAwait(false);
                vm.AssetTypeId = 34;
                vm.HeaderText = "Game Passes";
                vm.MaxActiveCount = 0;
                break;
            case "Badges":
                await PopulateGamesAsync(vm, showPublicOnly: true, cancellationToken).ConfigureAwait(false);
                await PopulateBadgesAsync(vm, cancellationToken).ConfigureAwait(false);
                vm.AssetTypeId = 21;
                vm.HeaderText = "Badges";
                vm.MaxActiveCount = 0;
                break;
            default:
                break;
        }

        return vm;
    }

    private static List<int>? CategoryToAssetTypeIds(int category)
    {
        return category switch
        {
            6 => new List<int> { 10 },
            7 => new List<int> { 38 },
            8 => new List<int> { 13 },
            9 => new List<int> { 3 },
            10 => new List<int> { 4 },
            _ => null,
        };
    }

    private async Task<Dictionary<long, long>> GetSalesLast7DaysAsync(
        NpgsqlConnection conn,
        IReadOnlyCollection<long> assetIds,
        CancellationToken cancellationToken)
    {
        if (assetIds == null || assetIds.Count == 0)
            return new Dictionary<long, long>();

        const string sql = @"SELECT asset_id, COUNT(*)
                             FROM asset_sales_log
                             WHERE asset_id = ANY(@ids) AND sold_at >= now() - interval '7 days'
                             GROUP BY asset_id";
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("ids", assetIds);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var dict = new Dictionary<long, long>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            dict[reader.GetInt64(0)] = reader.GetInt64(1);
        }
        return dict;
    }

    private async Task PopulateGamesAsync(Assemblies.Common.DevelopTabViewModel vm, bool showPublicOnly, CancellationToken cancellationToken)
    {
        vm.AssetTypeId = 0;
        vm.HeaderText = "Games";
        vm.MaxActiveCount = 200;

        var connStr = ConnectionString;
        if (string.IsNullOrWhiteSpace(connStr) || vm.UserId <= 0)
        {
            return;
        }

        try
        {
            var universes = await GameListingService
                .GetUniversesForUserAsync(connStr, vm.UserId, cancellationToken)
                .ConfigureAwait(false);

            var items = new List<Assemblies.Common.DevelopItem>(universes.Count);
            foreach (var u in universes)
            {
                if (showPublicOnly && u.PrivacyLevel != 1) continue;

                var configureUrl = "/universes/configure?id=" + u.UniverseId;
                items.Add(new Assemblies.Common.DevelopItem
                {
                    ItemId = u.UniverseId,
                    RootPlaceId = u.RootPlaceId,
                    Name = u.UniverseName,
                    ThumbnailUrl = u.ThumbnailUrl,
                    Type = "universes",
                    ConfigureUrl = configureUrl,
                    ConfigureLocalizationUrl = "/localization/games/" + u.UniverseId + "/configure",
                    CreateBadgeUrl = "/develop?selectedPlaceId=" + u.RootPlaceId + "&View=21",
                    CreateGamepassUrl = "/develop?selectedPlaceId=" + u.RootPlaceId + "&View=34",
                    DeveloperStatsUrl = "/places/" + u.RootPlaceId + "/stats",
                    AdvertiseUrl = "/My/NewUserAd.aspx?targetId=" + u.RootPlaceId + "&targettype=Asset",
                    ActivateUniverseUrl = "https://develop.roblox.com/v1/universes/" + u.UniverseId + "/activate",
                    DeactivateUniverseUrl = "https://develop.roblox.com/v1/universes/" + u.UniverseId + "/deactivate",
                    StartPlaceName = u.PlaceName,
                    StartPlaceId = u.RootPlaceId,
                    IsPublic = u.PrivacyLevel == 1,
                    VisitCount = u.VisitCount,
                    StatusText = u.PrivacyLevel switch
                    {
                        2 => "Friends",
                        3 => "Private",
                        _ => "Public",
                    },
                });
            }

            vm.Items = items;
            vm.ActiveCount = items.Count(i => i.IsPublic);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] PopulateGamesAsync: {ex}");
            vm.Items = new List<Assemblies.Common.DevelopItem>();
        }
    }

    private async Task PopulateGamePassesAsync(Assemblies.Common.DevelopTabViewModel vm, CancellationToken cancellationToken)
    {
        var connStr = ConnectionString;
        if (string.IsNullOrWhiteSpace(connStr) || vm.UserId <= 0)
        {
            return;
        }

        try
        {
            const string sql = @"select a.asset_id, a.name, a.created_at, a.thumbnail_url,
                    COALESCE(a.price, 0) as price, COALESCE(a.on_sale, false) as on_sale,
                    COALESCE(a.sales, 0) as sales, COALESCE(u.root_place_id, 0) as root_place_id
                from assets a
                left join universes u on u.universe_id = a.belongs_to_universe
                where a.owner_user_id = @uid
                  and a.asset_type_id = 34
                order by a.created_at desc, a.asset_id desc
                limit 50;";

            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            var items = new List<Assemblies.Common.DevelopItem>();
            {
                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("uid", vm.UserId);

                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    var assetId = reader.GetInt64(0);
                    var name = reader.IsDBNull(1) ? "Unnamed" : reader.GetString(1);
                    var createdAt = reader.GetDateTime(2);
                    var thumb = reader.IsDBNull(3) ? DefaultThumbnailUrl : reader.GetString(3);
                    var price = reader.IsDBNull(4) ? 0 : reader.GetInt32(4);
                    var onSale = !reader.IsDBNull(5) && reader.GetBoolean(5);
                    var sales = reader.IsDBNull(6) ? 0L : reader.GetInt64(6);
                    var rootPlaceId = reader.IsDBNull(7) ? 0L : reader.GetInt64(7);

                    items.Add(new Assemblies.Common.DevelopItem
                    {
                        ItemId = assetId,
                        AssetId = assetId,
                        RootPlaceId = rootPlaceId,
                        Name = name,
                        ThumbnailUrl = thumb,
                        Type = "gamepasses",
                        CatalogUrl = "/catalog/" + assetId + "/" + Assemblies.Common.DevelopSlugHelper.Slug(name),
                        CreatedAt = createdAt,
                        PriceRobux = price,
                        IsOnSale = onSale,
                        Sales = sales,
                    });
                }
            }

            vm.GamePasses = items;

            var assetIds = items.Select(i => i.AssetId).ToList();
            var salesLast7 = await GetSalesLast7DaysAsync(conn, assetIds, cancellationToken).ConfigureAwait(false);
            foreach (var item in items)
            {
                if (salesLast7.TryGetValue(item.AssetId, out var count))
                    item.SalesLast7Days = count;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] PopulateGamePassesAsync: {ex}");
            vm.GamePasses = new List<Assemblies.Common.DevelopItem>();
        }
    }

    private async Task PopulateBadgesAsync(Assemblies.Common.DevelopTabViewModel vm, CancellationToken cancellationToken)
    {
        var connStr = ConnectionString;
        if (string.IsNullOrWhiteSpace(connStr) || vm.UserId <= 0)
        {
            return;
        }

        try
        {
            const string sql = @"select a.asset_id, a.name, a.created_at, a.thumbnail_url,
                    COALESCE(a.sales, 0) as sales, COALESCE(u.root_place_id, 0) as root_place_id
                from assets a
                left join universes u on u.universe_id = a.belongs_to_universe
                where a.owner_user_id = @uid
                  and a.asset_type_id = 21
                order by a.created_at desc, a.asset_id desc
                limit 50;";

            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            var items = new List<Assemblies.Common.DevelopItem>();
            {
                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("uid", vm.UserId);

                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    var assetId = reader.GetInt64(0);
                    var name = reader.IsDBNull(1) ? "Unnamed" : reader.GetString(1);
                    var createdAt = reader.GetDateTime(2);
                    var thumb = reader.IsDBNull(3) ? DefaultThumbnailUrl : reader.GetString(3);
                    var sales = reader.IsDBNull(4) ? 0L : reader.GetInt64(4);
                    var rootPlaceId = reader.IsDBNull(5) ? 0L : reader.GetInt64(5);

                    items.Add(new Assemblies.Common.DevelopItem
                    {
                        ItemId = assetId,
                        AssetId = assetId,
                        RootPlaceId = rootPlaceId,
                        Name = name,
                        ThumbnailUrl = thumb,
                        Type = "badges",
                        CatalogUrl = "/badges/" + assetId + "/" + Assemblies.Common.DevelopSlugHelper.Slug(name),
                        CreatedAt = createdAt,
                        Sales = sales,
                    });
                }
            }

            vm.Badges = items;

            var assetIds = items.Select(i => i.AssetId).ToList();
            if (assetIds.Count > 0)
            {
                const string statsSql = @"SELECT asset_id, COUNT(*),
                        COUNT(*) FILTER (WHERE created_at >= now() - interval '1 day')
                    FROM user_assets
                    WHERE asset_id = ANY(@ids)
                    GROUP BY asset_id;";

                await using var statsCmd = new NpgsqlCommand(statsSql, conn);
                statsCmd.Parameters.AddWithValue("ids", assetIds);

                var subtractCreator = _configuration.GetValue<bool>("Badge:SubtractCreatorFromTotalWon");
                await using var statsReader = await statsCmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                while (await statsReader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    var badgeId = statsReader.GetInt64(0);
                    var totalWon = statsReader.GetInt64(1);
                    var wonYesterday = statsReader.GetInt64(2);

                    if (subtractCreator)
                    {
                        totalWon = Math.Max(0, totalWon - 1);
                        wonYesterday = Math.Max(0, wonYesterday - 1);
                    }

                    var item = items.FirstOrDefault(i => i.AssetId == badgeId);
                    if (item != null)
                    {
                        item.Sales = totalWon;
                        item.SalesLast7Days = wonYesterday;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] PopulateBadgesAsync: {ex}");
            vm.Badges = new List<Assemblies.Common.DevelopItem>();
        }
    }

    private async Task PopulatePlacesAsync(Assemblies.Common.DevelopTabViewModel vm, CancellationToken cancellationToken)
    {
        vm.AssetTypeId = 9;
        vm.HeaderText = "Places";
        vm.MaxActiveCount = 0;

        var connStr = ConnectionString;
        if (string.IsNullOrWhiteSpace(connStr) || vm.UserId <= 0)
        {
            return;
        }

        try
        {
            var places = await GameListingService
                .GetPlacesForUserAsync(connStr, vm.UserId, cancellationToken)
                .ConfigureAwait(false);

            var items = new List<Assemblies.Common.DevelopItem>(places.Count);
            foreach (var p in places)
            {
                var configureUrl = "/places/" + p.PlaceId + "/update";
                items.Add(new Assemblies.Common.DevelopItem
                {
                    ItemId = p.PlaceId,
                    RootPlaceId = p.PlaceId,
                    Name = p.PlaceName,
                    ThumbnailUrl = p.ThumbnailUrl,
                    Type = "game",
                    ConfigureUrl = configureUrl,
                    DeveloperStatsUrl = "/places/" + p.PlaceId + "/stats",
                    VisitCount = p.VisitCount,
                    CreatedAt = p.LastUpdated ?? DateTime.MinValue,
                });
            }

            vm.Items = items;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] PopulatePlacesAsync: {ex}");
            vm.Items = new List<Assemblies.Common.DevelopItem>();
        }
    }

    private async Task PopulateTShirtsAsync(Assemblies.Common.DevelopTabViewModel vm, CancellationToken cancellationToken)
    {
        vm.AssetTypeId = 2;
        vm.HeaderText = "Create a T-Shirt";
        vm.MaxActiveCount = 0;

        var connStr = ConnectionString;
        if (string.IsNullOrWhiteSpace(connStr) || vm.UserId <= 0)
        {
            return;
        }

        try
        {
            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = @"select t.asset_id as tshirt_asset_id,
        t.name,
        ua_t.created_at,
        t.thumbnail_url,
        i.asset_id as image_asset_id,
        COALESCE(t.sales, 0) as sales
from user_assets ua_t
join assets t on t.asset_id = ua_t.asset_id and t.asset_type_id = 2 and t.owner_user_id = @uid
left join assets i on i.owner_user_id = t.owner_user_id
                  and i.asset_type_id = 1
                  and i.name = t.name || ' Image'
where ua_t.user_id = @uid
order by ua_t.created_at desc, t.asset_id desc
limit 50;";

            var items = new List<Assemblies.Common.ClothingItem>();
            {
                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("uid", vm.UserId);

                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    var assetId = reader.GetInt64(0);
                    var name = reader.IsDBNull(1) ? "Unnamed" : reader.GetString(1);
                    var createdAt = reader.GetDateTime(2);
                    var thumb = reader.IsDBNull(3) ? DefaultThumbnailUrl : reader.GetString(3);
                    var imageAssetId = reader.IsDBNull(4) ? (long?)null : reader.GetInt64(4);
                    var sales = reader.IsDBNull(5) ? 0L : reader.GetInt64(5);

                    items.Add(new Assemblies.Common.ClothingItem
                    {
                        AssetId = assetId,
                        ImageAssetId = imageAssetId,
                        Name = name,
                        CreatedAt = createdAt,
                        ThumbnailUrl = thumb,
                        Type = "tshirts",
                        CatalogUrl = "/catalog/" + assetId + "/" + Assemblies.Common.DevelopSlugHelper.Slug(name),
                        Sales = sales,
                    });
                }
            }

            var assetIds = items.Select(i => i.AssetId).ToList();
            var salesLast7 = await GetSalesLast7DaysAsync(conn, assetIds, cancellationToken).ConfigureAwait(false);
            foreach (var item in items)
            {
                if (salesLast7.TryGetValue(item.AssetId, out var count))
                    item.SalesLast7Days = count;
            }

            vm.TShirts = items;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] PopulateTShirtsAsync: {ex}");
            vm.TShirts = new List<Assemblies.Common.ClothingItem>();
        }
    }

    private async Task PopulateShirtsAsync(Assemblies.Common.DevelopTabViewModel vm, CancellationToken cancellationToken)
    {
        vm.AssetTypeId = 11;
        vm.HeaderText = "Create Shirt";
        vm.MaxActiveCount = 0;

        var connStr = ConnectionString;
        if (string.IsNullOrWhiteSpace(connStr) || vm.UserId <= 0)
        {
            return;
        }

        try
        {
            var shirts = await _shirtAssetsRepository
                .GetUserShirtsWithImagesAsync(connStr, vm.UserId, cancellationToken)
                .ConfigureAwait(false);

            var clothingItems = shirts.Select(s => new Assemblies.Common.ClothingItem
            {
                AssetId = s.AssetId,
                ImageAssetId = s.ImageAssetId,
                Name = s.Name,
                CreatedAt = s.CreatedAt,
                ThumbnailUrl = s.ThumbnailUrl,
                Type = "shirts",
                CatalogUrl = "/catalog/" + s.AssetId + "/" + Assemblies.Common.DevelopSlugHelper.Slug(s.Name),
                Sales = s.Sales,
            }).ToList();

            var assetIds = clothingItems.Select(i => i.AssetId).ToList();
            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            var salesLast7 = await GetSalesLast7DaysAsync(conn, assetIds, cancellationToken).ConfigureAwait(false);
            foreach (var item in clothingItems)
            {
                if (salesLast7.TryGetValue(item.AssetId, out var count))
                    item.SalesLast7Days = count;
            }

            vm.Shirts = clothingItems;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] PopulateShirtsAsync: {ex}");
            vm.Shirts = new List<Assemblies.Common.ClothingItem>();
        }
    }

    private async Task PopulatePantsAsync(Assemblies.Common.DevelopTabViewModel vm, CancellationToken cancellationToken)
    {
        vm.AssetTypeId = 12;
        vm.HeaderText = "Create Pants";
        vm.MaxActiveCount = 0;

        var connStr = ConnectionString;
        if (string.IsNullOrWhiteSpace(connStr) || vm.UserId <= 0)
        {
            return;
        }

        try
        {
            var pants = await _userAssetsRepository
                .GetUserPantsWithImagesAsync(connStr, vm.UserId, cancellationToken)
                .ConfigureAwait(false);

            var clothingItems = pants.Select(p => new Assemblies.Common.ClothingItem
            {
                AssetId = p.AssetId,
                ImageAssetId = p.ImageAssetId,
                Name = p.Name,
                CreatedAt = p.CreatedAt,
                ThumbnailUrl = p.ThumbnailUrl,
                Type = "pants",
                CatalogUrl = "/catalog/" + p.AssetId + "/" + Assemblies.Common.DevelopSlugHelper.Slug(p.Name),
                Sales = p.Sales,
            }).ToList();

            var assetIds = clothingItems.Select(i => i.AssetId).ToList();
            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            var salesLast7 = await GetSalesLast7DaysAsync(conn, assetIds, cancellationToken).ConfigureAwait(false);
            foreach (var item in clothingItems)
            {
                if (salesLast7.TryGetValue(item.AssetId, out var count))
                    item.SalesLast7Days = count;
            }

            vm.Pants = clothingItems;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] PopulatePantsAsync: {ex}");
            vm.Pants = new List<Assemblies.Common.ClothingItem>();
        }
    }

    private async Task PopulateModelsAsync(Assemblies.Common.DevelopTabViewModel vm, CancellationToken cancellationToken)
    {
        vm.AssetTypeId = 10;
        vm.HeaderText = "Create Model";
        vm.MaxActiveCount = 0;

        var connStr = ConnectionString;
        if (string.IsNullOrWhiteSpace(connStr) || vm.UserId <= 0)
        {
            return;
        }

        try
        {
            const string sql = @"select a.asset_id, a.name, a.created_at, a.thumbnail_url, COALESCE(a.sales, 0) as sales
                from assets a
                where a.owner_user_id = @uid
                  and a.asset_type_id = 10
                  and (a.is_place = false OR a.is_place IS NULL)
                order by a.created_at desc, a.asset_id desc
                limit 50;";

            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            var items = new List<Assemblies.Common.DevelopItem>();
            {
                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("uid", vm.UserId);

                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    var assetId = reader.GetInt64(0);
                    var name = reader.IsDBNull(1) ? "Unnamed" : reader.GetString(1);
                    var createdAt = reader.GetDateTime(2);
                    var thumb = reader.IsDBNull(3) ? DefaultThumbnailUrl : reader.GetString(3);
                    var sales = reader.IsDBNull(4) ? 0L : reader.GetInt64(4);

                    items.Add(new Assemblies.Common.DevelopItem
                    {
                        ItemId = assetId,
                        AssetId = assetId,
                        RootPlaceId = assetId,
                        Name = name,
                        ThumbnailUrl = thumb,
                        Type = "models",
                        ConfigureUrl = "/asset/" + assetId + "/configure",
                        CatalogUrl = "/catalog/" + assetId + "/" + Assemblies.Common.DevelopSlugHelper.Slug(name),
                        CreatedAt = createdAt,
                        Sales = sales,
                    });
                }
            }

            var assetIds = items.Select(i => i.AssetId).ToList();
            var salesLast7 = await GetSalesLast7DaysAsync(conn, assetIds, cancellationToken).ConfigureAwait(false);
            foreach (var item in items)
            {
                if (salesLast7.TryGetValue(item.AssetId, out var count))
                    item.SalesLast7Days = count;
            }

            vm.Models = items;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] PopulateModelsAsync: {ex}");
            vm.Models = new List<Assemblies.Common.DevelopItem>();
        }
    }

    private async Task PopulateDecalsAsync(Assemblies.Common.DevelopTabViewModel vm, CancellationToken cancellationToken)
    {
        vm.AssetTypeId = 13;
        vm.HeaderText = "Create Decal";
        vm.MaxActiveCount = 0;

        var connStr = ConnectionString;
        if (string.IsNullOrWhiteSpace(connStr) || vm.UserId <= 0)
        {
            return;
        }

        try
        {
            const string sql = @"select a.asset_id, a.name, a.created_at, a.thumbnail_url, COALESCE(a.sales, 0) as sales
                from assets a
                where a.owner_user_id = @uid
                  and a.asset_type_id = 13
                order by a.created_at desc, a.asset_id desc
                limit 50;";

            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("uid", vm.UserId);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

            var items = new List<Assemblies.Common.DevelopItem>();
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var assetId = reader.GetInt64(0);
                var name = reader.IsDBNull(1) ? "Unnamed" : reader.GetString(1);
                var createdAt = reader.GetDateTime(2);
                var thumb = reader.IsDBNull(3) ? DefaultThumbnailUrl : reader.GetString(3);
                var sales = reader.IsDBNull(4) ? 0L : reader.GetInt64(4);

                items.Add(new Assemblies.Common.DevelopItem
                {
                    ItemId = assetId,
                    AssetId = assetId,
                    RootPlaceId = assetId,
                    Name = name,
                    ThumbnailUrl = thumb,
                    Type = "decals",
                    ConfigureUrl = "/asset/" + assetId + "/configure",
                    CatalogUrl = "/catalog/" + assetId + "/" + Assemblies.Common.DevelopSlugHelper.Slug(name),
                    CreatedAt = createdAt,
                    Sales = sales,
                });
            }

            vm.Decals = items;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] PopulateDecalsAsync: {ex}");
            vm.Decals = new List<Assemblies.Common.DevelopItem>();
        }
    }

    private async Task PopulateMeshesAsync(Assemblies.Common.DevelopTabViewModel vm, CancellationToken cancellationToken)
    {
        vm.AssetTypeId = 4;
        vm.HeaderText = "Meshes";
        vm.MaxActiveCount = 0;

        var connStr = ConnectionString;
        if (string.IsNullOrWhiteSpace(connStr) || vm.UserId <= 0)
        {
            return;
        }

        try
        {
            const string sql = @"select a.asset_id, a.name, a.created_at, a.thumbnail_url, COALESCE(a.sales, 0) as sales
                from assets a
                where a.owner_user_id = @uid
                  and a.asset_type_id = 4
                order by a.created_at desc, a.asset_id desc
                limit 50;";

            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("uid", vm.UserId);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

            var items = new List<Assemblies.Common.DevelopItem>();
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var assetId = reader.GetInt64(0);
                var name = reader.IsDBNull(1) ? "Unnamed" : reader.GetString(1);
                var createdAt = reader.GetDateTime(2);
                var thumb = reader.IsDBNull(3) ? DefaultThumbnailUrl : reader.GetString(3);
                var sales = reader.IsDBNull(4) ? 0L : reader.GetInt64(4);

                items.Add(new Assemblies.Common.DevelopItem
                {
                    ItemId = assetId,
                    AssetId = assetId,
                    RootPlaceId = assetId,
                    Name = name,
                    ThumbnailUrl = thumb,
                    Type = "meshes",
                    ConfigureUrl = "/asset/" + assetId + "/configure",
                    CatalogUrl = "/catalog/" + assetId + "/" + Assemblies.Common.DevelopSlugHelper.Slug(name),
                    CreatedAt = createdAt,
                    Sales = sales,
                });
            }

            vm.Meshes = items;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] PopulateMeshesAsync: {ex}");
            vm.Meshes = new List<Assemblies.Common.DevelopItem>();
        }
    }

    private async Task PopulateAudiosAsync(Assemblies.Common.DevelopTabViewModel vm, CancellationToken cancellationToken)
    {
        vm.AssetTypeId = 3;
        vm.HeaderText = "Audio";
        vm.MaxActiveCount = 0;

        var connStr = ConnectionString;
        if (string.IsNullOrWhiteSpace(connStr) || vm.UserId <= 0)
        {
            return;
        }

        try
        {
            const string sql = @"select a.asset_id, a.name, a.created_at, a.thumbnail_url, COALESCE(a.sales, 0) as sales
                from assets a
                where a.owner_user_id = @uid
                  and a.asset_type_id = 3
                order by a.created_at desc, a.asset_id desc
                limit 50;";

            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("uid", vm.UserId);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

            var defaultThumb = _configuration["Thumbnails:AudioThumbnailUrl"] ?? "/images/audio.png";
            var items = new List<Assemblies.Common.DevelopItem>();
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var assetId = reader.GetInt64(0);
                var name = reader.IsDBNull(1) ? "Unnamed" : reader.GetString(1);
                var createdAt = reader.GetDateTime(2);
                var thumb = reader.IsDBNull(3) ? defaultThumb : reader.GetString(3);
                var sales = reader.IsDBNull(4) ? 0L : reader.GetInt64(4);

                items.Add(new Assemblies.Common.DevelopItem
                {
                    ItemId = assetId,
                    AssetId = assetId,
                    RootPlaceId = assetId,
                    Name = name,
                    ThumbnailUrl = thumb,
                    Type = "audios",
                    ConfigureUrl = "/asset/" + assetId + "/configure",
                    CatalogUrl = "/catalog/" + assetId + "/" + Assemblies.Common.DevelopSlugHelper.Slug(name),
                    CreatedAt = createdAt,
                    Sales = sales,
                });
            }

            vm.Audios = items;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] PopulateAudiosAsync: {ex}");
            vm.Audios = new List<Assemblies.Common.DevelopItem>();
        }
    }

    private async Task PopulateLibraryAsync(Assemblies.Common.DevelopTabViewModel vm, CancellationToken cancellationToken)
    {
        vm.AssetTypeId = 0;
        vm.HeaderText = "Library";
        vm.MaxActiveCount = 0;

        var connStr = ConnectionString;
        if (string.IsNullOrWhiteSpace(connStr))
        {
            return;
        }

        try
        {
            var categoryTypeIds = CategoryToAssetTypeIds(vm.SelectedCategory);
            var assetTypeFilter = categoryTypeIds != null
                ? string.Join(",", categoryTypeIds)
                : "3, 4, 10, 13, 38";

            var whereClauses = new List<string>
            {
                $"a.asset_type_id in ({assetTypeFilter})",
                "coalesce(a.asset_image, false) = false",
            };

            if (vm.SelectedGenres.Count > 0)
            {
                var genreList = string.Join(",", vm.SelectedGenres);
                whereClauses.Add($"a.genre in ({genreList})");
            }

            if (!string.IsNullOrWhiteSpace(vm.Keyword))
            {
                whereClauses.Add($"a.name ilike @keyword");
            }

            var orderClause = vm.SelectedSortType switch
            {
                3 => "order by a.last_updated desc nulls last, a.asset_id desc",
                4 => "order by a.price asc nulls last, a.asset_id desc",
                5 => "order by a.price desc nulls last, a.asset_id desc",
                _ => "order by a.asset_id desc",
            };

            var whereSql = string.Join(" and ", whereClauses);

            var countSql = $"select count(*) from assets a where {whereSql};";

            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            await using var countCmd = new NpgsqlCommand(countSql, conn);
            if (!string.IsNullOrWhiteSpace(vm.Keyword))
                countCmd.Parameters.AddWithValue("keyword", $"%{vm.Keyword}%");
            var totalCount = (long)(await countCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false) ?? 0);

            vm.TotalItems = (int)totalCount;
            vm.TotalPages = Math.Max(1, (int)Math.Ceiling((double)totalCount / 50));

            var offset = (vm.PageNumber - 1) * 50;
            var sql = $@"select a.asset_id, a.name, a.thumbnail_url, a.asset_type_id, u.user_name, a.created_at,
                   a.on_sale, a.price, a.price_in_tix
                from assets a
                join users u on u.user_id = a.owner_user_id
                where {whereSql}
                {orderClause}
                limit 50 offset {offset};";

            await using var cmd = new NpgsqlCommand(sql, conn);
            if (!string.IsNullOrWhiteSpace(vm.Keyword))
                cmd.Parameters.AddWithValue("keyword", $"%{vm.Keyword}%");
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

            var items = new List<Assemblies.Common.DevelopItem>();
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var assetId = reader.GetInt64(0);
                var name = reader.IsDBNull(1) ? "Unnamed" : reader.GetString(1);
                var assetTypeId = reader.GetInt32(3);
                var thumb = reader.IsDBNull(2) ? GetFallbackThumbnailUrl(assetTypeId) : reader.GetString(2);
                var creatorName = reader.IsDBNull(4) ? "ROBLOX" : reader.GetString(4);
                var createdAt = reader.GetDateTime(5);
                var onSale = !reader.IsDBNull(6) && reader.GetBoolean(6);
                var price = reader.IsDBNull(7) ? (int?)null : reader.GetInt32(7);
                var priceTix = reader.IsDBNull(8) ? (int?)null : reader.GetInt32(8);

                items.Add(new Assemblies.Common.DevelopItem
                {
                    ItemId = assetId,
                    AssetId = assetId,
                    RootPlaceId = assetId,
                    Name = name,
                    ThumbnailUrl = thumb,
                    Type = assetTypeId switch
                    {
                        3 => "audios",
                        4 => "meshes",
                        10 => "models",
                        13 => "decals",
                        38 => "plugins",
                        _ => "models",
                    },
                    CatalogUrl = "/catalog/" + assetId + "/" + Assemblies.Common.DevelopSlugHelper.Slug(name),
                    CreatedAt = createdAt,
                    IsOnSale = onSale,
                    PriceRobux = price,
                    PriceTickets = priceTix,
                });
            }

            vm.LibraryItems = items;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] PopulateLibraryAsync: {ex}");
            vm.LibraryItems = new List<Assemblies.Common.DevelopItem>();
        }
    }

    private async Task PopulatePluginsAsync(Assemblies.Common.DevelopTabViewModel vm, CancellationToken cancellationToken)
    {
        vm.AssetTypeId = 38;
        vm.HeaderText = "Plugins";
        vm.MaxActiveCount = 0;

        var connStr = ConnectionString;
        if (string.IsNullOrWhiteSpace(connStr) || vm.UserId <= 0)
        {
            return;
        }

        try
        {
            const string sql = @"select a.asset_id, a.name, a.created_at, a.thumbnail_url, COALESCE(a.sales, 0) as sales
                from assets a
                where a.owner_user_id = @uid
                  and a.asset_type_id = 38
                order by a.created_at desc, a.asset_id desc
                limit 50;";

            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("uid", vm.UserId);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

            var items = new List<Assemblies.Common.DevelopItem>();
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var assetId = reader.GetInt64(0);
                var name = reader.IsDBNull(1) ? "Unnamed" : reader.GetString(1);
                var createdAt = reader.GetDateTime(2);
                var thumb = reader.IsDBNull(3) ? DefaultThumbnailUrl : reader.GetString(3);
                var sales = reader.IsDBNull(4) ? 0L : reader.GetInt64(4);

                items.Add(new Assemblies.Common.DevelopItem
                {
                    ItemId = assetId,
                    AssetId = assetId,
                    RootPlaceId = assetId,
                    Name = name,
                    ThumbnailUrl = thumb,
                    Type = "plugins",
                    ConfigureUrl = "/asset/" + assetId + "/configure",
                    CatalogUrl = "/catalog/" + assetId + "/" + Assemblies.Common.DevelopSlugHelper.Slug(name),
                    CreatedAt = createdAt,
                    Sales = sales,
                });
            }

            vm.Plugins = items;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] PopulatePluginsAsync: {ex}");
            vm.Plugins = new List<Assemblies.Common.DevelopItem>();
        }
    }
}
