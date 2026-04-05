using System.ComponentModel.DataAnnotations;

namespace Api.Data;

public class Setup
{
    public int Id { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public string? CurrentWindowsplayerVersion { get; set; }
    
    public string? CurrentRccVersion { get; set; }
    
    public string? CurrentStudioVersion { get; set; }
}
