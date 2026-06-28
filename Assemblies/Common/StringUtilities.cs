using System;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Common
{
    /// <summary>
    /// Utility class for string operations
    /// </summary>
    public static class StringUtilities
    {
        /// <summary>
        /// Generates a random hexadecimal string of specified length
        /// </summary>
        public static string GenerateRandomString(int length)
        {
            const string chars = "abcdef0123456789";
            var random = new Random();
            var result = new char[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = chars[random.Next(chars.Length)];
            }
            return new string(result);
        }

        public static string SanitizeString(string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input)) return "";
            var result = new System.Text.StringBuilder(input.Length);
            foreach (var c in input)
            {
                if (c >= 32 || c == '\t' || c == '\n' || c == '\r')
                    result.Append(c);
            }
            string trimmed = result.ToString().Trim();
            return trimmed.Length > maxLength
                ? trimmed.Substring(0, maxLength)
                : trimmed;
        }
    }
}
