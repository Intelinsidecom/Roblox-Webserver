using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace RCCArbiter.Endpoints
{
    public class GameServerByPlaceEndpoint : ICompiledEndpoint
    {
        private IConfiguration? _configuration;
        public string Route => "/gameserver/by-place";
        public string ScriptName => "GetGameServersByPlace";

        public void SetConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IDictionary<string, string> MapParameters(HttpRequest req)
        {
            var p = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            string placeId = "15";
            if (req.RouteValues.TryGetValue("placeId", out var routeValue))
            {
                placeId = routeValue?.ToString() ?? "15";
            }
            else if (req.Query.TryGetValue("placeId", out var queryValue))
            {
                placeId = queryValue.ToString();
            }

            p["placeId"] = placeId;

            return p;
        }
    }
}
