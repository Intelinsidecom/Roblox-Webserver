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
using System.Text;

namespace RobloxWebserver.Controllers
{


    /// <summary>
    /// Controller for universe (per-universe) management endpoints.
    /// </summary>
    [ApiController]
    [Authorize]
    public sealed class UniverseController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly AssetMetadataRepository _assetRepository;
        private readonly IThumbnailService _thumbnailService;

        public UniverseController(AppDbContext context, IConfiguration configuration, AssetMetadataRepository assetRepository, IThumbnailService thumbnailService)
        {
            _context = context;
            _configuration = configuration;
            _assetRepository = assetRepository;
            _thumbnailService = thumbnailService;
        }

        /// <summary>
        /// Helper function to update universe name
        /// </summary>
        private async Task UpdateUniverseNameAsync(long universeId, string name, string connectionString, CancellationToken cancellationToken)
        {
            await UniverseHandler.UpdateUniverseNameAsync(universeId, connectionString, name, cancellationToken);
        }

        /// <summary>
        /// Helper function to update in_universe field for a place
        /// </summary>
        private async Task UpdatePlaceInUniverseFieldAsync(long placeId, bool inUniverse, string connectionString, CancellationToken cancellationToken = default)
        {
            await UniverseHandler.UpdatePlaceInUniverseFieldAsync(placeId, inUniverse, connectionString, cancellationToken);
        }

        /// <summary>
        /// Helper function to sync in_universe field for all places in a universe
        /// </summary>
        private async Task SyncUniversePlacesInUniverseFieldAsync(long universeId, string connectionString, CancellationToken cancellationToken = default)
        {
            await UniverseHandler.SyncUniversePlacesInUniverseFieldAsync(universeId, connectionString, cancellationToken);
        }

        /// <summary>
        /// GET /universes/{universeId}/developer-products - Get developer products for a universe
        /// </summary>
        [HttpGet("universes/{universeId}/developer-products")]
        [Authorize]
       public async Task<IActionResult> GetUniverseDeveloperProducts(long universeId, int page = 1)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Content("<div class=\"error\">User not authenticated</div>");
                }

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var universeOwnerId = await GamesRepository.GetUniverseOwnerAsync(connectionString, universeId);
                
                if (!universeOwnerId.HasValue || universeOwnerId.Value != currentUserId)
                {
                    return Content("<div class=\"error\">Access denied</div>");
                }

                const int pageSize = 10;
                var (developerProducts, totalCount) = await DevProductHandler.GetUniverseDeveloperProductsPaginatedAsync(connectionString, universeId, page, pageSize);
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
        <a class=""btn-small btn-neutral developer-product-button"" id=""createNewButton"" data-form-post-url=""/universes/{universeId}/developer-products/create"" data-url=""/universes/{universeId}/developer-products/create"">Create new</a>
    </div>
    <div style=""clear: both;""></div>
</div>
{errorMessageHtml}{successMessageHtml}
<p style=""margin: 0;"">You do not have any developer products. Click <a href=""https://create.roblox.com/docs/production/monetization/developer-products"" target=""_blank"">here</a> for more information on<br>developer products.</p>";
                    
                    return Content(emptyHtml, "text/html; charset=utf-8");
                }

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
        <a class=""btn-small btn-neutral edit"" style=""text-align: left;""data-form-post-url=""/universes/{universeId}/developer-products/{productId}/configure"" data-url=""/universes/{universeId}/developer-products/{productId}/configure"">Edit</a>
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
        <a class=""btn-small btn-neutral developer-product-button"" id=""createNewButton"" data-form-post-url=""/universes/{universeId}/developer-products/create"" data-url=""/universes/{universeId}/developer-products/create"">Create new</a>
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
                Console.WriteLine($"ERROR GetUniverseDeveloperProducts: {ex.Message}");
                Console.WriteLine($"STACK TRACE: {ex.StackTrace}");
                return Content("<div class=\"error\">An error occurred while loading developer products. Please try again.</div>", "text/html");
            }
        }


        /// <summary>
        /// POST /universes/{universeId}/developer-products/upload-image - Handle developer product image upload
        /// </summary>
        [HttpPost("universes/{universeId}/developer-products/upload-image")]
        [Authorize]
        public async Task<IActionResult> UploadUniverseDeveloperProductImage(long universeId, IFormFile? image)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

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
                    Console.WriteLine($"ERROR UploadUniverseDeveloperProductImage (image processing): {ex.Message}");
                    Console.WriteLine($"STACK TRACE: {ex.StackTrace}");
                    return Json(new { 
                        success = false, 
                        message = "Failed to process image, try again later."
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
        <img src=""{imageUrl}"" alt=""Preview"" />
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
        /// GET /universes/{universeId}/developer-products/image/{assetId} - Get developer product image
        /// </summary>
        [HttpGet("universes/{universeId}/developer-products/image/{assetId}")]
        public async Task<IActionResult> GetUniverseDeveloperProductImage(long universeId, long assetId, int width = 150, int height = 150)
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
        /// GET /universes/{universeId}/developer-products/validate-name - Validate developer product name
        /// </summary>
        [HttpGet("universes/{universeId}/developer-products/validate-name")]
        [Authorize]
        public async Task<IActionResult> ValidateUniverseDeveloperProductName(long universeId, string developerProductName, long? developerProductId = null)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Json(new { Success = false, Message = "User not authenticated" });
                }

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var universeOwner = await GamesRepository.GetUniverseOwnerAsync(connectionString, universeId);
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
                    universeId,
                    developerProductName,
                    developerProductId);

                if (!isUnique)
                {
                    return Json(new { Success = false, Message = "A developer product with this name already exists in this universe" });
                }

                return Json(new { Success = true, Message = "" });
            }
            catch (Exception)
            {
                return Json(new { Success = false, Message = "An error occurred while validating the name" });
            }
        }

        /// <summary>
        /// GET /universes/{universeId}/developer-products/create - Show developer product creation page for universe
        /// </summary>
        [HttpGet("universes/{universeId}/developer-products/create")]
        [Authorize]
        public async Task<IActionResult> CreateUniverseDeveloperProductPage(long universeId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Redirect("/login");
                }

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var universeOwner = await GamesRepository.GetUniverseOwnerAsync(connectionString, universeId);
                if (universeOwner == null || universeOwner != currentUserId)
                {
                    return Redirect("/404");
                }

                ViewBag.gameid = universeId;
                ViewBag.PlaceUniverseId = universeId;
                ViewBag.gamename = $"Universe {universeId}";
                ViewBag.gamedesc = "";
                ViewBag.productCreated = TempData["DeveloperProductCreated"] as bool? ?? false;
                ViewBag.productId = TempData["DeveloperProductId"] as string;
                
                return View("~/Views/Pages/universes/{id}/developer-products/create.cshtml");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR CreateUniverseDeveloperProductPage: {ex.Message}");
                Console.WriteLine($"STACK TRACE: {ex.StackTrace}");
                return Redirect("/404");
            }
        }

        /// <summary>
        /// POST /universes/{universeId}/developer-products/create - Create or update a developer product
        /// </summary>
        [HttpPost("universes/{universeId}/developer-products/create")]
        [Authorize]
        public async Task<IActionResult> CreateUniverseDeveloperProduct(long universeId)
        {
            try
            {
                var (isAuthenticated, currentUserId) = AuthenticationHelper.GetCurrentUserId(User);
                if (!isAuthenticated)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var universeOwner = await GamesRepository.GetUniverseOwnerAsync(connectionString, universeId);
                if (universeOwner == null || universeOwner != currentUserId)
                {
                    return Json(new { success = false, message = "Access denied - you do not own this universe" });
                }

                var name = Request.Form["name"].FirstOrDefault();
                var description = Request.Form["description"].FirstOrDefault();
                var priceInRobuxStr = Request.Form["priceInRobux"].FirstOrDefault();
                var priceInTixStr = Request.Form["priceInTix"].FirstOrDefault();
                var imageAssetIdStr = Request.Form["imageAssetId"].FirstOrDefault();
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

                long? imageAssetId = null;
                if (!string.IsNullOrWhiteSpace(imageAssetIdStr) && long.TryParse(imageAssetIdStr, out var parsedImageAssetId))
                {
                    imageAssetId = parsedImageAssetId;
                }

                long existingProductId = 0;
                bool isUpdate = !string.IsNullOrWhiteSpace(productIdStr) && long.TryParse(productIdStr, out existingProductId);
                long productId;
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
                    createdAt = DateTime.UtcNow
                };

                var developerProductJson = JsonSerializer.SerializeToElement(developerProduct);
                var addedToUniverse = await GamesRepository.AddDeveloperProductToUniverseAsync(
                    connectionString, 
                    universeId, 
                    developerProductJson);

                if (!addedToUniverse)
                {
                    return Json(new { success = false, message = "Failed to add developer product to universe" });
                }

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

                TempData["DeveloperProductCreated"] = true;
                TempData["DeveloperProductId"] = productId.ToString();

                return Json(new { 
                    success = true, 
                    message = $"Product {productId} successfully created",
                    productId = productId,
                    universeId = universeId
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = "An error occurred while saving the developer product."
                });
            }
        }

        /// <summary>
        /// GET /universes/{universeId}/configure - Configure universe page
        /// </summary>
        [HttpGet("universes/{universeId}/configure")]
        [Authorize]
        public async Task<IActionResult> ConfigureUniversePage(long universeId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Redirect("/login");
                }

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var universeOwner = await GamesRepository.GetUniverseOwnerAsync(connectionString, universeId);
                if (universeOwner == null || universeOwner != currentUserId)
                {
                    return Redirect("/404");
                }

                var universeInfo = await GamesRepository.GetUniverseAsync(connectionString, universeId);
                if (universeInfo == null)
                {
                    return Redirect("/404");
                }

                ViewBag.gameid = universeId;
                ViewBag.gamename = universeInfo.Name ?? $"Universe {universeId}";
                ViewBag.gamedesc = universeInfo.Description ?? "";
                ViewBag.PlaceUniverseId = universeId;
                ViewBag.xcsrftoken = "";
                ViewBag.theme = "";
                ViewBag.privacyLevel = universeInfo.PrivacyLevel;
                ViewBag.studioAccessToApis = universeInfo.Studio_Access_To_APIs;
                
                return View("~/Views/Pages/universes/{id}/configure.cshtml");
            }
            catch (Exception ex)
            {
                return Redirect("/404");
            }
        }

        /// <summary>
        /// POST /universes/doconfigure - Handle universe configuration form submission
        /// </summary>
        [HttpPost("universes/doconfigure")]
        [Authorize]
        public async Task<IActionResult> DoConfigure()
        {
            try
            {
                var (isAuthenticated, currentUserId) = AuthenticationHelper.GetCurrentUserId(User);
                if (!isAuthenticated)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var idStr = Request.Form["Id"].FirstOrDefault();
                if (string.IsNullOrWhiteSpace(idStr) || !long.TryParse(idStr, out var universeId))
                {
                    return Json(new { success = false, message = "Invalid universe ID" });
                }

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var universeOwner = await GamesRepository.GetUniverseOwnerAsync(connectionString, universeId);
                if (universeOwner == null || universeOwner != currentUserId)
                {
                    return Json(new { success = false, message = "Access denied - you do not own this universe" });
                }

                var name = Request.Form["Name"].FirstOrDefault()?.Trim() ?? "";
                var publicLevelStr = Request.Form["PublicLevel"].FirstOrDefault();
                var allowStudioAccessToApisStr = Request.Form["AllowStudioAccessToApis"].FirstOrDefault();

                if (string.IsNullOrWhiteSpace(name))
                {
                    return Json(new { success = false, message = "Universe name is required" });
                }

                if (name.Length > 50)
                {
                    return Json(new { success = false, message = "Universe name is too long (max 50 characters)" });
                }

                if (CharacterValidationUtility.ContainsDangerousContent(name))
                {
                    return Json(new { success = false, message = "Universe name contains invalid characters" });
                }

                int privacyLevel = 1; // Default to Public
                if (!string.IsNullOrWhiteSpace(publicLevelStr))
                {
                    if (publicLevelStr == "0") privacyLevel = 1; // Public
                    else if (publicLevelStr == "2") privacyLevel = 2; // Friends
                    else if (publicLevelStr == "1") privacyLevel = 3; // Private
                }

                bool studioAccessToApis = false;
                if (!string.IsNullOrWhiteSpace(allowStudioAccessToApisStr))
                {
                    bool.TryParse(allowStudioAccessToApisStr, out studioAccessToApis);
                }

                var success = await UniverseHandler.UpdateUniverseConfigurationAsync(universeId, currentUserId, name, privacyLevel, studioAccessToApis, connectionString);

                if (!success)
                {
                    return Json(new { success = false, message = "Failed to update universe - you may not have permission" });
                }

                return Json(new { success = true, message = "Universe settings updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = $"An error occurred while updating universe settings: {ex.Message}" 
                });
            }
        }

        /// <summary>
        /// GET /universes/{universeId}/developer-products/{productId}/configure - Configure developer product page
        /// </summary>
        [HttpGet("universes/{universeId}/developer-products/{productId}/configure")]
        [Authorize]
        public async Task<IActionResult> ConfigureUniverseDeveloperProductPage(long universeId, long productId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Redirect("/login");
                }

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var universeOwner = await GamesRepository.GetUniverseOwnerAsync(connectionString, universeId);
                if (universeOwner == null || universeOwner != currentUserId)
                {
                    return Redirect("/404");
                }

                var (products, _) = await DevProductHandler.GetUniverseDeveloperProductsPaginatedAsync(connectionString, universeId, 1, 1000);
                
                if (products == null)
                {
                    return Redirect("/404");
                }

                var product = products.FirstOrDefault(p => 
                {
                    if (p.TryGetProperty("developerProductId", out var idElement))
                    {
                        return idElement.ValueKind != JsonValueKind.Null && idElement.GetInt64() == productId;
                    }
                    return false;
                });

                if (product.ValueKind == JsonValueKind.Undefined)
                {
                    return Redirect("/404");
                }

                var productData = new
                {
                    developerProductId = productId,
                    name = product.TryGetProperty("name", out var nameElement) && nameElement.ValueKind != JsonValueKind.Null ? nameElement.GetString() : "",
                    description = product.TryGetProperty("description", out var descElement) && descElement.ValueKind != JsonValueKind.Null ? descElement.GetString() : "",
                    priceInRobux = product.TryGetProperty("priceInRobux", out var robuxElement) && robuxElement.ValueKind != JsonValueKind.Null ? robuxElement.GetInt32() : 0,
                    priceInTix = DevProductHandler.GetTicketPriceFromJson(product),
                    imageAssetId = product.TryGetProperty("imageAssetId", out var imgElement) && imgElement.ValueKind != JsonValueKind.Null ? (long?)imgElement.GetInt64() : null
                };

                ViewBag.gameid = universeId;
                ViewBag.PlaceUniverseId = universeId;
                ViewBag.gamename = $"Universe {universeId}";
                ViewBag.gamedesc = "";
                ViewBag.DeveloperProductId = productId;
                ViewBag.ProductName = productData.name;
                ViewBag.ProductDescription = productData.description;
                ViewBag.PriceInRobux = productData.priceInRobux;
                ViewBag.PriceInTix = productData.priceInTix;
                ViewBag.ImageAssetId = productData.imageAssetId;
                
                return View("~/Views/Pages/universes/{id}/developer-products/{productId}/configure.cshtml");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR ConfigureUniverseDeveloperProductPage: {ex.Message}");
                Console.WriteLine($"STACK TRACE: {ex.StackTrace}");
                return Redirect("/404");
            }
        }

        /// <summary>
        /// GET /universes/{universeId}/developer-products/{productId}/data - Get developer product data
        /// </summary>
        [HttpGet("universes/{universeId}/developer-products/{productId}/data")]
        [Authorize]
        public async Task<IActionResult> GetDeveloperProductData(long universeId, long productId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var universeOwner = await GamesRepository.GetUniverseOwnerAsync(connectionString, universeId);
                if (universeOwner == null || universeOwner != currentUserId)
                {
                    return Json(new { success = false, message = "Access denied" });
                }

                var (products, _) = await DevProductHandler.GetUniverseDeveloperProductsPaginatedAsync(connectionString, universeId, 1, 1000);
                
                if (products == null)
                {
                    return Json(new { success = false, message = "No products found" });
                }

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
                Console.WriteLine($"ERROR GetDeveloperProductData: {ex.Message}");
                Console.WriteLine($"STACK TRACE: {ex.StackTrace}");
                return Json(new { success = false, message = "An error occurred while loading product data" });
            }
        }

        /// <summary>
        /// GET /universes/get-first-place-id - Get the first place ID for a universe (start place)
        /// </summary>
        [HttpGet("universes/get-first-place-id")]
        [Authorize]
        public async Task<IActionResult> GetFirstPlaceId(long universeId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var universeOwner = await GamesRepository.GetUniverseOwnerAsync(connectionString, universeId);
                if (universeOwner == null || universeOwner != currentUserId)
                {
                    return Json(new { success = false, message = "Access denied" });
                }

                var rootPlaceId = await UniverseHandler.GetUniverseRootPlaceIdAsync(connectionString, universeId);
                if (rootPlaceId.HasValue)
                {
                    return Json(new { success = true, placeId = rootPlaceId.Value });
                }
                else
                {
                    return Json(new { success = false, message = "No start place set" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while getting start place" });
            }
        }

        /// <summary>
        /// POST /universes/set-start-place - Set the start place for a universe
        /// </summary>
        [HttpPost("universes/set-start-place")]
        [Authorize]
        public async Task<IActionResult> SetStartPlace()
        {
            try
            {
                var (isAuthenticated, currentUserId) = AuthenticationHelper.GetCurrentUserId(User);
                if (!isAuthenticated)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var universeIdStr = Request.Form["universeId"].FirstOrDefault();
                var placeIdStr = Request.Form["placeId"].FirstOrDefault();

                if (string.IsNullOrWhiteSpace(universeIdStr) || !long.TryParse(universeIdStr, out var universeId))
                {
                    return Json(new { success = false, message = "Invalid universe ID" });
                }

                if (string.IsNullOrWhiteSpace(placeIdStr) || !long.TryParse(placeIdStr, out var placeId))
                {
                    return Json(new { success = false, message = "Invalid place ID" });
                }

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var universeOwner = await GamesRepository.GetUniverseOwnerAsync(connectionString, universeId);
                if (universeOwner == null || universeOwner != currentUserId)
                {
                    return Json(new { success = false, message = "Access denied - you do not own this universe" });
                }

                var placeAsset = await _assetRepository.GetAssetByIdAsync(connectionString, placeId);
                if (placeAsset == null || placeAsset.OwnerUserId != currentUserId)
                {
                    return Json(new { success = false, message = "Place not found or access denied" });
                }

                var success = await PlacesHandler.SetUniverseRootPlaceAsync(connectionString, universeId, placeId);
                
                if (success)
                {
                    return Json(new { success = true, message = "Start place set successfully" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to set start place" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred while setting start place: {ex.Message}" });
            }
        }

        /// <summary>
        /// POST /universes/remove-start-place - Remove the start place from a universe
        /// </summary>
        [HttpPost("universes/remove-start-place")]
        [Authorize]
        public async Task<IActionResult> RemoveStartPlace()
        {
            try
            {
                var (isAuthenticated, currentUserId) = AuthenticationHelper.GetCurrentUserId(User);
                if (!isAuthenticated)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var placeIdStr = Request.Form["placeId"].FirstOrDefault();
                if (string.IsNullOrWhiteSpace(placeIdStr) || !long.TryParse(placeIdStr, out var placeId))
                {
                    return Json(new { success = false, message = "Invalid place ID" });
                }

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var universeId = await GamesRepository.GetUniverseIdFromPlaceIdAsync(connectionString, placeId);
                if (!universeId.HasValue)
                {
                    return Json(new { success = false, message = "Place not found in any universe" });
                }

                var universeOwner = await GamesRepository.GetUniverseOwnerAsync(connectionString, universeId.Value);
                if (universeOwner == null || universeOwner != currentUserId)
                {
                    return Json(new { success = false, message = "Access denied - you do not own this universe" });
                }

                var rootPlaceId = await UniverseHandler.GetUniverseRootPlaceIdAsync(connectionString, universeId.Value);
                if (!rootPlaceId.HasValue || rootPlaceId.Value != placeId)
                {
                    return Json(new { success = false, message = "Place is not the start place of this universe" });
                }

                var success = await UniverseHandler.ClearUniverseRootPlaceAsync(universeId.Value, connectionString);
                
                if (!success)
                {
                    return Json(new { success = false, message = "Failed to remove start place" });
                }

                try
                {
                    await UpdatePlaceInUniverseFieldAsync(placeId, false, connectionString);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to update in_universe field for place {placeId}: {ex.Message}");
                }
                
                return Json(new { success = true, message = "Start place removed successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred while removing start place: {ex.Message}" });
            }
        }

        /// <summary>
        /// POST /universes/addplace - Add a place to a universe
        /// </summary>
        [HttpPost("universes/addplace")]
        [Authorize]
        public async Task<IActionResult> AddPlaceToUniverse()
        {
            try
            {
                var (isAuthenticated, currentUserId) = AuthenticationHelper.GetCurrentUserId(User);
                if (!isAuthenticated)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var universeIdStr = Request.Form["universeId"].FirstOrDefault();
                var placeIdStr = Request.Form["placeId"].FirstOrDefault();

                if (string.IsNullOrWhiteSpace(universeIdStr) || !long.TryParse(universeIdStr, out var universeId))
                {
                    return Json(new { success = false, message = "Invalid universe ID" });
                }

                if (string.IsNullOrWhiteSpace(placeIdStr) || !long.TryParse(placeIdStr, out var placeId))
                {
                    return Json(new { success = false, message = "Invalid place ID" });
                }

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var universeOwner = await GamesRepository.GetUniverseOwnerAsync(connectionString, universeId);
                if (universeOwner == null || universeOwner != currentUserId)
                {
                    return Json(new { success = false, message = "Access denied - you do not own this universe" });
                }

                var placeAsset = await _assetRepository.GetAssetByIdAsync(connectionString, placeId);
                if (placeAsset == null || placeAsset.OwnerUserId != currentUserId)
                {
                    return Json(new { success = false, message = "Place not found or access denied" });
                }

                var success = await PlacesHandler.AddPlaceToUniverseAsync(connectionString, universeId, placeId);
                
                if (success)
                {
                    try
                    {
                        await UpdatePlaceInUniverseFieldAsync(placeId, true, connectionString);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Warning: Failed to update in_universe field for place {placeId}: {ex.Message}");
                    }
                    
                    return Json(new { success = true, message = "Place added to universe successfully" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to add place to universe (place may already be in universe)" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred while adding place to universe: {ex.Message}" });
            }
        }

        /// <summary>
        /// GET /universes/configure-universe-places - Get complete HTML for places tab (start place + other places)
        /// </summary>
        [HttpGet("universes/configure-universe-places")]
        [Authorize]
        public async Task<IActionResult> ConfigureUniversePlaces(long universeId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Content("<div class=\"error\">User not authenticated</div>");
                }

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var universeOwner = await GamesRepository.GetUniverseOwnerAsync(connectionString, universeId);
                if (universeOwner == null || universeOwner != currentUserId)
                {
                    return Content("<div class=\"error\">Access denied</div>");
                }

                var html = await UniverseHandler.GenerateUniversePlacesHtmlAsync(universeId, connectionString, _assetRepository);
                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR ConfigureUniversePlaces: {ex.Message}");
                return Content("<div class=\"error\">An error occurred while loading universe places.</div>", "text/html");
            }
        }

        /// <summary>
        /// POST /universes/removeplace - Remove a place from a universe
        /// </summary>
        [HttpPost("universes/removeplace")]
        [Authorize]
        public async Task<IActionResult> RemovePlaceFromUniverse()
        {
            try
            {
                var (isAuthenticated, currentUserId) = AuthenticationHelper.GetCurrentUserId(User);
                if (!isAuthenticated)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var universeIdStr = Request.Form["universeId"].FirstOrDefault();
                var placeIdStr = Request.Form["placeId"].FirstOrDefault();

                if (string.IsNullOrWhiteSpace(universeIdStr) || !long.TryParse(universeIdStr, out var universeId))
                {
                    return Json(new { success = false, message = "Invalid universe ID" });
                }

                if (string.IsNullOrWhiteSpace(placeIdStr) || !long.TryParse(placeIdStr, out var placeId))
                {
                    return Json(new { success = false, message = "Invalid place ID" });
                }

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var universeOwner = await GamesRepository.GetUniverseOwnerAsync(connectionString, universeId);
                if (universeOwner == null || universeOwner != currentUserId)
                {
                    return Json(new { success = false, message = "Access denied - you do not own this universe" });
                }

                var rootPlaceId = await UniverseHandler.GetUniverseRootPlaceIdAsync(connectionString, universeId);
                if (rootPlaceId.HasValue && rootPlaceId.Value == placeId)
                {
                    return Json(new { success = false, message = "Cannot remove the start place. Please set a different start place first." });
                }

                var success = await PlacesHandler.RemovePlaceFromUniverseAsync(connectionString, universeId, placeId);
                
                if (success)
                {
                    try
                    {
                        await UpdatePlaceInUniverseFieldAsync(placeId, false, connectionString);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Warning: Failed to update in_universe field for place {placeId}: {ex.Message}");
                    }
                    
                    return Json(new { success = true, message = "Place removed from universe successfully" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to remove place from universe" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred while removing place from universe: {ex.Message}" });
            }
        }

        /// <summary>
        /// GET /universes/get-places-by-context - Get places for place selector modal
        /// </summary>
        [HttpGet("universes/get-places-by-context")]
        [Authorize]
        public async Task<IActionResult> GetPlacesByContext(string creationContext, long universeId = 0, int startIndex = 0, int maxRows = 10)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Json(new { 
                        TotalPages = 0,
                        TotalCount = 0,
                        PageNumber = 1,
                        data = new object[0]
                    });
                }

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                if (creationContext == "NonGameCreation" && universeId > 0)
                {
                    var universeOwner = await GamesRepository.GetUniverseOwnerAsync(connectionString, universeId);
                    if (universeOwner != currentUserId)
                    {
                        return Json(new
                        {
                            TotalPages = 0,
                            TotalCount = 0,
                            PageNumber = 1,
                            data = new object[0]
                        });
                    }
                }

                var (places, totalCount, totalPages, pageNumber) = await UniverseHandler.GetPlacesByContextAsync(
                    connectionString, currentUserId, creationContext, universeId, startIndex, maxRows);

                return Json(new
                {
                    TotalPages = totalPages,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    data = places.ToArray(),
                    totalItems = totalCount
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR GetPlacesByContext: {ex.Message}");
                Console.WriteLine($"STACK TRACE: {ex.StackTrace}");
                
                return Json(new
                {
                    TotalPages = 0,
                    TotalCount = 0,
                    PageNumber = 1,
                    data = new object[0]  // Changed from Data to data
                });
            }
        }

        /// <summary>
        /// GET /universes/get-universe-places - Get all places for a universe (excluding root place)
        /// </summary>
        [HttpGet("universes/get-universe-places")]
        [Authorize]
        public async Task<IActionResult> GetUniversePlaces(long universeId, int startRow = 0, int isUniverseCreation = 0)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    return Content("<div class=\"error\">User not authenticated</div>");
                }

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var universeOwner = await GamesRepository.GetUniverseOwnerAsync(connectionString, universeId);
                if (universeOwner == null || universeOwner != currentUserId)
                {
                    return Content("<div class=\"error\">Access denied</div>");
                }

                var html = await UniverseHandler.GenerateUniversePlacesListingHtmlAsync(universeId, startRow, connectionString, _assetRepository);
                return Content(html, "text/html; charset=utf-8");
            }
            catch (Exception ex)
            {
                return Content("<div class=\"error\">An error occurred while loading places.</div>", "text/html");
            }
        }
    }
}
