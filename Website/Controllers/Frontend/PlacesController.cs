using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Data;
using Assets;
using System.Security.Claims;
using Thumbnails;
using Npgsql;
using Common;
using System.Linq;
using Games;
using System.Text.Json;
using Webserver.Common;
using Users;
using RobloxWebserver.Models;

namespace RobloxWebserver.Controllers
{
    /// <summary>
    /// Controller for place (per-place) management endpoints.
    /// </summary>
    [ApiController]
    [Authorize]
    public sealed class PlacesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly AssetMetadataRepository _assetRepository;
        private readonly IThumbnailService _thumbnailService;
        private readonly PlaceTemplateMapping _templateMapping;

        public PlacesController(AppDbContext context, IConfiguration configuration, AssetMetadataRepository assetRepository, IThumbnailService thumbnailService)
        {
            _context = context;
            _configuration = configuration;
            _assetRepository = assetRepository;
            _thumbnailService = thumbnailService;
            _templateMapping = new PlaceTemplateMapping();
            var templatesSection = configuration.GetSection("PlaceTemplates");
            templatesSection.Bind(_templateMapping.Templates);
        }

        /// <summary>
        /// GET /places/create - Show place creation page
        /// </summary>
        [HttpGet("places/create")]
        [Authorize]
        public async Task<IActionResult> CreatePlace(CancellationToken cancellationToken)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Redirect("/login");
                }

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                
                var creatorUserName = await UserQueries.GetUserNameByIdAsync(connectionString, currentUserId, cancellationToken)
                    .ConfigureAwait(false) ?? User.Identity?.Name ?? "Player";
                int nextPlaceNumber = await GamesQueries.GetNextPlaceNumberAsync(currentUserId, connectionString, cancellationToken);
                ViewBag.UserName = creatorUserName;
                ViewBag.NextPlaceNumber = nextPlaceNumber;
                if (TempData["Error"] != null)
                {
                    ViewBag.ErrorMessage = TempData["Error"].ToString();
                }

                return View("~/Views/Pages/places/create.cshtml");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return Redirect("/develop?Page=universes");
            }
        }

        /// <summary>
        /// POST /places/create - Handle place creation form submission
        /// </summary>
        [HttpPost("places/create")]
        [Authorize]
        public async Task<IActionResult> CreatePlacePost(CancellationToken cancellationToken)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Redirect("/login");
                }

                var name = Request.Form["Name"].FirstOrDefault() ?? "";
                var description = Request.Form["Description"].FirstOrDefault() ?? "";
                var genre = Request.Form["Genre"].FirstOrDefault() ?? "All";
                var templateIdStr = Request.Form["TemplateID"].FirstOrDefault() ?? "";

                foreach (var key in Request.Form.Keys)
                {
                    var values = Request.Form[key];
                }

                if (string.IsNullOrWhiteSpace(name))
                {
                    TempData["Error"] = "Place name is required";
                    return RedirectToAction("CreatePlace");
                }

                if (name.Length > 50)
                {
                    TempData["Error"] = "Place name cannot exceed 50 characters";
                    return RedirectToAction("CreatePlace");
                }

                if (name.Contains("<") || name.Contains(">"))
                {
                    TempData["Error"] = "Place name contains illegal characters";
                    return RedirectToAction("CreatePlace");
                }

                // Additional security checks for name
                if (name.Contains("javascript:") || name.Contains("vbscript:") || name.Contains("onload=") || name.Contains("onerror="))
                {
                    TempData["Error"] = "Place name contains invalid content";
                    return RedirectToAction("CreatePlace");
                }

                // Description validation
                if (description.Length > 1000)
                {
                    TempData["Error"] = "Description cannot exceed 1000 characters";
                    return RedirectToAction("CreatePlace");
                }

                // Security checks for description
                if (description.Contains("<script") || description.Contains("javascript:") || description.Contains("vbscript:") || 
                    description.Contains("onload=") || description.Contains("onerror=") || description.Contains("onclick="))
                {
                    TempData["Error"] = "Description contains invalid content";
                    return RedirectToAction("CreatePlace");
                }

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var assetsRoot = _configuration["Assets:Directory"];
                var starterPlacePath = _configuration["Games:StarterPlacePath"];
                var enableCooldownRaw = _configuration["Games:EnableCreationCooldown"];
                var enableCooldown = true;
                if (!string.IsNullOrWhiteSpace(enableCooldownRaw) && bool.TryParse(enableCooldownRaw, out var parsedCooldown))
                {
                    enableCooldown = parsedCooldown;
                }

                var creatorUserName = await UserQueries.GetUserNameByIdAsync(connectionString, currentUserId, cancellationToken)
                    .ConfigureAwait(false) ?? User.Identity?.Name ?? "Player";
                string? templateContentHash = null;
                if (!string.IsNullOrWhiteSpace(templateIdStr))
                {
                    templateContentHash = await ProcessTemplateFileForCreationAsync(templateIdStr, connectionString, cancellationToken);
                }

                var universe = await GameCreationService.CreateUniverseWithRootPlaceAsync(
                    connectionString,
                    currentUserId,
                    creatorUserName,
                    assetsRoot,
                    starterPlacePath,
                    enableCooldown,
                    _thumbnailService,
                    _configuration,
                    cancellationToken,
                    customName: name,
                    templateContentHash: templateContentHash);

                if (!string.IsNullOrWhiteSpace(templateContentHash))
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var baseUrl = _configuration["Thumbnails:ThumbnailUrl"] ?? $"{Request.Scheme}://{Request.Host}/";
                            
                            await PlaceThumbnail.GeneratePlaceThumbnailAsync(
                                _thumbnailService,
                                connectionString,
                                universe.RootPlaceId,
                                templateContentHash,
                                baseUrl,
                                placeName: name,
                                cancellationToken: CancellationToken.None);

                            await PlaceThumbnail.GenerateAutoGeneratedThumbnailAsync(
                                _thumbnailService,
                                connectionString,
                                universe.RootPlaceId,
                                templateContentHash,
                                baseUrl,
                                CancellationToken.None);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to generate thumbnails for template place {universe.RootPlaceId}: {ex.Message}");
                        }
                    });
                }

                await UpdatePlaceSettingsFromFormAsync(universe.RootPlaceId, connectionString, description, genre, cancellationToken);
                await GameCreationService.UpdateUniverseNameAsync(universe.UniverseId, name, connectionString, cancellationToken);

                return Redirect("/develop?Page=universes");
            }
            catch (Exception ex)
            {
                return Redirect("/develop?Page=universes");
            }
        }

        /// <summary>
        /// Helper function to update place settings from form data
        /// </summary>
        private async Task UpdatePlaceSettingsFromFormAsync(long placeId, string connectionString, string description, string genre, CancellationToken cancellationToken)
        {
            description ??= "";
            genre ??= "All";

            int maxPlayers = 99;
            if (int.TryParse(Request.Form["NumberOfPlayersMax"].FirstOrDefault(), out var parsedMax))
            {
                maxPlayers = Math.Max(1, Math.Min(100, parsedMax));
            }

            var serverFillType = Request.Form["SocialSlotType"].FirstOrDefault() ?? "Automatic";
            int customSocialSlots = 4;
            if (int.TryParse(Request.Form["NumberOfCustomSocialSlots"].FirstOrDefault(), out var parsedSlots))
            {
                customSocialSlots = parsedSlots;
            }

            var accessType = Request.Form["Access"].FirstOrDefault() ?? "Everyone";
            bool privateServersAllowed = Request.Form["ArePrivateServersAllowed"].FirstOrDefault() == "true";
            bool privateServersFree = Request.Form["IsFreePrivateServer"].FirstOrDefault() == "True";
            int privateServersPrice = 100;
            if (int.TryParse(Request.Form["PrivateServersPrice"].FirstOrDefault(), out var parsedPrice))
            {
                privateServersPrice = Math.Max(10, parsedPrice);
            }

            var deviceCompatibility = new List<int>();
            var deviceTypes = new[] { "Computer", "Phone", "Tablet", "Console" };
            for (int i = 0; i < deviceTypes.Length; i++)
            {
                var selected = Request.Form[$"PlayableDevices[{i}].Selected"].FirstOrDefault() == "true";
                if (selected)
                {
                    deviceCompatibility.Add(i + 1); // 1=Computer, 2=Phone, 3=Tablet, 4=Console
                }
            }

            bool isAllGenresAllowed = Request.Form["IsAllGenresAllowed"].FirstOrDefault() == "True";
            var allowedGearTypes = new List<string>();
            for (int i = 0; i < 9; i++)
            {
                var isSelected = Request.Form[$"AllowedGearTypes[{i}].IsSelected"].FirstOrDefault() == "true";
                if (isSelected)
                {
                    var category = Request.Form[$"AllowedGearTypes[{i}].Category"].FirstOrDefault() ?? "";
                    if (!string.IsNullOrEmpty(category))
                    {
                        allowedGearTypes.Add(category);
                    }
                }
            }

            bool isCopyingAllowed = Request.Form["IsCopyingAllowed"].FirstOrDefault() == "true";
            await GameCreationService.UpdatePlaceSettingsAsync(
                placeId,
                connectionString,
                description,
                genre,
                maxPlayers,
                serverFillType,
                customSocialSlots,
                accessType,
                privateServersAllowed,
                privateServersFree,
                privateServersPrice,
                deviceCompatibility,
                isAllGenresAllowed,
                allowedGearTypes,
                isCopyingAllowed,
                cancellationToken);
        }


        /// <summary>
        /// Helper function to process template file and upload as asset before place creation
        /// </summary>
        private async Task<string?> ProcessTemplateFileForCreationAsync(string templateId, string connectionString, CancellationToken cancellationToken)
        {
            try
            {
                if (!_templateMapping.HasTemplate(templateId))
                {
                    return null;
                }

                var templatesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Templates");
                var templatePath = _templateMapping.GetTemplatePath(templateId, templatesDirectory);
                
                if (string.IsNullOrEmpty(templatePath) || !System.IO.File.Exists(templatePath))
                {
                    return null;
                }

                var assetsRoot = _configuration["Assets:Directory"];
                if (string.IsNullOrWhiteSpace(assetsRoot))
                {
                    return null;
                }

                return TemplateHelper.ProcessAndSaveTemplateAsync(templatePath, assetsRoot, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to process template file for creation: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Helper function to process template file and upload as asset (legacy method for post-creation updates)
        /// </summary>
        private async Task<string?> ProcessTemplateFileAsync(string templateId, long placeId, string connectionString, CancellationToken cancellationToken)
        {
            try
            {
                if (!_templateMapping.HasTemplate(templateId))
                {
                    return null;
                }

                var templatesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Templates");
                var templatePath = _templateMapping.GetTemplatePath(templateId, templatesDirectory);
                
                if (string.IsNullOrEmpty(templatePath) || !System.IO.File.Exists(templatePath))
                {
                    return null;
                }

                var assetsRoot = _configuration["Assets:Directory"];
                if (string.IsNullOrWhiteSpace(assetsRoot))
                {
                    return null;
                }

                var contentHash = TemplateHelper.ProcessAndSaveTemplateAsync(templatePath, assetsRoot, cancellationToken);
                if (string.IsNullOrEmpty(contentHash))
                {
                    return null;
                }

                await GamesQueries.UpdatePlaceAssetContentHashAsync(placeId, contentHash, connectionString, cancellationToken);

                return contentHash;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// </summary>
        [HttpGet("places/{id}/update")]
        public async Task<IActionResult> UpdatePlace(long id)
        {
            try
            {
                var successMessage = Request.Query["success"].FirstOrDefault();
                if (!string.IsNullOrEmpty(successMessage))
                {
                    ViewBag.SuccessMessage = successMessage;
                }

                var tabParam = Request.Query["tab"].FirstOrDefault();
                if (!string.IsNullOrEmpty(tabParam))
                {
                    ViewBag.ActiveTab = tabParam;
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Redirect("/login");
                }

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var isValidPlace = await PlaceValidationHelper.ValidatePlaceOwnershipAsync(id, currentUserId, connectionString, _assetRepository);
                if (!isValidPlace)
                {
                    return Redirect("/404");
                }

                var placeAsset = await _assetRepository.GetAssetByIdAsync(connectionString, id);
                if (placeAsset == null || placeAsset.AssetTypeId != 9)
                {
                    return Redirect($"/my/item.aspx?id={id}");
                }

                var universeId = await GamesRepository.GetUniverseIdFromPlaceIdAsync(connectionString, id);


                ViewBag.gameid = placeAsset.AssetId;
                ViewBag.PlaceUniverseId = universeId;
                ViewBag.gamename = placeAsset.Name ?? "";
                ViewBag.gamedesc = placeAsset.Description ?? "";
                ViewBag.gamegenre = AssetGenreNames.GetGenreLabel(placeAsset.Genre);

                var hasCustomIcon = await _thumbnailService.HasCustomIconAsync(id, connectionString);
                if (!hasCustomIcon && !string.IsNullOrWhiteSpace(placeAsset.PlaceCustomIconUrl))
                {
                    hasCustomIcon = true;
                }
                else if (hasCustomIcon && string.IsNullOrWhiteSpace(placeAsset.PlaceCustomIconUrl))
                {
                    hasCustomIcon = false;
                }
                
                ViewBag.hasCustomIcon = hasCustomIcon;
                
                bool customIconIsThumbnail = !string.IsNullOrWhiteSpace(placeAsset.PlaceCustomIconUrl) && 
                                          placeAsset.PlaceCustomIconUrl == placeAsset.ThumbnailUrl;
                ViewBag.customIconIsThumbnail = customIconIsThumbnail;
                
                ViewBag.iconUrl = placeAsset.PlaceCustomIconUrl ?? 
                                 placeAsset.PlaceGeneratedIconUrl ?? 
                                 placeAsset.ThumbnailUrl ?? 
                                 $"/game-icons/image?assetId={id}&width=512&height=512&format=Png";
                ViewBag.hasGeneratedIcon = placeAsset.GeneratedIcon;
                ViewBag.thumbnailUrl = placeAsset.ThumbnailUrl;
                ViewBag.hasAutoGeneratedThumbnail = placeAsset.PlaceAutoGeneratedThumbnail;
                ViewBag.placeGeneratedThumbnailUrl = placeAsset.PlaceGeneratedThumbnailUrl;
                ViewBag.hasCustomThumbnail = placeAsset.PlaceCustomThumbnail;
                ViewBag.hasVideoThumbnail = placeAsset.PlaceVideoThumbnail;
                ViewBag.maxVisitorCount = placeAsset.MaxVisitorCount;
                var playableDevices = Games.DeviceCompatibilityHelper.ConvertFromDeviceCompatibilityJson(placeAsset.DeviceCompatibility ?? "[1, 2, 3]");
                ViewBag.playableDevices = playableDevices;
                ViewBag.serverFillType = Games.AccessSettingsChanger.GetServerFillTypeString(placeAsset.ServerFillType);
                ViewBag.numberOfCustomSocialSlots = placeAsset.NumberOfCustomSocialSlots;
                ViewBag.accessType = Games.AccessSettingsChanger.GetAccessTypeString(placeAsset.AccessType);
                ViewBag.privateServersAllowed = placeAsset.PrivateServersAllowed;
                ViewBag.privateServersFree = placeAsset.IsPrivateServersFree;
                ViewBag.privateServersPrice = placeAsset.PrivateServersPrice;
                ViewBag.paidAccessEnabled = placeAsset.PaidAccessEnabled;
                ViewBag.paidAccessPrice = placeAsset.PaidAccessPrice;
                ViewBag.isCopyingAllowed = placeAsset.IsCopyingAllowed;
                ViewBag.isAllGenresAllowed = placeAsset.IsAllGenresAllowed;
                ViewBag.allowedGearTypes = placeAsset.AllowedGearTypes;
                ViewBag.allowPlaceToBeCopiedInGame = placeAsset.AllowPlaceToBeCopiedInGame;
                ViewBag.allowPlaceToBeUpdatedInGame = placeAsset.AllowPlaceToBeUpdatedInGame;
                var thumbnailData = await PlaceThumbnail.GetAllPlaceThumbnailsAsync(connectionString, id);
                
                ViewBag.placeThumbnails = thumbnailData;
                ViewBag.thumbnailCount = thumbnailData.Count;

                return View("~/Views/Pages/places/{id}/update.cshtml");
            }
            catch (Exception ex)
            {
                return Redirect("/404");
            }
        }

        /// <summary>
        /// POST /places/thumbnails/add-generated - Generate auto-generated thumbnail for a place
        /// </summary>
        [HttpPost("places/thumbnails/add-generated")]
        [Authorize]
        public async Task<IActionResult> AddGeneratedThumbnail()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var placeIdStr = Request.Form["placeId"].FirstOrDefault() ?? Request.Form["Id"].FirstOrDefault();
                if (string.IsNullOrWhiteSpace(placeIdStr) || !long.TryParse(placeIdStr, out var placeId))
                {
                    return Json(new { success = false, message = "Invalid place ID" });
                }

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var placeAsset = await _assetRepository.GetAssetByIdAsync(connectionString, placeId);
                
                if (placeAsset == null)
                {
                    return Json(new { success = false, message = "Place not found" });
                }

                if (placeAsset.OwnerUserId != currentUserId)
                {
                    return Json(new { success = false, message = "Access denied" });
                }

                var baseUrl = _configuration["Thumbnails:ThumbnailUrl"] ?? $"{Request.Scheme}://{Request.Host}";
                
                try
                {
                    var existingThumbnails = await PlaceThumbnail.GetPlaceThumbnailsAsync(connectionString, placeId);
                    var existingAutoGeneratedThumbnail = existingThumbnails.FirstOrDefault(t => 
                        t.GetType().GetProperty("type")?.GetValue(t)?.ToString() == "image" && 
                        t.GetType().GetProperty("altText")?.GetValue(t)?.ToString() == "Auto-generated Thumbnail");
                    
                    string? thumbnailUrl = null;
                    
                    if (existingAutoGeneratedThumbnail != null)
                    {
                        var urlProperty = existingAutoGeneratedThumbnail.GetType().GetProperty("url");
                        thumbnailUrl = urlProperty?.GetValue(existingAutoGeneratedThumbnail)?.ToString();
                    }
                    else
                    {
                        var baseUrlForTask = baseUrl;
                        var contentHashForTask = placeAsset.ContentHash ?? string.Empty;
                        _ = Task.Run(async () => {
                            try
                            {
                                var renderedResult1280x720 = await _thumbnailService.RenderPlaceAsync(
                                    placeId, 
                                    x: 1280, 
                                    y: 720, 
                                    connectionString: connectionString, 
                                    placeAssetHash: contentHashForTask);
                                
                                var cdnPlaceThumbnailsPath = CDNUtilities.GetCDNAssetsPath("place-thumbnails");
                                var sourcePath1280x720 = Path.Combine(renderedResult1280x720.FullPath);
                                var cdnThumbnailPath1280x720 = Path.Combine(cdnPlaceThumbnailsPath, renderedResult1280x720.FileName);
                                bool thumbnail1280x720Copied = CDNUtilities.SafeFileCopy(sourcePath1280x720, cdnThumbnailPath1280x720);
                                
                                
                                if (thumbnail1280x720Copied)
                                {
                                    var generatedThumbnailUrl = CDNUtilities.GeneratePlaceThumbnailUrl(baseUrlForTask, renderedResult1280x720.FileName);
                                    
                                    await ThumbnailQueries.ClearPlaceCustomThumbnailAsync(connectionString, placeId);
                                    await ThumbnailQueries.ClearPlaceVideoThumbnailAsync(connectionString, placeId);
                                    
                                    await PlaceThumbnail.GenerateAutoGeneratedThumbnailAsync(
                                        _thumbnailService,
                                        connectionString,
                                        placeId,
                                        contentHashForTask,
                                        baseUrlForTask);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        });
                        
                        thumbnailUrl = "/images/RobloxLogo.png";
                    }

                    return Json(new { 
                        success = true, 
                        message = "Auto-generated thumbnail created successfully",
                        thumbnailUrl = thumbnailUrl ?? "/images/RobloxLogo.png"
                    });
                }
                catch (Exception ex)
                {
                    return Json(new { 
                        success = false, 
                        message = $"Failed to generate thumbnail: {ex.Message}" 
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred while generating the thumbnail: {ex.Message}" });
            }
        }

        /// <summary>
        /// POST /places/icons/add-generated-image - Handle generated icon selection for a place
        /// </summary>
        [HttpPost("places/icons/add-generated-image")]
        [Authorize]
        public async Task<IActionResult> AddGeneratedIcon([FromForm] bool force = false)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var placeIdStr = Request.Form["placeId"].FirstOrDefault() ?? Request.Form["Id"].FirstOrDefault();
                if (string.IsNullOrWhiteSpace(placeIdStr) || !long.TryParse(placeIdStr, out var placeId))
                {
                    return Json(new { success = false, message = "Invalid place ID" });
                }

                var forceStr = Request.Form["force"].FirstOrDefault();
                force = !string.IsNullOrWhiteSpace(forceStr) && (forceStr.ToLower() == "true" || forceStr == "1");

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var placeAsset = await _assetRepository.GetAssetByIdAsync(connectionString, placeId);
                
                if (placeAsset == null)
                {
                    return Json(new { success = false, message = "Place not found" });
                }

                if (placeAsset.OwnerUserId != currentUserId)
                {
                    return Json(new { success = false, message = "Access denied" });
                }

                if (!force && placeAsset != null && !string.IsNullOrWhiteSpace(placeAsset.PlaceGeneratedIconUrl))
                {
                    var generatedIconUrl = placeAsset.PlaceGeneratedIconUrl;
                    await ThumbnailQueries.SetPlaceGeneratedIconFlagsAsync(connectionString, placeId, generatedIconUrl, placeAsset.PlaceGeneratedIconHighResUrl ?? generatedIconUrl, placeAsset.PlaceGeneratedIconHash ?? "");
                    

                    return Json(new { 
                        success = true, 
                        message = "Generated icon set successfully",
                        iconUrl = generatedIconUrl,
                        iconHash = placeAsset.PlaceGeneratedIconHash ?? ""
                    });
                }

                var baseUrl = _configuration["Thumbnails:ThumbnailUrl"];
                if (string.IsNullOrWhiteSpace(baseUrl))
                {
                    baseUrl = $"{Request.Scheme}://{Request.Host}/";
                }
                
                try
                {
                    var baseUrlForTask = baseUrl;
                    _ = Task.Run(async () => {
                        try
                        {
                            await PlaceThumbnail.GeneratePlaceThumbnailAsync(
                                _thumbnailService,
                                connectionString,
                                placeId,
                                placeAsset.ContentHash,
                                baseUrlForTask);
                        }
                        catch (Exception ex)
                        {
                        }
                    });

                    var updatedAsset = await _assetRepository.GetAssetByIdAsync(connectionString, placeId);
                    var iconUrl = updatedAsset?.PlaceGeneratedIconUrl ?? "/images/RobloxLogo.png";
                    var iconHash = updatedAsset?.PlaceGeneratedIconHash ?? "";
                    
                    if (string.IsNullOrEmpty(iconUrl))
                    {
                        iconUrl = updatedAsset?.ThumbnailUrl ?? $"{baseUrl}thumbnails/default.png";
                    }
                    

                    return Json(new { 
                        success = true, 
                        message = "Generated icon set successfully",
                        iconUrl = iconUrl,
                    });
                }
                catch (Exception ex)
                {
                    return Json(new { 
                        success = false, 
                        message = $"Failed to generate icon: {ex.Message}. Please try again." 
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while setting the icon. Please try again." });
            }
        }


        /// <summary>
        /// POST /places/icons/add-icon - Handle custom icon upload for a place
        /// </summary>
        [HttpPost("places/icons/add-icon")]
        [Authorize]
        public async Task<IActionResult> AddCustomIcon(IFormFile iconImageFile)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                if (iconImageFile == null || iconImageFile.Length == 0)
                {
                    return Json(new { success = false, message = "No file uploaded" });
                }

                if (!iconImageFile.ContentType.StartsWith("image/"))
                {
                    return Json(new { success = false, message = "Invalid file type. Please upload an image file." });
                }

                var placeIdStr = Request.Form["placeId"].FirstOrDefault() ?? Request.Query["placeId"].FirstOrDefault();
                if (string.IsNullOrWhiteSpace(placeIdStr) || !long.TryParse(placeIdStr, out var placeId))
                {
                    return Json(new { success = false, message = "Invalid place ID" });
                }

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var placeAsset = await _assetRepository.GetAssetByIdAsync(connectionString, placeId);
                
                if (placeAsset == null)
                {
                    return Json(new { success = false, message = "Place not found" });
                }

                if (placeAsset.OwnerUserId != currentUserId)
                {
                    return Json(new { success = false, message = "Access denied" });
                }

                var baseUrl = _configuration["Thumbnails:ThumbnailUrl"];
                if (string.IsNullOrWhiteSpace(baseUrl))
                {
                    baseUrl = $"{Request.Scheme}://{Request.Host}";
                }
                
                var (iconUrl, iconUrlHighRes, fileHash) = await PlaceThumbnail.ProcessCustomIconAsync(
                    placeId, iconImageFile.OpenReadStream(), iconImageFile.FileName, iconImageFile.ContentType, baseUrl);
                await PlaceThumbnail.SetPlaceCustomIconAsync(connectionString, placeId, iconUrl, iconUrlHighRes, fileHash);

                return Json(new { 
                    success = true, 
                    message = "Icon uploaded successfully",
                    iconUrl = iconUrl,
                    iconHash = fileHash
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while uploading the icon. Please try again." });
            }
        }


        /// <summary>
        /// POST /places/icons/remove-icon - Handle icon removal for a place
        /// </summary>
        [HttpPost("places/icons/remove-icon")]
        [Authorize]
        public async Task<IActionResult> RemoveIcon()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var placeIdStr = Request.Form["placeId"].FirstOrDefault() ?? Request.Form["Id"].FirstOrDefault();
                if (string.IsNullOrWhiteSpace(placeIdStr) || !long.TryParse(placeIdStr, out var placeId))
                {
                    return Json(new { success = false, message = "Invalid place ID" });
                }

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var placeAsset = await _assetRepository.GetAssetByIdAsync(connectionString, placeId);
                
                if (placeAsset == null)
                {
                    return Json(new { success = false, message = "Place not found" });
                }

                if (placeAsset.OwnerUserId != currentUserId)
                {
                    return Json(new { success = false, message = "Access denied" });
                }

                bool shouldClearThumbnail = !string.IsNullOrWhiteSpace(placeAsset.PlaceCustomIconUrl) && 
                                       placeAsset.PlaceCustomIconUrl == placeAsset.ThumbnailUrl;
                var placeholderUrl = "/images/icons/default-place-icon.svg";
                await PlaceThumbnail.ClearPlaceIconAsync(connectionString, placeId, placeholderUrl, shouldClearThumbnail);

                return Json(new { 
                    success = true, 
                    message = "Icon removed successfully",
                    iconUrl = placeholderUrl,
                    clearedThumbnail = shouldClearThumbnail
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while removing the icon. Please try again." });
            }
        }


        /// <summary>
        /// GET /places/{id}/thumbnail - Get current thumbnail URL for a place
        /// </summary>
        [HttpGet("places/{id}/thumbnail")]
        [Authorize]
        public async Task<IActionResult> GetPlaceThumbnail(long id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Unauthorized(new { error = "User not authenticated" });
                }

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var placeAsset = await _assetRepository.GetAssetByIdAsync(connectionString, id);
                
                if (placeAsset == null)
                {
                    return NotFound(new { error = "Place not found" });
                }

                if (placeAsset.OwnerUserId != currentUserId)
                {
                    return Unauthorized(new { error = "Access denied" });
                }

                return Ok(new { 
                    thumbnailUrl = placeAsset.ThumbnailUrl,
                    customIcon = placeAsset.CustomIcon,
                    placeCustomIconUrl = placeAsset.PlaceCustomIconUrl,
                    placeCustomIconHash = placeAsset.PlaceCustomIconHash,
                    generatedIcon = placeAsset.GeneratedIcon,
                    placeGeneratedIconUrl = placeAsset.PlaceGeneratedIconUrl,
                    placeGeneratedIconHash = placeAsset.PlaceGeneratedIconHash
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while retrieving thumbnail information" });
            }
        }

        /// <summary>
        /// GET /places/{id}/thumbnails - Get all thumbnails for a place
        /// </summary>
        [HttpGet("places/{id}/thumbnails")]
        [Authorize]
        public async Task<IActionResult> GetPlaceThumbnails(long id)
        {
            try
            {
                var thumbnails = new object[] { }; // Placeholder - empty array for now
                
                return Ok(new { 
                    success = true,
                    thumbnails = thumbnails,
                    count = thumbnails.Length
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false, 
                    error = "An error occurred while retrieving thumbnails" 
                });
            }
        }

        /// <summary>
        /// GET /places/{id}/votes - Get vote counts for a place
        /// </summary>
        [HttpGet("places/{id}/votes")]
        public async Task<IActionResult> GetPlaceVotes(long id)
        {
            try
            {
                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var votingService = new VotingService(connectionString);
                
                var (upvotes, downvotes) = await votingService.GetAssetVotesAsync(id);
                
                return Ok(new 
                { 
                    success = true,
                    data = new 
                    { 
                        assetId = id,
                        upVotes = upvotes,
                        downVotes = downvotes,
                        totalVotes = upvotes + downvotes,
                        voteRatio = (upvotes + downvotes) > 0 ? Math.Round((double)upvotes / (upvotes + downvotes) * 100, 2) : 0
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    success = false, 
                    error = "An error occurred while retrieving vote information" 
                });
            }
        }

        
        private async Task<bool> CanUserVoteAsync(long userId, long assetId)
        {
            // Implement voting validation logic here:
            // - Check if user has played the game
            // - Check if user account is old enough
            // - Check rate limiting
            // - Check if user is verified
            
            // For now, returning true as placeholder
            return await Task.FromResult(true);
        }

        /// <summary>
        /// POST /places/doconfigure2 - Handle place configuration form submission
        /// </summary>
        [HttpPost("places/doconfigure2")]
        [Authorize]
        public async Task<IActionResult> DoConfigure2()
        {
            long Id = 0;
            string Name = "", Description = "", Genre = "All";
            string iconType = "", iconChanged = "false";
            string thumbnailType = "";
            IFormFile iconImageFile = null;
            bool isAjax = false;
            
            try
            {
                isAjax = string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase)
                              || Request.Headers["Accept"].Any(h => h.Contains("application/json", StringComparison.OrdinalIgnoreCase));

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    if (isAjax)
                    {
                        return Json(new { success = false, message = "User not authenticated" });
                    }
                    return Redirect("/login");
                }

                if (!long.TryParse(Request.Form["Id"].FirstOrDefault(), out Id))
                {
                    if (isAjax)
                    {
                        return Json(new { success = false, message = "Invalid asset ID" });
                    }

                    return Redirect("/404");
                }
                Name = Request.Form["Name"].FirstOrDefault() ?? "";
                
                // Name validation
                if (string.IsNullOrWhiteSpace(Name))
                {
                    if (isAjax)
                    {
                        return Json(new { success = false, message = "Place name is required" });
                    }
                    TempData["Error"] = "Place name is required";
                    return Redirect($"/places/{Id}/update");
                }
                
                if (Name.Length > 50)
                {
                    if (isAjax)
                    {
                        return Json(new { success = false, message = "Place name cannot exceed 50 characters" });
                    }
                    TempData["Error"] = "Place name cannot exceed 50 characters";
                    return Redirect($"/places/{Id}/update");
                }
                
                if (Name.Contains("<") || Name.Contains(">"))
                {
                    if (isAjax)
                    {
                        return Json(new { success = false, message = "Place name contains illegal characters" });
                    }
                    TempData["Error"] = "Place name contains illegal characters";
                    return Redirect($"/places/{Id}/update");
                }
                
                // Additional security checks for name
                if (Name.Contains("javascript:") || Name.Contains("vbscript:") || Name.Contains("onload=") || Name.Contains("onerror="))
                {
                    if (isAjax)
                    {
                        return Json(new { success = false, message = "Place name contains invalid content" });
                    }
                    TempData["Error"] = "Place name contains invalid content";
                    return Redirect($"/places/{Id}/update");
                }
                
                Description = Request.Form["Description"].FirstOrDefault() ?? "";
                
                // Description validation
                if (Description.Length > 1000)
                {
                    if (isAjax)
                    {
                        return Json(new { success = false, message = "Description cannot exceed 1000 characters" });
                    }
                    TempData["Error"] = "Description cannot exceed 1000 characters";
                    return Redirect($"/places/{Id}/update");
                }
                
                // Security checks for description
                if (Description.Contains("<script") || Description.Contains("javascript:") || Description.Contains("vbscript:") || 
                    Description.Contains("onload=") || Description.Contains("onerror=") || Description.Contains("onclick="))
                {
                    if (isAjax)
                    {
                        return Json(new { success = false, message = "Description contains invalid content" });
                    }
                    TempData["Error"] = "Description contains invalid content";
                    return Redirect($"/places/{Id}/update");
                }
                Genre = Request.Form["Genre"].FirstOrDefault() ?? "All";
                iconType = Request.Form["IconType"].FirstOrDefault() ?? "";
                iconChanged = Request.Form["iconChanged"].FirstOrDefault() ?? "false";
                thumbnailType = Request.Form["ThumbnailType"].FirstOrDefault() ?? "";
                iconImageFile = Request.Form.Files["iconImageFile"];
                int numberOfPlayersMax = 8;
                var numberOfPlayersMaxStr = Request.Form["NumberOfPlayersMax"].FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(numberOfPlayersMaxStr) && int.TryParse(numberOfPlayersMaxStr, out var parsedMaxPlayers))
                {
                    if (parsedMaxPlayers < 1 || parsedMaxPlayers > 100)
                    {
                        if (isAjax)
                        {
                            return Json(new { success = false, message = "Maximum visitor count must be between 1 and 100" });
                        }
                        ViewBag.ErrorMessage = "Maximum visitor count must be between 1 and 100";
                        return View("~/Views/Pages/places/{id}/update.cshtml");
                    }
                    numberOfPlayersMax = parsedMaxPlayers;
                }

                var numberOfCustomSocialSlotsStr = Request.Form["NumberOfCustomSocialSlots"].FirstOrDefault();
                int numberOfCustomSocialSlots = 0;
                
                if (!string.IsNullOrEmpty(numberOfCustomSocialSlotsStr))
                {
                    int.TryParse(numberOfCustomSocialSlotsStr, out numberOfCustomSocialSlots);
                }

                if (numberOfCustomSocialSlots >= numberOfPlayersMax)
                {
                    if (isAjax)
                    {
                        return Json(new { success = false, message = "Custom social slots must be less than maximum visitor count" });
                    }
                    ViewBag.ErrorMessage = "Custom social slots must be less than maximum visitor count";
                    return View("~/Views/Pages/places/{id}/update.cshtml");
                }

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var placeAsset = await _assetRepository.GetAssetByIdAsync(connectionString, Id);
                
                if (placeAsset == null)
                {
                    if (isAjax)
                    {
                        return Json(new { success = false, message = "Place not found" });
                    }
                    return Redirect("/404");
                }

                if (placeAsset.OwnerUserId != currentUserId)
                {
                    if (isAjax)
                    {
                        return Json(new { success = false, message = "Access denied" });
                    }
                    return Redirect("/404");
                }

                if (string.IsNullOrWhiteSpace(Name))
                {
                    if (isAjax)
                    {
                        return Json(new { success = false, message = "Name cannot be empty" });
                    }
                    
                    ViewBag.ErrorMessage = "Name cannot be empty";
                    return View("~/Views/Pages/places/{id}/update.cshtml");
                }

                if (!string.IsNullOrWhiteSpace(thumbnailType) && (thumbnailType == "image" || thumbnailType == "video"))
                {
                    var existingThumbnails = await PlaceThumbnail.GetPlaceThumbnailsAsync(connectionString, Id);

                    bool hasImageThumbnail = existingThumbnails.Any(t =>
                        t.GetType().GetProperty("type")?.GetValue(t)?.ToString() == "image");
                    bool hasValidVideoThumbnail = false;
                    
                    if (!string.IsNullOrWhiteSpace(placeAsset.PlaceThumbnailVideo))
                    {
                        hasValidVideoThumbnail = YouTubeUtilities.IsValidYouTubeURL(placeAsset.PlaceThumbnailVideo);
                    }

                    if (!hasValidVideoThumbnail)
                    {
                        var videoThumbnail = existingThumbnails.FirstOrDefault(t =>
                            t.GetType().GetProperty("type")?.GetValue(t)?.ToString() == "video");
                        
                        if (videoThumbnail != null)
                        {
                            var videoUrlProperty = videoThumbnail.GetType().GetProperty("videoUrl");
                            var videoUrl = videoUrlProperty?.GetValue(videoThumbnail)?.ToString();
                            hasValidVideoThumbnail = !string.IsNullOrWhiteSpace(videoUrl) && YouTubeUtilities.IsValidYouTubeURL(videoUrl);
                        }
                    }


                    bool isInvalidImageSelection = thumbnailType == "image" && !hasImageThumbnail;
                    bool isInvalidVideoSelection = thumbnailType == "video" && !hasValidVideoThumbnail;

                    if (isInvalidImageSelection || isInvalidVideoSelection)
                    {
                        var message = thumbnailType == "image"
                            ? "No Uploaded Video URL was Found"
                            : "No Valid Uploaded Video URL was Found";
                        
                        
                        if (isAjax)
                        {
                            try
                            {
                                var errorResponse = Json(new { success = false, message });
                                return errorResponse;
                            }
                            catch (Exception jsonEx)
                            {
                                Response.StatusCode = 400;
                                Response.ContentType = "application/json";
                                return Content("{\"success\":false,\"message\":\"" + message.Replace("\"", "\\\"") + "\"}", "application/json");
                            }
                        }

                        ViewBag.gameid = Id;
                        ViewBag.ErrorMessage = message;
                        return View("~/Views/Pages/places/{id}/update.cshtml");
                    }
                }

                Description ??= "";
                Genre ??= "All";
                var assetsRepo = new Assets.AssetsRepository();
                await assetsRepo.UpdateAssetMetadataAsync(connectionString, Id, Name, Description);
                var genreId = AssetGenreNames.GetGenreIdFromString(Genre);
                await assetsRepo.UpdateAssetGenreAsync(connectionString, Id, genreId);
                bool sellGameAccessParsed = bool.TryParse(Request.Form["SellGameAccess"].FirstOrDefault(), out bool sellGameAccess);
                int paidAccessPrice = 0;
                int.TryParse(Request.Form["Price"].FirstOrDefault(), out paidAccessPrice);
                bool isCopyingAllowed = false;
                
                if (sellGameAccess)
                {
                    isCopyingAllowed = false;
                }
                
                if (sellGameAccess && (paidAccessPrice < 1 || paidAccessPrice > 10000))
                {
                    if (isAjax)
                    {
                        return Json(new { success = false, message = "Paid access price must be between 1 and 10000 Robux" });
                    }
                    ViewBag.gameid = Id;
                    ViewBag.ErrorMessage = "Paid access price must be between 1 and 10000 Robux";
                    return View("~/Views/Pages/places/{id}/update.cshtml");
                }

                var accessValue = Request.Form["Access"].FirstOrDefault() ?? "Everyone";
                bool arePrivateServersAllowed = true;
                bool.TryParse(Request.Form["ArePrivateServersAllowed"].FirstOrDefault(), out bool parsedPrivateServersAllowed);
                
                if (accessValue.ToLower() == "friends")
                {
                    arePrivateServersAllowed = false;
                }
                else
                {
                    arePrivateServersAllowed = parsedPrivateServersAllowed;
                }

                if (accessValue.ToLower() == "friends" && sellGameAccess)
                {
                    if (isAjax)
                    {
                        return Json(new { success = false, message = "Paid access cannot be enabled when access is set to Friends only" });
                    }
                    ViewBag.gameid = Id;
                    ViewBag.ErrorMessage = "Paid access cannot be enabled when access is set to Friends only";
                    return View("~/Views/Pages/places/{id}/update.cshtml");
                }

                bool privateServersPriceParsed = int.TryParse(Request.Form["PrivateServersPrice"].FirstOrDefault(), out int privateServersPrice);
                bool isFreePrivateServer = Request.Form["IsFreePrivateServer"].FirstOrDefault() == "true";

                if (accessValue.ToLower() == "friends" && arePrivateServersAllowed)
                {
                    if (isAjax)
                    {
                        return Json(new { success = false, message = "Private servers cannot be enabled when access is set to Friends only" });
                    }
                    ViewBag.gameid = Id;
                    ViewBag.ErrorMessage = "Private servers cannot be enabled when access is set to Friends only";
                    return View("~/Views/Pages/places/{id}/update.cshtml");
                }
                if (privateServersPriceParsed && !isFreePrivateServer && privateServersPrice < 10)
                {
                    if (isAjax)
                    {
                        return Json(new { success = false, message = "Private server price must be at least 10 Robux" });
                    }
                    ViewBag.gameid = Id;
                    ViewBag.ErrorMessage = "Private server price must be at least 10 Robux";
                    return View("~/Views/Pages/places/{id}/update.cshtml");
                }
                
                try
                {
                    var playableDevicesList = Request.Form["PlayableDevices"].Where(v => !string.IsNullOrWhiteSpace(v));
                    var playableDevices = string.Join(",", playableDevicesList);

                    var accessSettingsUpdated = await Games.AccessSettingsChanger.UpdatePlaceAccessSettingsAsync(
                        connectionString,
                        Id,
                        currentUserId,
                        numberOfPlayersMax,
                        Request.Form["SocialSlotType"].FirstOrDefault() ?? "Automatic",
                        numberOfCustomSocialSlots,
                        accessValue,
                        arePrivateServersAllowed,
                        isFreePrivateServer,
                        privateServersPrice,
                        playableDevices,
                        sellGameAccess,
                        paidAccessPrice
                    );
                    
                   
                }
                catch (Exception ex)
                {

                    if (isAjax)
                    {
                        return Json(new { success = false});
                    }
                    
                    ViewBag.gameid = Id;
                    ViewBag.ErrorMessage = $"Database error occurred while saving access settings: {ex.Message}";
                    return View("~/Views/Pages/places/{id}/update.cshtml");
                }

                if (!string.IsNullOrWhiteSpace(iconType))
                {
                    try
                    {
                        if (iconType == "image")
                        {
                            await ThumbnailQueries.ClearPlaceGeneratedIconAsync(connectionString, Id);
                            var commitCustomIcon = Request.Form["commitCustomIcon"].FirstOrDefault() ?? "false";
                            
                            if (commitCustomIcon.ToLower() == "true")
                            {
                                var existingAsset = await _assetRepository.GetAssetByIdAsync(connectionString, Id);
                                
                                if (existingAsset != null && !string.IsNullOrWhiteSpace(existingAsset.PlaceCustomIconUrl))
                                {
                                    await ThumbnailQueries.UpdateAssetThumbnailUrlsAsync(connectionString, Id, 
                                        existingAsset.PlaceCustomIconUrl, 
                                        existingAsset.PlaceCustomIconHighResUrl ?? existingAsset.PlaceCustomIconUrl);
                                    await ThumbnailQueries.ClearPlaceCustomIconAsync(connectionString, Id);
                                    
                                }
                                else
                                {
                                }
                            }
                            else if (iconImageFile != null && iconImageFile.Length > 0)
                            {
                                var baseUrl = _configuration["Thumbnails:ThumbnailUrl"] ?? $"{Request.Scheme}://{Request.Host}";
                                
                                var (iconUrl, iconUrlHighRes, fileHash) = await PlaceThumbnail.ProcessCustomIconAsync(
                                    Id, iconImageFile.OpenReadStream(), iconImageFile.FileName, iconImageFile.ContentType, baseUrl);
                                await PlaceThumbnail.SetPlaceCustomIconAsync(connectionString, Id, iconUrl, iconUrlHighRes, fileHash);
                                await ThumbnailQueries.UpdateAssetThumbnailUrlsAsync(connectionString, Id, iconUrl, iconUrlHighRes);
                                
                            }
                            else
                            {
                                var existingAsset = await _assetRepository.GetAssetByIdAsync(connectionString, Id);
                                if (existingAsset != null && !string.IsNullOrWhiteSpace(existingAsset.PlaceCustomIconUrl))
                                {
                                    await PlaceThumbnail.SetPlaceCustomIconAsync(connectionString, Id, 
                                        existingAsset.PlaceCustomIconUrl, 
                                        existingAsset.PlaceCustomIconHighResUrl ?? existingAsset.PlaceCustomIconUrl,
                                        existingAsset.PlaceCustomIconHash ?? "");
                                    await ThumbnailQueries.UpdateAssetThumbnailUrlsAsync(connectionString, Id, 
                                        existingAsset.PlaceCustomIconUrl, 
                                        existingAsset.PlaceCustomIconHighResUrl ?? existingAsset.PlaceCustomIconUrl);
                                    
                                }
                            }
                        }
                        else if (iconType == "autogenerated")
                        {
                            await ThumbnailQueries.ClearPlaceCustomIconAsync(connectionString, Id);
                            var existingAsset = await _assetRepository.GetAssetByIdAsync(connectionString, Id);
                            
                            if (existingAsset != null && !string.IsNullOrWhiteSpace(existingAsset.PlaceGeneratedIconUrl))
                            {
                                var generatedIconUrl = existingAsset.PlaceGeneratedIconUrl;
                                var generatedIconHighResUrl = existingAsset.PlaceGeneratedIconHighResUrl ?? generatedIconUrl;
                                await ThumbnailQueries.SetPlaceGeneratedIconFlagsAsync(connectionString, Id, generatedIconUrl, generatedIconHighResUrl, existingAsset.PlaceGeneratedIconHash ?? "");
                                await ThumbnailQueries.UpdateAssetThumbnailUrlsAsync(connectionString, Id, generatedIconUrl, generatedIconHighResUrl);
                                
                            }
                            else
                            {
                                var baseUrl = _configuration["Thumbnails:ThumbnailUrl"] ?? $"{Request.Scheme}://{Request.Host}";
                                var baseUrlForTask = baseUrl;
                                _ = Task.Run(async () => {
                                    try
                                    {
                                        await PlaceThumbnail.GeneratePlaceThumbnailAsync(_thumbnailService, connectionString, Id, existingAsset.ContentHash, baseUrlForTask, placeName: existingAsset.Name ?? "", cancellationToken: CancellationToken.None);
                                    }
                                    catch (Exception ex)
                                    {
                                    }
                                });
                                
                                var generatedIconUrl = "/images/RobloxLogo.png";
                                var generatedIconHighResUrl = generatedIconUrl;
                                
                            }
                        }
                        else
                        {
                            await ThumbnailQueries.ClearPlaceCustomIconAsync(connectionString, Id);
                            await ThumbnailQueries.ClearPlaceGeneratedIconAsync(connectionString, Id);
                        }
                    }
                    catch (Exception iconEx)
                    {
                        Console.WriteLine(iconEx.Message);
                    }
                }

                if (!string.IsNullOrWhiteSpace(thumbnailType))
                {
                    try
                    {
                        if (thumbnailType == "image")
                        {
                            await ThumbnailQueries.ClearPlaceAutoGeneratedThumbnailAsync(connectionString, Id);
                            await ThumbnailQueries.ClearPlaceVideoThumbnailAsync(connectionString, Id);
                            await ThumbnailQueries.SetPlaceCustomThumbnailFlagAsync(connectionString, Id, true);
                        }
                        else if (thumbnailType == "video")
                        {
                            await ThumbnailQueries.ClearPlaceAutoGeneratedThumbnailAsync(connectionString, Id);
                            await ThumbnailQueries.ClearPlaceCustomThumbnailAsync(connectionString, Id);
                            await ThumbnailQueries.SetPlaceVideoThumbnailFlagAsync(connectionString, Id, true);
                        }
                        else if (thumbnailType == "autogenerated")
                        {
                            await ThumbnailQueries.ClearPlaceCustomThumbnailAsync(connectionString, Id);
                            await ThumbnailQueries.ClearPlaceVideoThumbnailAsync(connectionString, Id);
                            await ThumbnailQueries.SetPlaceAutoGeneratedThumbnailFlagAsync(connectionString, Id, true);
                        }
                        else
                        {
                            await ThumbnailQueries.ClearPlaceCustomThumbnailAsync(connectionString, Id);
                            await ThumbnailQueries.ClearPlaceVideoThumbnailAsync(connectionString, Id);
                            await ThumbnailQueries.ClearPlaceAutoGeneratedThumbnailAsync(connectionString, Id);
                        }
                    }
                    catch (Exception thumbnailEx)
                    {
                        Console.WriteLine(thumbnailEx.Message);
                    }
                }

                bool.TryParse(Request.Form["IsCopyingAllowed"].FirstOrDefault(), out isCopyingAllowed);

                if (sellGameAccess)
                {
                    isCopyingAllowed = false;
                }

                bool isAllGenresAllowed = false;
                bool.TryParse(Request.Form["IsAllGenresAllowed"].FirstOrDefault(), out isAllGenresAllowed);
                bool allowPlaceToBeCopiedInGame = false;
                bool allowPlaceToBeUpdatedInGame = false;
                bool.TryParse(Request.Form["AllowPlaceToBeCopiedInGame"].FirstOrDefault(), out allowPlaceToBeCopiedInGame);
                bool.TryParse(Request.Form["AllowPlaceToBeUpdatedInGame"].FirstOrDefault(), out allowPlaceToBeUpdatedInGame);
                var selectedGearTypes = new List<string>();
                var allowedGearTypesFromForm = Request.Form["AllowedGearTypes"].FirstOrDefault();
                
                if (!string.IsNullOrEmpty(allowedGearTypesFromForm) && allowedGearTypesFromForm.StartsWith("["))
                {
                    try
                    {
                        var gearTypeIds = System.Text.Json.JsonSerializer.Deserialize<List<int>>(allowedGearTypesFromForm);
                        
                        if (gearTypeIds != null)
                        {
                            selectedGearTypes = gearTypeIds.Select(id => id.ToString()).ToList();
                        }
                    }
                    catch (System.Text.Json.JsonException ex)
                    {
                        selectedGearTypes = new List<string>();
                    }
                }
                else
                {
                    foreach (var key in Request.Form.Keys)
                    {
                        if (key.StartsWith("AllowedGearTypes[") && key.EndsWith(".IsSelected"))
                        {
                            var isSelected = Request.Form[key].FirstOrDefault() == "true";
                            if (isSelected)
                            {
                                var indexStr = key.Substring("AllowedGearTypes[".Length, key.IndexOf("]") - "AllowedGearTypes[".Length);
                                if (int.TryParse(indexStr, out var index))
                                {
                                    selectedGearTypes.Add((index + 1).ToString());
                                }
                            }
                        }
                    }
                }
                
                
                string allowedGearTypesJson = selectedGearTypes.Count > 0 
                    ? System.Text.Json.JsonSerializer.Serialize(selectedGearTypes.Select(int.Parse)) 
                    : "[]";
                    
                var permissionSettings = new Games.PermissionSettings();
                try
                {
                    await permissionSettings.UpdatePlaceCopyingAllowedAsync(connectionString, Id, isCopyingAllowed);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to update copying allowed for place {Id}: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    throw;
                }
                
                try
                {
                    await permissionSettings.UpdatePlaceGearPermissionsAsync(connectionString, Id, isAllGenresAllowed, allowedGearTypesJson);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Failed to update gear permissions for place {Id}: {ex.Message}");
                    Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                    throw;
                }

                try
                {
                    await permissionSettings.UpdatePlaceInGamePermissionsAsync(connectionString, Id, allowPlaceToBeCopiedInGame, allowPlaceToBeUpdatedInGame);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Failed to update in-game permissions for place {Id}: {ex.Message}");
                    Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                    throw;
                }

                if (isAjax)
                {
                    return Json(new { success = true, message = "Place updated successfully" });
                }

                return Redirect($"/places/{Id}/update");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                Console.WriteLine($"[ERROR] Inner exception: {ex.InnerException?.Message}");
                
                if (isAjax)
                {
                    return Json(new { success = false});
                }
                
                ModelState.AddModelError("", "An error occurred while saving. Please try again.");
                return View();
            }
        }

        /// <summary>
        /// POST /places/thumbnails/add-image - Handle thumbnail image upload for a place
        /// </summary>
        [HttpPost("places/thumbnails/add-image")]
        [Authorize]
        public async Task<IActionResult> AddThumbnailImage(IFormFile thumbnailImageFile)
        {
            try
            {
                Console.WriteLine($"[DEBUG] AddThumbnailImage called");
                
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    Console.WriteLine($"[DEBUG] User not authenticated");
                    return Json(new { success = false, message = "User not authenticated" });
                }

                Console.WriteLine($"[DEBUG] User ID: {currentUserId}");

                if (thumbnailImageFile == null || thumbnailImageFile.Length == 0)
                {
                    Console.WriteLine($"[DEBUG] No file uploaded - thumbnailImageFile is null or empty");
                    return Json(new { success = false, message = "No file uploaded" });
                }

                Console.WriteLine($"[DEBUG] File uploaded: {thumbnailImageFile.FileName}, Size: {thumbnailImageFile.Length}, ContentType: {thumbnailImageFile.ContentType}");

                if (!thumbnailImageFile.ContentType.StartsWith("image/"))
                {
                    Console.WriteLine($"[DEBUG] Invalid file type: {thumbnailImageFile.ContentType}");
                    return Json(new { success = false, message = "Invalid file type. Please upload an image file." });
                }

                var placeIdStr = Request.Form["id"].FirstOrDefault();
                Console.WriteLine($"[DEBUG] Place ID from form: '{placeIdStr}'");
                
                if (string.IsNullOrWhiteSpace(placeIdStr) || !long.TryParse(placeIdStr, out var placeId))
                {
                    Console.WriteLine($"[DEBUG] Invalid place ID: '{placeIdStr}'");
                    return Json(new { success = false, message = "Invalid place ID" });
                }

                Console.WriteLine($"[DEBUG] Parsed Place ID: {placeId}");

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var placeAsset = await _assetRepository.GetAssetByIdAsync(connectionString, placeId);
                
                Console.WriteLine($"[DEBUG] Retrieved place asset: {(placeAsset != null ? "Found" : "Not found")}");
                
                if (placeAsset == null)
                {
                    Console.WriteLine($"[DEBUG] Place not found for ID: {placeId}");
                    return Json(new { success = false, message = "Place not found" });
                }

                Console.WriteLine($"[DEBUG] Place Owner ID: {placeAsset.OwnerUserId}, Current User ID: {currentUserId}");

                if (placeAsset.OwnerUserId != currentUserId)
                {
                    Console.WriteLine($"[DEBUG] Access denied - owner mismatch");
                    return Json(new { success = false, message = "Access denied" });
                }

                var existingThumbnails = await PlaceThumbnail.GetPlaceThumbnailsAsync(connectionString, placeId);
                var imageThumbnailCount = existingThumbnails.Count(t => 
                    t.GetType().GetProperty("type")?.GetValue(t)?.ToString() == "image");
                
                Console.WriteLine($"[DEBUG] Existing image thumbnails count: {imageThumbnailCount}");
                
                if (imageThumbnailCount >= 10)
                {
                    Console.WriteLine($"[DEBUG] Maximum thumbnail limit reached");
                    return Json(new { success = false, message = "Maximum limit of 10 thumbnails per place has been reached." });
                }

                var baseUrl = _configuration["Thumbnails:ThumbnailUrl"] ?? $"{Request.Scheme}://{Request.Host}";
                Console.WriteLine($"[DEBUG] Base URL: {baseUrl}");
                
                Console.WriteLine($"[DEBUG] Starting ProcessThumbnailImageAsync...");
                var (thumbnailUrl, fileHash) = await PlaceThumbnail.ProcessThumbnailImageAsync(
                    placeId, thumbnailImageFile.OpenReadStream(), thumbnailImageFile.FileName, thumbnailImageFile.ContentType, baseUrl);
                
                Console.WriteLine($"[DEBUG] Processed thumbnail - URL: {thumbnailUrl}, Hash: {fileHash}");

                Console.WriteLine($"[DEBUG] Starting AddThumbnailImageAsync...");
                var thumbnailResult = await PlaceThumbnail.AddThumbnailImageAsync(
                    connectionString, placeId, placeAsset.Name, thumbnailUrl, fileHash);

                Console.WriteLine($"[DEBUG] AddThumbnailImageAsync result: {(thumbnailResult != null ? "Success" : "Null")}");

                if (thumbnailResult == null)
                {
                    Console.WriteLine($"[DEBUG] Thumbnail already exists (duplicate)");
                    return Json(new { success = false, message = "This image has already been added to your thumbnails." });
                }

                Console.WriteLine($"[DEBUG] Updating place thumbnail flags...");
                await PlaceThumbnail.UpdatePlaceThumbnailFlagsAsync(connectionString, placeId, hasCustom: true, hasVideo: false, hasAutogenerated: false);
                
                var updatedThumbnails = await PlaceThumbnail.GetPlaceThumbnailsAsync(connectionString, placeId);
                Console.WriteLine($"[DEBUG] Updated thumbnails count: {updatedThumbnails.Count}");
                
                return Json(new { 
                    success = true, 
                    message = "Thumbnail uploaded successfully",
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Exception in AddThumbnailImage: {ex.Message}");
                Console.WriteLine($"[DEBUG] Stack trace: {ex.StackTrace}");
                return Json(new { success = false, message = "An error occurred while uploading the thumbnail. Please try again." });
            }
        }

        /// <summary>
        /// POST /places/thumbnails/add-video - Handle thumbnail video upload for a place
        /// </summary>
        [HttpPost("places/thumbnails/add-video")]
        [Authorize]
        public async Task<IActionResult> AddThumbnailVideo()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var videoUrl = Request.Form["thumbnailYoutubeUrl"].FirstOrDefault();
                
                if (string.IsNullOrWhiteSpace(videoUrl))
                {
                    return Json(new { success = false, message = "Video URL is required" });
                }

                if (!YouTubeUtilities.IsValidYouTubeURL(videoUrl))
                {
                    return Json(new { success = false, message = "Invalid YouTube URL." });
                }

                var placeIdStr = Request.Form["id"].FirstOrDefault();
                
                if (string.IsNullOrWhiteSpace(placeIdStr) || !long.TryParse(placeIdStr, out var placeId))
                {
                    return Json(new { success = false, message = "Invalid place ID" });
                }

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var placeAsset = await _assetRepository.GetAssetByIdAsync(connectionString, placeId);
                
                if (placeAsset == null)
                {
                    return Json(new { success = false, message = "Place not found" });
                }

                if (placeAsset.OwnerUserId != currentUserId)
                {
                    return Json(new { success = false, message = "Access denied" });
                }

                await PlaceThumbnail.SetPlaceVideoUrlTempAsync(connectionString, placeId, videoUrl);
                var updatedThumbnails = await PlaceThumbnail.GetPlaceThumbnailsAsync(connectionString, placeId);
                
                return Json(new { 
                    success = true, 
                    message = "Video thumbnail added successfully",
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while adding the video thumbnail. Please try again." });
            }
        }

        /// <summary>
        /// POST /places/thumbnails/remove - Handle thumbnail removal for a place
        /// </summary>
        [HttpPost("places/thumbnails/remove")]
        [Authorize]
        public async Task<IActionResult> RemoveThumbnail()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var thumbnailIdStr = Request.Form["thumbnailid"].FirstOrDefault();
                if (string.IsNullOrWhiteSpace(thumbnailIdStr) || !long.TryParse(thumbnailIdStr, out var thumbnailId))
                {
                    return Json(new { success = false, message = "Invalid thumbnail ID" });
                }

                var placeIdStr = Request.Form["id"].FirstOrDefault();
                if (string.IsNullOrWhiteSpace(placeIdStr) || !long.TryParse(placeIdStr, out var placeId))
                {
                    return Json(new { success = false, message = "Invalid place ID" });
                }

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var placeAsset = await _assetRepository.GetAssetByIdAsync(connectionString, placeId);
                
                if (placeAsset == null || placeAsset.OwnerUserId != currentUserId)
                {
                    return Json(new { success = false, message = "Access denied" });
                }
                var success = await PlaceThumbnail.RemoveThumbnailAsync(connectionString, placeId, thumbnailId);
                
                if (success)
                {
                    await PlaceThumbnail.UpdatePlaceThumbnailFlagsAsync(connectionString, placeId, hasCustom: false, hasVideo: false, hasAutogenerated: false);
                    
                    var updatedThumbnails = await PlaceThumbnail.GetPlaceThumbnailsAsync(connectionString, placeId);
                    
                    return Json(new { 
                        success = true, 
                        message = "Thumbnail removed successfully",
                        thumbnails = updatedThumbnails
                    });
                }

                return Json(new { success = false, message = "Thumbnail not found" });
            }
    catch (Exception ex)
    {
        return Json(new { success = false, message = "An error occurred while removing the thumbnail. Please try again." });
    }
}

        /// <summary>
        /// GET /places/version-history-items - Get version history items for a place
        /// </summary>
        [HttpGet("places/version-history-items")]
        [Authorize]
        public async Task<IActionResult> GetVersionHistoryItems([FromQuery] long assetID, [FromQuery] int page = 1)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Content("<tbody><tr><td colspan='4'>User not authenticated</td></tr></tbody>", "text/html");
                }

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var placeAsset = await _assetRepository.GetAssetByIdAsync(connectionString, assetID);
                
                if (placeAsset == null || placeAsset.OwnerUserId != currentUserId)
                {
                    return Content("<tbody><tr><td colspan='4'>Access denied</td></tr></tbody>", "text/html");
                }

                var randomDate = DateTime.Now.AddDays(-new Random().Next(0, 365)).ToString("M/d/yyyy h:mm:ss tt");
                var html = $@"<thead>
                                                        <tr>
                                                            <th>Version number</th>
                                                            <th>Created</th>
                                                            <th></th>
                                                            <th></th>
                                                        </tr>
                                                    </thead>
                                                    <tbody>
                                                        <tr>
                                                            <td>1</td>
                                                            <td>{randomDate}</td>
                                                            <td><span class=""icon-checkmark-16x16""></span></td>
                                                            <td><span data-asset-version-id=""1""
                                                                    class=""btn-control btn-control-medium revertLink"">Revert
                                                                    to this version</span></td>
                                                        </tr>
                                                    </tbody>";

                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                return Content("<tbody><tr><td colspan='4'>An error occurred while loading version history</td></tr></tbody>", "text/html");
            }
        }

       
    }
}
