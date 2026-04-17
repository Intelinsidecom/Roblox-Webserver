using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace RCCArbiter.Endpoints
{
    /// <summary>
    /// HTTP endpoint for game servers to report their status
    /// </summary>
    public class GameServerReportEndpoint : ICompiledEndpoint
    {
        private IConfiguration? _configuration;
        
        public string Route => "/gameserver/report";
        public string ScriptName => "CollectGameServerStatus";

        public void SetConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IDictionary<string, string> MapParameters(HttpRequest req)
        {
            var p = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var gameId = req.Query.TryGetValue("gameId", out var gid) ? gid.ToString() : "";
            var placeId = req.Query.TryGetValue("placeId", out var pid) ? pid.ToString() : "0";
            var reportType = req.Query.TryGetValue("type", out var rt) ? rt.ToString() : "full";
            var totalPlayers = req.Query.TryGetValue("totalPlayers", out var tp) ? tp.ToString() : "0";
            var serverUptime = req.Query.TryGetValue("uptime", out var u) ? u.ToString() : "0";
            var isShutdown = req.Query.TryGetValue("shutdown", out var sd) ? sd.ToString() : "false";
            var shutdownReason = req.Query.TryGetValue("shutdownReason", out var sr) ? sr.ToString() : "";

            p["gameId"] = gameId;
            p["placeId"] = placeId;
            p["reportType"] = reportType;
            p["totalPlayers"] = totalPlayers;
            p["serverUptime"] = serverUptime;
            p["isShutdown"] = isShutdown;
            p["shutdownReason"] = shutdownReason;

            return p;
        }
    }
}
