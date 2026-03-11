using System.Text.Json;
using Assets;

namespace Assets
{
    public static class GearHelper
    {
        private static readonly Dictionary<string, string> GearTypeDisplayNames = new()
        {
            { "Melee", "Melee" },
            { "PowerUp", "Power up" },
            { "Explosive", "Explosive" },
            { "Navigation", "Navigation" },
            { "MusicalInstrument", "Musical" },
            { "Social", "Social" },
            { "Transport", "Transport" },
            { "Ranged", "Ranged" },
            { "Musical", "Musical" },
            { "Building", "Building" }
        };

        public static GearDisplayInfo GetGearDisplayInfo(bool isAllGenresAllowed, string allowedGearTypes, int? genreId)
        {
            var gearTypes = ParseGearTypes(allowedGearTypes);

            if (genreId == 1 && gearTypes.Count > 0)
            {
                var genreSpecificResult = new GearDisplayInfo
                {
                    DisplayMode = GearDisplayMode.GenreSpecific,
                    MainTooltip = "All Gear Only",
                    IconClass = "icon-partialgears",
                    GearTypes = gearTypes.Select(gt => new GearTypeInfo
                    {
                        Type = gt,
                        DisplayName = GearTypeDisplayNames.TryGetValue(gt, out var displayName) ? displayName : gt,
                        IconClass = $"icon-{gt}"
                    }).ToList()
                };
                return genreSpecificResult;
            }

            if (isAllGenresAllowed)
            {
                var allGenresResult = new GearDisplayInfo
                {
                    DisplayMode = GearDisplayMode.AllGears,
                    MainTooltip = "All Gears Allowed",
                    IconClass = "icon-allgears",
                    GearTypes = GetAllGearTypes()
                };
                return allGenresResult;
            }
            

            if (gearTypes.Count == 0)
            {
                var noGearResult = new GearDisplayInfo
                {
                    DisplayMode = GearDisplayMode.NoGear,
                    MainTooltip = "No Gear Allowed",
                    IconClass = "icon-nogear",
                    GearTypes = new List<GearTypeInfo>()
                };
                return noGearResult;
            }
            

            if (genreId == 1)
            {
                var allGenreResult = new GearDisplayInfo
                {
                    DisplayMode = GearDisplayMode.AllGears,
                    MainTooltip = "All Gears Allowed",
                    IconClass = "icon-allgears",
                    GearTypes = GetAllGearTypes()
                };
                return allGenreResult;
            }
            
            var genreName = GetGenreLabel(genreId.Value);
            var finalResult = new GearDisplayInfo
            {
                DisplayMode = GearDisplayMode.GenreSpecific,
                MainTooltip = $"{genreName} Gear Only",
                IconClass = "icon-partialgears",
                GearTypes = gearTypes.Select(gt => new GearTypeInfo
                {
                    Type = gt,
                    DisplayName = GearTypeDisplayNames.TryGetValue(gt, out var displayName) ? displayName : gt,
                    IconClass = $"icon-{gt}"
                }).ToList()
            };
            return finalResult;
        }

        private static List<string> ParseGearTypes(string allowedGearTypes)
        {
            try
            {
                if (string.IsNullOrEmpty(allowedGearTypes) || allowedGearTypes == "[]")
                    return new List<string>();
                
                var intGearTypes = JsonSerializer.Deserialize<List<int>>(allowedGearTypes);
                if (intGearTypes != null)
                {
                    var idToCategoryMap = new Dictionary<int, string>
                    {
                        { 1, "Melee" },
                        { 2, "PowerUp" },
                        { 3, "Ranged" },
                        { 4, "Navigation" },
                        { 5, "Explosive" },
                        { 6, "Musical" },
                        { 7, "Social" },
                        { 8, "Transport" },
                        { 9, "Building" }
                    };
                    
                    var parsedGearTypes = intGearTypes
                        .Where(id => idToCategoryMap.ContainsKey(id))
                        .Select(id => idToCategoryMap[id])
                        .ToList();
                    
                    return parsedGearTypes;
                }
                
                var stringGearTypes = JsonSerializer.Deserialize<List<string>>(allowedGearTypes);
                if (stringGearTypes != null)
                {
                    return stringGearTypes;
                }
                
                return new List<string>();
            }
            catch (Exception ex)
            {
                return new List<string>();
            }
        }

        private static List<GearTypeInfo> GetAllGearTypes()
        {
            return GearTypeDisplayNames.Select(kvp => new GearTypeInfo
            {
                Type = kvp.Key,
                DisplayName = kvp.Value,
                IconClass = $"icon-{kvp.Key}"
            }).ToList();
        }

        public static string GetGenreLabel(int genreId)
        {
            return genreId switch
            {
                1 => "All",
                2 => "Town & City",
                3 => "Fantasy",
                4 => "Sci-Fi",
                5 => "Ninja",
                6 => "Scary",
                7 => "Pirate",
                8 => "Adventure",
                9 => "Sports",
                10 => "Funny",
                11 => "Wild West",
                12 => "War",
                13 => "Skate Park",
                14 => "Tutorial",
                15 => "RPG",
                16 => "FPS",
                17 => "Fighting",
                18 => "Building",
                19 => "Military",
                20 => "Naval",
                21 => "Medieval",
                _ => "Unknown"
            };
        }
    }

    public class GearDisplayInfo
    {
        public GearDisplayMode DisplayMode { get; set; }
        public string MainTooltip { get; set; } = "";
        public string IconClass { get; set; } = "";
        public List<GearTypeInfo> GearTypes { get; set; } = new();
    }

    public class GearTypeInfo
    {
        public string Type { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string IconClass { get; set; } = "";
    }

    public enum GearDisplayMode
    {
        NoGear,
        AllGears,
        GenreSpecific
    }
}
