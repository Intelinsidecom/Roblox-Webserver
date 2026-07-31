using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Extensions.Configuration;

namespace RCCArbiter
{
    /// <summary>
    /// Scales Rendering RCC processes based on concurrent requests and idle time.
    /// </summary>
    public class RenderingRccManager : IDisposable
    {
        private readonly IConfiguration _config;
        private readonly object _lock = new();
        private readonly List<Instance> _instances = new();
        private readonly int _maxPerInstance;
        private readonly TimeSpan _idleTimeout;
        private readonly Timer _sweeper;

        private class Instance
        {
            public RCCProcessManager Proc { get; set; } = default!;
            public int Load; // number of active requests
            public DateTime LastUsedUtc;
        }

        public RenderingRccManager(IConfiguration config)
        {
            _config = config;
            // Allow tuning via configuration, with sensible defaults
            if (!int.TryParse(_config["Rendering:MaxPerInstance"], out _maxPerInstance) || _maxPerInstance <= 0)
            {
                _maxPerInstance = 5; // threshold to scale
            }
            if (!int.TryParse(_config["Rendering:IdleTimeoutMinutes"], out var idleMinutes) || idleMinutes <= 0)
            {
                idleMinutes = 10;
            }
            _idleTimeout = TimeSpan.FromMinutes(idleMinutes);
            _sweeper = new Timer(SweepIdle, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        }

        public (string url, IDisposable lease)? AcquireIfTriggered(string routeOrFunction)
        {
            // Only activate on triggers defined in config
            bool isTrigger = IsTrigger(routeOrFunction);
            if (!isTrigger)
                return null;

            var (inst, url) = EnsureInstance();
            inst.Proc.IncrementBusy();
            Interlocked.Increment(ref inst.Load);
            inst.LastUsedUtc = DateTime.UtcNow;

            return (url, new Release(() => ReleaseInstance(inst)));
        }

        private bool IsTrigger(string routeOrFunction)
        {
            var routes = _config.GetSection("Rendering:Triggers:Routes").Get<string[]>() ?? Array.Empty<string>();
            var funcs = _config.GetSection("Rendering:Triggers:Functions").Get<string[]>() ?? Array.Empty<string>();
            if (routeOrFunction.StartsWith("/"))
                return Array.Exists(routes, r => string.Equals(r, routeOrFunction, StringComparison.OrdinalIgnoreCase));
            return Array.Exists(funcs, f => string.Equals(f, routeOrFunction, StringComparison.OrdinalIgnoreCase));
        }

        private (Instance inst, string url) EnsureInstance()
        {
            lock (_lock)
            {
                // Prune dead instances
                for (int idx = _instances.Count - 1; idx >= 0; idx--)
                {
                    if (!_instances[idx].Proc.IsRunning)
                    {
                        try { _instances[idx].Proc.Dispose(); } catch { }
                        _instances.RemoveAt(idx);
                    }
                }

                // Choose least loaded instance below threshold
                Instance? best = null;
                foreach (var i in _instances)
                {
                    if (best == null || i.Load < best.Load)
                        best = i;
                }

                if (best == null || best.Load >= _maxPerInstance)
                {
                    // Start a new instance on a free port
                    int port = FindFreePort();
                    if (port < 0)
                        throw new InvalidOperationException("No free ports available for RCC rendering instance");
                    var proc = new RCCProcessManager(_config, "Rendering", port);
                    proc.Start();
                    proc.EnableAutoRestart();
                    best = new Instance { Proc = proc, Load = 0, LastUsedUtc = DateTime.UtcNow };
                    _instances.Add(best);
                }

                return (best, best.Proc.ServiceUrl);
            }
        }

        private static int FindFreePort()
        {
            return PortManager.FindFreePort();
        }

        private void ReleaseInstance(Instance inst)
        {
            Interlocked.Decrement(ref inst.Load);
            inst.Proc.DecrementBusy();
            inst.LastUsedUtc = DateTime.UtcNow;
        }

        private void SweepIdle(object? state)
        {
            lock (_lock)
            {
                var now = DateTime.UtcNow;
                for (int idx = _instances.Count - 1; idx >= 0; idx--)
                {
                    var i = _instances[idx];
                    if (i.Load == 0 && now - i.LastUsedUtc > _idleTimeout)
                    {
                        try { i.Proc.Dispose(); } catch (Exception ex) { Console.WriteLine($"[ERROR] SweepIdle dispose RCC process: {ex}"); }
                        _instances.RemoveAt(idx);
                    }
                }
            }
        }

        public void Dispose()
        {
            _sweeper.Dispose();
            lock (_lock)
            {
                foreach (var i in _instances)
                {
                    try { i.Proc.Dispose(); } catch (Exception ex) { Console.WriteLine($"[ERROR] Dispose RCC process: {ex}"); }
                }
                _instances.Clear();
            }
        }

        private sealed class Release : IDisposable
        {
            private readonly Action _onDispose;
            private bool _done;
            public Release(Action onDispose) { _onDispose = onDispose; }
            public void Dispose()
            {
                if (_done) return; _done = true; _onDispose();
            }
        }
    }
}
