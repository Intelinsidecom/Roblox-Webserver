using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace RCCArbiter.Endpoints
{
    public class StartGameServerEndpoint : ICompiledEndpoint
    {
        private IConfiguration? _configuration;

        public string Route => "/gameserver/start";
        public string ScriptName => "StartGameServer";

        public void SetConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static int GenerateAvailablePort()
        {
            return PortManager.GenerateGameServerPort();
        }

        public IDictionary<string, string> MapParameters(HttpRequest req)
        {
            var p = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var placeId = req.Query.TryGetValue("placeId", out var pid) ? pid.ToString() : "15";

            int port;
            if (req.Query.TryGetValue("port", out var pv) && int.TryParse(pv, out var providedPort))
            {
                if (providedPort >= PortManager.GameServerMinPort && providedPort <= PortManager.GameServerMaxPort && PortManager.ReservePort(providedPort))
                {
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
