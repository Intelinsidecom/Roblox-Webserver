using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RCCArbiter.Scripting
{
    public class FileScriptProvider : IScriptProvider
    {
        private readonly string _scriptsRoot;
        private readonly string? _year;

        public FileScriptProvider(string scriptsRoot, string? year = null)
        {
            _scriptsRoot = scriptsRoot;
            _year = year;
        }

        public IEnumerable<string> ListScripts()
        {
            var dirs = GetSearchDirectories();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var dir in dirs)
            {
                if (!Directory.Exists(dir))
                    continue;
                foreach (var f in Directory.EnumerateFiles(dir, "*.lua", SearchOption.TopDirectoryOnly)
                    .Select(Path.GetFileNameWithoutExtension))
                {
                    if (seen.Add(f))
                        yield return f;
                }
            }
        }

        public bool TryGetScript(string name, out string scriptContent)
        {
            scriptContent = string.Empty;
            if (string.IsNullOrWhiteSpace(name))
                return false;

            foreach (var dir in GetSearchDirectories())
            {
                var path = Path.Combine(dir, name + ".lua");
                if (File.Exists(path))
                {
                    scriptContent = File.ReadAllText(path);
                    return true;
                }
            }
            return false;
        }

        private IEnumerable<string> GetSearchDirectories()
        {
            // Year-specific directory first, then root as fallback
            if (!string.IsNullOrWhiteSpace(_year))
            {
                var yearDir = Path.Combine(_scriptsRoot, _year);
                yield return yearDir;
            }
            yield return _scriptsRoot;
        }
    }
}