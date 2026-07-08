using Microsoft.AspNetCore.Mvc;

namespace Data.Controllers;

[ApiController]
[Route("Error")]
public class ErrorController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public ErrorController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("Grid.ashx")]
    public async Task<IActionResult> GridApi([FromQuery] string filename)
    {
        bool AcceptCrashEvents = Convert.ToBoolean(_configuration["Service:AcceptCrashEvents"]);
        if (string.IsNullOrWhiteSpace(filename))
        {
            return BadRequest("Missing filename query parameter.");
        }

        if (AcceptCrashEvents)
        {
            string crashesDir = Path.Combine(Directory.GetCurrentDirectory(), "CrashEvents");
            Directory.CreateDirectory(crashesDir);

            var safeFileName = Path.GetFileName(filename);
            var targetPath = Path.Combine(crashesDir, safeFileName);

            await using var memory = new MemoryStream();
            await  Request.Body.CopyToAsync(memory);
            memory.Position = 0;

            await using (var targetStream = System.IO.File.Create(targetPath))
            {
                await memory.CopyToAsync(targetStream);
            }

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
                        await using var txtStream = System.IO.File.Create(txtPath);
                        await gzip.CopyToAsync(txtStream);
                    }
                    catch
                    {
                    }
                }
            }
        }

        return Ok("Crash event uploaded.");
    }

    [HttpPost("Dmp.ashx")]
    public async Task<IActionResult> DumpApi([FromQuery] string filename)
    {

        bool AcceptCrashEvents = Convert.ToBoolean(_configuration["Service:AcceptCrashEvents"]);

        if (string.IsNullOrWhiteSpace(filename))
        {
            return BadRequest("Missing filename query parameter.");
        }

        if (AcceptCrashEvents)
        {
            var crashesDir = Path.Combine(Directory.GetCurrentDirectory(), "CrashEvents");
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
                return Ok("Crash dump received successfully");
            }

            var safeFileName = Path.GetFileName(filename);
            var targetPath = Path.Combine(crashesDir, safeFileName);
            await using var targetStream = System.IO.File.Create(targetPath);
            await Request.Body.CopyToAsync(targetStream);
        }

        return Ok("Crash dump received successfully");
    }
}