using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Assets;

namespace Api.Controllers
{
    [ApiController]
    [Route("ownership")]
    public class OwnershipController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserAssetsRepository _userAssetsRepo = new UserAssetsRepository();

        public OwnershipController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("hasasset")]
        public async Task<IActionResult> HasAsset([FromQuery] long userId, [FromQuery] long assetId)
        {
            if (userId <= 0 || assetId <= 0)
                return Content("false", "text/plain");

            try
            {
                var connStr = _configuration.GetConnectionString("Default");
                if (string.IsNullOrWhiteSpace(connStr))
                    return Content("false", "text/plain");

                var owns = await _userAssetsRepo.UserOwnsAssetAsync(connStr, userId, assetId);
                return Content(owns.ToString().ToLower(), "text/plain");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] HasAsset: {ex.Message}");
                return Content("false", "text/plain");
            }
        }
    }
}
