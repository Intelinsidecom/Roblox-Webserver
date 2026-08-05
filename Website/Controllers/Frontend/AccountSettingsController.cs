using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Common;
using Users;
using Website.Services;

namespace RobloxWebserver.Controllers.Frontend
{
    [ApiController]
    [Authorize]
    public class AccountSettingsController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly IConfiguration _configuration;
        private readonly EmailSender _emailSender;

        public AccountSettingsController(IConfiguration configuration, EmailSender emailSender)
        {
            _configuration = configuration;
            _emailSender = emailSender;
        }

        private (bool ok, long userId) GetUserId()
        {
            var claim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(claim) || !long.TryParse(claim, out var id) || id <= 0)
                return (false, 0);
            return (true, id);
        }

        private string ConnStr() => _configuration.GetConnectionString("Default") ?? string.Empty;

        [HttpGet("account/settings/allowed-notification-destinations")]
        public IActionResult GetAllowedNotificationDestinations()
        {
            var (ok, _) = GetUserId();
            if (!ok) return StatusCode(403);
            return Json(new[] { "DesktopPush", "NotificationStream" });
        }

        [HttpGet("account/settings/app-chat-privacy")]
        public async Task<IActionResult> GetAppChatPrivacy()
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            var v = await UserQueries.GetAppChatPrivacyAsync(ConnStr(), uid);
            return Json(new { AppChatPrivacy = v });
        }

        [HttpPost("account/settings/app-chat-privacy")]
        public async Task<IActionResult> UpdateAppChatPrivacy()
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            var form = await Request.ReadFormAsync();
            await UserQueries.SetAppChatPrivacyAsync(ConnStr(), uid, form["AppChatPrivacy"].ToString());
            return Ok();
        }

        [HttpGet("account/settings/game-chat-privacy")]
        public async Task<IActionResult> GetGameChatPrivacy()
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            var v = await UserQueries.GetGameChatPrivacyAsync(ConnStr(), uid);
            return Json(new { GameChatPrivacy = v });
        }

        [HttpPost("account/settings/game-chat-privacy")]
        public async Task<IActionResult> UpdateGameChatPrivacy()
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            var form = await Request.ReadFormAsync();
            await UserQueries.SetGameChatPrivacyAsync(ConnStr(), uid, form["GameChatPrivacy"].ToString());
            return Ok();
        }

        [HttpGet("account/settings/private-message-privacy")]
        public async Task<IActionResult> GetPrivateMessagePrivacy()
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            var v = await UserQueries.GetPrivateMessagePrivacyAsync(ConnStr(), uid);
            return Json(new { PrivateMessagePrivacy = v });
        }

        [HttpPost("account/settings/private-message-privacy")]
        public async Task<IActionResult> UpdatePrivateMessagePrivacy()
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            var form = await Request.ReadFormAsync();
            await UserQueries.SetPrivateMessagePrivacyAsync(ConnStr(), uid, form["PrivateMessagePrivacy"].ToString());
            return Ok();
        }

        [HttpGet("account/settings/private-server-invite-privacy")]
        public async Task<IActionResult> GetPrivateServerInvitePrivacy()
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            var v = await UserQueries.GetPrivateServerInvitePrivacyAsync(ConnStr(), uid);
            return Json(new { PrivateServerInvitePrivacy = v });
        }

        [HttpPost("account/settings/private-server-invite-privacy")]
        public async Task<IActionResult> UpdatePrivateServerInvitePrivacy()
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            var form = await Request.ReadFormAsync();
            await UserQueries.SetPrivateServerInvitePrivacyAsync(ConnStr(), uid, form["privateServerInvitePrivacy"].ToString());
            return Ok();
        }

        [HttpGet("account/settings/follow-me-privacy")]
        public async Task<IActionResult> GetFollowMePrivacy()
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            var v = await UserQueries.GetFollowMePrivacyAsync(ConnStr(), uid);
            return Json(new { FollowMePrivacy = v });
        }

        [HttpPost("account/settings/follow-me-privacy")]
        public async Task<IActionResult> UpdateFollowMePrivacy()
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            var form = await Request.ReadFormAsync();
            await UserQueries.SetFollowMePrivacyAsync(ConnStr(), uid, form["followMePrivacy"].ToString());
            return Ok();
        }

        [HttpGet("account/settings/trade-privacy")]
        public async Task<IActionResult> GetTradePrivacy()
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            var v = await UserQueries.GetTradePrivacyAsync(ConnStr(), uid);
            return Json(new { TradePrivacy = v });
        }

        [HttpPost("account/settings/trade-privacy")]
        public async Task<IActionResult> UpdateTradePrivacy()
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            var form = await Request.ReadFormAsync();
            await UserQueries.SetTradePrivacyAsync(ConnStr(), uid, form["tradePrivacy"].ToString());
            return Ok();
        }

        [HttpGet("account/settings/trade-value")]
        public async Task<IActionResult> GetTradeValue()
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            var v = await UserQueries.GetTradeValueAsync(ConnStr(), uid);
            return Json(new { TradeValue = v });
        }

        [HttpPost("account/settings/trade-value")]
        public async Task<IActionResult> UpdateTradeValue()
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            var form = await Request.ReadFormAsync();
            if (int.TryParse(form["TradeValue"].ToString(), out var i))
                await UserQueries.SetTradeValueAsync(ConnStr(), uid, (short)i);
            return Ok();
        }

        [HttpGet("account/settings/account-restrictions")]
        public async Task<IActionResult> GetAccountRestrictions()
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            var v = await UserQueries.GetAccountRestrictionsEnabledAsync(ConnStr(), uid);
            return Json(new
            {
                IsEnabled = v,
                isFeatureEnabled = true,
                canToggleContentRestriction = true
            });
        }

        [HttpPost("account/settings/account-restrictions")]
        public async Task<IActionResult> UpdateAccountRestrictions()
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            var form = await Request.ReadFormAsync();
            var enabled = string.Equals(form["isEnabled"].ToString(), "true", StringComparison.OrdinalIgnoreCase);
            await UserQueries.SetAccountRestrictionsEnabledAsync(ConnStr(), uid, enabled);
            return Ok();
        }

        [HttpGet("account/settings/social-networks")]
        public async Task<IActionResult> GetSocialNetworks()
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            var (fb, tw, gp, yt, tc, vis) = await UserQueries.GetSocialNetworksAsync(ConnStr(), uid);
            return Json(new
            {
                FacebookUrl = fb ?? "",
                TwitterUrl = tw ?? "",
                GooglePlusUrl = gp ?? "",
                YouTubeUrl = yt ?? "",
                TwitchUrl = tc ?? "",
                SocialNetworksVisibilityPrivacy = vis
            });
        }

        [HttpPost("account/settings/social-networks")]
        public async Task<IActionResult> UpdateSocialNetworks()
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            var form = await Request.ReadFormAsync();
            var vis = ResolveSocialVisibility(form["SocialNetworksVisibilityPrivacy"].ToString());
            await UserQueries.SetSocialNetworksAsync(
                ConnStr(), uid,
                form["FacebookUrl"].ToString(),
                form["TwitterUrl"].ToString(),
                form["GooglePlusUrl"].ToString(),
                form["YouTubeUrl"].ToString(),
                form["TwitchUrl"].ToString(),
                vis);
            return Ok();
        }

        private static readonly Dictionary<string, short> SocialVisibilityByName =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { "AllUsers",                     6 },
                { "FriendsFollowingAndFollowers", 5 },
                { "FriendsAndFollowing",          4 },
                { "Friends",                      3 },
                { "NoOne",                        0 },
            };

        private static short ResolveSocialVisibility(string? wire)
            => wire != null && SocialVisibilityByName.TryGetValue(wire, out var v) ? v : (short)6;

        private static string? NullIfBlank(string? s)
            => string.IsNullOrWhiteSpace(s) ? null : s;


        [HttpPost("my/account/update-json")]
        public async Task<IActionResult> UpdateAccountInfoJson()
        {
            var (ok, uid) = GetUserId();
            if (!ok)
                return Json(new { success = false, error = "Not authenticated" });

            var connStr = ConnStr();
            if (string.IsNullOrWhiteSpace(connStr))
                return Json(new { success = false, error = "DB not configured" });

            try
            {
                var form = await Request.ReadFormAsync();
                bool.TryParse(form["ReceiveNewsletter"].ToString(), out var receiveNewsletter);
                var visibility = ResolveSocialVisibility(form["SocialNetworksVisibilityPrivacy"].ToString());

                await UserQueries.SetSocialNetworksAsync(
                    connStr, uid,
                    facebook:  NullIfBlank(form["Facebook"].ToString()),
                    twitter:   NullIfBlank(form["Twitter"].ToString()),
                    googleplus: NullIfBlank(form["GooglePlus"].ToString()),
                    youtube:   NullIfBlank(form["YouTube"].ToString()),
                    twitch:    NullIfBlank(form["Twitch"].ToString()),
                    visibility: visibility);

                await UserQueries.SetReceiveNewsletterAsync(connStr, uid, receiveNewsletter);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost("account/changeemail")]
        public async Task<IActionResult> ChangeEmail()
        {
            var (ok, uid) = GetUserId();
            if (!ok) return Json(new { Success = false, Message = "Not authenticated" });

            string email;
            string password;
            try
            {
                var form = await Request.ReadFormAsync();
                email    = (form["emailAddress"].ToString() ?? string.Empty).Trim();
                password = form["password"].ToString() ?? string.Empty;
            }
            catch
            {
                return Json(new { Success = false, Message = "Invalid form submission" });
            }

            if (email.Length == 0 || password.Length == 0)
                return Json(new { Success = false, Message = "Email and password are required" });

            var emailRegex = _configuration["Validation:EmailRegex"]
                ?? @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*@((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$";
            if (!Regex.IsMatch(email, emailRegex))
                return Json(new { Success = false, Message = "Invalid email address" });

            var connStr = ConnStr();
            if (string.IsNullOrWhiteSpace(connStr))
                return Json(new { Success = false, Message = "Service unavailable" });

            string? storedPassword = null;
            try
            {
                await using var conn = new NpgsqlConnection(connStr);
                await conn.OpenAsync();
                await using var cmd = new NpgsqlCommand("select password from users where user_id = @uid", conn);
                cmd.Parameters.AddWithValue("uid", uid);
                var r = await cmd.ExecuteScalarAsync();
                storedPassword = r == null || r is DBNull ? null : r.ToString();
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = $"Database error: {ex.Message}" });
            }

            if (string.IsNullOrEmpty(storedPassword))
                return Json(new { Success = false, Message = "Password not set on this account" });

            if (!PasswordHasher.VerifyPassword(password, storedPassword))
                return Json(new { Success = false, Message = "Incorrect password" });

            try
            {
                await EmailQueries.UpdateEmailAsync(connStr, uid, email);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = $"Failed to update email: {ex.Message}" });
            }

            return Json(new { Success = true, Message = "Email updated. Please check your inbox to verify." });
        }

        [HttpPost("my/account/sendverifyemail")]
        public async Task<IActionResult> SendVerifyEmail()
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);

            string email;
            if (Request.HasFormContentType)
            {
                var form = await Request.ReadFormAsync();
                email = (form["emailAddress"].ToString() ?? string.Empty).Trim();
            }
            else
            {
                email = await EmailQueries.GetEmailAsync(ConnStr(), uid) ?? string.Empty;
            }

            if (string.IsNullOrEmpty(email))
                return Json(new { Success = false, Message = "Email is required." });

            var emailRegex = _configuration["Validation:EmailRegex"]
                ?? @"^[\w!#$%&'*+\-/=?^_`{|}~]+(\.[\w!#$%&'*+\-/=?^_`{|}~]+)*@((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$";
            if (!Regex.IsMatch(email, emailRegex))
                return Json(new { Success = false, Message = "Invalid email address." });

            try
            {
                var connStr = ConnStr();
                var token = GenerateVerificationToken();
                await EmailQueries.UpsertVerificationTokenAsync(connStr, uid, email, token);

                string verificationLink = $"{Request.Scheme}://{Request.Host}/my/account/verifyemail?token={token}";
                // hmm, this should be made to be customizable perhaps more
                string subject = "Please verify your new email address";
                string body = $@"Hello,

Please verify your new email address by clicking the link below:
{verificationLink}

If you didn't request this, you can safely ignore this email.

Thanks,
{Request.Host} Team";

                _emailSender.Send(email, subject, body);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = $"Failed to send verification email: {ex.Message}" });
            }

            return Json(new { Success = true });
        }

        private static string GenerateVerificationToken()
        {
            byte[] tokenBytes = new byte[32];
            RandomNumberGenerator.Fill(tokenBytes);
            return Convert.ToBase64String(tokenBytes).TrimEnd('=');
        }

        [HttpGet("my/account/verifyemail")]
        public async Task<IActionResult> VerifyEmail(string token)
        {
            if (string.IsNullOrEmpty(token))
                return Content("Invalid verification link.");

            var connStr = ConnStr();
            if (string.IsNullOrWhiteSpace(connStr))
                return Content("Service unavailable.");

            var tokenInfo = await EmailQueries.GetVerificationTokenAsync(connStr, token);
            if (tokenInfo == null)
                return Content("Invalid or expired verification link.");

            if (tokenInfo.ExpiresAt < DateTime.UtcNow)
            {
                await EmailQueries.DeleteVerificationTokenAsync(connStr, token);
                return Content("Verification link has expired.");
            }

            await EmailQueries.MarkEmailVerifiedAsync(connStr, tokenInfo.UserId);
            await EmailQueries.DeleteVerificationTokenAsync(connStr, token);

            return Content("Email verified successfully. You can now close this page.");
        }

        [HttpGet("account/settings/description")]
        public async Task<IActionResult> GetDescription()
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            try
            {
                await using var conn = new NpgsqlConnection(ConnStr());
                await conn.OpenAsync();
                await using var cmd = new NpgsqlCommand(
                    "select description_bio from users where user_id = @uid", conn);
                cmd.Parameters.AddWithValue("uid", uid);
                var v = await cmd.ExecuteScalarAsync();
                return Json(new { Description = v?.ToString() ?? "" });
            }
            catch { return Json(new { Description = "" }); }
        }

        [HttpPost("account/settings/description")]
        public async Task<IActionResult> UpdateDescription()
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            var form = await Request.ReadFormAsync();
            var desc = form["Description"].ToString();
            try
            {
                await using var conn = new NpgsqlConnection(ConnStr());
                await conn.OpenAsync();
                await using var cmd = new NpgsqlCommand(
                    "update users set description_bio = @d where user_id = @uid", conn);
                cmd.Parameters.AddWithValue("d", desc ?? "");
                cmd.Parameters.AddWithValue("uid", uid);
                await cmd.ExecuteNonQueryAsync();
            }
            catch { }
            return Ok();
        }

        [HttpGet("account/settings/birthdate")]
        public async Task<IActionResult> GetBirthdate()
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            try
            {
                await using var conn = new NpgsqlConnection(ConnStr());
                await conn.OpenAsync();
                await using var cmd = new NpgsqlCommand(
                    "select birthday from users where user_id = @uid", conn);
                cmd.Parameters.AddWithValue("uid", uid);
                var v = await cmd.ExecuteScalarAsync();
                if (v == null || v == DBNull.Value)
                    return Json(new { BirthDay = 1, BirthMonth = 1, BirthYear = 2000 });
                var bday = (DateTime)v;
                return Json(new { BirthDay = bday.Day, BirthMonth = bday.Month, BirthYear = bday.Year });
            }
            catch { return Json(new { BirthDay = 1, BirthMonth = 1, BirthYear = 2000 }); }
        }

        [HttpPost("account/settings/birthdate")]
        public async Task<IActionResult> UpdateBirthdate()
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            var form = await Request.ReadFormAsync();
            if (int.TryParse(form["BirthDay"].ToString(), out var d) &&
                int.TryParse(form["BirthMonth"].ToString(), out var m) &&
                int.TryParse(form["BirthYear"].ToString(), out var y) &&
                m > 0 && m <= 12 && d > 0 && d <= 31 && y > 1900)
            {
                try
                {
                    var dt = new DateTime(y, m, d);
                    await using var conn = new NpgsqlConnection(ConnStr());
                    await conn.OpenAsync();
                    await using var cmd = new NpgsqlCommand(
                        "update users set birthday = @b where user_id = @uid", conn);
                    cmd.Parameters.AddWithValue("b", dt);
                    cmd.Parameters.AddWithValue("uid", uid);
                    await cmd.ExecuteNonQueryAsync();
                }
                catch { }
            }
            return Ok();
        }

        private static readonly (int id, string name, string iso)[] CountryList = new[]
        {
            (0, "Choose a Country/Region", ""),
            (1, "United States", "US"),
            (2, "Germany", "DE"),
            (3, "Netherlands", "NL"),
            (4, "France", "FR"),
            (5, "Spain", "ES"),
            (6, "Italy", "IT"),
            (7, "Ireland", "IE"),
            (8, "Portugal", "PT"),
            (9, "Canada", "CA"),
            (10, "United Kingdom", "GB"),
            (11, "Australia", "AU"),
            (12, "New Zealand", "NZ"),
            (13, "Brazil", "BR"),
            (14, "Philippines", "PH"),
            (15, "Denmark", "DK"),
            (16, "Sweden", "SE"),
            (17, "United Arab Emirates", "AE"),
            (18, "Poland", "PL"),
            (19, "Malaysia", "MY"),
            (20, "Turkey", "TR"),
            (21, "Norway", "NO"),
            (22, "Romania", "RO"),
            (23, "Thailand", "TH"),
            (24, "Singapore", "SG"),
            (25, "Mexico", "MX"),
            (26, "Saudi Arabia", "SA"),
            (27, "Belgium", "BE"),
            // the most important country btw
            (28, "Lithuania", "LT"),
            (30, "Indonesia", "ID"),
            (31, "Russia", "RU"),
            (32, "Finland", "FI")
        };

        [HttpGet("account/settings/countries")]
        public IActionResult GetCountries()
        {
            var (ok, _) = GetUserId();
            if (!ok) return StatusCode(403);
            return Json(new
            {
                success = true,
                countryList = CountryList.Select(c => new
                {
                    countryId = c.id,
                    countryName = c.name,
                    isoCode = c.iso
                })
            });
        }

        [HttpGet("account/settings/country")]
        public async Task<IActionResult> GetLegacyCountry()
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            try
            {
                await using var conn = new NpgsqlConnection(ConnStr());
                await conn.OpenAsync();
                await using var cmd = new NpgsqlCommand(
                    "select country_iso from users where user_id = @uid", conn);
                cmd.Parameters.AddWithValue("uid", uid);
                var v = await cmd.ExecuteScalarAsync();
                var iso = v?.ToString() ?? "US";
                var match = CountryList.FirstOrDefault(c => string.Equals(c.iso, iso, StringComparison.OrdinalIgnoreCase));
                return Json(new { CountryId = match.id <= 0 ? 1 : match.id });
            }
            catch { return Json(new { CountryId = 1 }); }
        }

        [HttpPost("account/settings/country")]
        public async Task<IActionResult> UpdateLegacyCountry()
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            var form = await Request.ReadFormAsync();
            if (int.TryParse(form["CountryId"].ToString(), out var cid))
            {
                var match = CountryList.FirstOrDefault(c => c.id == cid);
                if (match.iso != null)
                {
                    try
                    {
                        await using var conn = new NpgsqlConnection(ConnStr());
                        await conn.OpenAsync();
                        await using var cmd = new NpgsqlCommand(
                            "update users set country_iso = @c where user_id = @uid", conn);
                        cmd.Parameters.AddWithValue("c", match.iso);
                        cmd.Parameters.AddWithValue("uid", uid);
                        await cmd.ExecuteNonQueryAsync();
                    }
                    catch { }
                }
            }
            return Ok();
        }

        [HttpGet("account/settings/account-country")]
        public async Task<IActionResult> GetCountry()
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            try
            {
                await using var conn = new NpgsqlConnection(ConnStr());
                await conn.OpenAsync();
                await using var cmd = new NpgsqlCommand(
                    "select country_iso from users where user_id = @uid", conn);
                cmd.Parameters.AddWithValue("uid", uid);
                var v = await cmd.ExecuteScalarAsync();
                var iso = v?.ToString() ?? "US";
                var match = CountryList.FirstOrDefault(c => string.Equals(c.iso, iso, StringComparison.OrdinalIgnoreCase));
                return Json(new
                {
                    success = true,
                    countryId = match.id <= 0 ? 1 : match.id,
                    isoCode = match.iso ?? "US"
                });
            }
            catch { return Json(new { success = false, errorMessage = "DB error" }); }
        }

        [HttpPost("account/settings/account-country")]
        public async Task<IActionResult> UpdateCountry()
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            var form = await Request.ReadFormAsync();
            if (int.TryParse(form["countryId"].ToString(), out var cid))
            {
                var match = CountryList.FirstOrDefault(c => c.id == cid);
                if (match.iso != null)
                {
                    try
                    {
                        await using var conn = new NpgsqlConnection(ConnStr());
                        await conn.OpenAsync();
                        await using var cmd = new NpgsqlCommand(
                            "update users set country_iso = @c where user_id = @uid", conn);
                        cmd.Parameters.AddWithValue("c", match.iso);
                        cmd.Parameters.AddWithValue("uid", uid);
                        await cmd.ExecuteNonQueryAsync();
                    }
                    catch { }
                }
                return Json(new { success = true });
            }
            return Json(new { success = false, errorMessage = "Invalid countryId" });
        }

        [HttpGet("account/settings/phone")]
        public async Task<IActionResult> GetPhone()
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            try
            {
                await using var conn = new NpgsqlConnection(ConnStr());
                await conn.OpenAsync();
                await using var cmd = new NpgsqlCommand(
                    "select phone_number, phone_verified from users where user_id = @uid", conn);
                cmd.Parameters.AddWithValue("uid", uid);
                await using var r = await cmd.ExecuteReaderAsync();
                string phone = "";
                bool verified = false;
                if (await r.ReadAsync())
                {
                    phone = r.IsDBNull(0) ? "" : r.GetString(0) ?? "";
                    verified = !r.IsDBNull(1) && r.GetBoolean(1);
                }
                return Json(new
                {
                    CountryCode = "US",
                    Prefix = "1",
                    Phone = phone,
                    IsPhoneVerified = verified,
                    IsPhoneNumberVisible = !string.IsNullOrEmpty(phone),
                    VerificationCodeLength = 6
                });
            }
            catch { return Json(new { IsPhoneNumberVisible = false }); }
        }

        [HttpPost("account/settings/phone")]
        public async Task<IActionResult> UpdatePhone()
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            var form = await Request.ReadFormAsync();
            var phone = form["phone"].ToString();
            try
            {
                await using var conn = new NpgsqlConnection(ConnStr());
                await conn.OpenAsync();
                await using var cmd = new NpgsqlCommand(
                    "update users set phone_number = @p, phone_verified = false where user_id = @uid", conn);
                cmd.Parameters.AddWithValue("p", string.IsNullOrEmpty(phone) ? (object)DBNull.Value : phone);
                cmd.Parameters.AddWithValue("uid", uid);
                await cmd.ExecuteNonQueryAsync();
            }
            catch { }
            return Ok();
        }

        [HttpPost("account/settings/phone/delete")]
        public async Task<IActionResult> DeletePhone()
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            try
            {
                await using var conn = new NpgsqlConnection(ConnStr());
                await conn.OpenAsync();
                await using var cmd = new NpgsqlCommand(
                    "update users set phone_number = null, phone_verified = false where user_id = @uid", conn);
                cmd.Parameters.AddWithValue("uid", uid);
                await cmd.ExecuteNonQueryAsync();
            }
            catch { }
            return Ok();
        }

        [HttpPost("account/settings/phone/resend")]
        public IActionResult ResendPhoneCode() => Ok();

        [HttpPost("account/settings/phone/verify")]
        public IActionResult VerifyPhone() => Ok();

        [HttpGet("account/two-step-enabled")]
        public async Task<IActionResult> GetTwoStep()
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            var enabled = await UserQueries.GetTwoStepEnabledAsync(ConnStr(), uid);
            return Json(new { IsTwoStepEnabled = enabled });
        }

        [HttpPost("account/two-step-enabled")]
        public async Task<IActionResult> UpdateTwoStep()
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            var form = await Request.ReadFormAsync();
            var enabled = string.Equals(form["isEnabled"].ToString(), "true", StringComparison.OrdinalIgnoreCase);
            await UserQueries.SetTwoStepEnabledAsync(ConnStr(), uid, enabled);
            return Ok();
        }
        [HttpGet("v1/account/pin")]
        public async Task<IActionResult> GetPin()
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            var (enabled, unlockedUntil) = await UserQueries.GetAccountPinAsync(ConnStr(), uid);
            return Json(new
            {
                IsEnabled = enabled,
                IsSet = enabled,
                unlockedUntil = unlockedUntil > 0 ? unlockedUntil : (object?)null
            });
        }

        [HttpPost("v1/account/pin")]
        public async Task<IActionResult> CreatePin()
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            var unlockedUntil = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds();
            await UserQueries.SetAccountPinAsync(ConnStr(), uid, true);
            await UserQueries.SetAccountPinUnlockedUntilAsync(ConnStr(), uid, unlockedUntil);
            return Json(new { unlockedUntil });
        }

        [HttpDelete("v1/account/pin")]
        public async Task<IActionResult> DeletePin()
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            await UserQueries.SetAccountPinAsync(ConnStr(), uid, false);
            await UserQueries.SetAccountPinUnlockedUntilAsync(ConnStr(), uid, 0);
            return Ok();
        }

        [HttpPost("v1/account/pin/lock")]
        public async Task<IActionResult> LockPin()
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            await UserQueries.SetAccountPinUnlockedUntilAsync(ConnStr(), uid, 0);
            return Ok();
        }

        [HttpPost("v1/account/pin/unlock")]
        public async Task<IActionResult> UnlockPin()
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            var unlockedUntil = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds();
            await UserQueries.SetAccountPinUnlockedUntilAsync(ConnStr(), uid, unlockedUntil);
            return Json(new { unlockedUntil });
        }

        [HttpGet("v1/social/connected-providers")]
        public IActionResult GetConnectedProviders()
        {
            var (ok, _) = GetUserId();
            if (!ok) return StatusCode(403);
            return Json(new { providers = Array.Empty<object>() });
        }

        [HttpPost("v1/social/{provider}/disconnect")]
        public IActionResult DisconnectSocial(string provider) => Ok();

        [HttpGet("v1/xbox/connection")]
        public async Task<IActionResult> GetXboxConnection()
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            try
            {
                await using var conn = new NpgsqlConnection(ConnStr());
                await conn.OpenAsync();
                await using var cmd = new NpgsqlCommand(
                    "select xbox_user from users where user_id = @uid", conn);
                cmd.Parameters.AddWithValue("uid", uid);
                var v = await cmd.ExecuteScalarAsync();
                var has = v != null && v != DBNull.Value && (bool)v;
                return Json(new { hasConnectedXboxAccount = has });
            }
            catch { return Json(new { hasConnectedXboxAccount = false }); }
        }

        [HttpPost("v1/xbox/disconnect")]
        public async Task<IActionResult> DisconnectXbox()
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            try
            {
                await using var conn = new NpgsqlConnection(ConnStr());
                await conn.OpenAsync();
                await using var cmd = new NpgsqlCommand(
                    "update users set xbox_user = false where user_id = @uid", conn);
                cmd.Parameters.AddWithValue("uid", uid);
                await cmd.ExecuteNonQueryAsync();
                return Json(new { success = true });
            }
            catch { return Json(new { success = false }); }
        }

        [HttpGet("v2/notifications/get-settings")]
        public async Task<IActionResult> GetNotificationSettings()
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            var optOuts = await UserQueries.GetOptedOutDestinationsAsync(ConnStr(), uid);
            var bands = await UserQueries.GetNotificationBandsAsync(ConnStr(), uid);

            return Json(new
            {
                notificationBandSettings = bands.Select(b => new
                {
                    receiverDestinationType = b.ReceiverDestinationType,
                    notificationSourceType = b.NotificationSourceType,
                    isEnabled = b.IsEnabled,
                    isOverridable = true,
                    isSetByReceiver = false
                }).ToArray(),
                optedOutNotificationSourceTypes = Array.Empty<string>(),
                optedOutReceiverDestinationTypes = optOuts.ToArray()
            });
        }

        [HttpPost("v2/notifications/update-notification-settings")]
        public async Task<IActionResult> UpdateNotificationSettings([FromBody] Newtonsoft.Json.Linq.JObject body)
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            try
            {
                var bands = new List<NotificationBand>();
                var bandsToken = body["updatedSettings"];
                if (bandsToken is Newtonsoft.Json.Linq.JArray arr)
                {
                    foreach (var b in arr)
                    {
                        bands.Add(new NotificationBand
                        {
                            ReceiverDestinationType = b["receiverDestinationType"]?.ToString() ?? "NotificationStream",
                            NotificationSourceType   = b["notificationSourceType"]?.ToString() ?? "Test",
                            IsEnabled                = string.Equals(b["isEnabled"]?.ToString(), "True", StringComparison.OrdinalIgnoreCase)
                        });
                    }
                }
                await UserQueries.SetNotificationBandsAsync(ConnStr(), uid, bands);
            }
            catch { }
            return Ok();
        }

        [HttpPost("v2/notifications/receiver-destination-types/allow")]
        public async Task<IActionResult> AllowDestinationType([FromBody] Newtonsoft.Json.Linq.JObject body)
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            try
            {
                var dt = body["destinationType"]?.ToString() ?? "";
                if (!string.IsNullOrWhiteSpace(dt))
                    await UserQueries.AllowDestinationAsync(ConnStr(), uid, dt);
            }
            catch { }
            return Ok();
        }

        [HttpPost("v2/notifications/receiver-destination-types/opt-out")]
        public async Task<IActionResult> OptOutDestinationType([FromBody] Newtonsoft.Json.Linq.JObject body)
        {
            var (ok, uid) = GetUserId();
            if (!ok) return StatusCode(403);
            try
            {
                var dt = body["destinationType"]?.ToString() ?? "";
                if (!string.IsNullOrWhiteSpace(dt))
                    await UserQueries.OptOutDestinationAsync(ConnStr(), uid, dt);
            }
            catch { }
            return Ok();
        }

        
        [HttpGet("my/account-info")]
        public async Task<IActionResult> AccountInfo()
        {
            var (isValid, userId) = AuthenticationHelper.GetCurrentUserId(User);
            if (!isValid)
                return StatusCode(403);

            var connStr = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
                return StatusCode(500);

            var info = await UserQueries.GetAccountInfoAsync(connStr, userId);
            if (info == null)
                return StatusCode(403);

            return Json(new
            {
                username = info.UserName,
                email = info.Email ?? string.Empty,
                emailVerified = info.EmailVerified,
                hasPasswordSet = info.HasPassword,
                phoneNumber = info.PhoneNumber ?? string.Empty,
                phoneVerified = info.PhoneVerified,
                twoStepEnabled = info.TwoStepEnabled
            });
        }

        [HttpGet("account/settings/gender")]
        [HttpPost("account/settings/gender")]
        public async Task<IActionResult> AccountGender()
        {
            var (isValid, userId) = AuthenticationHelper.GetCurrentUserId(User);
            if (!isValid)
                return StatusCode(403);

            var connStr = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connStr))
                return StatusCode(500);

            if (Request.Method == "POST")
            {
                var form = await Request.ReadFormAsync();
                var genderStr = form["Gender"].ToString();
                string dbGender;
                if (genderStr == "2") dbGender = "male";
                else if (genderStr == "3") dbGender = "female";
                else if (genderStr == "1" || string.IsNullOrEmpty(genderStr)) dbGender = "none";
                else dbGender = "none";

                await using (var conn = new NpgsqlConnection(connStr))
                {
                    await conn.OpenAsync();
                    await using var cmd = new NpgsqlCommand(
                        "update users set gender = cast(@g as gender_enum) where user_id = @uid", conn);
                    cmd.Parameters.AddWithValue("g", dbGender);
                    cmd.Parameters.AddWithValue("uid", userId);
                    await cmd.ExecuteNonQueryAsync();
                }

                return Json(new { success = true });
            }

            string currentDbGender = "none";
            try
            {
                await using var conn = new NpgsqlConnection(connStr);
                await conn.OpenAsync();
                await using var cmd = new NpgsqlCommand(
                    "select gender::text from users where user_id = @uid", conn);
                cmd.Parameters.AddWithValue("uid", userId);
                var result = await cmd.ExecuteScalarAsync();
                if (result != null && result != DBNull.Value)
                    currentDbGender = result.ToString() ?? "none";
            }
            catch { }

            string clientGender = "1";
            if (string.Equals(currentDbGender, "male", StringComparison.OrdinalIgnoreCase)) clientGender = "2";
            else if (string.Equals(currentDbGender, "female", StringComparison.OrdinalIgnoreCase)) clientGender = "3";

            return Json(new { Gender = clientGender });
        }

        private string ResolveApiDomain()
        {
            return $"{Request.Scheme}://{Request.Host}";
        }

        private static string SocialVisibilityToWireName(short vis) => vis switch
        {
            5 => "FriendsFollowingAndFollowers",
            4 => "FriendsAndFollowing",
            3 => "Friends",
            0 => "NoOne",
            _ => "AllUsers",
        };

    }
}



