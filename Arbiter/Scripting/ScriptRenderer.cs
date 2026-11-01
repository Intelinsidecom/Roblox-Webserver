using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace RCCArbiter.Scripting
{
    public class ScriptRenderer
    {
        /// <summary>
        /// Selects how tokens are substituted into a template.
        /// </summary>
        public enum RenderMode
        {
            /// <summary>
            /// Lua templates: non-numeric values are wrapped in quotes when they are not
            /// already inside a quoted literal.
            /// </summary>
            Lua,

            /// <summary>
            /// JSON job descriptors: the template owns all quoting, values are only escaped,
            /// unresolved tokens are an error, and the result must parse as JSON.
            /// </summary>
            Json
        }

        private static readonly Regex TokenRegexDoubleBraces = new Regex(@"\{\{\s*([a-zA-Z0-9_]+)\s*\}\}", RegexOptions.Compiled);
        private static readonly Regex TokenRegexPercent = new Regex(@"%([a-zA-Z0-9_]+)%", RegexOptions.Compiled);

        public string Render(string template, IDictionary<string, string>? parameters)
        {
            return Render(template, parameters, RenderMode.Lua);
        }

        public string Render(string template, IDictionary<string, string>? parameters, RenderMode mode)
        {
            if (string.IsNullOrEmpty(template) || parameters == null || parameters.Count == 0)
                return template;

            return mode == RenderMode.Json
                ? RenderJson(template, parameters)
                : RenderLua(template, parameters);
        }

        private static string RenderLua(string template, IDictionary<string, string> parameters)
        {
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

        /// <summary>
        /// Substitutes tokens into a JSON job descriptor. Unlike the Lua path this never adds
        /// quotes, because a token may sit in the middle of a string literal (for example
        /// "%baseUrl%/asset?id=%assetId%") where quoting would corrupt the value. The template
        /// decides the JSON type: bare tokens render numbers and booleans, quoted tokens render
        /// strings.
        /// </summary>
        private static string RenderJson(string template, IDictionary<string, string> parameters)
        {
            var missing = new List<string>();

            string ReplaceMatch(Match m)
            {
                var key = m.Groups[1].Value;
                if (!parameters.TryGetValue(key, out var raw))
                {
                    // Collect but leave the token in place so the reported names line up with
                    // what the template actually asked for.
                    if (!missing.Contains(key, StringComparer.OrdinalIgnoreCase))
                        missing.Add(key);
                    return m.Value;
                }

                return EscapeJsonString(raw ?? string.Empty);
            }

            var withDouble = TokenRegexDoubleBraces.Replace(template, ReplaceMatch);
            var rendered = TokenRegexPercent.Replace(withDouble, ReplaceMatch);

            if (missing.Count > 0)
            {
                throw new InvalidOperationException(
                    $"JSON template is missing values for: {string.Join(", ", missing.Select(name => "%" + name + "%"))}");
            }

            try
            {
                // Fail here rather than handing a malformed descriptor to RCC and waiting for
                // the job to come back empty.
                JsonDocument.Parse(rendered);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    $"JSON template rendered invalid JSON after substitution: {ex.Message}", ex);
            }

            return rendered;
        }

        private static string EscapeJsonString(string value)
        {
            var builder = new System.Text.StringBuilder(value.Length);
            foreach (var c in value)
            {
                switch (c)
                {
                    case '\\': builder.Append("\\\\"); break;
                    case '"': builder.Append("\\\""); break;
                    case '\b': builder.Append("\\b"); break;
                    case '\f': builder.Append("\\f"); break;
                    case '\n': builder.Append("\\n"); break;
                    case '\r': builder.Append("\\r"); break;
                    case '\t': builder.Append("\\t"); break;
                    default:
                        if (c < 0x20)
                            builder.Append("\\u").Append(((int)c).ToString("x4"));
                        else
                            builder.Append(c);
                        break;
                }
            }

            return builder.ToString();
        }
    }
}
