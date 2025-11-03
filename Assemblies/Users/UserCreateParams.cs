using System;

namespace Users
{
    public class UserCreateParams
    {
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string ModerationStatus { get; set; } = "ok";
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public DateTime? Birthday { get; set; }
        public string Gender { get; set; } = "none"; // allowed: none, female, male
        public DateTimeOffset? UserCreated { get; set; } // defaults to now (UTC) if null
        public string CurrentLocation { get; set; }
        public string Locale { get; set; }
        public string Language { get; set; }
        public string CountryIso { get; set; } // must be 2-char ISO or null

        public void Normalize()
        {
            if (string.IsNullOrWhiteSpace(UserName))
                throw new ArgumentException("UserName is required", nameof(UserName));
            if (UserId <= 0)
                throw new ArgumentException("UserId must be a positive integer", nameof(UserId));
            if (string.IsNullOrWhiteSpace(ModerationStatus))
                ModerationStatus = "ok";
            if (string.IsNullOrWhiteSpace(Gender))
                Gender = "none";
            if (UserCreated == null)
                UserCreated = DateTimeOffset.UtcNow;
            if (!string.IsNullOrEmpty(CountryIso))
                CountryIso = CountryIso.Trim().ToUpperInvariant();
            if (!string.IsNullOrEmpty(Gender))
                Gender = Gender.Trim().ToLowerInvariant();
        }
    }
}
