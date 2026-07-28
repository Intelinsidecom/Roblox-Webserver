using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Users;

namespace RobloxWebserver.Controllers
{
    public class MyPagesController : Controller
    {
        private readonly ICompositeViewEngine _viewEngine;
        private readonly IConfiguration _configuration;

        public MyPagesController(ICompositeViewEngine viewEngine, IConfiguration configuration)
        {
            _viewEngine = viewEngine;
            _configuration = configuration;
        }

        [HttpGet("my/character")]
        public IActionResult Character()
        {
            if (User?.Identity?.IsAuthenticated != true)
                return Redirect("/");

            const string viewPath = "~/Views/Pages/My/Character.aspx.cshtml";
            var result = _viewEngine.GetView(null, viewPath, true);
            if (!result.Success)
                return NotFound();

            return View(viewPath);
        }

        [HttpGet("my/settings/json")]
        public async Task<IActionResult> SettingsJson()
        {
            var (isValid, userId) = AuthenticationHelper.GetCurrentUserId(User);
            if (!isValid)
                return Unauthorized();

            var connStr = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
                return Json(new { });

            string? userName = null;
            string? email = null;
            bool emailVerified = false;
            DateTime? userCreated = null;
            string? subscriptionType = null;
            bool premiumMember = false;
            bool restrictionsEnabled = false;
            bool twoStepEnabled = false;

            try
            {
                await using var conn = new NpgsqlConnection(connStr);
                await conn.OpenAsync();

                await using var cmd = new NpgsqlCommand(
                    @"select user_name, email, email_verified, user_created, subscription_type, premium_member,
                             account_restrictions_enabled, ""2sv_enabled""
                      from users where user_id = @uid", conn);
                cmd.Parameters.AddWithValue("uid", userId);

                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    userName = reader.IsDBNull(0) ? null : reader.GetString(0);
                    email = reader.IsDBNull(1) ? null : reader.GetString(1);
                    emailVerified = !reader.IsDBNull(2) && reader.GetBoolean(2);
                    userCreated = reader.IsDBNull(3) ? null : reader.GetDateTime(3);
                    subscriptionType = reader.IsDBNull(4) ? null : reader.GetString(4);
                    premiumMember = !reader.IsDBNull(5) && reader.GetBoolean(5);
                    restrictionsEnabled = !reader.IsDBNull(6) && reader.GetBoolean(6);
                    twoStepEnabled = !reader.IsDBNull(7) && reader.GetBoolean(7);
                }
            }
            catch
            {
                // Return partial data on DB error
            }

            var isAnyBc = subscriptionType == "BuildersClub" || subscriptionType == "TurboBuildersClub" ||
                          subscriptionType == "OutrageousBuildersClub";

            return Json(new
            {
                ChangeUsernameEnabled = true,
                IsAdmin = false,
                UserId = userId,
                Name = userName ?? "User",
                DisplayName = userName ?? "User",
                IsEmailOnFile = email != null,
                IsEmailVerified = emailVerified,
                IsPhoneFeatureEnabled = true,
                RobuxRemainingForUsernameChange = 0,
                PreviousUserNames = "",
                UseSuperSafePrivacyMode = false,
                IsSuperSafeModeEnabledForPrivacySetting = false,
                UseSuperSafeChat = false,
                IsAppChatSettingEnabled = true,
                IsGameChatSettingEnabled = true,
                IsAccountPrivacySettingsV2Enabled = true,
                IsSetPasswordNotificationEnabled = false,
                ChangePasswordRequiresTwoStepVerification = false,
                ChangeEmailRequiresTwoStepVerification = false,
                UserEmail = email ?? "",
                UserEmailMasked = email != null,
                UserEmailVerified = emailVerified,
                CanHideInventory = true,
                CanTrade = false,
                MissingParentEmail = false,
                IsUpdateEmailSectionShown = true,
                IsUnder13UpdateEmailMessageSectionShown = false,
                IsUserConnectedToFacebook = false,
                IsTwoStepToggleEnabled = twoStepEnabled,
                AgeBracket = 0,
                UserAbove13 = true,
                ClientIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                AccountAgeInDays = userCreated.HasValue ? (int)(DateTime.UtcNow - userCreated.Value).TotalDays : 0,
                IsOBC = subscriptionType == "OutrageousBuildersClub",
                IsTBC = subscriptionType == "TurboBuildersClub",
                IsAnyBC = isAnyBc,
                IsPremium = premiumMember,
                IsBcRenewalMembership = false,
                BcExpireDate = "/Date(-0)/",
                BcRenewalPeriod = (string?)null,
                BcLevel = (int?)null,
                HasCurrencyOperationError = false,
                CurrencyOperationErrorMessage = (string?)null,
                BlockedUsersModel = new
                {
                    BlockedUserIds = Array.Empty<int>(),
                    BlockedUsers = Array.Empty<string>(),
                    MaxBlockedUsers = 50,
                    Total = 0,
                    Page = 1
                },
                Tab = (string?)null,
                ChangePassword = false,
                IsAccountPinEnabled = true,
                IsAccountRestrictionsFeatureEnabled = true,
                IsAccountRestrictionsSettingEnabled = restrictionsEnabled,
                IsAccountSettingsSocialNetworksV2Enabled = false,
                IsUiBootstrapModalV2Enabled = true,
                IsI18nBirthdayPickerInAccountSettingsEnabled = true,
                InApp = false,
                MyAccountSecurityModel = new
                {
                    IsEmailSet = email != null,
                    IsEmailVerified = emailVerified,
                    IsTwoStepEnabled = twoStepEnabled,
                    ShowSignOutFromAllSessions = true,
                    TwoStepVerificationViewModel = new
                    {
                        UserId = userId,
                        IsEnabled = twoStepEnabled,
                        CodeLength = 6,
                        ValidCodeCharacters = (int?)null
                    }
                },
                ApiProxyDomain = _configuration["PublicBaseUrl"] ?? _configuration["BaseUrl"] ?? $"{Request.Scheme}://{Request.Host}",
                AccountSettingsApiDomain = _configuration["PublicBaseUrl"] ?? _configuration["BaseUrl"] ?? $"{Request.Scheme}://{Request.Host}",
                AuthDomain = _configuration["PublicBaseUrl"] ?? _configuration["BaseUrl"] ?? $"{Request.Scheme}://{Request.Host}",
                IsDisconnectFbSocialSignOnEnabled = true,
                IsDisconnectXboxEnabled = true,
                NotificationSettingsDomain = _configuration["PublicBaseUrl"] ?? _configuration["BaseUrl"] ?? $"{Request.Scheme}://{Request.Host}",
                AllowedNotificationSourceTypes = new[]
                {
                    "Test", "FriendRequestReceived", "FriendRequestAccepted",
                    "PartyInviteReceived", "PartyMemberJoined", "ChatNewMessage",
                    "PrivateMessageReceived", "UserAddedToPrivateServerWhiteList",
                    "ConversationUniverseChanged", "TeamCreateInvite", "GameUpdate",
                    "DeveloperMetricsAvailable"
                },
                AllowedReceiverDestinationTypes = new[]
                {
                    "DesktopPush", "NotificationStream"
                },
                BlacklistedNotificationSourceTypesForMobilePush = Array.Empty<string>(),
                MinimumChromeVersionForPushNotifications = 50,
                PushNotificationsEnabledOnFirefox = true,
                LocaleApiDomain = _configuration["PublicBaseUrl"] ?? _configuration["BaseUrl"] ?? $"{Request.Scheme}://{Request.Host}",
                HasValidPasswordSet = true,
                IsUpdateEmailApiEndpointEnabled = true,
                FastTrackMember = (string?)null,
                IsFastTrackAccessible = false,
                HasFreeNameChange = false,
                IsAgeDownEnabled = false,
                IsSendVerifyEmailApiEndpointEnabled = true,
                IsPromotionChannelsEndpointEnabled = true,
                ReceiveNewsletter = false,
                SocialNetworksVisibilityPrivacy = 6,
                SocialNetworksVisibilityPrivacyValue = "AllUsers",
                Facebook = (string?)null,
                Twitter = (string?)null,
                YouTube = (string?)null,
                Twitch = (string?)null
            });
        }

        [HttpGet("messages/compose")]
        public async Task<IActionResult> Compose([FromQuery] long? recipientId)
        {
            if (User?.Identity?.IsAuthenticated != true)
                return Redirect("/");

            var (isValid, currentUserId) = AuthenticationHelper.GetCurrentUserId(User);
            if (!isValid || currentUserId <= 0)
                return Redirect("/");

            if (recipientId == null || recipientId <= 0)
                return Redirect("/my/messages");

            if (recipientId.Value == currentUserId)
                return Redirect("/my/messages");

            var connStr = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
                return Redirect("/my/messages");

            bool recipientExists;
            try
            {
                recipientExists = await UserQueries.UserExistsAsync(connStr, recipientId.Value);
            }
            catch
            {
                return Redirect("/my/messages");
            }

            if (!recipientExists)
                return Redirect("/my/messages");

            string recipientUserName = "";
            try
            {
                recipientUserName = await UserQueries.GetUserNameByIdAsync(connStr, recipientId.Value) ?? "";
            }
            catch { }

            long robux = 0, tix = 0;
            try
            {
                robux = await UserQueries.GetCurrencyByIdAsync(connStr, currentUserId, "robux");
                tix = await UserQueries.GetCurrencyByIdAsync(connStr, currentUserId, "tix");
            }
            catch { }

            ViewBag.CurrentUserId = currentUserId;
            ViewBag.RecipientId = recipientId.Value;
            ViewBag.RecipientUserName = recipientUserName;
            ViewBag.Robux = robux;
            ViewBag.Tix = tix;

            const string viewPath = "~/Views/Pages/messages/compose.cshtml";
            var viewResult = _viewEngine.GetView(null, viewPath, true);
            if (!viewResult.Success)
                return NotFound();

            return View(viewPath);
        }

        [HttpGet("My/{*path}")]
        public IActionResult Route(string? path)
        {
            if (User?.Identity?.IsAuthenticated != true)
                return Redirect("/");

            var pagePath = string.IsNullOrWhiteSpace(path) ? "My/Index" : $"My/{path.Trim()}";
            pagePath = pagePath.Replace('\\', '/');
            while (pagePath.Contains("..", StringComparison.Ordinal))
                pagePath = pagePath.Replace("..", string.Empty, StringComparison.Ordinal);

            string viewPath = pagePath.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase)
                ? $"~/Views/Pages/{pagePath}"
                : $"~/Views/Pages/{pagePath}.cshtml";

            var result = _viewEngine.GetView(null, viewPath, true);
            if (!result.Success)
                return NotFound();

            return View(viewPath);
        }
    }
}
