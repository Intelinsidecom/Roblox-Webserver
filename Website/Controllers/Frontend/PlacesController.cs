using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Data;
using Assets;
using System.Security.Claims;
using Thumbnails;
using Npgsql;

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

                // Get place asset using Assets assembly
                var connectionString = _configuration.GetConnectionString("Default");
                var placeAsset = await _assetRepository.GetAssetByIdAsync(connectionString, id);
                
                if (placeAsset == null)
                {
                    return Redirect("/404");
                }

                // Check if user owns this place
                if (placeAsset.OwnerUserId != currentUserId)
                {
                    return Redirect("/404"); // User doesn't own this place
                }

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
                                 $"/asset-thumbnail/image?assetId={id}&width=512&height=512&format=Png";
                ViewBag.hasGeneratedIcon = placeAsset.GeneratedIcon;
                
                // Add the place asset thumbnail URL for comparison
                ViewBag.thumbnailUrl = placeAsset.ThumbnailUrl;

                // Return the update view
                return View("~/Views/Pages/places/{id}/update.cshtml");
            }
            catch (Exception ex)
            {
                // Log error and return 404 for simplicity
                Console.WriteLine($"Error loading place {id}: {ex.Message}");
                return Redirect("/404");
            }
        }

        /// <summary>
        /// POST /places/icons/add-generated-image - Handle generated icon selection for a place
        /// </summary>
        [HttpPost("places/icons/add-generated-image")]
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
                            Console.WriteLine($"Background thumbnail generation failed: {ex.Message}");
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
                    
                    Console.WriteLine($"Returning icon URL: {iconUrl}");

                    return Json(new { 
                        success = true, 
                        message = "Generated icon set successfully",
                        iconUrl = iconUrl,
                        iconHash = updatedAsset?.PlaceGeneratedIconHash ?? ""
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in thumbnail generation: {ex.Message}");
                    return Json(new { 
                        success = false, 
                        message = $"Failed to generate icon: {ex.Message}. Please try again." 
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in add-generated-image: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while setting the icon. Please try again." });
            }
        }


        /// <summary>
        /// POST /places/icons/add-icon - Handle custom icon upload for a place
        /// </summary>
        [HttpPost("places/icons/add-icon")]
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
                Console.WriteLine($"Error uploading icon: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while uploading the icon. Please try again." });
            }
        }


        /// <summary>
        /// POST /places/icons/remove-icon - Handle icon removal for a place
        /// </summary>
        [HttpPost("places/icons/remove-icon")]
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
                Console.WriteLine($"Error removing icon: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while removing the icon. Please try again." });
            }
        }


        /// <summary>
        /// GET /places/{id}/thumbnail - Get current thumbnail URL for a place
        /// </summary>
        [HttpGet("places/{id}/thumbnail")]
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
                Console.WriteLine($"Error getting place thumbnail {id}: {ex.Message}");
                return StatusCode(500, new { error = "An error occurred while retrieving thumbnail information" });
            }
        }

        /// <summary>
        /// POST /places/doconfigure2 - Handle place configuration form submission
        /// </summary>
        [HttpPost("places/doconfigure2")]
        public async Task<IActionResult> DoConfigure2()
        {
            long Id = 0;
            string Name = "", Description = "", Genre = "All";
            string iconType = "", iconChanged = "false";
            IFormFile iconImageFile = null;
            
            try
            {
                // Get current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "User not authenticated" });
                    }
                    return Redirect("/login");
                }

                // Read form data directly to bypass model binding issues
                if (!long.TryParse(Request.Form["Id"].FirstOrDefault(), out Id))
                {
                    return Json(new { success = false, message = "Invalid asset ID" });
                }
                Name = Request.Form["Name"].FirstOrDefault() ?? "";
                Description = Request.Form["Description"].FirstOrDefault() ?? "";
                Genre = Request.Form["Genre"].FirstOrDefault() ?? "All";
                iconType = Request.Form["IconType"].FirstOrDefault() ?? "";
                iconChanged = Request.Form["iconChanged"].FirstOrDefault() ?? "false";
                iconImageFile = Request.Form.Files["iconImageFile"];

                // Get place asset to verify ownership
                var connectionString = _configuration.GetConnectionString("Default");
                var placeAsset = await _assetRepository.GetAssetByIdAsync(connectionString, Id);
                
                if (placeAsset == null)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Place not found" });
                    }
                    return Redirect("/404");
                }

                // Check if user owns this place
                if (placeAsset.OwnerUserId != currentUserId)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Access denied" });
                    }
                    return Redirect("/404"); // User doesn't own this place
                }

                // Validate input
                if (string.IsNullOrWhiteSpace(Name))
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Name cannot be empty" });
                    }
                    
                    // Return error or validation message for non-AJAX requests
                    ViewBag.gameid = Id;
                    ViewBag.gamename = placeAsset.Name ?? "";
                    ViewBag.gamedesc = Description ?? "";
                    ViewBag.gamegenre = AssetGenreNames.GetGenreLabel(placeAsset.Genre); // Use actual genre from database
                    ViewBag.xcsrftoken = "";
                    ViewBag.ErrorMessage = "Name cannot be empty";
                    return View("~/Views/Pages/places/{id}/update.cshtml");
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
                                        await PlaceThumbnail.GeneratePlaceThumbnailAsync(_thumbnailService, connectionString, Id, existingAsset.ContentHash, baseUrlForTask);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Background thumbnail generation failed: {ex.Message}");
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

                // Return JSON response for AJAX requests
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Place updated successfully" });
                }

                // Redirect back to the update page with success for non-AJAX requests
                return Redirect($"/places/{Id}/update");
            }
            catch (Exception ex)
            {
                // Log error and return to form
                Console.WriteLine($"Error updating place {Id}: {ex.Message}");
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "An error occurred while saving. Please try again." });
                }
                
                ViewBag.gameid = Id;
                ViewBag.gamename = Name ?? "";
                ViewBag.gamedesc = Description ?? "";
                ViewBag.gamegenre = AssetGenreNames.GetGenreLabel(AssetGenreNames.GetGenreIdFromString(Genre)); // Use selected genre
                ViewBag.xcsrftoken = "";
                ViewBag.ErrorMessage = "An error occurred while saving. Please try again.";
                return View("~/Views/Pages/places/{id}/update.cshtml");
            }
        }
    }
}
