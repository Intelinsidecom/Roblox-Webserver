using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Website.Services
{
    public interface IRazorViewRenderer
    {
        Task<string> RenderPartialToStringAsync<TModel>(string partialName, TModel model);
    }

    public class RazorViewRenderer : IRazorViewRenderer
    {
        private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceScopeFactory _scopeFactory;

        public RazorViewRenderer(
            IRazorViewEngine viewEngine,
            ITempDataProvider tempDataProvider,
            IServiceScopeFactory scopeFactory)
        {
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _scopeFactory = scopeFactory;
        }

        public async Task<string> RenderPartialToStringAsync<TModel>(string partialName, TModel model)
        {
            using var scope = _scopeFactory.CreateScope();
            var scopedProvider = scope.ServiceProvider;

            var httpContext = new DefaultHttpContext { RequestServices = scopedProvider };
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            await using var sw = new StringWriter();
            var viewResult = _viewEngine.FindView(actionContext, partialName, isMainPage: false);

            if (viewResult.View == null)
            {
                viewResult = _viewEngine.GetView(null, partialName, isMainPage: false);
            }

            if (viewResult.View == null)
            {
                throw new ArgumentNullException($"{partialName} does not match any available view");
            }

            var viewDictionary = new ViewDataDictionary<TModel>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = model
            };

            var viewContext = new ViewContext(
                actionContext,
                viewResult.View,
                viewDictionary,
                new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
                sw,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);
            return sw.ToString();
        }
    }
}
