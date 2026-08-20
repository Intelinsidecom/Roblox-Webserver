using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Assets;
using Games;
using Thumbnails;
using Users;

namespace RobloxWebserver.Controllers
{
    // Minimal stub endpoints required by the front-end Build/Develop JavaScript.
    [ApiController]
    [Route("build")]
    [Authorize]
    public class BuildController : Controller
    {
        private readonly IConfiguration _configuration;

        public BuildController(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public string? ConnectionString => _configuration.GetConnectionString("Default");

        /// <summary>
        /// Legacy endpoint used by UniverseLoader.js. Returns HTML table of user's universes.
        /// </summary>
        [HttpGet("universes")]
        public async Task<IActionResult> Universes(int startRow = 0, bool activeOnly = false, long? groupId = null, CancellationToken cancellationToken = default)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !long.TryParse(userIdClaim, out var userId) || userId <= 0)
                return Content(string.Empty, "text/html");

            var connStr = ConnectionString;
            if (string.IsNullOrWhiteSpace(connStr))
                return Content(string.Empty, "text/html");

            try
            {
                var universes = await GameListingService
                    .GetUniversesForUserAsync(connStr, userId, cancellationToken)
                    .ConfigureAwait(false);

                var html = new System.Text.StringBuilder();
                
                foreach (var u in universes)
                {
                    if (activeOnly && u.PrivacyLevel != 1) continue;

                    var statusText = u.PrivacyLevel switch
                    {
                        2 => "Friends",
                        3 => "Private",
                        _ => "Public",
                    };

                    var privacyClass = u.PrivacyLevel == 1 ? "place-active" : "place-inactive";

                    html.AppendLine(@"<table class='item-table' data-item-id='" + u.UniverseId + @"' data-rootplace-id='" + u.RootPlaceId + @"' data-type='universes'>
<tr>
    <td class='image-col universe-image-col' style='text-align:center;'>
        <a href='/universes/configure?id=" + u.UniverseId + @"' class='game-image'>
            <img src='" + u.ThumbnailUrl + @"' alt='" + u.UniverseName + @"' />
        </a>
    </td>
    <td class='universe-name-col'>
        <a class='title notranslate' href='/universes/configure?id=" + u.UniverseId + @"'>" + u.UniverseName + @"</a>
        <table class='details-table'>
            <tr>
                <td class='item-universe'>
                    <span>Start Place:</span>
                    <a class='title notranslate start-place-url' href='/games/" + u.RootPlaceId + @"/" + System.Web.HttpUtility.UrlEncode(u.PlaceName) + @"'>" + u.PlaceName + @"</a>
                </td>
            </tr>
            <tr class='activate-cell'>
                <td>
                    <a class='" + privacyClass + @"' href='/universes/configure?id=" + u.UniverseId + @"'>" + statusText + @"</a>
                </td>
            </tr>
        </table>
    </td>
    <td class='edit-col'>
        <a class='roblox-edit-button btn-control btn-control-large' href='javascript:editGameInStudio(" + u.RootPlaceId + @", " + u.UniverseId + @", true)'>Edit</a>
    </td>
    <td class='menu-col'>
        <div class='gear-button-wrapper'>
            <a href='#' class='gear-button'></a>
        </div>
    </td>
</tr>
</table>
<div class='separator'></div>");
                }

                return Content(html.ToString(), "text/html");
            }
            catch
            {
                return Content(string.Empty, "text/html");
            }
        }

        /// <summary>
        /// Renders the embedded asset upload form loaded in an iframe by the Develop page.
        /// </summary>
        [HttpGet("upload")]
        public async Task<IActionResult> Upload(int assetTypeId = 0, long? targetPlaceId = null, string? groupId = null, CancellationToken cancellationToken = default)
        {
            ViewBag.AssetTypeId = assetTypeId;
            ViewBag.TargetPlaceId = targetPlaceId;
            ViewBag.GroupId = groupId;
            ViewBag.GameName = null;
            ViewBag.BadgeUploadCost = _configuration.GetValue<int>("Badge:UploadCost");

            if (targetPlaceId.HasValue && targetPlaceId.Value > 0)
            {
                var connStr = ConnectionString;
                if (!string.IsNullOrWhiteSpace(connStr))
                {
                    try
                    {
                        var place = await new AssetMetadataRepository()
                            .GetPlaceByIdAsync(connStr, targetPlaceId.Value, cancellationToken)
                            .ConfigureAwait(false);
                        ViewBag.GameName = string.IsNullOrWhiteSpace(place?.Name) ? null : place.Name;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] BuildUpload (place lookup): {ex}");
                    }
                }
            }

            return View("~/Views/Develop/BuildUpload.cshtml");
        }

        /// <summary>
        /// Handles the two-step upload flow for place-specific assets (game passes, badges).
        /// onVerificationPage=False: accepts the multipart file and renders the confirmation page.
        /// onVerificationPage=True: finalizes the upload and shows the success message.
        /// </summary>
        [HttpPost("doverifiedupload")]
        public async Task<IActionResult> DoVerifiedUpload(
            [FromForm] string? onVerificationPage = null,
            [FromForm] int assetTypeId = 0,
            [FromForm] long? targetPlaceId = null,
            [FromForm] string? groupId = null,
            [FromForm] string? name = null,
            [FromForm] string? description = null,
            [FromForm] string? img = null,
            [FromForm] IFormFile? file = null,
            CancellationToken cancellationToken = default)
        {
            ViewBag.AssetTypeId = assetTypeId;
            ViewBag.TargetPlaceId = targetPlaceId;
            ViewBag.GroupId = groupId;
            ViewBag.GameName = null;
            ViewBag.UploadName = name;
            ViewBag.UploadDescription = description;
            ViewBag.UploadImageBase64 = img;
            ViewBag.UploadSuccess = false;
            ViewBag.BadgeUploadCost = _configuration.GetValue<int>("Badge:UploadCost");

            if (targetPlaceId.HasValue && targetPlaceId.Value > 0)
            {
                var connStr = ConnectionString;
                if (!string.IsNullOrWhiteSpace(connStr))
                {
                    try
                    {
                        var place = await new AssetMetadataRepository()
                            .GetPlaceByIdAsync(connStr, targetPlaceId.Value, cancellationToken)
                            .ConfigureAwait(false);
                        ViewBag.GameName = string.IsNullOrWhiteSpace(place?.Name) ? null : place.Name;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] BuildUpload (place lookup): {ex}");
                    }
                }
            }

            if (string.Equals(onVerificationPage, "True", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.UploadAssetId = null;
                ViewBag.UploadError = null;
                string? uploadError = null;

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrWhiteSpace(userIdClaim) || !long.TryParse(userIdClaim, out var userId) || userId <= 0)
                {
                    uploadError = assetTypeId == 21
                        ? "You must be logged in to create a badge."
                        : "You must be logged in to create a game pass.";
                }
                else
                {
                    var connStr = ConnectionString;
                    if (string.IsNullOrWhiteSpace(connStr))
                    {
                        uploadError = "Server configuration error.";
                    }
                    else
                    {
                        var assetsDirectory = _configuration["Assets:Directory"];
                        var thumbnailsRoot = _configuration["Thumbnails:OutputDirectory"];
                        var thumbnailBaseUrl = _configuration["Thumbnails:ThumbnailUrl"];

                        if (string.IsNullOrWhiteSpace(assetsDirectory) || string.IsNullOrWhiteSpace(thumbnailsRoot) || string.IsNullOrWhiteSpace(thumbnailBaseUrl))
                        {
                            uploadError = "Server configuration error.";
                        }
                        else if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(img) || !targetPlaceId.HasValue || targetPlaceId.Value <= 0)
                        {
                            uploadError = "Missing upload details.";
                        }
                        else
                        {
                            var badgeUploadCost = _configuration.GetValue<int>("Badge:UploadCost");
                            if (badgeUploadCost > 0 && assetTypeId == 21)
                            {
                                var balance = await UserQueries.GetCurrencyByIdAsync(connStr, userId, "robux", cancellationToken).ConfigureAwait(false);
                                if (balance < badgeUploadCost)
                                {
                                    uploadError = "You do not have enough Robux to create this badge.";
                                }
                            }

                            if (uploadError == null)
                            {
                            try
                            {
                                var imageBytes = Convert.FromBase64String(img);

                                try
                                {
                                    long assetId;
                                    if (assetTypeId == 21)
                                    {
                                        assetId = await new BadgeAssetService().CreateBadgeAsync(
                                            connStr,
                                            userId,
                                            name,
                                            description ?? string.Empty,
                                            imageBytes,
                                            targetPlaceId.Value,
                                            assetsDirectory,
                                            thumbnailsRoot,
                                            thumbnailBaseUrl,
                                            cancellationToken).ConfigureAwait(false);
                                    }
                                    else
                                    {
                                        assetId = await new GamePassAssetService().CreateGamePassAsync(
                                            connStr,
                                            userId,
                                            name,
                                            description ?? string.Empty,
                                            imageBytes,
                                            targetPlaceId.Value,
                                            assetsDirectory,
                                            thumbnailsRoot,
                                            thumbnailBaseUrl,
                                            cancellationToken).ConfigureAwait(false);
                                    }

                                    if (assetTypeId == 21 && badgeUploadCost > 0)
                                    {
                                        await UserQueries.IncrementCurrencyByIdAsync(connStr, userId, "robux", -badgeUploadCost, cancellationToken).ConfigureAwait(false);
                                    }

                                    ViewBag.UploadSuccess = true;
                                    ViewBag.UploadAssetId = assetId;

                                    // Reset the form so the next upload starts clean.
                                    ViewBag.UploadName = null;
                                    ViewBag.UploadDescription = null;
                                    ViewBag.UploadImageBase64 = null;
                                }
                                catch (InvalidOperationException ex)
                                {
                                    uploadError = ex.Message;
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"[ERROR] DoVerifiedUpload (asset create): {ex}");
                                    uploadError = assetTypeId == 21
                                        ? "Something went wrong while creating your badge."
                                        : "Something went wrong while creating your game pass.";
                                }
                            }
                            catch (FormatException)
                            {
                                uploadError = "The uploaded image could not be read.";
                            }
                            }
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(uploadError))
                    ViewBag.UploadError = uploadError;

                // Always return to the upload form: it shows the success banner on
                // real success, or the red error box on failure so the user can retry.
                return View("~/Views/Develop/BuildUpload.cshtml");
            }

            if (file == null || file.Length == 0)
            {
                return View("~/Views/Develop/BuildUpload.cshtml");
            }

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
            ViewBag.UploadName = string.IsNullOrWhiteSpace(name)
                ? Path.GetFileNameWithoutExtension(file.FileName)
                : name;

            var fileBytes = ms.ToArray();
            var squareImage = PlaceThumbnail.ResizeImage(fileBytes, 512, 512);
            ViewBag.UploadImageBase64 = Convert.ToBase64String(squareImage);

            return View("~/Views/Develop/BuildVerifyUpload.cshtml");
        }
    }
}
