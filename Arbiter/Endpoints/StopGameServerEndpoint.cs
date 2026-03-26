using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace RCCArbiter.Endpoints
{
    public class StopGameServerEndpoint : ICompiledEndpoint
    {
        private IConfiguration? _configuration;
        public string Route => "/gameserver/stop";
        public string ScriptName => "StopGameServer";

        public void SetConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IDictionary<string, string> MapParameters(HttpRequest req)
        {
            var p = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var gameId = req.Query.TryGetValue("gameId", out var gid) ? gid.ToString() : "";

            p["gameId"] = gameId;

            return p;
        }
    }
}
