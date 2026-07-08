using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace RCCArbiter.Endpoints
{
    public sealed class RenderMeshEndpoint : ICompiledEndpoint
    {
        private IConfiguration? _configuration;

        public string Route => "/rendermesh";
        public string ScriptName => "Mesh";

        public void SetConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IDictionary<string, string> MapParameters(HttpRequest req)
        {
            var p = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var assetId = req.Query.TryGetValue("assetId", out var aid)
                ? aid.ToString()
                : string.Empty;

            var defaultX = "420";
            var defaultY = "420";

            var x = req.Query.TryGetValue("x", out var xv) ? xv.ToString() : defaultX;
            var y = req.Query.TryGetValue("y", out var yv) ? yv.ToString() : defaultY;

            string? configuredBase = _configuration?["Arbiter:BaseUrl"];
            var host = req.Host.HasValue ? req.Host.Value : "localhost";
            var scheme = string.IsNullOrEmpty(req.Scheme) ? "http" : req.Scheme;
            string inferred = $"{scheme}://{host}";
            var baseUrl = !string.IsNullOrWhiteSpace(configuredBase)
                ? configuredBase!
                : (req.Query.TryGetValue("baseUrl", out var bu) && !string.IsNullOrWhiteSpace(bu)
                    ? bu.ToString()
                    : inferred);

            var assetUrl = string.IsNullOrWhiteSpace(assetId)
                ? string.Empty
                : $"{baseUrl.TrimEnd('/', '\\')}/asset/?id={assetId}";

            p["assetId"] = assetId;
            p["assetUrl"] = assetUrl;
            p["x"] = x;
            p["y"] = y;
            p["baseUrl"] = baseUrl;

            Console.WriteLine($"[RenderMesh] assetId={assetId}, x={x}, y={y}, assetUrl={assetUrl}");

            return p;
        }
    }
}
