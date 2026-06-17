using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Assets;
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

    public async Task<Assemblies.Common.DevelopTabViewModel> BuildAsync(
        long userId,
        string? userName,
        string viewName,
        bool showPublicOnly = false,
        long? groupId = null,
        CancellationToken cancellationToken = default)
    {
        var vm = new Assemblies.Common.DevelopTabViewModel
        {
            ViewName = viewName,
            UserId = userId,
            UserName = userName,
            GroupId = groupId,
        };

        switch (viewName)
        {
            case "Games":
                await PopulateGamesAsync(vm, showPublicOnly, cancellationToken).ConfigureAwait(false);
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
            default:
                break;
        }

        return vm;
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
        catch
        {
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
        i.asset_id as image_asset_id
from user_assets ua_t
join assets t on t.asset_id = ua_t.asset_id and t.asset_type_id = 2 and t.owner_user_id = @uid
left join assets i on i.owner_user_id = t.owner_user_id
                  and i.asset_type_id = 1
                  and i.name = t.name || ' Image'
where ua_t.user_id = @uid
order by ua_t.created_at desc, t.asset_id desc
limit 50;";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("uid", vm.UserId);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

            var items = new List<Assemblies.Common.ClothingItem>();
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var assetId = reader.GetInt64(0);
                var name = reader.IsDBNull(1) ? "Unnamed" : reader.GetString(1);
                var createdAt = reader.GetDateTime(2);
                var thumb = reader.IsDBNull(3) ? null : reader.GetString(3);
                var imageAssetId = reader.IsDBNull(4) ? (long?)null : reader.GetInt64(4);

                items.Add(new Assemblies.Common.ClothingItem
                {
                    AssetId = assetId,
                    ImageAssetId = imageAssetId,
                    Name = name,
                    CreatedAt = createdAt,
                    ThumbnailUrl = thumb,
                    Type = "tshirts",
                    CatalogUrl = "/catalog/" + assetId + "/" + Assemblies.Common.DevelopSlugHelper.Slug(name),
                });
            }

            vm.TShirts = items;
        }
        catch
        {
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

            vm.Shirts = shirts.Select(s => new Assemblies.Common.ClothingItem
            {
                AssetId = s.AssetId,
                ImageAssetId = s.ImageAssetId,
                Name = s.Name,
                CreatedAt = s.CreatedAt,
                ThumbnailUrl = s.ThumbnailUrl,
                Type = "shirts",
                CatalogUrl = "/catalog/" + s.AssetId + "/" + Assemblies.Common.DevelopSlugHelper.Slug(s.Name),
            }).ToList();
        }
        catch
        {
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

            vm.Pants = pants.Select(p => new Assemblies.Common.ClothingItem
            {
                AssetId = p.AssetId,
                ImageAssetId = p.ImageAssetId,
                Name = p.Name,
                CreatedAt = p.CreatedAt,
                ThumbnailUrl = p.ThumbnailUrl,
                Type = "pants",
                CatalogUrl = "/catalog/" + p.AssetId + "/" + Assemblies.Common.DevelopSlugHelper.Slug(p.Name),
            }).ToList();
        }
        catch
        {
            vm.Pants = new List<Assemblies.Common.ClothingItem>();
        }
    }
}
