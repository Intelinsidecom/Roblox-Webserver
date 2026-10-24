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

        int targetWidth = 100, targetHeight = 100;
        if (!string.IsNullOrWhiteSpace(size))
        {
            var parts = size.Split('x', 'X');
            if (parts.Length == 2 && 
                int.TryParse(parts[0], out var w) && 
                int.TryParse(parts[1], out var h))
            {
                targetWidth = Math.Clamp(w, 1, 1024);
                targetHeight = Math.Clamp(h, 1, 1024);
            }
        }

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
                    var result = await _thumbnailService.RenderAvatarAsync("headshot", userId, targetWidth, targetHeight, cancellationToken)
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

    /// <summary>
    /// GET /thumbnail/place-thumbnails?placeId=
    /// </summary>
    [HttpGet("thumbnail/place-thumbnails")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPlaceThumbnails(
        [FromQuery] long placeId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connStr = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr) || placeId <= 0)
            {
                return Content(Serialize(new
                {
                    IsMobile = false,
                    IsVideoAutoplayedOnReady = false,
                    thumbnailCount = 0,
                    thumbnails = new object[] { }
                }), "application/json");
            }

            var universeId = await Games.GamesRepository.GetUniverseIdFromPlaceIdAsync(connStr, placeId, cancellationToken);
            if (!universeId.HasValue)
            {
                return Content(Serialize(new
                {
                    IsMobile = false,
                    IsVideoAutoplayedOnReady = false,
                    thumbnailCount = 0,
                    thumbnails = new object[] { }
                }), "application/json");
            }

            var placeThumbnails = await Thumbnails.PlaceThumbnail.GetPlaceThumbnailsAsync(connStr, universeId.Value, cancellationToken);
            
            var thumbnails = new List<object>();
            foreach (var thumb in placeThumbnails)
            {
                var thumbnailType = thumb.GetType().GetProperty("type")?.GetValue(thumb)?.ToString();
                var url = thumb.GetType().GetProperty("url")?.GetValue(thumb)?.ToString();
                var videoUrl = thumb.GetType().GetProperty("videoUrl")?.GetValue(thumb)?.ToString();
                var assetId = thumb.GetType().GetProperty("id")?.GetValue(thumb);
                var sortOrder = thumb.GetType().GetProperty("sortOrder")?.GetValue(thumb);
                
                if (thumbnailType == "image" && !string.IsNullOrWhiteSpace(url))
                {
                    thumbnails.Add(new
                    {
                        AssetId = assetId != null ? Convert.ToInt64(assetId) : placeId,
                        AssetTypeId = 1, // Image
                        Url = url,
                        IsFinal = true,
                        AssetHash = (string?)null
                    });
                }
                else if (thumbnailType == "video" && !string.IsNullOrWhiteSpace(videoUrl))
                {
                    thumbnails.Add(new
                    {
                        AssetId = assetId != null ? Convert.ToInt64(assetId) : placeId,
                        AssetTypeId = 33, // Video
                        Url = videoUrl,
                        IsFinal = true,
                        AssetHash = (string?)null
                    });
                }
            }

            // If no custom thumbnails, add auto-generated thumbnail
            if (thumbnails.Count == 0)
            {
                var assetRepo = new Assets.AssetMetadataRepository();
                var placeAsset = await assetRepo.GetPlaceByIdAsync(connStr, universeId.Value, cancellationToken);
                
                if (placeAsset != null && !string.IsNullOrWhiteSpace(placeAsset.PlaceGeneratedThumbnailUrl))
                {
                    thumbnails.Add(new
                    {
                        AssetId = placeId,
                        AssetTypeId = 1,
                        Url = placeAsset.PlaceGeneratedThumbnailUrl,
                        IsFinal = true,
                        AssetHash = (string?)null
                    });
                }
            }

            // Default fallback
            if (thumbnails.Count == 0)
            {
                thumbnails.Add(new
                {
                    AssetId = placeId,
                    AssetTypeId = 1,
                    Url = "/images/DefaultPlaceIcon.png",
                    IsFinal = true,
                    AssetHash = (string?)null
                });
            }

            var payload = new
            {
                IsMobile = false,
                IsVideoAutoplayedOnReady = false,
                thumbnailCount = thumbnails.Count,
                thumbnails = thumbnails
            };

            return Content(Serialize(payload), "application/json");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] GetPlaceThumbnails placeId={placeId}: {ex}");
            return Content(Serialize(new
            {
                IsMobile = false,
                IsVideoAutoplayedOnReady = false,
                thumbnailCount = 0,
                thumbnails = new object[] { }
            }), "application/json");
        }
    }
}
