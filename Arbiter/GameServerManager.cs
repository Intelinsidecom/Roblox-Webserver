using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RCCArbiter.Scripting;
using System.Collections.Concurrent;

namespace RCCArbiter
{
    public class GameServerManager
    {
        private string _rccUrl;
        private readonly IConfiguration _configuration;
        private readonly GameServerRccManager _rccManager;
        private readonly ConcurrentDictionary<string, GameServerInfo> _activeServers;
        private readonly object _serversLock;
        private readonly Timer _cleanupTimer;
        private readonly Timer _inactivityTimer;
        private readonly Timer _zeroPlayerKillTimer;
        private readonly Timer _rccHealthTimer;
        private readonly TimeSpan _rccKeepAliveDuration = TimeSpan.FromSeconds(10);
        private readonly int _zeroPlayerReportsRequired;

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
            public TimeSpan InactivityTimeout { get; set; } = TimeSpan.FromMinutes(30);
            public bool AutoKillEnabled { get; set; } = true;
            public bool HasReceivedReport { get; set; } = false;
            public int MaxInactiveMinutes { get; set; } = 0;
            public int AuthenticatedPlayerCount { get; set; } = 0;
            public int GuestPlayerCount { get; set; } = 0;
            public List<PlayerInfo> AuthenticatedPlayers { get; set; } = new();
            public List<PlayerInfo> GuestPlayers { get; set; } = new();
            public List<string> PlayerEvents { get; set; } = new();
            public string? ShutdownReason { get; set; }
            public int FailedReportCount { get; set; } = 0;
            public int ConsecutiveZeroPlayerReports { get; set; } = 0;
        }

        public class PlayerInfo
        {
            public string Name { get; set; } = string.Empty;
            public long UserId { get; set; } = 0;
            public string DisplayName { get; set; } = string.Empty;
            public bool CharacterLoaded { get; set; } = false;
            public double Ping { get; set; } = 0;
            public DateTime JoinTime { get; set; } = DateTime.UtcNow;
        }

        public GameServerManager(string rccUrl, IConfiguration config)
        {
            _rccUrl = rccUrl;
            _configuration = config;
            _rccManager = new GameServerRccManager(config);
            _activeServers = new();
            _serversLock = new();
            _zeroPlayerReportsRequired = int.TryParse(config["GameServer:ZeroPlayerReportsRequired"], out var zpr) ? zpr : 2;
            _cleanupTimer = new Timer(_ => Program.RunGuarded("GameServerManager.CleanupExpiredServers", () => CleanupExpiredServers()), null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
            _inactivityTimer = new Timer(_ => Program.RunGuarded("GameServerManager.CleanupInactiveServers", () => CleanupInactiveServers()), null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
            _zeroPlayerKillTimer = new Timer(_ => Program.RunGuarded("GameServerManager.CleanupZeroPlayerServers", () => CleanupZeroPlayerServers()), null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
            _rccHealthTimer = new Timer(_ => Program.RunGuarded("GameServerManager.CheckRccHealth", () => CheckRccHealth()), null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
        }

        public GameServerRccManager GetRccManager() => _rccManager;

        public void UpdateRccUrl(string newRccUrl)
        {
            _rccUrl = newRccUrl;
        }

        public async Task<string> StartGameServerAsync(int placeId, int port = 0, int maxPlayers = 10, string privateServerId = "", string baseUrl = "", int maxInactive = 0)
        {
            var gameId = Guid.NewGuid().ToString();
            
            if (string.IsNullOrWhiteSpace(baseUrl))
                baseUrl = "http://www.freblx.com";

            var rccReservation = _rccManager.ReserveForGameServer(gameId);
            if (rccReservation == null)
            {
                throw new InvalidOperationException("Failed to reserve RCC instance for game server");
            }

            var dedicatedRccUrl = rccReservation.Value.url;
            var job = new Job
            {
                id = gameId,
                expirationInSeconds = maxInactive > 0 ? maxInactive * 60 : 31536000, // 0 = 1 year (never expire)
                category = 2, // Game server category
                cores = 2
            };

            var accessKey = _configuration["Arbiter:AccessKey"] ?? "ChangeMe";
            var parameters = new Dictionary<string, string>
            {
                ["placeId"] = placeId.ToString(),
                ["port"] = port.ToString(),
                ["maxPlayers"] = maxPlayers.ToString(),
                ["privateServerId"] = privateServerId,
                ["baseUrl"] = baseUrl,
                ["gameId"] = gameId,
                ["arbiterUrl"] = "http://localhost:5000", // Default Arbiter URL
                ["accessKey"] = accessKey
            };
            
            try
            {
                using var client = new RCCClient(dedicatedRccUrl);
                
                int maxRetries = 6;
                int retryDelayMs = 2000;
                
                for (int attempt = 1; attempt <= maxRetries; attempt++)
                {
                    try
                    {
                        Console.WriteLine($"Testing RCC connection... Attempt {attempt}/{maxRetries}");
                        var version = client.GetVersion();
                        Console.WriteLine($"RCC connection successful, version: {version}");
                        break; // Success, exit retry loop
                    }
                    catch (Exception testEx)
                    {
                        Console.WriteLine($"RCC connection test failed (attempt {attempt}/{maxRetries}): {testEx.Message}");
                        
                        if (attempt == maxRetries)
                        {
                            throw new InvalidOperationException($"Cannot connect to RCC at {dedicatedRccUrl} after {maxRetries} attempts: {testEx.Message}", testEx);
                        }
                        
                        Console.WriteLine($"Waiting {retryDelayMs}ms before retry...");
                        await Task.Delay(retryDelayMs);
                    }
                }
                
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
                        Expiration = job.expirationInSeconds > 0 ? DateTime.UtcNow.AddSeconds(job.expirationInSeconds) : DateTime.MaxValue,
                        Status = "running",
                        BaseUrl = baseUrl,
                        LastStatus = ParseLuaResults(results),
                        LastActivityTime = DateTime.UtcNow,
                        InactivityTimeout = maxInactive > 0 ? TimeSpan.FromMinutes(maxInactive) : TimeSpan.FromMinutes(1),
                        AutoKillEnabled = true,
                        MaxInactiveMinutes = maxInactive
                    };
                    
                }

                return gameId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting game server: {ex.GetType().Name}: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
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

        public async Task<GameServerInfo> GetGameServerStatusAsync(string gameId, string? overrideRccUrl = null)
        {
            lock (_serversLock)
            {
                if (!_activeServers.ContainsKey(gameId))
                    throw new ArgumentException("Game server not found");

                var server = _activeServers[gameId];
                
                server.LastActivityTime = DateTime.UtcNow;
                return server;
            }
        }

        public void UpdatePlayerCount(string gameId, int playerCount)
        {
            lock (_serversLock)
            {
                if (_activeServers.ContainsKey(gameId))
                {
                    var server = _activeServers[gameId];
                    if (playerCount != server.PlayerCount)
                    {
                        server.LastActivityTime = DateTime.UtcNow;
                    }
                    server.PlayerCount = playerCount;
                    server.HasReceivedReport = true;
                }
            }
        }

        public void StopGameServer(string gameId, bool keepRccAlive = true)
        {
            try
            {
                var dedicatedRccUrl = _rccManager.GetGameServerUrl(gameId);
                if (!string.IsNullOrWhiteSpace(dedicatedRccUrl))
                {
                    using var client = new RCCClient(dedicatedRccUrl);
                    client.CloseJob(gameId);

                    if (keepRccAlive)
                    {
                        _ = Task.Run(async () =>
                        {
                            await Task.Delay(_rccKeepAliveDuration);
                            Program.RunGuarded("GameServerManager.ReleaseGameServer", () => _rccManager.ReleaseGameServer(gameId));
                        });
                    }
                    else
                    {
                        _rccManager.ReleaseGameServer(gameId);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error closing RCC job {gameId}: {ex.Message}");
                _rccManager.ReleaseGameServer(gameId);
            }

            lock (_serversLock)
            {
                if (_activeServers.TryGetValue(gameId, out var server))
                {
                    PortManager.ReleasePort(server.Port);
                    _activeServers.Remove(gameId, out _);
                }
            }
        }

        /// <summary>
        /// Immediately kill a server when reports show 0 players (fast shutdown path)
        /// </summary>
        public void KillZeroPlayerServer(string gameId)
        {

            lock (_serversLock)
            {
                if (_activeServers.TryGetValue(gameId, out var server))
                {
                    server.Status = "killing_0players";
                    server.ShutdownReason = "Zero players reported";
                }
            }
            
            StopGameServer(gameId, keepRccAlive: true);
        }

        public Task RenewGameServerLeaseAsync(string gameId, int additionalSeconds = 3600)
        {
            try
            {
                var dedicatedRccUrl = _rccManager.GetGameServerUrl(gameId);
                var urlToUse = !string.IsNullOrWhiteSpace(dedicatedRccUrl) ? dedicatedRccUrl : _rccUrl;
                using var client = new RCCClient(urlToUse);
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
                        }
                    }
                }
            }

            if (inactiveServers.Count == 0)
            {
                return;
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

        public string? GetGameServerRccUrl(string gameId)
        {
            return _rccManager.GetGameServerUrl(gameId);
        }

        public void UpdateGameServerStatus(string gameId, Dictionary<string, object> data)
        {
            lock (_serversLock)
            {
                if (!_activeServers.TryGetValue(gameId, out var server))
                    return;

                server.LastStatus = data;
                server.LastActivityTime = DateTime.UtcNow;
                server.FailedReportCount = 0;

                if (data.TryGetValue("players", out var playersObj) && playersObj is Dictionary<string, object> players)
                {
                    server.HasReceivedReport = true;
                    
                    if (players.TryGetValue("total", out var totalObj))
                    {
                        if (totalObj is double total)
                        {
                            server.PlayerCount = (int)total;
                        }
                        else if (totalObj is long totalLong)
                        {
                            server.PlayerCount = (int)totalLong;
                        }
                        else if (totalObj is int totalInt)
                        {
                            server.PlayerCount = totalInt;
                        }
                        else if (int.TryParse(totalObj?.ToString(), out var parsedInt))
                        {
                            server.PlayerCount = parsedInt;
                        }
                        else
                        {
                            Console.WriteLine($"Failed to parse total player count: {totalObj} (type: {totalObj?.GetType().Name})");
                        }
                    }
                    
                    if (players.TryGetValue("authenticatedCount", out var authObj))
                    {
                        if (authObj is double auth)
                        {
                            server.AuthenticatedPlayerCount = (int)auth;
                        }
                        else if (authObj is long authLong)
                        {
                            server.AuthenticatedPlayerCount = (int)authLong;
                        }
                        else if (authObj is int authInt)
                        {
                            server.AuthenticatedPlayerCount = authInt;
                        }
                        else if (int.TryParse(authObj?.ToString(), out var parsedAuth))
                        {
                            server.AuthenticatedPlayerCount = parsedAuth;
                        }
                    }
                    
                    if (players.TryGetValue("guestCount", out var guestObj))
                    {
                        if (guestObj is double guest)
                        {
                            server.GuestPlayerCount = (int)guest;
                        }
                        else if (guestObj is long guestLong)
                        {
                            server.GuestPlayerCount = (int)guestLong;
                        }
                        else if (guestObj is int guestInt)
                        {
                            server.GuestPlayerCount = guestInt;
                        }
                        else if (int.TryParse(guestObj?.ToString(), out var parsedGuest))
                        {
                            server.GuestPlayerCount = parsedGuest;
                        }
                    }

                    if (players.TryGetValue("authenticated", out var authListObj) && authListObj is object[] authList)
                    {
                        server.AuthenticatedPlayers = ParsePlayerList(authList);
                    }
                    if (players.TryGetValue("guests", out var guestListObj) && guestListObj is object[] guestList)
                    {
                        server.GuestPlayers = ParsePlayerList(guestList);
                    }
                }


                var serverAge = DateTime.UtcNow - server.StartTime;
                var canKillOnZeroPlayers = server.MaxInactiveMinutes == 0 || server.Expiration <= DateTime.UtcNow;
                
                if (server.PlayerCount == 0)
                {
                    server.ConsecutiveZeroPlayerReports++;
                    if (server.ConsecutiveZeroPlayerReports >= _zeroPlayerReportsRequired && server.AutoKillEnabled && server.Status != "killing_0players" && server.HasReceivedReport && serverAge.TotalSeconds >= 20 && canKillOnZeroPlayers)
                    {
                        _ = Task.Run(() => Program.RunGuarded("GameServerManager.KillZeroPlayerServer", () => KillZeroPlayerServer(gameId)));
                    }
                }
                else
                {
                    server.ConsecutiveZeroPlayerReports = 0;
                }
            }
        }

        /// <summary>
        /// Records a failed report attempt for a game server.
        /// Destroys the session after 3 consecutive failures (RCC is likely dead).
        /// </summary>
        public void RecordFailedReport(string gameId)
        {
            bool shouldStop = false;
            lock (_serversLock)
            {
                if (_activeServers.TryGetValue(gameId, out var server))
                {
                    server.FailedReportCount++;

                    if (server.FailedReportCount >= 3)
                    {
                        server.ShutdownReason = "RCC unreachable";
                        shouldStop = true;
                    }
                }
            }

            if (shouldStop)
            {
                try
                {
                    StopGameServer(gameId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RCC Health] Error stopping game server {gameId}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Cleanup servers that have 0 players reported via status updates
        /// Called every 10 seconds by timer
        /// </summary>
        private void CleanupZeroPlayerServers()
        {
            var now = DateTime.UtcNow;
            var zeroPlayerServers = new List<string>();

            lock (_serversLock)
            {
                foreach (var kvp in _activeServers)
                {
                    var server = kvp.Value;
                    
                    if (server.AutoKillEnabled && server.PlayerCount == 0 && server.Status != "killing_0players" && server.HasReceivedReport && server.ConsecutiveZeroPlayerReports >= _zeroPlayerReportsRequired)
                    {
                        var inactiveDuration = now - server.LastActivityTime;
                        var serverAge = now - server.StartTime;
                        var canKillZeroPlayer = server.MaxInactiveMinutes == 0 || server.Expiration <= now;
                        if (inactiveDuration >= TimeSpan.FromSeconds(30) && serverAge.TotalSeconds >= 20 && canKillZeroPlayer)
                        {
                            zeroPlayerServers.Add(kvp.Key);
                        }
                    }
                }
            }

            foreach (var gameId in zeroPlayerServers)
            {
                try
                {
                    Console.WriteLine($"[ZeroPlayer] Auto-killing zero-player server {gameId}");
                    KillZeroPlayerServer(gameId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ZeroPlayer] Error killing zero-player server {gameId}: {ex.Message}");
                }
            }
        }


        private List<PlayerInfo> ParsePlayerList(object[] players)
        {
            var list = new List<PlayerInfo>();
            foreach (var p in players)
            {
                if (p is PlayerInfo existing)
                {
                    list.Add(existing);
                }
                else if (p is Dictionary<string, object> dict)
                {
                    var player = new PlayerInfo
                    {
                        Name = dict.TryGetValue("Name", out var name) ? name?.ToString() ?? "" : "",
                        DisplayName = dict.TryGetValue("DisplayName", out var dname) ? dname?.ToString() ?? "" : "",
                        UserId = dict.TryGetValue("UserId", out var uid) && uid is double uidVal ? (long)uidVal : 0,
                        CharacterLoaded = dict.TryGetValue("CharacterLoaded", out var loaded) && loaded is bool loadedVal && loadedVal,
                        Ping = dict.TryGetValue("Ping", out var ping) && ping is double pingVal ? pingVal : 0
                    };
                    list.Add(player);
                }
            }
            return list;
        }

        public void RecordPlayerEvent(string gameId, long userId, string eventType)
        {
            lock (_serversLock)
            {
                if (!_activeServers.TryGetValue(gameId, out var server))
                    return;

                var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                var eventLog = $"[{timestamp}] User {userId} {eventType}";
                server.PlayerEvents.Add(eventLog);

                if (server.PlayerEvents.Count > 100)
                {
                    server.PlayerEvents.RemoveAt(0);
                }

                server.LastActivityTime = DateTime.UtcNow;

                if (string.Equals(eventType, "Join", StringComparison.OrdinalIgnoreCase))
                {
                    if (!server.AuthenticatedPlayers.Any(p => p.UserId == userId))
                    {
                        server.AuthenticatedPlayers.Add(new PlayerInfo
                        {
                            UserId = userId,
                            Name = "",
                            DisplayName = "",
                            JoinTime = DateTime.UtcNow
                        });
                        server.AuthenticatedPlayerCount = server.AuthenticatedPlayers.Count;
                        server.PlayerCount = server.AuthenticatedPlayerCount + server.GuestPlayerCount;
                    }
                }
                else if (string.Equals(eventType, "Leave", StringComparison.OrdinalIgnoreCase))
                {
                    var removed = server.AuthenticatedPlayers.RemoveAll(p => p.UserId == userId);
                    if (removed > 0)
                    {
                        server.AuthenticatedPlayerCount = server.AuthenticatedPlayers.Count;
                        server.PlayerCount = server.AuthenticatedPlayerCount + server.GuestPlayerCount;
                    }
                }
            }
        }

        public void RecordShutdown(string gameId, string reason)
        {
            lock (_serversLock)
            {
                if (!_activeServers.TryGetValue(gameId, out var server))
                    return;

                server.ShutdownReason = reason;
                server.Status = "shutting_down";

            }
        }

        public void SetServerInactivityTimeout(string gameId, TimeSpan timeout)
        {
            lock (_serversLock)
            {
                if (_activeServers.TryGetValue(gameId, out var server))
                {
                    server.InactivityTimeout = timeout;
                }
            }
        }

        /// <summary>
        /// Manually trigger inactivity cleanup (useful for testing)
        /// </summary>
        public void TriggerInactivityCheck()
        {
            CleanupInactiveServers();
        }

        /// <summary>
        /// Periodically check if dedicated RCC instances for game servers are alive.
        /// If unreachable, increment failure count and destroy after 3 failures.
        /// </summary>
        private void CheckRccHealth()
        {
            List<string> gameIds;
            lock (_serversLock)
            {
                gameIds = _activeServers.Keys.ToList();
            }

            foreach (var gameId in gameIds)
            {
                var dedicatedRccUrl = _rccManager.GetGameServerUrl(gameId);
                if (string.IsNullOrWhiteSpace(dedicatedRccUrl))
                    continue;

                try
                {
                    using var client = new RCCClient(dedicatedRccUrl);
                    client.GetVersion();
                }
                catch
                {
                    try
                    {
                        RecordFailedReport(gameId);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[RCC Health] Error recording failed report for game server {gameId}: {ex.Message}");
                    }
                }
            }
        }

        public void Dispose()
        {
            _cleanupTimer?.Dispose();
            _inactivityTimer?.Dispose();
            _zeroPlayerKillTimer?.Dispose();
            _rccHealthTimer?.Dispose();
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
                foreach (var kvp in _activeServers)
                {
                    var server = kvp.Value;
                    var timeUntilExpiration = server.Expiration - now;
                    
                    if (server.Expiration <= now)
                    {
                        expiredServers.Add(kvp.Key);
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
                {
                    var rccUrl = _rccManager.GetGameServerUrl(gameId);
                    StopGameServer(gameId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error cleaning up expired server {gameId}: {ex.Message}");
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

        private const int MaxLuaDepth = 32;

        private Dictionary<string, object> ParseLuaResults(LuaValue[] results)
        {
            return ParseLuaResults(results, 0);
        }

        private Dictionary<string, object> ParseLuaResults(LuaValue[] results, int depth)
        {
            var dict = new Dictionary<string, object>();

            if (results != null && results.Length > 0 && results[0].type == LuaType.LUA_TTABLE)
            {
                ParseLuaTable(results[0], dict, depth);
            }

            return dict;
        }

        private void ParseLuaTable(LuaValue table, Dictionary<string, object> dict, int depth)
        {
            if (table.table == null || depth > MaxLuaDepth) return;

            for (int i = 0; i < table.table.Length; i += 2)
            {
                if (i + 1 < table.table.Length)
                {
                    var key = table.table[i];
                    var value = table.table[i + 1];

                    if (key.type == LuaType.LUA_TSTRING && !string.IsNullOrWhiteSpace(key.value))
                    {
                        dict[key.value!] = ConvertLuaValue(value, depth);
                    }
                }
            }
        }

        private object ConvertLuaValue(LuaValue value, int depth)
        {
            if (depth > MaxLuaDepth)
                return new Dictionary<string, object>();

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
                    ParseLuaTable(value, dict, depth + 1);
                    return dict;
                default:
                    return value.value ?? string.Empty;
            }
        }

        /// <summary>
        /// Gets all players for a specific place ID by aggregating data from all running game servers for that place
        /// </summary>
        public List<PlayerInfo> GetPlayersByPlaceId(int placeId)
        {
            lock (_serversLock)
            {
                var allPlayers = new List<PlayerInfo>();

                foreach (var server in _activeServers.Values)
                {
                    if (server.PlaceId == placeId)
                    {
                        allPlayers.AddRange(server.AuthenticatedPlayers);
                    }
                }

                return allPlayers;
            }
        }

        /// <summary>
        /// Gets player count summary for a specific place ID
        /// </summary>
        public (int AuthenticatedCount, int GuestCount, int TotalCount, List<PlayerInfo> Players) GetPlayerCountByPlaceId(int placeId)
        {
            lock (_serversLock)
            {
                int authenticatedCount = 0;
                int guestCount = 0;
                var players = new List<PlayerInfo>();

                foreach (var server in _activeServers.Values)
                {
                    if (server.PlaceId == placeId)
                    {
                        authenticatedCount += server.AuthenticatedPlayerCount;
                        guestCount += server.GuestPlayerCount;
                        players.AddRange(server.AuthenticatedPlayers);
                    }
                }

                return (authenticatedCount, guestCount, authenticatedCount + guestCount, players);
            }
        }

        /// <summary>
        /// Gets all game servers for a specific place ID
        /// </summary>
        public List<GameServerInfo> GetGameServersByPlaceId(int placeId)
        {
            lock (_serversLock)
            {
                return _activeServers.Values.Where(s => s.PlaceId == placeId).ToList();
            }
        }
    }
}
