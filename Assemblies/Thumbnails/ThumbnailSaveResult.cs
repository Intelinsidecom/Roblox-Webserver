namespace Thumbnails;

public sealed class ThumbnailSaveResult
{
    public string Hash { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public bool AlreadyExisted { get; set; }
}
