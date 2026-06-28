var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<Games.TokenService>();

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

app.UseHttpsRedirection();
app.MapControllers();

// Endpoint used by RCC crash uploader, e.g.:
// https://data.freblx.xyz/Error/Grid.ashx?filename=...crashevent
app.MapPost("/Error/Grid.ashx", async (HttpRequest request) =>
{
    var filename = request.Query["filename"].ToString();

    if (string.IsNullOrWhiteSpace(filename))
    {
        return Results.BadRequest("Missing filename query parameter.");
    }

    var crashesDir = Path.Combine(app.Environment.ContentRootPath, "CrashEvents");
    Directory.CreateDirectory(crashesDir);

    // Normalize the file name to avoid writing outside the crashes directory
    var safeFileName = Path.GetFileName(filename);
    var targetPath = Path.Combine(crashesDir, safeFileName);

    // Buffer the body so we can both save it raw and optionally decompress it
    await using var memory = new MemoryStream();
    await request.Body.CopyToAsync(memory);
    memory.Position = 0;

    await using (var targetStream = File.Create(targetPath))
    {
        await memory.CopyToAsync(targetStream);
    }

    // Try to decompress as GZip into a sidecar .txt file if the magic header matches
    memory.Position = 0;
    if (memory.Length >= 2)
    {
        var header = new byte[2];
        _ = await memory.ReadAsync(header, 0, 2);
        if (header[0] == 0x1F && header[1] == 0x8B)
        {
            memory.Position = 0;
            var txtPath = targetPath + ".txt";
            try
            {
                await using var gzip = new System.IO.Compression.GZipStream(memory, System.IO.Compression.CompressionMode.Decompress, leaveOpen: true);
                await using var txtStream = File.Create(txtPath);
                await gzip.CopyToAsync(txtStream);
            }
            catch
            {
                // Ignore decompression errors; raw file is still stored.
            }
        }
    }

    return Results.Ok("Crash event uploaded.");
});

app.MapPost("/Error/Dmp.ashx", async (HttpRequest request) =>
{
    var filename = request.Query["filename"].ToString();

    if (string.IsNullOrWhiteSpace(filename))
    {
        return Results.BadRequest("Missing filename query parameter.");
    }

    var crashesDir = Path.Combine(app.Environment.ContentRootPath, "CrashEvents");
    Directory.CreateDirectory(crashesDir);
    const long MaxFolderSizeBytes = 1L * 1024 * 1024 * 1024; // 1 GB
    long folderSize = 0;
    
    try
    {
        var directoryInfo = new DirectoryInfo(crashesDir);
        foreach (var file in directoryInfo.GetFiles("*", SearchOption.AllDirectories))
        {
            folderSize += file.Length;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error calculating folder size: {ex.Message}");
    }

    if (folderSize > MaxFolderSizeBytes)
    {
        return Results.Ok("Crash dump received successfully");
    }

    var safeFileName = Path.GetFileName(filename);
    var targetPath = Path.Combine(crashesDir, safeFileName);
    await using var targetStream = File.Create(targetPath);
    await request.Body.CopyToAsync(targetStream);

    return Results.Ok("Crash dump received successfully");
});
app.Run();

