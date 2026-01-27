using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Webserver.Common
{
    /// <summary>
    /// Utility class for character and content validation
    /// </summary>
    public static class CharacterValidationUtility
    {
        /// <summary>
        /// Common patterns that indicate potentially dangerous or inappropriate content
        /// </summary>
        private static readonly string[] DangerousPatterns = {
            "<script",
            "</script>",
            "javascript:",
            "vbscript:",
            "onload=",
            "onerror=",
            "onclick=",
            "onmouseover=",
            "onfocus=",
            "onblur=",
            "onchange=",
            "onsubmit=",
            "eval(",
            "expression(",
            "url(",
            "@import",
            "binding(",
            "<iframe",
            "</iframe>",
            "<object",
            "</object>",
            "<embed",
            "</embed>",
            "<form",
            "</form>",
            "<input",
            "<button",
            "<link",
            "<meta",
            "<style",
            "</style>",
            "data:",
            "file:",
            "ftp:",
            "http:",
            "https:",
            "mailto:",
            "tel:",
            "sms:",
            "callto:",
            "skype:",
            "teams:",
            "discord:",
            "steam:",
            "spotify:",
            "itunes:",
            "appstore:",
            "play.google.com",
            "market://",
            "chrome://",
            "chrome-extension://",
            "moz-extension://",
            "safari://",
            "opera://",
            "edge://",
            "brave://",
            "vivaldi://",
            "about:",
            "blob:",
            "cid:",
            "mid:",
            "news:",
            "nntp:",
            "prospero:",
            "res:",
            "telnet:",
            "urn:",
            "webcal:",
            "wtai:",
            "wais:",
            "z39.50r:",
            "z39.50s:",
            "imap:",
            "pop:",
            "ldap:",
            "gopher:",
            "mms:",
            "rtsp:",
            "rtspu:",
            "shttp:",
            "sip:",
            "sips:",
            "tftp:",
            "btspp:",
            "btl2cap:",
            "btgoep:",
            "tcpobex:",
            "irdaobex:",
            "file://",
            "ftp://",
            "http://",
            "https://",
            "mailto:",
            "news:",
            "nntp:",
            "telnet:",
            "webcal:",
            "callto:",
            "wtai:",
            "sms:",
            "mms:",
            "mmsto:",
            "nokiags:",
            "nokiags+",
            "nokiabrowser:",
            "nokia-messaging:",
            "nokia-omads:",
            "nokia-omadl:",
            "nokia-omadm:",
            "nokia-omapp:",
            "nokia-omds:",
            "nokia-omdr:",
            "nokia-omdt:",
            "nokia-omdl:"
        };

        /// <summary>
        /// Checks if a string contains any potentially dangerous or inappropriate content
        /// </summary>
        /// <param name="input">The string to check</param>
        /// <param name="caseSensitive">Whether the check should be case sensitive (default: false)</param>
        /// <returns>True if dangerous content is found, false otherwise</returns>
        public static bool ContainsDangerousContent(string input, bool caseSensitive = false)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            
            return DangerousPatterns.Any(pattern => input.IndexOf(pattern, comparison) >= 0);
        }

        /// <summary>
        /// Checks if a string contains any illegal characters for user input
        /// </summary>
        /// <param name="input">The string to check</param>
        /// <param name="allowedCharacters">Additional characters to allow beyond basic alphanumeric and common punctuation</param>
        /// <returns>True if illegal characters are found, false otherwise</returns>
        public static bool ContainsIllegalCharacters(string input, string allowedCharacters = "")
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            // Allow letters, numbers, spaces, and common punctuation
            var allowedPattern = $"^[a-zA-Z0-9\\s\\-\\.,!?()\\[\\]{{}}'\":;@#$%&*+=_\\\\/|`~{Regex.Escape(allowedCharacters)}]*$";
            
            return !Regex.IsMatch(input, allowedPattern);
        }

        /// <summary>
        /// Validates a string for both dangerous content and illegal characters
        /// </summary>
        /// <param name="input">The string to validate</param>
        /// <param name="allowedCharacters">Additional characters to allow beyond basic alphanumeric and common punctuation</param>
        /// <param name="caseSensitive">Whether the dangerous content check should be case sensitive</param>
        /// <returns>Validation result with details about any issues found</returns>
        public static ValidationResult ValidateString(string input, string allowedCharacters = "", bool caseSensitive = false)
        {
            var result = new ValidationResult { IsValid = true };

            if (string.IsNullOrWhiteSpace(input))
            {
                result.IsValid = false;
                result.ErrorMessage = "Input cannot be empty";
                return result;
            }

            if (ContainsDangerousContent(input, caseSensitive))
            {
                result.IsValid = false;
                result.ErrorMessage = "Input contains potentially dangerous content";
                result.HasDangerousContent = true;
                return result;
            }

            if (ContainsIllegalCharacters(input, allowedCharacters))
            {
                result.IsValid = false;
                result.ErrorMessage = "Input contains illegal characters";
                result.HasIllegalCharacters = true;
                return result;
            }

            return result;
        }

        /// <summary>
        /// Sanitizes a string by removing or escaping dangerous content
        /// </summary>
        /// <param name="input">The string to sanitize</param>
        /// <returns>Sanitized string</returns>
        public static string SanitizeString(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Remove HTML tags
            var sanitized = Regex.Replace(input, @"<[^>]*>", string.Empty);
            
            // Remove potential script content
            sanitized = Regex.Replace(sanitized, @"javascript:", string.Empty, RegexOptions.IgnoreCase);
            sanitized = Regex.Replace(sanitized, @"vbscript:", string.Empty, RegexOptions.IgnoreCase);
            sanitized = Regex.Replace(sanitized, @"on\w+\s*=", string.Empty, RegexOptions.IgnoreCase);
            
            // Remove dangerous protocols
            sanitized = Regex.Replace(sanitized, @"(data|file|ftp):", string.Empty, RegexOptions.IgnoreCase);
            
            return sanitized.Trim();
        }
    }

    /// <summary>
    /// Result of string validation
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public bool HasDangerousContent { get; set; }
        public bool HasIllegalCharacters { get; set; }
    }
}