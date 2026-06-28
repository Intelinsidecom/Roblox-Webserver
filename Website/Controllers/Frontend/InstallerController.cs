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

        [HttpGet("/install/setup.ashx")]
        public IActionResult Setup()
        {
            var setupHost = _configuration["Setup:SetupServicePublicHost"] ?? "setup.freblx.xyz";
            var redirectUrl = $"https://{setupHost}/RobloxPlayerLauncher.exe";
            return Redirect(redirectUrl);
        }

        [HttpGet("/install/RobloxPlayerLauncher.exe")]
        public IActionResult DownloadLauncher()
        {
            var setupHost = _configuration["Setup:SetupServicePublicHost"] ?? "setup.freblx.xyz";
            var redirectUrl = $"https://{setupHost}/RobloxPlayerLauncher.exe";
            return Redirect(redirectUrl);
        }

        [HttpGet("/install/setupStudio.ashx")]
        public IActionResult SetupStudio()
        {
            var setupHost = _configuration["Setup:SetupServicePublicHost"] ?? "setup.freblx.xyz";
            var redirectUrl = $"https://{setupHost}/RobloxStudioLauncher.exe";
            return Redirect(redirectUrl);
        }
    }
}
