using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Webserver.Common
{
    public static class YouTubeUtilities
    {
        /// <summary>
        /// Extracts YouTube video ID from various YouTube URL formats
        /// </summary>
        /// <param name="videoUrl">The YouTube video URL</param>
        /// <returns>YouTube video ID if found, empty string otherwise</returns>
        public static string ExtractVideoId(string videoUrl)
        {
            if (string.IsNullOrWhiteSpace(videoUrl))
                return string.Empty;

            var videoId = "";

            // Handle standard YouTube watch URLs: https://www.youtube.com/watch?v=VIDEO_ID
            if (videoUrl.Contains("youtube.com/watch?v="))
            {
                var uri = new Uri(videoUrl);
                var query = uri.Query;
                if (!string.IsNullOrEmpty(query))
                {
                    // Parse query string manually
                    var queryParams = query.TrimStart('?').Split('&');
                    foreach (var param in queryParams)
                    {
                        var keyValue = param.Split('=');
                        if (keyValue.Length == 2 && keyValue[0] == "v")
                        {
                            videoId = keyValue[1];
                            break;
                        }
                    }
                }
            }
            // Handle short YouTube URLs: https://youtu.be/VIDEO_ID
            else if (videoUrl.Contains("youtu.be/"))
            {
                videoId = videoUrl.Split(new[] { "youtu.be/" }, StringSplitOptions.None)
                    .LastOrDefault()?.Split('?').FirstOrDefault() ?? "";
            }

            return videoId ?? string.Empty;
        }

        /// <summary>
        /// Checks if a URL is a valid YouTube URL
        /// </summary>
        /// <param name="url">The URL to check</param>
        /// <returns>True if it's a YouTube URL, false otherwise</returns>
        public static bool IsYouTubeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            return url.Contains("youtube.com/watch?v=") || url.Contains("youtu.be/");
        }

        /// <summary>
        /// Gets the YouTube embed URL from a video ID
        /// </summary>
        /// <param name="videoId">The YouTube video ID</param>
        /// <returns>YouTube embed URL</returns>
        public static string GetEmbedUrl(string videoId)
        {
            if (string.IsNullOrWhiteSpace(videoId))
                return string.Empty;

            return $"https://www.youtube.com/embed/{videoId}";
        }

        /// <summary>
        /// Gets the YouTube thumbnail URL from a video ID
        /// </summary>
        /// <param name="videoId">The YouTube video ID</param>
        /// <returns>YouTube thumbnail URL</returns>
        public static string GetThumbnailUrl(string videoId)
        {
            if (string.IsNullOrWhiteSpace(videoId))
                return string.Empty;

            return $"https://img.youtube.com/vi/{videoId}/maxresdefault.jpg";
        }

        /// <summary>
        /// Validates if the provided URL is a valid YouTube URL
        /// </summary>
        /// <param name="url">The URL to validate</param>
        /// <returns>True if valid YouTube URL, false otherwise</returns>
        public static bool IsValidYouTubeURL(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            var trimmedUrl = url.Trim();
                    
            // YouTube URL patterns
            var youtubePatterns = new[]
            {
                @"^https?:\/\/(www\.)?youtube\.com\/watch\?v=[a-zA-Z0-9_-]+$",
                @"^https?:\/\/(www\.)?youtube\.com\/embed\/[a-zA-Z0-9_-]+$",
                @"^https?:\/\/youtu\.be\/[a-zA-Z0-9_-]+$",
                @"^https?:\/\/(www\.)?youtube\.com\/v\/[a-zA-Z0-9_-]+$"
            };

            return youtubePatterns.Any(pattern => Regex.IsMatch(trimmedUrl, pattern));
        }
    }
}
