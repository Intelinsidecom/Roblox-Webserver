using System;
using System.Collections.Generic;

namespace Assemblies.Common;

public sealed class DevelopTabViewModel
{
    public string ViewName { get; set; } = "Games";
    public int AssetTypeId { get; set; }
    public long UserId { get; set; }
    public string? UserName { get; set; }
    public long? GroupId { get; set; }

    public string CreatePlaceUrl { get; set; } = "/places/create";
    public string HeaderText { get; set; } = string.Empty;
    public int ActiveCount { get; set; }
    public int MaxActiveCount { get; set; }

    public bool ShowPublicOnlyCheckbox { get; set; } = true;

    public IReadOnlyList<DevelopItem> Items { get; set; } = new List<DevelopItem>();

    public IReadOnlyList<ClothingItem> TShirts { get; set; } = new List<ClothingItem>();
    public IReadOnlyList<ClothingItem> Shirts { get; set; } = new List<ClothingItem>();
    public IReadOnlyList<ClothingItem> Pants { get; set; } = new List<ClothingItem>();
    public IReadOnlyList<DevelopItem> Models { get; set; } = new List<DevelopItem>();
    public IReadOnlyList<DevelopItem> Meshes { get; set; } = new List<DevelopItem>();
    public IReadOnlyList<DevelopItem> Decals { get; set; } = new List<DevelopItem>();
    public IReadOnlyList<DevelopItem> Audios { get; set; } = new List<DevelopItem>();
    public IReadOnlyList<DevelopItem> LibraryItems { get; set; } = new List<DevelopItem>();

    public int SelectedCategory { get; set; }
    public int SelectedSortType { get; set; }
    public List<int> SelectedGenres { get; set; } = new();
    public int TotalItems { get; set; }
    public int PageNumber { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
}

public sealed class DevelopItem
{
    public long ItemId { get; set; }
    public long AssetId { get; set; }
    public long RootPlaceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string Type { get; set; } = "universes";
    public string ConfigureUrl { get; set; } = string.Empty;
    public string ConfigureLocalizationUrl { get; set; } = string.Empty;
    public string CreateBadgeUrl { get; set; } = string.Empty;
    public string CreateGamepassUrl { get; set; } = string.Empty;
    public string DeveloperStatsUrl { get; set; } = string.Empty;
    public string AdvertiseUrl { get; set; } = string.Empty;
    public string ActivateUniverseUrl { get; set; } = string.Empty;
    public string DeactivateUniverseUrl { get; set; } = string.Empty;
    public bool InShowcase { get; set; }
    public string StartPlaceName { get; set; } = string.Empty;
    public long StartPlaceId { get; set; }
    public string StatusText { get; set; } = "Public";
    public bool IsPublic { get; set; } = true;
    public int VisitCount { get; set; }
    public string CatalogUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int? PriceRobux { get; set; }
    public int? PriceTickets { get; set; }
    public bool IsOnSale { get; set; }
}

public sealed class ClothingItem
{
    public long AssetId { get; set; }
    public long? ImageAssetId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string CatalogUrl { get; set; } = string.Empty;
    public string Type { get; set; } = "tshirts";
}
