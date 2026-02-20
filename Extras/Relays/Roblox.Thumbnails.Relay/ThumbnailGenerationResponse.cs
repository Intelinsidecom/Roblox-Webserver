using System.Runtime.Serialization;

namespace Roblox.Thumbnails.Relay
{
    public class ThumbnailGenerationResponse
    {
        public string Base64EncodedThumbnailData;
        public string[] DependencyUrls;
    }
}
