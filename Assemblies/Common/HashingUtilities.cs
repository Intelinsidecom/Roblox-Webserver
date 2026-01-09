using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Common;

public static class HashingUtilities
{
    /// <summary>
    /// Generates SHA256 hash for file bytes
    /// </summary>
    public static string GenerateFileHash(byte[] fileBytes)
    {
        if (fileBytes == null || fileBytes.Length == 0)
            throw new ArgumentException("File bytes cannot be null or empty", nameof(fileBytes));

        using var sha = SHA256.Create();
        var hashBytes = sha.ComputeHash(fileBytes);
        var hashBuilder = new StringBuilder(hashBytes.Length * 2);
        foreach (var b in hashBytes)
            hashBuilder.Append(b.ToString("x2"));
        return hashBuilder.ToString();
    }

    /// <summary>
    /// Generates SHA256 hash for a file at the specified path
    /// </summary>
    public static string GenerateFileHash(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path is required", nameof(filePath));
        
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        var bytes = File.ReadAllBytes(filePath);
        return GenerateFileHash(bytes);
    }

    /// <summary>
    /// Generates SHA256 hash for a stream
    /// </summary>
    public static string GenerateStreamHash(Stream stream)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        using var sha = SHA256.Create();
        var hashBytes = sha.ComputeHash(stream);
        var hashBuilder = new StringBuilder(hashBytes.Length * 2);
        foreach (var b in hashBytes)
            hashBuilder.Append(b.ToString("x2"));
        return hashBuilder.ToString();
    }

    /// <summary>
    /// Generates a random token for security purposes
    /// </summary>
    public static string GenerateSecurityToken(int byteLength = 48)
    {
        var bytes = new byte[byteLength];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        var b64 = Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
        return "_|WARNING:-DO-NOT-SHARE-THIS.--|_" + b64;
    }
}
