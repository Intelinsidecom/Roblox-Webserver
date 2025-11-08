using System;
using System.Collections.Generic;

namespace RCCArbiter.Scripting
{
    public class ScriptRunner
    {
        private readonly RCCClient _client;
        private readonly ScriptRenderer _renderer;

        public ScriptRunner(RCCClient client, ScriptRenderer renderer)
        {
            _client = client;
            _renderer = renderer;
        }

        public LuaValue[] RunScript(string name, string scriptTemplate, IDictionary<string, string>? parameters = null)
        {
            var job = new Job
            {
                id = Guid.NewGuid().ToString(),
                expirationInSeconds = 60,
                category = 0,
                cores = 1
            };

            // Ensure parameters dictionary exists and include jobId for template usage
            var renderParams = parameters != null
                ? new Dictionary<string, string>(parameters, StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            renderParams["jobId"] = job.id;

            var rendered = _renderer.Render(scriptTemplate, renderParams);

            var scriptExecution = new ScriptExecution
            {
                name = name,
                script = rendered,
                arguments = null
            };

            return _client.BatchJobEx(job, scriptExecution);
        }
    }
}
