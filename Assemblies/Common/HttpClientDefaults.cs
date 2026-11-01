using System;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Configuration;

namespace Common;

public static class HttpClientDefaults
{
    public static TimeSpan Timeout { get; private set; } = TimeSpan.FromSeconds(120);

    public static void Initialize(IConfiguration? configuration)
    {
        var raw = configuration?["HttpClient:TimeoutSeconds"];
        if (int.TryParse(raw, out var seconds) && seconds > 0)
            Timeout = TimeSpan.FromSeconds(seconds);
    }

    /// <summary>
    /// Creates an HttpClient that bypasses system proxy settings (e.g., Fiddler).
    /// Use this for local-to-local calls (Website → Arbiter) to avoid
    /// "connection refused" errors when the proxy port changes.
    /// </summary>
    public static HttpClient CreateLocalClient()
    {
        var handler = new HttpClientHandler
        {
            UseProxy = false,
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };
        return new HttpClient(handler);
    }
}
