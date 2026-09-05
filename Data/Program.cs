using Data.Middleware;
using Common;



var builder = WebApplication.CreateBuilder(args);
Common.HttpClientDefaults.Initialize(builder.Configuration);
var currentDirectory = Directory.GetCurrentDirectory();
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<Games.TokenService>();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<Assets.AssetService>();

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = null; // unlimited
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(5);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseMiddleware<RequestResponseLoggingMiddleware>();

//app.UseHttpsRedirection();

app.UseMiddleware<RequestResponseLoggingMiddleware>();

app.Use(async (context, next) =>
{
    if (context.Request.Path.HasValue)
    {
        var path = context.Request.Path.Value;
        if (path != null)
        {
            var normalized = path;
            if (path.Contains("//"))
            {
                var sb = new System.Text.StringBuilder(path.Length);
                bool prevWasSlash = false;
                for (int i = 0; i < path.Length; i++)
                {
                    if (path[i] == '/')
                    {
                        if (!prevWasSlash)
                            sb.Append('/');
                        prevWasSlash = true;
                    }
                    else
                    {
                        sb.Append(path[i]);
                        prevWasSlash = false;
                    }
                }
                normalized = sb.ToString();
            }
            if (normalized != path)
                context.Request.Path = new PathString(normalized);
        }
    }
    await next();
});

//app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseRouting();

app.MapControllers();


app.Run();

