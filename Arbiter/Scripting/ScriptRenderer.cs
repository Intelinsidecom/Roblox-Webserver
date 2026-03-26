using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RCCArbiter.Scripting
{
    public class ScriptRenderer
    {
        private static readonly Regex TokenRegexDoubleBraces = new Regex(@"\{\{\s*([a-zA-Z0-9_]+)\s*\}\}", RegexOptions.Compiled);
        private static readonly Regex TokenRegexPercent = new Regex(@"%([a-zA-Z0-9_]+)%", RegexOptions.Compiled);

        public string Render(string template, IDictionary<string, string>? parameters)
        {
            if (string.IsNullOrEmpty(template) || parameters == null || parameters.Count == 0)
                return template;


            string ReplaceMatch(Match m, string currentString)
            {
                var key = m.Groups[1].Value;
                if (!parameters.TryGetValue(key, out var raw))
                {
                    return m.Value; 
                }

                if (double.TryParse(raw, out _))
                {
                    return raw;
                }

                var escaped = (raw ?? string.Empty)
                    .Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("\n", "\\n")
                    .Replace("\r", "\\r");
                
                var precedingChar = m.Index > 0 ? currentString[m.Index - 1] : '\0';
                var followingChar = m.Index + m.Length < currentString.Length ? currentString[m.Index + m.Length] : '\0';
                
                if (precedingChar == '"' && followingChar == '"')
                {
                    return escaped;
                }
                
                return $"\"{escaped}\"";
            }

            var withDouble = TokenRegexDoubleBraces.Replace(template, m => ReplaceMatch(m, template));
            var withPercent = TokenRegexPercent.Replace(withDouble, m => ReplaceMatch(m, withDouble));
            
            
            return withPercent;
        }
    }
}
