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

        [HttpGet]
        [HttpPost]
        public IActionResult Route(string? path)
        {
            var pagePath = string.IsNullOrWhiteSpace(path) ? "index" : path.Trim().Trim('/');
            pagePath = pagePath.Replace('\\', '/');
            while (pagePath.Contains(".."))
                pagePath = pagePath.Replace("..", string.Empty);

            if (pagePath.StartsWith("ide/", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(pagePath, "ide/welcome", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(pagePath, "ide/login", StringComparison.OrdinalIgnoreCase) &&
                User?.Identity?.IsAuthenticated != true)
            {
                return Redirect("/ide/login?returnUrl=" + Uri.EscapeDataString("/" + pagePath));
            }

            string viewPath = pagePath.EndsWith(".cshtml", System.StringComparison.OrdinalIgnoreCase)
                ? $"~/Views/Pages/{pagePath}"
                : $"~/Views/Pages/{pagePath}.cshtml";

            var result = _viewEngine.GetView(executingFilePath: null, viewPath: viewPath, isMainPage: true);
            if (!result.Success)
            {
                var accept = Request.Headers.Accept.ToString() ?? "";
                if (!accept.Contains("text/html", StringComparison.OrdinalIgnoreCase))
                    return NotFound();

                return Redirect("/404");
            }

            return View(viewPath);
        }
    }
}
