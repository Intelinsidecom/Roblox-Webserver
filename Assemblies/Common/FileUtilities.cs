using System;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Common;

public static class FileUtilities
{
    /// <summary>
    /// Safely reads all bytes from a file with error handling
    /// </summary>
    public static byte[] ReadAllBytesSafe(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path is required", nameof(filePath));
        
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        try
        {
            return File.ReadAllBytes(filePath);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to read file: {filePath}", ex);
        }
    }

    /// <summary>
    /// Safely writes all bytes to a file with error handling
    /// </summary>
    public static void WriteAllBytesSafe(string filePath, byte[] bytes)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path is required", nameof(filePath));
        
        if (bytes == null || bytes.Length == 0)
            throw new ArgumentException("File bytes cannot be null or empty", nameof(bytes));

        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllBytes(filePath, bytes);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to write file: {filePath}", ex);
        }
    }

    /// <summary>
    /// Asynchronously reads all bytes from a file
    /// </summary>
    public static async Task<byte[]> ReadAllBytesAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path is required", nameof(filePath));
        
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        try
        {
            using var fileStream = new FileStream(
                filePath, 
                FileMode.Open, 
                FileAccess.Read, 
                FileShare.Read, 
                bufferSize: 4096, 
                useAsync: true);

            var bytes = new byte[fileStream.Length];
            await fileStream.ReadAsync(bytes, 0, bytes.Length, cancellationToken);
            return bytes;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to read file: {filePath}", ex);
        }
    }

    /// <summary>
    /// Asynchronously writes all bytes to a file
    /// </summary>
    public static async Task WriteAllBytesAsync(string filePath, byte[] bytes, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path is required", nameof(filePath));
        
        if (bytes == null || bytes.Length == 0)
            throw new ArgumentException("File bytes cannot be null or empty", nameof(bytes));

        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            using var fileStream = new FileStream(
                filePath, 
                FileMode.Create, 
                FileAccess.Write, 
                FileShare.None, 
                bufferSize: 4096, 
                useAsync: true);

            await fileStream.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to write file: {filePath}", ex);
        }
    }

    /// <summary>
    /// Checks if a file exists and is accessible
    /// </summary>
    public static bool FileExistsAndAccessible(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        try
        {
            return File.Exists(filePath);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Ensures a directory exists, creating it if necessary
    /// </summary>
    public static void EnsureDirectoryExists(string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
            throw new ArgumentException("Directory path is required", nameof(directoryPath));

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }

    /// <summary>
    /// Gets the size of a file in bytes
    /// </summary>
    public static long GetFileSize(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path is required", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        var fileInfo = new FileInfo(filePath);
        return fileInfo.Length;
    }

    /// <summary>
    /// Validates if a file is a valid image file by attempting to load it as an image
    /// </summary>
    /// <param name="filePath">Path to the file to validate</param>
    /// <returns>True if the file is a valid image, false otherwise</returns>
    public static bool IsValidImageFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        if (!File.Exists(filePath))
            return false;

        try
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            string[] validExtensions = { ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tiff", ".ico" };
            
            if (Array.IndexOf(validExtensions, extension) == -1)
                return false;

            using (var image = Image.FromFile(filePath))
            {
                return image != null;
            }
        }
        catch (OutOfMemoryException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (FileNotFoundException)
        {
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the image format from a valid image file
    /// </summary>
    /// <param name="filePath">Path to the image file</param>
    /// <returns>The image format (PNG, JPEG, BMP, etc.) or "Unknown" if not a valid image</returns>
    public static string GetImageFormat(string filePath)
    {
        if (!IsValidImageFile(filePath))
            return "Unknown";

        try
        {
            using (var image = Image.FromFile(filePath))
            {
                if (image.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Png))
                    return "PNG";
                else if (image.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Jpeg))
                    return "JPEG";
                else if (image.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Bmp))
                    return "BMP";
                else if (image.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Gif))
                    return "GIF";
                else if (image.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Tiff))
                    return "TIFF";
                else if (image.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Icon))
                    return "ICO";
                else
                    return "Unknown";
            }
        }
        catch
        {
            return "Unknown";
        }
    }
}
