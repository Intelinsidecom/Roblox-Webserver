using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace RCCArbiter.Endpoints
{
    public sealed class RenderAvatarAssetEndpoint : ICompiledEndpoint
    {
        private IConfiguration? _configuration;

        public string Route => "/renderavatarasset";
        public string ScriptName => "RenderAvatarAsset";

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

            var uploadUrl = req.Query.TryGetValue("uploadUrl", out var uu) && !string.IsNullOrWhiteSpace(uu)
                ? uu.ToString()
                : _configuration?["Arbiter:UploadUrl"] ?? string.Empty;

            var accessKey = req.Query.TryGetValue("accessKey", out var ak) && !string.IsNullOrWhiteSpace(ak)
                ? ak.ToString()
                : string.Empty;

            var fileExtension = req.Query.TryGetValue("fileExtension", out var fe) && !string.IsNullOrWhiteSpace(fe)
                ? fe.ToString()
                : "PNG";

            p["assetId"] = assetId;
            p["fileExtension"] = fileExtension;
            p["x"] = x;
            p["y"] = y;
            p["baseUrl"] = baseUrl;
            p["uploadUrl"] = uploadUrl;
            p["accessKey"] = accessKey;

            return p;
        }
    }
}
