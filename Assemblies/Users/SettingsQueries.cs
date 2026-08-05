using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Users
{
    /// <summary>
    /// Single notification band entry. The on-disk form is a JSONB array of these.
    /// </summary>
    public sealed class NotificationBand
    {
        public string ReceiverDestinationType { get; set; } = "NotificationStream";
        public string NotificationSourceType { get; set; } = "Test";
        public bool IsEnabled { get; set; }
    }

    /// <summary>
    /// Snapshot of every per-column user setting on the account settings page.
    /// One DB roundtrip yields the whole object via <see cref="UserQueries.GetSettingsAsync"/>.
    /// </summary>
    public sealed class UserSettings
    {
        public string AppChatPrivacy { get; set; } = "Friends";
        public string GameChatPrivacy { get; set; } = "AllUsers";
        public string PrivateMessagePrivacy { get; set; } = "Friends";
        public string PrivateServerInvitePrivacy { get; set; } = "Friends";
        public string FollowMePrivacy { get; set; } = "Friends";
        public string TradePrivacy { get; set; } = "Friends";
        public short TradeValue { get; set; }

        public bool AccountPinEnabled { get; set; }
        public long AccountPinUnlockedUntil { get; set; }
        public string? AccountPinHash { get; set; }
        public string? AccountPinSalt { get; set; }

        public string? SocialFacebookUrl { get; set; }
        public string? SocialTwitterUrl { get; set; }
        public string? SocialGoogleplusUrl { get; set; }
        public string? SocialYoutubeUrl { get; set; }
        public string? SocialTwitchUrl { get; set; }
        public short SocialNetworksVisibility { get; set; } = 6;

        public bool ReceiveNewsletter { get; set; }

        public IReadOnlyList<string> OptedOutReceiverDestinationTypes { get; set; } = Array.Empty<string>();
        public IReadOnlyList<NotificationBand> NotificationBands { get; set; } = Array.Empty<NotificationBand>();
    }

    /// <summary>
    /// Privacy values are stored as the verbatim wire strings the Angular frontend
    /// sends ("All", "Followers", "Following", "Friends", "NoOne", "AllUsers").
    /// </summary>
    public static partial class UserQueries
    {
        private const string DefaultAppChatPrivacy = "Friends";
        private const string DefaultGameChatPrivacy = "AllUsers";
        private const string DefaultMessagingPrivacy = "Friends";

        private static string NonNull(string? v, string fallback)
            => string.IsNullOrEmpty(v) ? fallback : v!;

        private static void Guard(string connectionString, long userId, string connParam = "connectionString")
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException($"{connParam} is required", connParam);
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId), "userId must be > 0");
        }

        /// <summary>
        /// Reads all per-column settings in one query.
        /// </summary>
        public static async Task<UserSettings> GetSettingsAsync(
            string connectionString,
            long userId,
            CancellationToken cancellationToken = default)
        {
            Guard(connectionString, userId);
            const string sql = @"
                select
                    app_chat_privacy,
                    game_chat_privacy,
                    private_message_privacy,
                    private_server_invite_privacy,
                    follow_me_privacy,
                    trade_privacy,
                    trade_value,
                    account_pin_enabled,
                    account_pin_unlocked_until,
                    account_pin_hash,
                    account_pin_salt,
                    social_facebook_url,
                    social_twitter_url,
                    social_googleplus_url,
                    social_youtube_url,
                    social_twitch_url,
                    social_networks_visibility,
                    receive_newsletter,
                    coalesce(opted_out_recv_dst_types, '{}'),
                    coalesce(notification_bands, '[]'::jsonb)
                from users
                where user_id = @uid";

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("uid", userId);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            if (!await reader.ReadAsync().ConfigureAwait(false))
                throw new InvalidOperationException($"user {userId} not found");

            var s = new UserSettings();
            s.AppChatPrivacy = NonNull(reader.GetString(0), DefaultAppChatPrivacy);
            s.GameChatPrivacy = NonNull(reader.GetString(1), DefaultGameChatPrivacy);
            s.PrivateMessagePrivacy = NonNull(reader.GetString(2), DefaultMessagingPrivacy);
            s.PrivateServerInvitePrivacy = NonNull(reader.GetString(3), DefaultMessagingPrivacy);
            s.FollowMePrivacy = NonNull(reader.GetString(4), DefaultMessagingPrivacy);
            s.TradePrivacy = NonNull(reader.GetString(5), DefaultMessagingPrivacy);
            s.TradeValue = reader.GetInt16(6);
            s.AccountPinEnabled = reader.GetBoolean(7);
            s.AccountPinUnlockedUntil = reader.GetInt64(8);
            s.AccountPinHash = reader.IsDBNull(9) ? null : reader.GetString(9);
            s.AccountPinSalt = reader.IsDBNull(10) ? null : reader.GetString(10);
            s.SocialFacebookUrl = reader.IsDBNull(11) ? null : reader.GetString(11);
            s.SocialTwitterUrl = reader.IsDBNull(12) ? null : reader.GetString(12);
            s.SocialGoogleplusUrl = reader.IsDBNull(13) ? null : reader.GetString(13);
            s.SocialYoutubeUrl = reader.IsDBNull(14) ? null : reader.GetString(14);
            s.SocialTwitchUrl = reader.IsDBNull(15) ? null : reader.GetString(15);
            s.SocialNetworksVisibility = reader.GetInt16(16);
            s.ReceiveNewsletter = reader.GetBoolean(17);
            s.OptedOutReceiverDestinationTypes = ReadStringArray(reader, 18);
            s.NotificationBands = ReadNotificationBands(reader, 19);
            return s;
        }

        public static Task<string> GetAppChatPrivacyAsync(string connectionString, long userId, CancellationToken ct = default)
            => GetPrivacyColumnAsync(connectionString, userId, "app_chat_privacy", DefaultAppChatPrivacy, ct);

        public static Task SetAppChatPrivacyAsync(string connectionString, long userId, string value, CancellationToken ct = default)
            => SetPrivacyColumnAsync(connectionString, userId, "app_chat_privacy", NonNull(value, DefaultAppChatPrivacy), ct);

        public static Task<string> GetGameChatPrivacyAsync(string connectionString, long userId, CancellationToken ct = default)
            => GetPrivacyColumnAsync(connectionString, userId, "game_chat_privacy", DefaultGameChatPrivacy, ct);

        public static Task SetGameChatPrivacyAsync(string connectionString, long userId, string value, CancellationToken ct = default)
            => SetPrivacyColumnAsync(connectionString, userId, "game_chat_privacy", NonNull(value, DefaultGameChatPrivacy), ct);

        public static Task<string> GetPrivateMessagePrivacyAsync(string connectionString, long userId, CancellationToken ct = default)
            => GetPrivacyColumnAsync(connectionString, userId, "private_message_privacy", DefaultMessagingPrivacy, ct);

        public static Task SetPrivateMessagePrivacyAsync(string connectionString, long userId, string value, CancellationToken ct = default)
            => SetPrivacyColumnAsync(connectionString, userId, "private_message_privacy", NonNull(value, DefaultMessagingPrivacy), ct);

        public static Task<string> GetPrivateServerInvitePrivacyAsync(string connectionString, long userId, CancellationToken ct = default)
            => GetPrivacyColumnAsync(connectionString, userId, "private_server_invite_privacy", DefaultMessagingPrivacy, ct);

        public static Task SetPrivateServerInvitePrivacyAsync(string connectionString, long userId, string value, CancellationToken ct = default)
            => SetPrivacyColumnAsync(connectionString, userId, "private_server_invite_privacy", NonNull(value, DefaultMessagingPrivacy), ct);

        public static Task<string> GetFollowMePrivacyAsync(string connectionString, long userId, CancellationToken ct = default)
            => GetPrivacyColumnAsync(connectionString, userId, "follow_me_privacy", DefaultMessagingPrivacy, ct);

        public static Task SetFollowMePrivacyAsync(string connectionString, long userId, string value, CancellationToken ct = default)
            => SetPrivacyColumnAsync(connectionString, userId, "follow_me_privacy", NonNull(value, DefaultMessagingPrivacy), ct);

        public static Task<string> GetTradePrivacyAsync(string connectionString, long userId, CancellationToken ct = default)
            => GetPrivacyColumnAsync(connectionString, userId, "trade_privacy", DefaultMessagingPrivacy, ct);

        public static Task SetTradePrivacyAsync(string connectionString, long userId, string value, CancellationToken ct = default)
            => SetPrivacyColumnAsync(connectionString, userId, "trade_privacy", NonNull(value, DefaultMessagingPrivacy), ct);

        public static async Task<short> GetTradeValueAsync(string connectionString, long userId, CancellationToken ct = default)
        {
            Guard(connectionString, userId);
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);
            using var cmd = new NpgsqlCommand("select trade_value from users where user_id = @uid", conn);
            cmd.Parameters.AddWithValue("uid", userId);
            var r = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
            return r == null || r is DBNull ? (short)0 : Convert.ToInt16(r);
        }

        public static Task SetTradeValueAsync(string connectionString, long userId, short value, CancellationToken ct = default)
        {
            if (value < 0) value = 0;
            if (value > 3) value = 3;
            return ExecNoResultAsync(connectionString, userId,
                "update users set trade_value = @v where user_id = @uid",
                c => c.Parameters.AddWithValue("v", value), ct);
        }

        public static async Task<bool> GetAccountRestrictionsEnabledAsync(string connectionString, long userId, CancellationToken ct = default)
        {
            Guard(connectionString, userId);
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);
            using var cmd = new NpgsqlCommand("select account_restrictions_enabled from users where user_id = @uid", conn);
            cmd.Parameters.AddWithValue("uid", userId);
            var r = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
            return r != null && r != DBNull.Value && Convert.ToBoolean(r);
        }

        public static Task SetAccountRestrictionsEnabledAsync(string connectionString, long userId, bool enabled, CancellationToken ct = default)
            => ExecNoResultAsync(connectionString, userId,
                "update users set account_restrictions_enabled = @v where user_id = @uid",
                c => c.Parameters.AddWithValue("v", enabled), ct);

        public static async Task<(bool enabled, long unlockedUntil)> GetAccountPinAsync(string connectionString, long userId, CancellationToken ct = default)
        {
            Guard(connectionString, userId);
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);
            using var cmd = new NpgsqlCommand("select account_pin_enabled, account_pin_unlocked_until from users where user_id = @uid", conn);
            cmd.Parameters.AddWithValue("uid", userId);
            await using var r = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
            if (!await r.ReadAsync().ConfigureAwait(false))
                return (false, 0);
            return (r.GetBoolean(0), r.GetInt64(1));
        }

        public static Task SetAccountPinAsync(string connectionString, long userId, bool enabled, CancellationToken ct = default)
            => ExecNoResultAsync(connectionString, userId,
                "update users set account_pin_enabled = @v where user_id = @uid",
                c => c.Parameters.AddWithValue("v", enabled), ct);

        public static Task SetAccountPinHashAsync(string connectionString, long userId, string? hash, string? salt, CancellationToken ct = default)
            => ExecNoResultAsync(connectionString, userId,
                "update users set account_pin_hash = @h, account_pin_salt = @s where user_id = @uid",
                c =>
                {
                    c.Parameters.AddWithValue("h", (object?)hash ?? DBNull.Value);
                    c.Parameters.AddWithValue("s", (object?)salt ?? DBNull.Value);
                }, ct);

        public static Task SetAccountPinUnlockedUntilAsync(string connectionString, long userId, long unixSeconds, CancellationToken ct = default)
            => ExecNoResultAsync(connectionString, userId,
                "update users set account_pin_unlocked_until = @v where user_id = @uid",
                c => c.Parameters.AddWithValue("v", unixSeconds), ct);

        /// <summary>
        /// Reads the five social URLs plus the visibility level in one roundtrip.
        /// </summary>
        public static async Task<(string? facebook, string? twitter, string? googleplus, string? youtube, string? twitch, short visibility)>
            GetSocialNetworksAsync(string connectionString, long userId, CancellationToken ct = default)
        {
            Guard(connectionString, userId);
            const string sql = @"
                select social_facebook_url, social_twitter_url, social_googleplus_url,
                       social_youtube_url, social_twitch_url, social_networks_visibility
                from users
                where user_id = @uid";

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("uid", userId);
            await using var r = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
            if (!await r.ReadAsync().ConfigureAwait(false))
                return (null, null, null, null, null, 6);
            return (
                r.IsDBNull(0) ? null : r.GetString(0),
                r.IsDBNull(1) ? null : r.GetString(1),
                r.IsDBNull(2) ? null : r.GetString(2),
                r.IsDBNull(3) ? null : r.GetString(3),
                r.IsDBNull(4) ? null : r.GetString(4),
                r.GetInt16(5));
        }

        /// <summary>Atomic all-in-one social network update so the controller doesn't issue 6 roundtrips.</summary>
        public static Task SetSocialNetworksAsync(
            string connectionString, long userId,
            string? facebook, string? twitter, string? googleplus, string? youtube, string? twitch,
            short visibility, CancellationToken ct = default)
        {
            Guard(connectionString, userId);
            const string sql = @"
                update users set
                    social_facebook_url        = @fb,
                    social_twitter_url         = @tw,
                    social_googleplus_url      = @gp,
                    social_youtube_url         = @yt,
                    social_twitch_url          = @tc,
                    social_networks_visibility = @vis
                where user_id = @uid";
            return ExecNoResultAsync(connectionString, userId, sql,
                c =>
                {
                    c.Parameters.AddWithValue("fb", (object?)facebook ?? DBNull.Value);
                    c.Parameters.AddWithValue("tw", (object?)twitter ?? DBNull.Value);
                    c.Parameters.AddWithValue("gp", (object?)googleplus ?? DBNull.Value);
                    c.Parameters.AddWithValue("yt", (object?)youtube ?? DBNull.Value);
                    c.Parameters.AddWithValue("tc", (object?)twitch ?? DBNull.Value);
                    c.Parameters.AddWithValue("vis", visibility);
                }, ct);
        }

        public static Task SetReceiveNewsletterAsync(string connectionString, long userId, bool value, CancellationToken ct = default)
            => ExecNoResultAsync(connectionString, userId,
                "update users set receive_newsletter = @v where user_id = @uid",
                c => c.Parameters.AddWithValue("v", value), ct);

        public static async Task<IReadOnlyList<string>> GetOptedOutDestinationsAsync(
            string connectionString, long userId, CancellationToken ct = default)
        {
            Guard(connectionString, userId);
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);
            using var cmd = new NpgsqlCommand("select opted_out_recv_dst_types from users where user_id = @uid", conn);
            cmd.Parameters.AddWithValue("uid", userId);
            var raw = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
            if (raw == null || raw is DBNull) return Array.Empty<string>();
            return (string[])raw;
        }

        /// <summary>Removes a destination type from the user's opt-out list in a single UPDATE.</summary>
        public static Task AllowDestinationAsync(string connectionString, long userId, string destinationType, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(destinationType))
                throw new ArgumentException("destinationType is required", nameof(destinationType));
            return ExecNoResultAsync(connectionString, userId,
                "update users set opted_out_recv_dst_types = array_remove(opted_out_recv_dst_types, @dt) where user_id = @uid",
                c => c.Parameters.AddWithValue("dt", destinationType), ct);
        }

        /// <summary>Adds a destination type to the user's opt-out list idempotently.</summary>
        public static Task OptOutDestinationAsync(string connectionString, long userId, string destinationType, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(destinationType))
                throw new ArgumentException("destinationType is required", nameof(destinationType));
            return ExecNoResultAsync(connectionString, userId,
                @"update users
                     set opted_out_recv_dst_types = array_append(
                        array_remove(opted_out_recv_dst_types, @dt), @dt)
                   where user_id = @uid",
                c => c.Parameters.AddWithValue("dt", destinationType), ct);
        }

        /// <summary>Reads the full notification band matrix from disk.</summary>
        public static async Task<IReadOnlyList<NotificationBand>> GetNotificationBandsAsync(
            string connectionString, long userId, CancellationToken ct = default)
        {
            Guard(connectionString, userId);
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);
            using var cmd = new NpgsqlCommand("select notification_bands from users where user_id = @uid", conn);
            cmd.Parameters.AddWithValue("uid", userId);
            var raw = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
            if (raw == null || raw is DBNull) return Array.Empty<NotificationBand>();
            return DeserializeBands(raw);
        }

        /// <summary>
        /// Atomically replaces the whole band matrix. Pass an empty list to clear.
        /// </summary>
        public static Task SetNotificationBandsAsync(
            string connectionString, long userId, IReadOnlyList<NotificationBand> bands, CancellationToken ct = default)
        {
            Guard(connectionString, userId);
            var json = bands == null || bands.Count == 0 ? "[]" : JsonSerializer.Serialize(bands);
            return ExecNoResultAsync(connectionString, userId,
                "update users set notification_bands = @j::jsonb where user_id = @uid",
                c => c.Parameters.AddWithValue("j", json), ct);
        }

        public static async Task<bool> GetTwoStepEnabledAsync(string connectionString, long userId, CancellationToken ct = default)
        {
            Guard(connectionString, userId);
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);
            using var cmd = new NpgsqlCommand("select \"2sv_enabled\" from users where user_id = @uid", conn);
            cmd.Parameters.AddWithValue("uid", userId);
            var r = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
            return r != null && r != DBNull.Value && Convert.ToBoolean(r);
        }

        public static Task SetTwoStepEnabledAsync(string connectionString, long userId, bool enabled, CancellationToken ct = default)
            => ExecNoResultAsync(connectionString, userId,
                "update users set \"2sv_enabled\" = @v where user_id = @uid",
                c => c.Parameters.AddWithValue("v", enabled), ct);

        public static async Task<string?> GetMiscValueAsync(string connectionString, long userId, string key, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("key is required", nameof(key));
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);
            using var cmd = new NpgsqlCommand(
                "select account_settings ->> @k from users where user_id = @uid", conn);
            cmd.Parameters.AddWithValue("uid", userId);
            cmd.Parameters.AddWithValue("k", key);
            var r = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
            return r == null || r is DBNull ? null : Convert.ToString(r);
        }

        public static Task SetMiscValueAsync(string connectionString, long userId, string key, string? value, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("key is required", nameof(key));
            return value == null
                ? ExecNoResultAsync(connectionString, userId,
                    "update users set account_settings = account_settings - @k where user_id = @uid",
                    c => c.Parameters.AddWithValue("k", key), ct)
                : ExecNoResultAsync(connectionString, userId,
                    "update users set account_settings = jsonb_set(account_settings, @path, to_jsonb(@v::text)) where user_id = @uid",
                    c =>
                    {
                        c.Parameters.AddWithValue("path", new[] { key });
                        c.Parameters.AddWithValue("v", value);
                    }, ct);
        }

        private static async Task<string> GetPrivacyColumnAsync(
            string connectionString, long userId, string column, string fallback, CancellationToken ct)
        {
            Guard(connectionString, userId);
            if (string.IsNullOrWhiteSpace(column))
                throw new ArgumentException("column is required", nameof(column));

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);
            using var cmd = new NpgsqlCommand($"select {column} from users where user_id = @uid", conn);
            cmd.Parameters.AddWithValue("uid", userId);
            var r = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
            if (r == null || r is DBNull) return fallback;
            return NonNull(Convert.ToString(r), fallback);
        }

        private static Task SetPrivacyColumnAsync(
            string connectionString, long userId, string column, string value, CancellationToken ct)
        {
            Guard(connectionString, userId);
            if (string.IsNullOrWhiteSpace(column))
                throw new ArgumentException("column is required", nameof(column));
            return ExecNoResultAsync(connectionString, userId,
                $"update users set {column} = @v where user_id = @uid",
                c => c.Parameters.AddWithValue("v", value), ct);
        }

        private static async Task ExecNoResultAsync(
            string connectionString, long userId, string sql,
            Action<NpgsqlCommand> bindParams, CancellationToken ct)
        {
            Guard(connectionString, userId);
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("uid", userId);
            bindParams(cmd);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        }

        private static IReadOnlyList<string> ReadStringArray(NpgsqlDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal)) return Array.Empty<string>();
            // Npgsql returns string[] for text[]
            return reader.GetValue(ordinal) as string[] ?? Array.Empty<string>();
        }

        private static IReadOnlyList<NotificationBand> ReadNotificationBands(NpgsqlDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal)) return Array.Empty<NotificationBand>();
            // Npgsql returns a string for jsonb column
            var raw = reader.GetString(ordinal);
            return DeserializeBands(raw);
        }

        private static IReadOnlyList<NotificationBand> DeserializeBands(object raw)
        {
            if (raw is string s) return DeserializeBands(s);
            return DeserializeBands(raw.ToString() ?? "[]");
        }

        private static IReadOnlyList<NotificationBand> DeserializeBands(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return Array.Empty<NotificationBand>();
            try
            {
                return (IReadOnlyList<NotificationBand>)(JsonSerializer.Deserialize<List<NotificationBand>>(raw) ?? new List<NotificationBand>());
            }
            catch
            {
                return Array.Empty<NotificationBand>();
            }
        }
    }
}
