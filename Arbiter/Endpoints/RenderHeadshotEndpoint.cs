using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace RCCArbiter.Endpoints
{
    public class RenderHeadshotEndpoint : ICompiledEndpoint
    {
        private IConfiguration? _configuration;
        public string Route => "/renderheadshot";
        public string ScriptName => "Headshot";

        public void SetConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IDictionary<string, string> MapParameters(HttpRequest req)
        {
            var p = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // Map required/optional inputs (same contract as /renderavatar, but dedicated route)
            var type = "headshot"; // Force headshot semantics for this endpoint
            var userId = req.Query.TryGetValue("userId", out var uid) ? uid.ToString() :
                        (req.Query.TryGetValue("playerId", out var pid) ? pid.ToString() : "1");

            // Default size suitable for headshots; caller may override via x/y
            var defaultX = "1024";
            var defaultY = "1024";

            var x = req.Query.TryGetValue("x", out var xv) ? xv.ToString() : defaultX;
            var y = req.Query.TryGetValue("y", out var yv) ? yv.ToString() : defaultY;

            // baseUrl preference: appsettings -> query -> inferred from request
            string? configuredBase = _configuration?["Arbiter:BaseUrl"];
            var host = req.Host.HasValue ? req.Host.Value : "localhost";
            var scheme = string.IsNullOrEmpty(req.Scheme) ? "http" : req.Scheme;
            string inferred = $"{scheme}://{host}";
            var baseUrl = !string.IsNullOrWhiteSpace(configuredBase)
                ? configuredBase!
                : (req.Query.TryGetValue("baseUrl", out var bu) && !string.IsNullOrWhiteSpace(bu)
                    ? bu.ToString()
                    : inferred);

            // Upload target preference: query -> appsettings
            var uploadUrl = req.Query.TryGetValue("uploadUrl", out var uu) && !string.IsNullOrWhiteSpace(uu)
                ? uu.ToString()
                : _configuration?["Arbiter:UploadUrl"] ?? string.Empty;

            var accessKey = req.Query.TryGetValue("accessKey", out var ak) && !string.IsNullOrWhiteSpace(ak)
                ? ak.ToString()
                : _configuration?["Arbiter:AccessKey"] ?? string.Empty;

            // Parameter names correspond to tokens used in the Lua template (%token%)
            p["type"] = type;
            p["userId"] = userId;
            p["x"] = x;
            p["y"] = y;
            p["baseUrl"] = baseUrl;
            p["uploadUrl"] = uploadUrl;
            p["accessKey"] = accessKey;

            Console.WriteLine($"[RenderHeadshot] Parameters: type={type}, userId={userId}, x={x}, y={y}, uploadUrl={(string.IsNullOrEmpty(uploadUrl) ? "<empty>" : "set")}");

            return p;
        }
    }
}
