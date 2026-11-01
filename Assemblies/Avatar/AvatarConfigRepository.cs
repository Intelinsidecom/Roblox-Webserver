using System;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Avatar
{
    /// <summary>
    /// Reads and writes the per-user avatar_config JSONB payload stored on the users table.
    /// The payload holds avatar scales and the R6/R15 body type, for example:
    /// {"height":1.0,"width":1.0,"head":1.0,"depth":1.0,"proportion":0.0,"bodyType":0.0,"playerAvatarType":"R15"}
    /// </summary>
    public sealed class AvatarConfigRepository
    {
        public const double DefaultHeight = 1.0;
        public const double DefaultWidth = 1.0;
        public const double DefaultHead = 1.0;
        public const double DefaultDepth = 1.0;
        public const double DefaultProportion = 0.0;
        public const double DefaultBodyType = 0.0;
        public const string DefaultPlayerAvatarType = "R15";

        public sealed class AvatarConfig
        {
            public double Height { get; set; } = DefaultHeight;
            public double Width { get; set; } = DefaultWidth;
            public double Head { get; set; } = DefaultHead;
            public double Depth { get; set; } = DefaultDepth;
            public double Proportion { get; set; } = DefaultProportion;
            public double BodyType { get; set; } = DefaultBodyType;
            public string PlayerAvatarType { get; set; } = DefaultPlayerAvatarType;
        }

        /// <summary>
        /// A partial update: null fields are left untouched in the stored JSON.
        /// </summary>
        public sealed class AvatarConfigPatch
        {
            public double? Height { get; set; }
            public double? Width { get; set; }
            public double? Head { get; set; }
            public double? Depth { get; set; }
            public double? Proportion { get; set; }
            public double? BodyType { get; set; }
            public string? PlayerAvatarType { get; set; }
        }

        public async Task<AvatarConfig> GetAvatarConfigAsync(string connectionString, long userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId));

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            // If the avatar_config column hasn't been added yet, return defaults immediately
            // so the app stays functional during the transition between deployment and migration.
            const string checkColumnSql = @"select count(*) from information_schema.columns
where table_schema = 'public' and table_name = 'users' and column_name = 'avatar_config'";
            using var checkCmd = new NpgsqlCommand(checkColumnSql, conn);
            var columnExists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false)) > 0;
            if (!columnExists)
                return new AvatarConfig();

            const string sql = @"select avatar_config::text from users where user_id = @uid";

            string? json = null;
            using (var cmd = new NpgsqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("uid", userId);
                var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                if (result != null && result != DBNull.Value)
                    json = Convert.ToString(result);
            }

            // Defaults first so a missing row, a NULL payload or a payload written before a
            // key existed all resolve to a complete config.
            var config = new AvatarConfig();
            if (string.IsNullOrWhiteSpace(json))
                return config;

            var payload = json!;
            try
            {
                using var doc = JsonDocument.Parse(payload);
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                    return config;

                config.Height = ReadDouble(doc, "height", config.Height);
                config.Width = ReadDouble(doc, "width", config.Width);
                config.Head = ReadDouble(doc, "head", config.Head);
                config.Depth = ReadDouble(doc, "depth", config.Depth);
                config.Proportion = ReadDouble(doc, "proportion", config.Proportion);
                config.BodyType = ReadDouble(doc, "bodyType", config.BodyType);

                var avatarType = ReadString(doc, "playerAvatarType");
                if (!string.IsNullOrWhiteSpace(avatarType))
                    config.PlayerAvatarType = NormalizeAvatarType(avatarType!) ?? config.PlayerAvatarType;
            }
            catch (JsonException)
            {
                // Corrupt payload: fall back to defaults rather than failing the request
                return new AvatarConfig();
            }

            return config;
        }

        /// <summary>
        /// Merges the non-null fields of the patch into the stored JSON
        /// ({"height":0.95} only changes the height key).
        /// </summary>
        public async Task UpdateAvatarConfigAsync(string connectionString, long userId, AvatarConfigPatch patch, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId));
            if (patch == null)
                throw new ArgumentNullException(nameof(patch));

            var patchJson = BuildPatchJson(patch);
            if (string.IsNullOrWhiteSpace(patchJson))
                return;

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            // Skip write if the avatar_config column hasn't been added yet
            const string checkColumnSql = @"select count(*) from information_schema.columns
where table_schema = 'public' and table_name = 'users' and column_name = 'avatar_config'";
            using var checkCmd = new NpgsqlCommand(checkColumnSql, conn);
            var columnExists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false)) > 0;
            if (!columnExists)
                return;

            const string sql = @"update users
set avatar_config = coalesce(avatar_config, '{}'::jsonb) || @patch::jsonb
where user_id = @uid";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("patch", patchJson);
            cmd.Parameters.AddWithValue("uid", userId);
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        private static string? BuildPatchJson(AvatarConfigPatch patch)
        {
            var entries = new StringBuilder();

            AppendNumber(entries, "height", patch.Height);
            AppendNumber(entries, "width", patch.Width);
            AppendNumber(entries, "head", patch.Head);
            AppendNumber(entries, "depth", patch.Depth);
            AppendNumber(entries, "proportion", patch.Proportion);
            AppendNumber(entries, "bodyType", patch.BodyType);

            if (!string.IsNullOrWhiteSpace(patch.PlayerAvatarType))
            {
                var normalized = NormalizeAvatarType(patch.PlayerAvatarType!);
                if (normalized == null)
                    throw new ArgumentException($"'{patch.PlayerAvatarType}' is not a valid avatar type", nameof(patch));

                Append(entries, "\"playerAvatarType\":\"" + normalized + "\"");
            }

            if (entries.Length == 0)
                return null;

            return "{" + entries + "}";
        }

        private static void AppendNumber(StringBuilder sb, string key, double? value)
        {
            if (!value.HasValue)
                return;

            // Round-trip format so 1.0 stays distinguishable from 1 and never loses precision
            Append(sb, "\"" + key + "\":" + value.Value.ToString("R", CultureInfo.InvariantCulture));
        }

        private static void Append(StringBuilder sb, string entry)
        {
            if (sb.Length > 0)
                sb.Append(',');
            sb.Append(entry);
        }

        /// <summary>
        /// Maps any casing of the two supported body types, or null when unsupported.
        /// </summary>
        public static string? NormalizeAvatarType(string value)
        {
            var trimmed = value?.Trim();
            if (string.Equals(trimmed, "R6", StringComparison.OrdinalIgnoreCase))
                return "R6";
            if (string.Equals(trimmed, "R15", StringComparison.OrdinalIgnoreCase))
                return "R15";
            return null;
        }

        private static double ReadDouble(JsonDocument doc, string key, double fallback)
        {
            if (!doc.RootElement.TryGetProperty(key, out var element))
                return fallback;

            switch (element.ValueKind)
            {
                case JsonValueKind.Number:
                    return element.TryGetDouble(out var number) ? number : fallback;
                case JsonValueKind.String:
                    return double.TryParse(element.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                        ? parsed
                        : fallback;
                default:
                    return fallback;
            }
        }

        private static string? ReadString(JsonDocument doc, string key)
        {
            if (!doc.RootElement.TryGetProperty(key, out var element) || element.ValueKind != JsonValueKind.String)
                return null;

            return element.GetString();
        }
    }
}
