namespace Games;

public sealed class UniverseInfo
{
    public long UniverseId { get; set; }
    public long RootPlaceId { get; set; }
    public long CreatorUserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public int PrivacyLevel { get; set; } = 1;
    public bool Studio_Access_To_APIs { get; set; } = false;
}
