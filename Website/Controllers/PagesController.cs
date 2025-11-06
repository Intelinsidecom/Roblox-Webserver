using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using System;

namespace RobloxWebserver.Controllers
{
    public class PagesController : Controller
    {
        private readonly ICompositeViewEngine _viewEngine;

        public PagesController(ICompositeViewEngine viewEngine)
        {
            _viewEngine = viewEngine;
        }

        public IActionResult Route(string? path)
        {
            // Normalize path: root -> index
            var pagePath = string.IsNullOrWhiteSpace(path) ? "index" : path.Trim();
            pagePath = pagePath.Replace('\\', '/');
            // prevent traversal
            while (pagePath.Contains(".."))
                pagePath = pagePath.Replace("..", string.Empty);

            // If not explicitly a .cshtml, append it
            string viewPath = pagePath.EndsWith(".cshtml", System.StringComparison.OrdinalIgnoreCase)
                ? $"~/Views/Pages/{pagePath}"
                : $"~/Views/Pages/{pagePath}.cshtml";

            var result = _viewEngine.GetView(executingFilePath: null, viewPath: viewPath, isMainPage: true);
            if (!result.Success)
            {
                return NotFound();
            }

            return View(viewPath);
        }
    }
}
