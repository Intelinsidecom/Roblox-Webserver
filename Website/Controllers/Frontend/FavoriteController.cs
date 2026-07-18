using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using Assets;
using Common;
using Users;
using Website.Hubs;
using Website.Services;

namespace RobloxWebserver.Controllers
{
    [Route("favorite")]
    public class FavoriteController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly AssetsRepository _assetsRepository = new AssetsRepository();
        private readonly UserFavoritesRepository _userFavoritesRepository = new UserFavoritesRepository();

        public FavoriteController(IConfiguration configuration, IHubContext<NotificationHub> hubContext)
        {
            _configuration = configuration;
            _hubContext = hubContext;
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

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            long? assetOwnerId = null;
                            string assetName = "";
                            using var conn = new Npgsql.NpgsqlConnection(connectionString);
                            await conn.OpenAsync().ConfigureAwait(false);
                            using var cmd = new Npgsql.NpgsqlCommand("SELECT owner_user_id, name FROM assets WHERE asset_id = @id", conn);
                            cmd.Parameters.AddWithValue("id", assetID);
                            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
                            if (await reader.ReadAsync().ConfigureAwait(false))
                            {
                                if (!reader.IsDBNull(0)) assetOwnerId = reader.GetInt64(0);
                                if (!reader.IsDBNull(1)) assetName = reader.GetString(1);
                            }

                            string userName = "";
                            using var nameConn = new Npgsql.NpgsqlConnection(connectionString);
                            await nameConn.OpenAsync().ConfigureAwait(false);
                            using var nameCmd = new Npgsql.NpgsqlCommand("SELECT user_name FROM users WHERE user_id = @id", nameConn);
                            nameCmd.Parameters.AddWithValue("id", userId);
                            var nameResult = await nameCmd.ExecuteScalarAsync().ConfigureAwait(false);
                            if (nameResult != null) userName = nameResult.ToString() ?? "";

                            if (assetOwnerId.HasValue && assetOwnerId.Value != userId)
                            {
                                var svc = new NotificationService(connectionString);
                                await svc.CreateNotificationAsync(
                                    assetOwnerId.Value,
                                    "AssetFavorited",
                                    userId,
                                    userName,
                                    "Asset",
                                    assetID,
                                    assetName,
                                    default
                                ).ConfigureAwait(false);
                                await NotificationBroadcaster.BroadcastNewNotification(_hubContext, assetOwnerId.Value, default).ConfigureAwait(false);
                            }
                        }
                        catch
                        {
                        }
                    });
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] ToggleFavorite assetId={assetID}: {ex}");
                return Json(new { success = false, message = "An error occurred while updating favorites." });
            }
        }
    }
}
