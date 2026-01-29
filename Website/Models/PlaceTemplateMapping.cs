namespace RobloxWebserver.Models
{
    /// <summary>
    /// Represents a mapping between template IDs and their corresponding .rbxl files
    /// </summary>
    public class PlaceTemplateMapping
    {
        /// <summary>
        /// Dictionary mapping TemplateID (string) to template file name (string)
        /// This matches the structure in appsettings.json under PlaceTemplates
        /// </summary>
        public Dictionary<string, string> Templates { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets the template file path for a given TemplateID
        /// </summary>
        /// <param name="templateId">The TemplateID to look up</param>
        /// <param name="templatesDirectory">Base directory where templates are stored</param>
        /// <returns>Full path to the template file, or null if not found</returns>
        public string? GetTemplatePath(string? templateId, string templatesDirectory)
        {
            if (string.IsNullOrWhiteSpace(templateId) || !Templates.ContainsKey(templateId))
            {
                return null;
            }

            var fileName = Templates[templateId];
            return Path.Combine(templatesDirectory, fileName);
        }

        /// <summary>
        /// Checks if a template exists for the given TemplateID
        /// </summary>
        /// <param name="templateId">The TemplateID to check</param>
        /// <returns>True if template exists, false otherwise</returns>
        public bool HasTemplate(string? templateId)
        {
            return !string.IsNullOrWhiteSpace(templateId) && Templates.ContainsKey(templateId);
        }
    }

    /// <summary>
    /// Configuration wrapper for PlaceTemplates to match appsettings.json structure
    /// </summary>
    public class PlaceTemplatesConfiguration
    {
        public Dictionary<string, string> Templates { get; set; } = new Dictionary<string, string>();
    }
}
