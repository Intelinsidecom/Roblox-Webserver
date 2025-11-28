using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Users
{
    public class UsersRepository
    {
        public async Task<int> CreateUserAsync(string connectionString, UserCreateParams p, bool failIfExists = true, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));

            p = p ?? throw new ArgumentNullException(nameof(p));
            p.Normalize();

            using (var conn = new NpgsqlConnection(connectionString))
            {
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

                // Optionally ensure not exists
                if (failIfExists)
                {
                    using (var checkCmd = new NpgsqlCommand("select 1 from users where user_id = @uid", conn))
                    {
                        checkCmd.Parameters.AddWithValue("uid", p.UserId);
                        var exists = await checkCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                        if (exists != null)
                            throw new InvalidOperationException($"User with id {p.UserId} already exists.");
                    }
                }

                const string sql = @"
insert into users (
    user_id,
    user_name,
    moderation_status,
    current_location,
    email,
    phone_number,
    password,
    birthday,
    gender,
    user_created,
    locale,
    language,
    country_iso
) values (
    @user_id,
    @user_name,
    @moderation_status,
    @current_location,
    @email,
    @phone_number,
    @password,
    @birthday,
    cast(@gender as gender_enum),
    @user_created,
    @locale,
    @language,
    @country_iso
);";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("user_id", p.UserId);
                    cmd.Parameters.AddWithValue("user_name", p.UserName);
                    cmd.Parameters.AddWithValue("moderation_status", (object)p.ModerationStatus ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("current_location", (object)p.CurrentLocation ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("email", (object)p.Email ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("phone_number", (object)p.PhoneNumber ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("password", (object)p.Password ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("birthday", (object)p.Birthday ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("gender", (object)p.Gender ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("user_created", (object?)p.UserCreated ?? DateTimeOffset.UtcNow);
                    cmd.Parameters.AddWithValue("locale", (object)p.Locale ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("language", (object)p.Language ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("country_iso", (object)p.CountryIso ?? DBNull.Value);

                    var rows = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                    return rows;
                }
            }
        }
    }
}
