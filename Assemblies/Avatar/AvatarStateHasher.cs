using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Avatar
{
    public static class AvatarStateHasher
    {
        public static async Task<string> RecomputeAndStoreAvatarHashAsync(
            string connectionString,
            long userId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId));

            // Load current avatar state (body colors + worn assets)
            var repo = new AvatarRepository();
            var state = await repo.GetAvatarAsync(connectionString, userId, cancellationToken).ConfigureAwait(false);

            var hash = ComputeHash(state);

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = @"update users set avatar_state_hash = @h where user_id = @uid";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("h", (object?)hash ?? DBNull.Value);
            cmd.Parameters.AddWithValue("uid", userId);
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            return hash;
        }

        public static string ComputeHash(AvatarState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            var sb = new StringBuilder();

            // Body colors
            var c = state.BodyColors;
            sb.Append("colors:");
            sb.Append("head=").Append(c.headColorId).Append(';');
            sb.Append("torso=").Append(c.torsoColorId).Append(';');
            sb.Append("rarm=").Append(c.rightArmColorId).Append(';');
            sb.Append("larm=").Append(c.leftArmColorId).Append(';');
            sb.Append("rleg=").Append(c.rightLegColorId).Append(';');
            sb.Append("lleg=").Append(c.leftLegColorId).Append("|");

            // Assets (just IDs, sorted for stability)
            var assetIds = (state.Assets ?? Array.Empty<AvatarAssetState>())
                .Select(a => a.id)
                .OrderBy(id => id)
                .ToArray();

            sb.Append("assets:");
            sb.Append(string.Join(",", assetIds));

            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var hashBytes = sha.ComputeHash(bytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
