using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Avatar
{
    public sealed class OutfitRecord
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public OutfitBodyColors? BodyColors { get; set; }
        public long[] AssetIds { get; set; } = Array.Empty<long>();
        public string? ThumbnailUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public sealed class OutfitBodyColors
    {
        public int HeadColorId { get; set; }
        public int TorsoColorId { get; set; }
        public int RightArmColorId { get; set; }
        public int LeftArmColorId { get; set; }
        public int RightLegColorId { get; set; }
        public int LeftLegColorId { get; set; }
    }

    public sealed class OutfitsRepository
    {
        public const int MaxOutfitsPerUser = 5;

        public async Task<(List<OutfitRecord> Items, int Total)> GetOutfitsAsync(
            string connectionString, long userId, int itemsPerPage, int page, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId));
            if (itemsPerPage <= 0) itemsPerPage = 50;
            if (page <= 0) page = 1;

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string countSql = @"select count(*) from outfits where user_id = @uid";
            long total;
            using (var countCmd = new NpgsqlCommand(countSql, conn))
            {
                countCmd.Parameters.AddWithValue("uid", userId);
                var result = await countCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                total = result is long l ? l : 0;
            }

            var offset = (page - 1) * itemsPerPage;
            const string sql = @"select outfit_id, user_id, name, body_colors, asset_ids, thumbnail_url, created_at, updated_at
from outfits
where user_id = @uid
order by created_at desc
limit @limit offset @offset";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("uid", userId);
            cmd.Parameters.AddWithValue("limit", itemsPerPage);
            cmd.Parameters.AddWithValue("offset", offset);

            var items = new List<OutfitRecord>();
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                items.Add(ReadOutfit(reader));
            }

            return (items, (int)total);
        }

        public async Task<OutfitRecord?> GetOutfitDetailsAsync(
            string connectionString, long outfitId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (outfitId <= 0)
                throw new ArgumentOutOfRangeException(nameof(outfitId));

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = @"select outfit_id, user_id, name, body_colors, asset_ids, thumbnail_url, created_at, updated_at
from outfits
where outfit_id = @oid";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("oid", outfitId);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                return ReadOutfit(reader);
            }

            return null;
        }

        public async Task<int> GetOutfitCountAsync(
            string connectionString, long userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId));

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = @"select count(*) from outfits where user_id = @uid";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("uid", userId);
            var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            return result is long l ? (int)l : 0;
        }

        public async Task<OutfitRecord> CreateOutfitAsync(
            string connectionString, long userId, string name, OutfitBodyColors bodyColors, long[] assetIds, string? thumbnailUrl = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name is required", nameof(name));
            if (bodyColors == null)
                throw new ArgumentNullException(nameof(bodyColors));

            var colorsJson = JsonSerializer.Serialize(new
            {
                headColorId = bodyColors.HeadColorId,
                torsoColorId = bodyColors.TorsoColorId,
                rightArmColorId = bodyColors.RightArmColorId,
                leftArmColorId = bodyColors.LeftArmColorId,
                rightLegColorId = bodyColors.RightLegColorId,
                leftLegColorId = bodyColors.LeftLegColorId
            });

            var assetIdsJson = JsonSerializer.Serialize(assetIds ?? Array.Empty<long>());

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var tx = conn.BeginTransaction();

            const string insertSql = @"insert into outfits(user_id, name, body_colors, asset_ids, thumbnail_url)
values(@uid, @name, @colors::jsonb, @assetIds::jsonb, @thumb)
returning outfit_id, created_at, updated_at";

            long outfitId;
            DateTime createdAt, updatedAt;
            using (var insertCmd = new NpgsqlCommand(insertSql, conn, tx))
            {
                insertCmd.Parameters.AddWithValue("uid", userId);
                insertCmd.Parameters.AddWithValue("name", name);
                insertCmd.Parameters.AddWithValue("colors", colorsJson);
                insertCmd.Parameters.AddWithValue("assetIds", assetIdsJson);
                insertCmd.Parameters.AddWithValue("thumb", (object?)thumbnailUrl ?? DBNull.Value);

                await using var reader = await insertCmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                    throw new InvalidOperationException("Failed to create outfit");
                outfitId = reader.GetInt64(0);
                createdAt = reader.GetDateTime(1);
                updatedAt = reader.GetDateTime(2);
                await reader.CloseAsync().ConfigureAwait(false);
            }

            const string userSql = @"update users
set owned_outfits = coalesce(owned_outfits, '[]'::jsonb) || to_jsonb(@oid::bigint)
where user_id = @uid";

            using (var userCmd = new NpgsqlCommand(userSql, conn, tx))
            {
                userCmd.Parameters.AddWithValue("uid", userId);
                userCmd.Parameters.AddWithValue("oid", outfitId);
                await userCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            tx.Commit();

            return new OutfitRecord
            {
                Id = outfitId,
                UserId = userId,
                Name = name,
                BodyColors = bodyColors,
                AssetIds = assetIds ?? Array.Empty<long>(),
                ThumbnailUrl = thumbnailUrl,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt
            };
        }

        public async Task<bool> DeleteOutfitAsync(
            string connectionString, long outfitId, long userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (outfitId <= 0)
                throw new ArgumentOutOfRangeException(nameof(outfitId));
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId));

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var tx = conn.BeginTransaction();

            const string deleteSql = @"delete from outfits where outfit_id = @oid and user_id = @uid";
            using (var deleteCmd = new NpgsqlCommand(deleteSql, conn, tx))
            {
                deleteCmd.Parameters.AddWithValue("oid", outfitId);
                deleteCmd.Parameters.AddWithValue("uid", userId);
                var rows = await deleteCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                if (rows == 0)
                {
                    tx.Rollback();
                    return false;
                }
            }

            const string userSql = @"update users
set owned_outfits = coalesce(
    (
        select jsonb_agg(value::bigint)
        from jsonb_array_elements(owned_outfits) as value
        where value::text <> @oid::text
    ),
    '[]'::jsonb
)
where user_id = @uid";

            using (var userCmd = new NpgsqlCommand(userSql, conn, tx))
            {
                userCmd.Parameters.AddWithValue("uid", userId);
                userCmd.Parameters.AddWithValue("oid", outfitId);
                await userCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            tx.Commit();
            return true;
        }

        public async Task<bool> PatchOutfitAsync(
            string connectionString, long outfitId, long userId, string? name, OutfitBodyColors? bodyColors, long[]? assetIds, string? thumbnailUrl = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (outfitId <= 0)
                throw new ArgumentOutOfRangeException(nameof(outfitId));
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId));

            var sets = new List<string>();
            var parameters = new Dictionary<string, object>();

            if (name != null)
            {
                sets.Add("name = @name");
                parameters["name"] = name;
            }

            if (bodyColors != null)
            {
                sets.Add("body_colors = @colors::jsonb");
                parameters["colors"] = JsonSerializer.Serialize(new
                {
                    headColorId = bodyColors.HeadColorId,
                    torsoColorId = bodyColors.TorsoColorId,
                    rightArmColorId = bodyColors.RightArmColorId,
                    leftArmColorId = bodyColors.LeftArmColorId,
                    rightLegColorId = bodyColors.RightLegColorId,
                    leftLegColorId = bodyColors.LeftLegColorId
                });
            }

            if (assetIds != null)
            {
                sets.Add("asset_ids = @assetIds::jsonb");
                parameters["assetIds"] = JsonSerializer.Serialize(assetIds);
            }

            if (thumbnailUrl != null)
            {
                sets.Add("thumbnail_url = @thumb");
                parameters["thumb"] = thumbnailUrl;
            }

            if (sets.Count == 0) return true;

            sets.Add("updated_at = now()");
            parameters["oid"] = outfitId;
            parameters["uid"] = userId;

            var sql = $"update outfits set {string.Join(", ", sets)} where outfit_id = @oid and user_id = @uid";

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("oid", outfitId);
            cmd.Parameters.AddWithValue("uid", userId);
            foreach (var kv in parameters)
            {
                if (kv.Key != "oid" && kv.Key != "uid")
                    cmd.Parameters.AddWithValue(kv.Key, kv.Value);
            }

            var rows = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            return rows > 0;
        }

        private static OutfitRecord ReadOutfit(NpgsqlDataReader reader)
        {
            var id = reader.GetInt64(0);
            var userId = reader.GetInt64(1);
            var name = reader.GetString(2);

            OutfitBodyColors? bodyColors = null;
            if (!reader.IsDBNull(3))
            {
                var json = reader.GetString(3);
                bodyColors = JsonSerializer.Deserialize<OutfitBodyColors>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            long[] assetIds = Array.Empty<long>();
            if (!reader.IsDBNull(4))
            {
                var json = reader.GetString(4);
                var raw = JsonSerializer.Deserialize<long[]>(json);
                if (raw != null) assetIds = raw;
            }

            var thumbnailUrl = reader.IsDBNull(5) ? null : reader.GetString(5);
            var createdAt = reader.GetDateTime(6);
            var updatedAt = reader.GetDateTime(7);

            return new OutfitRecord
            {
                Id = id,
                UserId = userId,
                Name = name,
                BodyColors = bodyColors,
                AssetIds = assetIds,
                ThumbnailUrl = thumbnailUrl,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt
            };
        }
    }
}
