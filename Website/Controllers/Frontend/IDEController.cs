using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Assets;
using Common;
using Games;
using Thumbnails;
using Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Website.Services;

namespace RobloxWebserver.Controllers;

[ApiController]
[Route("ide")]
[Authorize]
public class IDEController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly ToolboxService _toolboxService;
    private const int PageSize = 50;

    public IDEController(IConfiguration configuration, ToolboxService toolboxService)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _toolboxService = toolboxService ?? throw new ArgumentNullException(nameof(toolboxService));
    }

    private string? ConnectionString => _configuration.GetConnectionString("Default");

    private long GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !long.TryParse(userIdClaim, out var userId) || userId <= 0)
            return 0;
        return userId;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginPost(
        [FromServices] TokenService tokenService,
        [FromForm] string? username = null,
        [FromForm] string? password = null,
        [FromForm] string? returnUrl = null)
    {
        if (User?.Identity?.IsAuthenticated == true)
            return Redirect(string.IsNullOrWhiteSpace(returnUrl) ? "/ide/welcome" : returnUrl);

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ViewBag.ErrorMessage = "Username and password are required.";
            ViewBag.ReturnUrl = returnUrl;
            return View("~/Views/Pages/ide/Login.cshtml");
        }

        var connString = ConnectionString;
        if (string.IsNullOrWhiteSpace(connString))
        {
            ViewBag.ErrorMessage = "Service unavailable.";
            ViewBag.ReturnUrl = returnUrl;
            return View("~/Views/Pages/ide/Login.cshtml");
        }

        long userId = 0;
        string? storedPassword = null;
        try
        {
            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(
                "SELECT user_id, password FROM users WHERE lower(user_name) = lower(@u) LIMIT 1", conn);
            cmd.Parameters.AddWithValue("u", username);
            await using var rdr = await cmd.ExecuteReaderAsync();
            if (await rdr.ReadAsync())
            {
                userId = rdr.IsDBNull(0) ? 0 : rdr.GetInt64(0);
                storedPassword = rdr.IsDBNull(1) ? null : rdr.GetString(1);
            }
        }
        catch
        {
            ViewBag.ErrorMessage = "Login failed. Please try again.";
            ViewBag.ReturnUrl = returnUrl;
            return View("~/Views/Pages/Login.cshtml");
        }

        if (userId <= 0 || string.IsNullOrEmpty(storedPassword) || !PasswordHasher.VerifyPassword(password, storedPassword))
        {
            ViewBag.ErrorMessage = "Invalid username or password.";
            ViewBag.ReturnUrl = returnUrl;
            return View("~/Views/Pages/Login.cshtml");
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        string token;
        try
        {
            token = await tokenService.CreateSessionAsync(userId, ip);
        }
        catch
        {
            ViewBag.ErrorMessage = "Login failed. Please try again.";
            ViewBag.ReturnUrl = returnUrl;
            return View("~/Views/Pages/ide/Login.cshtml");
        }

        var expires = DateTimeOffset.UtcNow.AddYears(1);
        var isHttps = Request.IsHttps;
        var allowInsecure = _configuration.GetValue<bool>("Auth:AllowInsecureCookies");
        var cookieDomain = _configuration["Auth:CookieDomain"];

        Response.Cookies.Append(".ROBLOSECURITY", token, new CookieOptions
        {
            HttpOnly = false,
            Secure = isHttps && !allowInsecure,
            SameSite = SameSiteMode.Unspecified,
            Expires = expires,
            Path = "/",
            Domain = string.IsNullOrWhiteSpace(cookieDomain) ? null : cookieDomain
        });

        return Redirect(string.IsNullOrWhiteSpace(returnUrl) ? "/ide/welcome" : returnUrl);
    }

    [HttpGet("gamelist")]
    public async Task<IActionResult> GameList([FromQuery] int page = 0, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId <= 0) return Unauthorized();

        var connStr = ConnectionString;
        if (string.IsNullOrWhiteSpace(connStr))
            return Content("", "text/html");

        var universes = await GameListingService.GetUniversesForUserAsync(connStr, userId, cancellationToken);

        var skip = page * PageSize;
        var pageItems = universes.Skip(skip).Take(PageSize).ToList();
        var hasMore = (skip + PageSize) < universes.Count;

        ViewBag.NextPage = page + 1;
        ViewBag.HasMore = hasMore;

        return PartialView("~/Views/IDE/GameList.cshtml", pageItems);
    }

    [HttpGet("gamelist/{groupId:long}")]
    public IActionResult GroupGameList(long groupId, [FromQuery] int page = 0)
    {
        return Content("", "text/html");
    }

    [HttpGet("placelist")]
    public async Task<IActionResult> PlaceList([FromQuery] int startRow = 0, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId <= 0) return Unauthorized();

        var connStr = ConnectionString;
        if (string.IsNullOrWhiteSpace(connStr))
            return Content("", "text/html");

        var places = await GetUserPlacesAsync(connStr, userId, startRow, PageSize + 1, cancellationToken);
        var hasMore = places.Count > PageSize;
        if (hasMore) places.RemoveAt(places.Count - 1);

        ViewBag.HasMore = hasMore;
        ViewBag.NextStartRow = startRow + PageSize;

        return PartialView("~/Views/IDE/PlaceList.cshtml", places);
    }

    [HttpGet("placelist/{groupId:long}")]
    public IActionResult GroupPlaceList(long groupId)
    {
        return Content("", "text/html");
    }

    [HttpGet("PublishAs/upload/{assetId:long}")]
    public async Task<IActionResult> Publish(long assetId, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId <= 0) return Unauthorized();

        var connStr = ConnectionString;
        if (string.IsNullOrWhiteSpace(connStr))
            return Content("", "text/html");

        await using var conn = new NpgsqlConnection(connStr);
        await conn.OpenAsync(cancellationToken);

        const string sql = "SELECT name, asset_type_id FROM assets WHERE asset_id = @pid AND owner_user_id = @uid";
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("pid", assetId);
        cmd.Parameters.AddWithValue("uid", userId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return Redirect("/404");

        var name = reader.IsDBNull(0) ? "Unnamed" : reader.GetString(0);
        var assetTypeId = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);

        var typeName = assetTypeId switch
        {
            9 => "Place",
            38 => "Plugin",
            _ => "Model"
        };
        var isPlace = assetTypeId == 9;

        var baseUrl = _configuration["PublicBaseUrl"]?.TrimEnd('/') ?? $"{Request.Scheme}://{Request.Host}";
        var dataBaseUrl = _configuration["DataBaseUrl"] ?? baseUrl
            .Replace("https://www.", "http://data.")
            .Replace("http://www.", "http://data.")
            .Replace("://assetgame.", "://data.");
        var uploadUrl = $"{dataBaseUrl}/Data/Upload.ashx?assetid={assetId}&type={typeName}&ispublic=False&groupId=";
        var previousUrl = baseUrl + "/ide/Upload.ashx";

        ViewBag.gameid = assetId;
        ViewBag.gamename = name;
        ViewBag.uploadUrl = uploadUrl;
        ViewBag.previousUrl = previousUrl;
        ViewBag.newUpload = "False";
        ViewBag.isplace = isPlace ? "True" : "False";
        ViewBag.ispackage = "False";

        return View("~/Views/Pages/ide/PublishAs/upload.cshtml");
    }

    [HttpGet("publishas/newgame")]
    public async Task<IActionResult> NewGame([FromQuery] long universeId, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId <= 0) return Unauthorized();

        var connStr = ConnectionString;
        ViewBag.xcsrftoken = "";
        ViewBag.universeId = universeId;
        ViewBag.privateServersAllowed = false;
        ViewBag.privateServersFree = true;
        ViewBag.privateServersPrice = 100;
        ViewBag.playableDevices = new List<string> { "Computer", "Phone", "Tablet" };
        ViewBag.maxVisitorCount = 8;
        ViewBag.accessType = "Everyone";
        ViewBag.isCopyingAllowed = false;
        ViewBag.isAllGenresAllowed = true;
        ViewBag.areCommentsEnabled = true;
        ViewBag.genre = "All";
        ViewBag.allowedGearTypes = "[]";

        if (!string.IsNullOrWhiteSpace(connStr))
        {
            try
            {
                var userName = await Users.UserQueries.GetUserNameByIdAsync(connStr, userId);
                ViewBag.userName = userName ?? "Player";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] NewGame fetch username for userId {userId}: {ex}");
            }
        }

        return View("~/Views/Pages/ide/PublishAs/NewGame.cshtml");
    }

    [HttpPost("publish/newplace")]
    public async Task<IActionResult> CreateNewPlace(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId <= 0) return Unauthorized();

        var connStr = ConnectionString;
        if (string.IsNullOrWhiteSpace(connStr))
            return StatusCode(500, new { error = "Service unavailable" });

        var universeIdStr = Request.Query["universeId"].FirstOrDefault() ?? Request.Form["universeId"].FirstOrDefault();
        long.TryParse(universeIdStr, out var universeId);

        var name = Request.Form["Name"].FirstOrDefault() ?? "";
        if (string.IsNullOrWhiteSpace(name))
            return StatusCode(400, new { error = "Name is required" });

        var description = Request.Form["Description"].FirstOrDefault() ?? "";
        var assetsRoot = _configuration["Assets:Directory"] ?? "";
        var templatesDir = Path.Combine(Directory.GetCurrentDirectory(), "Templates");

        var templatePlaceIdStr = _configuration["PlaceTemplates:DefaultTemplate"] ?? "95206881";
        long.TryParse(templatePlaceIdStr, out var templatePlaceId);
        if (templatePlaceId <= 0) templatePlaceId = 95206881;

        var templateMappings = new Dictionary<string, string>();
        foreach (var tmpl in _configuration.GetSection("PlaceTemplates").GetChildren())
        {
            if (tmpl.Key != "DefaultTemplate" && !string.IsNullOrWhiteSpace(tmpl.Value))
                templateMappings[tmpl.Key] = Path.Combine(templatesDir, tmpl.Value);
        }
        var contentHash = await PlacesHandler.ResolveTemplateContentHashAsync(connStr, templatePlaceId, assetsRoot, templateMappings);

        long placeId;

        if (universeId > 0)
        {
            var ownerId = await GamesRepository.GetUniverseOwnerAsync(connStr, universeId);
            if (ownerId == null || ownerId.Value != userId)
                return StatusCode(403, new { error = "You do not own this universe" });

            var assetsRepo = new Assets.AssetsRepository();
            placeId = await assetsRepo.CreateAssetAsync(connStr, new Assets.AssetCreateParams
            {
                Name = name,
                AssetTypeId = 9,
                OwnerUserId = userId,
                ContentHash = contentHash,
                FileExtension = ".rbxl",
                ContentType = "application/octet-stream",
                IsPlace = true,
                InUniverse = false,
                Description = description
            });

            var added = await PlacesHandler.AddPlaceToUniverseAsync(connStr, universeId, placeId);
            if (!added)
                return StatusCode(500, new { error = "Failed to add place to universe" });
        }
        else
        {
            var userName = await Users.UserQueries.GetUserNameByIdAsync(connStr, userId) ?? "Player";
            var thumbnailService = HttpContext.RequestServices.GetService(typeof(IThumbnailService)) as IThumbnailService;

            var universeInfo = await GameCreationService.CreateUniverseWithRootPlaceAsync(
                connStr,
                userId,
                userName,
                assetsRoot,
                starterPlacePath: null,
                enableCreationCooldown: false,
                thumbnailService: thumbnailService,
                configuration: _configuration,
                cancellationToken: cancellationToken,
                customName: name,
                templateContentHash: contentHash);

            placeId = universeInfo.RootPlaceId;
            universeId = universeInfo.UniverseId;
        }

        // Apply settings from all tabs (name, description already set during creation)
        await ApplyPlaceSettingsAsync(connStr, placeId, userId);

        _ = Task.Run(async () =>
        {
            try
            {
                var thumbnailService = HttpContext.RequestServices.GetService(typeof(IThumbnailService)) as IThumbnailService;
                if (thumbnailService != null && !string.IsNullOrWhiteSpace(contentHash) && contentHash != "pending-place-content")
                {
                    var baseUrl = _configuration["Thumbnails:ThumbnailUrl"] ?? $"{Request.Scheme}://{Request.Host}/";
                    await PlaceThumbnail.GeneratePlaceThumbnailAsync(thumbnailService, connStr, placeId, contentHash, baseUrl, placeName: name, cancellationToken: CancellationToken.None);
                    await PlaceThumbnail.GenerateAutoGeneratedThumbnailAsync(thumbnailService, connStr, placeId, contentHash, baseUrl, CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] PublishNewGame thumbnail generation for place {placeId}: {ex}");
            }
        });

        return Redirect($"/ide/PublishAs/upload/{placeId}");
    }

    private async Task ApplyPlaceSettingsAsync(string connStr, long placeId, long userId)
    {
        try
        {
            var maxPlayersStr = Request.Form["NumberOfPlayersMax"].FirstOrDefault() ?? "8";
            var accessStr = Request.Form["Access"].FirstOrDefault() ?? "Everyone";
            var buildersClubOnly = Request.Form["BuildersClubOnly"].Contains("true");
            var arePrivateServersAllowed = Request.Form["ArePrivateServersAllowed"].Contains("true");
            var isFreePrivateServer = Request.Form["IsFreePrivateServer"].FirstOrDefault() == "True";
            var privateServersPriceStr = Request.Form["PrivateServersPrice"].FirstOrDefault() ?? "100";
            var isCopyingAllowed = Request.Form["IsCopyingAllowed"].Contains("true");
            var allowComments = Request.Form["AreCommentsEnabled"].Contains("true");
            var isAllGenresAllowed = Request.Form["IsAllGenresAllowed"].FirstOrDefault() == "True";

            int.TryParse(maxPlayersStr, out var maxPlayers);
            if (maxPlayers < 1) maxPlayers = 8;
            if (maxPlayers > 100) maxPlayers = 100;

            int.TryParse(privateServersPriceStr, out var privateServersPrice);
            if (privateServersPrice < 0) privateServersPrice = 0;

            var accessTypeId = accessStr == "Friends" ? 2 : 1;

            // Collect playable devices
            var deviceNameToId = new Dictionary<string, int>
            {
                { "Computer", 1 }, { "Phone", 2 }, { "Tablet", 3 }, { "Console", 4 }
            };
            var playableDeviceIds = new List<int>();
            for (int i = 0; i < 4; i++)
            {
                var selected = Request.Form[$"PlayableDevices[{i}].Selected"].FirstOrDefault();
                var deviceType = Request.Form[$"PlayableDevices[{i}].DeviceType"].FirstOrDefault();
                if (selected == "true" && !string.IsNullOrWhiteSpace(deviceType) && deviceNameToId.TryGetValue(deviceType, out var deviceId))
                    playableDeviceIds.Add(deviceId);
            }
            if (playableDeviceIds.Count == 0) playableDeviceIds = new List<int> { 1, 2, 3 };

            // Collect allowed gear types
            var allowedGearTypeCategories = new List<string>();
            for (int i = 0; i < 9; i++)
            {
                var selected = Request.Form[$"AllowedGearTypes[{i}].IsSelected"].FirstOrDefault();
                var category = Request.Form[$"AllowedGearTypes[{i}].Category"].FirstOrDefault();
                if (selected == "true" && !string.IsNullOrWhiteSpace(category))
                    allowedGearTypeCategories.Add(category);
            }

            var gearTypeCategoryToId = new Dictionary<string, int>
            {
                { "Melee", 1 }, { "PowerUps", 2 }, { "Ranged", 3 }, { "Navigation", 4 },
                { "Explosive", 5 }, { "Musical", 6 }, { "Social", 7 }, { "PersonalTransport", 8 }, { "Building", 9 }
            };

            var allowedGearTypeIds = allowedGearTypeCategories
                .Where(c => gearTypeCategoryToId.ContainsKey(c))
                .Select(c => gearTypeCategoryToId[c])
                .ToList();

            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync();

            const string sql = @"UPDATE assets SET
                description = @description,
                max_visitor_count = @maxPlayers,
                access_type = @accessType,
                private_servers_allowed = @privateServersAllowed,
                private_servers_free = @privateServersFree,
                private_servers_price = @privateServersPrice,
                device_compatibility = @deviceCompatibility::jsonb,
                is_all_genres_allowed = @isAllGenres,
                allowed_gear_types = @allowedGearTypes::jsonb,
                is_copying_allowed = @isCopyingAllowed,
                allow_comments = @allowComments,
                last_updated = now()
                WHERE asset_id = @placeId AND owner_user_id = @userId";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("description", (object?)(Request.Form["Description"].FirstOrDefault() ?? "") ?? DBNull.Value);
            cmd.Parameters.AddWithValue("maxPlayers", maxPlayers);
            cmd.Parameters.AddWithValue("accessType", accessTypeId);
            cmd.Parameters.AddWithValue("privateServersAllowed", arePrivateServersAllowed);
            cmd.Parameters.AddWithValue("privateServersFree", isFreePrivateServer);
            cmd.Parameters.AddWithValue("privateServersPrice", privateServersPrice);
            cmd.Parameters.AddWithValue("deviceCompatibility", System.Text.Json.JsonSerializer.Serialize(playableDeviceIds));
            cmd.Parameters.AddWithValue("isAllGenres", isAllGenresAllowed);
            cmd.Parameters.AddWithValue("allowedGearTypes", System.Text.Json.JsonSerializer.Serialize(allowedGearTypeIds));
            cmd.Parameters.AddWithValue("isCopyingAllowed", isCopyingAllowed);
            cmd.Parameters.AddWithValue("allowComments", allowComments);
            cmd.Parameters.AddWithValue("placeId", placeId);
            cmd.Parameters.AddWithValue("userId", userId);
            await cmd.ExecuteNonQueryAsync();
        }
        catch
        {
            // Settings application is best-effort; place creation already succeeded
        }
    }

    [HttpGet("publish/listmodels")]
    public async Task<IActionResult> ListModels(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId <= 0) return Unauthorized();

        var connStr = ConnectionString;
        if (string.IsNullOrWhiteSpace(connStr))
            return StatusCode(500, new { error = "Service unavailable" });

        var models = new List<ModelEntry>();

        try
        {
            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync(cancellationToken);

            const string sql = @"SELECT asset_id, name, description, thumbnail_url, created_at
FROM assets
WHERE owner_user_id = @uid AND asset_type_id = 10
ORDER BY created_at DESC";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("uid", userId);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                models.Add(new ModelEntry
                {
                    AssetId = reader.GetInt64(0),
                    Name = reader.IsDBNull(1) ? "Unnamed Model" : reader.GetString(1),
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

        return PartialView("~/Views/IDE/ModelList.cshtml", models);
    }

    [HttpPost("Publish/Model")]
    public async Task<IActionResult> PublishNewModel(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId <= 0) return Unauthorized();

        var connStr = ConnectionString;
        if (string.IsNullOrWhiteSpace(connStr))
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
            AssetTypeId = 10,
            OwnerUserId = userId,
            Description = description,
            ContentHash = "pending",
            FileExtension = ".rbxm",
            ContentType = "application/octet-stream",
            IsCopyingAllowed = isCopyingAllowed
        };

        var assetId = await repo.CreateAssetAsync(connStr, createParams, cancellationToken);

        var userAssetsRepo = new UserAssetsRepository();
        await userAssetsRepo.AddUserAssetAsync(connStr, userId, assetId, cancellationToken);

        if (genreTypeId is >= 0 and <= 20)
            await repo.UpdateAssetGenreAsync(connStr, assetId, genreTypeId, cancellationToken);

        await repo.UpdateAssetAllowCommentsAsync(connStr, assetId, allowComments, cancellationToken);

        return Redirect($"/ide/PublishAs/upload/{assetId}");
    }

    [HttpGet("places/defaultsettings")]
    public IActionResult DefaultSettings()
    {
        var templateId = _configuration["PlaceTemplates:DefaultTemplate"] ?? "95206881";
        return Content($"{{\"DefaultTemplate\":\"{templateId}\"}}", "application/json");
    }

    [HttpPost("places/create")]
    public async Task<IActionResult> CreatePlace([FromQuery] long universeId)
    {
        var userId = GetUserId();
        if (userId <= 0) return Unauthorized();

        var connStr = ConnectionString;
        if (string.IsNullOrWhiteSpace(connStr))
            return StatusCode(500, new { error = "Service unavailable" });

        var ownerId = await GamesRepository.GetUniverseOwnerAsync(connStr, universeId);
        if (ownerId == null || ownerId.Value != userId)
            return StatusCode(403, new { error = "You do not own this universe" });

        using var reader = new System.IO.StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();
        var data = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(body);

        var templatePlaceIdStr = data.TryGetProperty("DefaultTemplate", out var templateEl) ? templateEl.GetString() : null;
        long.TryParse(templatePlaceIdStr, out var templatePlaceId);
        if (templatePlaceId <= 0) templatePlaceId = long.Parse(_configuration["PlaceTemplates:DefaultTemplate"] ?? "95206881");

        var assetsRoot = _configuration["Assets:Directory"] ?? "";
        var templatesDir = Path.Combine(Directory.GetCurrentDirectory(), "Templates");
        var templateMappings = new Dictionary<string, string>();
        foreach (var tmpl in _configuration.GetSection("PlaceTemplates").GetChildren())
        {
            if (tmpl.Key != "DefaultTemplate" && !string.IsNullOrWhiteSpace(tmpl.Value))
                templateMappings[tmpl.Key] = Path.Combine(templatesDir, tmpl.Value);
        }
        var contentHash = await PlacesHandler.ResolveTemplateContentHashAsync(connStr, templatePlaceId, assetsRoot, templateMappings);

        var userName = await Users.UserQueries.GetUserNameByIdAsync(connStr, userId) ?? "Player";
        var nextPlaceNumber = await GamesQueries.GetNextPlaceNumberAsync(userId, connStr);

        var assetsRepo = new Assets.AssetsRepository();
        var placeId = await assetsRepo.CreateAssetAsync(connStr, new Assets.AssetCreateParams
        {
            Name = $"{userName}'s Place Number: {nextPlaceNumber}",
            AssetTypeId = 9,
            OwnerUserId = userId,
            ContentHash = contentHash,
            FileExtension = ".rbxl",
            ContentType = "application/octet-stream",
            IsPlace = true,
            InUniverse = false,
            Description = ""
        });

        var added = await PlacesHandler.AddPlaceToUniverseAsync(connStr, universeId, placeId);
        if (!added)
            return StatusCode(500, new { error = "Failed to add place to universe" });

        _ = Task.Run(async () =>
        {
            try
            {
                var thumbnailService = HttpContext.RequestServices.GetService(typeof(IThumbnailService)) as IThumbnailService;
                if (thumbnailService != null && !string.IsNullOrWhiteSpace(contentHash) && contentHash != "pending-place-content")
                {
                    var baseUrl = _configuration["Thumbnails:ThumbnailUrl"] ?? $"{Request.Scheme}://{Request.Host}/";
                    await PlaceThumbnail.GeneratePlaceThumbnailAsync(thumbnailService, connStr, placeId, contentHash, baseUrl, placeName: $"{userName}'s Place", cancellationToken: CancellationToken.None);
                    await PlaceThumbnail.GenerateAutoGeneratedThumbnailAsync(thumbnailService, connStr, placeId, contentHash, baseUrl, CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] PublishNewPlace thumbnail generation for place {placeId}: {ex}");
            }
        });

        return Ok(new { placeId });
    }

    [HttpPost("places/{placeId:long}/updatesettings")]
    public async Task<IActionResult> UpdatePlaceSettings(long placeId)
    {
        var userId = GetUserId();
        if (userId <= 0) return Unauthorized();

        var connStr = ConnectionString;
        if (string.IsNullOrWhiteSpace(connStr))
            return StatusCode(500);

        try
        {
            using var reader = new System.IO.StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            var data = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(body);

            var name = SanitizeString(data.TryGetProperty("Name", out var nameEl) ? nameEl.GetString() ?? "" : "", 80);
            var description = SanitizeString(data.TryGetProperty("Description", out var descEl) ? descEl.GetString() ?? "" : "", 1000);
            var maxPlayersStr = data.TryGetProperty("MaxPlayers", out var mpEl) ? mpEl.GetString() : "0";
            var priceStr = data.TryGetProperty("Price", out var priceEl) ? priceEl.GetString() : "0";
            var isForSale = data.TryGetProperty("IsForSale", out var saleEl) && saleEl.GetBoolean();

            if (!int.TryParse(maxPlayersStr, out var maxPlayers) || maxPlayers < 1)
                return StatusCode(400, new { error = "Invalid MaxPlayers value" });
            if (!long.TryParse(priceStr, out var price) || price < 0)
                return StatusCode(400, new { error = "Invalid Price value" });
            if (string.IsNullOrWhiteSpace(name))
                return StatusCode(400, new { error = "Place name is required" });

            maxPlayers = Math.Clamp(maxPlayers, 1, 100);
            price = Math.Max(0, price);

            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync();

            await using var ownerCmd = new NpgsqlCommand(
                "SELECT owner_user_id FROM assets WHERE asset_id = @pid AND is_place = true", conn);
            ownerCmd.Parameters.AddWithValue("pid", placeId);
            var ownerId = await ownerCmd.ExecuteScalarAsync();
            if (ownerId == null || ownerId == DBNull.Value || (long)ownerId != userId)
                return StatusCode(403);

            const string sql = @"UPDATE assets 
                SET name = @name,
                    description = @description,
                    max_visitor_count = @maxPlayers,
                    price = @price,
                    on_sale = @isForSale,
                    last_updated = now()
                WHERE asset_id = @pid";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("name", name);
            cmd.Parameters.AddWithValue("description", string.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description);
            cmd.Parameters.AddWithValue("maxPlayers", maxPlayers);
            cmd.Parameters.AddWithValue("price", price);
            cmd.Parameters.AddWithValue("isForSale", isForSale);
            cmd.Parameters.AddWithValue("pid", placeId);
            await cmd.ExecuteNonQueryAsync();

            return Ok(new { success = true, placeId = placeId });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("insertasset")]
    public async Task<IActionResult> InsertAsset([FromBody] InsertAssetRequest request)
    {
        try
        {
            if (request == null || request.assetId <= 0)
                return BadRequest(new { error = "Invalid request" });

            var userId = GetUserId();
            if (userId <= 0)
                return Unauthorized(new { error = "Not authenticated" });

            var connStr = ConnectionString;
            if (string.IsNullOrWhiteSpace(connStr))
                return StatusCode(500, new { error = "Database not configured" });

            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync();

            const string sql = @"INSERT INTO user_assets (user_id, asset_id, created_at)
                                 SELECT @uid, @aid, now()
                                 WHERE NOT EXISTS (
                                     SELECT 1 FROM user_assets WHERE user_id = @uid AND asset_id = @aid
                                 )";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("uid", userId);
            cmd.Parameters.AddWithValue("aid", request.assetId);
            await cmd.ExecuteNonQueryAsync();

            using var limitCmd = new NpgsqlCommand(
                "SELECT limited_unique FROM assets WHERE asset_id = @aid", conn);
            limitCmd.Parameters.AddWithValue("aid", request.assetId);
            var limitObj = await limitCmd.ExecuteScalarAsync();
            if (limitObj is bool isLimitedUnique && isLimitedUnique)
            {
                var limitedService = new Economy.LimitedItemService();
                await using var tx = await conn.BeginTransactionAsync();
                try
                {
                    await limitedService.AssignSerialNumberAsync(conn, tx, request.assetId, userId);
                    await tx.CommitAsync();
                }
                catch { await tx.RollbackAsync(); }
            }

            return Ok(new { success = true });
        }
        catch
        {
            return Ok(new { success = false });
        }
    }

    [HttpGet("toolbox/items")]
    [AllowAnonymous]
    public async Task<IActionResult> GetToolboxItems(
        [FromQuery] string? category = null,
        [FromQuery] string? keyword = null,
        [FromQuery] int num = 30,
        [FromQuery] int page = 1,
        [FromQuery] string? sort = null,
        [FromQuery] long? creatorId = null,
        [FromQuery] string? creatorType = null,
        [FromQuery] long? userId = null,
        [FromQuery] long? groupId = null)
    {
        try
        {
            var typeId = category?.ToLowerInvariant() switch
            {
                "freemodels" or "mymodels" or "recentmodels" => 10,
                "freedecals" or "mydecals" or "recentdecals" => 13,
                "freemeshes" or "mymeshes" or "recentmeshes" => 4,
                "freeaudio" or "myaudio" or "recentaudio" => 3,
                _ => (int?)null
            };

            var filterUserId = userId ?? creatorId;
            var currentUserId = GetUserId();

            var (results, totalCount) = await _toolboxService.SearchToolboxAsync(
                typeId, keyword, sort, page, num, filterUserId, groupId, currentUserId);

            var responseResults = results.Select(r => new
            {
                Asset = new
                {
                    Id = r.AssetId,
                    Name = r.Name,
                    TypeId = r.AssetTypeId,
                    IsEndorsed = false
                },
                Creator = new
                {
                    Id = r.OwnerUserId,
                    Name = r.OwnerName,
                    Type = 1
                },
                Thumbnail = new
                {
                    Final = true,
                    Url = r.ThumbnailUrl,
                    RetryUrl = (string?)null,
                    UserId = 0L,
                    EndpointType = "Avatar"
                },
                Voting = new
                {
                    ShowVotes = true,
                    UpVotes = r.Upvotes,
                    DownVotes = r.Downvotes,
                    CanVote = r.IsOwnedByCurrentUser,
                    UserVote = (bool?)null,
                    HasVoted = false,
                    ReasonForNotVoteable = r.IsOwnedByCurrentUser ? "" : "InvalidAssetOrUser"
                }
            }).ToList();

            return Ok(new { Results = responseResults, TotalResults = totalCount });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    private static string SanitizeString(string input, int maxLength)
    {
        if (string.IsNullOrEmpty(input)) return "";
        var result = new System.Text.StringBuilder(input.Length);
        foreach (var c in input)
        {
            if (c >= 32 || c == '\t' || c == '\n' || c == '\r')
                result.Append(c);
        }
        return result.ToString().Trim().Length > maxLength
            ? result.ToString().Trim()[..maxLength]
            : result.ToString().Trim();
    }

    private static async Task<List<PlaceEntry>> GetUserPlacesAsync(string connStr, long userId, int offset, int limit, CancellationToken ct)
    {
        var results = new List<PlaceEntry>();

        await using var conn = new NpgsqlConnection(connStr);
        await conn.OpenAsync(ct).ConfigureAwait(false);

        const string sql = @"SELECT asset_id, name, thumbnail_url
FROM assets
WHERE owner_user_id = @uid AND is_place = true
ORDER BY created_at DESC, asset_id DESC
OFFSET @off LIMIT @lim;";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("uid", userId);
        cmd.Parameters.AddWithValue("off", offset);
        cmd.Parameters.AddWithValue("lim", limit);

        await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        while (await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            results.Add(new PlaceEntry
            {
                PlaceId = reader.GetInt64(0),
                Name = reader.IsDBNull(1) ? "Unnamed Place" : reader.GetString(1),
                ThumbnailUrl = reader.IsDBNull(2) ? null : reader.GetString(2),
            });
        }

        return results;
    }
}

public sealed class PlaceEntry
{
    public long PlaceId { get; set; }
    public string Name { get; set; } = "Unnamed Place";
    public string? ThumbnailUrl { get; set; }
}

public sealed class ModelEntry
{
    public long AssetId { get; set; }
    public string Name { get; set; } = "Unnamed Model";
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class InsertAssetRequest
{
    public long assetId { get; set; }
}
