using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RCCArbiter.Scripting
{
    public class FileScriptProvider : IScriptProvider
    {
        private readonly string _scriptsRoot;

        public FileScriptProvider(string scriptsRoot)
        {
            _scriptsRoot = scriptsRoot;
        }

        public IEnumerable<string> ListScripts()
        {
            if (!Directory.Exists(_scriptsRoot))
                return Enumerable.Empty<string>();

            return Directory
                .EnumerateFiles(_scriptsRoot, "*.lua", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileNameWithoutExtension);
        }

        public bool TryGetScript(string name, out string scriptContent)
        {
            scriptContent = string.Empty;
            if (string.IsNullOrWhiteSpace(name))
                return false;

            var path = Path.Combine(_scriptsRoot, name + ".lua");
            if (!File.Exists(path))
                return false;

            scriptContent = File.ReadAllText(path);
            return true;
        }
    }
}
