using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RCCArbiter.Scripting;

namespace RCCArbiter
{
    public class GameServerManager
    {
        private string _rccUrl;
        private readonly GameServerRccManager _rccManager;
        private readonly Dictionary<string, GameServerInfo> _activeServers = new();
        private readonly object _serversLock = new object();
        private readonly Timer _cleanupTimer;

        public class GameServerInfo
        {
            public string GameId { get; set; } = string.Empty;
            public int PlaceId { get; set; }
            public int Port { get; set; }
            public int MaxPlayers { get; set; }
            public string PrivateServerId { get; set; } = string.Empty;
            public DateTime StartTime { get; set; }
            public DateTime Expiration { get; set; }
            public int PlayerCount { get; set; }
            public string Status { get; set; } = "starting";
            public string BaseUrl { get; set; } = string.Empty;
            public Dictionary<string, object> LastStatus { get; set; } = new();
            public DateTime LastActivityTime { get; set; }
            public TimeSpan InactivityTimeout { get; set; } = TimeSpan.FromMinutes(30); // Default 30 minutes
            public bool AutoKillEnabled { get; set; } = true;
        }

        public GameServerManager(string rccUrl, IConfiguration config)
        {
            _rccUrl = rccUrl;
            _rccManager = new GameServerRccManager(config);
            _activeServers = new();
            _serversLock = new();
            _cleanupTimer = new Timer(CleanupExpiredServers, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        }

        public void UpdateRccUrl(string newRccUrl)
        {
            _rccUrl = newRccUrl;
        }

        public async Task<string> StartGameServerAsync(int placeId, int port = 53640, int maxPlayers = 10, string privateServerId = "", string baseUrl = "", int maxInactive = 0)
        {
            var gameId = Guid.NewGuid().ToString();
            
            if (string.IsNullOrWhiteSpace(baseUrl))
                baseUrl = "http://www.freblx.xyz";

            var rccReservation = _rccManager.ReserveForGameServer(gameId);
            if (rccReservation == null)
            {
                throw new InvalidOperationException("Failed to reserve RCC instance for game server");
            }

            var dedicatedRccUrl = rccReservation.Value.url;
            var job = new Job
            {
                id = gameId,
                expirationInSeconds = maxInactive > 0 ? maxInactive * 60 : 3600, // Use maxInactive minutes or default to 1 hour
                category = 2, // Game server category
                cores = 2
            };

            var parameters = new Dictionary<string, string>
            {
                ["placeId"] = placeId.ToString(),
                ["port"] = port.ToString(),
                ["maxPlayers"] = maxPlayers.ToString(),
                ["privateServerId"] = privateServerId,
                ["baseUrl"] = baseUrl
            };

            try
            {
                using var client = new RCCClient(dedicatedRccUrl);
                
                var scriptRenderer = new ScriptRenderer();
                var scriptTemplate = LoadScriptTemplate("StartGameServer");
                var renderedScript = scriptRenderer.Render(scriptTemplate, parameters);
                var lines = renderedScript.Split('\n');
                
                var script = new ScriptExecution
                {
                    name = "StartGameServer",
                    script = renderedScript,
                    arguments = null
                };

                var results = client.OpenJobEx(job, script);

                lock (_serversLock)
                {
                    _activeServers[gameId] = new GameServerInfo
                    {
                        GameId = gameId,
                        PlaceId = placeId,
                        Port = port,
                        MaxPlayers = maxPlayers,
                        PrivateServerId = privateServerId,
                        StartTime = DateTime.UtcNow,
                        Expiration = DateTime.UtcNow.AddSeconds(job.expirationInSeconds),
                        Status = "running",
                        BaseUrl = baseUrl,
                        LastStatus = ParseLuaResults(results),
                        LastActivityTime = DateTime.UtcNow,
                        InactivityTimeout = maxInactive > 0 ? TimeSpan.FromMinutes(maxInactive) : TimeSpan.MaxValue,
                        AutoKillEnabled = maxInactive > 0
                    };
                }

                return gameId;
            }
            catch (Exception ex)
            {
                _rccManager.ReleaseGameServer(gameId);
                throw new InvalidOperationException($"Failed to start game server: {ex.Message}", ex);
            }
        }

        private string LoadScriptTemplate(string scriptName)
        {
            var scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "Scripts", $"{scriptName}.lua");
            if (!File.Exists(scriptPath))
                throw new FileNotFoundException($"Script '{scriptName}.lua' not found");

            return File.ReadAllText(scriptPath);
        }

        public async Task<GameServerInfo> GetGameServerStatusAsync(string gameId)
        {
            lock (_serversLock)
            {
                if (!_activeServers.ContainsKey(gameId))
                    throw new ArgumentException("Game server not found");
            }

            var dedicatedRccUrl = _rccManager.GetGameServerUrl(gameId);
            if (string.IsNullOrWhiteSpace(dedicatedRccUrl))
            {
                throw new InvalidOperationException($"Dedicated RCC instance not found for game {gameId}");
            }

            var script = new ScriptExecution
            {
                name = "GetGameServerStatus",
                script = await LoadScriptAsync("GetGameServerStatus"),
                arguments = new LuaValue[]
                {
                    new LuaValue { type = LuaType.LUA_TSTRING, value = gameId }
                }
            };

            try
            {
                using var client = new RCCClient(dedicatedRccUrl);
                var results = client.ExecuteEx(gameId, script);
                var statusData = ParseLuaResults(results);

                lock (_serversLock)
                {
                    if (_activeServers.ContainsKey(gameId))
                    {
                        var server = _activeServers[gameId];
                        server.LastStatus = statusData;
                        
                        if (statusData.TryGetValue("players", out var playersObj) && 
                            playersObj is JsonElement playersElement &&
                            playersElement.TryGetProperty("count", out var countElement))
                        {
                            var newPlayerCount = countElement.GetInt32();
                            if (newPlayerCount != server.PlayerCount)
                            {
                                server.LastActivityTime = DateTime.UtcNow;
                            }
                            server.PlayerCount = newPlayerCount;
                        }
                        
                        server.LastActivityTime = DateTime.UtcNow;
                    }
                }

                return _activeServers[gameId];
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get game server status: {ex.Message}", ex);
            }
        }

        public void StopGameServer(string gameId)
        {
            try
            {
                var dedicatedRccUrl = _rccManager.GetGameServerUrl(gameId);
                if (!string.IsNullOrWhiteSpace(dedicatedRccUrl))
                {
                    using var client = new RCCClient(dedicatedRccUrl);
                    client.CloseJob(gameId);
                }
                else
                {
                    Console.WriteLine($"No dedicated RCC found for game {gameId}, skipping job close");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error closing RCC job {gameId}: {ex.Message}");
            }
            finally
            {
                _rccManager.ReleaseGameServer(gameId);
            }

            lock (_serversLock)
            {
                _activeServers.Remove(gameId);
            }
        }

        public Task RenewGameServerLeaseAsync(string gameId, int additionalSeconds = 3600)
        {
            try
            {
                using var client = new RCCClient(_rccUrl);
                client.RenewLease(gameId, additionalSeconds);

                lock (_serversLock)
                {
                    if (_activeServers.ContainsKey(gameId))
                    {
                        _activeServers[gameId].Expiration = DateTime.UtcNow.AddSeconds(additionalSeconds);
                    }
                }
                
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to renew game server lease: {ex.Message}", ex);
            }
        }

        public IEnumerable<GameServerInfo> GetAllGameServers()
        {
            lock (_serversLock)
            {
                return new List<GameServerInfo>(_activeServers.Values);
            }
        }

        public GameServerInfo? GetGameServerInfo(string gameId)
        {
            lock (_serversLock)
            {
                return _activeServers.TryGetValue(gameId, out var info) ? info : null;
            }
        }

        public void CleanupExpiredServers()
        {
            var now = DateTime.UtcNow;
            var expiredServers = new List<string>();

            lock (_serversLock)
            {
                foreach (var kvp in _activeServers)
                {
                    if (kvp.Value.Expiration <= now)
                    {
                        expiredServers.Add(kvp.Key);
                    }
                }
            }

            foreach (var gameId in expiredServers)
            {
                try
                {
                    StopGameServer(gameId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error cleaning up expired server {gameId}: {ex.Message}");
                }
            }
        }

        public void CleanupInactiveServers()
        {
            var now = DateTime.UtcNow;
            var inactiveServers = new List<string>();

            lock (_serversLock)
            {
                foreach (var kvp in _activeServers)
                {
                    var server = kvp.Value;
                    if (server.AutoKillEnabled && server.PlayerCount == 0)
                    {
                        var inactiveDuration = now - server.LastActivityTime;
                        if (inactiveDuration >= server.InactivityTimeout)
                        {
                            inactiveServers.Add(kvp.Key);
                            Console.WriteLine($"Server {server.GameId} inactive for {inactiveDuration.TotalMinutes:F1} minutes, auto-killing...");
                        }
                    }
                }
            }

            foreach (var gameId in inactiveServers)
            {
                try
                {
                    StopGameServer(gameId);
                    Console.WriteLine($"Auto-killed inactive server {gameId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error auto-killing inactive server {gameId}: {ex.Message}");
                }
            }
        }

        public void UpdateServerActivity(string gameId)
        {
            lock (_serversLock)
            {
                if (_activeServers.TryGetValue(gameId, out var server))
                {
                    server.LastActivityTime = DateTime.UtcNow;
                }
            }
        }

        public void SetServerInactivityTimeout(string gameId, TimeSpan timeout)
        {
            lock (_serversLock)
            {
                if (_activeServers.TryGetValue(gameId, out var server))
                {
                    server.InactivityTimeout = timeout;
                    Console.WriteLine($"Set inactivity timeout for server {gameId} to {timeout.TotalMinutes} minutes");
                }
            }
        }

        public void Dispose()
        {
            _cleanupTimer?.Dispose();
            _rccManager?.Dispose();
        }

        /// <summary>
        /// Cleanup expired game servers (called by timer)
        /// </summary>
        private void CleanupExpiredServers(object? state)
        {
            var now = DateTime.UtcNow;
            var expiredServers = new List<string>();

            lock (_serversLock)
            {
                Console.WriteLine($"Checking { _activeServers.Count} servers for expiration...");
                
                foreach (var kvp in _activeServers)
                {
                    var server = kvp.Value;
                    var timeUntilExpiration = server.Expiration - now;
                    
                    Console.WriteLine($"Server {server.GameId}: expires in {timeUntilExpiration.TotalMinutes:F1} minutes");
                    
                    if (server.Expiration <= now)
                    {
                        expiredServers.Add(kvp.Key);
                        Console.WriteLine($"Server {server.GameId} is EXPIRED and will be cleaned up");
                    }
                }
            }

            if (expiredServers.Count == 0)
            {
                return;
            }

            foreach (var gameId in expiredServers)
            {
                try
                {ing
                    var rccUrl = _rccManager.GetGameServerUrl(gameId);
                    StopGameServer(gameId);
                    Console.WriteLine($"Successfully cleaned up expired server {gameId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error cleaning up expired server {gameId}: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }

        }

        private async Task<string> LoadScriptAsync(string scriptName)
        {
            var scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "Scripts", $"{scriptName}.lua");
            if (!File.Exists(scriptPath))
                throw new FileNotFoundException($"Script '{scriptName}.lua' not found");

            return await File.ReadAllTextAsync(scriptPath);
        }

        private Dictionary<string, object> ParseLuaResults(LuaValue[] results)
        {
            var dict = new Dictionary<string, object>();

            if (results != null && results.Length > 0 && results[0].type == LuaType.LUA_TTABLE)
            {
                ParseLuaTable(results[0], dict);
            }

            return dict;
        }

        private void ParseLuaTable(LuaValue table, Dictionary<string, object> dict)
        {
            if (table.table == null) return;

            for (int i = 0; i < table.table.Length; i += 2)
            {
                if (i + 1 < table.table.Length)
                {
                    var key = table.table[i];
                    var value = table.table[i + 1];

                    if (key.type == LuaType.LUA_TSTRING && !string.IsNullOrWhiteSpace(key.value))
                    {
                        dict[key.value!] = ConvertLuaValue(value);
                    }
                }
            }
        }

        private object ConvertLuaValue(LuaValue value)
        {
            switch (value.type)
            {
                case LuaType.LUA_TNIL:
                    return null;
                case LuaType.LUA_TBOOLEAN:
                    return bool.Parse(value.value ?? "false");
                case LuaType.LUA_TNUMBER:
                    return double.Parse(value.value ?? "0");
                case LuaType.LUA_TSTRING:
                    return value.value ?? string.Empty;
                case LuaType.LUA_TTABLE:
                    var dict = new Dictionary<string, object>();
                    ParseLuaTable(value, dict);
                    return dict;
                default:
                    return value.value;
            }
        }
    }
}
