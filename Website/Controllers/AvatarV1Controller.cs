using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Thumbnails;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Linq;
using Users;
using Avatar;

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

    // GET v1/avatar
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAvatar(CancellationToken cancellationToken)
    {
        var idStr = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(idStr) || !long.TryParse(idStr, out var userId) || userId <= 0)
            return Unauthorized(new { error = "Authentication required" });

        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr))
            return Problem("Database not configured");

        var repo = new AvatarRepository();
        var state = await repo.GetAvatarAsync(connStr, userId, cancellationToken);

        return Ok(new
        {
            bodyColors = state.BodyColors,
            scales = state.Scales,
            assets = state.Assets
        });
    }

    // GET v1/avatar/avatar-rules and /v1/avatar-rules
    [HttpGet("avatar-rules")]
    [HttpGet("/v1/avatar-rules")]
    public IActionResult GetAvatarRules()
    {
        var payload = new
        {
            wearableAssetTypes = new object[]
            {
                new { id = 2,  name = "T-Shirt",         maxNumber = 1 },
                new { id = 8,  name = "Hat",             maxNumber = 3 },
                new { id = 11, name = "Shirt",           maxNumber = 1 },
                new { id = 12, name = "Pants",           maxNumber = 1 },
                new { id = 17, name = "Head",            maxNumber = 1 },
                new { id = 18, name = "Face",            maxNumber = 1 },
                new { id = 19, name = "Gear",            maxNumber = 1 },
                new { id = 27, name = "Torso",           maxNumber = 1 },
                new { id = 28, name = "Right Arm",       maxNumber = 1 },
                new { id = 29, name = "Left Arm",        maxNumber = 1 },
                new { id = 30, name = "Left Leg",        maxNumber = 1 },
                new { id = 31, name = "Right Leg",       maxNumber = 1 },
                new { id = 41, name = "Hair Accessory",  maxNumber = 1 },
                new { id = 42, name = "Face Accessory",  maxNumber = 1 },
                new { id = 43, name = "Neck Accessory",  maxNumber = 1 },
                new { id = 44, name = "Shoulder Accessory", maxNumber = 1 },
                new { id = 45, name = "Front Accessory", maxNumber = 1 },
                new { id = 46, name = "Back Accessory",  maxNumber = 1 },
                new { id = 47, name = "Waist Accessory", maxNumber = 1 },
                new { id = 48, name = "Climb Animation", maxNumber = 1 },
                new { id = 50, name = "Fall Animation",  maxNumber = 1 },
                new { id = 51, name = "Idle Animation",  maxNumber = 1 },
                new { id = 52, name = "Jump Animation",  maxNumber = 1 },
                new { id = 53, name = "Run Animation",   maxNumber = 1 },
                new { id = 54, name = "Swim Animation",  maxNumber = 1 },
                new { id = 55, name = "Walk Animation",  maxNumber = 1 }
            },
            scales = new
            {
                height = new { min = 0.90, max = 1.05, increment = 0.01 },
                width  = new { min = 0.70, max = 1.00, increment = 0.01 },
                head   = new { min = 0.95, max = 1.00, increment = 0.01 }
            },
            bodyColorsPalette = new object[]
            {
                // Full palette imported from frontend Roblox avatarules.json
                new { brickColorId = 361, hexColor = "#564236", name = "Dirt brown" },
                new { brickColorId = 192, hexColor = "#694028", name = "Reddish brown" },
                new { brickColorId = 217, hexColor = "#7C5C46", name = "Brown" },
                new { brickColorId = 153, hexColor = "#957977", name = "Sand red" },
                new { brickColorId = 359, hexColor = "#AF9483", name = "Linen" },
                new { brickColorId = 352, hexColor = "#C7AC78", name = "Burlap" },
                new { brickColorId = 5,   hexColor = "#D7C59A", name = "Brick yellow" },
                new { brickColorId = 101, hexColor = "#DA867A", name = "Medium red" },
                new { brickColorId = 1007,hexColor = "#A34B4B", name = "Dusty Rose" },
                new { brickColorId = 1014,hexColor = "#AA5500", name = "CGA brown" },
                new { brickColorId = 38,  hexColor = "#A05F35", name = "Dark orange" },
                new { brickColorId = 18,  hexColor = "#CC8E69", name = "Nougat" },
                new { brickColorId = 125, hexColor = "#EAB892", name = "Light orange" },
                new { brickColorId = 1030,hexColor = "#FFCC99", name = "Pastel brown" },
                new { brickColorId = 133, hexColor = "#D5733D", name = "Neon orange" },
                new { brickColorId = 106, hexColor = "#DA8541", name = "Bright orange" },
                new { brickColorId = 105, hexColor = "#E29B40", name = "Br. yellowish orange" },
                new { brickColorId = 1017,hexColor = "#FFAF00", name = "Deep orange" },
                new { brickColorId = 24,  hexColor = "#F5CD30", name = "Bright yellow" },
                new { brickColorId = 334, hexColor = "#F8D96D", name = "Daisy orange" },
                new { brickColorId = 226, hexColor = "#FDEA8D", name = "Cool yellow" },
                new { brickColorId = 141, hexColor = "#27462D", name = "Earth green" },
                new { brickColorId = 1021,hexColor = "#3A7D15", name = "Camo" },
                new { brickColorId = 28,  hexColor = "#287F47", name = "Dark green" },
                new { brickColorId = 37,  hexColor = "#4B974B", name = "Bright green" },
                new { brickColorId = 310, hexColor = "#5B9A4C", name = "Shamrock" },
                new { brickColorId = 317, hexColor = "#7C9C6B", name = "Moss" },
                new { brickColorId = 119, hexColor = "#A4BD47", name = "Br. yellowish green" },
                new { brickColorId = 1011,hexColor = "#002060", name = "Navy blue" },
                new { brickColorId = 1012,hexColor = "#2154B9", name = "Deep blue" },
                new { brickColorId = 1010,hexColor = "#0000FF", name = "Really blue" },
                new { brickColorId = 23,  hexColor = "#0D69AC", name = "Bright blue" },
                new { brickColorId = 305, hexColor = "#527CAE", name = "Steel blue" },
                new { brickColorId = 102, hexColor = "#6E99CA", name = "Medium blue" },
                new { brickColorId = 45,  hexColor = "#B4D2E4", name = "Light blue" },
                new { brickColorId = 107, hexColor = "#008F9C", name = "Bright bluish green" },
                new { brickColorId = 1018,hexColor = "#12EED4", name = "Teal" },
                new { brickColorId = 1027,hexColor = "#9FF3E9", name = "Pastel blue-green" },
                new { brickColorId = 1019,hexColor = "#00FFFF", name = "Toothpaste" },
                new { brickColorId = 1013,hexColor = "#04AFEC", name = "Cyan" },
                new { brickColorId = 11,  hexColor = "#80BBDC", name = "Pastel Blue" },
                new { brickColorId = 1024,hexColor = "#AFDDFF", name = "Pastel light blue" },
                new { brickColorId = 104, hexColor = "#6B327C", name = "Bright violet" },
                new { brickColorId = 1023,hexColor = "#8C5B9F", name = "Lavender" },
                new { brickColorId = 321, hexColor = "#A75E9B", name = "Lilac" },
                new { brickColorId = 1015,hexColor = "#AA00AA", name = "Magenta" },
                new { brickColorId = 1031,hexColor = "#6225D1", name = "Royal purple" },
                new { brickColorId = 1006,hexColor = "#B480FF", name = "Alder" },
                new { brickColorId = 1026,hexColor = "#B1A7FF", name = "Pastel violet" },
                new { brickColorId = 21,  hexColor = "#C4281C", name = "Bright red" },
                new { brickColorId = 1004,hexColor = "#FF0000", name = "Really red" },
                new { brickColorId = 1032,hexColor = "#FF00BF", name = "Hot pink" },
                new { brickColorId = 1016,hexColor = "#FF66CC", name = "Pink" },
                new { brickColorId = 330, hexColor = "#FF98DC", name = "Carnation pink" },
                new { brickColorId = 9,   hexColor = "#E8BAC8", name = "Light reddish violet" },
                new { brickColorId = 1025,hexColor = "#FFC9C9", name = "Pastel orange" },
                new { brickColorId = 364, hexColor = "#5A4C42", name = "Dark taupe" },
                new { brickColorId = 351, hexColor = "#BC9B5D", name = "Cork" },
                new { brickColorId = 1008,hexColor = "#C1BE42", name = "Olive" },
                new { brickColorId = 29,  hexColor = "#A1C48C", name = "Medium green" },
                new { brickColorId = 1022,hexColor = "#7F8E64", name = "Grime" },
                new { brickColorId = 151, hexColor = "#789082", name = "Sand green" },
                new { brickColorId = 135, hexColor = "#74869D", name = "Sand blue" },
                new { brickColorId = 1020,hexColor = "#00FF00", name = "Lime green" },
                new { brickColorId = 1028,hexColor = "#CCFFCC", name = "Pastel green" },
                new { brickColorId = 1009,hexColor = "#FFFF00", name = "New Yeller" },
                new { brickColorId = 1029,hexColor = "#FFFFCC", name = "Pastel yellow" },
                new { brickColorId = 1003,hexColor = "#111111", name = "Really black" },
                new { brickColorId = 26,  hexColor = "#1B2A35", name = "Black" },
                new { brickColorId = 199, hexColor = "#635F62", name = "Dark stone grey" },
                new { brickColorId = 194, hexColor = "#A3A2A5", name = "Medium stone grey" },
                new { brickColorId = 1002,hexColor = "#CDCDCD", name = "Mid gray" },
                new { brickColorId = 208, hexColor = "#E5E4DF", name = "Light stone grey" },
                new { brickColorId = 1,   hexColor = "#F2F3F3", name = "White" },
                new { brickColorId = 1001,hexColor = "#F8F8F8", name = "Institutional white" },
            },
            basicBodyColorsPalette = new object[]
            {
                new { brickColorId = 364, hexColor = "#5A4C42", name = "Dark taupe" },
                new { brickColorId = 217, hexColor = "#7C5C46", name = "Brown" },
                new { brickColorId = 359, hexColor = "#AF9483", name = "Linen" },
                new { brickColorId = 18,  hexColor = "#CC8E69", name = "Nougat" },
                new { brickColorId = 125, hexColor = "#EAB892", name = "Light orange" },
                new { brickColorId = 361, hexColor = "#564236", name = "Dirt brown" },
                new { brickColorId = 192, hexColor = "#694028", name = "Reddish brown" },
                new { brickColorId = 351, hexColor = "#BC9B5D", name = "Cork" },
                new { brickColorId = 352, hexColor = "#C7AC78", name = "Burlap" },
                new { brickColorId = 5,   hexColor = "#D7C59A", name = "Brick yellow" },
                new { brickColorId = 153, hexColor = "#957977", name = "Sand red" },
                new { brickColorId = 1007,hexColor = "#A34B4B", name = "Dusty Rose" },
                new { brickColorId = 101, hexColor = "#DA867A", name = "Medium red" },
                new { brickColorId = 1025,hexColor = "#FFC9C9", name = "Pastel orange" },
                new { brickColorId = 330, hexColor = "#FF98DC", name = "Carnation pink" },
                new { brickColorId = 135, hexColor = "#74869D", name = "Sand blue" },
                new { brickColorId = 305, hexColor = "#527CAE", name = "Steel blue" },
                new { brickColorId = 11,  hexColor = "#80BBDC", name = "Pastel Blue" },
                new { brickColorId = 1026,hexColor = "#B1A7FF", name = "Pastel violet" },
                new { brickColorId = 321, hexColor = "#A75E9B", name = "Lilac" },
                new { brickColorId = 107, hexColor = "#008F9C", name = "Bright bluish green" },
                new { brickColorId = 310, hexColor = "#5B9A4C", name = "Shamrock" },
                new { brickColorId = 317, hexColor = "#7C9C6B", name = "Moss" },
                new { brickColorId = 29,  hexColor = "#A1C48C", name = "Medium green" },
                new { brickColorId = 105, hexColor = "#E29B40", name = "Br. yellowish orange" },
                new { brickColorId = 24,  hexColor = "#F5CD30", name = "Bright yellow" },
                new { brickColorId = 334, hexColor = "#F8D96D", name = "Daisy orange" },
                new { brickColorId = 199, hexColor = "#635F62", name = "Dark stone grey" },
                new { brickColorId = 1002,hexColor = "#CDCDCD", name = "Mid gray" },
                new { brickColorId = 1001,hexColor = "#F8F8F8", name = "Institutional white" }
            },
            minimumDeltaEBodyColorDifference = 11.4
        };

        return Ok(payload);
    }

    // POST v1/avatar/redraw-thumbnail?type=headshot
    // Only the authenticated user may redraw their own thumbnail.
    [Authorize]
    [HttpPost("redraw-thumbnail")]
    [HttpGet("redraw-thumbnail")]
    public async Task<IActionResult> RedrawThumbnail([FromQuery] string? type, CancellationToken cancellationToken)
    {
        // Resolve target type
        var renderType = (type ?? "headshot").Trim().ToLowerInvariant();
        switch (renderType)
        {
            case "headshot":
            case "avatar":
            case "full":
                break;
            default:
                return BadRequest(new { error = "Invalid type. Allowed: headshot, avatar, full" });
        }

        // Resolve user from authentication only
        var idStr = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(idStr) || !long.TryParse(idStr, out var targetUserId) || targetUserId <= 0)
            return Unauthorized(new { error = "Authentication required" });

        try
        {
            var save = await _thumbnailService.RenderAvatarAsync(renderType, targetUserId, cancellationToken: cancellationToken);
            var hash = save.Hash;
            var baseUrl = _configuration["Thumbnails:ThumbnailUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                var scheme = string.IsNullOrWhiteSpace(Request.Scheme) ? "http" : Request.Scheme;
                var host = Request.Host.HasValue ? Request.Host.Value : "localhost";
                baseUrl = $"{scheme}://{host}/";
            }
            var fullUrl = CombineUrl(baseUrl!, save.FileName);

            var connStr = _configuration.GetConnectionString("Default");
            if (!string.IsNullOrWhiteSpace(connStr))
            {
                if (renderType == "headshot")
                {
                    await ThumbnailQueries.SetUserHeadshotUrlAsync(connStr!, targetUserId, fullUrl, cancellationToken);
                }
                else if (renderType == "avatar")
                {
                    await ThumbnailQueries.SetUserThumbnailUrlAsync(connStr!, targetUserId, fullUrl, cancellationToken);
                }
                // 'full' does not update DB
            }

            return Ok(new { hash, thumbnail_url = fullUrl });
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    [Authorize]
    [HttpPost("set-body-colors")]
    public async Task<IActionResult> SetBodyColors([FromBody] BodyColorsModel bodyColorsModel, CancellationToken cancellationToken)
    {
        var idStr = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(idStr) || !long.TryParse(idStr, out var userId) || userId <= 0)
            return Unauthorized(new { error = "Authentication required" });

        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr))
            return Problem("Database not configured");

        var repo = new Avatar.BodyColorsRepository();
        var colors = new Avatar.BodyColorsRepository.BodyColors
        {
            HeadColorId = bodyColorsModel.headColorId,
            TorsoColorId = bodyColorsModel.torsoColorId,
            RightArmColorId = bodyColorsModel.rightArmColorId,
            LeftArmColorId = bodyColorsModel.leftArmColorId,
            RightLegColorId = bodyColorsModel.rightLegColorId,
            LeftLegColorId = bodyColorsModel.leftLegColorId
        };
        await repo.SetBodyColorsAsync(connStr, userId, colors, cancellationToken);

        // Update avatar_state_hash so caches and diagnostics can
        // detect that the avatar configuration has changed.
        await AvatarStateHasher.RecomputeAndStoreAvatarHashAsync(connStr, userId, cancellationToken);

        return Ok(new { success = true });
    }

    [Authorize]
    [HttpPost("set-wearing-assets")]
    public async Task<IActionResult> SetWearingAssets([FromBody] SetWearingAssetsModel model, CancellationToken cancellationToken)
    {
        var idStr = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(idStr) || !long.TryParse(idStr, out var userId) || userId <= 0)
            return Unauthorized(new { error = "Authentication required" });

        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr))
            return Problem("Database not configured");

        var repo = new AvatarWornAssetsRepository();
        var rawIds = model.assetIds ?? Array.Empty<long>();

        // Normalize and enforce server-side clothing rules so callers cannot
        // bypass them by hitting the API directly.
        var filteredIds = rawIds.Distinct().ToList();

        if (filteredIds.Count > 0)
        {
            // Enforce "only one T-Shirt" at the persistence layer even if
            // a buggy or malicious client sends multiple T-Shirt asset IDs.
            const int TShirtAssetTypeId = 2;

            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = @"select asset_id from assets where asset_type_id = @tshirtTypeId and asset_id = any(@ids) order by asset_id";
            await using (var cmd = new NpgsqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("tshirtTypeId", TShirtAssetTypeId);
                cmd.Parameters.AddWithValue("ids", filteredIds.ToArray());

                var tshirtIds = new System.Collections.Generic.List<long>();
                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    tshirtIds.Add(reader.GetInt64(0));
                }

                if (tshirtIds.Count > 1)
                {
                    var keep = tshirtIds[0];
                    var tshirtSet = new System.Collections.Generic.HashSet<long>(tshirtIds);

                    filteredIds = filteredIds
                        .Where(id => !tshirtSet.Contains(id) || id == keep)
                        .ToList();
                }
            }
        }

        await repo.SetWornAssetIdsAsync(connStr, userId, filteredIds, cancellationToken);

        // Keep avatar_state_hash in sync with current body colors and
        // worn assets so downstream systems can see configuration changes.
        await AvatarStateHasher.RecomputeAndStoreAvatarHashAsync(connStr, userId, cancellationToken);

        return Ok(new { success = true });
    }

    private static string CombineUrl(string baseUrl, string relative)
    {
        if (string.IsNullOrEmpty(baseUrl)) return relative;
        if (string.IsNullOrEmpty(relative)) return baseUrl;
        var trimmedBase = baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/";
        return trimmedBase + relative.TrimStart('/');
    }

    public sealed class BodyColorsModel
    {
        public int headColorId { get; set; }
        public int torsoColorId { get; set; }
        public int rightArmColorId { get; set; }
        public int leftArmColorId { get; set; }
        public int rightLegColorId { get; set; }
        public int leftLegColorId { get; set; }
    }

    public sealed class SetWearingAssetsModel
    {
        public long[] assetIds { get; set; } = Array.Empty<long>();
    }
}
