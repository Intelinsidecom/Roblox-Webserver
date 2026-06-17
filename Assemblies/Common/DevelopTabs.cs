using System.Text;

namespace Assemblies.Common;

public static class DevelopSlugHelper
{
    public static string Slug(string name)
    {
        if (string.IsNullOrEmpty(name)) return string.Empty;
        var sb = new StringBuilder(name.Length);
        foreach (var raw in name)
        {
            var ch = char.ToLowerInvariant(raw);
            if ((ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9'))
            {
                sb.Append(ch);
            }
            else
            {
                sb.Append('-');
            }
        }
        return sb.ToString().Trim('-');
    }
}
