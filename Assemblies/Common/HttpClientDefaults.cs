using System;
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
}