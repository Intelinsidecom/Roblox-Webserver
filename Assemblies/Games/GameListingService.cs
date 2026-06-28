using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Games;

public sealed class UniverseListEntry
{
    public long UniverseId { get; set; }
    public string UniverseName { get; set; } = string.Empty;
    public long RootPlaceId { get; set; }
    public string PlaceName { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public short PrivacyLevel { get; set; }
}

public sealed class PlaceListEntry
{
    public long PlaceId { get; set; }
    public string PlaceName { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public long? UniverseId { get; set; }
    public int VisitCount { get; set; }
}

public static class GameListingService
{
    /// <summary>
    /// Returns all universes owned by the specified user, along with their root
    /// place metadata, ordered by creation time (newest first).
    /// </summary>
    public static async Task<IReadOnlyList<UniverseListEntry>> GetUniversesForUserAsync(
        string? connectionString,
        long userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new System.ArgumentException("Connection string is required", nameof(connectionString));

        if (userId <= 0)
            throw new System.ArgumentOutOfRangeException(nameof(userId));

        var results = new List<UniverseListEntry>();

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"select u.universe_id,
       u.name as universe_name,
       u.root_place_id,
       a.name as place_name,
       COALESCE(a.thumbnail_url, '/images/blocked.png') as thumbnail_url,
       COALESCE(u.privacy_level, 3) as privacy_level
from universes u
left join assets a on a.asset_id = u.root_place_id
where u.creator_user_id = @uid
order by u.created_at desc, u.universe_id desc;";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("uid", userId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var universeId = reader.GetInt64(0);
            var universeName = reader.IsDBNull(1) ? "Unnamed Game" : reader.GetString(1);
            var rootPlaceId = reader.IsDBNull(2) ? 0L : reader.GetInt64(2);
            var placeName = rootPlaceId > 0 && !reader.IsDBNull(3) ? reader.GetString(3) : "";
            var thumbUrl = reader.IsDBNull(4) ? null : reader.GetString(4);
            var privacyLevel = reader.IsDBNull(5) ? (short)1 : reader.GetInt16(5);

            results.Add(new UniverseListEntry
            {
                UniverseId = universeId,
                UniverseName = universeName,
                RootPlaceId = rootPlaceId,
                PlaceName = placeName,
                ThumbnailUrl = thumbUrl,
                PrivacyLevel = privacyLevel,
            });
        }

        return results;
    }

    /// <summary>
    /// Returns all places owned by the specified user, ordered by creation time (newest first).
    /// </summary>
    public static async Task<IReadOnlyList<PlaceListEntry>> GetPlacesForUserAsync(
        string? connectionString,
        long userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new System.ArgumentException("Connection string is required", nameof(connectionString));

        if (userId <= 0)
            throw new System.ArgumentOutOfRangeException(nameof(userId));

        var results = new List<PlaceListEntry>();

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"select a.asset_id as place_id,
       a.name as place_name,
       a.thumbnail_url,
       u.universe_id,
       COALESCE(a.visit_count, 0) as visit_count
from assets a
left join universes u on u.root_place_id = a.asset_id
where a.asset_type_id = 9
  and a.owner_user_id = @uid
order by a.created_at desc, a.asset_id desc;";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("uid", userId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var placeId = reader.GetInt64(0);
            var placeName = reader.IsDBNull(1) ? "Unnamed Place" : reader.GetString(1);
            var thumbUrl = reader.IsDBNull(2) ? null : reader.GetString(2);
            var universeId = reader.IsDBNull(3) ? (long?)null : reader.GetInt64(3);
            var visitCount = reader.IsDBNull(4) ? 0 : reader.GetInt32(4);

            results.Add(new PlaceListEntry
            {
                PlaceId = placeId,
                PlaceName = placeName,
                ThumbnailUrl = thumbUrl,
                UniverseId = universeId,
                VisitCount = visitCount,
            });
        }

        return results;
    }
}
