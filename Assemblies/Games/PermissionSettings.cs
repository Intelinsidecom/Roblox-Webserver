using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;

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
set is_copying_allowed = @is_copying_allowed
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
    allowed_gear_types = @allowed_gear_types
where asset_id = @asset_id and is_place = true;";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("asset_id", placeId);
            cmd.Parameters.AddWithValue("is_all_genres_allowed", isAllGenresAllowed);
            
            // Handle JSONB parameter properly
            var jsonParam = new NpgsqlParameter("allowed_gear_types", NpgsqlDbType.Jsonb);
            jsonParam.Value = string.IsNullOrWhiteSpace(allowedGearTypesJson) ? "[]" : allowedGearTypesJson;
            cmd.Parameters.Add(jsonParam);

            var rowsAffected = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            
            // Verify the update by reading back the data
            const string verifySql = @"select is_all_genres_allowed, allowed_gear_types from assets where asset_id = @asset_id and is_place = true;";
            using var verifyCmd = new NpgsqlCommand(verifySql, conn);
            verifyCmd.Parameters.AddWithValue("asset_id", placeId);
            
            using var reader = await verifyCmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var savedIsAllGenresAllowed = reader.GetBoolean(0);
                var savedAllowedGearTypes = reader.IsDBNull(1) ? "NULL" : reader.GetString(1);
            }
            else
            {
            }
        }

        public async Task UpdatePlaceInGamePermissionsAsync(string connectionString, long placeId, bool allowPlaceToBeCopiedInGame, bool allowPlaceToBeUpdatedInGame, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (placeId <= 0)
                throw new ArgumentOutOfRangeException(nameof(placeId));

            using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = @"update assets
set allow_place_to_be_copied_in_game = @allow_place_to_be_copied_in_game,
    allow_place_to_be_updated_in_game = @allow_place_to_be_updated_in_game
where asset_id = @asset_id and is_place = true;";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("asset_id", placeId);
            cmd.Parameters.AddWithValue("allow_place_to_be_copied_in_game", allowPlaceToBeCopiedInGame);
            cmd.Parameters.AddWithValue("allow_place_to_be_updated_in_game", allowPlaceToBeUpdatedInGame);

            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
