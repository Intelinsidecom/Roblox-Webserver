using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

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

            var bodyColors = await _bodyColorsRepository
                .GetBodyColorsAsync(connectionString, userId, cancellationToken)
                .ConfigureAwait(false);

            state.BodyColors.headColorId = bodyColors.HeadColorId;
            state.BodyColors.torsoColorId = bodyColors.TorsoColorId;
            state.BodyColors.rightArmColorId = bodyColors.RightArmColorId;
            state.BodyColors.leftArmColorId = bodyColors.LeftArmColorId;
            state.BodyColors.rightLegColorId = bodyColors.RightLegColorId;
            state.BodyColors.leftLegColorId = bodyColors.LeftLegColorId;
            state.Assets = await GetWornAssetStatesAsync(connectionString, userId, cancellationToken).ConfigureAwait(false);

            return state;
        }

        private async Task<AvatarAssetState[]> GetWornAssetStatesAsync(string connectionString, long userId, CancellationToken cancellationToken)
        {
            var states = new List<AvatarAssetState>();

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = @"select w.asset_id, a.name, a.asset_type_id
from avatar_worn_assets w
left join assets a on a.asset_id = w.asset_id
where w.user_id = @uid
order by w.asset_id";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("uid", userId);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var assetTypeId = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                states.Add(new AvatarAssetState
                {
                    id = reader.GetInt64(0),
                    name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    assetType = new AvatarAssetTypeState
                    {
                        id = assetTypeId,
                        name = GetAssetTypeName(assetTypeId)
                    }
                });
            }

            return states.ToArray();
        }

        private static string GetAssetTypeName(int assetTypeId)
        {
            return assetTypeId switch
            {
                2 => "T-Shirt",
                8 => "Hat",
                11 => "Shirt",
                12 => "Pants",
                16 => "Face",
                17 => "Head",
                21 => "Face Accessory",
                24 or 27 => "Gear",
                32 => "Package",
                41 => "Hair",
                42 or 43 => "Hat",
                _ => "Asset",
            };
        }
    }
}

