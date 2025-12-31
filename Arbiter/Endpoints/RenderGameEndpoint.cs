using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace RCCArbiter.Endpoints
{
    public class RenderGameEndpoint : ICompiledEndpoint
    {
        private IConfiguration? _configuration;
        public string Route => "/rendergame";
        public string ScriptName => "RenderGame";

        public void SetConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IDictionary<string, string> MapParameters(HttpRequest req)
        {
            var p = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var placeId = req.Query.TryGetValue("placeId", out var pid) ? pid.ToString() : "15";
            var x = req.Query.TryGetValue("x", out var xv) ? xv.ToString() : "1920";
            var y = req.Query.TryGetValue("y", out var yv) ? yv.ToString() : "1080";

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
            p["x"] = x;
            p["y"] = y;
            p["baseUrl"] = baseUrl;

            Console.WriteLine($"[RenderGame] Parameters: placeId={placeId}, x={x}, y={y}, baseUrl={baseUrl}");

            return p;
        }
    }
}
