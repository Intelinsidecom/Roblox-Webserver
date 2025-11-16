using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using RCCArbiter.Scripting;
using RCCArbiter.Endpoints;
using System.Reflection;
using System.Net.Sockets;

namespace RCCArbiter
{
    partial class Program
    {
        private static RCCClient? _rccClient;
        private static IConfiguration? _configuration;
        private static RCCProcessManager? _renderingRCC;
        private static RenderingRccManager? _renderMgr;

        static void Main(string[] args)
        {
            Console.WriteLine("=== RCC Service Arbiter ===");
            Console.WriteLine();

            // Load configuration
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            // Determine RCC URL based on mode
            string rccUrl;
            bool autoStartRCC = false;

            // HTTP server mode if --http flag is present OR config enables it; otherwise interactive console
            bool httpFlag = args.Any(a => string.Equals(a, "--http", StringComparison.OrdinalIgnoreCase));
            bool httpEnabledInConfig = string.Equals(_configuration["HttpServer:Enabled"], "true", StringComparison.OrdinalIgnoreCase);
            
            if (httpFlag || httpEnabledInConfig)
            {
                // HTTP mode: check if we should auto-start Rendering RCC
                autoStartRCC = string.Equals(_configuration["Rendering:AutoStart"], "true", StringComparison.OrdinalIgnoreCase);
                
                if (autoStartRCC)
                {
                    try
                    {
                        _renderingRCC = new RCCProcessManager(_configuration, "Rendering");
                        _renderingRCC.Start();
                        rccUrl = _renderingRCC.ServiceUrl;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to start Rendering RCC: {ex.Message}");
                        Console.WriteLine("Falling back to configured RCC Service URL...");
                        Console.WriteLine();
                        rccUrl = _configuration["RCCService:ServiceUrl"] ?? "http://localhost:53640";
                    }
                }
                else
                {
                    rccUrl = _configuration["RCCService:ServiceUrl"] ?? "http://localhost:53640";
                    if (args.Length > 0 && !args[0].StartsWith("--"))
                    {
                        rccUrl = args[0];
                    }
                }
                
                Console.WriteLine($"HTTP Mode - Connecting to RCC Service at: {rccUrl}");
                Console.WriteLine();
                StartHttpServer(rccUrl);
                return;
            }
            
            // Interactive mode: use Rendering RCC configuration
            autoStartRCC = string.Equals(_configuration["Rendering:AutoStart"], "true", StringComparison.OrdinalIgnoreCase);
            
            if (autoStartRCC)
            {
                try
                {
                    _renderingRCC = new RCCProcessManager(_configuration, "Rendering");
                    _renderingRCC.Start();
                    rccUrl = _renderingRCC.ServiceUrl;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to start Rendering RCC: {ex.Message}");
                    Console.WriteLine("Falling back to configured RCC Service URL...");
                    Console.WriteLine();
                    rccUrl = _configuration["RCCService:ServiceUrl"] ?? "http://localhost:53640";
                }
            }
            else
            {
                rccUrl = _configuration["RCCService:ServiceUrl"] ?? "http://localhost:53640";
                if (args.Length > 0 && !args[0].StartsWith("--"))
                {
                    rccUrl = args[0];
                }
            }
            
            Console.WriteLine($"Interactive Mode - Connecting to RCC Service at: {rccUrl}");
            Console.WriteLine();

            // Interactive console mode
            try
            {
                using (var client = new RCCClient(rccUrl))
                {
                    Console.WriteLine("Interactive Mode");
                    Console.WriteLine("Type Lua and press Enter to execute.");
                    Console.WriteLine("Type 'exit' to quit.");
                    Console.WriteLine("Shortcut: press 'o' to run Scripts/HelloWorld.lua if present.");
                    Console.WriteLine();

                    while (true)
                    {
                        Console.Write("Lua> ");
                        string? input = Console.ReadLine();

                        if (string.IsNullOrWhiteSpace(input))
                            continue;

                        if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
                            break;

                        string luaScript = input;
                        if (input.Length == 1 && (input.Equals("o", StringComparison.OrdinalIgnoreCase)))
                        {
                            var helloPath = Path.Combine(Directory.GetCurrentDirectory(), "Scripts", "HelloWorld.lua");
                            if (File.Exists(helloPath))
                            {
                                luaScript = File.ReadAllText(helloPath);
                            }
                            else
                            {
                                luaScript = "print('Hello World from Lua!')";
                            }
                            Console.WriteLine("(HelloWorld)");
                        }
                        else
                        {
                            // If input looks like a script name or filename, load it from Scripts/
                            var scriptsDir = Path.Combine(Directory.GetCurrentDirectory(), "Scripts");
                            string fileName = input.EndsWith(".lua", StringComparison.OrdinalIgnoreCase) ? input : input + ".lua";
                            var fullPath = Path.Combine(scriptsDir, fileName);
                            if (File.Exists(fullPath))
                            {
                                luaScript = File.ReadAllText(fullPath);
                                Console.WriteLine($"(Loaded {fileName})");
                            }
                        }

                        try
                        {
                            var job = new Job
                            {
                                id = Guid.NewGuid().ToString(),
                                expirationInSeconds = 60,
                                category = 0,
                                cores = 1
                            };

                            var scriptExecution = new ScriptExecution
                            {
                                name = "InteractiveScript",
                                script = luaScript,
                                arguments = null
                            };

                            var results = client.BatchJobEx(job, scriptExecution);

                            var formatted = FormatLuaValues(results);
                            string json = JsonSerializer.Serialize(formatted, new JsonSerializerOptions { WriteIndented = true });
                            Console.WriteLine(json);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error: {ex.Message}");
                        }

                        Console.WriteLine();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
            }
            finally
            {
                // Clean up RCC process if we started it
                if (_renderingRCC != null)
                {
                    Console.WriteLine();
                    _renderingRCC.Dispose();
                }
            }
        }

        // HTTP server removed; interactive console mode only

        private static void StartHttpServer(string rccUrl)
        {
            var builder = WebApplication.CreateBuilder();
            // Bind to configured URLs if provided
            var urls = builder.Configuration["HttpServer:Urls"];
            if (!string.IsNullOrWhiteSpace(urls))
            {
                builder.WebHost.UseUrls(urls);
            }
            
            // Register shutdown handler to clean up RCC process
            builder.Services.AddHostedService<RCCCleanupService>();
            
            var app = builder.Build();

            var scriptsRoot = Path.Combine(Directory.GetCurrentDirectory(), "Scripts");
            var provider = new FileScriptProvider(scriptsRoot);
            var renderer = new ScriptRenderer();

            // Initialize scalable Rendering RCC manager
            _renderMgr = new RenderingRccManager(builder.Configuration);

            app.MapGet("/health", () => Results.Ok(new { ok = true }));

            // Use RenderingRccManager to acquire URL per request if triggered

            // Register all functions from registry as POST/GET /{name}
            foreach (var kv in Functions.Registry)
            {
                var name = kv.Key;
                var fn = kv.Value;
                var route = "/" + name;

                app.MapPost(route, async (HttpRequest req) =>
                {
                    try
                    {
                        string urlToUse = rccUrl;
                        IDisposable? lease = null;
                        if (_renderMgr != null)
                        {
                            var acquired = _renderMgr.AcquireIfTriggered(name);
                            if (acquired.HasValue)
                            {
                                urlToUse = acquired.Value.url;
                                lease = acquired.Value.lease;
                            }
                        }
                        using (lease)
                        {
                            var parameters = await Functions.ExtractParametersAsync(req, fn);
                            var results = Functions.Run(urlToUse, provider, renderer, fn, parameters);
                            var formatted = FormatLuaValues(results);
                            return Results.Json(formatted);
                        }
                    }
                    catch (Exception ex)
                    {
                        return Results.Problem(ex.Message);
                    }
                });

                app.MapGet(route, async (HttpRequest req) =>
                {
                    try
                    {
                        string urlToUse = rccUrl;
                        IDisposable? lease = null;
                        if (_renderMgr != null)
                        {
                            var acquired = _renderMgr.AcquireIfTriggered(name);
                            if (acquired.HasValue)
                            {
                                urlToUse = acquired.Value.url;
                                lease = acquired.Value.lease;
                            }
                        }
                        using (lease)
                        {
                            var parameters = await Functions.ExtractParametersAsync(req, fn);
                            var results = Functions.Run(urlToUse, provider, renderer, fn, parameters);
                            var formatted = FormatLuaValues(results);
                            return Results.Json(formatted);
                        }
                    }
                    catch (Exception ex)
                    {
                        return Results.Problem(ex.Message);
                    }
                });
            }

            // Legacy: /run/{name} supports both GET and POST
            app.MapMethods("/run/{name}", new[] { "GET", "POST" }, async (HttpRequest req) =>
            {
                try
                {
                    var name = (string?)req.RouteValues["name"] ?? string.Empty;
                    if (!Functions.Registry.TryGetValue(name, out var fn))
                    {
                        return Results.NotFound(new { error = $"Function '{name}' not found" });
                    }

                    string urlToUse = rccUrl;
                    IDisposable? lease = null;
                    if (_renderMgr != null)
                    {
                        var acquired = _renderMgr.AcquireIfTriggered(name);
                        if (acquired.HasValue)
                        {
                            urlToUse = acquired.Value.url;
                            lease = acquired.Value.lease;
                        }
                    }
                    using (lease)
                    {
                        var parameters = await Functions.ExtractParametersAsync(req, fn);
                        var results = Functions.Run(urlToUse, provider, renderer, fn, parameters);
                        var formatted = FormatLuaValues(results);
                        return Results.Json(formatted);
                    }
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            });

            // Compiled endpoints discovery and mapping
            var endpointTypes = Assembly.GetExecutingAssembly()
                .GetTypes();
            foreach (var t in endpointTypes)
            {
                if (!typeof(ICompiledEndpoint).IsAssignableFrom(t) || t.IsInterface || t.IsAbstract)
                    continue;

                if (Activator.CreateInstance(t) is not ICompiledEndpoint endpoint)
                    continue;

                // Provide configuration to endpoint
                endpoint.SetConfiguration(builder.Configuration);

                var route = endpoint.Route.StartsWith("/") ? endpoint.Route : "/" + endpoint.Route;

                async Task<IResult> Handle(HttpRequest req)
                {
                    try
                    {
                        var mapped = endpoint.MapParameters(req);

                        if (!provider.TryGetScript(endpoint.ScriptName, out var template))
                            return Results.NotFound(new { error = $"Script '{endpoint.ScriptName}.lua' not found" });

                        // Treat compiled endpoint route as trigger route
                        string urlToUse = rccUrl;
                        IDisposable? lease = null;
                        if (_renderMgr != null)
                        {
                            var acquired = _renderMgr.AcquireIfTriggered(route);
                            if (acquired.HasValue)
                            {
                                urlToUse = acquired.Value.url;
                                lease = acquired.Value.lease;
                            }
                        }
                        using (lease)
                        {
                            using var client = new RCCClient(urlToUse);
                            var runner = new ScriptRunner(client, renderer);
                            var results = runner.RunScript(endpoint.ScriptName, template, mapped);
                            var formatted = FormatLuaValues(results);
                            return Results.Json(formatted);
                        }
                    }
                    catch (Exception ex)
                    {
                        return Results.Problem(ex.Message);
                    }
                }

                app.MapGet(route, Handle);
                app.MapPost(route, Handle);
            }

            var bound = string.IsNullOrWhiteSpace(urls) ? "default Kestrel URLs (e.g. http://localhost:5000)" : urls;
            Console.WriteLine($"HTTP mode: listening on {bound}");
            app.Run();
        }

        static object[] FormatLuaValues(LuaValue[]? values)
        {
            if (values == null || values.Length == 0)
                return Array.Empty<object>();

            var result = new object[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                result[i] = FormatLuaValue(values[i]);
            }
            return result;
        }

        static object FormatLuaValue(LuaValue value)
        {
            return value.type switch
            {
                LuaType.LUA_TNIL => new { type = "nil", value = (string?)null },
                LuaType.LUA_TBOOLEAN => new { type = "boolean", value = value.value },
                LuaType.LUA_TNUMBER => new { type = "number", value = value.value },
                LuaType.LUA_TSTRING => new { type = "string", value = value.value },
                LuaType.LUA_TTABLE => new { type = "table", value = FormatLuaValues(value.table) },
                _ => new { type = "unknown", value = (string?)null }
            };
        }

        // Removed console print utilities; HTTP responses still use FormatLuaValues
    }

    /// <summary>
    /// Background service to clean up RCC process when HTTP server shuts down
    /// </summary>
    public class RCCCleanupService : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // Clean up RCC process when server stops
            if (Program.GetRenderingRCC() != null)
            {
                Console.WriteLine();
                Console.WriteLine("Shutting down HTTP server...");
                Program.GetRenderingRCC()?.Dispose();
            }
            Program.GetRenderingManager()?.Dispose();
            return Task.CompletedTask;
        }
    }

    partial class Program
    {
        public static RCCProcessManager? GetRenderingRCC() => _renderingRCC;
        public static RenderingRccManager? GetRenderingManager() => _renderMgr;
    }
}
