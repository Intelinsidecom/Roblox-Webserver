using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Assets;
using Games;
using Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Games;

/// <summary>
/// Database query helpers for games and universe operations
/// </summary>
public static class GamesQueries
{
    /// <summary>
    /// Gets the next place number for a user (for auto-generating place names)
    /// </summary>
    public static async Task<int> GetNextPlaceNumberAsync(long userId, string connectionString, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));
        if (userId <= 0)
            throw new ArgumentOutOfRangeException(nameof(userId));

        const string maxPlaceSeqSql = @"select coalesce(count(*), 0) from assets where owner_user_id = @uid and is_place = true;";

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        using var seqCmd = new NpgsqlCommand(maxPlaceSeqSql, conn);
        seqCmd.Parameters.AddWithValue("uid", userId);
        
        var obj = await seqCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        var existingCount = Convert.ToInt32(obj ?? 0);
        
        return existingCount + 1;
    }

    /// <summary>
    /// Updates the content hash for a place asset
    /// </summary>
    public static async Task UpdatePlaceAssetContentHashAsync(long placeId, string contentHash, string connectionString, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));
        if (placeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(placeId));
        if (string.IsNullOrWhiteSpace(contentHash))
            throw new ArgumentException("Content hash is required", nameof(contentHash));

        string? currentHash;
        using (var selectConn = new NpgsqlConnection(connectionString))
        {
            await selectConn.OpenAsync(cancellationToken).ConfigureAwait(false);
            const string selectSql = "SELECT content_hash FROM assets WHERE asset_id = @placeId";
            using var selectCmd = new NpgsqlCommand(selectSql, selectConn);
            selectCmd.Parameters.AddWithValue("placeId", placeId);
            var selectResult = await selectCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            currentHash = selectResult == null || selectResult == DBNull.Value ? null : Convert.ToString(selectResult);
        }

        if (!string.IsNullOrWhiteSpace(currentHash))
        {
            try
            {
                var repo = new AssetsRepository();
                await repo.AppendVersionHistoryAsync(connectionString, placeId, currentHash, cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                // Version history failure should not block the update
            }
        }

        const string updateSql = @"UPDATE assets SET content_hash = @contentHash WHERE asset_id = @placeId";

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        using var cmd = new NpgsqlCommand(updateSql, conn);
        cmd.Parameters.AddWithValue("contentHash", contentHash);
        cmd.Parameters.AddWithValue("placeId", placeId);
        
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the first place ID from a universe (the root place)
    /// Note: This is different from GamesRepository.GetUniverseIdFromPlaceIdAsync which gets universe ID from place ID
    /// </summary>
    public static async Task<long> GetFirstPlaceIdFromUniverseAsync(long universeId, string connectionString, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));
        if (universeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(universeId));

        using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync().ConfigureAwait(false);

        const string sql = @"SELECT root_place_id 
                           FROM universes 
                           WHERE universe_id = @universeId AND root_place_id IS NOT NULL";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("universeId", universeId);

        var result = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
        
        if (result == null || result == DBNull.Value)
        {
            throw new InvalidOperationException($"Universe {universeId} does not have a root place set");
        }

        return Convert.ToInt64(result);
    }

    /// <summary>
    /// Represents a game entry for the games page, containing universe and place information
    /// </summary>
    public class GameEntry
    {
        public long UniverseId { get; set; }
        public long PlaceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CreatorName { get; set; } = string.Empty;
        public long CreatorUserId { get; set; }
        public string IconUrl { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public int Playing { get; set; }
        public int VisitCount { get; set; }
        public int UpVotes { get; set; }
        public int DownVotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalVotes => UpVotes + DownVotes;
        public double VotePercentage => TotalVotes > 0 ? (double)UpVotes / TotalVotes * 100 : 0;
    }

    /// <summary>
    /// Gets public universes with their game information for the games page
    /// Filters for public universes only and includes game icon, name, and creator info
    /// </summary>
    public static async Task<List<GameEntry>> GetPublicGamesAsync(
        int sortFilter = 1,
        int timeFilter = 0,
        int genreFilter = 1,
        int regionFilter = 183,
        int startRow = 0,
        int maxRows = 14,
        string connectionString = "",
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        var sql = @"
            SELECT 
                u.universe_id,
                u.root_place_id,
                u.name,
                u.creator_user_id,
                creator.user_name as creator_name,
                COALESCE(a.thumbnail_url, '/images/blocked.png') as icon_url,
                COALESCE(a.thumbnail_url, '/images/blocked.png') as thumbnail_url,
                u.created_at,
                COALESCE(a.upvotes, 0) as up_votes,
                COALESCE(a.downvotes, 0) as down_votes,
                COALESCE(u.visit_count, 0) as visit_count
            FROM universes u
            INNER JOIN users creator ON u.creator_user_id = creator.user_id
            INNER JOIN assets a ON u.root_place_id = a.asset_id
            WHERE a.is_place = true
            AND a.access_type = 1 -- Public access only
            AND u.root_place_id IS NOT NULL";

        if (genreFilter > 1)
        {
            sql += " AND a.genre = @genreFilter";
        }

        var orderBy = sortFilter switch
        {
            1 => "ORDER BY u.created_at DESC",
            2 => "ORDER BY u.created_at DESC",
            3 => "ORDER BY LOWER(u.name) ASC",
            8 => "ORDER BY u.created_at DESC",
            9 => "ORDER BY LOWER(u.name) DESC",
            11 => "ORDER BY u.visit_count DESC",
            16 => "ORDER BY u.created_at ASC",
            _ => "ORDER BY u.created_at DESC"
        };

        if (timeFilter > 0)
        {
            var timeCondition = timeFilter switch
            {
                1 => "AND u.created_at >= NOW() - INTERVAL '24 hours'", // Daily
                2 => "AND u.created_at >= NOW() - INTERVAL '7 days'", // Weekly
                4 => "", // All time (no additional filter)
                _ => "" // Default to no time filter
            };
            sql += $" {timeCondition}";
        }

        sql += $" {orderBy} LIMIT @maxRows OFFSET @startRow";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("maxRows", maxRows);
        cmd.Parameters.AddWithValue("startRow", startRow);
        
        if (genreFilter > 1)
        {
            cmd.Parameters.AddWithValue("genreFilter", genreFilter);
        }

        using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var games = new List<GameEntry>();

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var game = new GameEntry
            {
                UniverseId = reader.GetInt64(reader.GetOrdinal("universe_id")),
                PlaceId = reader.GetInt64(reader.GetOrdinal("root_place_id")),
                Name = reader.GetString(reader.GetOrdinal("name")),
                CreatorName = reader.GetString(reader.GetOrdinal("creator_name")),
                CreatorUserId = reader.GetInt64(reader.GetOrdinal("creator_user_id")),
                IconUrl = reader.GetString(reader.GetOrdinal("icon_url")),
                ThumbnailUrl = reader.GetString(reader.GetOrdinal("thumbnail_url")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                UpVotes = reader.GetInt32(reader.GetOrdinal("up_votes")),
                DownVotes = reader.GetInt32(reader.GetOrdinal("down_votes")),
                VisitCount = reader.GetInt32(reader.GetOrdinal("visit_count")),
                Playing = 0
            };
            games.Add(game);
        }

        return games;
    }

    /// <summary>
    /// Gets public universes for a specific user (their created games)
    /// </summary>
    public static async Task<List<GameEntry>> GetUserPublicGamesAsync(
        long userId,
        int sortFilter = 1,
        int startRow = 0,
        int maxRows = 14,
        int genreFilter = 1,
        string connectionString = "",
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));
        if (userId <= 0)
            throw new ArgumentOutOfRangeException(nameof(userId));

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        var sql = @"
            SELECT 
                u.universe_id,
                u.root_place_id,
                u.name,
                u.creator_user_id,
                creator.user_name as creator_name,
                COALESCE(a.place_custom_icon_url, a.place_generated_icon_url, a.thumbnail_url, '/images/blocked.png') as icon_url,
                COALESCE(a.place_custom_icon_url, a.place_generated_icon_url, a.thumbnail_url, '/images/blocked.png') as thumbnail_url,
                u.created_at,
                COALESCE(a.upvotes, 0) as up_votes,
                COALESCE(a.downvotes, 0) as down_votes,
                COALESCE(u.visit_count, 0) as visit_count
            FROM universes u
            INNER JOIN users creator ON u.creator_user_id = creator.user_id
            INNER JOIN assets a ON u.root_place_id = a.asset_id
            WHERE u.creator_user_id = @userId
            AND a.is_place = true
            AND a.access_type = 1 -- Public access only
            AND u.root_place_id IS NOT NULL";

        if (genreFilter > 1)
        {
            sql += " AND a.genre = @genreFilter";
        }

        sql += " ORDER BY u.created_at DESC LIMIT @maxRows OFFSET @startRow";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("userId", userId);
        cmd.Parameters.AddWithValue("maxRows", maxRows);
        cmd.Parameters.AddWithValue("startRow", startRow);
        

        if (genreFilter > 1)
        {
            cmd.Parameters.AddWithValue("genreFilter", genreFilter);
        }

        using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var games = new List<GameEntry>();

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var game = new GameEntry
            {
                UniverseId = reader.GetInt64(reader.GetOrdinal("universe_id")),
                PlaceId = reader.GetInt64(reader.GetOrdinal("root_place_id")),
                Name = reader.GetString(reader.GetOrdinal("name")),
                CreatorName = reader.GetString(reader.GetOrdinal("creator_name")),
                CreatorUserId = reader.GetInt64(reader.GetOrdinal("creator_user_id")),
                IconUrl = reader.GetString(reader.GetOrdinal("icon_url")),
                ThumbnailUrl = reader.GetString(reader.GetOrdinal("thumbnail_url")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                UpVotes = reader.GetInt32(reader.GetOrdinal("up_votes")),
                DownVotes = reader.GetInt32(reader.GetOrdinal("down_votes")),
                VisitCount = reader.GetInt32(reader.GetOrdinal("visit_count")),
                Playing = 0
            };
            games.Add(game);
        }

        return games;
    }

    /// <summary>
    /// Searches public universes by name with filtering options
    /// </summary>
    public static async Task<List<GameEntry>> SearchPublicGamesAsync(
        string keyword,
        int startRow = 0,
        int maxRows = 40,
        string connectionString = "",
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));
        if (string.IsNullOrWhiteSpace(keyword))
            return new List<GameEntry>();

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        string searchCondition;
        if (keyword.Length < 4)
        {
            searchCondition = @"
                (LOWER(u.name) LIKE LOWER(@keyword) OR 
                 LOWER(creator.user_name) LIKE LOWER(@keyword))";
        }
        else
        {
            searchCondition = @"
                (to_tsvector('english', u.name) @@ plainto_tsquery('english', @keyword) OR
                 to_tsvector('english', creator.user_name) @@ plainto_tsquery('english', @keyword))";
        }

        var sql = $@"
            SELECT 
                u.universe_id,
                u.root_place_id,
                u.name,
                u.creator_user_id,
                creator.user_name as creator_name,
                COALESCE(a.place_custom_icon_url, a.place_generated_icon_url, a.thumbnail_url, '/images/blocked.png') as icon_url,
                COALESCE(a.place_custom_icon_url, a.place_generated_icon_url, a.thumbnail_url, '/images/blocked.png') as thumbnail_url,
                u.created_at,
                COALESCE(a.upvotes, 0) as up_votes,
                COALESCE(a.downvotes, 0) as down_votes,
                COALESCE(u.visit_count, 0) as visit_count
            FROM universes u
            INNER JOIN users creator ON u.creator_user_id = creator.user_id
            INNER JOIN assets a ON u.root_place_id = a.asset_id
            WHERE a.is_place = true
            AND a.access_type = 1 -- Public access only
            AND u.root_place_id IS NOT NULL
            AND ({searchCondition})
            ORDER BY u.created_at DESC
            LIMIT @maxRows OFFSET @startRow";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("keyword", keyword.Length < 4 ? "%" + keyword + "%" : keyword);
        cmd.Parameters.AddWithValue("maxRows", maxRows);
        cmd.Parameters.AddWithValue("startRow", startRow);

        using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var games = new List<GameEntry>();

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var game = new GameEntry
            {
                UniverseId = reader.GetInt64(reader.GetOrdinal("universe_id")),
                PlaceId = reader.GetInt64(reader.GetOrdinal("root_place_id")),
                Name = reader.GetString(reader.GetOrdinal("name")),
                CreatorName = reader.GetString(reader.GetOrdinal("creator_name")),
                CreatorUserId = reader.GetInt64(reader.GetOrdinal("creator_user_id")),
                IconUrl = reader.GetString(reader.GetOrdinal("icon_url")),
                ThumbnailUrl = reader.GetString(reader.GetOrdinal("thumbnail_url")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                UpVotes = reader.GetInt32(reader.GetOrdinal("up_votes")),
                DownVotes = reader.GetInt32(reader.GetOrdinal("down_votes")),
                VisitCount = reader.GetInt32(reader.GetOrdinal("visit_count")),
                Playing = 0
            };
            games.Add(game);
        }

        return games;
    }

    /// <summary>
    /// Gets the total count of public universes for pagination
    /// </summary>
    public static async Task<int> GetPublicGamesCountAsync(
        int sortFilter = 1,
        int timeFilter = 0,
        string connectionString = "",
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        var sql = @"
            SELECT COUNT(*)
            FROM universes u
            INNER JOIN assets a ON u.root_place_id = a.asset_id
            WHERE a.is_place = true 
            AND a.access_type = 1 -- Public access only
            AND u.root_place_id IS NOT NULL";

        if (timeFilter > 0)
        {
            var timeCondition = timeFilter switch
            {
                1 => "AND u.created_at >= NOW() - INTERVAL '24 hours'", // Daily
                2 => "AND u.created_at >= NOW() - INTERVAL '7 days'", // Weekly
                4 => "", // All time (no additional filter)
                _ => "" // Default to no time filter
            };
            sql += $" {timeCondition}";
        }

        using var cmd = new NpgsqlCommand(sql, conn);
        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        
        return Convert.ToInt32(result ?? 0);
    }

    /// <summary>
    /// Gets recommended games (popular public games) for universe page
    /// </summary>
    public static async Task<List<GameEntry>> GetRecommendedGamesAsync(
        int limit = 7,
        string connectionString = "",
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        var sql = @"
            SELECT 
                u.universe_id,
                u.root_place_id,
                u.name,
                u.creator_user_id,
                creator.user_name as creator_name,
                COALESCE(a.thumbnail_url, '/images/blocked.png') as icon_url,
                COALESCE(a.thumbnail_url, '/images/blocked.png') as thumbnail_url,
                u.created_at,
                COALESCE(a.upvotes, 0) as up_votes,
                COALESCE(a.downvotes, 0) as down_votes,
                COALESCE(u.visit_count, 0) as visit_count
            FROM universes u
            INNER JOIN users creator ON u.creator_user_id = creator.user_id
            INNER JOIN assets a ON u.root_place_id = a.asset_id
            WHERE a.is_place = true 
            AND a.access_type = 1 -- Public access only
            AND u.root_place_id IS NOT NULL
            ORDER BY u.created_at DESC
            LIMIT @limit";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@limit", limit);
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        
        var games = new List<GameEntry>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var game = new GameEntry
            {
                UniverseId = reader.GetInt64(reader.GetOrdinal("universe_id")),
                PlaceId = reader.GetInt64(reader.GetOrdinal("root_place_id")),
                Name = reader.GetString(reader.GetOrdinal("name")),
                CreatorName = reader.GetString(reader.GetOrdinal("creator_name")),
                CreatorUserId = reader.GetInt64(reader.GetOrdinal("creator_user_id")),
                IconUrl = reader.GetString(reader.GetOrdinal("icon_url")),
                ThumbnailUrl = reader.GetString(reader.GetOrdinal("thumbnail_url")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                UpVotes = reader.GetInt32(reader.GetOrdinal("up_votes")),
                DownVotes = reader.GetInt32(reader.GetOrdinal("down_votes")),
                VisitCount = reader.GetInt32(reader.GetOrdinal("visit_count")),
                Playing = 0
            };
            games.Add(game);
        }

        return games;
    }

    /// <summary>
    /// Advanced search for games with keyword matching in name and description
    /// Supports exact phrase search (with quotes) and multi-word searches
    /// Results are filtered by last updated date and limited by specified count
    /// </summary>
    public static async Task<List<GameEntry>> SearchGamesAdvancedAsync(
        string keyword,
        int startRow = 0,
        int maxRows = 40,
        string connectionString = "",
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));
        if (string.IsNullOrWhiteSpace(keyword))
            return new List<GameEntry>();

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        var searchConditions = new List<string>();
        var searchParams = new List<NpgsqlParameter>();
        var paramIndex = 0;

        if (keyword.StartsWith("\"") && keyword.EndsWith("\"") && keyword.Length > 2)
        {
            var exactPhrase = keyword.Substring(1, keyword.Length - 2);
            searchConditions.Add(@"
                (LOWER(u.name) LIKE LOWER(@search" + paramIndex + @") OR 
                 COALESCE(LOWER(a.description), '') LIKE LOWER(@search" + paramIndex + @"))");
            searchParams.Add(new NpgsqlParameter("@search" + paramIndex, "%" + exactPhrase + "%"));
            paramIndex++;
        }
        else
        {
            var words = keyword.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            
            if (words.Length == 1)
            {
                var word = words[0];
                if (word.Length < 4)
                {
                    // Use simple LIKE for very short keywords that full-text search might ignore
                    searchConditions.Add(@"
                        (LOWER(u.name) LIKE LOWER(@search" + paramIndex + @") OR 
                         COALESCE(LOWER(a.description), '') LIKE LOWER(@search" + paramIndex + @"))");
                    searchParams.Add(new NpgsqlParameter("@search" + paramIndex, "%" + word + "%"));
                }
                else
                {
                    // Use full-text search for longer keywords
                    searchConditions.Add(@"
                        (to_tsvector('english', u.name) @@ plainto_tsquery('english', @search" + paramIndex + @") OR 
                         to_tsvector('english', COALESCE(a.description, '')) @@ plainto_tsquery('english', @search" + paramIndex + @"))");
                    searchParams.Add(new NpgsqlParameter("@search" + paramIndex, word));
                }
                paramIndex++;
            }
            else
            {
                var nameConditions = new List<string>();
                var descConditions = new List<string>();
                
                foreach (var word in words)
                {
                    if (word.Length < 4)
                    {
                        // Use simple LIKE for very short keywords
                        nameConditions.Add("LOWER(u.name) LIKE LOWER(@search" + paramIndex + ")");
                        descConditions.Add("COALESCE(LOWER(a.description), '') LIKE LOWER(@search" + paramIndex + ")");
                        searchParams.Add(new NpgsqlParameter("@search" + paramIndex, "%" + word + "%"));
                    }
                    else
                    {
                        // Use full-text search for longer keywords
                        nameConditions.Add("to_tsvector('english', u.name) @@ plainto_tsquery('english', @search" + paramIndex + ")");
                        descConditions.Add("to_tsvector('english', COALESCE(a.description, '')) @@ plainto_tsquery('english', @search" + paramIndex + ")");
                        searchParams.Add(new NpgsqlParameter("@search" + paramIndex, word));
                    }
                    paramIndex++;
                }
                
                searchConditions.Add("(" + string.Join(" OR ", nameConditions) + " OR " + string.Join(" OR ", descConditions) + ")");
            }
        }

        var sql = $@"
            SELECT 
                u.universe_id,
                u.root_place_id,
                u.name,
                u.creator_user_id,
                creator.user_name as creator_name,
                COALESCE(a.place_custom_icon_url, a.place_generated_icon_url, a.thumbnail_url, '/images/blocked.png') as icon_url,
                COALESCE(a.place_custom_icon_url, a.place_generated_icon_url, a.thumbnail_url, '/images/blocked.png') as thumbnail_url,
                u.created_at,
                a.last_updated,
                COALESCE(a.upvotes, 0) as up_votes,
                COALESCE(a.downvotes, 0) as down_votes,
                COALESCE(u.visit_count, 0) as visit_count
            FROM universes u
            INNER JOIN users creator ON u.creator_user_id = creator.user_id
            INNER JOIN assets a ON u.root_place_id = a.asset_id
            WHERE a.is_place = true
            AND a.access_type = 1 -- Public access only
            AND u.root_place_id IS NOT NULL
            AND ({string.Join(" AND ", searchConditions)})
            ORDER BY a.last_updated DESC -- Filter by last updated
            LIMIT @maxRows OFFSET @startRow";

        using var cmd = new NpgsqlCommand(sql, conn);
        
        foreach (var param in searchParams)
        {
            cmd.Parameters.Add(param);
        }
        
        cmd.Parameters.AddWithValue("@maxRows", maxRows);
        cmd.Parameters.AddWithValue("@startRow", startRow);

        using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var games = new List<GameEntry>();

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var game = new GameEntry
            {
                UniverseId = reader.GetInt64(reader.GetOrdinal("universe_id")),
                PlaceId = reader.GetInt64(reader.GetOrdinal("root_place_id")),
                Name = reader.GetString(reader.GetOrdinal("name")),
                CreatorName = reader.GetString(reader.GetOrdinal("creator_name")),
                CreatorUserId = reader.GetInt64(reader.GetOrdinal("creator_user_id")),
                IconUrl = reader.GetString(reader.GetOrdinal("icon_url")),
                ThumbnailUrl = reader.GetString(reader.GetOrdinal("thumbnail_url")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                UpVotes = reader.GetInt32(reader.GetOrdinal("up_votes")),
                DownVotes = reader.GetInt32(reader.GetOrdinal("down_votes")),
                VisitCount = reader.GetInt32(reader.GetOrdinal("visit_count")),
                Playing = 0
            };
            games.Add(game);
        }

        return games;
    }

    public static async Task<List<GameEntry>> GetGameEntriesByUniverseIdsAsync(
        List<long> universeIds,
        string connectionString = "",
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));
        if (universeIds == null || universeIds.Count == 0)
            return new List<GameEntry>();

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        var sql = @"
            SELECT u.universe_id, u.root_place_id, u.name, u.creator_user_id,
                   creator.user_name as creator_name,
                   COALESCE(a.thumbnail_url, '/images/blocked.png') as icon_url,
                   COALESCE(a.thumbnail_url, '/images/blocked.png') as thumbnail_url,
                   u.created_at,
                   COALESCE(a.upvotes, 0) as up_votes,
                   COALESCE(a.downvotes, 0) as down_votes,
                   COALESCE(u.visit_count, 0) as visit_count
            FROM universes u
            INNER JOIN users creator ON u.creator_user_id = creator.user_id
            INNER JOIN assets a ON u.root_place_id = a.asset_id
            WHERE u.universe_id = ANY(@universeIds)";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("universeIds", universeIds.ToArray());

        using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var games = new List<GameEntry>();
        var idLookup = new HashSet<long>(universeIds);

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var uid = reader.GetInt64(reader.GetOrdinal("universe_id"));
            if (!idLookup.Contains(uid)) continue;

            games.Add(new GameEntry
            {
                UniverseId = uid,
                PlaceId = reader.GetInt64(reader.GetOrdinal("root_place_id")),
                Name = reader.GetString(reader.GetOrdinal("name")),
                CreatorName = reader.GetString(reader.GetOrdinal("creator_name")),
                CreatorUserId = reader.GetInt64(reader.GetOrdinal("creator_user_id")),
                IconUrl = reader.GetString(reader.GetOrdinal("icon_url")),
                ThumbnailUrl = reader.GetString(reader.GetOrdinal("thumbnail_url")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                UpVotes = reader.GetInt32(reader.GetOrdinal("up_votes")),
                DownVotes = reader.GetInt32(reader.GetOrdinal("down_votes")),
                VisitCount = reader.GetInt32(reader.GetOrdinal("visit_count")),
                Playing = 0
            });
        }

        return games;
    }

    public static async Task<List<long>> GetPopularUniverseIdsAsync(
        int maxResults = 100,
        string connectionString = "",
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        var sql = @"
            SELECT u.universe_id, COALESCE(u.visit_count, 0) as visit_count
            FROM universes u
            INNER JOIN assets a ON u.root_place_id = a.asset_id
            WHERE a.is_place = true
            AND a.access_type = 1
            AND u.root_place_id IS NOT NULL
            ORDER BY u.visit_count DESC
            LIMIT @maxResults";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("maxResults", maxResults);

        using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var ids = new List<long>();

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            ids.Add(reader.GetInt64(0));

        return ids;
    }

    public static async Task<List<long>> GetTopRatedUniverseIdsAsync(
        int maxResults = 50,
        string connectionString = "",
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        var sql = @"
            SELECT u.universe_id,
                   ((COALESCE(a.upvotes, 0) * 1000 + COALESCE(jsonb_array_length(a.favorites), 0) * 1000)::float /
                    GREATEST(COALESCE(a.upvotes, 0) * 1000 + COALESCE(jsonb_array_length(a.favorites), 0) * 1000 + COALESCE(a.downvotes, 0), 1)) * 1000 +
                   LN(COALESCE(u.visit_count, 0) * 0.0001 + 1) as score
            FROM universes u
            INNER JOIN assets a ON u.root_place_id = a.asset_id
            WHERE a.is_place = true
            AND a.access_type = 1
            AND u.root_place_id IS NOT NULL
            ORDER BY score DESC
            LIMIT @maxResults";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("maxResults", maxResults);

        using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var ids = new List<long>();

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            ids.Add(reader.GetInt64(0));

        return ids;
    }

    public static async Task<Dictionary<long, int>> GetLivePlayerCountsForUniverseIdsAsync(
        List<long> universeIds,
        string connectionString,
        string arbiterBaseUrl,
        CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<long, int>();
        if (universeIds == null || universeIds.Count == 0)
            return result;

        var tasks = universeIds.Select(async uid =>
        {
            try
            {
                var placeResult = await GetLivePlayerCountForUniverseAsync(uid, connectionString, arbiterBaseUrl, false, cancellationToken).ConfigureAwait(false);
                return (UniverseId: uid, Count: placeResult.TotalPlayers);
            }
            catch
            {
                return (UniverseId: uid, Count: 0);
            }
        });

        var counts = await Task.WhenAll(tasks).ConfigureAwait(false);
        foreach (var (uid, count) in counts)
            result[uid] = count;

        return result;
    }

    /// <summary>
    /// Gets live player count for a specific place by querying the Arbiter
    /// Returns real-time player data from all running game servers for this place
    /// </summary>
    public static async Task<(int TotalPlayers, int AuthenticatedPlayers, int GuestPlayers, List<PlayerInfo> Players)> GetLivePlayerCountForPlaceAsync(
        int placeId,
        string arbiterBaseUrl,
        bool useLiveData = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            var url = $"{arbiterBaseUrl}/api/gameservers/players/{placeId}?live={useLiveData}";
            var response = await httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                return (0, 0, 0, new List<PlayerInfo>());
            }

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            int totalPlayers = root.GetProperty("totalPlayerCount").GetInt32();
            int authPlayers = root.GetProperty("authenticatedPlayerCount").GetInt32();
            int guestPlayers = root.GetProperty("guestPlayerCount").GetInt32();

            var players = new List<PlayerInfo>();
            if (root.TryGetProperty("players", out var playersElement))
            {
                foreach (var player in playersElement.EnumerateArray())
                {
                    players.Add(new PlayerInfo
                    {
                        UserId = player.TryGetProperty("userId", out var uid) ? uid.GetInt64() : 0,
                        Name = player.TryGetProperty("name", out var name) ? name.GetString() ?? "" : "",
                        DisplayName = player.TryGetProperty("displayName", out var dname) ? dname.GetString() ?? "" : ""
                    });
                }
            }

            return (totalPlayers, authPlayers, guestPlayers, players);
        }
        catch
        {
            return (0, 0, 0, new List<PlayerInfo>());
        }
    }

    /// <summary>
    /// Player info returned from Arbiter player queries
    /// </summary>
    public class PlayerInfo
    {
        public long UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Gets all place IDs associated with a universe
    /// </summary>
    public static async Task<List<long>> GetUniversePlaceIdsAsync(long universeId, string connectionString, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));
        if (universeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(universeId));

        const string sql = @"
            SELECT root_place_id, place_ids
            FROM universes
            WHERE universe_id = @universeId";

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("universeId", universeId);

        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        var placeIds = new List<long>();

        if (result != null && result != DBNull.Value)
        {
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var rootPlaceId = reader.IsDBNull(0) ? 0L : reader.GetInt64(0);
                var placeIdsArray = reader.IsDBNull(1) ? new long[0] : (long[])reader.GetValue(1);

                if (rootPlaceId > 0)
                    placeIds.Add(rootPlaceId);

                placeIds.AddRange(placeIdsArray.Where(pid => pid > 0));
            }
        }

        return placeIds.Distinct().ToList();
    }

    /// <summary>
    /// Represents a place entry with basic display data
    /// </summary>
    public class PlaceEntry
    {
        public long PlaceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
    }

    /// <summary>
    /// Gets place display data (name, thumbnail) for the given ordered place IDs
    /// Results are returned in the same order as the input list
    /// </summary>
    public static async Task<List<PlaceEntry>> GetPlacesByIdsAsync(List<long> placeIds, string connectionString, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));
        if (placeIds == null || placeIds.Count == 0)
            return new List<PlaceEntry>();

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        var sql = @"SELECT asset_id, name, COALESCE(thumbnail_url, '/images/blocked.png') as thumbnail_url
                    FROM assets
                    WHERE asset_id = ANY(@ids)
                    AND is_place = true";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("ids", placeIds.ToArray());

        var fetched = new Dictionary<long, (string Name, string Thumbnail)>();
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var id = reader.GetInt64(0);
            fetched[id] = (reader.GetString(1), reader.GetString(2));
        }

        var results = new List<PlaceEntry>(placeIds.Count);
        foreach (var id in placeIds)
        {
            if (fetched.TryGetValue(id, out var data))
            {
                results.Add(new PlaceEntry
                {
                    PlaceId = id,
                    Name = data.Name,
                    ThumbnailUrl = data.Thumbnail
                });
            }
        }

        return results;
    }

    /// <summary>
    /// Gets live player counts for all places in a universe by querying the Arbiter
    /// Aggregates player counts across all running game servers for all places in the universe
    /// </summary>
    public static async Task<UniversePlayersResult> GetLivePlayerCountForUniverseAsync(
        long universeId,
        string connectionString,
        string arbiterBaseUrl,
        bool useLiveData = false,
        CancellationToken cancellationToken = default)
    {
        var placeIds = await GetUniversePlaceIdsAsync(universeId, connectionString, cancellationToken).ConfigureAwait(false);

        if (placeIds.Count == 0)
        {
            return new UniversePlayersResult
            {
                UniverseId = universeId,
                TotalPlayers = 0,
                AuthenticatedPlayers = 0,
                GuestPlayers = 0,
                PlaceCount = 0,
                ActiveServers = 0,
                Players = new List<PlayerInfo>(),
                PlaceBreakdown = new List<PlacePlayerInfo>()
            };
        }

        var allPlayers = new List<PlayerInfo>();
        var placeBreakdown = new List<PlacePlayerInfo>();
        int totalAuth = 0;
        int totalGuest = 0;
        int totalServers = 0;
        var tasks = placeIds.Select(async placeId =>
        {
            var result = await GetLivePlayerCountForPlaceAsync((int)placeId, arbiterBaseUrl, useLiveData, cancellationToken).ConfigureAwait(false);

            lock (allPlayers)
            {
                totalAuth += result.AuthenticatedPlayers;
                totalGuest += result.GuestPlayers;
                allPlayers.AddRange(result.Players);
            }

            return new PlacePlayerInfo
            {
                PlaceId = placeId,
                TotalPlayers = result.TotalPlayers,
                AuthenticatedPlayers = result.AuthenticatedPlayers,
                GuestPlayers = result.GuestPlayers,
                PlayerNames = result.Players.Select(p => p.Name).ToList()
            };
        });

        var results = await Task.WhenAll(tasks).ConfigureAwait(false);
        placeBreakdown.AddRange(results);

        return new UniversePlayersResult
        {
            UniverseId = universeId,
            TotalPlayers = totalAuth + totalGuest,
            AuthenticatedPlayers = totalAuth,
            GuestPlayers = totalGuest,
            PlaceCount = placeIds.Count,
            ActiveServers = totalServers,
            Players = allPlayers,
            PlaceBreakdown = placeBreakdown
        };
    }


    /// <summary>
    /// Result containing player info for a universe
    /// </summary>
    public class UniversePlayersResult
    {
        public long UniverseId { get; set; }
        public int TotalPlayers { get; set; }
        public int AuthenticatedPlayers { get; set; }
        public int GuestPlayers { get; set; }
        public int PlaceCount { get; set; }
        public int ActiveServers { get; set; }
        public List<PlayerInfo> Players { get; set; } = new();
        public List<PlacePlayerInfo> PlaceBreakdown { get; set; } = new();
    }

    /// <summary>
    /// Player info per place
    /// </summary>
    public class PlacePlayerInfo
    {
        public long PlaceId { get; set; }
        public int TotalPlayers { get; set; }
        public int AuthenticatedPlayers { get; set; }
        public int GuestPlayers { get; set; }
        public List<string> PlayerNames { get; set; } = new();
    }

    public static async Task<List<long>> GetFriendActivityUniverseIdsAsync(
        long userId, string connectionString, int limit = 10,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString) || userId <= 0)
            return new List<long>();

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"
            WITH friend_visits AS (
                SELECT unnest(u.visited_universes) AS universe_id
                FROM user_friends uf
                JOIN users u ON u.user_id = uf.friend_user_id
                WHERE uf.user_id = @userId
                AND u.visited_universes IS NOT NULL
                AND array_length(u.visited_universes, 1) > 0
            )
            SELECT universe_id, COUNT(*) AS friend_count
            FROM friend_visits
            GROUP BY universe_id
            ORDER BY friend_count DESC, universe_id
            LIMIT @limit";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("userId", userId);
        cmd.Parameters.AddWithValue("limit", limit);

        var ids = new List<long>();
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            ids.Add(reader.GetInt64(0));

        return ids;
    }

    public static async Task<List<long>> GetRecentlyVisitedUniverseIdsAsync(
        long userId, string connectionString,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString) || userId <= 0)
            return new List<long>();

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string sql = "SELECT visited_universes FROM users WHERE user_id = @userId";
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("userId", userId);
        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return result is long[] ids ? ids.Take(10).ToList() : new List<long>();
    }

    public static async Task<List<long>> GetFavoritePlaceUniverseIdsAsync(
        long userId, string connectionString,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString) || userId <= 0)
            return new List<long>();

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string sql = "SELECT favorites FROM users WHERE user_id = @userId";
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("userId", userId);
        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);

        var universeIds = new List<long>();
        if (result is not string json || string.IsNullOrWhiteSpace(json))
            return universeIds;

        try
        {
            var placeIds = JsonSerializer.Deserialize<List<long>>(json);
            if (placeIds == null || placeIds.Count == 0)
                return universeIds;

            foreach (var placeId in placeIds.Take(20))
            {
                var universeId = await GamesRepository.GetUniverseIdFromPlaceIdAsync(
                    connectionString, placeId, cancellationToken).ConfigureAwait(false);
                if (universeId.HasValue)
                    universeIds.Add(universeId.Value);
            }
        }
        catch { }

        return universeIds;
    }

    public static async Task<List<long>> GetFeaturedGameUniverseIdsAsync(
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        var sql = "SELECT universe_id FROM featured_games ORDER BY featured_game_id ASC LIMIT 4";
        using var cmd = new NpgsqlCommand(sql, conn);
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        var ids = new List<long>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            ids.Add(reader.GetInt64(0));
        }
        return ids;
    }
}
