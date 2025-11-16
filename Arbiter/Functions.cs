using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RCCArbiter.Scripting;

namespace RCCArbiter
{
    public record ArbiterFunction(string Name, string ScriptName, string[] ParamKeys);

    public static class Functions
    {
        // Register arbiter functions here for clean, easy maintenance
        public static readonly Dictionary<string, ArbiterFunction> Registry =
            new(StringComparer.OrdinalIgnoreCase)
            {
                // Example: /userinfo maps to Scripts/UserInfo.lua and accepts userId, username (optional)
                ["userinfo"] = new ArbiterFunction(
                    Name: "userinfo",
                    ScriptName: "UserInfo",
                    ParamKeys: new [] { "userId", "username" } // order used for pipe-separated mapping
                )
            };

        // Extract parameters from query, optional pipe-separated list, and JSON body (last one wins)
        public static async Task<Dictionary<string, string>> ExtractParametersAsync(HttpRequest req, ArbiterFunction fn)
        {
            var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // 1) All query key/value pairs (?userId=123)
            foreach (var kv in req.Query)
            {
                if (!string.Equals(kv.Key, "params", StringComparison.OrdinalIgnoreCase))
                {
                    parameters[kv.Key] = kv.Value.ToString();
                }
            }

            // 2) Pipe separated in ?params=val1|val2|...
            if (req.Query.TryGetValue("params", out var pipeVals) && pipeVals.Count > 0)
            {
                var raw = pipeVals[0] ?? string.Empty;
                var pieces = raw.Split('|', StringSplitOptions.None);
                for (int i = 0; i < pieces.Length && i < fn.ParamKeys.Length; i++)
                {
                    parameters[fn.ParamKeys[i]] = pieces[i] ?? string.Empty;
                }
            }

            // 3) JSON body overrides (if present)
            if (req.Body != null && req.ContentLength.GetValueOrDefault() != 0)
            {
                try
                {
                    using var doc = await JsonDocument.ParseAsync(req.Body);
                    if (doc.RootElement.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var prop in doc.RootElement.EnumerateObject())
                        {
                            parameters[prop.Name] = prop.Value.ToString() ?? string.Empty;
                        }
                    }
                }
                catch
                {
                    // Ignore malformed JSON; rely on query/pipe values
                }
            }

            return parameters;
        }

        public static LuaValue[] Run(
            string rccUrl,
            FileScriptProvider provider,
            ScriptRenderer renderer,
            ArbiterFunction fn,
            IDictionary<string, string> parameters)
        {
            using var client = new RCCClient(rccUrl);
            if (!provider.TryGetScript(fn.ScriptName, out var template))
                throw new InvalidOperationException($"Script '{fn.ScriptName}.lua' not found");

            var runner = new ScriptRunner(client, renderer);
            return runner.RunScript(fn.ScriptName, template, new Dictionary<string, string>(parameters, StringComparer.OrdinalIgnoreCase));
        }
    }
}
