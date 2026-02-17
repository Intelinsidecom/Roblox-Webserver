using System;

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
        /// <param name="length">The length of the random string to generate</param>
        /// <returns>A random string containing hexadecimal characters</returns>
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
    }
}
