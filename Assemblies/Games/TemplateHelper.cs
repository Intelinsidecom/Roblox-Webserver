using Common;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Games;

/// <summary>
/// Helper methods for processing and saving template files as hashed assets
/// </summary>
public static class TemplateHelper
{
    /// <summary>
    /// Processes a template file and saves it to the CDN assets directory with hash-based naming
    /// </summary>
    /// <param name="templateFilePath">Full path to the template file</param>
    /// <param name="assetsRoot">Root directory where assets are stored</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Content hash of the template file, or null if processing failed</returns>
    public static string? ProcessAndSaveTemplateAsync(string templateFilePath, string assetsRoot, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(templateFilePath))
            throw new ArgumentException("Template file path is required", nameof(templateFilePath));
        if (string.IsNullOrWhiteSpace(assetsRoot))
            throw new ArgumentException("Assets root directory is required", nameof(assetsRoot));
        
        if (!File.Exists(templateFilePath))
            return null;

        try
        {
            // Read template file bytes (use synchronous version for .NET Standard 2.0 compatibility)
            var templateBytes = File.ReadAllBytes(templateFilePath);
            
            // Generate content hash
            var contentHash = HashingUtilities.GenerateFileHash(templateBytes);
            
            // Use CDNUtilities to save to the asset subdirectory
            var fileName = contentHash + ".rbxl";
            CDNUtilities.SaveToCDN("asset", fileName, templateBytes);
            
            return contentHash;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to process template file {templateFilePath}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Processes a template file and saves it to the CDN assets directory with hash-based naming
    /// Overload that takes template bytes directly
    /// </summary>
    /// <param name="templateBytes">Template file bytes</param>
    /// <param name="assetsRoot">Root directory where assets are stored</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Content hash of the template file, or null if processing failed</returns>
    public static string? ProcessAndSaveTemplateAsync(byte[] templateBytes, string assetsRoot, CancellationToken cancellationToken = default)
    {
        if (templateBytes == null || templateBytes.Length == 0)
            throw new ArgumentException("Template bytes cannot be null or empty", nameof(templateBytes));
        if (string.IsNullOrWhiteSpace(assetsRoot))
            throw new ArgumentException("Assets root directory is required", nameof(assetsRoot));

        try
        {
            // Generate content hash
            var contentHash = HashingUtilities.GenerateFileHash(templateBytes);
            
            // Use CDNUtilities to save to the asset subdirectory
            var fileName = contentHash + ".rbxl";
            CDNUtilities.SaveToCDN("asset", fileName, templateBytes);
            
            return contentHash;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to process template bytes: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Checks if a template file already exists in the CDN assets directory
    /// </summary>
    /// <param name="contentHash">Content hash to check</param>
    /// <param name="assetsRoot">Root directory where assets are stored</param>
    /// <returns>True if the file exists, false otherwise</returns>
    public static bool DoesTemplateExist(string contentHash, string assetsRoot)
    {
        if (string.IsNullOrWhiteSpace(contentHash))
            throw new ArgumentException("Content hash is required", nameof(contentHash));
        if (string.IsNullOrWhiteSpace(assetsRoot))
            throw new ArgumentException("Assets root directory is required", nameof(assetsRoot));

        try
        {
            var fileName = contentHash + ".rbxl";
            var cdnAssetsPath = CDNUtilities.GetCDNAssetsPath("asset");
            var fullPath = Path.Combine(cdnAssetsPath, fileName);
            
            return File.Exists(fullPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to check if template exists {contentHash}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Gets the full path to a template file in the CDN assets directory
    /// </summary>
    /// <param name="contentHash">Content hash of the template</param>
    /// <param name="assetsRoot">Root directory where assets are stored</param>
    /// <returns>Full path to the template file, or null if it doesn't exist</returns>
    public static string? GetTemplatePath(string contentHash, string assetsRoot)
    {
        if (string.IsNullOrWhiteSpace(contentHash))
            throw new ArgumentException("Content hash is required", nameof(contentHash));
        if (string.IsNullOrWhiteSpace(assetsRoot))
            throw new ArgumentException("Assets root directory is required", nameof(assetsRoot));

        try
        {
            var fileName = contentHash + ".rbxl";
            var cdnAssetsPath = CDNUtilities.GetCDNAssetsPath("asset");
            var fullPath = Path.Combine(cdnAssetsPath, fileName);
            
            return File.Exists(fullPath) ? fullPath : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to get template path {contentHash}: {ex.Message}");
            return null;
        }
    }
}
