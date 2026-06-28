using Microsoft.Extensions.Configuration;
using RobloxStudioTest.Services;

namespace RobloxStudioTest.Services;

public static class UserStore
{
    public static List<UserAccount> Users { get; private set; } = new();

    public static void Initialize(IConfiguration configuration)
    {
        var userConfigs = configuration.GetSection("Users").Get<List<UserConfig>>();
        if (userConfigs != null)
        {
            Users = userConfigs.Select(u => new UserAccount
            {
                UserId = u.UserId,
                Username = u.Username,
                PasswordHash = PasswordHasher.HashPassword(u.Password),
                DisplayName = u.DisplayName,
                MembershipStatus = u.MembershipStatus,
                CountryCode = u.CountryCode,
                Birthday = DateTime.TryParse(u.Birthday, out var birthday) ? birthday : null,
                ModerationStatus = u.ModerationStatus
            }).ToList();
        }
    }

    public static UserAccount? FindUser(string username)
    {
        return Users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }

    public static UserAccount? FindUserById(long userId)
    {
        return Users.FirstOrDefault(u => u.UserId == userId);
    }

    public static UserAccount? FindUserBySessionId(string sessionId)
    {
        return null;
    }
}

public class UserConfig
{
    public long UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public short MembershipStatus { get; set; }
    public string CountryCode { get; set; } = string.Empty;
    public string? Birthday { get; set; }
    public string? ModerationStatus { get; set; }
}

public class UserAccount
{
    public long UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public short MembershipStatus { get; set; }
    public string CountryCode { get; set; } = string.Empty;
    public DateTime? Birthday { get; set; }
    public string? ModerationStatus { get; set; }
}
