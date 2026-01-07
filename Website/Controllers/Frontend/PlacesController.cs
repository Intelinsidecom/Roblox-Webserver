using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Data;
using Assets;
using System.Security.Claims;

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

        public PlacesController(AppDbContext context, IConfiguration configuration, AssetMetadataRepository assetRepository)
        {
            _context = context;
            _configuration = configuration;
            _assetRepository = assetRepository;
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
        /// POST /places/doconfigure2 - Handle place configuration form submission
        /// </summary>
        [HttpPost("places/doconfigure2")]
        public async Task<IActionResult> DoConfigure2()
        {
            long Id = 0;
            string Name = "", Description = "", Genre = "All";
            
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
                Console.WriteLine($"[DEBUG] Saving genre: '{Genre}' -> ID: {genreId}");
                await assetsRepo.UpdateAssetGenreAsync(connectionString, Id, genreId);
                Console.WriteLine($"[DEBUG] Genre saved to database successfully");

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
