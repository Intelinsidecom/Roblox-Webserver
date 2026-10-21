using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Assets;
using Avatar;
using Games;
using Users;

namespace Api.Controllers
{
    [ApiController]
    [Route("xbox")]
    public class XboxController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly TokenService _tokenService;

        public XboxController(IConfiguration config, TokenService tokenService)
        {
            _config = config;
            _tokenService = tokenService;
        }

        private string ConnStr() => _config.GetConnectionString("Default") ?? string.Empty;

        private async Task<long> GetAuthenticatedUserIdAsync()
        {
            Request.Headers.TryGetValue("xbl-authz-actor-10", out var xuidHeader);
            var xuid = (xuidHeader.ToString() ?? string.Empty).Trim();
            if (!string.IsNullOrEmpty(xuid))
            {
                var byXuid = await UserQueries.GetUserIdByXuidAsync(ConnStr(), xuid);
                if (byXuid.HasValue)
                    return byXuid.Value;
            }

            var cookie = Request.Cookies[".ROBLOSECURITY"];
            if (!string.IsNullOrEmpty(cookie))
            {
                var byCookie = await _tokenService.ValidateSessionAsync(cookie);
                if (byCookie.HasValue)
                    return byCookie.Value;
            }

            return 0;
        }

        public class TranslateRequest
        {
            public List<string> Ids { get; set; } = new List<string>();
        }

        public class TranslateUserEntry
        {
            public string Id { get; set; } = string.Empty;
            public long UserId { get; set; }
        }

        public class TranslateResponse
        {
            public List<TranslateUserEntry> Users { get; set; } = new List<TranslateUserEntry>();
        }

        [HttpPost("translate")]
        public async Task<IActionResult> Translate([FromBody] TranslateRequest request)
        {
            if (await GetAuthenticatedUserIdAsync() <= 0)
                return StatusCode(401);

            var ids = request?.Ids ?? new List<string>();
            var map = await UserQueries.GetUserIdByXuidsAsync(ConnStr(), ids);

            var response = new TranslateResponse();
            foreach (var id in ids)
            {
                if (!string.IsNullOrEmpty(id) && map.TryGetValue(id, out var userId))
                {
                    response.Users.Add(new TranslateUserEntry { Id = id, UserId = userId });
                }
            }

            return Content(JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = null }), "application/json");
        }

        [HttpGet("catalog/contents")]
        public async Task<IActionResult> CatalogContents([FromQuery] int startIndex = 0, [FromQuery] int count = 20)
        {
            if (await GetAuthenticatedUserIdAsync() <= 0)
                return StatusCode(401);

            var products = await XboxCatalogQueries.GetCatalogPackagesAsync(ConnStr(), startIndex, count);

            var items = new object[products.Count];
            for (int i = 0; i < products.Count; i++)
            {
                var p = products[i];
                items[i] = new
                {
                    AssetId = p.AssetId,
                    AssetTypeId = p.AssetTypeId,
                    Name = p.Name,
                    Description = p.Description ?? string.Empty,
                    PriceInRobux = p.PriceInRobux,
                    IsPublicDomain = p.IsPublicDomain,
                    IsForSale = p.IsForSale,
                    Creator = new { Id = p.CreatorId, Name = p.CreatorName }
                };
            }

            return Ok(new { Products = items });
        }

        [HttpGet("currently-wearing")]
        public async Task<IActionResult> CurrentlyWearing()
        {
            var userId = await GetAuthenticatedUserIdAsync();
            if (userId <= 0)
                return StatusCode(401);

            var repo = new AvatarWornAssetsRepository();
            var assetId = await repo.GetCurrentlyWornPackageAssetIdAsync(ConnStr(), userId);

            return Ok(new { AssetId = assetId > 0 ? assetId : (long?)null });
        }

        [HttpGet("get-login-consecutive-days")]
        public async Task<IActionResult> GetLoginConsecutiveDays()
        {
            var userId = await GetAuthenticatedUserIdAsync();
            if (userId <= 0)
                return StatusCode(401);

            var days = await _tokenService.GetConsecutiveLoginDaysAsync(userId);
            return Ok(new { count = days });
        }
    }
}
