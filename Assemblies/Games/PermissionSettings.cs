using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Games
{
    public sealed class PermissionSettings
    {
        public async Task UpdatePlaceCopyingAllowedAsync(string connectionString, long placeId, bool isCopyingAllowed, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (placeId <= 0)
                throw new ArgumentOutOfRangeException(nameof(placeId));

            using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = @"update assets
set is_copying_allowed = @is_copying_allowed,
    last_updated = now()
where asset_id = @asset_id and is_place = true;";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("asset_id", placeId);
            cmd.Parameters.AddWithValue("is_copying_allowed", isCopyingAllowed);

            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task UpdatePlaceGearPermissionsAsync(string connectionString, long placeId, bool isAllGenresAllowed, string allowedGearTypesJson, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (placeId <= 0)
                throw new ArgumentOutOfRangeException(nameof(placeId));

            using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = @"update assets
set is_all_genres_allowed = @is_all_genres_allowed,
    allowed_gear_types = @allowed_gear_types,
    last_updated = now()
where asset_id = @asset_id and is_place = true;";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("asset_id", placeId);
            cmd.Parameters.AddWithValue("is_all_genres_allowed", isAllGenresAllowed);
            cmd.Parameters.AddWithValue("allowed_gear_types", string.IsNullOrWhiteSpace(allowedGearTypesJson) ? "[]::jsonb" : allowedGearTypesJson);

            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
