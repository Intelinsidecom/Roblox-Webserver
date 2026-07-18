using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Users
{
    public static partial class UserQueries
    {
        /// <summary>
        /// Gets the membership type string for a user by id.
        /// </summary>
        /// <param name="connectionString">Connection string to the Postgres database.</param>
        /// <param name="userId">User id.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Membership type string ("BuildersClub", "TurboBuildersClub", "OutrageousBuildersClub", or "None").</returns>
        public static async Task<string> GetMembershipTypeAsync(string connectionString, long userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                return "None";

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            using var cmd = new NpgsqlCommand("SELECT membership_status FROM users WHERE user_id = @uid", conn);
            cmd.Parameters.AddWithValue("uid", userId);
            var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);

            if (result == null || result == DBNull.Value)
                return "None";

            var membershipStatus = Convert.ToInt16(result);
            return membershipStatus switch
            {
                1 => "BuildersClub",
                2 => "TurboBuildersClub",
                3 => "OutrageousBuildersClub",
                _ => "None"
            };
        }
    }
}
