namespace Games;

public sealed class UniverseInfo
{
    public long UniverseId { get; set; }
    public long RootPlaceId { get; set; }
    public long CreatorUserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
}
