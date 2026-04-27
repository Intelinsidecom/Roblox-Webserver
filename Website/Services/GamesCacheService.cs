using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Threading;
using System.Threading.Tasks;
using Games;

namespace Website.Services
{
    /// <summary>
    /// Configuration options for games caching
    /// </summary>
    public class GamesCacheOptions
    {
        public bool Enabled { get; set; } = true;
        public int RefreshIntervalMinutes { get; set; } = 15;
        public int MaxCachedGames { get; set; } = 200;
        public int CacheTimeoutMinutes { get; set; } = 30;
    }

    /// <summary>
    /// Cached game entry with additional metadata
    /// </summary>
    public class CachedGameEntry
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
        public DateTime CachedAt { get; set; }
        public int SortFilter { get; set; }
        public int GenreFilter { get; set; }
        public int CacheOrder { get; set; }
        public int TotalVotes => UpVotes + DownVotes;
        public double VotePercentage => TotalVotes > 0 ? (double)UpVotes / TotalVotes * 100 : 0;
    }

    /// <summary>
    /// Service for caching games data to improve performance
    /// Caches frequently accessed game data in database with periodic refresh
    /// </summary>
    public class GamesCacheService : BackgroundService
    {
        private readonly ILogger<GamesCacheService> _logger;
        private readonly IConfiguration _configuration;
        private readonly GamesCacheOptions _options;
        private readonly string _connectionString;

        public GamesCacheService(
            ILogger<GamesCacheService> logger,
            IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            
            _connectionString = _configuration.GetConnectionString("Default") 
                ?? throw new InvalidOperationException("Connection string 'Default' not found");
            
            _options = new GamesCacheOptions();
            _configuration.GetSection("Games:Cache").Bind(_options);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.Enabled)
            {
                _logger.LogInformation("Games caching is disabled");
                return;
            }
            try
            {
                await RefreshCacheAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize games cache - service will continue running");
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(_options.RefreshIntervalMinutes), stoppingToken);
                    await RefreshCacheAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error refreshing games cache - will retry on next interval");
                }
            }
        }

        /// <summary>
        /// Refreshes the games cache by fetching fresh data from database
        /// </summary>
        public async Task RefreshCacheAsync(CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled) return;

            
            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

                // Clear existing cache
                await ClearCacheTableAsync(conn, cancellationToken);

                // Cache most popular games (SortFilter 1 - Popular)
                await CacheGamesByFilterAsync(conn, 1, 1, 100, cancellationToken);

                // Cache top rated games (SortFilter 11 - Top Rated)  
                await CacheGamesByFilterAsync(conn, 11, 1, 50, cancellationToken);

                // Cache newest games (SortFilter 1 - Popular but with newest ordering)
                await CacheNewestGamesAsync(conn, 50, cancellationToken);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh games cache");
                throw;
            }
        }

        /// <summary>
        /// Gets cached games for the specified filters
        /// </summary>
        public async Task<List<CachedGameEntry>> GetCachedGamesAsync(
            int sortFilter = 1,
            int genreFilter = 1,
            int startRow = 0,
            int maxRows = 14,
            CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled)
                return new List<CachedGameEntry>();

            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

                var sql = @"
                    SELECT universe_id, place_id, name, creator_name, creator_user_id,
                           icon_url, thumbnail_url, playing, up_votes, down_votes,
                           created_at, cached_at, sort_filter, genre_filter, cache_order
                    FROM cached_games 
                    WHERE sort_filter = @sortFilter 
                    AND (@genreFilter = 1 OR genre_filter = @genreFilter)
                    AND cached_at > NOW() - INTERVAL '" + _options.CacheTimeoutMinutes + @" minutes'
                    ORDER BY cache_order
                    LIMIT @maxRows OFFSET @startRow";

                using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("sortFilter", sortFilter);
                cmd.Parameters.AddWithValue("genreFilter", genreFilter);
                cmd.Parameters.AddWithValue("maxRows", maxRows);
                cmd.Parameters.AddWithValue("startRow", startRow);

                using var reader = await cmd.ExecuteReaderAsync(CancellationToken.None).ConfigureAwait(false);
                var games = new List<CachedGameEntry>();

                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    var game = new CachedGameEntry
                    {
                        UniverseId = reader.GetInt64(reader.GetOrdinal("universe_id")),
                        PlaceId = reader.GetInt64(reader.GetOrdinal("place_id")),
                        Name = reader.GetString(reader.GetOrdinal("name")),
                        CreatorName = reader.GetString(reader.GetOrdinal("creator_name")),
                        CreatorUserId = reader.GetInt64(reader.GetOrdinal("creator_user_id")),
                        IconUrl = reader.GetString(reader.GetOrdinal("icon_url")),
                        ThumbnailUrl = reader.GetString(reader.GetOrdinal("thumbnail_url")),
                        Playing = reader.GetInt32(reader.GetOrdinal("playing")),
                        UpVotes = reader.GetInt32(reader.GetOrdinal("up_votes")),
                        DownVotes = reader.GetInt32(reader.GetOrdinal("down_votes")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                        CachedAt = reader.GetDateTime(reader.GetOrdinal("cached_at")),
                        SortFilter = reader.GetInt32(reader.GetOrdinal("sort_filter")),
                        GenreFilter = reader.GetInt32(reader.GetOrdinal("genre_filter")),
                        CacheOrder = reader.GetInt32(reader.GetOrdinal("cache_order"))
                    };
                    games.Add(game);
                }

                return games;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cached games");
                return new List<CachedGameEntry>();
            }
        }

        /// <summary>
        /// Checks if cache is available and valid for the specified filters
        /// </summary>
        public async Task<bool> IsCacheAvailableAsync(
            int sortFilter = 1, 
            int genreFilter = 1, 
            CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled) return false;

            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

                var sql = @"
                    SELECT COUNT(*)
                    FROM cached_games 
                    WHERE sort_filter = @sortFilter 
                    AND (@genreFilter = 1 OR genre_filter = @genreFilter)
                    AND cached_at > NOW() - INTERVAL '" + _options.CacheTimeoutMinutes + @" minutes'";

                using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("sortFilter", sortFilter);
                cmd.Parameters.AddWithValue("genreFilter", genreFilter);

                var result = await cmd.ExecuteScalarAsync(CancellationToken.None).ConfigureAwait(false);
                return Convert.ToInt32(result ?? 0) > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking cache availability");
                return false;
            }
        }

        /// <summary>
        /// Converts cached game entries to GameEntry format
        /// </summary>
        public static List<GamesQueries.GameEntry> ToGameEntries(List<CachedGameEntry> cachedGames)
        {
            var games = new List<GamesQueries.GameEntry>();
            
            foreach (var cached in cachedGames)
            {
                games.Add(new GamesQueries.GameEntry
                {
                    UniverseId = cached.UniverseId,
                    PlaceId = cached.PlaceId,
                    Name = cached.Name,
                    CreatorName = cached.CreatorName,
                    CreatorUserId = cached.CreatorUserId,
                    IconUrl = cached.IconUrl,
                    ThumbnailUrl = cached.ThumbnailUrl,
                    Playing = cached.Playing,
                    UpVotes = cached.UpVotes,
                    DownVotes = cached.DownVotes,
                    CreatedAt = cached.CreatedAt
                });
            }

            return games;
        }

        private async Task ClearCacheTableAsync(NpgsqlConnection conn, CancellationToken cancellationToken)
        {
            var sql = "DELETE FROM cached_games";
            using var cmd = new NpgsqlCommand(sql, conn);
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task CacheGamesByFilterAsync(
            NpgsqlConnection conn, 
            int sortFilter, 
            int genreFilter, 
            int maxGames, 
            CancellationToken cancellationToken)
        {
            var games = await GamesQueries.GetPublicGamesAsync(
                sortFilter, 0, genreFilter, 183, 0, maxGames, _connectionString, cancellationToken);

            await InsertCachedGamesAsync(conn, games, sortFilter, genreFilter, cancellationToken);
        }

        private async Task CacheNewestGamesAsync(NpgsqlConnection conn, int maxGames, CancellationToken cancellationToken)
        {
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
                    0 as up_votes,
                    0 as down_votes,
                    0 as playing_count
                FROM universes u
                INNER JOIN users creator ON u.creator_user_id = creator.user_id
                INNER JOIN assets a ON u.root_place_id = a.asset_id
                WHERE a.is_place = true 
                AND a.access_type = 1
                AND u.root_place_id IS NOT NULL
                ORDER BY u.created_at DESC
                LIMIT @maxGames";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("maxGames", maxGames);

            var games = new List<GamesQueries.GameEntry>();

            using (var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
            {
                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    var game = new GamesQueries.GameEntry
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
            }

            await InsertCachedGamesAsync(conn, games, 1, 1, cancellationToken);
        }

        private async Task InsertCachedGamesAsync(
            NpgsqlConnection conn, 
            List<GamesQueries.GameEntry> games, 
            int sortFilter, 
            int genreFilter, 
            CancellationToken cancellationToken)
        {
            if (games.Count == 0) return;

            var sql = @"
                INSERT INTO cached_games (
                    universe_id, place_id, name, creator_name, creator_user_id,
                    icon_url, thumbnail_url, playing, up_votes, down_votes,
                    created_at, cached_at, sort_filter, genre_filter, cache_order
                ) VALUES (
                    @universeId, @placeId, @name, @creatorName, @creatorUserId,
                    @iconUrl, @thumbnailUrl, @playing, @upVotes, @downVotes,
                    @createdAt, NOW(), @sortFilter, @genreFilter, @cacheOrder
                )";

            for (int i = 0; i < games.Count && i < _options.MaxCachedGames; i++)
            {
                var game = games[i];
                using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("universeId", game.UniverseId);
                cmd.Parameters.AddWithValue("placeId", game.PlaceId);
                cmd.Parameters.AddWithValue("name", game.Name);
                cmd.Parameters.AddWithValue("creatorName", game.CreatorName);
                cmd.Parameters.AddWithValue("creatorUserId", game.CreatorUserId);
                cmd.Parameters.AddWithValue("iconUrl", game.IconUrl);
                cmd.Parameters.AddWithValue("thumbnailUrl", game.ThumbnailUrl);
                cmd.Parameters.AddWithValue("playing", game.Playing);
                cmd.Parameters.AddWithValue("upVotes", game.UpVotes);
                cmd.Parameters.AddWithValue("downVotes", game.DownVotes);
                cmd.Parameters.AddWithValue("createdAt", game.CreatedAt);
                cmd.Parameters.AddWithValue("sortFilter", sortFilter);
                cmd.Parameters.AddWithValue("genreFilter", genreFilter);
                cmd.Parameters.AddWithValue("cacheOrder", i + 1);

                await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Generates a cache key for logging/debugging purposes
        /// </summary>
        public static string GenerateCacheKey(int sortFilter, int genreFilter, int startRow, int maxRows)
        {
            return $"games_sf{sortFilter}_gf{genreFilter}_sr{startRow}_mr{maxRows}";
        }
    }
}
