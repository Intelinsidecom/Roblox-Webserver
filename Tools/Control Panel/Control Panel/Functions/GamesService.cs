using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Control_Panel.Properties;

namespace ControlPanel.Functions
{
    /// <summary>
    /// Service for communicating with the Arbiter game server management API
    /// </summary>
    public class GamesService
    {
        private readonly HttpClient _httpClient;
        private readonly string _arbiterBaseUrl;

        public GamesService()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                {
                    #if DEBUG
                    return true;
                    #else
                    return errors == SslPolicyErrors.None;
                    #endif
                },
                SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13,
                AllowAutoRedirect = true
            };

            _httpClient = new HttpClient(handler);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "ControlPanel/1.0");
            _arbiterBaseUrl = $"http://{Settings.Default.ArbiterHost}:{Settings.Default.ArbiterPort}";
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        #region Data Models

        public class GameServerInfo
        {
            [JsonPropertyName("gameId")]
            public string GameId { get; set; } = string.Empty;

            [JsonPropertyName("placeId")]
            public int PlaceId { get; set; }

            [JsonPropertyName("port")]
            public int Port { get; set; }

            [JsonPropertyName("maxPlayers")]
            public int MaxPlayers { get; set; }

            [JsonPropertyName("playerCount")]
            public int PlayerCount { get; set; }

            [JsonPropertyName("status")]
            public string Status { get; set; } = string.Empty;

            [JsonPropertyName("startTime")]
            public DateTime StartTime { get; set; }

            [JsonPropertyName("expiration")]
            public DateTime Expiration { get; set; }

            [JsonPropertyName("baseUrl")]
            public string BaseUrl { get; set; } = string.Empty;

            [JsonPropertyName("privateServerId")]
            public string PrivateServerId { get; set; } = string.Empty;

            [JsonPropertyName("lastActivityTime")]
            public DateTime LastActivityTime { get; set; }

            [JsonPropertyName("inactivityTimeout")]
            public TimeSpan InactivityTimeout { get; set; }

            [JsonPropertyName("autoKillEnabled")]
            public bool AutoKillEnabled { get; set; }

            [JsonPropertyName("connectionUrl")]
            public string ConnectionUrl => $"roblox://localhost:{Port}?gameId={GameId}&placeId={PlaceId}";
        }

        public class StartGameServerRequest
        {
            [JsonPropertyName("placeId")]
            public int PlaceId { get; set; }

            [JsonPropertyName("port")]
            public int Port { get; set; } = 0;

            [JsonPropertyName("maxPlayers")]
            public int MaxPlayers { get; set; } = 10;

            [JsonPropertyName("privateServerId")]
            public string? PrivateServerId { get; set; }

            [JsonPropertyName("baseUrl")]
            public string? BaseUrl { get; set; }

            [JsonPropertyName("maxInactive")]
            public int MaxInactive { get; set; } = 0;
        }

        public class StartGameServerResponse
        {
            [JsonPropertyName("gameId")]
            public string GameId { get; set; } = string.Empty;

            [JsonPropertyName("status")]
            public string Status { get; set; } = string.Empty;
        }

        public class GameServerStatusResponse
        {
            [JsonPropertyName("gameId")]
            public string GameId { get; set; } = string.Empty;

            [JsonPropertyName("placeId")]
            public int PlaceId { get; set; }

            [JsonPropertyName("port")]
            public int Port { get; set; }

            [JsonPropertyName("maxPlayers")]
            public int MaxPlayers { get; set; }

            [JsonPropertyName("playerCount")]
            public int PlayerCount { get; set; }

            [JsonPropertyName("status")]
            public string Status { get; set; } = string.Empty;

            [JsonPropertyName("startTime")]
            public DateTime StartTime { get; set; }

            [JsonPropertyName("expiration")]
            public DateTime Expiration { get; set; }

            [JsonPropertyName("baseUrl")]
            public string BaseUrl { get; set; } = string.Empty;

            [JsonPropertyName("privateServerId")]
            public string PrivateServerId { get; set; } = string.Empty;

            [JsonPropertyName("lastActivityTime")]
            public DateTime LastActivityTime { get; set; }

            [JsonPropertyName("lastStatus")]
            public Dictionary<string, object> LastStatus { get; set; } = new Dictionary<string, object>();
        }

        public class StopGameServerResponse
        {
            [JsonPropertyName("gameId")]
            public string GameId { get; set; } = string.Empty;

            [JsonPropertyName("status")]
            public string Status { get; set; } = string.Empty;
        }

        public class ArbiterHealthResponse
        {
            [JsonPropertyName("ok")]
            public bool Ok { get; set; }

            [JsonPropertyName("arbiter")]
            public ArbiterInfo Arbiter { get; set; } = new ArbiterInfo();

            [JsonPropertyName("rcc")]
            public RccInfo Rcc { get; set; } = new RccInfo();
        }

        public class ArbiterInfo
        {
            [JsonPropertyName("name")]
            public string Name { get; set; } = string.Empty;

            [JsonPropertyName("version")]
            public string Version { get; set; } = string.Empty;
        }

        public class RccInfo
        {
            [JsonPropertyName("url")]
            public string Url { get; set; } = string.Empty;

            [JsonPropertyName("version")]
            public string? Version { get; set; }

            [JsonPropertyName("environmentCount")]
            public int? EnvironmentCount { get; set; }

            [JsonPropertyName("error")]
            public string? Error { get; set; }
        }

        #endregion

        #region API Methods

        /// <summary>
        /// Check if the Arbiter service is healthy and reachable
        /// </summary>
        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_arbiterBaseUrl}/health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get detailed health information from Arbiter
        /// </summary>
        public async Task<ArbiterHealthResponse?> GetHealthAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_arbiterBaseUrl}/health");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<ArbiterHealthResponse>(json);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get all active game servers
        /// </summary>
        public async Task<List<GameServerInfo>> GetAllGameServersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_arbiterBaseUrl}/api/gameservers");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var servers = JsonSerializer.Deserialize<List<GameServerInfo>>(json);
                    return servers ?? new List<GameServerInfo>();
                }
                return new List<GameServerInfo>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting game servers: {ex.Message}");
                return new List<GameServerInfo>();
            }
        }

        /// <summary>
        /// Start a new game server
        /// </summary>
        public async Task<StartGameServerResponse?> StartGameServerAsync(StartGameServerRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_arbiterBaseUrl}/api/gameservers/start", content);
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<StartGameServerResponse>(responseJson);
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error starting game server: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get the status of a specific game server
        /// </summary>
        public async Task<GameServerStatusResponse?> GetGameServerStatusAsync(string gameId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_arbiterBaseUrl}/api/gameservers/{gameId}/status");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<GameServerStatusResponse>(json);
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting game server status: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Stop a game server
        /// </summary>
        public async Task<StopGameServerResponse?> StopGameServerAsync(string gameId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_arbiterBaseUrl}/api/gameservers/{gameId}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<StopGameServerResponse>(json);
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error stopping game server: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Kill all game servers
        /// </summary>
        public async Task<bool> KillAllGameServersAsync()
        {
            try
            {
                var servers = await GetAllGameServersAsync();
                var successCount = 0;
                
                foreach (var server in servers)
                {
                    var result = await StopGameServerAsync(server.GameId);
                    if (result != null)
                        successCount++;
                }

                return successCount == servers.Count;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error killing all game servers: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get player count for a specific game server from the web API
        /// </summary>
        public async Task<int?> GetPlayerCountFromWebApiAsync(string gameId)
        {
            try
            {
                var websiteHost = Settings.Default.WebsiteHost?.Trim();
                var websitePort = Settings.Default.WebsitePort?.Trim();
                
                if (string.IsNullOrEmpty(websiteHost))
                {
                    throw new Exception("Website Host not configured in settings");
                }

                var baseUrl = $"http://{websiteHost}";
                if (!string.IsNullOrEmpty(websitePort) && websitePort != "80")
                {
                    baseUrl += $":{websitePort}";
                }

                var requestUrl = $"{baseUrl}/games/players/count?jobid={gameId}";
                _httpClient.DefaultRequestHeaders.Clear();
                var response = await _httpClient.GetAsync(requestUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var playerCountResponse = JsonSerializer.Deserialize<PlayerCountResponse>(json);
                    return playerCountResponse?.PlayerCount;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to get player count: HTTP {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting player count from web API: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get statistics about game servers
        /// </summary>
        public async Task<Dictionary<string, int>> GetGameServerStatisticsAsync()
        {
            var servers = await GetAllGameServersAsync();
            var stats = new Dictionary<string, int>
            {
                ["TotalServers"] = servers.Count,
                ["ActivePlayers"] = servers.Sum(s => s.PlayerCount),
                ["Running"] = servers.Count(s => s.Status.Equals("running", StringComparison.OrdinalIgnoreCase)),
                ["Starting"] = servers.Count(s => s.Status.Equals("starting", StringComparison.OrdinalIgnoreCase))
            };

            return stats;
        }

        /// <summary>
        /// Refresh server list and update statistics
        /// </summary>
        public async Task<List<GameServerInfo>> RefreshServerListAsync()
        {
            return await GetAllGameServersAsync();
        }

        #endregion

        #region Authentication Ticket Methods

        /// <summary>
        /// Create an authentication ticket for a specific game server
        /// </summary>
        public async Task<AuthenticationTicketInfo?> CreateAuthenticationTicketForServerAsync(long placeId, string gameId, int serverPort)
        {
            try
            {
                // Debug logging to help identify the issue
                System.Diagnostics.Debug.WriteLine($"[DEBUG] CreateAuthenticationTicketForServerAsync called with placeId={placeId}, gameId='{gameId}', serverPort={serverPort}");
                
                var websiteHost = Settings.Default.WebsiteHost?.Trim();
                var websitePort = Settings.Default.WebsitePort?.Trim();
                
                if (string.IsNullOrEmpty(websiteHost))
                {
                    throw new Exception("Website Host not configured in settings");
                }

                var baseUrl = $"http://{websiteHost}";
                if (!string.IsNullOrEmpty(websitePort) && websitePort != "80")
                {
                    baseUrl += $":{websitePort}";
                }

                var defaultUserIdStr = Settings.Default.DefaultOwnerUserId;
                if (!long.TryParse(defaultUserIdStr, out var defaultUserId) || defaultUserId <= 0)
                {
                    throw new Exception("Default Owner User ID not configured in settings");
                }

                // Request ticket for specific server with job ID
                var requestUrl = $"{baseUrl}/Game/PlaceLauncher.ashx?request=RequestGame&placeId={placeId}&serverPort={serverPort}&jobid={gameId}";
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Request URL: {requestUrl}");
                
                _httpClient.DefaultRequestHeaders.Clear();
                var response = await _httpClient.GetAsync(requestUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] PlaceLauncher API response: {responseJson}");
                    var placeLauncherResponse = JsonSerializer.Deserialize<PlaceLauncherResponse>(responseJson);
                    if (placeLauncherResponse != null && placeLauncherResponse.status == 2)
                    {
                        return new AuthenticationTicketInfo
                        {
                            TicketToken = placeLauncherResponse.authenticationTicket,
                            AuthenticationUrl = placeLauncherResponse.authenticationUrl,
                            JoinScriptUrl = placeLauncherResponse.joinScriptUrl,
                            ExpiresAt = DateTime.Now.AddHours(1),
                            PlaceId = placeId,
                            UniverseId = placeId,
                            GameId = gameId,
                            ServerPort = serverPort
                        };
                    }
                    else
                    {
                        throw new Exception(placeLauncherResponse?.message ?? "Failed to create ticket for specific server");
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    string errorMessage = $"HTTP {response.StatusCode}";
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(errorContent);
                        if (errorResponse != null && errorResponse.ContainsKey("error"))
                        {
                            errorMessage = errorResponse["error"].ToString() ?? errorMessage;
                            
                            if (errorResponse.ContainsKey("placeId"))
                            {
                                errorMessage += $" (Place ID: {errorResponse["placeId"]})";
                            }
                        }
                    }
                    catch
                    {
                        if (!string.IsNullOrEmpty(errorContent))
                        {
                            errorMessage += $": {errorContent}";
                        }
                    }
                    
                    throw new Exception(errorMessage);
                }
            }
            catch (HttpRequestException httpEx)
            {
                if (httpEx.InnerException?.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Error Details: {httpEx.InnerException.InnerException.Message}");
                }
                throw new Exception($"Failed to create authentication ticket for server (HTTP/SSL error): {httpEx.Message}", httpEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create authentication ticket for server: {ex.Message}", ex);
            }

            return null;
        }

        /// <summary>
        /// Create an authentication ticket for the specified place
        /// </summary>
        public async Task<AuthenticationTicketInfo?> CreateAuthenticationTicketAsync(long placeId)
        {
            try
            {
                var websiteHost = Settings.Default.WebsiteHost?.Trim();
                var websitePort = Settings.Default.WebsitePort?.Trim();
                
                if (string.IsNullOrEmpty(websiteHost))
                {
                    throw new Exception("Website Host not configured in settings");
                }

                var baseUrl = $"http://{websiteHost}";
                if (!string.IsNullOrEmpty(websitePort) && websitePort != "80")
                {
                    baseUrl += $":{websitePort}";
                }

                var defaultUserIdStr = Settings.Default.DefaultOwnerUserId;
                if (!long.TryParse(defaultUserIdStr, out var defaultUserId) || defaultUserId <= 0)
                {
                    throw new Exception("Default Owner User ID not configured in settings");
                }

                var requestUrl = $"{baseUrl}/Game/PlaceLauncher.ashx?request=RequestGame&placeId={placeId}";
                _httpClient.DefaultRequestHeaders.Clear();
                var response = await _httpClient.GetAsync(requestUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var placeLauncherResponse = JsonSerializer.Deserialize<PlaceLauncherResponse>(responseJson);
                    if (placeLauncherResponse != null && placeLauncherResponse.status == 2)
                    {
                        return new AuthenticationTicketInfo
                        {
                            TicketToken = placeLauncherResponse.authenticationTicket,
                            AuthenticationUrl = placeLauncherResponse.authenticationUrl,
                            JoinScriptUrl = placeLauncherResponse.joinScriptUrl,
                            ExpiresAt = DateTime.Now.AddHours(1),
                            PlaceId = placeId,
                            UniverseId = placeId
                        };
                    }
                    else
                    {
                        throw new Exception(placeLauncherResponse?.message ?? "Failed to create ticket");
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    string errorMessage = $"HTTP {response.StatusCode}";
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(errorContent);
                        if (errorResponse != null && errorResponse.ContainsKey("error"))
                        {
                            errorMessage = errorResponse["error"].ToString() ?? errorMessage;
                            
                            if (errorResponse.ContainsKey("placeId"))
                            {
                                errorMessage += $" (Place ID: {errorResponse["placeId"]})";
                            }
                        }
                    }
                    catch
                    {
                        if (!string.IsNullOrEmpty(errorContent))
                        {
                            errorMessage += $": {errorContent}";
                        }
                    }
                    
                    throw new Exception(errorMessage);
                }
            }
            catch (HttpRequestException httpEx)
            {
                if (httpEx.InnerException?.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Error Details: {httpEx.InnerException.InnerException.Message}");
                }
                throw new Exception($"Failed to create authentication ticket (HTTP/SSL error): {httpEx.Message}", httpEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create authentication ticket: {ex.Message}", ex);
            }

            return null;
        }

        #endregion

        #region Authentication Ticket Classes

        /// <summary>
        /// Information about an authentication ticket
        /// </summary>
        public class AuthenticationTicketInfo
        {
            public string TicketToken { get; set; } = string.Empty;
            public string AuthenticationUrl { get; set; } = string.Empty;
            public string JoinScriptUrl { get; set; } = string.Empty;
            public DateTime ExpiresAt { get; set; }
            public long PlaceId { get; set; }
            public long? UniverseId { get; set; }
            public string? GameId { get; set; }
            public int? ServerPort { get; set; }
        }

        /// <summary>
        /// Response from player count API
        /// </summary>
        public class PlayerCountResponse
        {
            [JsonPropertyName("success")]
            public bool Success { get; set; }

            [JsonPropertyName("jobId")]
            public string JobId { get; set; } = string.Empty;

            [JsonPropertyName("playerCount")]
            public int PlayerCount { get; set; }
        }

        /// <summary>
        /// Response from PlaceLauncher API
        /// </summary>
        public class PlaceLauncherResponse
        {
            [JsonPropertyName("jobId")]
            public string? jobId { get; set; }

            [JsonPropertyName("status")]
            public int status { get; set; }

            [JsonPropertyName("joinScriptUrl")]
            public string? joinScriptUrl { get; set; }

            [JsonPropertyName("authenticationUrl")]
            public string? authenticationUrl { get; set; }

            [JsonPropertyName("authenticationTicket")]
            public string? authenticationTicket { get; set; }

            [JsonPropertyName("message")]
            public string? message { get; set; }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        #endregion
    }
}
