 using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Api.Middleware;
using Api.Services;
using Microsoft.EntityFrameworkCore;
using Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AuthCors", policy =>
    {
        policy.WithOrigins(
                "https://www.freblx.xyz",
                "https://api.freblx.xyz",
                "http://www.freblx.xyz",
                "http://api.freblx.xyz",
                "http://localhost:5077",
                "http://localhost:3000",
                "http://127.0.0.1:5077",
                "http://127.0.0.1:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
builder.Services.AddHttpLogging(o =>
{
    o.LoggingFields = HttpLoggingFields.RequestPropertiesAndHeaders | HttpLoggingFields.ResponsePropertiesAndHeaders | HttpLoggingFields.RequestBody | HttpLoggingFields.ResponseBody;
    o.RequestBodyLogLimit = 1048576;
    o.ResponseBodyLogLimit = 1048576;
    o.MediaTypeOptions.AddText("application/json");
    o.MediaTypeOptions.AddText("text/plain");
    o.MediaTypeOptions.AddText("application/x-www-form-urlencoded");
    o.MediaTypeOptions.AddText("text/*");
});
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddHostedService<ConsoleKeyListenerHostedService>();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"));
});

builder.Services.AddMemoryCache();

builder.Services.AddSingleton<Games.AuthenticationTicketService>();
builder.Services.AddSingleton<Games.TokenService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CurrentUserService>();

var app = builder.Build();
 
 
if (app.Environment.IsDevelopment())

{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<RequestResponseLoggingMiddleware>();



app.UseForwardedHeaders();
app.UseCors("AuthCors");
app.UseAuthorization();
app.Use(async (context, next) =>
{
    await next();
    if (context.Response.StatusCode == StatusCodes.Status404NotFound && !context.Response.HasStarted)
    {
        context.Response.ContentType = "application/json; charset=utf-8";
        var payload = new { errors = new[] { new { code = 0, message = string.Empty } } };
        var json = System.Text.Json.JsonSerializer.Serialize(payload);
        await context.Response.WriteAsync(json);
    }
});

app.MapControllers();

app.MapGet("/", () => Results.Json(new { errors = new[] { new { code = 0, message = string.Empty } } }));

app.Run();
