using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RCCArbiter.Scripting
{
    public class ScriptRenderer
    {
        private static readonly Regex TokenRegexDoubleBraces = new Regex(@"\{\{\s*([a-zA-Z0-9_]+)\s*\}\}", RegexOptions.Compiled);
        private static readonly Regex TokenRegexPercent = new Regex(@"%\s*([a-zA-Z0-9_]+)\s*%", RegexOptions.Compiled);

        public string Render(string template, IDictionary<string, string>? parameters)
        {
            if (string.IsNullOrEmpty(template) || parameters == null || parameters.Count == 0)
                return template;

            string ReplaceMatch(Match m)
            {
                var key = m.Groups[1].Value;
                if (!parameters.TryGetValue(key, out var raw))
                    return m.Value; // leave as-is if missing

                if (double.TryParse(raw, out _))
                    return raw;

                var escaped = (raw ?? string.Empty)
                    .Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("\n", "\\n")
                    .Replace("\r", "\\r");
                return $"\"{escaped}\"";
            }

            // Apply both syntaxes: {{token}} and %token%
            var withDouble = TokenRegexDoubleBraces.Replace(template, ReplaceMatch);
            var withPercent = TokenRegexPercent.Replace(withDouble, ReplaceMatch);
            return withPercent;
        }
    }
}
