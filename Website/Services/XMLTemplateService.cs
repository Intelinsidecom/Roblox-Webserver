using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;

namespace Website.Services;

public class XMLTemplateService
{
    private readonly string _directory;
    private static readonly ConcurrentDictionary<string, string> _cache = new(StringComparer.OrdinalIgnoreCase);

    public XMLTemplateService(IConfiguration configuration)
    {
        _directory = configuration["XMLTemplates:Directory"] ?? Path.Combine(Directory.GetCurrentDirectory(), "xml");

        if (!Directory.Exists(_directory))
            Directory.CreateDirectory(_directory);
    }

    public string Render(string templateName, IReadOnlyDictionary<string, string> parameters)
    {
        var template = GetTemplate(templateName);

        var result = template;
        foreach (var (key, value) in parameters)
        {
            result = result.Replace($"{{{{{key}}}}}", value);
        }

        return result;
    }

    private string GetTemplate(string templateName)
    {
        return _cache.GetOrAdd(templateName, name =>
        {
            var templatePath = Path.Combine(_directory, name);

            if (!File.Exists(templatePath))
                throw new FileNotFoundException($"Template '{name}' not found at {templatePath}");

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
