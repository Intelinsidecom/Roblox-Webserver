using System;
using System.Threading;
using System.Threading.Tasks;

namespace Avatar
{
    public sealed class AvatarState
    {
        public BodyColorsState BodyColors { get; set; } = new BodyColorsState();
        public ScalesState Scales { get; set; } = new ScalesState();
        public AvatarAssetState[] Assets { get; set; } = Array.Empty<AvatarAssetState>();
    }

    public sealed class BodyColorsState
    {
        public int headColorId { get; set; }
        public int torsoColorId { get; set; }
        public int rightArmColorId { get; set; }
        public int leftArmColorId { get; set; }
        public int rightLegColorId { get; set; }
        public int leftLegColorId { get; set; }
    }

    public sealed class ScalesState
    {
        public double height { get; set; } = 1.0;
        public double width { get; set; } = 1.0;
        public double head { get; set; } = 1.0;
    }

    public sealed class AvatarAssetState
    {
        public long id { get; set; }
        public string name { get; set; } = string.Empty;
        public AvatarAssetTypeState assetType { get; set; } = new AvatarAssetTypeState();
    }

    public sealed class AvatarAssetTypeState
    {
        public int id { get; set; }
        public string name { get; set; } = string.Empty;
    }

    public sealed class AvatarRepository
    {
        private readonly BodyColorsRepository _bodyColorsRepository = new BodyColorsRepository();

        public async Task<AvatarState> GetAvatarAsync(string connectionString, long userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId));

            var state = new AvatarState();

            var bodyColors = await _bodyColorsRepository.GetBodyColorsAsync(connectionString, userId, cancellationToken)
                .ConfigureAwait(false);

            state.BodyColors.headColorId = bodyColors.HeadColorId;
            state.BodyColors.torsoColorId = bodyColors.TorsoColorId;
            state.BodyColors.rightArmColorId = bodyColors.RightArmColorId;
            state.BodyColors.leftArmColorId = bodyColors.LeftArmColorId;
            state.BodyColors.rightLegColorId = bodyColors.RightLegColorId;
            state.BodyColors.leftLegColorId = bodyColors.LeftLegColorId;

            // Scales and Assets are currently not stored in the database in this project
            // and are therefore returned with conservative defaults.

            return state;
        }
    }
}

