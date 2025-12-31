using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using System;

namespace RobloxWebserver.Controllers
{
    public class MyPagesController : Controller
    {
        private readonly ICompositeViewEngine _viewEngine;

        public MyPagesController(ICompositeViewEngine viewEngine)
        {
            _viewEngine = viewEngine;
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
