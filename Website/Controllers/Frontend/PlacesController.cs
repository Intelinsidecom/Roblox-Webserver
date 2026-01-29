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
            
            // Load template mapping from configuration
            _templateMapping = new PlaceTemplateMapping();
            
            // Get the PlaceTemplates section and bind it directly to our Templates dictionary
            var templatesSection = configuration.GetSection("PlaceTemplates");
            
            // Bind the PlaceTemplates section directly to the Templates dictionary
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
                // Get current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Redirect("/login");
                }

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                
                // Get user name
                var creatorUserName = await UserQueries.GetUserNameByIdAsync(connectionString, currentUserId, cancellationToken)
                    .ConfigureAwait(false) ?? User.Identity?.Name ?? "Player";

                // Get next place number for this user
                int nextPlaceNumber = await GamesQueries.GetNextPlaceNumberAsync(currentUserId, connectionString, cancellationToken);

                // Set ViewBags for the create.cshtml view
                ViewBag.UserName = creatorUserName;
                ViewBag.NextPlaceNumber = nextPlaceNumber;
                
                // Show any error messages from previous POST attempts
                if (TempData["Error"] != null)
                {
                    ViewBag.ErrorMessage = TempData["Error"].ToString();
                }

                return View("~/Views/Pages/places/create.cshtml");
            }
            catch (Exception ex)
            {
                // If anything fails, redirect to develop page
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

                if (name.Contains("<") || name.Contains(">"))
                {
                    TempData["Error"] = "Place name contains illegal characters";
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

                // Process template file BEFORE creating the universe to get the content hash
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

                // If template was used, we don't need to process it again since it's already applied
                // Just generate thumbnails for the correct content
                if (!string.IsNullOrWhiteSpace(templateContentHash))
                {
                    // Generate thumbnails for the template content immediately
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var baseUrl = _configuration["Thumbnails:ThumbnailUrl"] ?? $"{Request.Scheme}://{Request.Host}/";
                            
                            // Generate icon thumbnails (256x256 and 1024x1024) for the place with template content
                            await PlaceThumbnail.GeneratePlaceThumbnailAsync(
                                _thumbnailService,
                                connectionString,
                                universe.RootPlaceId,
                                templateContentHash,
                                baseUrl,
                                placeName: name,
                                cancellationToken: CancellationToken.None);

                            // Generate 720p auto-generated thumbnail for the place with template content
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
                            // Log error if needed, but don't fail the request
                            Console.WriteLine($"Failed to generate thumbnails for template place {universe.RootPlaceId}: {ex.Message}");
                        }
                    });
                }

                await UpdatePlaceSettingsFromFormAsync(universe.RootPlaceId, connectionString, description, genre, cancellationToken);
                await UpdateUniverseNameAsync(universe.UniverseId, name, connectionString, cancellationToken);

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
                privateServersPrice = Math.Max(10, parsedPrice); // minimum 10 robux
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

            // Parse gear settings
            bool isAllGenresAllowed = Request.Form["IsAllGenresAllowed"].FirstOrDefault() == "True";
            var allowedGearTypes = new List<string>();
            for (int i = 0; i < 9; i++) // 9 gear types as shown in form
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

            // Use GameCreationService to update the place
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
        /// Helper function to update universe name
        /// </summary>
        private async Task UpdateUniverseNameAsync(long universeId, string name, string connectionString, CancellationToken cancellationToken)
        {

            await GameCreationService.UpdateUniverseNameAsync(universeId, connectionString, name, cancellationToken);
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

                // Get assets directory
                var assetsRoot = _configuration["Assets:Directory"];
                if (string.IsNullOrWhiteSpace(assetsRoot))
                {
                    return null;
                }

                // Use TemplateHelper to process and save the template file
                return TemplateHelper.ProcessAndSaveTemplateAsync(templatePath, assetsRoot, cancellationToken);
            }
            catch (Exception ex)
            {
                // Log error if needed, but return null to fall back to default behavior
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

                // Get assets directory
                var assetsRoot = _configuration["Assets:Directory"];
                if (string.IsNullOrWhiteSpace(assetsRoot))
                {
                    return null;
                }

                // Use TemplateHelper to process and save the template file
                var contentHash = TemplateHelper.ProcessAndSaveTemplateAsync(templatePath, assetsRoot, cancellationToken);
                if (string.IsNullOrEmpty(contentHash))
                {
                    return null;
                }

                // Update the place asset with the new content hash
                await GamesQueries.UpdatePlaceAssetContentHashAsync(placeId, contentHash, connectionString, cancellationToken);

                return contentHash;
            }
            catch (Exception ex)
            {
                // Log error if needed, but return null to fall back to default behavior
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
                // Check for success message in query string
                var successMessage = Request.Query["success"].FirstOrDefault();
                if (!string.IsNullOrEmpty(successMessage))
                {
                    ViewBag.SuccessMessage = successMessage;
                }

                // Check for tab parameter and store it for JavaScript
                var tabParam = Request.Query["tab"].FirstOrDefault();
                if (!string.IsNullOrEmpty(tabParam))
                {
                    ViewBag.ActiveTab = tabParam;
                }

                // Get current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Redirect("/login");
                }

                // Validate place ownership and type using Thumbnails assembly helper
                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var isValidPlace = await PlaceValidationHelper.ValidatePlaceOwnershipAsync(id, currentUserId, connectionString, _assetRepository);
                if (!isValidPlace)
                {
                    return Redirect("/404");
                }

                // Get place asset to verify it's actually a place (AssetType 9)
                var placeAsset = await _assetRepository.GetAssetByIdAsync(connectionString, id);
                if (placeAsset == null || placeAsset.AssetTypeId != 9)
                {
                    // Redirect non-place assets to ASPX edit page
                    return Redirect($"/my/item.aspx?id={id}");
                }

                // Get universe ID for the place
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

                // Load actual thumbnail data from database
                var thumbnailData = await PlaceThumbnail.GetPlaceThumbnailsAsync(connectionString, id);
                
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
        [Authorize] // Ensure authentication is required
        public async Task<IActionResult> AddGeneratedThumbnail()
        {
            try
            {
                // Get current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                // Get place ID from form
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
                        // Fire-and-forget thumbnail generation
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
                                // Log error if needed, but don't fail the request
                            }
                        });
                        
                        // Return immediately with existing thumbnail or placeholder
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
                return Json(new { success = false, message = "An error occurred while generating the thumbnail. Please try again." });
            }
        }

        /// <summary>
        /// POST /places/icons/add-generated-image - Handle generated icon selection for a place
        /// </summary>
        [HttpPost("places/icons/add-generated-image")]
        [Authorize] // Ensure authentication is required
        public async Task<IActionResult> AddGeneratedIcon([FromForm] bool force = false)
        {
            try
            {
                // Get current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                // Get place ID from form
                var placeIdStr = Request.Form["placeId"].FirstOrDefault() ?? Request.Form["Id"].FirstOrDefault();
                if (string.IsNullOrWhiteSpace(placeIdStr) || !long.TryParse(placeIdStr, out var placeId))
                {
                    return Json(new { success = false, message = "Invalid place ID" });
                }

                // Get force parameter from form (override the parameter binding)
                var forceStr = Request.Form["force"].FirstOrDefault();
                force = !string.IsNullOrWhiteSpace(forceStr) && (forceStr.ToLower() == "true" || forceStr == "1");

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var placeAsset = await _assetRepository.GetAssetByIdAsync(connectionString, placeId);
                
                if (placeAsset == null)
                {
                    return Json(new { success = false, message = "Place not found" });
                }

                // Check if user owns this place
                if (placeAsset.OwnerUserId != currentUserId)
                {
                    return Json(new { success = false, message = "Access denied" });
                }

                // Check if there's already a generated icon (unless force is true)
                if (!force && placeAsset != null && !string.IsNullOrWhiteSpace(placeAsset.PlaceGeneratedIconUrl))
                {
                    // Use existing generated icon - no re-rendering needed
                    var generatedIconUrl = placeAsset.PlaceGeneratedIconUrl;
                    
                    // Ensure generated_icon flag is set and thumbnail URL matches generated icon URL
                    await ThumbnailQueries.SetPlaceGeneratedIconFlagsAsync(connectionString, placeId, generatedIconUrl, placeAsset.PlaceGeneratedIconHighResUrl ?? generatedIconUrl, placeAsset.PlaceGeneratedIconHash ?? "");
                    

                    return Json(new { 
                        success = true, 
                        message = "Generated icon set successfully",
                        iconUrl = generatedIconUrl,
                        iconHash = placeAsset.PlaceGeneratedIconHash ?? ""
                    });
                }

                // No existing generated icon or force=true, generate a new one
                var baseUrl = _configuration["Thumbnails:ThumbnailUrl"];
                if (string.IsNullOrWhiteSpace(baseUrl))
                {
                    baseUrl = $"{Request.Scheme}://{Request.Host}/";
                }
                
                try
                {
                    // Fire-and-forget thumbnail generation
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

                    // Return existing asset info immediately - thumbnail will be updated in background
                    var updatedAsset = await _assetRepository.GetAssetByIdAsync(connectionString, placeId);
                    var iconUrl = updatedAsset?.PlaceGeneratedIconUrl ?? "/images/RobloxLogo.png";
                    var iconHash = updatedAsset?.PlaceGeneratedIconHash ?? "";
                    
                    if (string.IsNullOrEmpty(iconUrl))
                    {
                        // Fallback to auto-generated thumbnail URL if generated icon URL is not set
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
        [Authorize] // Ensure authentication is required
        public async Task<IActionResult> AddCustomIcon(IFormFile iconImageFile)
        {
            try
            {
                // Get current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                if (iconImageFile == null || iconImageFile.Length == 0)
                {
                    return Json(new { success = false, message = "No file uploaded" });
                }

                // Validate file type
                if (!iconImageFile.ContentType.StartsWith("image/"))
                {
                    return Json(new { success = false, message = "Invalid file type. Please upload an image file." });
                }

                // Get place ID from form or query
                var placeIdStr = Request.Form["placeId"].FirstOrDefault() ?? Request.Query["placeId"].FirstOrDefault();
                if (string.IsNullOrWhiteSpace(placeIdStr) || !long.TryParse(placeIdStr, out var placeId))
                {
                    return Json(new { success = false, message = "Invalid place ID" });
                }

                // Get place asset to verify ownership
                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var placeAsset = await _assetRepository.GetAssetByIdAsync(connectionString, placeId);
                
                if (placeAsset == null)
                {
                    return Json(new { success = false, message = "Place not found" });
                }

                // Check if user owns this place
                if (placeAsset.OwnerUserId != currentUserId)
                {
                    return Json(new { success = false, message = "Access denied" });
                }

                // Process the uploaded image using PlaceThumbnail helper
                var baseUrl = _configuration["Thumbnails:ThumbnailUrl"];
                if (string.IsNullOrWhiteSpace(baseUrl))
                {
                    baseUrl = $"{Request.Scheme}://{Request.Host}";
                }
                
                var (iconUrl, iconUrlHighRes, fileHash) = await PlaceThumbnail.ProcessCustomIconAsync(
                    placeId, iconImageFile.OpenReadStream(), iconImageFile.FileName, iconImageFile.ContentType, baseUrl);

                // Update place asset with custom icon information
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
        [Authorize] // Ensure authentication is required
        public async Task<IActionResult> RemoveIcon()
        {
            try
            {
                // Get current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                // Get place ID from form
                var placeIdStr = Request.Form["placeId"].FirstOrDefault() ?? Request.Form["Id"].FirstOrDefault();
                if (string.IsNullOrWhiteSpace(placeIdStr) || !long.TryParse(placeIdStr, out var placeId))
                {
                    return Json(new { success = false, message = "Invalid place ID" });
                }

                // Get place asset to verify ownership
                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var placeAsset = await _assetRepository.GetAssetByIdAsync(connectionString, placeId);
                
                if (placeAsset == null)
                {
                    return Json(new { success = false, message = "Place not found" });
                }

                // Check if user owns this place
                if (placeAsset.OwnerUserId != currentUserId)
                {
                    return Json(new { success = false, message = "Access denied" });
                }

                // Check if custom icon URL matches thumbnail URL and clear both if they do
                bool shouldClearThumbnail = !string.IsNullOrWhiteSpace(placeAsset.PlaceCustomIconUrl) && 
                                       placeAsset.PlaceCustomIconUrl == placeAsset.ThumbnailUrl;

                // Remove icon using PlaceThumbnail helper
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
        [Authorize] // Ensure authentication is required
        public async Task<IActionResult> GetPlaceThumbnail(long id)
        {
            try
            {
                // Get current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Unauthorized(new { error = "User not authenticated" });
                }

                // Get place asset using Assets assembly
                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var placeAsset = await _assetRepository.GetAssetByIdAsync(connectionString, id);
                
                if (placeAsset == null)
                {
                    return NotFound(new { error = "Place not found" });
                }

                // Check if user owns this place
                if (placeAsset.OwnerUserId != currentUserId)
                {
                    return Unauthorized(new { error = "Access denied" });
                }

                // Return thumbnail URL information
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
        [Authorize] // Ensure authentication is required
        public async Task<IActionResult> GetPlaceThumbnails(long id)
        {
            try
            {
                // TODO: Implement actual thumbnail loading logic
                // This should return a list of thumbnails/videos for the place
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
        /// POST /places/doconfigure2 - Handle place configuration form submission
        /// </summary>
        [HttpPost("places/doconfigure2")]
        [Authorize] // Ensure authentication is required
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
                // Detect AJAX-style requests more robustly (not just X-Requested-With)
                isAjax = string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase)
                              || Request.Headers["Accept"].Any(h => h.Contains("application/json", StringComparison.OrdinalIgnoreCase));

                // Get current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    if (isAjax)
                    {
                        return Json(new { success = false, message = "User not authenticated" });
                    }
                    return Redirect("/login");
                }

                // Read form data directly to bypass model binding issues
                
                if (!long.TryParse(Request.Form["Id"].FirstOrDefault(), out Id))
                {
                    if (isAjax)
                    {
                        return Json(new { success = false, message = "Invalid asset ID" });
                    }

                    return Redirect("/404");
                }
                Name = Request.Form["Name"].FirstOrDefault() ?? "";
                
                if (string.IsNullOrWhiteSpace(Name))
                {
                    if (isAjax)
                    {
                        return Json(new { success = false, message = "Place name is required" });
                    }
                    TempData["Error"] = "Place name is required";
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
                
                Description = Request.Form["Description"].FirstOrDefault() ?? "";
                Genre = Request.Form["Genre"].FirstOrDefault() ?? "All";
                iconType = Request.Form["IconType"].FirstOrDefault() ?? "";
                iconChanged = Request.Form["iconChanged"].FirstOrDefault() ?? "false";
                thumbnailType = Request.Form["ThumbnailType"].FirstOrDefault() ?? "";
                iconImageFile = Request.Form.Files["iconImageFile"];

                // Read and validate NumberOfPlayersMax
                int numberOfPlayersMax = 8; // default value
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

                // Read and validate NumberOfCustomSocialSlots
                var numberOfCustomSocialSlotsStr = Request.Form["NumberOfCustomSocialSlots"].FirstOrDefault();
                int numberOfCustomSocialSlots = 0;
                
                if (!string.IsNullOrEmpty(numberOfCustomSocialSlotsStr))
                {
                    int.TryParse(numberOfCustomSocialSlotsStr, out numberOfCustomSocialSlots);
                }


                // Validate that custom social slots don't exceed max player count
                if (numberOfCustomSocialSlots >= numberOfPlayersMax)
                {
                    if (isAjax)
                    {
                        return Json(new { success = false, message = "Custom social slots must be less than maximum visitor count" });
                    }
                    ViewBag.ErrorMessage = "Custom social slots must be less than maximum visitor count";
                    return View("~/Views/Pages/places/{id}/update.cshtml");
                }


                // Get place asset to verify ownership
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

                // Check if user owns this place
                if (placeAsset.OwnerUserId != currentUserId)
                {
                    if (isAjax)
                    {
                        return Json(new { success = false, message = "Access denied" });
                    }
                    return Redirect("/404"); // User doesn't own this place
                }

                // Validate input
                if (string.IsNullOrWhiteSpace(Name))
                {
                    if (isAjax)
                    {
                        return Json(new { success = false, message = "Name cannot be empty" });
                    }
                    
                    // Return error or validation message for non-AJAX requests
                    ViewBag.ErrorMessage = "Name cannot be empty";
                    return View("~/Views/Pages/places/{id}/update.cshtml");
                }

                // Validate thumbnail selection: prevent switching to image/video mode
                // when no corresponding thumbnails have been uploaded.
                if (!string.IsNullOrWhiteSpace(thumbnailType) && (thumbnailType == "image" || thumbnailType == "video"))
                {
                    var existingThumbnails = await PlaceThumbnail.GetPlaceThumbnailsAsync(connectionString, Id);

                    bool hasImageThumbnail = existingThumbnails.Any(t =>
                        t.GetType().GetProperty("type")?.GetValue(t)?.ToString() == "image");

                    // For video validation, check both place_thumbnails table (legacy) and place_thumbnail_video field
                    bool hasValidVideoThumbnail = false;
                    
                    // Check if there's a video URL in the place_thumbnail_video field
                    if (!string.IsNullOrWhiteSpace(placeAsset.PlaceThumbnailVideo))
                    {
                        hasValidVideoThumbnail = YouTubeUtilities.IsValidYouTubeURL(placeAsset.PlaceThumbnailVideo);
                    }
                    
                    // Also check legacy place_thumbnails table for existing videos
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

                        // For non-AJAX requests, redisplay the form with an error message
                        ViewBag.gameid = Id;
                        ViewBag.ErrorMessage = message;
                        return View("~/Views/Pages/places/{id}/update.cshtml");
                    }
                }

                // Provide default values for optional fields
                Description ??= "";
                Genre ??= "All";

                // Update place metadata using Assets assembly
                var assetsRepo = new Assets.AssetsRepository();
                await assetsRepo.UpdateAssetMetadataAsync(connectionString, Id, Name, Description);
                
                // Also update the genre separately
                var genreId = AssetGenreNames.GetGenreIdFromString(Genre);
                await assetsRepo.UpdateAssetGenreAsync(connectionString, Id, genreId);

                // Parse and validate paid access settings
                bool sellGameAccessParsed = bool.TryParse(Request.Form["SellGameAccess"].FirstOrDefault(), out bool sellGameAccess);
                int paidAccessPrice = 0;
                int.TryParse(Request.Form["Price"].FirstOrDefault(), out paidAccessPrice);
                
                // Parse copying permission setting (initialize to false, will be properly parsed later)
                bool isCopyingAllowed = false;
                
                // Business rule: If sell game access is enabled, automatically disable copying
                if (sellGameAccess)
                {
                    isCopyingAllowed = false;
                }
                
                // Validate paid access price range (1-10000)
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

                // Parse access type and validate private servers setting
                var accessValue = Request.Form["Access"].FirstOrDefault() ?? "Everyone";
                bool arePrivateServersAllowed = true; // default to true
                bool.TryParse(Request.Form["ArePrivateServersAllowed"].FirstOrDefault(), out bool parsedPrivateServersAllowed);
                
                // If access is Friends, private servers must be disabled
                if (accessValue.ToLower() == "friends")
                {
                    arePrivateServersAllowed = false;
                }
                else
                {
                    // Only use the form value if access is not Friends
                    arePrivateServersAllowed = parsedPrivateServersAllowed;
                }

                // Business rule validation: Paid access is not allowed when access is Friends
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

                // Update access settings using Games assembly helper
                bool privateServersPriceParsed = int.TryParse(Request.Form["PrivateServersPrice"].FirstOrDefault(), out int privateServersPrice);
                bool isFreePrivateServer = Request.Form["IsFreePrivateServer"].FirstOrDefault() == "true";
                
                // Business rule validation: Private servers are not allowed when access is Friends
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
                
                // Validate private server price minimum of 10 (only when private servers are not free)
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
                    // Parse PlayableDevices checkbox array into comma-separated string
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
                        return Json(new { success = false, message = $"Database error: {ex.Message}" });
                    }
                    
                    ViewBag.gameid = Id;
                    ViewBag.ErrorMessage = $"Database error occurred while saving access settings: {ex.Message}";
                    return View("~/Views/Pages/places/{id}/update.cshtml");
                }

                // Handle icon changes if any
                if (!string.IsNullOrWhiteSpace(iconType))
                {
                    try
                    {
                        if (iconType == "image")
                        {
                            // When IconType is image, set custom_icon = true and generated_icon = false
                            await ThumbnailQueries.ClearPlaceGeneratedIconAsync(connectionString, Id);
                            
                            // Check if we should commit custom icon to thumbnail and clear custom icon URL
                            var commitCustomIcon = Request.Form["commitCustomIcon"].FirstOrDefault() ?? "false";
                            
                            if (commitCustomIcon.ToLower() == "true")
                            {
                                var existingAsset = await _assetRepository.GetAssetByIdAsync(connectionString, Id);
                                
                                if (existingAsset != null && !string.IsNullOrWhiteSpace(existingAsset.PlaceCustomIconUrl))
                                {
                                    // Set the custom icon URL as the thumbnail URL
                                    await ThumbnailQueries.UpdateAssetThumbnailUrlsAsync(connectionString, Id, 
                                        existingAsset.PlaceCustomIconUrl, 
                                        existingAsset.PlaceCustomIconHighResUrl ?? existingAsset.PlaceCustomIconUrl);
                                    
                                    // Clear the custom icon URL and flags but keep custom_icon=true since we're committing to thumbnail
                                    await ThumbnailQueries.ClearPlaceCustomIconAsync(connectionString, Id);
                                    
                                }
                                else
                                {
                                }
                            }
                            else if (iconImageFile != null && iconImageFile.Length > 0)
                            {
                                // Process new custom icon upload
                                var baseUrl = _configuration["Thumbnails:ThumbnailUrl"] ?? $"{Request.Scheme}://{Request.Host}";
                                
                                var (iconUrl, iconUrlHighRes, fileHash) = await PlaceThumbnail.ProcessCustomIconAsync(
                                    Id, iconImageFile.OpenReadStream(), iconImageFile.FileName, iconImageFile.ContentType, baseUrl);

                                // Update place asset with custom icon information (this sets custom_icon = true)
                                await PlaceThumbnail.SetPlaceCustomIconAsync(connectionString, Id, iconUrl, iconUrlHighRes, fileHash);
                                
                                // Update both thumbnail_url and high_res_thumbnail_url to use custom icon URLs
                                await ThumbnailQueries.UpdateAssetThumbnailUrlsAsync(connectionString, Id, iconUrl, iconUrlHighRes);
                                
                            }
                            else
                            {
                                // No new file uploaded, but icon type is image - ensure custom icon is set and thumbnail URL matches
                                var existingAsset = await _assetRepository.GetAssetByIdAsync(connectionString, Id);
                                if (existingAsset != null && !string.IsNullOrWhiteSpace(existingAsset.PlaceCustomIconUrl))
                                {
                                    // Ensure custom_icon flag is set and thumbnail URL matches custom icon URL
                                    await PlaceThumbnail.SetPlaceCustomIconAsync(connectionString, Id, 
                                        existingAsset.PlaceCustomIconUrl, 
                                        existingAsset.PlaceCustomIconHighResUrl ?? existingAsset.PlaceCustomIconUrl,
                                        existingAsset.PlaceCustomIconHash ?? "");
                                    
                                    // Update both thumbnail_url and high_res_thumbnail_url to use custom icon URLs
                                    await ThumbnailQueries.UpdateAssetThumbnailUrlsAsync(connectionString, Id, 
                                        existingAsset.PlaceCustomIconUrl, 
                                        existingAsset.PlaceCustomIconHighResUrl ?? existingAsset.PlaceCustomIconUrl);
                                    
                                }
                            }
                        }
                        else if (iconType == "autogenerated")
                        {
                            // When IconType is autogenerated, set generated_icon = true and custom_icon = false
                            await ThumbnailQueries.ClearPlaceCustomIconAsync(connectionString, Id);
                            
                            // Get existing asset to check for existing generated icon
                            var existingAsset = await _assetRepository.GetAssetByIdAsync(connectionString, Id);
                            
                            if (existingAsset != null && !string.IsNullOrWhiteSpace(existingAsset.PlaceGeneratedIconUrl))
                            {
                                // Use existing generated icon URL
                                var generatedIconUrl = existingAsset.PlaceGeneratedIconUrl;
                                var generatedIconHighResUrl = existingAsset.PlaceGeneratedIconHighResUrl ?? generatedIconUrl;
                                
                                // Ensure generated_icon flag is set and thumbnail URL matches generated icon URL
                                await ThumbnailQueries.SetPlaceGeneratedIconFlagsAsync(connectionString, Id, generatedIconUrl, generatedIconHighResUrl, existingAsset.PlaceGeneratedIconHash ?? "");
                                
                                // Update both thumbnail_url and high_res_thumbnail_url to use generated icon URLs
                                await ThumbnailQueries.UpdateAssetThumbnailUrlsAsync(connectionString, Id, generatedIconUrl, generatedIconHighResUrl);
                                
                            }
                            else
                            {
                                // Fire-and-forget thumbnail generation
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
                                
                                // Return pending status immediately - thumbnail will be updated in background
                                var generatedIconUrl = "/images/RobloxLogo.png";
                                var generatedIconHighResUrl = generatedIconUrl;
                                
                            }
                        }
                        else
                        {
                            // For any other icon type, clear both flags as a safety measure
                            await ThumbnailQueries.ClearPlaceCustomIconAsync(connectionString, Id);
                            await ThumbnailQueries.ClearPlaceGeneratedIconAsync(connectionString, Id);
                        }
                    }
                    catch (Exception iconEx)
                    {
                        // Don't fail the whole operation if icon update fails, just log it
                    }
                }

                // Handle thumbnail type changes if any
                if (!string.IsNullOrWhiteSpace(thumbnailType))
                {
                    try
                    {
                        if (thumbnailType == "image")
                        {
                            // When ThumbnailType is image, set custom thumbnail flags and clear others
                            await ThumbnailQueries.ClearPlaceAutoGeneratedThumbnailAsync(connectionString, Id);
                            await ThumbnailQueries.ClearPlaceVideoThumbnailAsync(connectionString, Id);
                            
                            // Set custom thumbnail flag to true
                            await ThumbnailQueries.SetPlaceCustomThumbnailFlagAsync(connectionString, Id, true);
                        }
                        else if (thumbnailType == "video")
                        {
                            // When ThumbnailType is video, set video thumbnail flag and clear others
                            await ThumbnailQueries.ClearPlaceAutoGeneratedThumbnailAsync(connectionString, Id);
                            await ThumbnailQueries.ClearPlaceCustomThumbnailAsync(connectionString, Id);
                            
                            // Set video thumbnail flag to true
                            await ThumbnailQueries.SetPlaceVideoThumbnailFlagAsync(connectionString, Id, true);
                        }
                        else if (thumbnailType == "autogenerated")
                        {
                            // When ThumbnailType is autogenerated, set auto-generated flag and clear others
                            await ThumbnailQueries.ClearPlaceCustomThumbnailAsync(connectionString, Id);
                            await ThumbnailQueries.ClearPlaceVideoThumbnailAsync(connectionString, Id);
                            
                            // Set auto-generated thumbnail flag to true
                            await ThumbnailQueries.SetPlaceAutoGeneratedThumbnailFlagAsync(connectionString, Id, true);
                        }
                        else
                        {
                            // For any other thumbnail type, clear all flags as a safety measure
                            await ThumbnailQueries.ClearPlaceCustomThumbnailAsync(connectionString, Id);
                            await ThumbnailQueries.ClearPlaceVideoThumbnailAsync(connectionString, Id);
                            await ThumbnailQueries.ClearPlaceAutoGeneratedThumbnailAsync(connectionString, Id);
                        }
                    }
                    catch (Exception thumbnailEx)
                    {
                        // Don't fail the whole operation if thumbnail update fails, just log it
                    }
                }

                // Parse copying permission setting
                bool.TryParse(Request.Form["IsCopyingAllowed"].FirstOrDefault(), out isCopyingAllowed);

                // Business rule: If sell game access is enabled, automatically disable copying
                if (sellGameAccess)
                {
                    isCopyingAllowed = false;
                }

                // Parse gear permission settings
                bool isAllGenresAllowed = false;
                bool.TryParse(Request.Form["IsAllGenresAllowed"].FirstOrDefault(), out isAllGenresAllowed);
                
                // Parse in-game permission settings
                bool allowPlaceToBeCopiedInGame = false;
                bool allowPlaceToBeUpdatedInGame = false;
                bool.TryParse(Request.Form["AllowPlaceToBeCopiedInGame"].FirstOrDefault(), out allowPlaceToBeCopiedInGame);
                bool.TryParse(Request.Form["AllowPlaceToBeUpdatedInGame"].FirstOrDefault(), out allowPlaceToBeUpdatedInGame);
                
                // Parse gear type checkboxes - collect all selected gear type categories
                var selectedGearTypes = new List<string>();
                
                // Check if this is an AJAX request with JSON data (from Configure.js)
                var allowedGearTypesFromForm = Request.Form["AllowedGearTypes"].FirstOrDefault();
                
                if (!string.IsNullOrEmpty(allowedGearTypesFromForm) && allowedGearTypesFromForm.StartsWith("["))
                {
                    // Parse JSON array of gear type IDs from JavaScript
                    try
                    {
                        // Parse as List<int> and use the IDs directly for storage (more space efficient)
                        var gearTypeIds = System.Text.Json.JsonSerializer.Deserialize<List<int>>(allowedGearTypesFromForm);
                        
                        if (gearTypeIds != null)
                        {
                            // Store the numeric IDs directly (more space efficient than category names)
                            selectedGearTypes = gearTypeIds.Select(id => id.ToString()).ToList();
                        }
                    }
                    catch (System.Text.Json.JsonException ex)
                    {
                        // If JSON parsing fails, fall back to empty array
                        selectedGearTypes = new List<string>();
                    }
                }
                else
                {
                    // Parse from form checkbox array (direct form submission)
                    // Note: This path is rarely used since most submissions go through AJAX
                    foreach (var key in Request.Form.Keys)
                    {
                        if (key.StartsWith("AllowedGearTypes[") && key.EndsWith(".IsSelected"))
                        {
                            var isSelected = Request.Form[key].FirstOrDefault() == "true";
                            if (isSelected)
                            {
                                // Extract the index from the key (e.g., "AllowedGearTypes[0].IsSelected" -> "0")
                                var indexStr = key.Substring("AllowedGearTypes[".Length, key.IndexOf("]") - "AllowedGearTypes[".Length);
                                if (int.TryParse(indexStr, out var index))
                                {
                                    // Use the index + 1 as the gear type ID (since checkboxes are 0-indexed but gear types are 1-indexed)
                                    selectedGearTypes.Add((index + 1).ToString());
                                }
                            }
                        }
                    }
                }
                
                
                string allowedGearTypesJson = selectedGearTypes.Count > 0 
                    ? System.Text.Json.JsonSerializer.Serialize(selectedGearTypes.Select(int.Parse)) 
                    : "[]";
                    

                // Update permission settings using Games assembly
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

                // Update in-game permissions using Games assembly
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

                // Return JSON response for AJAX requests
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
                    return Json(new { success = false, message = ex.Message });
                }
                
                ModelState.AddModelError("", "An error occurred while saving. Please try again.");
                return View();
            }
        }

        /// <summary>
        /// POST /places/thumbnails/add-image - Handle thumbnail image upload for a place
        /// </summary>
        [HttpPost("places/thumbnails/add-image")]
        [Authorize] // Ensure authentication is required
        public async Task<IActionResult> AddThumbnailImage(IFormFile thumbnailImageFile)
        {
            try
            {
                // Get current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                if (thumbnailImageFile == null || thumbnailImageFile.Length == 0)
                {
                    return Json(new { success = false, message = "No file uploaded" });
                }

                // Validate file type
                if (!thumbnailImageFile.ContentType.StartsWith("image/"))
                {
                    return Json(new { success = false, message = "Invalid file type. Please upload an image file." });
                }

                // Get place ID from form
                var placeIdStr = Request.Form["id"].FirstOrDefault();
                if (string.IsNullOrWhiteSpace(placeIdStr) || !long.TryParse(placeIdStr, out var placeId))
                {
                    return Json(new { success = false, message = "Invalid place ID" });
                }

                // Get place asset to verify ownership
                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var placeAsset = await _assetRepository.GetAssetByIdAsync(connectionString, placeId);
                
                if (placeAsset == null)
                {
                    return Json(new { success = false, message = "Place not found" });
                }

                // Check if user owns this place
                if (placeAsset.OwnerUserId != currentUserId)
                {
                    return Json(new { success = false, message = "Access denied" });
                }

                // Check thumbnail limit (max 10 thumbnails per place)
                var existingThumbnails = await PlaceThumbnail.GetPlaceThumbnailsAsync(connectionString, placeId);
                var imageThumbnailCount = existingThumbnails.Count(t => 
                    t.GetType().GetProperty("type")?.GetValue(t)?.ToString() == "image");
                
                if (imageThumbnailCount >= 10)
                {
                    return Json(new { success = false, message = "Maximum limit of 10 thumbnails per place has been reached." });
                }

                // Process uploaded image using PlaceThumbnail helper
                var baseUrl = _configuration["Thumbnails:ThumbnailUrl"] ?? $"{Request.Scheme}://{Request.Host}";
                
                var (thumbnailUrl, fileHash) = await PlaceThumbnail.ProcessThumbnailImageAsync(
                    placeId, thumbnailImageFile.OpenReadStream(), thumbnailImageFile.FileName, thumbnailImageFile.ContentType, baseUrl);

                // Insert into place_thumbnails table using helper (checks for duplicates)
                var thumbnailResult = await PlaceThumbnail.AddThumbnailImageAsync(
                    connectionString, placeId, placeAsset.Name, thumbnailUrl, fileHash);

                if (thumbnailResult == null)
                {
                    return Json(new { success = false, message = "This image has already been added to your thumbnails." });
                }

                // Update assets table flags (but don't replace main thumbnail URL)
                await PlaceThumbnail.UpdatePlaceThumbnailFlagsAsync(connectionString, placeId, hasCustom: true, hasVideo: false, hasAutogenerated: false);
                
                // Get updated thumbnails list to return to client
                var updatedThumbnails = await PlaceThumbnail.GetPlaceThumbnailsAsync(connectionString, placeId);
                
                return Json(new { 
                    success = true, 
                    message = "Thumbnail uploaded successfully",
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while uploading the thumbnail. Please try again." });
            }
        }

        /// <summary>
        /// POST /places/thumbnails/add-video - Handle thumbnail video upload for a place
        /// </summary>
        [HttpPost("places/thumbnails/add-video")]
        [Authorize] // Ensure authentication is required
        public async Task<IActionResult> AddThumbnailVideo()
        {
            try
            {
                // Get current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                // Get video URL from form
                var videoUrl = Request.Form["thumbnailYoutubeUrl"].FirstOrDefault();
                
                if (string.IsNullOrWhiteSpace(videoUrl))
                {
                    return Json(new { success = false, message = "Video URL is required" });
                }

                // Validate YouTube URL format
                if (!YouTubeUtilities.IsValidYouTubeURL(videoUrl))
                {
                    return Json(new { success = false, message = "Invalid YouTube URL." });
                }

                // Get place ID from form
                var placeIdStr = Request.Form["id"].FirstOrDefault();
                
                if (string.IsNullOrWhiteSpace(placeIdStr) || !long.TryParse(placeIdStr, out var placeId))
                {
                    return Json(new { success = false, message = "Invalid place ID" });
                }

                // Get place asset to verify ownership
                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var placeAsset = await _assetRepository.GetAssetByIdAsync(connectionString, placeId);
                
                if (placeAsset == null)
                {
                    return Json(new { success = false, message = "Place not found" });
                }

                // Check if user owns this place
                if (placeAsset.OwnerUserId != currentUserId)
                {
                    return Json(new { success = false, message = "Access denied" });
                }

                // Update only the place_thumbnail_video field in assets table (temporary storage)
                await PlaceThumbnail.SetPlaceVideoUrlTempAsync(connectionString, placeId, videoUrl);

                // Do NOT update flags or set as primary thumbnail yet - wait for user to press save
                // The video will only appear in main thumbnail box after save is pressed
                
                // Get updated thumbnails list to return to client
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
        [Authorize] // Ensure authentication is required
        public async Task<IActionResult> RemoveThumbnail()
        {
            try
            {
                // Get current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                // Get thumbnail ID from form
                var thumbnailIdStr = Request.Form["thumbnailid"].FirstOrDefault();
                if (string.IsNullOrWhiteSpace(thumbnailIdStr) || !long.TryParse(thumbnailIdStr, out var thumbnailId))
                {
                    return Json(new { success = false, message = "Invalid thumbnail ID" });
                }

                // Get place ID from form
                var placeIdStr = Request.Form["id"].FirstOrDefault();
                if (string.IsNullOrWhiteSpace(placeIdStr) || !long.TryParse(placeIdStr, out var placeId))
                {
                    return Json(new { success = false, message = "Invalid place ID" });
                }

                // Verify ownership
                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var placeAsset = await _assetRepository.GetAssetByIdAsync(connectionString, placeId);
                
                if (placeAsset == null || placeAsset.OwnerUserId != currentUserId)
                {
                    return Json(new { success = false, message = "Access denied" });
                }

                // Delete the thumbnail using helper
                var success = await PlaceThumbnail.RemoveThumbnailAsync(connectionString, placeId, thumbnailId);
                
                if (success)
                {
                    // Update assets table flags if needed
                    await PlaceThumbnail.UpdatePlaceThumbnailFlagsAsync(connectionString, placeId, hasCustom: false, hasVideo: false, hasAutogenerated: false);
                    
                    // Get updated thumbnails list to return to client
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
                // Get current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Content("<tbody><tr><td colspan='4'>User not authenticated</td></tr></tbody>", "text/html");
                }

                // Verify place ownership
                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var placeAsset = await _assetRepository.GetAssetByIdAsync(connectionString, assetID);
                
                if (placeAsset == null || placeAsset.OwnerUserId != currentUserId)
                {
                    return Content("<tbody><tr><td colspan='4'>Access denied</td></tr></tbody>", "text/html");
                }

                // Generate complete HTML response including table headers
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

        /// <summary>
        /// GET /universes/{universeId}/developer-products - Get developer products listing for a universe
        /// </summary>
        [HttpGet("universes/{universeId}/developer-products")]
        [Authorize]
        public async Task<IActionResult> GetUniverseDeveloperProducts(long universeId, int page = 1)
        {
            try
            {
                // Get current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Content("<div class=\"error\">User not authenticated</div>");
                }

                // Verify universe ownership
                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var universeOwnerId = await GamesRepository.GetUniverseOwnerAsync(connectionString, universeId);
                
                if (!universeOwnerId.HasValue || universeOwnerId.Value != currentUserId)
                {
                    return Content("<div class=\"error\">Access denied</div>");
                }

                // Get a place ID from this universe for create/edit URLs
                var placeIdForUrls = await GetFirstPlaceIdFromUniverseAsync(connectionString, universeId);

                // Get paginated developer products from universe
                const int pageSize = 10;
                var (developerProducts, totalCount) = await DevProductHandler.GetUniverseDeveloperProductsPaginatedAsync(connectionString, universeId, page, pageSize);
                
                // Build status messages (success and error) using TempData so HTML is rendered by the view, not JS
                var successMessageHtml = "";
                var productCreated = TempData["DeveloperProductCreated"] as bool? ?? false;
                var createdProductId = TempData["DeveloperProductId"] as string;
                var productUpdated = TempData["DeveloperProductUpdated"] as bool? ?? false;
                var updatedProductId = TempData["DeveloperProductId"] as string;

                if (productCreated && !string.IsNullOrEmpty(createdProductId))
                {
                    successMessageHtml = $@"<div class=""status-confirm"" style=""margin-bottom: 10px; width: 60%;"">
    Product {createdProductId} successfully created
</div>";
                }
                else if (productUpdated && !string.IsNullOrEmpty(updatedProductId))
                {
                    successMessageHtml = $@"<div class=""status-confirm"" style=""margin-bottom: 10px; width: 60%;"">
    Product {updatedProductId} successfully updated
</div>";
                }

                var errorMessageHtml = "";
                var errorMessage = TempData["DeveloperProductError"] as string;
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessageHtml = $@"<div class=""status-error"" style=""margin-bottom: 10px; width: 60%;"">
    {System.Net.WebUtility.HtmlEncode(errorMessage)}
</div>";
                }
                
                if (developerProducts == null || developerProducts.Count == 0)
                {
                    // Return empty state HTML
                    var emptyHtml = $@"<div class=""headline"">
    <h2 style=""display: inline-block; vertical-align: middle;"">Developer Products</h2>
    <div class=""createNewButtonSection"" style=""display: inline-block; margin-left: 10px; vertical-align: middle; line-height: 1;"">
        <a class=""btn-small btn-neutral developer-product-button"" id=""createNewButton"" data-form-post-url=""/places/{placeIdForUrls}/developer-products/create"" data-url=""/places/{placeIdForUrls}/developer-products/create"">Create new</a>
    </div>
    <div style=""clear: both;""></div>
</div>
{errorMessageHtml}{successMessageHtml}
<p style=""margin: 0;"">You do not have any developer products. Click <a href=""https://create.roblox.com/docs/production/monetization/developer-products"" target=""_blank"">here</a> for more information on<br>developer products.</p>";
                    
                    return Content(emptyHtml, "text/html; charset=utf-8");
                }

                // Generate HTML for developer products listing
                var productsHtmlList = developerProducts.Select(product => 
                {
                    var productId = product.GetProperty("developerProductId").GetInt64();
                    var name = product.GetProperty("name").GetString() ?? "";
                    var description = product.GetProperty("description").GetString() ?? "";
                    var price = product.GetProperty("priceInRobux").GetInt32();
                    
                    var ticketPrice = DevProductHandler.GetTicketPriceFromJson(product);
                    
                    long? imageAssetId = null;
                    if (product.TryGetProperty("imageAssetId", out var imageAssetElement) && imageAssetElement.ValueKind != System.Text.Json.JsonValueKind.Null)
                    {
                        imageAssetId = imageAssetElement.GetInt64();
                    }

                    var imageUrl = imageAssetId.HasValue ? $"/game-assets/image?assetId={imageAssetId.Value}&width=150&height=150" : "/images/empty-asset.png";

                    return $@"<tr class=""dev-product-row"" data-product-id=""{productId}"">
    <td class=""dev-product-id"">{productId}</td>
    <td class=""dev-product-name"">
        <div class=""dev-product-title"">{name}</div>
    </td>
    <td class=""dev-product-price"">
        <span class=""robux-price"">{price} R$</span>
    </td>
    <td class=""dev-product-ticket-price"">
        <span class=""ticket-price"">{ticketPrice} Tix</span>
    </td>
    <td class=""dev-product-actions"">
        <a class=""btn-small btn-neutral edit"" style=""text-align: left;""data-form-post-url=""/places/{placeIdForUrls}/developer-products/{productId}/configure"" data-url=""/places/{placeIdForUrls}/developer-products/{productId}/configure"">Edit</a>
    </td>
</tr>";
                }).ToList();

                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                var hasPreviousPage = page > 1;
                var hasNextPage = page < totalPages;

                var paginationHtml = "";
                if (totalPages > 1)
                {
                    paginationHtml = $@"<div class=""developerProductpagerContainer"">
    <a class=""pager first {(hasPreviousPage ? "" : "disabled")}"" data-page=""1""></a>
    <a class=""pager previous {(hasPreviousPage ? "" : "disabled")}"" data-page=""{Math.Max(1, page - 1)}""></a>
    <span class=""page text"">
        Page <span class=""robloxDeveloperProductsPageNum"">{page}</span> of {totalPages}
    </span>
    <a class=""pager next {(hasNextPage ? "" : "disabled")}"" data-page=""{page + 1}""></a>
    <a class=""pager last {(hasNextPage ? "" : "disabled")}"" data-page=""{totalPages}""></a>
</div>";
                }

                var html = $@"<div class=""headline"">
    <h2 style=""display: inline-block; vertical-align: middle;"">Developer Products</h2>
    <div class=""createNewButtonSection"" style=""display: inline-block; margin-left: 10px; vertical-align: middle; line-height: 1;"">
        <a class=""btn-small btn-neutral developer-product-button"" id=""createNewButton"" data-form-post-url=""/places/{placeIdForUrls}/developer-products/create"" data-url=""/places/{placeIdForUrls}/developer-products/create"">Create new</a>
    </div>
    <div style=""clear: both;""></div>
</div>
{errorMessageHtml}{successMessageHtml}
<div class=""developerProductsTableContainer"">
    <table id=""DeveloperProductsTable"">
        <thead>
            <tr>
                <th>ID</th>
                <th>Name</th>
                <th>Price In Robux</th>
                <th>Price In Tickets</th>
                <th>Edit</th>
            </tr>
        </thead>
        <tbody>
            {string.Join("", productsHtmlList)}
        </tbody>
    </table>
</div>
{paginationHtml}";

                return Content(html, "text/html; charset=utf-8");
            }
            catch (Exception ex)
            {
                return Content("<div class=\"error\">An error occurred while loading developer products. Please try again.</div>", "text/html");
            }
        }

        /// <summary>
        /// Helper method to get the first place ID from a universe for URL generation
        /// </summary>
        private async Task<long> GetFirstPlaceIdFromUniverseAsync(string connectionString, long universeId)
        {
            return await GamesQueries.GetFirstPlaceIdFromUniverseAsync(universeId, connectionString);
        }

        /// <summary>
        /// GET /places/{id}/developer-products - Redirect to universe-level developer products
        /// </summary>
        [HttpGet("places/{id}/developer-products")]
        [Authorize]
        public async Task<IActionResult> GetDeveloperProducts(long id, int page = 1)
        {
            try
            {
                // Get current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Content("<div class=\"error\">User not authenticated</div>");
                }

                // Get universe ID for the place and redirect to universe-level endpoint
                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var universeId = await GamesRepository.GetUniverseIdFromPlaceIdAsync(connectionString, id);
                if (!universeId.HasValue)
                {
                    return Content("<div class=\"error\">Universe not found for this place</div>");
                }

                // Redirect to the universe-level endpoint
                return Redirect($"/universes/{universeId.Value}/developer-products?page={page}");
            }
            catch (Exception ex)
            {
                return Content("<div class=\"error\">An error occurred while loading developer products. Please try again.</div>", "text/html");
            }
        }

        /// <summary>
        /// GET /places/{id}/developer-products/create - Show developer product creation page
        /// </summary>
        [HttpGet("places/{id}/developer-products/create")]
        [Authorize]
        public async Task<IActionResult> CreateDeveloperProductPage(long id)
        {
            try
            {
                // Get current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Redirect("/login");
                }

                // Verify place ownership
                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var placeAsset = await _assetRepository.GetAssetByIdAsync(connectionString, id);
                
                if (placeAsset == null || placeAsset.OwnerUserId != currentUserId)
                {
                    return Redirect("/404");
                }

                // Get universe ID for the place
                var universeId = await GamesRepository.GetUniverseIdFromPlaceIdAsync(connectionString, id);
                
                // Set ViewBag data for the view
                ViewBag.gameid = placeAsset.AssetId;
                ViewBag.PlaceUniverseId = universeId ?? id; // Fallback to place ID if universe not found
                ViewBag.gamename = placeAsset.Name ?? "";
                ViewBag.gamedesc = placeAsset.Description ?? "";
                
                // Pass success message data if available
                ViewBag.productCreated = TempData["DeveloperProductCreated"] as bool? ?? false;
                ViewBag.productId = TempData["DeveloperProductId"] as string;
                
                // Return the specific developer products create view
                return View("~/Views/Pages/places/{id}/developer-products/create.cshtml");
            }
            catch (Exception ex)
            {
                return Redirect("/404");
            }
        }

        /// <summary>
        /// GET /places/{id}/developer-products/{productId}/configure - Show developer product configuration page
        /// </summary>
        [HttpGet("places/{id}/developer-products/{productId}/configure")]
        [Authorize]
        public async Task<IActionResult> ConfigureDeveloperProductPage(long id, long productId)
        {
            try
            {
                // Get current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Redirect("/login");
                }

                // Verify place ownership
                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var placeAsset = await _assetRepository.GetAssetByIdAsync(connectionString, id);
                
                if (placeAsset == null || placeAsset.OwnerUserId != currentUserId)
                {
                    return Redirect("/404");
                }

                // Get universe ID for the place
                var universeId = await GamesRepository.GetUniverseIdFromPlaceIdAsync(connectionString, id);
                
                // Set ViewBag data for the view
                ViewBag.gameid = placeAsset.AssetId;
                ViewBag.PlaceUniverseId = universeId ?? id; // Fallback to place ID if universe not found
                ViewBag.gamename = placeAsset.Name ?? "";
                ViewBag.gamedesc = placeAsset.Description ?? "";
                ViewBag.DeveloperProductId = productId;
                
                // Return the specific developer products configure view
                return View("~/Views/Pages/places/{id}/developer-products/{id}/configure.cshtml");
            }
            catch (Exception ex)
            {
                return Redirect("/404");
            }
        }

        /// <summary>
        /// GET /places/{id}/developer-products/{productId}/data - Get developer product data for editing
        /// </summary>
        [HttpGet("places/{id}/developer-products/{productId}/data")]
        [Authorize]
        public async Task<IActionResult> GetDeveloperProductData(long id, long productId)
        {
            try
            {
                // Get current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                // Verify place ownership
                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var placeAsset = await _assetRepository.GetAssetByIdAsync(connectionString, id);
                
                if (placeAsset == null || placeAsset.OwnerUserId != currentUserId)
                {
                    return Json(new { success = false, message = "Access denied" });
                }

                // Get universe ID for the place
                var universeId = await GamesRepository.GetUniverseIdFromPlaceIdAsync(connectionString, id);
                if (!universeId.HasValue)
                {
                    return Json(new { success = false, message = "Universe not found" });
                }

                // Get developer products from universe storage
                var (products, _) = await DevProductHandler.GetUniverseDeveloperProductsPaginatedAsync(connectionString, universeId.Value, 1, 1000);
                
                if (products == null)
                {
                    return Json(new { success = false, message = "No products found" });
                }

                // Find the specific product
                var product = products.FirstOrDefault(p => 
                {
                    if (p.TryGetProperty("developerProductId", out var idElement) && idElement.ValueKind != JsonValueKind.Null)
                    {
                        return idElement.GetInt64() == productId;
                    }
                    return false;
                });

                if (product.ValueKind == JsonValueKind.Undefined)
                {
                    return Json(new { success = false, message = "Product not found" });
                }

                // Extract product data
                var productData = new
                {
                    developerProductId = productId,
                    name = product.TryGetProperty("name", out var nameElement) && nameElement.ValueKind != JsonValueKind.Null ? nameElement.GetString() : "",
                    description = product.TryGetProperty("description", out var descElement) && descElement.ValueKind != JsonValueKind.Null ? descElement.GetString() : "",
                    priceInRobux = product.TryGetProperty("priceInRobux", out var robuxElement) && robuxElement.ValueKind != JsonValueKind.Null ? robuxElement.GetInt32() : 0,
                    priceInTix = DevProductHandler.GetTicketPriceFromJson(product),
                    imageAssetId = product.TryGetProperty("imageAssetId", out var imageElement) && imageElement.ValueKind != JsonValueKind.Null ? (long?)imageElement.GetInt64() : null
                };

                return Json(productData);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while loading product data" });
            }
        }

        /// <summary>
        /// POST /universes/{universeId}/developer-products/upload-image - Handle developer product image upload for universe
        /// </summary>
        [HttpPost("universes/{universeId}/developer-products/upload-image")]
        [Authorize]
        public async Task<IActionResult> UploadUniverseDeveloperProductImage(long universeId, IFormFile? image)
        {
            try
            {
                // Get current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                // Validate universe ownership
                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var universeOwnerId = await GamesRepository.GetUniverseOwnerAsync(connectionString, universeId);
                
                if (!universeOwnerId.HasValue || universeOwnerId.Value != currentUserId)
                {
                    return Json(new { success = false, message = "Access denied - you do not own this universe" });
                }

                if (image == null || image.Length == 0 || string.IsNullOrWhiteSpace(image.FileName))
                {
                    return Json(new { success = false, message = "No file uploaded or file is empty" });
                }

                // Validate file type
                if (!image.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                {
                    return Json(new { success = false, message = "Invalid file type. Please upload an image file." });
                }

                // Additional validation for file size (max 10MB)
                const long maxFileSize = 10 * 1024 * 1024; // 10MB
                if (image.Length > maxFileSize)
                {
                    return Json(new { success = false, message = "File size too large. Maximum size is 10MB." });
                }

                // Create an asset record for the uploaded image
                var baseUrl = _configuration["Thumbnails:ThumbnailUrl"] ?? $"{Request.Scheme}://{Request.Host}";
                
                string imageUrl, fileHash;
                try
                {
                    (imageUrl, fileHash) = await DevProductHandler.ProcessDeveloperProductImageAsync(
                        image.OpenReadStream(), image.FileName, image.ContentType, baseUrl);
                }
                catch (Exception ex)
                {
                    return Json(new { 
                        success = false, 
                        message = $"Failed to process image: {ex.Message}",
                        details = ex.ToString()
                    });
                }

                // Create asset record for the developer product image
                long imageAssetId;
                try
                {
                    imageAssetId = await DevProductHandler.CreateDeveloperProductImageAsset(
                        connectionString, 
                        image.FileName, 
                        imageUrl, 
                        fileHash,
                        currentUserId);
                }
                catch (Exception ex)
                {
                    return Json(new { 
                        success = false, 
                        message = $"Failed to create asset record: {ex.Message}",
                        details = ex.ToString()
                    });
                }

                return Content($@"<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ 
            margin: 0; 
            padding: 0; 
            font-family: Arial, sans-serif; 
        }}
        .upload-success {{ 
            text-align: center; 
            padding: 20px; 
            background-color: #f0f8ff; 
        }}
        .image-preview {{ 
            max-width: 150px; 
            max-height: 150px; 
            margin: 10px auto; 
            display: block; 
            border: 1px solid #ccc; 
        }}
    </style>
</head>
<body>
    <div class=""upload-success"">
        <h3>Image uploaded successfully!</h3>
        <img src=""{imageUrl}"" alt=""Preview"" class=""image-preview"" />
        <p>Asset ID: {imageAssetId}</p>
    </div>
    <script>
        // Notify parent window about successful upload
        if (window.parent && window.parent.postMessage) {{
            window.parent.postMessage({{
                type: 'imageUploadSuccess',
                assetId: {imageAssetId},
                imageUrl: '{imageUrl}'
            }}, '*');
        }}
    </script>
</body>
</html>", "text/html");
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = $"An error occurred during image upload: {ex.Message}",
                    details = ex.ToString()
                });
            }
        }

        /// <summary>
        /// POST /places/{id}/developer-products/upload-image - Handle developer product image upload
        /// </summary>
        [HttpPost("places/{id}/developer-products/upload-image")]
        [Authorize]
        public async Task<IActionResult> UploadDeveloperProductImage(long id, IFormFile? image)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var placeAsset = await _assetRepository.GetAssetByIdAsync(connectionString, id);
                
                if (placeAsset == null || placeAsset.OwnerUserId != currentUserId)
                {
                    return Json(new { success = false, message = "Access denied" });
                }

                var universeId = await GamesRepository.GetUniverseIdFromPlaceIdAsync(connectionString, id);
                if (universeId.HasValue)
                {
                    var universeOwner = await GamesRepository.GetUniverseOwnerAsync(connectionString, universeId.Value);
                    if (universeOwner == null || universeOwner != currentUserId)
                    {
                        return Json(new { success = false, message = "Access denied - you do not own this universe" });
                    }
                }

                if (image == null || image.Length == 0 || string.IsNullOrWhiteSpace(image.FileName))
                {
                    return Json(new { success = false, message = "No file uploaded or file is empty" });
                }

                if (!image.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                {
                    return Json(new { success = false, message = "Invalid file type. Please upload an image file." });
                }
                const long maxFileSize = 10 * 1024 * 1024; // 10MB
                if (image.Length > maxFileSize)
                {
                    return Json(new { success = false, message = "File size too large. Maximum size is 10MB." });
                }

                var baseUrl = _configuration["Thumbnails:ThumbnailUrl"] ?? $"{Request.Scheme}://{Request.Host}";
                
                string imageUrl, fileHash;
                try
                {
                    (imageUrl, fileHash) = await DevProductHandler.ProcessDeveloperProductImageAsync(
                        image.OpenReadStream(), image.FileName, image.ContentType, baseUrl);
                }
                catch (Exception ex)
                {
                    return Json(new { 
                        success = false, 
                        message = $"Failed to process image: {ex.Message}",
                        details = ex.ToString()
                    });
                }

                long imageAssetId;
                try
                {
                    imageAssetId = await DevProductHandler.CreateDeveloperProductImageAsset(
                        connectionString, 
                        image.FileName, 
                        imageUrl, 
                        fileHash,
                        currentUserId);
                }
                catch (Exception ex)
                {
                    return Json(new { 
                        success = false, 
                        message = $"Failed to create asset record: {ex.Message}",
                        details = ex.ToString()
                    });
                }

                return Content($@"<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ 
            margin: 0; 
            padding: 0; 
            display: flex; 
            justify-content: center; 
            align-items: center; 
            min-height: 256px; 
            min-width: 256px; 
            overflow: hidden; 
        }} 
        img {{ 
            display: block; 
            width: 1000px; 
            height: 256px; 
            object-fit: contain; 
        }}
    </style>
</head>
<body>
    <img src=""{imageUrl}"" alt=""Product Image"" />
    <script>
        // Communicate the asset ID back to the parent window
        window.parent.postMessage({{
            type: 'imageUploadComplete',
            success: true,
            assetId: {imageAssetId},
            imageUrl: '{imageUrl}'
        }}, '*');
    </script>
</body>
</html>", "text/html");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while uploading the image. Please try again." });
            }
        }

        /// <summary>
        /// Gets the CDN URL for a developer product image asset
        /// </summary>
        [HttpGet("places/{id}/developer-products/image/{assetId}")]
        public async Task<IActionResult> GetDeveloperProductImage(long id, long assetId, int width = 150, int height = 150)
        {
            try
            {
                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);

                var assetInfo = await DevProductHandler.GetDeveloperProductAssetAsync(connectionString, assetId);
                if (assetInfo == null)
                {
                    return NotFound(new { error = "Product image asset not found" });
                }

                var (thumbnailUrl, fileName, contentHash) = assetInfo.Value;

                if (!string.IsNullOrEmpty(thumbnailUrl))
                {
                    return Json(new { 
                        success = true, 
                        imageUrl = thumbnailUrl,
                        assetId = assetId,
                        fileName = fileName
                    });
                }

                var baseUrl = _configuration["Thumbnails:ThumbnailUrl"] ?? $"{Request.Scheme}://{Request.Host}";
                var cdnUrl = $"{baseUrl.TrimEnd('/')}/dev-product-icons/{contentHash}.png";

                return Json(new { 
                    success = true, 
                    imageUrl = cdnUrl,
                    assetId = assetId,
                    fileName = fileName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve product image", details = ex.Message });
            }
        }

        /// <summary>
        /// GET /places/{id}/developer-products/validate-name - Validate developer product name
        /// </summary>
        [HttpGet("places/{id}/developer-products/validate-name")]
        [Authorize]
        public async Task<IActionResult> ValidateDeveloperProductName(long id, string developerProductName, long? developerProductId = null)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Json(new { Success = false, Message = "User not authenticated" });
                }

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var isValidPlace = await PlaceValidationHelper.ValidatePlaceOwnershipAsync(id, currentUserId, connectionString, _assetRepository);
                if (!isValidPlace)
                {
                    return Json(new { Success = false, Message = "Access denied" });
                }

                var universeId = await GamesRepository.GetUniverseIdFromPlaceIdAsync(connectionString, id);
                if (!universeId.HasValue)
                {
                    return Json(new { Success = false, Message = "Universe not found" });
                }

                var universeOwner = await GamesRepository.GetUniverseOwnerAsync(connectionString, universeId.Value);
                if (universeOwner == null || universeOwner != currentUserId)
                {
                    return Json(new { Success = false, Message = "Access denied - you do not own this universe" });
                }

                if (string.IsNullOrWhiteSpace(developerProductName))
                {
                    return Json(new { Success = false, Message = "Name cannot be empty" });
                }

                if (developerProductName.Trim().Length == 0)
                {
                    return Json(new { Success = false, Message = "Name cannot be empty" });
                }

                if (developerProductName.Length > 50)
                {
                    return Json(new { Success = false, Message = "Name is too long (max 50 characters)" });
                }

                if (CharacterValidationUtility.ContainsDangerousContent(developerProductName))
                {
                    return Json(new { Success = false, Message = "Name contains invalid characters" });
                }

                var isUnique = await DevProductHandler.IsDeveloperProductNameUniqueAsync(
                    connectionString,
                    universeId.Value,
                    developerProductName,
                    developerProductId);

                if (!isUnique)
                {
                    return Json(new { Success = false, Message = "A developer product with this name already exists in this universe" });
                }

                // Name is valid
                return Json(new { Success = true, Message = "" });
            }
            catch (Exception)
            {
                return Json(new { Success = false, Message = "An error occurred while validating the name" });
            }
        }

        /// <summary>
        /// POST /places/{id}/developer-products/create - Create or update a developer product
        /// </summary>
        [HttpPost("places/{id}/developer-products/create")]
        [Authorize]
        public async Task<IActionResult> CreateDeveloperProduct(long id)
        {
            try
            {

                var (isAuthenticated, currentUserId) = AuthenticationHelper.GetCurrentUserId(User);
                if (!isAuthenticated)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                // Validate place ownership
                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var placeAsset = await _assetRepository.GetAssetByIdAsync(connectionString, id);
                
                if (placeAsset == null || placeAsset.OwnerUserId != currentUserId)
                {

                    return Json(new { success = false, message = "Access denied" });
                }

                var name = Request.Form["name"].FirstOrDefault();
                var description = Request.Form["description"].FirstOrDefault();
                var priceInRobuxStr = Request.Form["priceInRobux"].FirstOrDefault();
                var priceInTixStr = Request.Form["priceInTix"].FirstOrDefault();
                var imageAssetIdStr = Request.Form["imageAssetId"].FirstOrDefault();
                var universeIdStr = Request.Form["universeId"].FirstOrDefault();
                var productIdStr = Request.Form["developerProductId"].FirstOrDefault();

                if (string.IsNullOrWhiteSpace(name))
                {
                    return Json(new { success = false, message = "Product name is required" });
                }

                int priceInRobux = 0;
                if (!string.IsNullOrWhiteSpace(priceInRobuxStr))
                {
                    if (!int.TryParse(priceInRobuxStr, out priceInRobux) || priceInRobux < 0)
                    {
                        return Json(new { success = false, message = "Robux price must be a non-negative number" });
                    }
                }

                int priceInTix = 0;
                if (!string.IsNullOrWhiteSpace(priceInTixStr))
                {
                    if (!int.TryParse(priceInTixStr, out priceInTix) || priceInTix < 0)
                    {
                        return Json(new { success = false, message = "Tickets price must be a non-negative number" });
                    }
                }

                if (priceInRobux < 0 || priceInTix < 0)
                {
                    return Json(new { success = false, message = "Prices cannot be negative" });
                }

                if (!long.TryParse(universeIdStr, out var universeId) || universeId <= 0)
                {
                    return Json(new { success = false, message = "Valid universe ID is required" });
                }


                // Validate universe ownership
                var universeOwner = await GamesRepository.GetUniverseOwnerAsync(connectionString, universeId);
                if (universeOwner == null || universeOwner != currentUserId)
                {
                    return Json(new { success = false, message = "Access denied - you do not own this universe" });
                }

                // Parse image asset ID if provided
                long? imageAssetId = null;
                if (!string.IsNullOrWhiteSpace(imageAssetIdStr) && long.TryParse(imageAssetIdStr, out var parsedImageAssetId))
                {
                    imageAssetId = parsedImageAssetId;
                }

                // Check if this is an update (productId provided) or create
                long existingProductId = 0;
                bool isUpdate = !string.IsNullOrWhiteSpace(productIdStr) && long.TryParse(productIdStr, out existingProductId);
                long productId;


                // Enforce universe-scoped unique product names
                var isUniqueName = await DevProductHandler.IsDeveloperProductNameUniqueAsync(
                    connectionString,
                    universeId,
                    name,
                    isUpdate ? (long?)existingProductId : null);

                if (!isUniqueName)
                {
                    TempData["DeveloperProductError"] = "A developer product with this name already exists in this universe";
                    return Json(new { 
                        success = true, 
                    });
                }

                if (isUpdate)
                {
                    productId = existingProductId;
                    var updatedInUniverse = await DevProductHandler.UpdateDeveloperProductInUniverseAsync(
                        connectionString, 
                        universeId, 
                        productId, 
                        name, 
                        description ?? "", 
                        priceInRobux, 
                        priceInTix, 
                        imageAssetId);

                    if (!updatedInUniverse)
                    {
                        return Json(new { success = false, message = "Failed to update developer product in universe" });
                    }

                    try
                    {
                        await DevProductHandler.UpdateDeveloperProductInDatabaseAsync(
                            connectionString, 
                            productId, 
                            name, 
                            description ?? "", 
                            priceInRobux, 
                            priceInTix, 
                            imageAssetId);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Warning: Failed to update database record: {ex.Message}");
                        Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    }

                    TempData["DeveloperProductUpdated"] = true;
                    TempData["DeveloperProductId"] = productId.ToString();

                    return Json(new { 
                        success = true, 
                        message = $"Product {productId} successfully updated",
                        productId = productId,
                        universeId = universeId
                    });
                }
                else
                {
                    productId = await GamesRepository.GenerateUniverseDeveloperProductIdAsync(connectionString);
                }

                // Create developer product object for universe storage
                var developerProduct = new
                {
                    developerProductId = productId,
                    universeId = universeId,
                    name = name,
                    description = description ?? "",
                    priceInRobux = priceInRobux,
                    priceInTix = priceInTix,
                    imageAssetId = imageAssetId,
                    creatorUserId = currentUserId,
                    createdAt = DateTime.UtcNow,
                    placeId = id // Keep track of which place created it, but store in universe
                };


                // Add developer product to universe
                var developerProductJson = JsonSerializer.SerializeToElement(developerProduct);
                var addedToUniverse = await GamesRepository.AddDeveloperProductToUniverseAsync(
                    connectionString, 
                    universeId, 
                    developerProductJson);

                if (!addedToUniverse)
                {
                    return Json(new { success = false, message = "Failed to add developer product to universe" });
                }

                // Also create a database record for the developer product (for potential future use)
                try
                {
                    await DevProductHandler.CreateDeveloperProduct(
                        connectionString, 
                        universeId, 
                        name, 
                        description ?? "", 
                        priceInRobux, 
                        priceInTix, 
                        imageAssetId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to create database record: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }

                // Set success message in TempData
                TempData["DeveloperProductCreated"] = true;
                TempData["DeveloperProductId"] = productId.ToString();

                return Json(new { 
                    success = true, 
                    message = "Developer product created successfully",
                    productId = productId,
                    universeId = universeId
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while creating the developer product. Please try again." });
            }
        }


    }
}
