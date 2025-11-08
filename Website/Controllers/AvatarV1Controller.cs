using Microsoft.AspNetCore.Mvc;
using Thumbnails;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;

namespace Website.Controllers;

[ApiController]
[Route("v1/avatar")]
public class AvatarV1Controller : ControllerBase
{
    private readonly IThumbnailService _thumbnailService;
    private readonly IConfiguration _configuration;

    public AvatarV1Controller(IThumbnailService thumbnailService, IConfiguration configuration)
    {
        _thumbnailService = thumbnailService;
        _configuration = configuration;
    }

    // POST v1/avatar/redraw-thumbnail
    [HttpPost("redraw-thumbnail")]
    public async Task<IActionResult> RedrawThumbnail(CancellationToken cancellationToken)
    {
        var idStr = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(idStr) || !long.TryParse(idStr, out var userId) || userId <= 0)
            return Unauthorized(new { error = "Authentication required" });

        try
        {
            var hash = await _thumbnailService.RenderAvatarAsync("headshot", userId, cancellationToken: cancellationToken);
            // Compose full URL: base (config) + hash + ".png"
            var baseUrl = _configuration["Thumbnails:ThumbnailUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                var scheme = string.IsNullOrWhiteSpace(Request.Scheme) ? "http" : Request.Scheme;
                var host = Request.Host.HasValue ? Request.Host.Value : "localhost";
                baseUrl = $"{scheme}://{host}/";
            }
            var fullUrl = CombineUrl(baseUrl!, hash + ".png");

            // Save to DB: users.thumbnail_url
            var connStr = _configuration.GetConnectionString("Default");
            if (!string.IsNullOrWhiteSpace(connStr))
            {
                await using var conn = new NpgsqlConnection(connStr);
                await conn.OpenAsync(cancellationToken);
                await using var cmd = new NpgsqlCommand("update users set thumbnail_url = @u where user_id = @id", conn);
                cmd.Parameters.AddWithValue("u", fullUrl);
                cmd.Parameters.AddWithValue("id", userId);
                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }

            return Ok(new { hash, thumbnail_url = fullUrl });
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    private static string CombineUrl(string baseUrl, string relative)
    {
        if (string.IsNullOrEmpty(baseUrl)) return relative;
        if (string.IsNullOrEmpty(relative)) return baseUrl;
        var trimmedBase = baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/";
        return trimmedBase + relative.TrimStart('/');
    }
}
