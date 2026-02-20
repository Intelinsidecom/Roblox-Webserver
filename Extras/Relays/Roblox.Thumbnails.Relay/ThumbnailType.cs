using System.Runtime.Serialization;

namespace Roblox.Thumbnails.Relay
{
    public enum ThumbnailType
    {
        Avatar = 1,
        AvatarHeadShot,

        // Face and decal are handled with the same lua script
        Face,
        Decal,

        Gear,
        Hat,
        Head,
        MeshPart,
        Mesh,
        Model,
        Package,
        Pants,
        Place,
        Shirt
    }
}
