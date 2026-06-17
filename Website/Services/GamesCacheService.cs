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
    public class GamesCacheOptions
    {
        public bool Enabled { get; set; } = true;
        public int RefreshIntervalMinutes { get; set; } = 2;
        public int MaxCachedGames { get; set; } = 200;
        public int CacheTimeoutMinutes { get; set; } = 30;
    }

    public class CachedGameEntry
    {
        public long UniverseId { get; set; }
        public DateTime CachedAt { get; set; }
        public int SortFilter { get; set; }
        public int GenreFilter { get; set; }
        public int CacheOrder { get; set; }
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

        public async Task RefreshCacheAsync(CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled) return;

            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

                await CachePopularGamesAsync(conn, 100, cancellationToken);
                await CacheTopRatedGamesAsync(conn, 50, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh games cache");
                throw;
            }
        }

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
                    SELECT universe_id, cached_at, sort_filter, genre_filter, cache_order
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

        public async Task<List<long>> GetCachedUniverseIdsAsync(
            int sortFilter = 1,
            int genreFilter = 1,
            int startRow = 0,
            int maxRows = 14,
            CancellationToken cancellationToken = default)
        {
            var entries = await GetCachedGamesAsync(sortFilter, genreFilter, startRow, maxRows, cancellationToken);
            var ids = new List<long>();
            foreach (var e in entries)
                ids.Add(e.UniverseId);
            return ids;
        }

        private async Task CachePopularGamesAsync(NpgsqlConnection conn, int maxGames, CancellationToken cancellationToken)
        {
            var universeIds = await GamesQueries.GetPopularUniverseIdsAsync(maxGames, _connectionString, cancellationToken);
            await InsertCachedUniverseIdsAsync(conn, universeIds, 1, 1, cancellationToken);
        }

        private async Task CacheTopRatedGamesAsync(NpgsqlConnection conn, int maxGames, CancellationToken cancellationToken)
        {
            var universeIds = await GamesQueries.GetTopRatedUniverseIdsAsync(maxGames, _connectionString, cancellationToken);
            await InsertCachedUniverseIdsAsync(conn, universeIds, 11, 1, cancellationToken);
        }

        private async Task InsertCachedUniverseIdsAsync(
            NpgsqlConnection conn,
            List<long> universeIds,
            int sortFilter,
            int genreFilter,
            CancellationToken cancellationToken)
        {
            if (universeIds.Count == 0) return;

            var deleteSql = "DELETE FROM cached_games WHERE sort_filter = @sortFilter AND genre_filter = @genreFilter";
            using (var deleteCmd = new NpgsqlCommand(deleteSql, conn))
            {
                deleteCmd.Parameters.AddWithValue("sortFilter", sortFilter);
                deleteCmd.Parameters.AddWithValue("genreFilter", genreFilter);
                await deleteCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            var insertSql = @"
                INSERT INTO cached_games (universe_id, cached_at, sort_filter, genre_filter, cache_order)
                VALUES (@universeId, NOW(), @sortFilter, @genreFilter, @cacheOrder)";

            for (int i = 0; i < universeIds.Count && i < _options.MaxCachedGames; i++)
            {
                using var cmd = new NpgsqlCommand(insertSql, conn);
                cmd.Parameters.AddWithValue("universeId", universeIds[i]);
                cmd.Parameters.AddWithValue("sortFilter", sortFilter);
                cmd.Parameters.AddWithValue("genreFilter", genreFilter);
                cmd.Parameters.AddWithValue("cacheOrder", i + 1);
                await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
