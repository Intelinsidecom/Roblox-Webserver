namespace Thumbnails;

public sealed class Avatar3DCacheResult
{
    public string Hash { get; set; } = string.Empty;
    public string DirectoryPath { get; set; } = string.Empty;
    public string ObjFileName { get; set; } = "avatar.obj";
    public string MtlFileName { get; set; } = "avatar.mtl";
    public bool AlreadyExisted { get; set; }
}
