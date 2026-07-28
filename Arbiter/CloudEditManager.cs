using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RCCArbiter.Scripting;

namespace RCCArbiter
{
    public class CloudEditManager : IDisposable
    {
        private readonly string _rccUrl;
        private readonly IConfiguration _configuration;
        private readonly CloudEditRccManager _rccManager;
        private readonly ConcurrentDictionary<string, CloudEditSession> _activeSessions;
        private readonly Timer _healthTimer;

        public class CloudEditSession
        {
            public string GameId { get; set; } = string.Empty;
            public int PlaceId { get; set; }
            public int UniverseId { get; set; }
            public int Port { get; set; }
            public int MaxPlayers { get; set; }
            public string BaseUrl { get; set; } = string.Empty;
            public DateTime StartTime { get; set; }
            public string Status { get; set; } = "starting";
            public Dictionary<string, object> LastStatus { get; set; } = new();
            public DateTime LastActivityTime { get; set; }
            public List<string> Editors { get; set; } = new();
        }

        public CloudEditManager(string rccUrl, IConfiguration configuration, CloudEditRccManager rccManager)
        {
            _rccUrl = rccUrl;
            _configuration = configuration;
            _rccManager = rccManager;
            _activeSessions = new();
            _healthTimer = new Timer(CheckHealth, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
        }

        public async Task<string> StartCloudEditServerAsync(int placeId, int port, int maxPlayers, string baseUrl, int universeId = 0)
        {
            var gameId = Guid.NewGuid().ToString();

            Console.WriteLine($"[CloudEdit] Starting cloud edit server for place {placeId}, gameId={gameId}");

            if (string.IsNullOrWhiteSpace(baseUrl))
                baseUrl = "http://www.freblx.xyz";

            var rccReservation = _rccManager.ReserveForCloudEdit(gameId);
            if (rccReservation == null)
            {
                throw new InvalidOperationException("Failed to reserve RCC instance for cloud edit server");
            }

            var dedicatedRccUrl = rccReservation.Value.url;
            var job = new Job
            {
                id = gameId,
                expirationInSeconds = 31536000,
                category = 2,
                cores = 2
            };

            var accessKey = _configuration["Arbiter:AccessKey"] ?? "ChangeMe";
            var parameters = new Dictionary<string, string>
            {
                ["placeId"] = placeId.ToString(),
                ["port"] = port.ToString(),
                ["maxPlayers"] = maxPlayers.ToString(),
                ["baseUrl"] = baseUrl,
                ["gameId"] = gameId,
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
                        Console.WriteLine($"[CloudEdit] Testing RCC connection... Attempt {attempt}/{maxRetries}");
                        var version = client.GetVersion();
                        Console.WriteLine($"[CloudEdit] RCC connection successful, version: {version}");
                        break;
                    }
                    catch (Exception testEx)
                    {
                        Console.WriteLine($"[CloudEdit] RCC connection test failed (attempt {attempt}/{maxRetries}): {testEx.Message}");

                        if (attempt == maxRetries)
                        {
                            throw new InvalidOperationException($"Cannot connect to RCC at {dedicatedRccUrl} after {maxRetries} attempts: {testEx.Message}", testEx);
                        }

                        Console.WriteLine($"[CloudEdit] Waiting {retryDelayMs}ms before retry...");
                        await Task.Delay(retryDelayMs);
                    }
                }

                var scriptRenderer = new ScriptRenderer();
                var scriptsRoot = Path.Combine(Directory.GetCurrentDirectory(), "Scripts");
                var provider = new FileScriptProvider(scriptsRoot);

                if (!provider.TryGetScript("StartCloudEditServer", out var scriptTemplate))
                {
                    throw new FileNotFoundException("Script 'StartCloudEditServer.lua' not found");
                }

                var renderedScript = scriptRenderer.Render(scriptTemplate, parameters);

                var script = new ScriptExecution
                {
                    name = "StartCloudEditServer",
                    script = renderedScript,
                    arguments = null
                };

                var results = client.OpenJobEx(job, script);

                _activeSessions[gameId] = new CloudEditSession
                {
                    GameId = gameId,
                    PlaceId = placeId,
                    UniverseId = universeId,
                    Port = port,
                    MaxPlayers = maxPlayers,
                    BaseUrl = baseUrl,
                    StartTime = DateTime.UtcNow,
                    Status = "running",
                    LastActivityTime = DateTime.UtcNow
                };

                return gameId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CloudEdit] Error starting cloud edit server: {ex.GetType().Name}: {ex.Message}");
                _rccManager.ReleaseCloudEdit(gameId);
                throw new InvalidOperationException($"Failed to start cloud edit server: {ex.Message}", ex);
            }
        }

        public CloudEditSession? GetSession(string gameId)
        {
            _activeSessions.TryGetValue(gameId, out var session);
            return session;
        }

        /// <summary>
        /// Get all cloud edit sessions for a specific universe
        /// </summary>
        public List<CloudEditSession> GetSessionsByUniverseId(int universeId)
        {
            var results = new List<CloudEditSession>();
            foreach (var kvp in _activeSessions)
            {
                if (kvp.Value.UniverseId == universeId && kvp.Value.Status == "running")
                {
                    results.Add(kvp.Value);
                }
            }
            return results;
        }

        /// <summary>
        /// Update the list of editors for a cloud edit session
        /// </summary>
        public void UpdateEditors(string gameId, List<string> editors)
        {
            if (_activeSessions.TryGetValue(gameId, out var session))
            {
                session.Editors = editors;
                session.LastActivityTime = DateTime.UtcNow;
            }
        }

        public void StopSession(string gameId)
        {
            if (_activeSessions.TryRemove(gameId, out _))
            {
                _rccManager.ReleaseCloudEdit(gameId);
            }
        }

        private void CheckHealth(object? state)
        {
            foreach (var kvp in _activeSessions)
            {
                if (kvp.Value.Status != "running") continue;

                try
                {
                    if (!_rccManager.IsCloudEditAlive(kvp.Key))
                    {
                        Console.WriteLine($"[CloudEdit] RCC for session {kvp.Key} (place {kvp.Value.PlaceId}) is dead, cleaning up");
                        _activeSessions.TryRemove(kvp.Key, out _);
                        try { _rccManager.ReleaseCloudEdit(kvp.Key); } catch { }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CloudEdit] Health check error for {kvp.Key}: {ex.Message}");
                }
            }
        }

        public void Dispose()
        {
            _healthTimer?.Dispose();
            foreach (var kvp in _activeSessions)
            {
                try
                {
                    _rccManager.ReleaseCloudEdit(kvp.Key);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CloudEdit] Error disposing session {kvp.Key}: {ex.Message}");
                }
            }
            _activeSessions.Clear();
            _rccManager?.Dispose();
        }
    }
}
