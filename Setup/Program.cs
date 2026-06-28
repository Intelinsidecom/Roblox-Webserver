using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using RobloxSetupServer.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<NpgsqlConnection>(provider => 
    new NpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

var lifetime = app.Lifetime;
lifetime.ApplicationStarted.Register(() =>
{
    var urls = string.Join(", ", app.Urls.DefaultIfEmpty("http://localhost:5000"));
    Console.WriteLine($"info: Setup.Hosting[0]");
    Console.WriteLine($"      Now hosting on: {urls}");
    Console.WriteLine($"      Environment: {app.Environment.EnvironmentName}");
    Console.WriteLine($"      Content root: {app.Environment.ContentRootPath}");
});

lifetime.ApplicationStopping.Register(() =>
{
    Console.WriteLine("info: Setup.Hosting[0]");
    Console.WriteLine("      Application is shutting down...");
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseAuthorization();

app.UseStaticFiles();

app.MapControllers();
app.Run();
