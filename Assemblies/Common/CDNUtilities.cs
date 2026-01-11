using System;
using System.IO;

namespace Common;

public static class CDNUtilities
{
    /// <summary>
    /// Gets the solution directory path
    /// </summary>
    public static string GetSolutionDirectory()
    {
        return Directory.GetParent(Directory.GetCurrentDirectory())?.FullName ?? Directory.GetCurrentDirectory();
    }

    /// <summary>
    /// Gets the CDN assets path for a specific subdirectory
    /// </summary>
    public static string GetCDNAssetsPath(string subDirectory = "")
    {
        var solutionDir = GetSolutionDirectory();
        var cdnPath = Path.Combine(solutionDir, "CDN", "Assets");
        
        if (!string.IsNullOrWhiteSpace(subDirectory))
            cdnPath = Path.Combine(cdnPath, subDirectory);
            
        Directory.CreateDirectory(cdnPath);
        return cdnPath;
    }

    /// <summary>
    /// Gets the CDN thumbnails path
    /// </summary>
    public static string GetCDNThumbnailsPath()
    {
        return GetCDNAssetsPath("thumbnails");
    }

    /// <summary>
    /// Gets the CDN place icons path
    /// </summary>
    public static string GetCDNPlaceIconsPath()
    {
        return GetCDNAssetsPath("place-icons");
    }

    /// <summary>
    /// Generates a CDN URL for a file
    /// </summary>
    public static string GenerateCDNUrl(string baseUrl, string subDirectory, string fileName)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new ArgumentException("Base URL is required", nameof(baseUrl));
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name is required", nameof(fileName));

        var url = baseUrl.TrimEnd('/');
        
        if (!string.IsNullOrWhiteSpace(subDirectory))
            url += "/" + subDirectory.Trim('/');
            
        return url + "/" + fileName.TrimStart('/');
    }

    /// <summary>
    /// Generates a CDN URL for a thumbnail file
    /// </summary>
    public static string GenerateThumbnailUrl(string baseUrl, string fileName)
    {
        return GenerateCDNUrl(baseUrl, "thumbnails", fileName);
    }

    /// <summary>
    /// Generates a CDN URL for a place icon file
    /// </summary>
    public static string GeneratePlaceIconUrl(string baseUrl, string fileName)
    {
        return GenerateCDNUrl(baseUrl, "place-icons", fileName);
    }

    /// <summary>
    /// Generates a CDN URL for a place thumbnail file
    /// </summary>
    public static string GeneratePlaceThumbnailUrl(string baseUrl, string fileName)
    {
        return GenerateCDNUrl(baseUrl, "place-thumbnails", fileName);
    }

    /// <summary>
    /// Safely copies a file by reading bytes first to avoid file access issues
    /// </summary>
    public static bool SafeFileCopy(string sourcePath, string destinationPath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
            throw new ArgumentException("Source path is required", nameof(sourcePath));
        if (string.IsNullOrWhiteSpace(destinationPath))
            throw new ArgumentException("Destination path is required", nameof(destinationPath));

        if (!File.Exists(sourcePath))
            return false;

        try
        {
            // Read file bytes first to avoid file access issues
            var fileBytes = File.ReadAllBytes(sourcePath);
            
            // Ensure destination directory exists
            var destinationDir = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrWhiteSpace(destinationDir))
                Directory.CreateDirectory(destinationDir);
                
            File.WriteAllBytes(destinationPath, fileBytes);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error copying file from {sourcePath} to {destinationPath}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Saves bytes to a file in the CDN directory
    /// </summary>
    public static void SaveToCDN(string subDirectory, string fileName, byte[] fileBytes)
    {
        if (string.IsNullOrWhiteSpace(subDirectory))
            throw new ArgumentException("Subdirectory is required", nameof(subDirectory));
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name is required", nameof(fileName));
        if (fileBytes == null || fileBytes.Length == 0)
            throw new ArgumentException("File bytes cannot be null or empty", nameof(fileBytes));

        var cdnPath = GetCDNAssetsPath(subDirectory);
        var fullPath = Path.Combine(cdnPath, fileName);
        File.WriteAllBytes(fullPath, fileBytes);
    }
}
