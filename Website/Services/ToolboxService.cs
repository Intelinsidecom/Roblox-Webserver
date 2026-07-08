using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Website.Services
{
    public class ToolboxCacheOptions
    {
        public bool Enabled { get; set; } = true;
        public int RefreshIntervalHours { get; set; } = 24;
        public int MaxCachedPerSort { get; set; } = 5000;
    }

    public class ToolboxAssetEntry
    {
        public long AssetId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int AssetTypeId { get; set; }
        public long OwnerUserId { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public long Upvotes { get; set; }
        public long Downvotes { get; set; }
        public bool IsOwnedByCurrentUser { get; set; }
    }

    public class ToolboxService : BackgroundService
    {
        private readonly ILogger<ToolboxService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ToolboxCacheOptions _options;
        private readonly string _connectionString;

        private static readonly int[] AssetTypeIds = { 3, 4, 10, 13 };

        private static readonly Dictionary<string, (int SortType, string OrderBy)> SortMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Updated"] = (1, "a.last_updated DESC NULLS LAST"),
            ["Favorites"] = (2, "jsonb_array_length(COALESCE(a.favorites,'[]'::jsonb)) DESC"),
            ["Ratings"] = (3, "(COALESCE(a.upvotes,0) - COALESCE(a.downvotes,0)) DESC"),
            ["MostTaken"] = (4, "COALESCE(a.sales,0) DESC"),
            ["Relevance"] = (5, "(COALESCE(a.upvotes,0) + COALESCE(a.sales,0) + jsonb_array_length(COALESCE(a.favorites,'[]'::jsonb))) DESC"),
        };

        private const string DefaultSort = "Relevance";

        public ToolboxService(ILogger<ToolboxService> logger, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _connectionString = _configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Connection string 'Default' not found");
            _options = new ToolboxCacheOptions();
            _configuration.GetSection("Toolbox:Cache").Bind(_options);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.Enabled)
            {
                _logger.LogInformation("Toolbox cache is disabled");
                return;
            }

            try
            {
                await RefreshCacheAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize toolbox cache");
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromHours(_options.RefreshIntervalHours), stoppingToken);
                    await RefreshCacheAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error refreshing toolbox cache");
                }
            }
        }

        public async Task RefreshCacheAsync(CancellationToken ct = default)
        {
            if (!_options.Enabled) return;

            _logger.LogInformation("Starting toolbox cache refresh");

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);

            foreach (var assetTypeId in AssetTypeIds)
            {
                foreach (var kv in SortMap)
                {
                    await RefreshSortTypeAsync(conn, kv.Value.SortType, assetTypeId, ct).ConfigureAwait(false);
                }
            }

            _logger.LogInformation("Toolbox cache refresh completed");
        }

        private async Task RefreshSortTypeAsync(NpgsqlConnection conn, int sortType, int assetTypeId, CancellationToken ct)
        {
            var orderBy = SortMap.Values.First(v => v.SortType == sortType).OrderBy;

            var deleteSql = "DELETE FROM toolbox_cache WHERE sort_type = @sortType AND asset_type_id = @assetTypeId";
            await using (var deleteCmd = new NpgsqlCommand(deleteSql, conn))
            {
                deleteCmd.Parameters.AddWithValue("sortType", sortType);
                deleteCmd.Parameters.AddWithValue("assetTypeId", assetTypeId);
                await deleteCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }

            var insertSql = $@"
                WITH ordered AS (
                    SELECT a.asset_id,
                           ROW_NUMBER() OVER (ORDER BY {orderBy}) AS rn
                    FROM assets a
                    WHERE a.asset_type_id = @assetTypeId
                      AND (a.is_place = false OR a.is_place IS NULL)
                      AND a.is_copying_allowed = true
                )
                INSERT INTO toolbox_cache (sort_type, asset_type_id, asset_id, sort_order)
                SELECT @sortType, @assetTypeId, asset_id, rn
                FROM ordered
                WHERE rn <= @maxOrder";

            await using (var insertCmd = new NpgsqlCommand(insertSql, conn))
            {
                insertCmd.Parameters.AddWithValue("sortType", sortType);
                insertCmd.Parameters.AddWithValue("assetTypeId", assetTypeId);
                insertCmd.Parameters.AddWithValue("maxOrder", _options.MaxCachedPerSort);
                await insertCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }
        }

        public async Task<(List<ToolboxAssetEntry> Results, long TotalCount)> SearchToolboxAsync(
            int? assetTypeId,
            string? keyword,
            string? sort,
            int page,
            int pageSize,
            long? creatorId,
            long? groupId,
            long currentUserId,
            CancellationToken ct = default)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);
            var offset = (page - 1) * pageSize;

            var sortType = DefaultSort;
            if (!string.IsNullOrWhiteSpace(sort) && SortMap.ContainsKey(sort))
                sortType = sort;

            var useCache = string.IsNullOrWhiteSpace(keyword) && creatorId == null && groupId == null;

            if (useCache && assetTypeId.HasValue && _options.Enabled)
                return await SearchFromCacheAsync(assetTypeId.Value, SortMap[sortType].SortType, offset, pageSize, currentUserId, ct).ConfigureAwait(false);

            return await SearchFromAssetsAsync(assetTypeId, keyword, sortType, offset, pageSize, creatorId, currentUserId, ct).ConfigureAwait(false);
        }

        private async Task<(List<ToolboxAssetEntry>, long)> SearchFromCacheAsync(
            int assetTypeId, int sortType, int offset, int pageSize,
            long currentUserId, CancellationToken ct)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);

            var countSql = "SELECT COUNT(*) FROM toolbox_cache WHERE sort_type = @sortType AND asset_type_id = @assetTypeId";
            long totalCount;
            await using (var countCmd = new NpgsqlCommand(countSql, conn))
            {
                countCmd.Parameters.AddWithValue("sortType", sortType);
                countCmd.Parameters.AddWithValue("assetTypeId", assetTypeId);
                totalCount = (long)(await countCmd.ExecuteScalarAsync(ct).ConfigureAwait(false) ?? 0L);
            }

            var userJoin = currentUserId > 0
                ? "LEFT JOIN user_assets ua ON ua.asset_id = a.asset_id AND ua.user_id = @currentUserId"
                : "";

            var dataSql = $@"
                SELECT a.asset_id, a.name, a.asset_type_id, a.owner_user_id,
                       a.thumbnail_url, a.upvotes, a.downvotes, u.user_name
                       {(currentUserId > 0 ? ", ua.user_id" : "")}
                FROM toolbox_cache tc
                JOIN assets a ON a.asset_id = tc.asset_id
                LEFT JOIN users u ON u.user_id = a.owner_user_id
                {userJoin}
                WHERE tc.sort_type = @sortType AND tc.asset_type_id = @assetTypeId
                ORDER BY tc.sort_order
                OFFSET @offset LIMIT @limit";

            await using var cmd = new NpgsqlCommand(dataSql, conn);
            cmd.Parameters.AddWithValue("sortType", sortType);
            cmd.Parameters.AddWithValue("assetTypeId", assetTypeId);
            cmd.Parameters.AddWithValue("offset", offset);
            cmd.Parameters.AddWithValue("limit", pageSize);
            if (currentUserId > 0)
                cmd.Parameters.AddWithValue("currentUserId", currentUserId);

            var results = new List<ToolboxAssetEntry>();
            await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
            while (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                results.Add(new ToolboxAssetEntry
                {
                    AssetId = reader.GetInt64(0),
                    Name = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    AssetTypeId = reader.GetInt32(2),
                    OwnerUserId = reader.GetInt64(3),
                    ThumbnailUrl = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    Upvotes = reader.IsDBNull(5) ? 0 : reader.GetInt64(5),
                    Downvotes = reader.IsDBNull(6) ? 0 : reader.GetInt64(6),
                    OwnerName = reader.IsDBNull(7) ? "" : reader.GetString(7),
                    IsOwnedByCurrentUser = currentUserId > 0 && !reader.IsDBNull(8) && reader.GetInt64(8) > 0,
                });
            }

            return (results, totalCount);
        }

        private async Task<(List<ToolboxAssetEntry>, long)> SearchFromAssetsAsync(
            int? assetTypeId, string? keyword, string sortType, int offset, int pageSize,
            long? creatorId, long currentUserId, CancellationToken ct)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);

            var whereClauses = new List<string>();
            var parameters = new Dictionary<string, object>();

            whereClauses.Add("a.is_copying_allowed = true");

            if (assetTypeId.HasValue)
            {
                whereClauses.Add("a.asset_type_id = @typeId");
                parameters["typeId"] = assetTypeId.Value;
                whereClauses.Add("(a.is_place = false OR a.is_place IS NULL)");
            }

            if (creatorId.HasValue)
            {
                whereClauses.Add("a.owner_user_id = @creatorId");
                parameters["creatorId"] = creatorId.Value;
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                whereClauses.Add("a.name ILIKE @keywordPattern");
                parameters["keywordPattern"] = "%" + keyword.Trim() + "%";
            }

            var whereSql = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : "";

            var countSql = $"SELECT COUNT(*) FROM assets a {whereSql}";
            long totalCount;
            await using (var countCmd = new NpgsqlCommand(countSql, conn))
            {
                foreach (var kv in parameters)
                    countCmd.Parameters.AddWithValue(kv.Key, kv.Value);
                totalCount = (long)(await countCmd.ExecuteScalarAsync(ct).ConfigureAwait(false) ?? 0L);
            }

            var orderBy = SortMap.TryGetValue(sortType, out var sortInfo) ? sortInfo.OrderBy : SortMap[DefaultSort].OrderBy;

            var userJoin = currentUserId > 0
                ? "LEFT JOIN user_assets ua ON ua.asset_id = a.asset_id AND ua.user_id = @currentUserId"
                : "";

            var dataSql = $@"
                SELECT a.asset_id, a.name, a.asset_type_id, a.owner_user_id,
                       a.thumbnail_url, a.upvotes, a.downvotes, u.user_name
                       {(currentUserId > 0 ? ", ua.user_id" : "")}
                FROM assets a
                LEFT JOIN users u ON u.user_id = a.owner_user_id
                {userJoin}
                {whereSql}
                ORDER BY {orderBy}
                OFFSET @offset LIMIT @limit";

            await using var cmd = new NpgsqlCommand(dataSql, conn);
            foreach (var kv in parameters)
                cmd.Parameters.AddWithValue(kv.Key, kv.Value);
            if (currentUserId > 0)
                cmd.Parameters.AddWithValue("currentUserId", currentUserId);
            cmd.Parameters.AddWithValue("offset", offset);
            cmd.Parameters.AddWithValue("limit", pageSize);

            var results = new List<ToolboxAssetEntry>();
            await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
            while (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                results.Add(new ToolboxAssetEntry
                {
                    AssetId = reader.GetInt64(0),
                    Name = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    AssetTypeId = reader.GetInt32(2),
                    OwnerUserId = reader.GetInt64(3),
                    ThumbnailUrl = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    Upvotes = reader.IsDBNull(5) ? 0 : reader.GetInt64(5),
                    Downvotes = reader.IsDBNull(6) ? 0 : reader.GetInt64(6),
                    OwnerName = reader.IsDBNull(7) ? "" : reader.GetString(7),
                    IsOwnedByCurrentUser = currentUserId > 0 && !reader.IsDBNull(8) && reader.GetInt64(8) > 0,
                });
            }

            return (results, totalCount);
        }
    }
}
