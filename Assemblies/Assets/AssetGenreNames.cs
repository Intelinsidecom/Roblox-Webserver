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
            if (genreId <= 1)
                return "All";

            switch (genreId)
            {
                case 1:
                    return "All";
                case 2:
                    return "Town & City";
                case 3:
                    return "Fantasy";
                case 4:
                    return "Sci-Fi";
                case 5:
                    return "Ninja";
                case 6:
                    return "Scary";
                case 7:
                    return "Pirate";
                case 8:
                    return "Adventure";
                case 9:
                    return "Sports";
                case 10:
                    return "Funny";
                case 11:
                    return "Wild West";
                case 12:
                    return "War";
                case 13:
                    return "Skate Park";
                case 14:
                    return "Tutorial";
                case 15:
                    return "RPG";
                case 16:
                    return "FPS";
                case 17:
                    return "Fighting";
                case 18:
                    return "Building";
                case 19:
                    return "Military";
                case 20:
                    return "Naval";
                case 21:
                    return "Medieval";
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
                case "Town & City":
                case "Town and City":
                    return 2;
                case "Fantasy":
                    return 3;
                case "Sci-Fi":
                    return 4;
                case "Ninja":
                    return 5;
                case "Scary":
                case "Horror":
                    return 6;
                case "Pirate":
                    return 7;
                case "Adventure":
                    return 8;
                case "Sports":
                    return 9;
                case "Funny":
                case "Comedy":
                    return 10;
                case "Wild West":
                case "Western":
                    return 11;
                case "War":
                    return 12;
                case "Skate Park":
                    return 13;
                case "Tutorial":
                    return 14;
                case "RPG":
                    return 15;
                case "FPS":
                    return 16;
                case "Fighting":
                    return 17;
                case "Building":
                    return 18;
                case "Military":
                    return 19;
                case "Naval":
                    return 20;
                case "Medieval":
                    return 21;
                default:
                    return 1; // Default to "All" for unknown genres
            }
        }
    }
}
