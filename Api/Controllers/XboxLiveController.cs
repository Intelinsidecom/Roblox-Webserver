using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Common;
using Games;
using Users;

namespace Api.Controllers
{
    [ApiController]
    [Route("xboxlive")]
    public class XboxLiveController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly TokenService _tokenService;
        private readonly string _cookieDomain;

        public XboxLiveController(IConfiguration config, TokenService tokenService)
        {
            _config = config;
            _tokenService = tokenService;
            _cookieDomain = config["Auth:CookieDomain"] ?? ".freblx.com";
        }

        private string ConnStr() => _config.GetConnectionString("Default") ?? string.Empty;

        private string GetXuid()
        {
            Request.Headers.TryGetValue("xbl-authz-actor-10", out var xuid);
            return (xuid.ToString() ?? string.Empty).Trim();
        }

        private async Task<(long UserId, string Xuid)> ResolveIdentityAsync()
        {
            var xuid = GetXuid();
            if (!string.IsNullOrEmpty(xuid))
            {
                var byXuid = await UserQueries.GetUserIdByXuidAsync(ConnStr(), xuid);
                return (byXuid ?? 0, xuid);
            }

            var cookie = Request.Cookies[".ROBLOSECURITY"];
            if (!string.IsNullOrEmpty(cookie))
            {
                var byCookie = await _tokenService.ValidateSessionAsync(cookie);
                if (byCookie.HasValue)
                    return (byCookie.Value, string.Empty);
            }

            return (0, string.Empty);
        }

        private async Task IssueSessionAsync(long userId)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var token = await _tokenService.CreateSessionAsync(userId, ip);

            Response.Cookies.Append(".ROBLOSECURITY", token, new Microsoft.AspNetCore.Http.CookieOptions
            {
                HttpOnly = false,
                Secure = false,
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                Path = "/",
                Domain = _cookieDomain
            });
        }

        private ContentResult ClientError(int statusCode, string code)
        {
            return new ContentResult
            {
                StatusCode = statusCode,
                Content = code,
                ContentType = "text/plain"
            };
        }

        private static bool IsValidUsernameFormat(string username)
        {
            if (string.IsNullOrEmpty(username) || username.Length < 3 || username.Length > 20)
                return false;
            foreach (var c in username)
            {
                if (!char.IsLetterOrDigit(c) && c != '_')
                    return false;
            }
            if (username.StartsWith("_") || username.EndsWith("_") || username.Contains("__"))
                return false;
            return true;
        }

        private static bool IsValidPasswordFormat(string username, string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8 || password.Length > 128)
                return false;
            if (string.Equals(password, username, System.StringComparison.Ordinal))
                return false;
            return true;
        }

        [HttpGet("has-linked-account")]
        public async Task<IActionResult> HasLinkedAccount()
        {
            var (userId, _) = await ResolveIdentityAsync();
            return Ok(new { hasLinkedAccount = userId > 0 });
        }

        [HttpGet("has-set-username-password")]
        public async Task<IActionResult> HasSetUsernamePassword()
        {
            var (userId, _) = await ResolveIdentityAsync();
            if (userId <= 0)
                return Ok(new { hasSetUsernameAndPassword = false });

            var hash = await UserQueries.GetUserPasswordHashAsync(ConnStr(), userId);
            var hasPassword = !string.IsNullOrEmpty(hash);
            return Ok(new { hasSetUsernameAndPassword = hasPassword });
        }

        [HttpPost("connect")]
        public async Task<IActionResult> Connect()
        {
            var (userId, xuid) = await ResolveIdentityAsync();

            if (userId <= 0 && !string.IsNullOrEmpty(xuid))
            {
                var created = await UserQueries.GetOrCreateXboxAccountAsync(ConnStr(), xuid);
                userId = created.UserId;
            }

            if (userId > 0)
                await IssueSessionAsync(userId);

            return Ok(new { status = "ok" });
        }

        [HttpGet("get-roblox-userInfo")]
        public async Task<IActionResult> GetRobloxUserInfo()
        {
            var (userId, _) = await ResolveIdentityAsync();
            if (userId <= 0)
                return StatusCode(401);

            return Ok(new { userId });
        }

        [HttpPost("link-existing-user")]
        public async Task<IActionResult> LinkExistingUser([FromQuery] string username, [FromQuery] string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return ClientError(400, "InvalidCredentials");

            var (identityUserId, xuid) = await ResolveIdentityAsync();
            var connStr = ConnStr();

            var targetUserId = await UserQueries.GetUserIdByUsernameAsync(connStr, username);
            if (!targetUserId.HasValue || targetUserId.Value <= 0)
            {
                return ClientError(400, "InvalidRobloxUser");
            }

            var storedHash = await UserQueries.GetUserPasswordHashAsync(connStr, targetUserId.Value);
            if (string.IsNullOrEmpty(storedHash) || !PasswordHasher.VerifyPassword(password, storedHash))
            {
                return ClientError(400, "InvalidCredentials");
            }

            if (!string.IsNullOrEmpty(xuid))
            {
                var xuidHolder = await UserQueries.GetUserIdByXuidAsync(connStr, xuid);
                if (xuidHolder == targetUserId.Value)
                {
                    await IssueSessionAsync(targetUserId.Value);
                    return Ok();
                }

                if (xuidHolder.HasValue)
                    return ClientError(400, "XboxUserAlreadyLinked");

                if (await UserQueries.IsXboxLinkedAsync(connStr, targetUserId.Value))
                    return ClientError(400, "RobloxUserAlreadyLinked");

                if (!await UserQueries.LinkXuidToUserAsync(connStr, targetUserId.Value, xuid))
                    return ClientError(400, "XboxUserAlreadyLinked");

                Console.WriteLine($"[XboxLive] linked xuid {xuid} to user {targetUserId.Value} ('{username}')");
            }
            else if (identityUserId > 0 && identityUserId != targetUserId.Value)
            {
                return ClientError(400, "RobloxUserAlreadyLinked");
            }

            await IssueSessionAsync(targetUserId.Value);
            return Ok();
        }

        [HttpPost("disconnect")]
        public async Task<IActionResult> Disconnect()
        {
            var (userId, xuid) = await ResolveIdentityAsync();

            if (!string.IsNullOrEmpty(xuid))
            {
                await UserQueries.UnlinkXuidAsync(ConnStr(), xuid);
            }
            else if (userId > 0)
            {
                await UserQueries.UnlinkXboxForUserAsync(ConnStr(), userId);
            }

            return Ok();
        }

        [HttpPost("set-roblox-username-password")]
        public async Task<IActionResult> SetRobloxUsernamePassword([FromQuery] string username, [FromQuery] string password)
        {
            username = (username ?? string.Empty).Trim();
            password = password ?? string.Empty;

            if (!IsValidUsernameFormat(username))
                return ClientError(400, "InvalidCredentials");
            if (!IsValidPasswordFormat(username, password))
                return ClientError(400, "InvalidPassword");

            var connStr = ConnStr();
            var (userId, xuid) = await ResolveIdentityAsync();

            if (userId <= 0 && !string.IsNullOrEmpty(xuid))
            {
                var created = await UserQueries.GetOrCreateXboxAccountAsync(connStr, xuid);
                userId = created.UserId;
            }

            if (userId <= 0)
                return ClientError(401, "UsernamePasswordNotSet");

            var hashed = PasswordHasher.HashPassword(password);
            if (!await UserQueries.SetUserCredentialsAsync(connStr, userId, username, hashed))
            {
                return ClientError(400, "Already Taken");
            }

            await IssueSessionAsync(userId);
            return Ok();
        }
    }
}
