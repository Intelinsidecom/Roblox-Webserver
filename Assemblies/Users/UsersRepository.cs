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
    previous_user_names,
    moderation_status,
    premium_member,
    subscription_type,
    subscription_expiration_date,
    subscription_renewal_date,
    loyal_since,
    current_location,
    xbox_user,
    email,
    phone_number,
    password,
    birthday,
    gender,
    ""2sv_enabled"",
    ""2sv_verification_types"",
    user_created,
    last_location,
    last_activity,
    role_set,
    last_payment_at,
    last_purchase_at,
    robux_balance,
    badges_count,
    friends_count,
    followers_count,
    following_count,
    favorite_games_count,
    favorite_items_count,
    blocked_users,
    presence_universe_id,
    presence_place_id,
    last_seen_game_id,
    last_seen_universe_id,
    last_login_ip,
    last_login_at,
    email_verified,
    phone_verified,
    account_restrictions_enabled,
    ""2sv_recovery_codes_set"",
    password_last_changed_at,
    description_bio,
    avatar_thumbnail_url,
    headshot_thumbnail_url,
    profile_visibility,
    inventory_privacy,
    can_pm,
    can_chat,
    can_trade,
    chat_privacy_level,
    is_developer,
    account_age_days,
    locale,
    language,
    country_iso
) values (
    @user_id,
    @user_name,
    '[]'::jsonb,
    @moderation_status,
    false,
    NULL,
    NULL,
    NULL,
    NULL,
    @current_location,
    false,
    @email,
    @phone_number,
    @password,
    @birthday,
    @gender::gender_enum,
    false,
    '[]'::jsonb,
    @user_created,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    false,
    false,
    false,
    false,
    NULL,
    NULL,
    NULL,
    NULL,
    'public',
    'public',
    false,
    false,
    false,
    NULL,
    false,
    0,
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
                    cmd.Parameters.AddWithValue("user_created", (object)p.UserCreated ?? DateTimeOffset.UtcNow);
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
