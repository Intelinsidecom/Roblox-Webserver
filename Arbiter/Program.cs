using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using RCCArbiter.Scripting;
using RCCArbiter.Endpoints;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;

namespace RCCArbiter
{
    class Program
    {
        private static RCCClient? _rccClient;
        private static IConfiguration? _configuration;

        static void Main(string[] args)
        {
            Console.WriteLine("=== RCC Service Arbiter ===");
            Console.WriteLine();

            // Load configuration
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            // Get RCC Service URL from config or command line
            string rccUrl = _configuration["RCCService:ServiceUrl"] ?? "http://localhost:53640";
            
            if (args.Length > 0 && !args[0].StartsWith("--"))
            {
                rccUrl = args[0];
            }

            Console.WriteLine($"Connecting to RCC Service at: {rccUrl}");
            Console.WriteLine();

            // HTTP server mode if --http flag is present OR config enables it; otherwise interactive console
            bool httpFlag = args.Any(a => string.Equals(a, "--http", StringComparison.OrdinalIgnoreCase));
            bool httpEnabledInConfig = string.Equals(_configuration["HttpServer:Enabled"], "true", StringComparison.OrdinalIgnoreCase);
            if (httpFlag || httpEnabledInConfig)
            {
                StartHttpServer(rccUrl);
                return;
            }

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
            var app = builder.Build();

            var scriptsRoot = Path.Combine(Directory.GetCurrentDirectory(), "Scripts");
            var provider = new FileScriptProvider(scriptsRoot);
            var renderer = new ScriptRenderer();

            app.MapGet("/health", () => Results.Ok(new { ok = true }));

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
                        var parameters = await Functions.ExtractParametersAsync(req, fn);
                        var results = Functions.Run(rccUrl, provider, renderer, fn, parameters);
                        var formatted = FormatLuaValues(results);
                        return Results.Json(formatted);
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
                        var parameters = await Functions.ExtractParametersAsync(req, fn);
                        var results = Functions.Run(rccUrl, provider, renderer, fn, parameters);
                        var formatted = FormatLuaValues(results);
                        return Results.Json(formatted);
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

                    var parameters = await Functions.ExtractParametersAsync(req, fn);
                    var results = Functions.Run(rccUrl, provider, renderer, fn, parameters);
                    var formatted = FormatLuaValues(results);
                    return Results.Json(formatted);
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

                        using var client = new RCCClient(rccUrl);
                        var runner = new ScriptRunner(client, renderer);
                        var results = runner.RunScript(endpoint.ScriptName, template, mapped);
                        var formatted = FormatLuaValues(results);
                        return Results.Json(formatted);
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
}
