using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Games;

/// <summary>
/// Personalized recommended-games scoring (pure function, no hosted service).
/// Guests get popular-only results. Authed users get per-place related results
/// re-ranked by their interests (favorites, recents, upvotes, friends).
/// </summary>
public static class GameRecommendations
{
    public static async Task<List<GamesQueries.GameEntry>> GetPersonalizedRecommendedGamesAsync(
        long? userId,
        long currentPlaceId,
        int limit,
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));

        if (limit < 1) limit = 1;
        if (limit > 12) limit = 12;
        var authedUserId = userId.HasValue && userId.Value > 0 ? userId.Value : 0;

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        long? currentUniverseId = null;
        long currentCreatorId = 0;
        if (currentPlaceId > 0)
        {
            const string ctxSql = @"SELECT universe_id, creator_user_id FROM universes
                WHERE root_place_id = @place OR @place = ANY(place_ids) LIMIT 1";
            using (var ctxCmd = new NpgsqlCommand(ctxSql, conn))
            {
                ctxCmd.Parameters.AddWithValue("place", currentPlaceId);
                using var r = await ctxCmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                if (await r.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    currentUniverseId = r.GetInt64(0);
                    currentCreatorId = r.GetInt64(1);
                }
            }
        }

        var favoriteUniverseIds = new List<long>();
        var recentUniverseIds = new List<long>();
        var friendUniverseIds = new List<long>();
        var upvotedPlaceIds = new List<long>();
        if (authedUserId > 0)
        {
            try { favoriteUniverseIds = await GamesQueries.GetFavoritePlaceUniverseIdsAsync(authedUserId, connectionString, cancellationToken).ConfigureAwait(false); } catch { }
            try { recentUniverseIds = await GamesQueries.GetRecentlyVisitedUniverseIdsAsync(authedUserId, connectionString, cancellationToken).ConfigureAwait(false); } catch { }
            try { friendUniverseIds = await GamesQueries.GetFriendActivityUniverseIdsAsync(authedUserId, connectionString, 20, cancellationToken).ConfigureAwait(false); } catch { }
            try
            {
                var voting = new VotingService(connectionString);
                var (upvoted, _) = await voting.GetUserVotedAssetsAsync(authedUserId, cancellationToken).ConfigureAwait(false);
                upvotedPlaceIds = upvoted.Take(50).ToList();
            }
            catch { }
        }

        var upvotedUniverseIds = await MapPlaceIdsToUniverseIdsAsync(conn, upvotedPlaceIds, cancellationToken).ConfigureAwait(false);
        var likedUniverseIds = favoriteUniverseIds.Concat(recentUniverseIds).Concat(upvotedUniverseIds).Distinct().Take(40).ToList();
        var likedCreatorIds = await GetCreatorIdsForUniversesAsync(conn, likedUniverseIds, cancellationToken).ConfigureAwait(false);
        var candidateIds = new List<long>();
        candidateIds.AddRange(friendUniverseIds);
        if (currentCreatorId > 0)
            candidateIds.AddRange(await GetUniversesByCreatorsAsync(conn, new List<long> { currentCreatorId }, currentUniverseId, 20, cancellationToken).ConfigureAwait(false));
        if (likedCreatorIds.Count > 0)
            candidateIds.AddRange(await GetUniversesByCreatorsAsync(conn, likedCreatorIds, currentUniverseId, 20, cancellationToken).ConfigureAwait(false));

        List<long> popularIds = new();
        List<long> topRatedIds = new();
        try { popularIds = await GamesQueries.GetPopularUniverseIdsAsync(40, connectionString, cancellationToken).ConfigureAwait(false); } catch { }
        try { topRatedIds = await GamesQueries.GetTopRatedUniverseIdsAsync(40, connectionString, cancellationToken).ConfigureAwait(false); } catch { }
        candidateIds.AddRange(popularIds);
        candidateIds.AddRange(topRatedIds);

        var exclude = new HashSet<long>();
        if (currentUniverseId.HasValue) exclude.Add(currentUniverseId.Value);
        foreach (var id in favoriteUniverseIds) exclude.Add(id);
        foreach (var id in recentUniverseIds) exclude.Add(id);

        var topRatedSet = new HashSet<long>(topRatedIds);
        var friendSet = new HashSet<long>(friendUniverseIds);
        var likedCreatorSet = new HashSet<long>(likedCreatorIds);
        var upvotedSet = new HashSet<long>(upvotedUniverseIds);

        var orderedCandidateIds = candidateIds
            .Where(id => id > 0 && !exclude.Contains(id))
            .Distinct()
            .Take(60)
            .ToList();

        if (orderedCandidateIds.Count == 0)
            return new List<GamesQueries.GameEntry>();

        var entries = await GamesQueries.GetGameEntriesByUniverseIdsAsync(orderedCandidateIds, connectionString, cancellationToken).ConfigureAwait(false);
        var byUniverse = entries.ToDictionary(e => e.UniverseId);

        var scored = new List<(GamesQueries.GameEntry entry, double score)>();
        foreach (var uid in orderedCandidateIds)
        {
            if (!byUniverse.TryGetValue(uid, out var e)) continue;
            double score = 0;
            if (currentCreatorId > 0 && e.CreatorUserId == currentCreatorId) score += 30;
            if (friendSet.Contains(uid)) score += 20;
            if (likedCreatorSet.Contains(e.CreatorUserId)) score += 15;
            if (upvotedSet.Contains(uid)) score -= 5; // already voted, slight demote
            if (topRatedSet.Contains(uid)) score += 5;
            score += Math.Log(e.VisitCount + 1);
            score += Math.Min(e.UpVotes / 1000.0, 5);
            scored.Add((e, score));
        }

        return scored
            .OrderByDescending(s => s.score)
            .ThenByDescending(s => s.entry.VisitCount)
            .Take(limit)
            .Select(s => s.entry)
            .ToList();
    }

    private static async Task<List<long>> MapPlaceIdsToUniverseIdsAsync(NpgsqlConnection conn, List<long> placeIds, CancellationToken ct)
    {
        var result = new List<long>();
        var ids = placeIds.Where(i => i > 0).Distinct().Take(50).ToArray();
        if (ids.Length == 0) return result;
        const string sql = @"SELECT universe_id FROM universes WHERE root_place_id = ANY(@ids) OR place_ids && @ids";
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("ids", ids);
        using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        while (await reader.ReadAsync(ct).ConfigureAwait(false))
            result.Add(reader.GetInt64(0));
        return result;
    }

    private static async Task<List<long>> GetCreatorIdsForUniversesAsync(NpgsqlConnection conn, List<long> universeIds, CancellationToken ct)
    {
        var ids = universeIds.Where(i => i > 0).Distinct().Take(40).ToArray();
        if (ids.Length == 0) return new List<long>();
        const string sql = @"SELECT DISTINCT creator_user_id FROM universes WHERE universe_id = ANY(@ids)";
        var result = new List<long>();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("ids", ids);
        using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        while (await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            var cid = reader.GetInt64(0);
            if (cid > 0) result.Add(cid);
        }
        return result;
    }

    private static async Task<List<long>> GetUniversesByCreatorsAsync(NpgsqlConnection conn, List<long> creatorIds, long? excludeUniverseId, int perCreatorLimit, CancellationToken ct)
    {
        var creators = creatorIds.Where(i => i > 0).Distinct().Take(10).ToArray();
        if (creators.Length == 0) return new List<long>();
        const string sql = @"SELECT u.universe_id FROM universes u
            INNER JOIN assets a ON u.root_place_id = a.asset_id
            WHERE u.creator_user_id = ANY(@creators)
            AND a.is_place = true AND a.access_type = 1
            AND COALESCE(u.privacy_level, 1) = 1
            AND u.root_place_id IS NOT NULL
            AND (@exclude IS NULL OR u.universe_id != @exclude)
            ORDER BY u.visit_count DESC LIMIT @limit";
        var result = new List<long>();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("creators", creators);
        cmd.Parameters.AddWithValue("exclude", excludeUniverseId.HasValue ? (object)excludeUniverseId.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("limit", creators.Length * perCreatorLimit);
        using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        while (await reader.ReadAsync(ct).ConfigureAwait(false))
            result.Add(reader.GetInt64(0));
        return result;
    }
}
