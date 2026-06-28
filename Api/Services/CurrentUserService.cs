using System.Security.Claims;
using Games;
using Microsoft.AspNetCore.Http;

namespace Api.Services;

public class CurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly TokenService _tokenService;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, TokenService tokenService)
    {
        _httpContextAccessor = httpContextAccessor;
        _tokenService = tokenService;
    }

    public async Task<long> GetUserIdAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return 0;

        var claimVal = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        if (claimVal != null && !string.IsNullOrEmpty(claimVal.Value) && long.TryParse(claimVal.Value, out var userId))
            return userId;

        var cookie = httpContext.Request.Cookies[".ROBLOSECURITY"];
        if (!string.IsNullOrWhiteSpace(cookie))
        {
            var id = await _tokenService.ValidateSessionAsync(cookie);
            if (id.HasValue && id.Value > 0)
                return id.Value;
        }

        cookie = httpContext.Request.Cookies[".PUPPYSECURITY"];
        if (!string.IsNullOrWhiteSpace(cookie))
        {
            var id = await _tokenService.ValidateSessionAsync(cookie);
            if (id.HasValue && id.Value > 0)
                return id.Value;
        }

        return 0;
    }
}