using Microsoft.EntityFrameworkCore;
using Api.Data;
using Api.Controllers;
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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddControllersWithViews()
    .AddApplicationPart(typeof(LoginController).Assembly)
    .AddApplicationPart(System.Reflection.Assembly.GetExecutingAssembly());

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
        npgsql => npgsql.MigrationsAssembly("Api")
    )
);
// Thumbnails service
builder.Services.AddSingleton<IThumbnailService>(sp => new ThumbnailService(sp.GetRequiredService<IConfiguration>()));
builder.Services.AddSingleton<AvatarThumbnailRefreshService>();
builder.Services.AddSingleton<GamesCacheService>();
builder.Services.AddHostedService<GamesCacheService>(sp => sp.GetRequiredService<GamesCacheService>());
builder.Services.AddSingleton<ICatalogRepository, CatalogRepository>();
builder.Services.AddSingleton<ICatalogService, CatalogService>();
builder.Services.AddSingleton<AssetMetadataRepository>();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();
builder.Services.AddWebOptimizerPipeline();

// Add Games services
builder.Services.AddSingleton<AuthenticationTicketService>();
builder.Services.AddSingleton<TokenService>();
builder.Services.AddHostedService<TokenCleanupService>();
builder.Services.AddSingleton<GamePresenceService>();
builder.Services.AddHostedService<GamePresenceCleanupService>();

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

app.UseWebOptimizer();

var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
provider.Mappings[".file"] = "application/octet-stream";

app.UseStaticFiles(new StaticFileOptions
{
    ServeUnknownFileTypes = true,
    DefaultContentType = "application/octet-stream",
    ContentTypeProvider = provider
});

app.UseMiddleware<LockdownMiddleware>();
app.UseMiddleware<AntiCacheMiddleware>();

app.UseRouting();

app.UseSession();
app.UseRateLimiting();

if (enableRequestLogging)
{
    app.UseHttpLogging();
}

app.Use(async (context, next) =>
{
    var originalResponseStream = context.Response.Body;
    try
    {
        using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;
        
        await next();
        
        {
            responseBodyStream.Seek(0, SeekOrigin.Begin);
            await responseBodyStream.CopyToAsync(originalResponseStream);
        }
    }
    finally
    {
        context.Response.Body = originalResponseStream;
    }
});

app.Use(async (context, next) =>
{
    var cookies = context.Request.Cookies;
    if (cookies.TryGetValue(".ROBLOSECURITY", out var raw))
    {
        var tokenService = context.RequestServices.GetRequiredService<TokenService>();
        
        try
        {
            long? userId = null;
            
            if (raw.StartsWith("sess_"))
            {
                userId = await tokenService.ValidateSessionAsync(raw);
            }
            else
            {
                userId = await tokenService.ValidateSessionAsync(raw);
            }
            
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
            else
            {
                context.Response.Cookies.Delete(".ROBLOSECURITY", new CookieOptions 
                { 
                    Path = "/",
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax
                });
            }
        }
        catch (Exception ex) { Console.WriteLine($"[AUTH] Error during auth: {ex.Message}"); }
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

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<PageErrorRedirectMiddleware>();

app.MapControllers();

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
