using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Assets;
using Common;
using Games;
using Npgsql;
using Users;
using Website.Hubs;
using Website.Services;

namespace RobloxWebserver.Controllers.Frontend
{
    [Authorize]
    public class PrivateServerController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHubContext<NotificationHub> _hubContext;

        public PrivateServerController(IConfiguration configuration, IHubContext<NotificationHub> hubContext)
        {
            _configuration = configuration;
            _hubContext = hubContext;
        }

        private string ConnStr() => _configuration.GetConnectionString("Default") ?? string.Empty;

        private long GetUserId()
        {
            var claim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(claim, out var id) && id > 0 ? id : 0;
        }

        private string ArbiterUrl => _configuration["ArbiterUrl"] ?? "http://localhost:5000";

        private static object ErrorPayload(string title, string message) => new
        {
            status = "error",
            showDivID = "TransactionFailureView",
            title,
            errorMsg = message
        };

        private async Task<(UniverseInfo? Universe, (DateTime CreatedAt, DateTime UpdatedAt, int MaxPlayers, int Genre, bool IsAllGenresAllowed, string AllowedGearTypes, bool PrivateServersAllowed, bool PrivateServersFree, int PrivateServersPrice)? PlaceData)>
            LoadUniverseAndPlaceAsync(string connStr, long universeId, CancellationToken ct)
        {
            var universe = await GamesRepository.GetUniverseAsync(connStr, universeId, ct);
            if (universe == null || universe.RootPlaceId <= 0)
                return (null, null);

            var placeData = await AssetsRepository.GetPlaceAdditionalDataAsync(connStr, universe.RootPlaceId, ct);
            return (universe, placeData);
        }

        private async Task<JsonElement?> GetArbiterServerByPrivateServerIdAsync(long privateServerId, CancellationToken ct)
        {
            try
            {
                using var http = new HttpClient { Timeout = Common.HttpClientDefaults.Timeout };
                using var resp = await http.GetAsync($"{ArbiterUrl}/api/gameservers/by-private-server/{privateServerId}", ct);
                if (!resp.IsSuccessStatusCode)
                    return null;

                var json = await resp.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(json);
                return doc.RootElement.Clone();
            }
            catch
            {
                return null;
            }
        }

        // ------------------------------------------------------------------
        // Purchase / create
        // ------------------------------------------------------------------

        [HttpPost("private-server/purchase")]
        [AllowAnonymous]
        public async Task<IActionResult> Purchase(CancellationToken cancellationToken = default)
        {
            var userId = GetUserId();
            if (userId <= 0)
                return Json(ErrorPayload("Login Required", "You must be logged in to create a VIP Server."));

            var form = await Request.ReadFormAsync(cancellationToken);
            var connStr = ConnStr();
            if (string.IsNullOrWhiteSpace(connStr))
                return Json(ErrorPayload("Transaction Failed", "Server configuration error."));

            if (!long.TryParse(form["universeId"], out var universeId) || universeId <= 0)
                return Json(ErrorPayload("Transaction Failed", "Invalid game."));

            var name = form["privateServerName"].ToString();

            var (universe, placeData) = await LoadUniverseAndPlaceAsync(connStr, universeId, cancellationToken);
            if (universe == null)
                return Json(ErrorPayload("Transaction Failed", "Game not found."));

            if (placeData == null || !placeData.Value.PrivateServersAllowed)
                return Json(ErrorPayload("Transaction Failed", "This game does not support VIP Servers."));

            var price = placeData.Value.PrivateServersFree ? 0 : Math.Max(0, placeData.Value.PrivateServersPrice);

            if (int.TryParse(form["expectedPrice"], out var expectedPrice) && expectedPrice != price)
                return Json(ErrorPayload("Transaction Failed", "The price of this VIP Server has changed. Please refresh and try again."));

            var (server, error, newBalance) = await PrivateServersRepository.PurchaseAsync(
                connStr, universeId, universe.RootPlaceId, userId, name, price, cancellationToken);

            if (server == null)
                return Json(ErrorPayload("Transaction Failed", error ?? "Unknown error"));

            var sellerName = await UserQueries.GetUserNameByIdAsync(connStr, universe.CreatorUserId, cancellationToken);

            return Json(new
            {
                status = "success",
                TransactionVerb = "bought",
                AssetName = server.Name,
                AssetType = "VIP Server",
                SellerName = sellerName ?? "ROBLOX",
                Price = price,
                PrivateServerId = server.PrivateServerId,
                UniverseId = server.UniverseId,
                PlaceId = server.PlaceId,
                newBalance,
                showDivID = "TransactionSuccessView"
            });
        }

        // ------------------------------------------------------------------
        // Instance list (consumed by JS/PrivateServers/PrivateServer.js)
        // ------------------------------------------------------------------

        [HttpGet("private-server/instance-list-json")]
        [AllowAnonymous]
        public async Task<IActionResult> InstanceListJson([FromQuery] long universeId, [FromQuery] int page = 1, CancellationToken cancellationToken = default)
        {
            var connStr = ConnStr();
            var userId = GetUserId();

            if (universeId <= 0 || string.IsNullOrWhiteSpace(connStr))
                return Json(new { Instances = Array.Empty<object>(), CurrentPage = 1, TotalPages = 1 });

            var (universe, placeData) = await LoadUniverseAndPlaceAsync(connStr, universeId, cancellationToken);
            if (universe == null)
                return Json(new { Instances = Array.Empty<object>(), CurrentPage = 1, TotalPages = 1 });

            var capacity = placeData?.MaxPlayers ?? 10;

            List<PrivateServerInfo> servers;
            int totalPages;
            if (userId <= 0)
            {
                servers = new List<PrivateServerInfo>();
                totalPages = 1;
            }
            else
            {
                (servers, totalPages) = await PrivateServersRepository.GetForUserInUniverseAsync(
                    connStr, userId, universeId, page < 1 ? 1 : page, 10, cancellationToken);
            }

            var instances = new List<object>();
            foreach (var server in servers)
            {
                var statusType = server.GetStatusType();
                var ownerName = await UserQueries.GetUserNameByIdAsync(connStr, server.OwnerUserId, cancellationToken).ConfigureAwait(false) ?? "Unknown";

                object? gameInstance = null;
                var arbiterServer = await GetArbiterServerByPrivateServerIdAsync(server.PrivateServerId, cancellationToken);
                if (arbiterServer.HasValue && arbiterServer.Value.ValueKind == JsonValueKind.Object)
                {
                    var el = arbiterServer.Value;
                    var gameId = el.TryGetProperty("gameId", out var gid) ? gid.GetString() : null;
                    var status = el.TryGetProperty("status", out var st) ? st.GetString() : null;
                    if (!string.IsNullOrEmpty(gameId) && status != "stopped" && status != "expired")
                    {
                        var playerIds = new List<long>();
                        if (el.TryGetProperty("players", out var playersArr) && playersArr.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var p in playersArr.EnumerateArray())
                            {
                                if (p.TryGetProperty("userId", out var uidEl))
                                    playerIds.Add(uidEl.GetInt64());
                            }
                        }
                        gameInstance = new { Id = gameId, PlayerIds = playerIds };
                    }
                }

                instances.Add(new
                {
                    PlaceId = server.PlaceId,
                    PlaceCapacity = capacity,
                    Name = server.Name,
                    PrivateServerOwnerName = ownerName,
                    DoesBelongToUser = true,
                    UserCanConfigure = true,
                    UserCanShutdown = true,
                    CanRenew = statusType != PrivateServerStatusType.Active,
                    IsPrivateServerSubscriptionActive = statusType == PrivateServerStatusType.Active,
                    JoinScript = $"Roblox.GameLauncher.joinPrivateGame({server.PlaceId}, '{server.AccessCode}', '')",
                    MostRecentPrivateServerStatusChangeReasonType = (int?)null,
                    PrivateServer = new
                    {
                        Id = server.PrivateServerId,
                        UniverseId = server.UniverseId,
                        OwnerUserId = server.OwnerUserId,
                        StatusType = (int)statusType
                    },
                    GameInstance = gameInstance
                });
            }

            return Json(new
            {
                Instances = instances,
                CurrentPage = page < 1 ? 1 : page,
                TotalPages = totalPages
            });
        }

        [HttpGet("private-server/instance-list")]
        [AllowAnonymous]
        public async Task<IActionResult> InstanceList([FromQuery] long universeId, [FromQuery] int page = 1, CancellationToken cancellationToken = default)
        {
            var connStr = ConnStr();
            var userId = GetUserId();

            ViewBag.Servers = new List<PrivateServerInfo>();
            ViewBag.RunningJobIds = new HashSet<long>();
            ViewBag.IsOwnerViewing = false;

            if (universeId > 0 && !string.IsNullOrWhiteSpace(connStr) && userId > 0)
            {
                var (servers, _) = await PrivateServersRepository.GetForUserInUniverseAsync(
                    connStr, userId, universeId, page < 1 ? 1 : page, 10, cancellationToken);

                var running = new HashSet<long>();
                foreach (var server in servers)
                {
                    var arbiterServer = await GetArbiterServerByPrivateServerIdAsync(server.PrivateServerId, cancellationToken);
                    if (arbiterServer.HasValue && arbiterServer.Value.ValueKind == JsonValueKind.Object &&
                        arbiterServer.Value.TryGetProperty("gameId", out var gid))
                    {
                        running.Add(server.PrivateServerId);
                    }
                }

                ViewBag.Servers = servers;
                ViewBag.RunningJobIds = running;
                ViewBag.IsOwnerViewing = true;
            }

            return View("~/Views/Pages/private-server/instance-list.cshtml");
        }

        // ------------------------------------------------------------------
        // Renew
        // ------------------------------------------------------------------

        [HttpPost("private-server/renew")]
        [AllowAnonymous]
        public async Task<IActionResult> Renew(CancellationToken cancellationToken = default)
        {
            var userId = GetUserId();
            if (userId <= 0)
                return Json(new { success = false, error = "You must be logged in." });

            var form = await Request.ReadFormAsync(cancellationToken);
            var connStr = ConnStr();

            if (!long.TryParse(form["privateServerId"], out var privateServerId) || privateServerId <= 0)
                return Json(new { success = false, error = "Invalid VIP server." });

            var server = await PrivateServersRepository.GetByIdAsync(connStr, privateServerId, cancellationToken);
            if (server == null)
                return Json(new { success = false, error = "VIP server not found." });

            var (_, placeData) = await LoadUniverseAndPlaceAsync(connStr, server.UniverseId, cancellationToken);
            var price = placeData != null && !placeData.Value.PrivateServersFree ? Math.Max(0, placeData.Value.PrivateServersPrice) : 0;

            if (int.TryParse(form["expectedPrice"], out var expectedPrice) && expectedPrice != price)
                return Json(new { success = false, error = "The renewal price has changed. Please refresh and try again." });

            var (success, error, _) = await PrivateServersRepository.RenewAsync(
                connStr, privateServerId, userId, price, cancellationToken);

            return Json(success ? new { success = true } : new { success = false, error = error ?? "Unknown error" });
        }

        // ------------------------------------------------------------------
        // Configure page + actions
        // ------------------------------------------------------------------

        private async Task<IActionResult> RedirectToConfigureAsync(long privateServerId, string? msg = null, string? err = null)
        {
            var url = $"/private-server/configure?privateServerId={privateServerId}";
            if (!string.IsNullOrEmpty(msg)) url += "&msg=" + System.Net.WebUtility.UrlEncode(msg);
            if (!string.IsNullOrEmpty(err)) url += "&err=" + System.Net.WebUtility.UrlEncode(err);
            return Redirect(url);
        }

        [HttpGet("private-server/configure")]
        [AllowAnonymous]
        public async Task<IActionResult> Configure([FromQuery] long privateServerId, [FromQuery] string? msg = null, [FromQuery] string? err = null, CancellationToken cancellationToken = default)
        {
            var connStr = ConnStr();
            var userId = GetUserId();

            var server = await PrivateServersRepository.GetByIdAsync(connStr, privateServerId, cancellationToken);
            if (server == null)
                return NotFound();

            if (server.OwnerUserId != userId)
                return StatusCode(403);

            var (universe, placeData) = await LoadUniverseAndPlaceAsync(connStr, server.UniverseId, cancellationToken);
            var whitelist = await PrivateServersRepository.GetWhitelistAsync(connStr, privateServerId, cancellationToken);

            ViewBag.Server = server;
            ViewBag.StatusType = server.GetStatusType();
            ViewBag.Whitelist = whitelist;
            ViewBag.GameName = universe?.Name ?? "Unknown Game";
            ViewBag.MaxPlayers = placeData?.MaxPlayers ?? 10;
            ViewBag.Price = placeData != null && !placeData.Value.PrivateServersFree ? placeData.Value.PrivateServersPrice : 0;
            ViewBag.Message = msg;
            ViewBag.Error = err;

            return View("~/Views/Pages/private-server/configure.cshtml");
        }

        [HttpPost("private-server/configure/rename")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfigureRename(CancellationToken cancellationToken = default)
        {
            var userId = GetUserId();
            if (userId <= 0) return Unauthorized();

            var form = await Request.ReadFormAsync(cancellationToken);
            if (!long.TryParse(form["privateServerId"], out var privateServerId))
                return BadRequest();

            var nameError = PrivateServersRepository.ValidateName(form["name"].ToString());
            if (nameError != null)
                return await RedirectToConfigureAsync(privateServerId, err: nameError);

            var ok = await PrivateServersRepository.RenameAsync(ConnStr(), privateServerId, userId, form["name"].ToString(), cancellationToken);
            return await RedirectToConfigureAsync(privateServerId,
                ok ? "VIP server name updated." : null,
                ok ? null : "Could not rename VIP server.");
        }

        [HttpPost("private-server/configure/whitelist-add")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfigureWhitelistAdd(CancellationToken cancellationToken = default)
        {
            var userId = GetUserId();
            if (userId <= 0) return Unauthorized();

            var form = await Request.ReadFormAsync(cancellationToken);
            var connStr = ConnStr();

            if (!long.TryParse(form["privateServerId"], out var privateServerId))
                return BadRequest();

            var username = form["username"].ToString().Trim();
            if (string.IsNullOrEmpty(username))
                return await RedirectToConfigureAsync(privateServerId, err: "Enter a username to invite.");

            var targetUserId = await GetUserIdByUserNameAsync(connStr, username, cancellationToken);
            if (targetUserId <= 0)
                return await RedirectToConfigureAsync(privateServerId, err: $"User '{username}' was not found.");

            var privacy = await UserQueries.GetPrivateServerInvitePrivacyAsync(connStr, targetUserId, cancellationToken);
            if (privacy.Equals("NoOne", StringComparison.OrdinalIgnoreCase))
                return await RedirectToConfigureAsync(privateServerId, err: $"{username} does not allow private server invites.");

            if (privacy.Equals("Friends", StringComparison.OrdinalIgnoreCase) && !await AreFriendsAsync(connStr, userId, targetUserId))
                return await RedirectToConfigureAsync(privateServerId, err: $"{username} only allows friends to send private server invites.");

            var (ok, addError) = await PrivateServersRepository.WhitelistAddAsync(connStr, privateServerId, userId, targetUserId, cancellationToken);
            if (!ok)
                return await RedirectToConfigureAsync(privateServerId, err: addError);

            var ownerName = await UserQueries.GetUserNameByIdAsync(connStr, userId, cancellationToken).ConfigureAwait(false) ?? "";
            var server = await PrivateServersRepository.GetByIdAsync(connStr, privateServerId, cancellationToken);
            var svc = new NotificationService(connStr);
            await svc.CreateNotificationAsync(
                targetUserId,
                "UserAddedToPrivateServerWhiteList",
                userId,
                ownerName,
                "PrivateServer",
                privateServerId,
                server?.Name ?? "",
                cancellationToken).ConfigureAwait(false);
            await NotificationBroadcaster.BroadcastNewNotification(_hubContext, targetUserId, cancellationToken).ConfigureAwait(false);

            return await RedirectToConfigureAsync(privateServerId, msg: $"{username} can now join this VIP server.");
        }

        [HttpPost("private-server/configure/whitelist-remove")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfigureWhitelistRemove(CancellationToken cancellationToken = default)
        {
            var userId = GetUserId();
            if (userId <= 0) return Unauthorized();

            var form = await Request.ReadFormAsync(cancellationToken);
            if (!long.TryParse(form["privateServerId"], out var privateServerId) ||
                !long.TryParse(form["userId"], out var targetUserId))
                return BadRequest();

            var ok = await PrivateServersRepository.WhitelistRemoveAsync(ConnStr(), privateServerId, userId, targetUserId, cancellationToken);
            return await RedirectToConfigureAsync(privateServerId,
                ok ? "User removed from this VIP server." : null,
                ok ? null : "Could not remove user.");
        }

        [HttpPost("private-server/configure/cancel")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfigureCancel(CancellationToken cancellationToken = default)
        {
            var userId = GetUserId();
            if (userId <= 0) return Unauthorized();

            var form = await Request.ReadFormAsync(cancellationToken);
            if (!long.TryParse(form["privateServerId"], out var privateServerId))
                return BadRequest();

            var ok = await PrivateServersRepository.CancelAsync(ConnStr(), privateServerId, userId, cancellationToken);
            return await RedirectToConfigureAsync(privateServerId,
                ok ? "This VIP server has been cancelled and will no longer renew." : null,
                ok ? null : "Could not cancel VIP server.");
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private static async Task<long> GetUserIdByUserNameAsync(string connectionString, string userName, CancellationToken ct = default)
        {
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(ct);
            await using var cmd = new NpgsqlCommand("select user_id from users where lower(user_name) = lower(@name) limit 1", conn);
            cmd.Parameters.AddWithValue("name", userName);
            var result = await cmd.ExecuteScalarAsync(ct);
            return result == null || result == DBNull.Value ? 0 : Convert.ToInt64(result);
        }

        private static async Task<bool> AreFriendsAsync(string connectionString, long userId1, long userId2, CancellationToken ct = default)
        {
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(ct);
            await using var cmd = new NpgsqlCommand(@"
                select 1 from user_friends where user_id = @u1 and friend_user_id = @u2
                union
                select 1 from user_friends where user_id = @u2 and friend_user_id = @u1
                limit 1", conn);
            cmd.Parameters.AddWithValue("u1", userId1);
            cmd.Parameters.AddWithValue("u2", userId2);
            return await cmd.ExecuteScalarAsync(ct) != null;
        }
    }
}
