namespace Assets
{
    /// <summary>
    /// Centralized mapping from asset_type_id to human-readable labels
    /// for use in views (e.g. "Configure {type}").
    /// </summary>
    public static class AssetTypeNames
    {
        public static string GetConfigureLabel(int assetTypeId)
        {
            // Extend this switch as you support more asset types.
            switch (assetTypeId)
            {
                case 2:
                    return "T-Shirt";
                default:
                    return string.Empty;
            }
        }
    }
}
