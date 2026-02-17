using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Games;

/// <summary>
/// Database query helpers for games and universe operations
/// This class contains SQL helpers that were moved from PlacesController to better organize code
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
                0 as playing_count
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
            1 => "ORDER BY u.created_at DESC", // Popular (by newest for now since no playing data)
            2 => "ORDER BY u.created_at DESC", // Top Favorite (by newest for now since no vote data)
            3 => "ORDER BY u.created_at DESC", // Featured (by newest)
            8 => "ORDER BY u.created_at DESC", // Top Earning (by newest for now)
            9 => "ORDER BY u.created_at DESC", // Top Paid (by newest for now)
            11 => "ORDER BY u.created_at DESC", // Top Rated (by newest for now since no vote data)
            16 => "ORDER BY u.created_at DESC", // Top Retaining (by newest for now)
            _ => "ORDER BY u.created_at DESC" // Default to newest
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
                Playing = reader.GetInt32(reader.GetOrdinal("playing_count"))
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
                0 as playing_count
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
                Playing = reader.GetInt32(reader.GetOrdinal("playing_count"))
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
                0 as playing_count
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
                Playing = reader.GetInt32(reader.GetOrdinal("playing_count"))
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
                0 as playing_count
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
                PlaceId = reader.GetInt64(1), // root_place_id
                Name = reader.GetString(2),
                CreatorUserId = reader.GetInt64(3),
                CreatorName = reader.GetString(4),
                IconUrl = reader.GetString(5),
                ThumbnailUrl = reader.GetString(6),
                CreatedAt = reader.GetDateTime(7),
                UpVotes = reader.GetInt32(8),
                DownVotes = reader.GetInt32(9),
                Playing = reader.GetInt32(10)
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
                0 as playing_count
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
                Playing = reader.GetInt32(reader.GetOrdinal("playing_count"))
            };
            games.Add(game);
        }

        return games;
    }
}
