using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Assets;
using Npgsql;

namespace RobloxWebserver.Controllers
{
    // Handles endpoints used by the legacy /develop page JavaScript
    [ApiController]
    [Route("develop")]
    [Authorize]
    public class DevelopController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly TShirtAssetService _tshirtService = new TShirtAssetService();
        private readonly PantsAssetService _pantsService = new PantsAssetService();
        private readonly ShirtAssetService _shirtService = new ShirtAssetService();
        private readonly ModelAssetService _modelService;
        private readonly PluginAssetService _pluginService;
        private readonly AudioAssetService _audioService = new AudioAssetService();
        private readonly ShirtAssetsRepository _shirtAssetsRepository = new ShirtAssetsRepository();
        private readonly UserAssetsRepository _userAssetsRepository = new UserAssetsRepository();

        public DevelopController(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _modelService = new ModelAssetService(configuration);
            _pluginService = new PluginAssetService(configuration);
        }

        [HttpGet("develop")]
        public IActionResult DevelopPage()
        {
            if (User?.Identity?.IsAuthenticated == false)
            {
            return View("~/Views/Develop/Guest.cshtml");
            }
            else
            {
            return View("~/Views/Pages/Develop.cshtml");
            }
        }

        [HttpPost("upload-model")]
        public async Task<IActionResult> UploadModel([FromForm] string name, [FromForm] IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is required.");

            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Name is required.");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !long.TryParse(userIdClaim, out var userId) || userId <= 0)
                return Unauthorized("User must be logged in to upload assets.");

            var connStr = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
                return StatusCode(500, "Database connection string is not configured.");

            byte[] fileBytes;
            await using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
                fileBytes = ms.ToArray();
            }

            var assetsDirectory = _configuration["Assets:Directory"];
            if (string.IsNullOrWhiteSpace(assetsDirectory))
                return StatusCode(500, "Assets directory is not configured.");

            try
            {
                _ = await _modelService.CreateModelAsync(
                    connStr,
                    userId,
                    name,
                    fileBytes,
                    assetsDirectory,
                    cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "Failed to save asset record.");
            }

            return Redirect("/develop?view=10");
        }

        [HttpPost("upload-plugin")]
        public async Task<IActionResult> UploadPlugin([FromForm] string name, [FromForm] IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is required.");

            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Name is required.");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !long.TryParse(userIdClaim, out var userId) || userId <= 0)
                return Unauthorized("User must be logged in to upload assets.");

            var connStr = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
                return StatusCode(500, "Database connection string is not configured.");

            byte[] fileBytes;
            await using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
                fileBytes = ms.ToArray();
            }

            var assetsDirectory = _configuration["Assets:Directory"];
            if (string.IsNullOrWhiteSpace(assetsDirectory))
                return StatusCode(500, "Assets directory is not configured.");

            try
            {
                _ = await _pluginService.CreatePluginAsync(
                    connStr,
                    userId,
                    name,
                    fileBytes,
                    assetsDirectory,
                    cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "Failed to save asset record.");
            }

            return Redirect("/develop?view=38");
        }

        [HttpPost("upload-tshirt")]
        public async Task<IActionResult> UploadTShirt([FromForm] string name, [FromForm] IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is required.");

            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Name is required.");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !long.TryParse(userIdClaim, out var userId) || userId <= 0)
                return Unauthorized("User must be logged in to upload assets.");

            var connStr = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
                return StatusCode(500, "Database connection string is not configured.");

            if (await _userAssetsRepository.HasUploadedClothingInLastHourAsync(connStr, userId, cancellationToken).ConfigureAwait(false))
                return BadRequest("You can only upload one shirt, pants, or T-Shirt per hour. Please try again later.");

            byte[] fileBytes;
            await using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
                fileBytes = ms.ToArray();
            }

            var assetsDirectory = _configuration["Assets:Directory"];
            if (string.IsNullOrWhiteSpace(assetsDirectory))
                return StatusCode(500, "Assets directory is not configured.");

            var thumbnailsRoot = _configuration["Thumbnails:OutputDirectory"];
            var thumbnailBaseUrl = _configuration["Thumbnails:ThumbnailUrl"];
            var tshirtTemplatePath = _configuration["Thumbnails:TshirtTemplatePath"];
            var tshirtTemplateHighResPath = _configuration["Thumbnails:TshirtTemplateHighResPath"];
            var publicAssetBaseUrl = _configuration["Assets:PublicBaseUrl"];

            try
            {
                var scheme = string.IsNullOrEmpty(Request.Scheme) ? "http" : Request.Scheme;
                var host = Request.Host.HasValue ? Request.Host.Value : "localhost";
                var baseUrl = $"{scheme}://{host}";

                _ = await _tshirtService.CreateTShirtAsync(
                    connStr,
                    userId,
                    name,
                    file.FileName,
                    file.ContentType,
                    fileBytes,
                    assetsDirectory,
                    thumbnailsRoot ?? string.Empty,
                    thumbnailBaseUrl ?? string.Empty,
                    tshirtTemplatePath ?? string.Empty,
                    tshirtTemplateHighResPath ?? string.Empty,
                    baseUrl,
                    publicAssetBaseUrl,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return StatusCode(500, "Failed to save asset record.");
            }

            return Redirect("/develop?view=2");
        }

        [HttpPost("upload-pants")]
        public async Task<IActionResult> UploadPants([FromForm] string name, [FromForm] IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is required.");

            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Name is required.");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !long.TryParse(userIdClaim, out var userId) || userId <= 0)
                return Unauthorized("User must be logged in to upload assets.");

            var connStr = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
                return StatusCode(500, "Database connection string is not configured.");

            byte[] fileBytes;
            await using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
                fileBytes = ms.ToArray();
            }

            var assetsDirectory = _configuration["Assets:Directory"];
            var thumbnailsRoot = _configuration["Thumbnails:OutputDirectory"];
            var thumbnailBaseUrl = _configuration["Thumbnails:ThumbnailUrl"];

            if (string.IsNullOrWhiteSpace(assetsDirectory))
                return StatusCode(500, "Assets directory is not configured.");
            if (string.IsNullOrWhiteSpace(thumbnailsRoot) || string.IsNullOrWhiteSpace(thumbnailBaseUrl))
                return StatusCode(500, "Thumbnail configuration is not configured.");

            try
            {
                var scheme = string.IsNullOrEmpty(Request.Scheme) ? "http" : Request.Scheme;
                var host = Request.Host.HasValue ? Request.Host.Value : "localhost";
                var baseUrl = $"{scheme}://{host}";
                var arbiterBaseUrl = _configuration["Arbiter:BaseUrl"];

                _ = await _pantsService.CreatePantsAsync(
                    connStr,
                    userId,
                    name,
                    file.FileName,
                    file.ContentType,
                    fileBytes,
                    assetsDirectory,
                    baseUrl,
                    thumbnailsRoot,
                    thumbnailBaseUrl ?? string.Empty,
                    _configuration["Assets:PublicBaseUrl"],
                    arbiterBaseUrl,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "Failed to save asset record.");
            }

            return Redirect("/develop?view=12");
        }

        [HttpPost("upload-shirt")]
        public async Task<IActionResult> UploadShirt([FromForm] string name, [FromForm] IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is required.");

            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Name is required.");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !long.TryParse(userIdClaim, out var userId) || userId <= 0)
                return Unauthorized("User must be logged in to upload assets.");

            var connStr = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
                return StatusCode(500, "Database connection string is not configured.");

            byte[] fileBytes;
            await using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
                fileBytes = ms.ToArray();
            }

            var assetsDirectory = _configuration["Assets:Directory"];
            var thumbnailsRoot = _configuration["Thumbnails:OutputDirectory"];
            var thumbnailBaseUrl = _configuration["Thumbnails:ThumbnailUrl"];

            if (string.IsNullOrWhiteSpace(assetsDirectory))
                return StatusCode(500, "Assets directory is not configured.");
            if (string.IsNullOrWhiteSpace(thumbnailsRoot) || string.IsNullOrWhiteSpace(thumbnailBaseUrl))
                return StatusCode(500, "Thumbnail configuration is not configured.");

            try
            {
                var scheme = string.IsNullOrEmpty(Request.Scheme) ? "http" : Request.Scheme;
                var host = Request.Host.HasValue ? Request.Host.Value : "localhost";
                var baseUrl = $"{scheme}://{host}";
                var arbiterBaseUrl = _configuration["Arbiter:BaseUrl"];

                _ = await _shirtService.CreateShirtAsync(
                    connStr,
                    userId,
                    name,
                    file.FileName,
                    file.ContentType,
                    fileBytes,
                    assetsDirectory,
                    baseUrl,
                    thumbnailsRoot,
                    thumbnailBaseUrl ?? string.Empty,
                    _configuration["Assets:PublicBaseUrl"],
                    arbiterBaseUrl,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "Failed to save asset record.");
            }

            return Redirect("/develop?view=11");
        }

        [HttpPost("audio-upload")]
        public async Task<IActionResult> UploadAudio(CancellationToken cancellationToken)
        {
            var form = await Request.ReadFormAsync(cancellationToken);
            var name = form["name"].FirstOrDefault();
            var file = form.Files.GetFile("file");

            if (file == null || file.Length == 0)
            {
                TempData["AudioUploadError"] = "File is required.";
                return Redirect("/develop?view=3");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["AudioUploadError"] = "Name is required.";
                return Redirect("/develop?view=3");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !long.TryParse(userIdClaim, out var userId) || userId <= 0)
            {
                TempData["AudioUploadError"] = "You must be logged in to upload assets.";
                return Redirect("/develop?view=3");
            }

            var connStr = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
            {
                TempData["AudioUploadError"] = "Database connection string is not configured.";
                return Redirect("/develop?view=3");
            }

            byte[] fileBytes;
            await using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
                fileBytes = ms.ToArray();
            }

            var assetsDirectory = _configuration["Assets:Directory"];
            if (string.IsNullOrWhiteSpace(assetsDirectory))
            {
                TempData["AudioUploadError"] = "Assets directory is not configured.";
                return Redirect("/develop?view=3");
            }

            var audioThumbnailUrl = _configuration["AudioThumbnailUrl"];
            var audioHighResThumbnailUrl = _configuration["AudioHighResThumbnailUrl"];

            try
            {
                _ = await _audioService.CreateAudioAsync(
                    connStr,
                    userId,
                    name,
                    fileBytes,
                    assetsDirectory,
                    thumbnailUrl: audioThumbnailUrl,
                    highResThumbnailUrl: audioHighResThumbnailUrl,
                    cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            catch (ArgumentException ex)
            {
                TempData["AudioUploadError"] = ex.Message;
                return Redirect("/develop?view=3");
            }
            catch (Exception)
            {
                TempData["AudioUploadError"] = "Failed to save asset record.";
                return Redirect("/develop?view=3");
            }

            return Redirect("/develop?view=3");
        }
    }
}


