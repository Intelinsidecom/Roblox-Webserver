using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Thumbnails;
using Users;

namespace Website.Controllers.Frontend;

[ApiController]
public class ThumbnailsApiController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IThumbnailService _thumbnailService;

    public ThumbnailsApiController(IConfiguration configuration, IThumbnailService thumbnailService)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _thumbnailService = thumbnailService ?? throw new ArgumentNullException(nameof(thumbnailService));
    }

    private static string Serialize(object obj)
    {
        return JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
    }

    [Authorize]
    [HttpGet("v1/users/avatar-headshot")]
    public async Task<IActionResult> GetAvatarHeadshots(
        CancellationToken cancellationToken = default)
    {
        var size = Request.Query["size"].FirstOrDefault();
        var format = Request.Query["format"].FirstOrDefault();

        var allUserIds = new List<string>();
        foreach (var kv in Request.Query.Where(kv => kv.Key == "userIds"))
            allUserIds.AddRange(kv.Value);
        var userIdsStr = string.Join(",", allUserIds);

        if (string.IsNullOrWhiteSpace(userIdsStr))
            return Content(Serialize(new { data = new object[] { } }), "application/json");

        var userIds = userIdsStr.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => long.TryParse(s.Trim(), out var id) ? id : 0)
            .Where(id => id > 0)
            .Distinct()
            .Take(100)
            .ToList();

        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr))
            return Content(Serialize(new { data = new object[] { } }), "application/json");

        var data = new List<object>();

        foreach (var userId in userIds)
        {
            try
            {
                var headshotUrl = await ThumbnailQueries.GetUserHeadshotUrlAsync(connStr, userId, cancellationToken)
                    .ConfigureAwait(false);

                if (string.IsNullOrWhiteSpace(headshotUrl))
                {
                    var result = await _thumbnailService.RenderAvatarAsync("headshot", userId, 100, 100, cancellationToken)
                        .ConfigureAwait(false);
                    var baseUrl = _configuration["Thumbnails:ThumbnailUrl"];
                    if (string.IsNullOrWhiteSpace(baseUrl))
                    {
                        var scheme = string.IsNullOrWhiteSpace(Request.Scheme) ? "http" : Request.Scheme;
                        var host = Request.Host.HasValue ? Request.Host.Value : "localhost";
                        baseUrl = $"{scheme}://{host}/";
                    }
                    headshotUrl = baseUrl!.TrimEnd('/') + "/" + result.FileName.TrimStart('/');
                }

                data.Add(new
                {
                    targetId = userId,
                    state = "Completed",
                    imageUrl = headshotUrl ?? "/images/DefaultProfile.png",
                    version = ""
                });
            }
            catch
            {
                data.Add(new
                {
                    targetId = userId,
                    state = "Completed",
                    imageUrl = "/images/DefaultProfile.png",
                    version = ""
                });
            }
        }

        return Content(Serialize(new { data }), "application/json");
    }

    [Authorize]
    [HttpGet("v1/games/thumbnails")]
    public async Task<IActionResult> GetGameThumbnails(
        CancellationToken cancellationToken = default)
    {
        var returnPolicy = Request.Query["returnPolicy"].FirstOrDefault();
        var size = Request.Query["size"].FirstOrDefault();
        var format = Request.Query["format"].FirstOrDefault();
        var isCircular = Request.Query["isCircular"].FirstOrDefault() == "true";

        var allUniverseIds = new List<string>();
        foreach (var kv in Request.Query.Where(kv => kv.Key == "universeIds"))
            allUniverseIds.AddRange(kv.Value);
        var universeIdsStr = string.Join(",", allUniverseIds);

        if (string.IsNullOrWhiteSpace(universeIdsStr))
            return Content(Serialize(new { data = new object[] { } }), "application/json");

        var universeIds = universeIdsStr.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => long.TryParse(s.Trim(), out var id) ? id : 0)
            .Where(id => id > 0)
            .Distinct()
            .Take(100)
            .ToList();

        var connStr = _configuration.GetConnectionString("Default");
        var data = new List<object>();

        foreach (var universeId in universeIds)
        {
            try
            {
                var iconUrl = $"/Thumbs/Asset.ashx?x=150&y=150&asset={universeId}";
                data.Add(new
                {
                    targetId = universeId,
                    state = "Completed",
                    imageUrl = iconUrl,
                    version = ""
                });
            }
            catch
            {
                data.Add(new
                {
                    targetId = universeId,
                    state = "Completed",
                    imageUrl = "/images/DefaultProfile.png",
                    version = ""
                });
            }
        }

        return Content(Serialize(new { data }), "application/json");
    }
}
