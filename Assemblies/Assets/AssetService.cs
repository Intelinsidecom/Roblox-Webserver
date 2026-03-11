using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Npgsql;

namespace Assets
{
    public class AssetService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public AssetService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Gets the character fetch response for a user, returning semicolon-separated URLs for worn assets
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="scheme">The request scheme (http/https)</param>
        /// <param name="host">The request host</param>
        /// <returns>Semicolon-separated URLs for character assets</returns>
        public async Task<string> GetCharacterFetchAsync(string userId, string scheme, string host)
        {
            var pid = string.IsNullOrWhiteSpace(userId) ? "0" : userId;
            var baseUrl = $"{scheme}://{host}";

            long.TryParse(pid, out var uid);

            var wornAssetIds = new List<long>();
            var connStr = _configuration.GetConnectionString("Default");
            if (!string.IsNullOrWhiteSpace(connStr) && uid > 0)
            {
                try
                {
                    await using var conn = new NpgsqlConnection(connStr);
                    await conn.OpenAsync().ConfigureAwait(false);

                    const string sql = @"select awa.asset_id
from avatar_worn_assets awa
join assets a on a.asset_id = awa.asset_id
where awa.user_id = @uid
order by awa.asset_id";

                    using var cmd = new NpgsqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("uid", uid);

                    await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var assetId = reader.GetInt64(0);
                        wornAssetIds.Add(assetId);
                    }
                }
                catch
                {
                    // Ignore DB errors and fall back to empty list
                }
            }

            var bodyColorsUrl = $"{baseUrl}/asset/bodycolors.ashx?userId={pid}";
            var urls = new List<string> { bodyColorsUrl };

            foreach (var assetId in wornAssetIds)
            {
                urls.Add($"{baseUrl}/asset/?id={assetId}");
            }

            // Response format (semicolon-separated URLs), per request:
            // http://your.url.here/Asset/bodycolors.ashx;http://your.url.here/Asset/?id=TSHIRT;http://your.url.here/Asset/?id=PANTS
            return string.Join(";", urls);
        }

        /// <summary>
        /// Attempts to fetch an asset from Roblox's asset delivery service
        /// </summary>
        /// <param name="assetId">The asset ID to fetch</param>
        /// <param name="contentType">The expected content type</param>
        /// <returns>File stream result if successful, null if failed</returns>
        public async Task<(Stream? Stream, string ContentType, string? Error)> TryFetchFromRobloxAssetDeliveryAsync(long assetId, string? contentType)
        {
            try
            {
                var baseUrl = _configuration["RobloxAssetDelivery:BaseUrl"] ?? "https://assetdelivery.roblox.com";
                var client = _httpClientFactory.CreateClient();
                
                var response = await client.GetAsync($"{baseUrl}/v1/asset/?id={assetId}");
                if (!response.IsSuccessStatusCode)
                {
                    return (null, string.Empty, "Asset not found locally or on Roblox asset delivery");
                }

                var stream = await response.Content.ReadAsStreamAsync();
                var ct = string.IsNullOrWhiteSpace(contentType) 
                    ? response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream" 
                    : contentType;
                
                return (stream, ct, null);
            }
            catch (Exception)
            {
                return (null, string.Empty, "Failed to fetch asset from Roblox asset delivery");
            }
        }

        /// <summary>
        /// Gets asset metadata including content hash, file extension, and content type
        /// </summary>
        /// <param name="assetId">The asset ID</param>
        /// <returns>Asset metadata or null if not found</returns>
        public async Task<(string? Hash, string? Extension, string? ContentType)> GetAssetMetadataAsync(long assetId)
        {
            var connStr = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
                return (null, null, null);

            try
            {
                await using var conn = new NpgsqlConnection(connStr);
                await conn.OpenAsync().ConfigureAwait(false);

                const string sql = @"select content_hash, file_extension, content_type from assets where asset_id = @id";
                using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("id", assetId);

                await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
                if (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var hash = reader.IsDBNull(0) ? null : reader.GetString(0);
                    var ext = reader.IsDBNull(1) ? null : reader.GetString(1);
                    var contentType = reader.IsDBNull(2) ? null : reader.GetString(2);
                    return (hash, ext, contentType);
                }
            }
            catch
            {
                // Return null on any error
            }

            return (null, null, null);
        }

        /// <summary>
        /// Gets the full file path for an asset
        /// </summary>
        /// <param name="hash">The content hash</param>
        /// <param name="extension">The file extension</param>
        /// <returns>Full file path or null if assets directory not configured</returns>
        public string? GetAssetFilePath(string? hash, string? extension)
        {
            if (string.IsNullOrWhiteSpace(hash))
                return null;

            var assetsRoot = _configuration["Assets:Directory"];
            if (string.IsNullOrWhiteSpace(assetsRoot))
                return null;

            var assetFolder = Path.Combine(assetsRoot, "asset");
            var fileName = string.IsNullOrWhiteSpace(extension) ? hash : hash + extension;
            return Path.Combine(assetFolder, fileName);
        }

        /// <summary>
        /// Checks if Roblox asset delivery is enabled
        /// </summary>
        /// <returns>True if enabled, false otherwise</returns>
        public bool IsRobloxAssetDeliveryEnabled()
        {
            var configValue = _configuration["RobloxAssetDelivery:Enabled"];
            return bool.TryParse(configValue, out var enabled) && enabled;
        }
    }
}
