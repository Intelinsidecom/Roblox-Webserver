using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace Common;

public static class CDNUtilities
{
    /// <summary>
    /// Gets the solution directory path by walking up from the current directory
    /// until a directory containing CDN/Assets (production) or Webserver.sln (development) is found.
    /// </summary>
    public static string GetSolutionDirectory()
    {
        var current = Directory.GetCurrentDirectory();
        var directory = new DirectoryInfo(current);
        while (directory != null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "CDN", "Assets")) ||
                File.Exists(Path.Combine(directory.FullName, "Webserver.sln")))
                return directory.FullName;
            directory = directory.Parent;
        }
        return current;
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
    /// Gets the CDN place thumbnails path
    /// </summary>
    public static string GetCDNPlaceThumbnailsPath()
    {
        return GetCDNAssetsPath("place-thumbnails");
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
    /// Safely copies a file by using streams with proper sharing to avoid file access issues
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
            var destinationDir = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrWhiteSpace(destinationDir))
                Directory.CreateDirectory(destinationDir);
            
            using var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
            var buffer = new byte[8192];
            int bytesRead;
            while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                destinationStream.Write(buffer, 0, bytesRead);
            }
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error copying file from {sourcePath} to {destinationPath}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Saves bytes to a file in the CDN directory using streams to avoid locking
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
        using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        fileStream.Write(fileBytes, 0, fileBytes.Length);
    }

    /// <summary>
    /// Gets place thumbnail bytes from the CDN directory using streams to avoid locking
    /// </summary>
    public static byte[]? GetPlaceThumbnailFromCDN(string hash)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(hash))
                return null;

            var cdnPath = GetCDNPlaceThumbnailsPath();
            var imagePath = Path.Combine(cdnPath, $"{hash}.png");
            
            if (!File.Exists(imagePath))
                return null;

            using var fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var memoryStream = new MemoryStream();
            var buffer = new byte[8192];
            int bytesRead;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                memoryStream.Write(buffer, 0, bytesRead);
            }
            
            return memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting place thumbnail: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Resizes a place thumbnail from the CDN directory
    /// </summary>
    public static byte[]? ResizePlaceThumbnailFromCDN(string hash, int width, int height, string? format = "jpeg")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(hash))
                return null;

            var cdnPath = GetCDNPlaceThumbnailsPath();
            var imagePath = Path.Combine(cdnPath, $"{hash}.png");
            
            if (!File.Exists(imagePath))
                return null;
            using var fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
            using var originalImage = Image.FromStream(fileStream);
            var resizedImage = new Bitmap(width, height);
            using var graphics = Graphics.FromImage(resizedImage);
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            graphics.DrawImage(originalImage, 0, 0, width, height);
            var imageFormat = GetImageFormat(format);
            using var outputStream = new MemoryStream();
            resizedImage.Save(outputStream, imageFormat);
            
            return outputStream.ToArray();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error resizing place thumbnail: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Extracts hash from a CDN URL
    /// </summary>
    public static string? ExtractHashFromUrl(string url)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            var uri = new Uri(url);
            var pathWithoutDomain = uri.AbsolutePath;
            var fileName = pathWithoutDomain.Split('/').Last();

            if (fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                return fileName.Substring(0, fileName.Length - 4);
            }
            return fileName;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the content type for the specified image format
    /// </summary>
    public static string GetContentType(string? format)
    {
        return format?.ToLowerInvariant() switch
        {
            "png" => "image/png",
            "gif" => "image/gif",
            "bmp" => "image/bmp",
            _ => "image/jpeg"
        };
    }

    private static ImageFormat GetImageFormat(string? format)
    {
        return format?.ToLowerInvariant() switch
        {
            "png" => ImageFormat.Png,
            "gif" => ImageFormat.Gif,
            "bmp" => ImageFormat.Bmp,
            _ => ImageFormat.Jpeg
        };
    }
}
