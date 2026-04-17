using System;
using System.Security.Cryptography;
using System.Text;

namespace Common
{
    /// <summary>
    /// Password Hashinh utitlity for security
    /// </summary>
    public static class PasswordHasher
    {
        private const int SaltSize = 16; // 128 bits
        private const string HashPrefix = "SHA256:";

        /// <summary>
        /// Hashes a plaintext password using SHA256 with a random salt.
        /// Returns string in format: "SHA256:{salt}:{hash}"
        /// </summary>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty", nameof(password));

            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            byte[] hash = ComputeHash(password, salt);
            return $"{HashPrefix}{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }

        /// <summary>
        /// Verifies a plaintext password against a stored hash.
        /// Supports both hashed passwords (SHA256:salt:hash) and legacy plaintext.
        /// Returns true if password matches.
        /// </summary>
        public static bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHash))
                return false;

            if (storedHash.StartsWith(HashPrefix, StringComparison.Ordinal))
            {
                return VerifyHashedPassword(password, storedHash);
            }

            return string.Equals(password, storedHash, StringComparison.Ordinal);
        }

        /// <summary>
        /// Checks if a password appears to be hashed (starts with SHA256: prefix).
        /// </summary>
        public static bool IsPasswordHashed(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;
            return password.StartsWith(HashPrefix, StringComparison.Ordinal);
        }

        private static bool VerifyHashedPassword(string password, string storedHash)
        {
            try
            {
                string hashData = storedHash.Substring(HashPrefix.Length);
                string[] parts = hashData.Split(':');

                if (parts.Length != 2)
                    return false;

                byte[] salt = Convert.FromBase64String(parts[0]);
                byte[] expectedHash = Convert.FromBase64String(parts[1]);
                byte[] actualHash = ComputeHash(password, salt);
                return CryptographicEquals(expectedHash, actualHash);
            }
            catch
            {
                return false;
            }
        }

        private static byte[] ComputeHash(string password, byte[] salt)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] combined = new byte[passwordBytes.Length + salt.Length];

                Buffer.BlockCopy(passwordBytes, 0, combined, 0, passwordBytes.Length);
                Buffer.BlockCopy(salt, 0, combined, passwordBytes.Length, salt.Length);

                return sha256.ComputeHash(combined);
            }
        }


        private static bool CryptographicEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;

            int result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }
            return result == 0;
        }
    }
}
