using Microsoft.EntityFrameworkCore;
using RobloxWebserver.Data;
using System.Security.Claims;
using Npgsql;
using Microsoft.AspNetCore.HttpOverrides;
using Thumbnails;
using Microsoft.AspNetCore.Authentication;
using Website.Auth;
using WebOptimizer;
using Website.Extensions;
using RobloxWebserver.Assemblies.Catalog;
using Assets;
using Website.Services;
using Website.Middleware;
using Games;
using Website.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddControllersWithViews()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
    })
    .AddApplicationPart(System.Reflection.Assembly.GetExecutingAssembly());

// Add CORS service
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("*");
    });
});

// Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "XSRF-COOKIE";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
    options.FormFieldName = "__RequestVerificationToken";
});
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Default"),
        npgsql => npgsql.MigrationsAssembly("RobloxWebserver")
    )
);
// Thumbnails service
builder.Services.AddSingleton<IThumbnailService>(sp => new ThumbnailService(sp.GetRequiredService<IConfiguration>()));
builder.Services.AddSingleton<AvatarThumbnailRefreshService>();
builder.Services.AddSingleton<GamesCacheService>();
builder.Services.AddHostedService<GamesCacheService>(sp => sp.GetRequiredService<GamesCacheService>());
builder.Services.AddSingleton<ICatalogRepository, CatalogRepository>();
builder.Services.AddSingleton<IRazorViewRenderer, RazorViewRenderer>();
builder.Services.AddSingleton<ICatalogItemRenderer, RazorCatalogItemRenderer>();
builder.Services.AddSingleton<ICatalogService, CatalogService>();
builder.Services.AddSingleton<Website.Services.DevelopTabService>();
builder.Services.AddSingleton<ScriptTemplateService>();
builder.Services.AddSingleton<XMLTemplateService>();
builder.Services.AddSingleton<AssetMetadataRepository>();
builder.Services.AddSingleton<ToolboxService>();
builder.Services.AddHostedService<ToolboxService>(sp => sp.GetRequiredService<ToolboxService>());
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<Assets.AssetService>();
builder.Services.AddHttpClient();
builder.Services.AddWebOptimizerPipeline();
builder.Services.AddSignalR();

// Add Games services
builder.Services.AddSingleton<AuthenticationTicketService>();
builder.Services.AddSingleton<Games.TokenService>();
builder.Services.AddHostedService<TokenCleanupService>();
builder.Services.AddSingleton<GamePresenceService>();
builder.Services.AddHostedService<GamePresenceCleanupService>();

// Add Presence tracking
builder.Services.AddSingleton<PresenceTracker>();
builder.Services.AddHostedService<PresenceUpdateService>();

// Add Chat typing tracker
builder.Services.AddSingleton<TypingTracker>();

// Add Limited Items expiry service
builder.Services.AddHostedService<Economy.LimitedExpiryService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Passthrough";
    options.DefaultChallengeScheme = "Passthrough";
    options.DefaultSignInScheme = "Passthrough";
})
.AddScheme<AuthenticationSchemeOptions, PassthroughAuthHandler>("Passthrough", options => { });
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});
var enableRequestLogging = builder.Configuration.GetValue<bool>("Features:EnableRequestLogging");
if (enableRequestLogging)
{
    builder.Services.AddHttpLogging(options =>
    {
        options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestPropertiesAndHeaders |
                                 Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponsePropertiesAndHeaders |
                                 Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestBody |
                                 Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponseBody;
        options.RequestBodyLogLimit = 4096;
        options.ResponseBodyLogLimit = 4096;
    });
}




var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }
}

app.UseForwardedHeaders();
 
// Commented out HTTPS redirection to allow HTTP for development

// app.UseHttpsRedirection();

app.UseWebSockets();

app.UseWebOptimizer();

// Add CORS for UWP WebView support
app.UseCors(policy => policy
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader()
    .WithExposedHeaders("*"));

var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
provider.Mappings[".file"] = "application/octet-stream";

app.UseStaticFiles(new StaticFileOptions
{
    ServeUnknownFileTypes = true,
    DefaultContentType = "application/octet-stream",
    ContentTypeProvider = provider,
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, OPTIONS");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Headers", "*");
        ctx.Context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
        ctx.Context.Response.Headers["Pragma"] = "no-cache";
        ctx.Context.Response.Headers["Expires"] = "0";
    }
});

app.UseMiddleware<LockdownMiddleware>();

app.UseRouting();


app.UseWhen(
    context => !context.Request.Path.StartsWithSegments("/hubs"),
    appBuilder => appBuilder.UseSession()
);

app.UseWhen(
    context => !context.Request.Path.StartsWithSegments("/hubs"),
    appBuilder => appBuilder.UseRateLimiting()
);

if (enableRequestLogging)
{
    app.UseWhen(
        context => !context.Request.Path.StartsWithSegments("/hubs"),
        appBuilder => appBuilder.UseHttpLogging()
    );
}

app.Use(async (context, next) =>
{
    var cookies = context.Request.Cookies;
    var tokenService = context.RequestServices.GetRequiredService<Games.TokenService>();

    string? raw = null;
    if (cookies.TryGetValue(".ROBLOSECURITY", out var roblox))
        raw = roblox;

    if (raw != null)
    {
        try
        {
            long? userId = await tokenService.ValidateSessionAsync(raw);
            if (userId.HasValue && userId.Value > 0)
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()),
                    new Claim(ClaimTypes.Name, $"User_{userId.Value}")
                };
                var identity = new ClaimsIdentity(claims, "Cookie");
                context.User = new ClaimsPrincipal(identity);
            }
        }
        catch { }
    }
    else
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            context.User = new ClaimsPrincipal(new ClaimsIdentity());
        }
    }
    await next();
});
if (enableRequestLogging)
app.UseMiddleware<RequestResponseLoggingMiddleware>();

app.UseMiddleware<PresenceMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.UseWhen(
    context => !context.Request.Path.StartsWithSegments("/hubs"),
    appBuilder => appBuilder.UseMiddleware<PageErrorRedirectMiddleware>()
);

app.MapControllers();

app.MapHub<NotificationHub>("/hubs/notifications");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "pages",
    pattern: "{*path}",
    defaults: new { controller = "Pages", action = "Route" },
    constraints: new { path = @"^(?!.*\.(js|css|png|jpg|jpeg|gif|svg|ico|woff|woff2|ttf|eot|map|gz|download)$).*" }
);

app.Run();
