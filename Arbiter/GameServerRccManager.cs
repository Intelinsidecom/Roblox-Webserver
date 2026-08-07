using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Extensions.Configuration;

namespace RCCArbiter
{
    /// <summary>
    /// Manages dedicated RCC instances for game servers with proper reservation and cleanup
    /// </summary>
    public class GameServerRccManager : IDisposable
    {
        private readonly IConfiguration _config;
        private readonly object _lock = new();
        private readonly Dictionary<string, Instance> _reservedInstances = new(); // gameId -> Instance
        private readonly Timer _cleanupTimer;

        private class Instance
        {
            public RCCProcessManager Proc { get; set; } = default!;
            public string GameId { get; set; } = string.Empty;
            public DateTime CreatedUtc;
            public DateTime LastUsedUtc;
        }

        public GameServerRccManager(IConfiguration config)
        {
            _config = config;
            _cleanupTimer = new Timer(_ => Program.RunGuarded("GameServerRccManager.CleanupExpired", () => CleanupExpired(null)), null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
        }

        /// <summary>
        /// Reserve a dedicated RCC instance for a game server
        /// </summary>
        public (string url, IDisposable lease)? ReserveForGameServer(string gameId)
        {
            if (string.IsNullOrWhiteSpace(gameId))
                return null;

            lock (_lock)
            {
                if (_reservedInstances.ContainsKey(gameId))
                {
                    var existing = _reservedInstances[gameId];
                    existing.LastUsedUtc = DateTime.UtcNow;
                    return (existing.Proc.ServiceUrl, new Release(() => ReleaseGameServer(gameId)));
                }

                try
                {
                    int port = FindFreePort();
                    Console.WriteLine($"Creating RCC instance for game {gameId} on port {port}");
                    
                    var proc = new RCCProcessManager(_config, "RCCService", port);
                    
                    
                    proc.Start();

                    if (!proc.IsRunning)
                    {
                        throw new InvalidOperationException($"RCC process failed to start on port {port}");
                    }

                    var instance = new Instance 
                    { 
                        Proc = proc, 
                        GameId = gameId,
                        CreatedUtc = DateTime.UtcNow,
                        LastUsedUtc = DateTime.UtcNow
                    };

                    _reservedInstances[gameId] = instance;

                    return (proc.ServiceUrl, new Release(() => ReleaseGameServer(gameId)));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to reserve RCC for game {gameId}: {ex.Message}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Get the RCC URL for a specific game server
        /// </summary>
        public string? GetGameServerUrl(string gameId)
        {
            lock (_lock)
            {
                if (_reservedInstances.TryGetValue(gameId, out var instance))
                {
                    instance.LastUsedUtc = DateTime.UtcNow;
                    return instance.Proc.ServiceUrl;
                }
                return null;
            }
        }

        /// <summary>
        /// Release and kill the RCC instance for a game server
        /// </summary>
        public void ReleaseGameServer(string gameId)
        {
            lock (_lock)
            {
                if (_reservedInstances.TryGetValue(gameId, out var instance))
                {
                    try
                    {
                        instance.Proc.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error disposing RCC for game {gameId}: {ex.Message}");
                    }
                    finally
                    {
                        _reservedInstances.Remove(gameId);
                    }
                }
                else
                {
                    Console.WriteLine($"No RCC instance found to release for game {gameId}");
                }
            }
        }

        /// <summary>
        /// Cleanup expired RCC instances (failsafe)
        /// </summary>
        private void CleanupExpired(object? state)
        {
            lock (_lock)
            {
                var now = DateTime.UtcNow;
                var toRemove = new List<string>();

                foreach (var kvp in _reservedInstances)
                {
                    var instance = kvp.Value;

                    if (now - instance.CreatedUtc > TimeSpan.FromHours(2))
                    {
                        toRemove.Add(kvp.Key);
                    }
                }

                foreach (var gameId in toRemove)
                {
                    Console.WriteLine($"Auto-cleanup expired RCC instance for game {gameId}");
                    ReleaseGameServer(gameId);
                }
            }
        }

        private static int FindFreePort()
        {
            return PortManager.FindFreePort();
        }

        public void Dispose()
        {
            _cleanupTimer.Dispose();
            lock (_lock)
            {
                foreach (var kvp in _reservedInstances)
                {
                    try 
                    { 
                        kvp.Value.Proc.Dispose(); 
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error disposing RCC for game {kvp.Key}: {ex.Message}");
                    }
                }
                _reservedInstances.Clear();
            }
        }

        private sealed class Release : IDisposable
        {
            private readonly Action _onDispose;
            private bool _done;
            public Release(Action onDispose) { _onDispose = onDispose; }
            public void Dispose()
            {
                if (_done) return; 
                _done = true; 
                _onDispose();
            }
        }
    }
}
