using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Users
{
    public static partial class UserQueries
    {
        /// <summary>
        /// Resets a user's avatar by dequipping all worn items and resetting body colors to defaults.
        /// </summary>
        /// <param name="connectionString">Connection string to the Postgres database.</param>
        /// <param name="userId">User id.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if successful, false if user not found.</returns>
        public static async Task<bool> ResetAvatarAsync(string connectionString, long userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                return false;

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var tx = conn.BeginTransaction();

            try
            {
                // 1. Remove all worn assets
                const string deleteWornSql = @"delete from avatar_worn_assets where user_id = @uid";
                using (var deleteCmd = new NpgsqlCommand(deleteWornSql, conn, tx))
                {
                    deleteCmd.Parameters.AddWithValue("uid", userId);
                    await deleteCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                }

                // 2. Reset body colors to defaults (all color ID = 1)
                const string resetColorsSql = @"update bodycolors 
                    set head_color = 1, 
                        torso_color = 1, 
                        right_arm_color = 1, 
                        left_arm_color = 1, 
                        right_leg_color = 1, 
                        left_leg_color = 1 
                    where user_id = @uid";
                using (var resetCmd = new NpgsqlCommand(resetColorsSql, conn, tx))
                {
                    resetCmd.Parameters.AddWithValue("uid", userId);
                    var colorRows = await resetCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                    
                    // If no rows were updated, the user might not have body colors, so insert defaults
                    if (colorRows == 0)
                    {
                        const string insertColorsSql = @"insert into bodycolors(
                            user_id, head_color, left_arm_color, left_leg_color, right_arm_color, right_leg_color, torso_color
                        ) values(@uid, 1, 1, 1, 1, 1, 1)
                        on conflict (user_id) do update set
                            head_color = excluded.head_color,
                            left_arm_color = excluded.left_arm_color,
                            left_leg_color = excluded.left_leg_color,
                            right_arm_color = excluded.right_arm_color,
                            right_leg_color = excluded.right_leg_color,
                            torso_color = excluded.torso_color";
                        using var insertCmd = new NpgsqlCommand(insertColorsSql, conn, tx);
                        insertCmd.Parameters.AddWithValue("uid", userId);
                        await insertCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                    }
                }

                await tx.CommitAsync(cancellationToken).ConfigureAwait(false);
                return true;
            }
            catch
            {
                await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
                throw;
            }
        }

        /// <summary>
        /// Gets the count of worn assets for a user.
        /// </summary>
        /// <param name="connectionString">Connection string to the Postgres database.</param>
        /// <param name="userId">User id.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Number of worn assets, or 0 if user not found.</returns>
        public static async Task<int> GetWornAssetsCountAsync(string connectionString, long userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                return 0;

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = @"select count(*) from avatar_worn_assets where user_id = @uid";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("uid", userId);

            var obj = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            return obj is long l ? (int)l : (obj is int i ? i : 0);
        }

        /// <summary>
        /// Checks if a user has any custom body colors (non-default colors).
        /// </summary>
        /// <param name="connectionString">Connection string to the Postgres database.</param>
        /// <param name="userId">User id.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if user has custom body colors, false if all are defaults or user not found.</returns>
        public static async Task<bool> HasCustomBodyColorsAsync(string connectionString, long userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                return false;

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = @"select exists(
                select 1 from bodycolors 
                where user_id = @uid 
                and (head_color != 1 or torso_color != 1 or right_arm_color != 1 
                     or left_arm_color != 1 or right_leg_color != 1 or left_leg_color != 1)
            )";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("uid", userId);

            var obj = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            return obj is bool b && b;
        }
    }
}
