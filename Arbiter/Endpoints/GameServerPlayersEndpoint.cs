using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace RCCArbiter.Endpoints
{
    /// <summary>
    /// Endpoint to query player information for a specific place
    /// Returns player count and list of authenticated players for all running game servers of the specified place
    /// </summary>
    public class GameServerPlayersEndpoint : ICompiledEndpoint
    {
        private IConfiguration? _configuration;
        public string Route => "/gameserver/players";
        public string ScriptName => "GetGameServerPlayers";

        public void SetConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IDictionary<string, string> MapParameters(HttpRequest req)
        {
            var p = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var placeId = req.Query.TryGetValue("placeId", out var pid) ? pid.ToString() : "";
            var gameId = req.Query.TryGetValue("gameId", out var gid) ? gid.ToString() : "";

            p["placeId"] = placeId;
            p["gameId"] = gameId;

            return p;
        }
    }
}
