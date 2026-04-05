using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RobloxSetupServer.Data;

[Table("setup")]
public class Setup
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
    
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
    
    [Column("current_windowsplayer_version")]
    public string? CurrentWindowsplayerVersion { get; set; }
    
    [Column("current_rcc_version")]
    public string? CurrentRccVersion { get; set; }
    
    [Column("current_studio_version")]
    public string? CurrentStudioVersion { get; set; }
}

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Setup> Setup { get; set; }
}
