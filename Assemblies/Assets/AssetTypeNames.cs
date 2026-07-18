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
                case 3:
                    return "Audio";
                case 10:
                    return "Model";
                case 4:
                    return "Mesh";
                case 9:
                    return "Game";
                case 11:
                    return "Shirt";
                case 12:
                    return "Pants";
                case 13:
                    return "Decal";
                case 38:
                    return "Plugin";
                default:
                    return string.Empty;
            }
        }

        public static string GetTypeName(int assetTypeId)
        {
            // Extend this switch as you add support for more asset types.
            switch (assetTypeId)
            {
                case 2:
                    return "T-Shirt";
                case 3:
                    return "Audio";
                case 10:
                    return "Model";
                case 4:
                    return "Mesh";
                case 8:
                    return "Hat";
                case 9:
                    return "Game";
                case 11:
                    return "Shirt";
                case 12:
                    return "Pants";
                case 13:
                    return "Decal";
                case 16:
                    return "Face";
                case 17:
                    return "Head";
                case 21:
                    return "Face Accessory";
                case 24:
                case 27:
                    return "Gear";
                case 32:
                    return "Package";
                case 38:
                    return "Plugin";
                case 41:
                    return "Hair";
                case 42:
                case 43:
                    return "Hat";
                default:
                    return "Asset";
            }
        }

        public static bool Is3DSupported(int assetTypeId)
        {
            switch (assetTypeId)
            {
                case 2:    // T-Shirt
                case 4:    // Mesh
                case 8:    // Hat
                case 10:   // Model
                case 11:   // Shirt
                case 12:   // Pants
                case 16:   // Face
                case 17:   // Head
                case 21:   // Face Accessory
                case 24:   // Gear
                case 27:   // Gear
                case 32:   // Package
                case 41:   // Hair
                case 42:   // Hat (Accessory)
                case 43:   // Hat
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsWearableAssetType(int assetTypeId)
        {
            switch (assetTypeId)
            {
                case 2:    // T-Shirt
                case 8:    // Hat
                case 11:   // Shirt
                case 12:   // Pants
                case 16:   // Face
                case 17:   // Head
                case 21:   // Face Accessory
                case 24:   // Gear
                case 27:   // Gear
                case 32:   // Package
                case 41:   // Hair
                case 42:   // Hat (Accessory)
                case 43:   // Hat
                    return true;
                default:
                    return false;
            }
        }
    }
}
