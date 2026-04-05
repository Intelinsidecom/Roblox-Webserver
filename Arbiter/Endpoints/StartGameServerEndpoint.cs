using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace RCCArbiter.Endpoints
{
    public class StartGameServerEndpoint : ICompiledEndpoint
    {
        private IConfiguration? _configuration;
        public static readonly HashSet<int> _allocatedPorts = new();
        public static readonly object _portLock = new object();
        private const int MinPort = 54000;
        private const int MaxPort = 55000;
        
        public string Route => "/gameserver/start";
        public string ScriptName => "StartGameServer";

        public void SetConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Generates an available port for game server
        /// </summary>
        public static int GenerateAvailablePort()
        {
            lock (_portLock)
            {
                var random = new Random();
                var availablePorts = Enumerable.Range(MinPort, MaxPort - MinPort + 1)
                    .Where(port => !_allocatedPorts.Contains(port) && IsPortAvailable(port))
                    .ToList();

                if (availablePorts.Any())
                {
                    var selectedPort = availablePorts[random.Next(availablePorts.Count)];
                    _allocatedPorts.Add(selectedPort);
                    return selectedPort;
                }

                throw new InvalidOperationException("No available ports found for game server allocation");
            }
        }

        /// <summary>
        /// Checks if a specific port is available (not in use)
        /// </summary>
        private static bool IsPortAvailable(int port)
        {
            try
            {
                using var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Any, port);
                listener.Start();
                listener.Stop();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public IDictionary<string, string> MapParameters(HttpRequest req)
        {
            var p = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var placeId = req.Query.TryGetValue("placeId", out var pid) ? pid.ToString() : "15";
            
            int port;
            if (req.Query.TryGetValue("port", out var pv) && int.TryParse(pv, out var providedPort))
            {
                if (providedPort >= MinPort && providedPort <= MaxPort && !_allocatedPorts.Contains(providedPort))
                {
                    lock (_portLock)
                    {
                        _allocatedPorts.Add(providedPort);
                    }
                    port = providedPort;
                }
                else
                {
                    port = GenerateAvailablePort();
                }
            }
            else
            {
                port = GenerateAvailablePort();
            }
            var maxPlayers = req.Query.TryGetValue("maxPlayers", out var mpv) ? mpv.ToString() : "10";
            var privateServerId = req.Query.TryGetValue("privateServerId", out var psv) ? psv.ToString() : "";
            var maxInactive = req.Query.TryGetValue("maxInactive", out var mi) ? mi.ToString() : "0"; // 0 = never auto-kill

            string? configuredBase = _configuration?["Arbiter:BaseUrl"];
            var host = req.Host.HasValue ? req.Host.Value : "localhost";
            var scheme = string.IsNullOrEmpty(req.Scheme) ? "http" : req.Scheme;
            string inferred = $"{scheme}://{host}";
            var baseUrl = !string.IsNullOrWhiteSpace(configuredBase)
                ? configuredBase!
                : (req.Query.TryGetValue("baseUrl", out var bu) && !string.IsNullOrWhiteSpace(bu)
                    ? bu.ToString()
                    : inferred);

            p["placeId"] = placeId;
            p["port"] = port.ToString();
            p["maxPlayers"] = maxPlayers;
            p["privateServerId"] = privateServerId;
            p["baseUrl"] = baseUrl;
            p["maxInactive"] = maxInactive;

            return p;
        }
    }
}
