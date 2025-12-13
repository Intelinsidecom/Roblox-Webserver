using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace RCCArbiter.Endpoints
{
    /// <summary>
    /// Compiled endpoint that renders a thumbnail for a specific asset (e.g. Pants .rbxm or its image).
    ///
    /// Route: /renderavatarasset
    /// Script: RenderAvatarAsset.lua (looked up via FileScriptProvider)
    ///
    /// The Lua script is expected to honor the following parameters:
    ///   - assetId  : ID of the asset to render
    ///   - x, y     : output size in pixels
    ///   - baseUrl  : base URL for web callbacks (defaults from config or request)
    ///   - uploadUrl: where the renderer should upload the generated thumbnail
    ///   - accessKey: optional auth key for upload target
    ///
    /// The script should return a JSON-compatible table which includes at least
    ///   { thumbnailUrl = "https://..." }
    /// so callers such as PantsAssetService can persist it.
    /// </summary>
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

            // Required: which asset to render
            var assetId = req.Query.TryGetValue("assetId", out var aid)
                ? aid.ToString()
                : string.Empty;

            // Reasonable defaults for an item thumbnail; callers can override via query
            var defaultX = "420";
            var defaultY = "420";

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

            // Access key is fully optional; default to empty unless explicitly provided.
            var accessKey = req.Query.TryGetValue("accessKey", out var ak) && !string.IsNullOrWhiteSpace(ak)
                ? ak.ToString()
                : string.Empty;

            // Tokens used by the RenderAvatarAsset.lua template
            p["assetId"] = assetId;
            p["x"] = x;
            p["y"] = y;
            p["baseUrl"] = baseUrl;
            p["uploadUrl"] = uploadUrl;
            p["accessKey"] = accessKey;

            Console.WriteLine($"[RenderAvatarAsset] assetId={assetId}, x={x}, y={y}, uploadUrl={(string.IsNullOrEmpty(uploadUrl) ? "<empty>" : "set")}");

            return p;
        }
    }
}
