using System;
using System.Security.Cryptography;
using System.Text;
using Sodium;

namespace Common
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const string HashPrefix = "SHA256:";

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

        public static bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHash))
                return false;

            if (storedHash.StartsWith(HashPrefix, StringComparison.Ordinal))
                return VerifyHashedPassword(password, storedHash);

            if (storedHash.StartsWith("$argon", StringComparison.Ordinal))
                return VerifyArgon2Password(password, storedHash);

            return string.Equals(password, storedHash, StringComparison.Ordinal);
        }

        public static bool IsPasswordHashed(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;
            return password.StartsWith(HashPrefix, StringComparison.Ordinal) ||
                   password.StartsWith("$argon", StringComparison.Ordinal);
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

        private static bool VerifyArgon2Password(string password, string storedHash)
        {
            try
            {
                return PasswordHash.ArgonHashStringVerify(storedHash, password);
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
