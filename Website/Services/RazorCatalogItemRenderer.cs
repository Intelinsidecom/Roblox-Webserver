using System;
using RobloxWebserver.Assemblies.Catalog;

namespace Website.Services
{
    public class RazorCatalogItemRenderer : ICatalogItemRenderer
    {
        private readonly IRazorViewRenderer _viewRenderer;

        public RazorCatalogItemRenderer(IRazorViewRenderer viewRenderer)
        {
            _viewRenderer = viewRenderer;
        }

        public string RenderItem(CatalogItem item, string size)
        {

            var model = new AssetBoxViewModel
            {
                Item = item,
                Size = size
            };

            return _viewRenderer.RenderPartialToStringAsync(
                "/Views/Catalog/AssetBox.cshtml",
                model).GetAwaiter().GetResult();
        }
    }
}
