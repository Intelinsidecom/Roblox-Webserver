using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Data;
using Assets;
using System.Security.Claims;
using Thumbnails;
using Npgsql;
using Common;
using System.Linq;

namespace RobloxWebserver.Controllers
{
    /// <summary>
    /// Controller for place (per-place) management endpoints.
    ///
    /// Implemented responsibilities:
    /// - Configure Start Place: GET endpoint similar to Roblox's /places/{id}/update
    ///   that allows editing name, description, and basic settings for a place inside a universe.
    ///
    /// Planned responsibilities (to be implemented later):
    /// - List Places in a Universe: endpoint used by place selector modal
    ///   (/universes/get-places-by-context today is stubbed by static HTML). The list of
    ///   place ids comes from the universes.place_ids array.
    /// - Add New Place to Universe: create an additional place asset and append its id to
    ///   the universes.place_ids array for that universe.
    /// - Toggle place public/private and shutdown servers knobs that the gear menu exposes.
    /// </summary>
    [ApiController]
    [Authorize]
    public sealed class PlacesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly AssetMetadataRepository _assetRepository;
        private readonly IThumbnailService _thumbnailService;

        public PlacesController(AppDbContext context, IConfiguration configuration, AssetMetadataRepository assetRepository, IThumbnailService thumbnailService)
        {
            _context = context;
            _configuration = configuration;
            _assetRepository = assetRepository;
            _thumbnailService = thumbnailService;
        }

        /// <summary>
        /// </summary>
        [HttpGet("places/{id}/update")]
        public async Task<IActionResult> UpdatePlace(long id)
        {
            try
            {
                // Get current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Redirect("/login");
                }

                // Validate place ownership and type using Thumbnails assembly helper
                var connectionString = _configuration.GetConnectionString("Default");
                var isValidPlace = await PlaceValidationHelper.ValidatePlaceOwnershipAsync(id, currentUserId, connectionString, _assetRepository);
                if (!isValidPlace)
                {
                    return Redirect("/404");
                }

                // Get place asset using Assets assembly (now validated)
                var placeAsset = await _assetRepository.GetAssetByIdAsync(connectionString, id);

                // Populate ViewBag with place data
                ViewBag.gameid = placeAsset.AssetId;
                ViewBag.gamename = placeAsset.Name ?? "";
                ViewBag.gamedesc = placeAsset.Description ?? "";
                ViewBag.gamegenre = AssetGenreNames.GetGenreLabel(placeAsset.Genre); // Use actual genre from database
                ViewBag.xcsrftoken = ""; // Will be populated by anti-forgery system

                // Check if place has custom icon and get icon URLs
                var hasCustomIcon = await _thumbnailService.HasCustomIconAsync(id, connectionString);
                
                
                // Only check for custom icon URL if the flag is false (to catch edge cases)
                if (!hasCustomIcon && !string.IsNullOrWhiteSpace(placeAsset.PlaceCustomIconUrl))
                {
                    hasCustomIcon = true;
                }
                // Double-check: if custom icon URL is null/empty, ensure hasCustomIcon is false
                else if (hasCustomIcon && string.IsNullOrWhiteSpace(placeAsset.PlaceCustomIconUrl))
                {
                    hasCustomIcon = false;
                }
                
                ViewBag.hasCustomIcon = hasCustomIcon;
                
                // Check if custom icon URL matches thumbnail URL (for delete button visibility)
                bool customIconIsThumbnail = !string.IsNullOrWhiteSpace(placeAsset.PlaceCustomIconUrl) && 
                                          placeAsset.PlaceCustomIconUrl == placeAsset.ThumbnailUrl;
                ViewBag.customIconIsThumbnail = customIconIsThumbnail;
                
                // Get icon URLs for display
                // Priority: custom icon (if still set) > generated icon > thumbnail URL > default
                ViewBag.iconUrl = placeAsset.PlaceCustomIconUrl ?? 
                                 placeAsset.PlaceGeneratedIconUrl ?? 
                                 placeAsset.ThumbnailUrl ?? 
                                 $"/game-icons/image?assetId={id}&width=512&height=512&format=Png";
                ViewBag.hasGeneratedIcon = placeAsset.GeneratedIcon;
                
                // Add place asset thumbnail URL for comparison
                ViewBag.thumbnailUrl = placeAsset.ThumbnailUrl;

                // Add auto-generated thumbnail information
                ViewBag.hasAutoGeneratedThumbnail = placeAsset.PlaceAutoGeneratedThumbnail;
                ViewBag.placeGeneratedThumbnailUrl = placeAsset.PlaceGeneratedThumbnailUrl;

                // Add custom and video thumbnail flags for radio button selection
                ViewBag.hasCustomThumbnail = placeAsset.PlaceCustomThumbnail;
                ViewBag.hasVideoThumbnail = placeAsset.PlaceVideoThumbnail;

                // Load actual thumbnail data from database
                var thumbnailData = await PlaceThumbnail.GetPlaceThumbnailsAsync(connectionString, id);
                
                ViewBag.placeThumbnails = thumbnailData;
                ViewBag.thumbnailCount = thumbnailData.Count;

                // Return the update view
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

                // Get place asset to verify ownership
                var connectionString = _configuration.GetConnectionString("Default");
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

                // Generate auto-generated thumbnail (only 1280x720, skip if already exists)
                var baseUrl = _configuration["Thumbnails:ThumbnailUrl"] ?? $"{Request.Scheme}://{Request.Host}";
                
                try
                {
                    // First check if auto-generated thumbnail already exists
                    var existingThumbnails = await PlaceThumbnail.GetPlaceThumbnailsAsync(connectionString, placeId);
                    var existingAutoGeneratedThumbnail = existingThumbnails.FirstOrDefault(t => 
                        t.GetType().GetProperty("type")?.GetValue(t)?.ToString() == "image" && 
                        t.GetType().GetProperty("altText")?.GetValue(t)?.ToString() == "Auto-generated Thumbnail");
                    
                    string? thumbnailUrl = null;
                    
                    if (existingAutoGeneratedThumbnail != null)
                    {
                        // Thumbnail already exists, just return its URL
                        var urlProperty = existingAutoGeneratedThumbnail.GetType().GetProperty("url");
                        thumbnailUrl = urlProperty?.GetValue(existingAutoGeneratedThumbnail)?.ToString();
                    }
                    else
                    {
                        var renderedResult1280x720 = await _thumbnailService.RenderPlaceAsync(
                            placeId, 
                            x: 1280, 
                            y: 720, 
                            connectionString: connectionString, 
                            placeAssetHash: placeAsset.ContentHash);
                        
                        var cdnPlaceThumbnailsPath = CDNUtilities.GetCDNAssetsPath("place-thumbnails");
                        var sourcePath1280x720 = Path.Combine(renderedResult1280x720.FullPath);
                        var cdnThumbnailPath1280x720 = Path.Combine(cdnPlaceThumbnailsPath, renderedResult1280x720.FileName);
                        bool thumbnail1280x720Copied = CDNUtilities.SafeFileCopy(sourcePath1280x720, cdnThumbnailPath1280x720);
                        
                        
                        if (thumbnail1280x720Copied)
                        {
                            // Generate CDN URL for 1280x720 thumbnail
                            thumbnailUrl = CDNUtilities.GeneratePlaceThumbnailUrl(baseUrl, renderedResult1280x720.FileName);
                            
                            // Update auto-generated thumbnail URL field and clear other flags
                            await ThumbnailQueries.ClearPlaceCustomThumbnailAsync(connectionString, placeId);
                            await ThumbnailQueries.ClearPlaceVideoThumbnailAsync(connectionString, placeId);
                            
                            // Generate auto-generated 720p thumbnail for place (1280x720)
                            await PlaceThumbnail.GenerateAutoGeneratedThumbnailAsync(
                                _thumbnailService,
                                connectionString,
                                placeId,
                                placeAsset.ContentHash ?? string.Empty,
                                baseUrl);
                        }
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

                // Get place asset to verify ownership
                var connectionString = _configuration.GetConnectionString("Default");
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
                var connectionString = _configuration.GetConnectionString("Default");
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
                var connectionString = _configuration.GetConnectionString("Default");
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
                var connectionString = _configuration.GetConnectionString("Default");
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
                Description = Request.Form["Description"].FirstOrDefault() ?? "";
                Genre = Request.Form["Genre"].FirstOrDefault() ?? "All";
                iconType = Request.Form["IconType"].FirstOrDefault() ?? "";
                iconChanged = Request.Form["iconChanged"].FirstOrDefault() ?? "false";
                thumbnailType = Request.Form["ThumbnailType"].FirstOrDefault() ?? "";
                iconImageFile = Request.Form.Files["iconImageFile"];


                // Get place asset to verify ownership
                var connectionString = _configuration.GetConnectionString("Default");
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
                        hasValidVideoThumbnail = IsValidYouTubeURL(placeAsset.PlaceThumbnailVideo);
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
                            hasValidVideoThumbnail = !string.IsNullOrWhiteSpace(videoUrl) && IsValidYouTubeURL(videoUrl);
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

                // Return JSON response for AJAX requests
                if (isAjax)
                {
                    return Json(new { success = true, message = "Place updated successfully" });
                }

                return Redirect($"/places/{Id}/update");
            }
            catch (Exception ex)
            {
                if (isAjax)
                {
                    return Json(new { success = false, message = "An error occurred while saving. Please try again." });
                }
                
                ViewBag.gameid = Id;
                ViewBag.ErrorMessage = "An error occurred while saving. Please try again.";
                return View("~/Views/Pages/places/{id}/update.cshtml");
            }
            
            if (isAjax)
            {
                return Json(new { success = false, message = "Unexpected error occurred" });
            }
            return Redirect("/404");
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
                var connectionString = _configuration.GetConnectionString("Default");
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
                Console.WriteLine($"[AddThumbnailVideo] Video URL received: '{videoUrl}'");
                
                if (string.IsNullOrWhiteSpace(videoUrl))
                {
                    Console.WriteLine($"[AddThumbnailVideo] Video URL is empty or null");
                    return Json(new { success = false, message = "Video URL is required" });
                }

                // Validate YouTube URL format
                if (!IsValidYouTubeURL(videoUrl))
                {
                    Console.WriteLine($"[AddThumbnailVideo] Invalid YouTube URL format: '{videoUrl}'");
                    return Json(new { success = false, message = "Invalid YouTube URL." });
                }

                // Get place ID from form
                var placeIdStr = Request.Form["id"].FirstOrDefault();
                Console.WriteLine($"[AddThumbnailVideo] Place ID from form: '{placeIdStr}'");
                
                if (string.IsNullOrWhiteSpace(placeIdStr) || !long.TryParse(placeIdStr, out var placeId))
                {
                    Console.WriteLine($"[AddThumbnailVideo] Invalid place ID: '{placeIdStr}'");
                    return Json(new { success = false, message = "Invalid place ID" });
                }

                // Get place asset to verify ownership
                var connectionString = _configuration.GetConnectionString("Default");
                var placeAsset = await _assetRepository.GetAssetByIdAsync(connectionString, placeId);
                
                if (placeAsset == null)
                {
                    Console.WriteLine($"[AddThumbnailVideo] Place not found for ID: {placeId}");
                    return Json(new { success = false, message = "Place not found" });
                }

                // Check if user owns this place
                if (placeAsset.OwnerUserId != currentUserId)
                {
                    Console.WriteLine($"[AddThumbnailVideo] Access denied - User {currentUserId} does not own place {placeId}");
                    return Json(new { success = false, message = "Access denied" });
                }

                Console.WriteLine($"[AddThumbnailVideo] All validations passed, proceeding to save video URL for place {placeId}");

                // Update only the place_thumbnail_video field in assets table (temporary storage)
                await PlaceThumbnail.SetPlaceVideoUrlTempAsync(connectionString, placeId, videoUrl);

                Console.WriteLine($"[AddThumbnailVideo] Video URL saved successfully to place_thumbnail_video field");

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
                Console.WriteLine($"[AddThumbnailVideo] Exception occurred: {ex.GetType().Name}: {ex.Message}");
                Console.WriteLine($"[AddThumbnailVideo] Stack trace: {ex.StackTrace}");
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
                var connectionString = _configuration.GetConnectionString("Default");
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
/// Validates if the provided URL is a valid YouTube URL
/// </summary>
/// <param name="url">The URL to validate</param>
/// <returns>True if valid YouTube URL, false otherwise</returns>
private static bool IsValidYouTubeURL(string url)
{
    if (string.IsNullOrWhiteSpace(url))
        return false;

    var trimmedUrl = url.Trim();
            
    // YouTube URL patterns
    var youtubePatterns = new[]
    {
        @"^https?:\/\/(www\.)?youtube\.com\/watch\?v=[a-zA-Z0-9_-]+$",
        @"^https?:\/\/(www\.)?youtube\.com\/embed\/[a-zA-Z0-9_-]+$",
        @"^https?:\/\/youtu\.be\/[a-zA-Z0-9_-]+$",
        @"^https?:\/\/(www\.)?youtube\.com\/v\/[a-zA-Z0-9_-]+$"
    };

    return youtubePatterns.Any(pattern => System.Text.RegularExpressions.Regex.IsMatch(trimmedUrl, pattern));
}
    }
}
