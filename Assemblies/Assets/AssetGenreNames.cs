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
                case 12:
                    return "Building";
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

        /// <summary>
        /// Convert genre string to integer ID based on genre mapping
        /// </summary>
        public static int GetGenreIdFromString(string genre)
        {
            if (string.IsNullOrWhiteSpace(genre))
                return 1; // Default to "All"

            switch (genre)
            {
                case "All":
                    return 1;
                case "Adventure":
                    return 2;
                case "Horror":
                    return 3;
                case "Town and City":
                    return 4;
                case "Military":
                    return 5;
                case "Comedy":
                    return 6;
                case "Medieval":
                    return 7;
                case "Sci-Fi":
                    return 8;
                case "Naval":
                    return 9;
                case "Sports":
                    return 10;
                case "Fantasy":
                    return 11;
                case "Building":
                    return 12;
                case "FPS":
                    return 13;
                case "RPG":
                    return 14;
                case "Western":
                case "Wild West":
                    return 15;
                case "Fighting":
                    return 1; // Map to "All" since no available ID for Fighting
                default:
                    return 1; // Default to "All" for unknown genres
            }
        }
    }
}
