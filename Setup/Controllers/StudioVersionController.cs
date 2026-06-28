using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RobloxSetupServer.Data;
using System.Data;

namespace RobloxSetupServer.Controllers;

[ApiController]
public class StudioVersionController : ControllerBase
{
    private readonly ILogger<StudioVersionController> _logger;
    private readonly AppDbContext _dbContext;

    public StudioVersionController(ILogger<StudioVersionController> logger, AppDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    [HttpGet("versionQTStudio")]
    public async Task<IActionResult> GetVersion([FromQuery] int? guid = null)
    {
        try
        {
            var version = await _dbContext.Setup
                .OrderByDescending(s => s.Id)
                .Select(s => s.CurrentStudioVersion)
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