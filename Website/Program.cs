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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddControllersWithViews()
    .AddApplicationPart(typeof(LoginController).Assembly);

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
builder.Services.AddSingleton<ICatalogRepository, CatalogRepository>();
builder.Services.AddSingleton<ICatalogService, CatalogService>();
builder.Services.AddSingleton<AssetMetadataRepository>();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();
builder.Services.AddWebOptimizerPipeline();
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
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseForwardedHeaders();

app.UseHttpsRedirection();

app.UseWebOptimizer();

var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
provider.Mappings[".file"] = "application/octet-stream";

app.UseStaticFiles(new StaticFileOptions
{
    ServeUnknownFileTypes = true,
    DefaultContentType = "application/octet-stream",
    ContentTypeProvider = provider
});

app.UseRouting();

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
        var connStr = context.RequestServices.GetRequiredService<IConfiguration>().GetConnectionString("Default");
        if (!string.IsNullOrWhiteSpace(connStr))
        {
            try
            {
                await using var conn = new NpgsqlConnection(connStr);
                await conn.OpenAsync();
                await using var cmd = new NpgsqlCommand("select user_id from sessions where token = @t and (expires_at is null or expires_at > now() at time zone 'utc')", conn);
                cmd.Parameters.AddWithValue("t", raw);
                var obj = await cmd.ExecuteScalarAsync();
                if (obj is long uid && uid > 0)
                {
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, uid.ToString()),
                        new Claim(ClaimTypes.Name, $"User_{uid}")
                    };
                    var identity = new ClaimsIdentity(claims, "Cookie");
                    context.User = new ClaimsPrincipal(identity);
                }
            }
            catch { /* ignore lookup errors */ }
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