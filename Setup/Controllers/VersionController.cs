using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RobloxSetupServer.Data;
using System.Data;

namespace RobloxSetupServer.Controllers;

[ApiController]
public class VersionController : ControllerBase
{
    private readonly ILogger<VersionController> _logger;
    private readonly AppDbContext _dbContext;

    public VersionController(ILogger<VersionController> logger, AppDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    [HttpGet("version")]
    public async Task<IActionResult> GetVersion([FromQuery] int? guid = null)
    {
        try
        {
            var version = await _dbContext.Setup
                .OrderByDescending(s => s.Id)
                .Select(s => s.CurrentWindowsplayerVersion)
                .FirstOrDefaultAsync();
            
            if (string.IsNullOrEmpty(version))
            {
                return NotFound("No version found");
            }
            
            return Content("version-" + version, "text/plain");
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{product:alpha}")]
    public async Task<IActionResult> GetProductVersion(string product, [FromQuery] int? guid = null)
    {
        try
        {
            string version;
            
            switch (product?.ToLower())
            {
                case "robloxplayer":
                version = await _dbContext.Setup
                    .OrderByDescending(s => s.Id)
                    .Select(s => s.CurrentWindowsplayerVersion)
                    .FirstOrDefaultAsync();
                break;
                case "roblox":
                    version = await _dbContext.Setup
                        .OrderByDescending(s => s.Id)
                        .Select(s => s.CurrentWindowsplayerVersion)
                        .FirstOrDefaultAsync();
                    break;
                case "robloxstudio":
                    version = await _dbContext.Setup
                        .OrderByDescending(s => s.Id)
                        .Select(s => s.CurrentStudioVersion)
                        .FirstOrDefaultAsync();
                    break;
                case "rcc":
                    version = await _dbContext.Setup
                        .OrderByDescending(s => s.Id)
                        .Select(s => s.CurrentRccVersion)
                        .FirstOrDefaultAsync();
                    break;
                case "bootstrapper":
                    version = await _dbContext.Setup
                        .OrderByDescending(s => s.Id)
                        .Select(s => s.CurrentWindowsplayerVersion)
                        .FirstOrDefaultAsync();
                    break;
                default:
                    return NotFound($"Unknown product: {product}");
            }
            
            if (string.IsNullOrEmpty(version))
            {
                return NotFound("No version found");
            }
            
            return Content("version-" + version, "text/plain");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving version for product: {product}");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{*filename}")]
    public IActionResult GetFile(string filename)
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filename);
        
        if (System.IO.File.Exists(filePath))
        {
            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            var contentType = GetContentType(filename);
            return File(fileBytes, contentType, filename);
        }
        else
        {
            return NotFound($"File {filename} not found");
        }
    }
    
    private string GetContentType(string filename)
    {
        var ext = Path.GetExtension(filename).ToLowerInvariant();
        return ext switch
        {
            ".exe" => "application/octet-stream",
            ".zip" => "application/zip",
            ".txt" => "text/plain",
            ".json" => "application/json",
            _ => "application/octet-stream"
        };
    }
}