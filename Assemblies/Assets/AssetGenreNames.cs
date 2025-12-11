namespace Assets
{
    /// <summary>
    /// Centralized mapping from assets.genre (1-15) to human-readable labels
    /// for use in catalog and item views.
    /// </summary>
    public static class AssetGenreNames
    {
        /// <summary>
        /// Returns a friendly label for a numeric genre id.
        /// The mapping is based on classic Roblox genre names.
        /// </summary>
        public static string GetGenreLabel(int genreId)
        {
            // Default to "All" when the value is missing or out of range.
            if (genreId <= 0)
                return "All";

            switch (genreId)
            {
                case 1:
                    return "All";
                case 2:
                    return "Adventure";
                case 3:
                    return "Horror";
                case 4:
                    return "Town and City";
                case 5:
                    return "Military";
                case 6:
                    return "Comedy";
                case 7:
                    return "Medieval";
                case 8:
                    return "Sci-Fi";
                case 9:
                    return "Naval";
                case 10:
                    return "Sports";
                case 11:
                    return "Fantasy";
                case 13:
                    return "FPS";
                case 14:
                    return "RPG";
                case 15:
                    return "Wild West";
                default:
                    return "All";
            }
        }
    }
}
