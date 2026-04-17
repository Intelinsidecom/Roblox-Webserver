using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace RobloxWebserver.Controllers
{
    /// <summary>
    /// Handles installer download requests and redirects to the setup service.
    /// </summary>
    [ApiController]
    public class InstallerController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public InstallerController(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Handles /install/setup.ashx requests and redirects to the RobloxPlayerLauncher.exe
        /// </summary>
        [HttpGet("/install/setup.ashx")]
        public IActionResult Setup()
        {
            var setupHost = _configuration["Setup:SetupServicePublicHost"] ?? "setup.freblx.xyz";
            var redirectUrl = $"https://{setupHost}/RobloxPlayerLauncher.exe";
            return Redirect(redirectUrl);
        }

        /// <summary>
        /// Alternative endpoint for downloading the installer directly.
        /// </summary>
        [HttpGet("/install/RobloxPlayerLauncher.exe")]
        public IActionResult DownloadLauncher()
        {
            var setupHost = _configuration["Setup:SetupServicePublicHost"] ?? "setup.freblx.xyz";
            var redirectUrl = $"https://{setupHost}/RobloxPlayerLauncher.exe";
            return Redirect(redirectUrl);
        }
    }
}
