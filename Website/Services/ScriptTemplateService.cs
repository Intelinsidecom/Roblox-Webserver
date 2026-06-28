using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;

namespace Website.Services;

public class ScriptTemplateService
{
    private readonly string _directory;
    private static readonly ConcurrentDictionary<string, string> _cache = new(StringComparer.OrdinalIgnoreCase);

    public ScriptTemplateService(IConfiguration configuration)
    {
        var configured = configuration["ScriptTemplates:Directory"];
        _directory = configured ?? Path.Combine(Directory.GetCurrentDirectory(), "Scripts");

        if (!Directory.Exists(_directory))
            Directory.CreateDirectory(_directory);
    }

    public string Render(string templateName, string replacements)
    {
        var map = ParseReplacements(replacements);

        var template = GetTemplate(templateName);

        var result = template;
        foreach (var (key, value) in map)
        {
            result = result.Replace(key, value);
        }

        return result;
    }

    private static Dictionary<string, string> ParseReplacements(string replacements)
    {
        var map = new Dictionary<string, string>();
        if (string.IsNullOrWhiteSpace(replacements))
            return map;

        var segments = replacements.Split(';', StringSplitOptions.RemoveEmptyEntries);
        foreach (var segment in segments)
        {
            var eqIdx = segment.IndexOf('=');
            if (eqIdx < 0) continue;

            var key = segment[..eqIdx].Trim();
            var value = segment[(eqIdx + 1)..].Trim();

            if (!string.IsNullOrEmpty(key))
                map[key] = value;
        }

        return map;
    }

    private string GetTemplate(string templateName)
    {
        return _cache.GetOrAdd(templateName, name =>
        {
            var templatePath = Path.Combine(_directory, name);
            if (!templatePath.EndsWith(".lua", StringComparison.OrdinalIgnoreCase))
                templatePath += ".lua";

            if (!File.Exists(templatePath))
                throw new FileNotFoundException($"Script template '{name}' not found at {templatePath}");

            return File.ReadAllText(templatePath);
        });
    }

    public void InvalidateCache(string? templateName = null)
    {
        if (templateName != null)
            _cache.TryRemove(templateName, out _);
        else
            _cache.Clear();
    }
}
