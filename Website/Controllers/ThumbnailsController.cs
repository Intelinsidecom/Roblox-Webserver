using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Thumbnails;

namespace Website.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ThumbnailsController : ControllerBase
{
    private readonly IThumbnailService _thumbnailService;
    private readonly IConfiguration _configuration;

    public ThumbnailsController(IThumbnailService thumbnailService, IConfiguration configuration)
    {
        _thumbnailService = thumbnailService;
        _configuration = configuration;
    }

    // Accepts Content-Type: text/plain or application/json. Body must be a JSON object.
    // { thumbnail: base64, userId: <number|string>, accessKey: string, type: string, jobId: string }
    [HttpPost("upload")]
    public async Task<IActionResult> Upload()
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();
        if (string.IsNullOrWhiteSpace(body))
        {
            return BadRequest(new { error = "Empty request body" });
        }

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(body);
        }
        catch (JsonException)
        {
            return BadRequest(new { error = "Invalid JSON payload" });
        }

        using (doc)
        {
            var root = doc.RootElement;
            if (!root.TryGetProperty("thumbnail", out var thumbEl) || thumbEl.ValueKind != JsonValueKind.String)
            {
                return BadRequest(new { error = "Missing field 'thumbnail'" });
            }
            var base64 = thumbEl.GetString() ?? string.Empty;

            var providedKey = root.TryGetProperty("accessKey", out var keyEl) && keyEl.ValueKind == JsonValueKind.String
                ? keyEl.GetString() ?? string.Empty
                : string.Empty;
            var expectedKey = _configuration["Upload:AccessKey"] ?? string.Empty;
            if (string.IsNullOrEmpty(expectedKey) || !string.Equals(providedKey, expectedKey, StringComparison.Ordinal))
            {
                return Unauthorized(new { error = "Invalid access key" });
            }

            try
            {
                var result = await _thumbnailService.SaveBase64PngAsync(base64);
                return Ok(new { hash = result.Hash });
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }
    }
}
