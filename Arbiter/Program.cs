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
    public class PlayerCountRequest
    {
        public int PlayerCount { get; set; }
    }

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

            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            string rccUrl;
            bool autoStartRCC = false;
            bool httpFlag = args.Any(a => string.Equals(a, "--http", StringComparison.OrdinalIgnoreCase));
            bool httpEnabledInConfig = string.Equals(_configuration["HttpServer:Enabled"], "true", StringComparison.OrdinalIgnoreCase);
            
            if (httpFlag || httpEnabledInConfig)
            {
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
                if (_renderingRCC != null)
                {
                    Console.WriteLine();
                    _renderingRCC.Dispose();
                }
            }
        }

        private static void StartHttpServer(string rccUrl)
        {
            var builder = WebApplication.CreateBuilder();
            var urls = builder.Configuration["HttpServer:Urls"];
            if (!string.IsNullOrWhiteSpace(urls))
            {
                builder.WebHost.UseUrls(urls);
            }
            
            builder.Services.AddHostedService<RCCCleanupService>();
            
            var app = builder.Build();

            var scriptsRoot = Path.Combine(Directory.GetCurrentDirectory(), "Scripts");
            var jsonRoot = Path.Combine(Directory.GetCurrentDirectory(), "Json");
            var provider = new FileScriptProvider(scriptsRoot);
            var renderer = new ScriptRenderer();
            _renderMgr = new RenderingRccManager(builder.Configuration);

            app.MapGet("/health", () => Results.Ok(new { ok = true }));

            app.MapGet("/version", () =>
            {
                var asm = Assembly.GetExecutingAssembly();
                var name = asm.GetName();
                var arbiterVersion = name.Version != null ? name.Version.ToString() : "unknown";

                string currentRccUrl = rccUrl;
                string? rccVersion = null;
                int? rccEnvironmentCount = null;
                string? rccError = null;

                bool TryFetchStatus(string url)
                {
                    try
                    {
                        using var client = new RCCClient(url);
                        var status = client.GetStatus();
                        rccVersion = status.version;
                        rccEnvironmentCount = status.environmentCount;
                        rccError = null;
                        return true;
                    }
                    catch (Exception ex)
                    {
                        rccError = ex.Message;
                        return false;
                    }
                }

                if (!TryFetchStatus(currentRccUrl))
                {
                    try
                    {
                        if (_configuration != null)
                        {
                            if (_renderingRCC == null)
                            {
                                _renderingRCC = new RCCProcessManager(_configuration, "Rendering");
                                _renderingRCC.Start();
                            }

                            currentRccUrl = _renderingRCC.ServiceUrl;
                            TryFetchStatus(currentRccUrl);
                        }
                    }
                    catch (Exception ex)
                    {
                        rccError = ex.Message;
                    }
                }

                var payload = new
                {
                    arbiter = new
                    {
                        name = name.Name,
                        version = arbiterVersion
                    },
                    rcc = new
                    {
                        url = currentRccUrl,
                        version = rccVersion,
                        environmentCount = rccEnvironmentCount,
                        error = rccError
                    }
                };

                return Results.Ok(payload);
            });

            app.MapGet("/status", async () =>
            {
                bool rccReachable = false;
                string? rccError = null;

                try
                {
                    if (Uri.TryCreate(rccUrl, UriKind.Absolute, out var uri))
                    {
                        var host = uri.Host;
                        var port = uri.Port;
                        using var client = new TcpClient();
                        var connectTask = client.ConnectAsync(host, port);
                        var timeoutTask = Task.Delay(1000);
                        var completed = await Task.WhenAny(connectTask, timeoutTask);
                        if (completed == connectTask && client.Connected)
                        {
                            rccReachable = true;
                        }
                        else if (!client.Connected)
                        {
                            rccError = "Timeout while connecting to RCC";
                        }
                    }
                    else
                    {
                        rccError = "Invalid RCC URL";
                    }
                }
                catch (Exception ex)
                {
                    rccError = ex.Message;
                }

                var status = new
                {
                    ok = true,
                    rcc = new
                    {
                        url = rccUrl,
                        reachable = rccReachable,
                        error = rccError
                    }
                };

                return Results.Ok(status);
            });

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

            var endpointTypes = Assembly.GetExecutingAssembly()
                .GetTypes();
            foreach (var t in endpointTypes)
            {
                if (!typeof(ICompiledEndpoint).IsAssignableFrom(t) || t.IsInterface || t.IsAbstract)
                    continue;

                if (Activator.CreateInstance(t) is not ICompiledEndpoint endpoint)
                    continue;

                endpoint.SetConfiguration(builder.Configuration);

                var route = endpoint.Route.StartsWith("/") ? endpoint.Route : "/" + endpoint.Route;

                async Task<IResult> Handle(HttpRequest req)
                {
                    try
                    {
                        var mapped = endpoint.MapParameters(req);

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
                            const int maxAttempts = 3;
                            LuaValue[] results = Array.Empty<LuaValue>();
                            SocketException? lastConnectionException = null;

                            for (int attempt = 1; attempt <= maxAttempts; attempt++)
                            {
                                try
                                {
                                    using var client = new RCCClient(urlToUse);

                                    bool forceJson = false;
                                    if (req.Query.TryGetValue("ForceJson", out var fq) || req.Query.TryGetValue("forceJson", out fq))
                                    {
                                        var raw = fq.ToString();
                                        if (!string.IsNullOrWhiteSpace(raw))
                                        {
                                            forceJson = string.Equals(raw, "true", StringComparison.OrdinalIgnoreCase) ||
                                                        raw == "1";
                                        }
                                    }

                                    bool preferJson = client.IsJsonPreferred(forceJson);

                                    var runner = new ScriptRunner(client, renderer);

                                    if (preferJson)
                                    {
                                        var jsonPath = Path.Combine(jsonRoot, endpoint.ScriptName + ".json");
                                        if (!File.Exists(jsonPath))
                                        {
                                            if (!provider.TryGetScript(endpoint.ScriptName, out var luaTemplateFallback))
                                            {
                                                return Results.NotFound(new { error = $"Script '{endpoint.ScriptName}.lua' or JSON template '{endpoint.ScriptName}.json' not found" });
                                            }

                                            results = runner.RunScript(endpoint.ScriptName, luaTemplateFallback, mapped);
                                        }
                                        else
                                        {
                                            var jsonTemplate = File.ReadAllText(jsonPath, Encoding.UTF8);
                                            var renderedJson = renderer.Render(jsonTemplate, mapped);
                                            var scriptName = endpoint.ScriptName + "JsonPayload";
                                            results = runner.RunRawScript(scriptName, renderedJson ?? string.Empty);
                                        }
                                    }
                                    else
                                    {
                                        if (!provider.TryGetScript(endpoint.ScriptName, out var template))
                                        {
                                            return Results.NotFound(new { error = $"Script '{endpoint.ScriptName}.lua' not found" });
                                        }

                                        results = runner.RunScript(endpoint.ScriptName, template, mapped);
                                    }

                                    lastConnectionException = null;
                                    break;
                                }
                                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionRefused)
                                {
                                    lastConnectionException = ex;
                                    if (attempt < maxAttempts)
                                    {
                                        await Task.Delay(500);
                                    }
                                }
                            }

                            if (lastConnectionException != null && (results == null || results.Length == 0))
                            {
                                throw lastConnectionException;
                            }

                            if (req.Query.TryGetValue("GiveImage", out var giveImg) &&
                                string.Equals(giveImg.ToString(), "true", StringComparison.OrdinalIgnoreCase))
                            {
                                if (results != null && results.Length > 0 && results[0].type == LuaType.LUA_TSTRING)
                                {
                                    try
                                    {
                                        var base64 = results[0].value as string ?? string.Empty;
                                        var bytes = Convert.FromBase64String(base64);
                                        return Results.File(bytes, "image/png");
                                    }
                                    catch
                                    {
                                    }
                                }
                            }

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

            var gameServerManager = new GameServerManager(rccUrl, builder.Configuration);

            app.MapPost("/api/gameservers/start", async (StartGameServerRequest request) =>
            {
                try
                {
                    string urlToUse = rccUrl;
                    IDisposable? lease = null;
                    if (_renderMgr != null)
                    {
                        var acquired = _renderMgr.AcquireIfTriggered("gameserver-start");
                        if (acquired.HasValue)
                        {
                            urlToUse = acquired.Value.url;
                            lease = acquired.Value.lease;
                        }
                    }
                    using (lease)
                    {
                        // Generate dynamic port if not provided
                        var port = request.Port;
                        if (port == 0)
                        {
                            port = RCCArbiter.Endpoints.StartGameServerEndpoint.GenerateAvailablePort();
                        }
                        
                        var gameId = await gameServerManager.StartGameServerAsync(
                            request.PlaceId, 
                            port, 
                            request.MaxPlayers, 
                            request.PrivateServerId ?? "", 
                            request.BaseUrl ?? "",
                            request.MaxInactive ?? 0
                        );
                        return Results.Ok(new { gameId = gameId, status = "started" });
                    }
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            });

            app.MapGet("/api/gameservers/start", async (HttpRequest req) =>
            {
                try
                {
                    string urlToUse = rccUrl;
                    IDisposable? lease = null;
                    if (_renderMgr != null)
                    {
                        var acquired = _renderMgr.AcquireIfTriggered("gameserver-start");
                        if (acquired.HasValue)
                        {
                            urlToUse = acquired.Value.url;
                            lease = acquired.Value.lease;
                        }
                    }
                    using (lease)
                    {
                        var placeId = int.TryParse(req.Query["placeId"], out var pid) ? pid : 15;
                        
                        int port;
                        if (int.TryParse(req.Query["port"], out var p))
                        {
                            port = p;
                        }
                        else
                        {
                            port = RCCArbiter.Endpoints.StartGameServerEndpoint.GenerateAvailablePort();
                        }
                        
                        var maxPlayers = int.TryParse(req.Query["maxPlayers"], out var mp) ? mp : 10;
                        var privateServerId = req.Query["privateServerId"].FirstOrDefault() ?? "";
                        var baseUrl = req.Query["baseUrl"].FirstOrDefault() ?? "http://www.freblx.xyz";
                        var maxInactive = int.TryParse(req.Query["maxInactive"], out var mi) ? mi : 0;

                        var gameId = await gameServerManager.StartGameServerAsync(
                            placeId, 
                            port, 
                            maxPlayers, 
                            privateServerId, 
                            baseUrl,
                            maxInactive
                        );
                        return Results.Ok(new { gameId = gameId, status = "started" });
                    }
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            });

            app.MapGet("/api/gameservers/{gameId}/status", async (string gameId) =>
            {
                try
                {
                    string urlToUse = rccUrl;
                    IDisposable? lease = null;
                    if (_renderMgr != null)
                    {
                        var acquired = _renderMgr.AcquireIfTriggered("gameserver-status");
                        if (acquired.HasValue)
                        {
                            urlToUse = acquired.Value.url;
                            lease = acquired.Value.lease;
                        }
                    }
                    using (lease)
                    {
                        var status = await gameServerManager.GetGameServerStatusAsync(gameId, urlToUse);
                        var serverInfo = gameServerManager.GetGameServerInfo(gameId);
                        
                        var response = new
                        {
                            gameId = status.GameId,
                            placeId = status.PlaceId,
                            port = status.Port,
                            maxPlayers = status.MaxPlayers,
                            playerCount = status.PlayerCount,
                            status = status.Status,
                            startTime = status.StartTime,
                            expiration = status.Expiration,
                            baseUrl = status.BaseUrl,
                            privateServerId = status.PrivateServerId,
                            lastActivityTime = status.LastActivityTime,
                            inactivityTimeout = serverInfo?.InactivityTimeout ?? TimeSpan.MaxValue,
                            autoKillEnabled = serverInfo?.AutoKillEnabled ?? false,
                            lastStatus = status.LastStatus
                        };
                        
                        return Results.Ok(response);
                    }
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            });

            app.MapDelete("/api/gameservers/{gameId}", (string gameId) =>
            {
                try
                {
                    string urlToUse = rccUrl;
                    IDisposable? lease = null;
                    if (_renderMgr != null)
                    {
                        var acquired = _renderMgr.AcquireIfTriggered("gameserver-stop");
                        if (acquired.HasValue)
                        {
                            urlToUse = acquired.Value.url;
                            lease = acquired.Value.lease;
                        }
                    }
                    using (lease)
                    {
                        gameServerManager.StopGameServer(gameId);
                        return Results.Ok(new { gameId = gameId, status = "stopped" });
                    }
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            });

            app.MapPost("/api/gameservers/trigger-inactivity-check", () =>
            {
                try
                {
                    gameServerManager.TriggerInactivityCheck();
                    return Results.Ok(new { message = "Inactivity check triggered" });
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            });

            app.MapGet("/api/gameservers/by-place/{placeId}", (int placeId) =>
            {
                try
                {
                    var servers = gameServerManager.GetAllGameServers()
                        .Where(s => s.PlaceId == placeId)
                        .Select(s => new
                        {
                            gameId = s.GameId,
                            port = s.Port,
                            playerCount = s.PlayerCount,
                            status = s.Status,
                            startTime = s.StartTime,
                            maxPlayers = s.MaxPlayers,
                            baseUrl = s.BaseUrl,
                            connectionUrl = $"roblox://localhost:{s.Port}?gameId={s.GameId}&placeId={placeId}"
                        })
                        .OrderByDescending(s => s.startTime)
                        .ToList();

                    return Results.Ok(new
                    {
                        placeId = placeId,
                        serverCount = servers.Count,
                        servers = servers
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            });

            app.MapGet("/api/gameservers/best-server/{placeId}", (int placeId) =>
            {
                try
                {
                    var bestServer = gameServerManager.GetAllGameServers()
                        .Where(s => s.PlaceId == placeId && s.Status == "running")
                        .OrderBy(s => s.PlayerCount)
                        .ThenByDescending(s => s.StartTime)
                        .FirstOrDefault();

                    if (bestServer == null)
                    {
                        return Results.NotFound(new { error = $"No active servers found for place {placeId}" });
                    }

                    return Results.Ok(new
                    {
                        gameId = bestServer.GameId,
                        port = bestServer.Port,
                        playerCount = bestServer.PlayerCount,
                        maxPlayers = bestServer.MaxPlayers,
                        status = bestServer.Status,
                        startTime = bestServer.StartTime,
                        baseUrl = bestServer.BaseUrl,
                        connectionUrl = $"roblox://localhost:{bestServer.Port}?gameId={bestServer.GameId}&placeId={placeId}",
                        reason = bestServer.PlayerCount == 0 ? "Empty server available" : 
                                bestServer.PlayerCount < bestServer.MaxPlayers / 2 ? "Low population server" : "Best available server"
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            });

            app.MapGet("/api/gameservers/{gameId}/stop", (string gameId) =>
            {
                try
                {
                    string urlToUse = rccUrl;
                    IDisposable? lease = null;
                    if (_renderMgr != null)
                    {
                        var acquired = _renderMgr.AcquireIfTriggered("gameserver-stop");
                        if (acquired.HasValue)
                        {
                            urlToUse = acquired.Value.url;
                            lease = acquired.Value.lease;
                        }
                    }
                    using (lease)
                    {
                        gameServerManager.StopGameServer(gameId);
                        return Results.Ok(new { gameId = gameId, status = "stopped" });
                    }
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            });

            app.MapPost("/api/gameservers/{gameId}/renew", async (string gameId, RenewLeaseRequest request) =>
            {
                try
                {
                    string urlToUse = rccUrl;
                    IDisposable? lease = null;
                    if (_renderMgr != null)
                    {
                        var acquired = _renderMgr.AcquireIfTriggered("gameserver-renew");
                        if (acquired.HasValue)
                        {
                            urlToUse = acquired.Value.url;
                            lease = acquired.Value.lease;
                        }
                    }
                    using (lease)
                    {
                        await gameServerManager.RenewGameServerLeaseAsync(gameId, request.AdditionalSeconds);
                        return Results.Ok(new { gameId = gameId, status = "renewed" });
                    }
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            });

            app.MapGet("/api/gameservers/{gameId}/renew", async (string gameId, HttpRequest req) =>
            {
                try
                {
                    string urlToUse = rccUrl;
                    IDisposable? lease = null;
                    if (_renderMgr != null)
                    {
                        var acquired = _renderMgr.AcquireIfTriggered("gameserver-renew");
                        if (acquired.HasValue)
                        {
                            urlToUse = acquired.Value.url;
                            lease = acquired.Value.lease;
                        }
                    }
                    using (lease)
                    {
                        var additionalSeconds = int.TryParse(req.Query["additionalSeconds"], out var seconds) ? seconds : 3600;
                        await gameServerManager.RenewGameServerLeaseAsync(gameId, additionalSeconds);
                        return Results.Ok(new { gameId = gameId, status = "renewed", additionalSeconds = additionalSeconds });
                    }
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            });

            app.MapGet("/api/gameservers", () =>
            {
                try
                {
                    string urlToUse = rccUrl;
                    IDisposable? lease = null;
                    if (_renderMgr != null)
                    {
                        var acquired = _renderMgr.AcquireIfTriggered("gameserver-list");
                        if (acquired.HasValue)
                        {
                            urlToUse = acquired.Value.url;
                            lease = acquired.Value.lease;
                        }
                    }
                    using (lease)
                    {
                        var servers = gameServerManager.GetAllGameServers();
                        return Results.Ok(servers);
                    }
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            });

            app.MapPost("/api/gameservers/cleanup", () =>
            {
                try
                {
                    string urlToUse = rccUrl;
                    IDisposable? lease = null;
                    if (_renderMgr != null)
                    {
                        var acquired = _renderMgr.AcquireIfTriggered("gameserver-cleanup");
                        if (acquired.HasValue)
                        {
                            urlToUse = acquired.Value.url;
                            lease = acquired.Value.lease;
                        }
                    }
                    using (lease)
                    {
                        gameServerManager.CleanupExpiredServers();
                        return Results.Ok(new { status = "cleanup_completed" });
                    }
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            });

            app.MapPost("/api/gameservers/{gameId}/playercount", (string gameId, PlayerCountRequest request) =>
            {
                try
                {
                    gameServerManager.UpdatePlayerCount(gameId, request.PlayerCount);
                    return Results.Ok(new { gameId = gameId, playerCount = request.PlayerCount, updated = true });
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            });

            app.MapGet("/api/gameservers/cleanup", () =>
            {
                try
                {
                    string urlToUse = rccUrl;
                    IDisposable? lease = null;
                    if (_renderMgr != null)
                    {
                        var acquired = _renderMgr.AcquireIfTriggered("gameserver-cleanup");
                        if (acquired.HasValue)
                        {
                            urlToUse = acquired.Value.url;
                            lease = acquired.Value.lease;
                        }
                    }
                    using (lease)
                    {
                        gameServerManager.CleanupExpiredServers();
                        return Results.Ok(new { status = "cleanup_completed" });
                    }
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            });

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

public class StartGameServerRequest
{
    public int PlaceId { get; set; }
    public int Port { get; set; } = 0;
    public int MaxPlayers { get; set; } = 10;
    public string? PrivateServerId { get; set; }
    public string? BaseUrl { get; set; }
    public int? MaxInactive { get; set; }
}

public class RenewLeaseRequest
{
    public int AdditionalSeconds { get; set; } = 3600;
}
