using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Avatar
{
    /// <summary>
    /// Builds a canonical avatar render configuration and its SHA256 hash
    /// for use by 2D and 3D thumbnail caches.
    /// </summary>
    public sealed class AvatarRenderConfigBuilder
    {
        private readonly AvatarRepository _avatarRepository = new AvatarRepository();

        public async Task<(string configHash, object configObject)> BuildAvatarRenderConfigAsync(
            string connectionString,
            long userId,
            string renderType,
            int width,
            int height,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId));
            if (string.IsNullOrWhiteSpace(renderType))
                throw new ArgumentException("renderType is required", nameof(renderType));

            var avatarStateFull = await _avatarRepository
                .GetAvatarAsync(connectionString, userId, cancellationToken)
                .ConfigureAwait(false);

            var wornIds = avatarStateFull.Assets ?? Array.Empty<AvatarAssetState>();
            var wornAssetIds = wornIds
                .Select(a => a.id)
                .OrderBy(id => id)
                .ToArray();

            var configObject = new
            {
                renderType = renderType,
                width = width,
                height = height,
                bodyColors = new
                {
                    head = avatarStateFull.BodyColors.headColorId,
                    torso = avatarStateFull.BodyColors.torsoColorId,
                    rightArm = avatarStateFull.BodyColors.rightArmColorId,
                    leftArm = avatarStateFull.BodyColors.leftArmColorId,
                    rightLeg = avatarStateFull.BodyColors.rightLegColorId,
                    leftLeg = avatarStateFull.BodyColors.leftLegColorId
                },
                wornAssetIds = wornAssetIds
            };

            var json = JsonSerializer.Serialize(configObject);
            string configHash;
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(json);
                var digest = sha.ComputeHash(bytes);
                var hashSb = new StringBuilder(digest.Length * 2);
                foreach (var b in digest)
                    hashSb.Append(b.ToString("x2"));
                configHash = hashSb.ToString();
            }

            return (configHash, configObject);
        }
    }
}
