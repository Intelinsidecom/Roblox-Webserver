using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using Assets;
using Users;

namespace RobloxWebserver.Controllers
{
    [Route("favorite")]
    public class FavoriteController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly AssetsRepository _assetsRepository = new AssetsRepository();
        private readonly UserFavoritesRepository _userFavoritesRepository = new UserFavoritesRepository();

        public FavoriteController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("toggle")]
        public async Task<IActionResult> Toggle(long assetID)
        {
            var connectionString = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return Json(new { success = false, message = "Database connection string is not configured." });
            }

            var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !long.TryParse(userIdClaim, out var userId) || userId <= 0)
            {
                return Json(new { success = false, message = "You must be logged in to favorite this item." });
            }

            if (assetID <= 0)
            {
                return Json(new { success = false, message = "Invalid asset." });
            }

            try
            {
                var alreadyFavorited = await _assetsRepository.UserHasFavoritedAsync(connectionString, userId, assetID).ConfigureAwait(false);
                if (alreadyFavorited)
                {
                    await _assetsRepository.RemoveFavoriteAsync(connectionString, userId, assetID).ConfigureAwait(false);
                    await _userFavoritesRepository.RemoveUserFavoriteAsync(connectionString, userId, assetID).ConfigureAwait(false);
                }
                else
                {
                    await _assetsRepository.AddFavoriteAsync(connectionString, userId, assetID).ConfigureAwait(false);
                    await _userFavoritesRepository.AddUserFavoriteAsync(connectionString, userId, assetID).ConfigureAwait(false);
                }

                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred while updating favorites." });
            }
        }
    }
}
