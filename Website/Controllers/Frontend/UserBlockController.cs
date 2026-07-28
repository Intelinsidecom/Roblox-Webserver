using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Users;

namespace Website.Controllers;

public class UserBlockController : Controller
{
    private readonly IConfiguration _configuration;

    public UserBlockController(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    [Authorize]
    [HttpPost("userblock/blockuser")]
    public async Task<IActionResult> BlockUser([FromBody] BlockRequest? request, CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId <= 0)
            return Json(new { success = false, error = "Authentication required" });

        if (request?.blockeeId == null || request.blockeeId <= 0)
            return Json(new { success = false, error = "blockeeId is required" });

        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr))
            return Json(new { success = false, error = "Database not configured" });

        var result = await UserQueries.BlockUserAsync(connStr, currentUserId, request.blockeeId.Value, cancellationToken);
        return Json(result);
    }

    [Authorize]
    [HttpPost("userblock/unblockuser")]
    public async Task<IActionResult> UnblockUser([FromBody] BlockRequest? request, CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId <= 0)
            return Json(new { success = false, error = "Authentication required" });

        if (request?.blockeeId == null || request.blockeeId <= 0)
            return Json(new { success = false, error = "blockeeId is required" });

        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr))
            return Json(new { success = false, error = "Database not configured" });

        var result = await UserQueries.UnblockUserAsync(connStr, currentUserId, request.blockeeId.Value, cancellationToken);
        return Json(result);
    }

    private long GetCurrentUserId()
    {
        var idClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(idClaim))
            return 0;
        if (long.TryParse(idClaim, out var id))
            return id;
        return 0;
    }

    public class BlockRequest
    {
        public long? blockeeId { get; set; }
    }
}
