using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Assets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Users;

namespace RobloxWebserver.Controllers
{
    [Route("studio/plugins")]
    public class StudioPluginsController : Controller
    {
        private readonly AssetMetadataRepository _assetMetadataRepository = new();
        private readonly IConfiguration _configuration;

        public StudioPluginsController(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        [HttpGet("info")]
        public async Task<IActionResult> Info([FromQuery] long assetId, CancellationToken cancellationToken = default)
        {
            if (assetId <= 0)
                return Content(string.Empty, "text/html");

            var connectionString = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connectionString))
                return Content(string.Empty, "text/html");

            var asset = await _assetMetadataRepository.GetAssetByIdAsync(connectionString, assetId, cancellationToken).ConfigureAwait(false);
            if (asset == null)
                return Content(string.Empty, "text/html");

            var creatorName = "ROBLOX";
            if (asset.OwnerUserId > 0)
            {
                var creator = await UserQueries.GetUserNameByIdAsync(connectionString, asset.OwnerUserId, cancellationToken).ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(creator))
                    creatorName = creator;
            }

            long versionId;
            try
            {
                await using var conn = new NpgsqlConnection(connectionString);
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
                await using var cmd = new NpgsqlCommand("select extract(epoch from last_updated)::bigint from assets where asset_id = @id", conn);
                cmd.Parameters.AddWithValue("id", assetId);
                var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                versionId = result is long l ? l : asset.AssetId;
            }
            catch
            {
                versionId = asset.AssetId;
            }

            var slug = ToSlug(asset.Name);
            var catalogUrl = $"/catalog/{asset.AssetId}/{slug}";
            var profileUrl = $"/users/{asset.OwnerUserId}/profile";
            var thumbnailUrl = !string.IsNullOrWhiteSpace(asset.ThumbnailUrl) ? asset.ThumbnailUrl : "/images/RobloxLogo.png";
            var description = !string.IsNullOrWhiteSpace(asset.Description) ? System.Net.WebUtility.HtmlEncode(asset.Description) : "";
            var name = System.Net.WebUtility.HtmlEncode(asset.Name);
            var creatorEncoded = System.Net.WebUtility.HtmlEncode(creatorName);

            var html = $@"<div class=""plugin-info"">
    <div class=""plugin-info-image"">
        <a href=""{catalogUrl}"" class=""plugin-image""><img class="""" src=""{thumbnailUrl}""></a>
    </div>
    <div class=""plugin-info-text"">
        <h3 class=""plugin-title"">{name}</h3>
             by <a href=""{profileUrl}"">{creatorEncoded}</a>
        <div class=""more-less"">
            <div class=""more-block"">
                <p>{description}</p>
            </div>
            <a href=""#"" class=""adjust"">More...</a>
        </div>
        <input type=""hidden"" class=""asset-version-id"" value=""{versionId}"">
    </div>
</div>
<script type=""text/javascript"">
    $(function () {{
        Roblox.Plugins.PluginInfo.Resources = {{
            moreText: ""More..."",
            lessText: ""Less""
        }};
    }});
</script>";

            return Content(html, "text/html");
        }

        private static string ToSlug(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;
            var sb = new System.Text.StringBuilder(name.Length);
            foreach (var ch in name.ToLowerInvariant())
            {
                if ((ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9'))
                    sb.Append(ch);
                else
                    sb.Append('-');
            }
            return sb.ToString().Trim('-');
        }

        [HttpGet("installed")]
        [Authorize]
        public async Task<IActionResult> Installed(CancellationToken cancellationToken = default)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !long.TryParse(userIdClaim, out var userId) || userId <= 0)
                return Unauthorized(new { error = "Authentication required" });

            var connectionString = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connectionString))
                return StatusCode(500, new { error = "Database not configured" });

            try
            {
                var results = new List<object>();
                await using var conn = new NpgsqlConnection(connectionString);
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

                const string sql = @"select a.asset_id, a.name, a.description, a.thumbnail_url, a.last_updated
                    from user_assets ua
                    join assets a on a.asset_id = ua.asset_id and a.asset_type_id = 38
                    where ua.user_id = @uid
                    order by ua.created_at desc";

                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("uid", userId);

                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    var assetId = reader.GetInt64(0);
                    var name = reader.IsDBNull(1) ? "Unnamed" : reader.GetString(1);
                    var description = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    var thumbnailUrl = reader.IsDBNull(3) ? null : reader.GetString(3);
                    var lastUpdated = reader.GetDateTime(4);
                    var epoch = new DateTimeOffset(lastUpdated, TimeSpan.Zero).ToUnixTimeSeconds();

                    results.Add(new
                    {
                        AssetId = assetId,
                        Name = name,
                        Description = description,
                        ThumbnailUrl = thumbnailUrl,
                        AssetVersion = epoch
                    });
                }

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("install")]
        [Authorize]
        public async Task<IActionResult> Install([FromForm] long assetId, CancellationToken cancellationToken = default)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !long.TryParse(userIdClaim, out var userId) || userId <= 0)
                return Unauthorized(new { isValid = false, success = false, error = "Authentication required" });

            if (assetId <= 0)
                return BadRequest(new { isValid = false, success = false, error = "Invalid assetId" });

            var connectionString = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connectionString))
                return StatusCode(500, new { isValid = false, success = false, error = "Database not configured" });

            try
            {
                var metadataRepo = new AssetMetadataRepository();
                var asset = await metadataRepo.GetAssetByIdAsync(connectionString, assetId, cancellationToken).ConfigureAwait(false);
                if (asset == null)
                    return Ok(new { isValid = false, success = false, error = "Asset not found." });

                if (asset.AssetTypeId != 38)
                    return Ok(new { isValid = false, success = false, error = "Asset is not a plugin." });

                var repo = new UserAssetsRepository();
                var owns = await repo.UserOwnsAssetAsync(connectionString, userId, assetId, cancellationToken).ConfigureAwait(false);
                if (owns)
                    return Ok(new { isValid = true, success = true, alreadyOwned = true });

                await repo.AddUserAssetAsync(connectionString, userId, assetId, cancellationToken).ConfigureAwait(false);

                return Ok(new { isValid = true, success = true });
            }
            catch (Exception)
            {
                return StatusCode(500, new { isValid = false, success = false, error = "Failed to install plugin" });
            }
        }

        [HttpPost("uninstall")]
        [Authorize]
        public async Task<IActionResult> Uninstall([FromForm] long assetId, CancellationToken cancellationToken = default)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !long.TryParse(userIdClaim, out var userId) || userId <= 0)
                return Unauthorized(new { isValid = false, success = false, error = "Authentication required" });

            if (assetId <= 0)
                return BadRequest(new { isValid = false, success = false, error = "Invalid assetId" });

            var connectionString = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connectionString))
                return StatusCode(500, new { isValid = false, success = false, error = "Database not configured" });

            try
            {
                var metadataRepo = new AssetMetadataRepository();
                var asset = await metadataRepo.GetAssetByIdAsync(connectionString, assetId, cancellationToken).ConfigureAwait(false);
                if (asset == null)
                    return Ok(new { isValid = false, success = false, error = "Asset not found." });

                if (asset.AssetTypeId != 38)
                    return Ok(new { isValid = false, success = false, error = "Asset is not a plugin." });

                var repo = new UserAssetsRepository();
                var owns = await repo.UserOwnsAssetAsync(connectionString, userId, assetId, cancellationToken).ConfigureAwait(false);
                if (!owns)
                    return Ok(new { isValid = false, success = false, error = "You do not own this plugin." });

                await repo.RemoveUserAssetAsync(connectionString, userId, assetId, cancellationToken).ConfigureAwait(false);

                return Ok(new { isValid = true, success = true });
            }
            catch (Exception)
            {
                return StatusCode(500, new { isValid = false, success = false, error = "Failed to uninstall plugin" });
            }
        }

        [HttpGet("publish/list")]
        [Authorize]
        public async Task<IActionResult> PublishList(CancellationToken cancellationToken = default)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !long.TryParse(userIdClaim, out var userId) || userId <= 0)
                return Unauthorized();

            var connectionString = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connectionString))
                return StatusCode(500, "Database not configured");

            var plugins = new List<PluginEntry>();
            try
            {
                await using var conn = new NpgsqlConnection(connectionString);
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

                const string sql = @"SELECT asset_id, name, description, thumbnail_url, created_at
FROM assets
WHERE owner_user_id = @uid AND asset_type_id = 38
ORDER BY created_at DESC";

                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("uid", userId);

                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    plugins.Add(new PluginEntry
                    {
                        AssetId = reader.GetInt64(0),
                        Name = reader.IsDBNull(1) ? "Unnamed Plugin" : reader.GetString(1),
                        Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                        ThumbnailUrl = reader.IsDBNull(3) ? null : reader.GetString(3),
                        CreatedAt = reader.GetDateTime(4)
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }

            return PartialView("~/Views/IDE/PluginList.cshtml", plugins);
        }

        [HttpPost("publish/new")]
        [Authorize]
        public async Task<IActionResult> PublishNewPlugin(CancellationToken cancellationToken = default)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !long.TryParse(userIdClaim, out var userId) || userId <= 0)
                return Unauthorized();

            var connectionString = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connectionString))
                return StatusCode(500, new { error = "Service unavailable" });

            var name = Request.Form["Name"].FirstOrDefault() ?? "";
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Name is required.");

            var description = Request.Form["Description"].FirstOrDefault() ?? "";
            var genreStr = Request.Form["Genre"].FirstOrDefault() ?? "All";
            var allowComments = Request.Form["AllowComments"].Contains("true");
            var isCopyingAllowed = Request.Form["IsCopyingAllowed"].Contains("true");
            var genreTypeId = AssetGenreNames.GetGenreIdFromString(genreStr);

            var repo = new AssetsRepository();

            var createParams = new AssetCreateParams
            {
                Name = name,
                AssetTypeId = 38,
                OwnerUserId = userId,
                Description = description,
                ContentHash = "pending",
                FileExtension = ".rbxm",
                ContentType = "application/octet-stream",
                IsCopyingAllowed = isCopyingAllowed
            };

            var assetId = await repo.CreateAssetAsync(connectionString, createParams, cancellationToken).ConfigureAwait(false);

            var userAssetsRepo = new UserAssetsRepository();
            await userAssetsRepo.AddUserAssetAsync(connectionString, userId, assetId, cancellationToken).ConfigureAwait(false);

            if (genreTypeId is >= 0 and <= 20)
                await repo.UpdateAssetGenreAsync(connectionString, assetId, genreTypeId, cancellationToken).ConfigureAwait(false);

            await repo.UpdateAssetAllowCommentsAsync(connectionString, assetId, allowComments, cancellationToken).ConfigureAwait(false);

            return Redirect($"/ide/PublishAs/upload/{assetId}");
        }
    }
}

public sealed class PluginEntry
{
    public long AssetId { get; set; }
    public string Name { get; set; } = "Unnamed Plugin";
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}
