using Npgsql;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Common;

namespace Thumbnails;

public static class PlaceThumbnail
{
    /// <summary>
    /// Generates and sets a thumbnail for a place using fire-and-forget pattern
    /// </summary>
    public static async Task GeneratePlaceThumbnailAsync(
        IThumbnailService thumbnailService,
        string connectionString,
        long placeId,
        string placeAssetHash,
        string baseUrl,
        int? width = null,
        int? height = null,
        string placeName = "",
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(placeAssetHash))
                throw new ArgumentException("Place asset hash is required", nameof(placeAssetHash));
            
            var targetWidth = width ?? 110;
            var targetHeight = height ?? 110;
            var highResWidth = 110;
            var highResHeight = 110;
            
            if (width.HasValue && height.HasValue)
            {
                targetWidth = width.Value;
                targetHeight = height.Value;
                highResWidth = width.Value;
                highResHeight = height.Value;
            }
        
            var renderedResult = await thumbnailService.RenderPlaceAsync(placeId, x: targetWidth, y: targetHeight, connectionString: connectionString, placeAssetHash: placeAssetHash);
            var renderedResultHighRes = await thumbnailService.RenderPlaceAsync(placeId, x: highResWidth, y: highResHeight, connectionString: connectionString, placeAssetHash: placeAssetHash);
            var cdnThumbnailsPath = CDNUtilities.GetCDNThumbnailsPath();
            var sourcePath = Path.Combine(renderedResult.FullPath);
            var sourcePathHighRes = Path.Combine(renderedResultHighRes.FullPath);
            var cdnThumbnailPath = Path.Combine(cdnThumbnailsPath, renderedResult.FileName);
            var cdnThumbnailPathHighRes = Path.Combine(cdnThumbnailsPath, renderedResultHighRes.FileName);
            bool thumbnailCopied = string.Equals(Path.GetFullPath(sourcePath), Path.GetFullPath(cdnThumbnailPath), StringComparison.OrdinalIgnoreCase) 
                ? true 
                : CDNUtilities.SafeFileCopy(sourcePath, cdnThumbnailPath);
            bool highResThumbnailCopied = string.Equals(Path.GetFullPath(sourcePathHighRes), Path.GetFullPath(cdnThumbnailPathHighRes), StringComparison.OrdinalIgnoreCase) 
                ? true 
                : CDNUtilities.SafeFileCopy(sourcePathHighRes, cdnThumbnailPathHighRes);
            
            if (thumbnailCopied || highResThumbnailCopied)
            {
                var placeIconUrl = thumbnailCopied ? CDNUtilities.GenerateThumbnailUrl(baseUrl, renderedResult.FileName) : null;
                var placeIconUrlHighRes = highResThumbnailCopied ? CDNUtilities.GenerateThumbnailUrl(baseUrl, renderedResultHighRes.FileName) : null;
                if (placeIconUrl != null)
                {
                    var shouldUpdateMainThumbnailUrl = (targetWidth == 110 && targetHeight == 110);
                    var shouldSetGeneratedThumbnailUrl = (targetWidth != 110 || targetHeight != 110);
                    
                    await SetPlaceGeneratedIconAsync(
                        connectionString,
                        placeId,
                        placeIconUrl,
                        placeIconUrlHighRes,
                        renderedResult.Hash,
                        updateMainThumbnailUrl: shouldUpdateMainThumbnailUrl,
                        shouldSetGeneratedThumbnailUrl ? placeIconUrlHighRes : null,
                        cancellationToken);
                }
            }
            else
            {
                throw new InvalidOperationException("Failed to copy any thumbnail files to CDN location");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in background thumbnail generation: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Updates the thumbnail URL for a place asset in the database.
    /// </summary>
    private static async Task SetPlaceThumbnailUrlAsync(
        string connectionString,
        long placeId,
        string thumbnailUrl,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));
        if (placeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(placeId));
        if (string.IsNullOrWhiteSpace(thumbnailUrl))
            throw new ArgumentException("Thumbnail URL is required", nameof(thumbnailUrl));

        const string sql = @"UPDATE assets 
                           SET thumbnail_url = @thumbnailUrl 
                           WHERE asset_id = @placeId AND is_place = true";

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("thumbnailUrl", thumbnailUrl);
        cmd.Parameters.AddWithValue("placeId", placeId);
        
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Updates the generated icon URL and hash for a place asset in the database.
    /// Only updates the main thumbnail_url field when appropriate (for 256x256 icons)
    /// </summary>
    private static async Task SetPlaceGeneratedIconAsync(
        string connectionString,
        long placeId,
        string iconUrl,
        string iconUrlHighRes,
        string iconHash,
        bool updateMainThumbnailUrl = false,
        string? generatedThumbnailUrl = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));
        if (placeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(placeId));
        if (string.IsNullOrWhiteSpace(iconUrl))
            throw new ArgumentException("Icon URL is required", nameof(iconUrl));
        if (string.IsNullOrWhiteSpace(iconUrlHighRes))
            throw new ArgumentException("High-res icon URL is required", nameof(iconUrlHighRes));
        if (string.IsNullOrWhiteSpace(iconHash))
            throw new ArgumentException("Icon hash is required", nameof(iconHash));

        string sql = @"
            UPDATE assets 
                           SET place_generated_icon_url = @iconUrl,
                               place_generated_icon_high_res_url = @iconUrlHighRes,
                               place_generated_icon_hash = @iconHash" +
                               (updateMainThumbnailUrl ? ", thumbnail_url = @iconUrl" : "") +
                               (generatedThumbnailUrl != null ? ", place_generated_thumbnail_url = @generatedThumbnailUrl" : "") + @"
                           WHERE asset_id = @placeId AND is_place = true";

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("iconUrl", iconUrl);
        cmd.Parameters.AddWithValue("iconUrlHighRes", iconUrlHighRes);
        cmd.Parameters.AddWithValue("iconHash", iconHash);
        cmd.Parameters.AddWithValue("placeId", placeId);
        
        if (generatedThumbnailUrl != null)
        {
            cmd.Parameters.AddWithValue("generatedThumbnailUrl", generatedThumbnailUrl);
        }
        
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Resizes an image to the specified dimensions with high quality
    /// </summary>
    private static byte[] ResizeImage(byte[] imageBytes, int width, int height)
    {
        using var originalStream = new MemoryStream(imageBytes);
        using var originalImage = Image.FromStream(originalStream);
        
        var resizedImage = new Bitmap(width, height);
        using var graphics = Graphics.FromImage(resizedImage);
        
        graphics.CompositingQuality = CompositingQuality.HighQuality;
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.SmoothingMode = SmoothingMode.HighQuality;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        
        graphics.DrawImage(originalImage, 0, 0, width, height);
        
        using var resizedStream = new MemoryStream();
        resizedImage.Save(resizedStream, ImageFormat.Png);
        return resizedStream.ToArray();
    }

    /// <summary>
    /// Processes and saves a custom icon upload for a place
    /// </summary>
    public static async Task<(string iconUrl, string iconUrlHighRes, string fileHash)> ProcessCustomIconAsync(
        long placeId,
        Stream fileStream,
        string fileName,
        string contentType,
        string baseUrl)
    {
        if (fileStream == null || fileStream.Length == 0)
            throw new ArgumentException("No file uploaded", nameof(fileStream));

        if (!contentType.StartsWith("image/"))
            throw new ArgumentException("Invalid file type. Please upload an image file.", nameof(contentType));

        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        var fileBytes = memoryStream.ToArray();
        var fileHash = HashingUtilities.GenerateFileHash(fileBytes);
        var resized110 = ResizeImage(fileBytes, 110, 110);
        var resized110HighRes = ResizeImage(fileBytes, 110, 110);
        var cdnAssetsPath = CDNUtilities.GetCDNPlaceIconsPath();
        CDNUtilities.SaveToCDN("place-icons", $"{fileHash}.png", resized110);
        CDNUtilities.SaveToCDN("place-icons", $"{fileHash}.png", resized110HighRes);
        var iconUrl = CDNUtilities.GeneratePlaceIconUrl(baseUrl, $"{fileHash}.png");
        var iconUrlHighRes = CDNUtilities.GeneratePlaceIconUrl(baseUrl, $"{fileHash}.png");

        return (iconUrl, iconUrlHighRes, fileHash);
    }

    /// <summary>
    /// Processes and saves a thumbnail image for the place gallery
    /// </summary>
    public static async Task<(string thumbnailUrl, string fileHash)> ProcessThumbnailImageAsync(
        long placeId,
        Stream fileStream,
        string fileName,
        string contentType,
        string baseUrl)
    {
        if (fileStream == null || fileStream.Length == 0)
            throw new ArgumentException("No file uploaded", nameof(fileStream));

        if (!contentType.StartsWith("image/"))
            throw new ArgumentException("Invalid file type. Please upload an image file.", nameof(contentType));

        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        var fileBytes = memoryStream.ToArray();
        var fileHash = HashingUtilities.GenerateFileHash(fileBytes);
        var resized1280x720 = ResizeImage(fileBytes, 1280, 720);
        CDNUtilities.SaveToCDN("place-thumbnails", $"{fileHash}.png", resized1280x720);
        var thumbnailUrl = CDNUtilities.GeneratePlaceThumbnailUrl(baseUrl, $"{fileHash}.png");

        return (thumbnailUrl, fileHash);
    }

    /// <summary>
    /// Updates custom icon fields for a place in the database
    /// </summary>
    public static async Task SetPlaceCustomIconAsync(
        string connectionString,
        long placeId,
        string iconUrl,
        string iconUrlHighRes,
        string iconHash,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));
        if (placeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(placeId));
        if (string.IsNullOrWhiteSpace(iconUrl))
            throw new ArgumentException("Icon URL is required", nameof(iconUrl));
        if (string.IsNullOrWhiteSpace(iconUrlHighRes))
            throw new ArgumentException("High-res icon URL is required", nameof(iconUrlHighRes));
        if (string.IsNullOrWhiteSpace(iconHash))
            throw new ArgumentException("Icon hash is required", nameof(iconHash));

        const string sql = @"UPDATE assets 
                           SET custom_icon = true,
                               place_custom_icon_url = @iconUrl,
                               place_custom_icon_high_res_url = @iconUrlHighRes,
                               place_custom_icon_hash = @iconHash,
                               generated_icon = false
                           WHERE asset_id = @placeId AND is_place = true";

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("iconUrl", iconUrl);
        cmd.Parameters.AddWithValue("iconUrlHighRes", iconUrlHighRes);
        cmd.Parameters.AddWithValue("iconHash", iconHash);
        cmd.Parameters.AddWithValue("placeId", placeId);
        
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Clears the custom icon for a place but preserves any generated icon.
    ///
    /// Behavior:
    /// - Disables the custom icon and clears its URLs and hash.
    /// - Leaves generated_icon and its URLs/hashes untouched so the
    ///   auto-generated preview can still be shown in the UI.
    /// - Optionally clears the thumbnail_url if the custom icon was being used as the thumbnail.
    /// </summary>
    public static async Task ClearPlaceIconAsync(
        string connectionString,
        long placeId,
        string placeholderUrl,
        bool clearThumbnail = false,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));
        if (placeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(placeId));
        if (string.IsNullOrWhiteSpace(placeholderUrl))
            throw new ArgumentException("Placeholder URL is required", nameof(placeholderUrl));

        const string sql = @"UPDATE assets 
                           SET custom_icon = false,
                               place_custom_icon_url = NULL,
                               place_custom_icon_high_res_url = NULL,
                               place_custom_icon_hash = NULL,
                               generated_icon = true,
                               thumbnail_url = CASE WHEN @clearThumbnail = true THEN NULL ELSE thumbnail_url END
                           WHERE asset_id = @placeId AND is_place = true";

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("placeId", placeId);
        cmd.Parameters.AddWithValue("clearThumbnail", clearThumbnail);
        
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Adds a thumbnail image to place_thumbnails table, checking for duplicates
    /// </summary>
    public static async Task<(long id, string url, string altText, string fileHash, int sortOrder, DateTime createdAt)?> AddThumbnailImageAsync(
        string connectionString,
        long placeId,
        string placeName,
        string thumbnailUrl,
        string fileHash,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));
        if (placeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(placeId));
        if (string.IsNullOrWhiteSpace(thumbnailUrl))
            throw new ArgumentException("Thumbnail URL is required", nameof(thumbnailUrl));
        if (string.IsNullOrWhiteSpace(fileHash))
            throw new ArgumentException("File hash is required", nameof(fileHash));

        const string duplicateCheckSql = "SELECT id, sort_order FROM place_thumbnails WHERE place_id = @placeId AND thumbnail_type = 'image' AND file_hash = @fileHash LIMIT 1";
        
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var duplicateCmd = new NpgsqlCommand(duplicateCheckSql, conn);
        duplicateCmd.Parameters.AddWithValue("placeId", placeId);
        duplicateCmd.Parameters.AddWithValue("fileHash", fileHash);
        
        await using var reader = await duplicateCmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        long existingId = 0;
        int existingSortOrder = 0;
        
        if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            existingId = reader.GetInt64(0);
            existingSortOrder = reader.GetInt32(1);
        }
        
        await reader.CloseAsync();
        
        if (existingId > 0)
        {
            const string getExistingSql = "SELECT url, alt_text, file_hash, sort_order, created_at FROM place_thumbnails WHERE id = @id";
            using var getCmd = new NpgsqlCommand(getExistingSql, conn);
            getCmd.Parameters.AddWithValue("id", existingId);
            
            await using var existingReader = await getCmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            if (await existingReader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                return (
                    existingId,
                    existingReader.GetString(0), // url
                    existingReader.GetString(1), // alt_text
                    existingReader.GetString(2), // file_hash
                    existingReader.GetInt32(3), // sort_order
                    existingReader.GetDateTime(4)  // created_at
                );
            }
        }

        const string sql = @"
            INSERT INTO place_thumbnails (place_id, thumbnail_type, url, alt_text, file_hash, sort_order)
            VALUES (@placeId, 'image', @url, @altText, @fileHash, 
                    COALESCE((SELECT MAX(sort_order) + 1 FROM place_thumbnails WHERE place_id = @placeId), 0))
            RETURNING id, place_id, thumbnail_type, url, alt_text, file_hash, sort_order, created_at";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("placeId", placeId);
        cmd.Parameters.AddWithValue("url", thumbnailUrl);
        cmd.Parameters.AddWithValue("altText", $"{placeName} Thumbnail");
        cmd.Parameters.AddWithValue("fileHash", fileHash);
        
        await using var insertReader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (await insertReader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return (
                insertReader.GetInt64(0),
                insertReader.GetString(3),
                insertReader.GetString(4),
                insertReader.GetString(5),
                insertReader.GetInt32(6),
                insertReader.GetDateTime(7)
            );
        }

        throw new InvalidOperationException("Failed to insert thumbnail image");
    }

    /// <summary>
    /// Sets or replaces the YouTube video URL for a place
    /// Only one video URL is allowed per place
    /// </summary>
    public static async Task<(long id, string url, string altText, string videoUrl, int sortOrder, DateTime createdAt)> SetPlaceVideoUrlAsync(
        string connectionString,
        long placeId,
        string placeName,
        string videoUrl,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));
        if (placeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(placeId));
        if (string.IsNullOrWhiteSpace(videoUrl))
            throw new ArgumentException("Video URL is required", nameof(videoUrl));

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        const string insertSql = @"
            INSERT INTO place_thumbnails (place_id, thumbnail_type, url, alt_text, video_url, sort_order)
            VALUES (@placeId, 'video', @url, @altText, @videoUrl, 0)
            RETURNING id, place_id, thumbnail_type, url, alt_text, video_url, sort_order, created_at";

        try
        {
            using var insertCmd = new NpgsqlCommand(insertSql, conn);
            insertCmd.Parameters.AddWithValue("@placeId", placeId);
            insertCmd.Parameters.AddWithValue("@url", videoUrl);
            insertCmd.Parameters.AddWithValue("@altText", $"{placeName} Video");
            insertCmd.Parameters.AddWithValue("@videoUrl", videoUrl);
            
            await using var insertReader = await insertCmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            if (await insertReader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                return (
                    insertReader.GetInt64(0),
                    insertReader.GetString(3),
                    insertReader.GetString(4),
                    insertReader.GetString(5),
                    insertReader.GetInt32(6),
                    insertReader.GetDateTime(7)
                );
            }
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {
            const string updateSql = @"
                UPDATE place_thumbnails 
                SET url = @url, alt_text = @altText, video_url = @videoUrl, updated_at = NOW()
                WHERE place_id = @placeId AND thumbnail_type = 'video'
                RETURNING id, place_id, thumbnail_type, url, alt_text, video_url, sort_order, created_at";

            using var updateCmd = new NpgsqlCommand(updateSql, conn);
            updateCmd.Parameters.AddWithValue("@placeId", placeId);
            updateCmd.Parameters.AddWithValue("@url", videoUrl);
            updateCmd.Parameters.AddWithValue("@altText", $"{placeName} Video");
            updateCmd.Parameters.AddWithValue("@videoUrl", videoUrl);
            
            await using var reader = await updateCmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                return (
                    reader.GetInt64(0),
                    reader.GetString(3),
                    reader.GetString(4),
                    reader.GetString(5),
                    reader.GetInt32(6),
                    reader.GetDateTime(7)
                );
            }
        }

        throw new InvalidOperationException("Failed to set video URL");
    }

    /// <summary>
    /// Removes a thumbnail from the place_thumbnails table
    /// </summary>
    public static async Task<bool> RemoveThumbnailAsync(
        string connectionString,
        long placeId,
        long thumbnailId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));
        if (placeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(placeId));
        if (thumbnailId <= 0)
            throw new ArgumentOutOfRangeException(nameof(thumbnailId));

        const string sql = "DELETE FROM place_thumbnails WHERE id = @thumbnailId AND place_id = @placeId";

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("thumbnailId", thumbnailId);
        cmd.Parameters.AddWithValue("placeId", placeId);
        
        var rowsAffected = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        return rowsAffected > 0;
    }

    /// <summary>
    /// Gets all thumbnails for a place including video from assets table
    /// </summary>
    public static async Task<List<object>> GetAllPlaceThumbnailsAsync(
        string connectionString,
        long placeId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));
        if (placeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(placeId));

        var thumbnails = new List<object>();

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        const string imageThumbnailsSql = @"
            SELECT id, thumbnail_type, url, alt_text, video_url, sort_order, created_at
            FROM place_thumbnails 
            WHERE place_id = @placeId AND thumbnail_type = 'image'
            ORDER BY sort_order ASC, created_at ASC";
        
        using var imageCmd = new NpgsqlCommand(imageThumbnailsSql, conn);
        imageCmd.Parameters.AddWithValue("placeId", placeId);
        
        await using var imageReader = await imageCmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await imageReader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var thumbnailObj = new {
                id = imageReader.GetInt64(0),
                type = "image",
                url = imageReader.GetString(2),
                altText = imageReader.GetString(3),
                sortOrder = imageReader.GetInt32(5),
                createdAt = imageReader.GetDateTime(6),
                videoUrl = (string)null
            };
            
            thumbnails.Add(thumbnailObj);
        }
        
        await imageReader.CloseAsync();
        const string videoSql = @"
            SELECT place_thumbnail_video, place_video_thumbnail
            FROM assets 
            WHERE asset_id = @placeId AND is_place = true AND place_video_thumbnail = true";
        
        using var videoCmd = new NpgsqlCommand(videoSql, conn);
        videoCmd.Parameters.AddWithValue("placeId", placeId);
        
        await using var videoReader = await videoCmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (await videoReader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            if (!videoReader.IsDBNull(0))
            {
                var videoUrl = videoReader.GetString(0);
                var videoObj = new {
                    id = (long)0,
                    type = "video",
                    url = videoUrl,
                    altText = "Video Thumbnail",
                    sortOrder = 0,
                    createdAt = DateTime.UtcNow,
                    videoUrl = videoUrl
                };
                
                thumbnails.Add(videoObj);
            }
        }

        return thumbnails;
    }

    /// <summary>
    /// Gets all thumbnails for a place (legacy method - only gets from place_thumbnails table)
    /// </summary>
    public static async Task<List<object>> GetPlaceThumbnailsAsync(
        string connectionString,
        long placeId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));
        if (placeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(placeId));

        const string sql = @"
            SELECT id, thumbnail_type, url, alt_text, video_url, sort_order, created_at
            FROM place_thumbnails 
            WHERE place_id = @placeId 
            ORDER BY sort_order ASC, created_at ASC";

        var thumbnails = new List<object>();

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("placeId", placeId);
        
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var thumbnailType = reader.GetString(1);
            var thumbnailObj = new {
                id = reader.GetInt64(0),
                type = thumbnailType,
                url = reader.GetString(2),
                altText = reader.GetString(3),
                sortOrder = reader.GetInt32(5),
                createdAt = reader.GetDateTime(6),
                videoUrl = thumbnailType == "video" && !reader.IsDBNull(4) ? reader.GetString(4) : null
            };
            
            thumbnails.Add(thumbnailObj);
        }

        return thumbnails;
    }

    /// <summary>
    /// Updates place thumbnail flags in the assets table
    /// </summary>
    public static async Task UpdatePlaceThumbnailFlagsAsync(
        string connectionString,
        long placeId,
        bool hasCustom,
        bool hasVideo,
        bool hasAutogenerated,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));
        if (placeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(placeId));

        const string sql = @"
            UPDATE assets 
            SET place_custom_thumbnail = @hasCustom,
                place_video_thumbnail = @hasVideo,
                place_autogenerated_thumbnail = @hasAutogenerated
            WHERE asset_id = @placeId AND is_place = true";

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("hasCustom", hasCustom);
        cmd.Parameters.AddWithValue("hasVideo", hasVideo);
        cmd.Parameters.AddWithValue("hasAutogenerated", hasAutogenerated);
        cmd.Parameters.AddWithValue("placeId", placeId);
        
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Updates the sort order of multiple thumbnails for a place
    /// </summary>
    public static async Task<bool> UpdateThumbnailSortOrderAsync(
        string connectionString,
        long placeId,
        List<long> thumbnailIds,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));
        if (placeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(placeId));
        if (thumbnailIds == null || thumbnailIds.Count == 0)
            throw new ArgumentException("Thumbnail IDs are required", nameof(thumbnailIds));

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        
        using var transaction = connection.BeginTransaction();
        
        try
        {
            var updateQuery = "UPDATE place_thumbnails SET sort_order = @sort_order WHERE id = @thumbnail_id AND place_id = @place_id";
            
            for (int i = 0; i < thumbnailIds.Count; i++)
            {
                var updateCommand = new NpgsqlCommand(updateQuery, connection, transaction);
                updateCommand.Parameters.AddWithValue("@sort_order", i);
                updateCommand.Parameters.AddWithValue("@thumbnail_id", thumbnailIds[i]);
                updateCommand.Parameters.AddWithValue("@place_id", placeId);
                
                await updateCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                updateCommand.Dispose();
            }

            transaction.Commit();
            
            await UpdateMainThumbnailFromFirstSortedAsync(connectionString, placeId, cancellationToken);
            
            return true;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    /// <summary>
    /// Gets the place ID for a thumbnail and verifies user ownership
    /// </summary>
    public static async Task<(long placeId, bool isValid)> GetThumbnailPlaceAndOwnershipAsync(
        string connectionString,
        long thumbnailId,
        long userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));
        if (thumbnailId <= 0)
            throw new ArgumentOutOfRangeException(nameof(thumbnailId));
        if (userId <= 0)
            throw new ArgumentOutOfRangeException(nameof(userId));

        const string sql = @"
            SELECT pt.place_id, a.owner_user_id 
            FROM place_thumbnails pt
            JOIN assets a ON pt.place_id = a.asset_id
            WHERE pt.id = @thumbnailId LIMIT 1";

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        
        using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@thumbnailId", thumbnailId);
        
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var placeId = reader.GetInt64(0);
            var ownerId = reader.GetInt64(1);
            return (placeId, ownerId == userId);
        }

        return (0, false);
    }

    /// <summary>
    /// Adds an auto-generated thumbnail image to place_thumbnails table
    /// </summary>
    public static async Task AddAutoGeneratedThumbnailAsync(
        string connectionString,
        long placeId,
        string altText,
        string thumbnailUrl,
        string fileHash,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));
        if (placeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(placeId));
        if (string.IsNullOrWhiteSpace(thumbnailUrl))
            throw new ArgumentException("Thumbnail URL is required", nameof(thumbnailUrl));
        if (string.IsNullOrWhiteSpace(fileHash))
            throw new ArgumentException("File hash is required", nameof(fileHash));

        const string duplicateCheckSql = @"
            SELECT id FROM place_thumbnails 
            WHERE place_id = @placeId AND thumbnail_type = 'image' AND alt_text = @altText 
            LIMIT 1";
        
        await using var dupConn = new NpgsqlConnection(connectionString);
        await dupConn.OpenAsync(cancellationToken).ConfigureAwait(false);
        
        using var duplicateCmd = new NpgsqlCommand(duplicateCheckSql, dupConn);
        duplicateCmd.Parameters.AddWithValue("placeId", placeId);
        duplicateCmd.Parameters.AddWithValue("altText", altText);
        
        var existingId = await duplicateCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        if (existingId != null)
        {
            const string updateSql = @"
                UPDATE place_thumbnails 
                SET url = @url, file_hash = @fileHash, updated_at = NOW()
                WHERE id = @id";
            
            using var updateCmd = new NpgsqlCommand(updateSql, dupConn);
            updateCmd.Parameters.AddWithValue("url", thumbnailUrl);
            updateCmd.Parameters.AddWithValue("fileHash", fileHash);
            updateCmd.Parameters.AddWithValue("id", existingId);
            
            await updateCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
        else
        {
            const string insertSql = @"
                INSERT INTO place_thumbnails (place_id, thumbnail_type, url, alt_text, file_hash, sort_order)
                VALUES (@placeId, 'image', @url, @altText, @fileHash, 
                        COALESCE((SELECT MAX(sort_order) + 1 FROM place_thumbnails WHERE place_id = @placeId), 0))";
            
            using var insertCmd = new NpgsqlCommand(insertSql, dupConn);
            insertCmd.Parameters.AddWithValue("placeId", placeId);
            insertCmd.Parameters.AddWithValue("url", thumbnailUrl);
            insertCmd.Parameters.AddWithValue("altText", altText);
            insertCmd.Parameters.AddWithValue("fileHash", fileHash);
            
            await insertCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Creates an auto-generated 720p thumbnail for a place and updates the assets table
    /// </summary>
    public static async Task GenerateAutoGeneratedThumbnailAsync(
        IThumbnailService thumbnailService,
        string connectionString,
        long placeId,
        string placeAssetHash,
        string baseUrl,
        CancellationToken cancellationToken = default)
    {
        if (thumbnailService == null)
            throw new ArgumentNullException(nameof(thumbnailService));
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));
        if (placeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(placeId));
        if (string.IsNullOrWhiteSpace(placeAssetHash))
            throw new ArgumentException("Place asset hash is required", nameof(placeAssetHash));
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new ArgumentException("Base URL is required", nameof(baseUrl));

        try
        {
            var renderedResult = await thumbnailService.RenderPlaceAsync(
                placeId, 
                x: 1280, 
                y: 720, 
                connectionString: connectionString, 
                placeAssetHash: placeAssetHash, 
                cancellationToken: cancellationToken);
            var cdnPlaceThumbnailsPath = CDNUtilities.GetCDNAssetsPath("place-thumbnails");
            var sourcePath = Path.Combine(renderedResult.FullPath);
            var cdnThumbnailPath = Path.Combine(cdnPlaceThumbnailsPath, renderedResult.FileName);
            
            bool thumbnailCopied = string.Equals(Path.GetFullPath(sourcePath), Path.GetFullPath(cdnThumbnailPath), StringComparison.OrdinalIgnoreCase) 
                ? true 
                : CDNUtilities.SafeFileCopy(sourcePath, cdnThumbnailPath);
            
            if (thumbnailCopied)
            {
                var thumbnailUrl = CDNUtilities.GeneratePlaceThumbnailUrl(baseUrl, renderedResult.FileName);
                await SetPlaceAutoGeneratedThumbnailUrlAsync(
                    connectionString,
                    placeId,
                    thumbnailUrl,
                    renderedResult.Hash,
                    cancellationToken);
                
            }
            else
            {
                throw new InvalidOperationException("Failed to copy thumbnail file to CDN location");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating auto-generated thumbnail for place {placeId}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Updates the auto-generated thumbnail URL for a place asset in the database
    /// </summary>
    private static async Task SetPlaceAutoGeneratedThumbnailAsync(
        string connectionString,
        long placeId,
        string thumbnailUrl,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));
        if (placeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(placeId));
        if (string.IsNullOrWhiteSpace(thumbnailUrl))
            throw new ArgumentException("Thumbnail URL is required", nameof(thumbnailUrl));

        const string sql = @"
            UPDATE assets 
            SET place_generated_thumbnail_url = @thumbnailUrl
            WHERE asset_id = @placeId AND is_place = true";

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("thumbnailUrl", thumbnailUrl);
        cmd.Parameters.AddWithValue("placeId", placeId);
        
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Updates the auto-generated thumbnail URL and hash for a place
    /// </summary>
    public static async Task SetPlaceAutoGeneratedThumbnailUrlAsync(
        string connectionString,
        long placeId,
        string thumbnailUrl,
        string fileHash,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));
        if (placeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(placeId));

        const string sql = @"
            UPDATE assets 
            SET place_generated_thumbnail_url = @thumbnailUrl,
                place_autogenerated_thumbnail = true
            WHERE asset_id = @placeId AND is_place = true";

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        
        using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("thumbnailUrl", thumbnailUrl);
        cmd.Parameters.AddWithValue("fileHash", fileHash);
        cmd.Parameters.AddWithValue("placeId", placeId);
        
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Updates only the place_thumbnail_video field in the assets table
    /// This is used for temporary video storage before user saves configuration
    /// </summary>
    public static async Task SetPlaceVideoUrlTempAsync(
        string connectionString,
        long placeId,
        string videoUrl,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));
        if (placeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(placeId));
        if (string.IsNullOrWhiteSpace(videoUrl))
            throw new ArgumentException("Video URL is required", nameof(videoUrl));

        const string sql = @"
            UPDATE assets 
            SET place_thumbnail_video = @videoUrl
            WHERE asset_id = @placeId AND is_place = true";

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("videoUrl", videoUrl);
        cmd.Parameters.AddWithValue("placeId", placeId);
        
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Updates the main thumbnail URL for a place to point to the first thumbnail in sort order
    /// </summary>
    public static async Task UpdateMainThumbnailFromFirstSortedAsync(
        string connectionString,
        long placeId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));
        if (placeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(placeId));

        const string sql = @"
            UPDATE assets 
            SET thumbnail_url = (
                SELECT CASE 
                    WHEN pt.thumbnail_type = 'image' THEN pt.url
                    WHEN pt.thumbnail_type = 'video' THEN pt.video_url
                    ELSE pt.url
                END
                FROM place_thumbnails pt
                WHERE pt.place_id = assets.asset_id 
                ORDER BY pt.sort_order ASC, pt.created_at ASC
                LIMIT 1
            )
            WHERE asset_id = @placeId AND is_place = true";

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        
        using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@placeId", placeId);
        
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }
}
