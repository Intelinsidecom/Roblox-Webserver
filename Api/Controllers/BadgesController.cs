using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Assets;

namespace Api.Controllers
{
    [ApiController]
    [Route("badges")]
    public class BadgesController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AssetsRepository _assetsRepo = new AssetsRepository();
        private readonly UserAssetsRepository _userAssetsRepo = new UserAssetsRepository();

        public BadgesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("award")]
        public async Task<IActionResult> AwardBadge([FromQuery] long badgeId, [FromQuery] long userId)
        {
            if (badgeId <= 0 || userId <= 0)
                return Content("0", "text/plain");

            try
            {
                var connStr = _configuration.GetConnectionString("Default");
                if (string.IsNullOrWhiteSpace(connStr))
                    return Content("0", "text/plain");

                var exists = await _assetsRepo.BadgeExistsAsync(connStr, badgeId);
                if (!exists)
                    return Content("0", "text/plain");

                await _userAssetsRepo.AddUserAssetAsync(connStr, userId, badgeId);

                await _assetsRepo.IncrementAssetSalesAsync(connStr, badgeId);

                return Content("Success", "text/plain");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] AwardBadge: {ex.Message}");
                return Content("0", "text/plain");
            }
        }

        [HttpGet("has-badge")]
        public async Task<IActionResult> HasBadge([FromQuery] long userId, [FromQuery] long badgeId)
        {
            if (userId <= 0 || badgeId <= 0)
                return Content("Failure", "text/plain");

            try
            {
                var connStr = _configuration.GetConnectionString("Default");
                if (string.IsNullOrWhiteSpace(connStr))
                    return Content("Failure", "text/plain");

                var owns = await _userAssetsRepo.UserOwnsAssetAsync(connStr, userId, badgeId);
                return Content(owns ? "Success" : "Failure", "text/plain");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] HasBadge: {ex.Message}");
                return Content("Failure", "text/plain");
            }
        }

        [HttpGet("is-disabled")]
        public IActionResult IsDisabled([FromQuery] long badgeId, [FromQuery] long placeId)
        {
            if (badgeId <= 0)
                return Content("0", "text/plain");

            return Content("0", "text/plain");
        }

        [HttpGet("is-legal")]
        public async Task<IActionResult> IsLegal([FromQuery] long badgeId, [FromQuery] long placeId)
        {
            if (badgeId <= 0)
                return Content("0", "text/plain");

            try
            {
                var connStr = _configuration.GetConnectionString("Default");
                if (string.IsNullOrWhiteSpace(connStr))
                    return Content("0", "text/plain");

                var exists = await _assetsRepo.BadgeExistsAsync(connStr, badgeId);
                return Content(exists ? "1" : "0", "text/plain");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] IsLegal: {ex.Message}");
                return Content("0", "text/plain");
            }
        }

        [HttpPost("enable")]
        public IActionResult EnableBadge([FromQuery] long badgeId)
        {
            return Ok(new { success = true });
        }

        [HttpPost("disable")]
        public IActionResult DisableBadge([FromQuery] long badgeId)
        {
            return Ok(new { success = true });
        }
    }
}
