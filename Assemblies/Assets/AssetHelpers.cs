using System;

namespace Assets
{
    public static class AssetHelpers
    {
        public static string GetFriendlyUpdatedText(DateTimeOffset lastUpdated, DateTimeOffset? now = null)
        {
            var reference = now ?? DateTimeOffset.UtcNow;
            if (lastUpdated > reference)
                lastUpdated = reference;

            var diff = reference - lastUpdated;

            if (diff.TotalMinutes < 2)
                return "Now";

            if (diff.TotalDays < 2)
                return "Recently";

            if (diff.TotalDays < 60)
            {
                var days = (int)Math.Round(diff.TotalDays);
                return days == 1 ? "1 day ago" : $"{days} days ago";
            }

            var months = (int)Math.Round(diff.TotalDays / 30.0);
            if (months < 24)
            {
                return months == 1 ? "1 month ago" : $"{months} months ago";
            }

            var years = (int)Math.Round(diff.TotalDays / 365.0);
            if (years < 1) years = 1;
            return years == 1 ? "1 year ago" : $"{years} years ago";
        }

        public static bool IsNew(DateTimeOffset createdAt, DateTimeOffset? now = null)
        {
            var reference = now ?? DateTimeOffset.UtcNow;
            if (createdAt > reference)
                createdAt = reference;

            var diff = reference - createdAt;

            return diff.TotalDays < 1;
        }

        public static long ResolveLinkedAssetId(string connectionString, long assetId)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (assetId <= 0)
                throw new ArgumentOutOfRangeException(nameof(assetId));

            var repo = new AssetMetadataRepository();
            var record = repo.GetAssetByIdAsync(connectionString, assetId).GetAwaiter().GetResult();
            if (record == null)
                return assetId;

            if (record.AssetImage && record.AssetLink.HasValue && record.AssetLink.Value > 0)
                return record.AssetLink.Value;

            return assetId;
        }
    }
}
