using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Configuration;

namespace RCCArbiter
{
    /// <summary>
    /// Manages dedicated RCC instances for cloud edit sessions using the CloudEdit config section.
    /// </summary>
    public class CloudEditRccManager : IDisposable
    {
        private readonly IConfiguration _config;
        private readonly object _lock = new();
        private readonly Dictionary<string, Instance> _reservedInstances = new();
        private readonly Timer _cleanupTimer;

        private class Instance
        {
            public RCCProcessManager Proc { get; set; } = default!;
            public string GameId { get; set; } = string.Empty;
            public DateTime CreatedUtc;
            public DateTime LastUsedUtc;
        }

        public CloudEditRccManager(IConfiguration config)
        {
            _config = config;
            _cleanupTimer = new Timer(_ => Program.RunGuarded("CloudEditRccManager.CleanupExpired", () => CleanupExpired(null)), null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
        }

        public (string url, IDisposable lease)? ReserveForCloudEdit(string gameId)
        {
            if (string.IsNullOrWhiteSpace(gameId))
                return null;

            lock (_lock)
            {
                if (_reservedInstances.ContainsKey(gameId))
                {
                    var existing = _reservedInstances[gameId];
                    existing.LastUsedUtc = DateTime.UtcNow;
                    return (existing.Proc.ServiceUrl, new Release(() => ReleaseCloudEdit(gameId)));
                }

                try
                {
                    int port = PortManager.FindFreePort();
                    Console.WriteLine($"[CloudEdit] Creating RCC instance for cloud edit {gameId} on port {port}");

                    var proc = new RCCProcessManager(_config, "CloudEdit", port);
                    proc.Start();

                    if (!proc.IsRunning)
                    {
                        throw new InvalidOperationException($"CloudEdit RCC process failed to start on port {port}");
                    }

                    var instance = new Instance
                    {
                        Proc = proc,
                        GameId = gameId,
                        CreatedUtc = DateTime.UtcNow,
                        LastUsedUtc = DateTime.UtcNow
                    };

                    _reservedInstances[gameId] = instance;

                    return (proc.ServiceUrl, new Release(() => ReleaseCloudEdit(gameId)));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to reserve CloudEdit RCC for {gameId}: {ex.Message}");
                    throw;
                }
            }
        }

        public string? GetCloudEditUrl(string gameId)
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

        public bool IsCloudEditAlive(string gameId)
        {
            lock (_lock)
            {
                if (_reservedInstances.TryGetValue(gameId, out var instance))
                {
                    return instance.Proc.IsRunning;
                }
                return false;
            }
        }

        public void ReleaseCloudEdit(string gameId)
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
                        Console.WriteLine($"Error disposing CloudEdit RCC for {gameId}: {ex.Message}");
                    }
                    finally
                    {
                        _reservedInstances.Remove(gameId);
                    }
                }
                else
                {
                    Console.WriteLine($"No CloudEdit RCC instance found to release for {gameId}");
                }
            }
        }

        private void CleanupExpired(object? state)
        {
            lock (_lock)
            {
                var now = DateTime.UtcNow;
                var toRemove = new List<string>();

                foreach (var kvp in _reservedInstances)
                {
                    if (now - kvp.Value.CreatedUtc > TimeSpan.FromHours(2))
                    {
                        toRemove.Add(kvp.Key);
                    }
                }

                foreach (var gameId in toRemove)
                {
                    Console.WriteLine($"[CloudEdit] Auto-cleanup expired RCC instance for {gameId}");
                    ReleaseCloudEdit(gameId);
                }
            }
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
                        Console.WriteLine($"Error disposing CloudEdit RCC for {kvp.Key}: {ex.Message}");
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
