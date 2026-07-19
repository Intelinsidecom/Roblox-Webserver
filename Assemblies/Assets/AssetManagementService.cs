using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Common;

namespace Assets
{
    /// <summary>
    /// Universal asset data contract for updates
    /// </summary>
    public class AssetUpdateData
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public long RobuxPrice { get; set; }
        public long TixPrice { get; set; }
        public bool PutOnSale { get; set; }
        public bool IsLimited { get; set; }
        public long? LimitedQuantity { get; set; }
        public long? LimitedRemaining { get; set; }
        public DateTime? LimitedUntil { get; set; }
    }

    /// <summary>
    /// Universal asset search result for read operations
    /// </summary>
    public class AssetData
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int AssetTypeId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Price { get; set; } = string.Empty;
        public string TixPrice { get; set; } = string.Empty;
        public string Creator { get; set; } = string.Empty;
        public long CreatorId { get; set; }
        public string UpdatedTime { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public string HighResThumbnailUrl { get; set; } = string.Empty;
        public bool IsLimited { get; set; } = false;
        public bool PutOnSale { get; set; } = false;
        public long? LimitedQuantity { get; set; }
        public long? LimitedRemaining { get; set; }
        public DateTime? LimitedUntil { get; set; }
    }

    /// <summary>
    /// Universal asset search parameters
    /// </summary>
    public class AssetSearchParameters
    {
        public string SearchQuery { get; set; } = string.Empty;
        public string AssetIdQuery { get; set; } = string.Empty;
        public string AssetTypeFilter { get; set; } = "All Types";
        public string DateFilter { get; set; } = "Any Time";
        public string SortFilter { get; set; } = "Most Recent";
        public int Limit { get; set; } = 50;
        public int Offset { get; set; } = 0;
    }

    /// <summary>
    /// Configuration values for asset upload
    /// </summary>
    public class AssetUploadConfiguration
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string AssetsDirectory { get; set; } = string.Empty;
        public string ThumbnailsOutputDirectory { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public string TshirtTemplatePath { get; set; } = string.Empty;
        public string TshirtTemplateHighResPath { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string PublicBaseUrl { get; set; } = string.Empty;
        public long DefaultOwnerUserId { get; set; } = 1;
    }

    /// <summary>
    /// Result of image-based asset upload
    /// </summary>
    public class ImageBasedAssetUploadResult
    {
        public long AssetId { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public string AssetType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Result of asset upload validation
    /// </summary>
    public class AssetUploadValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public long? AssetId { get; set; }
        public int AssetTypeId { get; set; }
        public long RobuxPrice { get; set; }
        public long TixPrice { get; set; }
    }
    /// <summary>
    /// Advanced asset management service for site-wide asset operations
    /// </summary>
    public class AssetManagementService
    {
        private readonly AssetsRepository _repository;
        private readonly TShirtAssetService _tShirtService;
        private readonly ShirtAssetService _shirtService;
        private readonly PantsAssetService _pantsService;
        private readonly DecalAssetService _decalService;
        private readonly AssetUploadConfiguration _config;
        private readonly IConfiguration _configuration;

        public AssetManagementService(IConfiguration configuration, AssetUploadConfiguration config)
        {
            _configuration = configuration;
            _config = config;
            _repository = new AssetsRepository();
            _tShirtService = new TShirtAssetService();
            _shirtService = new ShirtAssetService();
            _pantsService = new PantsAssetService();
            _decalService = new DecalAssetService();
        }

        /// <summary>
        /// Changes an asset's ID to a custom value
        /// </summary>
        public async Task<long> ChangeAssetIdAsync(string connectionString, long currentAssetId, long newAssetId)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string is required", nameof(connectionString));
            
            if (currentAssetId <= 0)
                throw new ArgumentOutOfRangeException(nameof(currentAssetId), "Current asset ID must be positive");
            
            if (newAssetId <= 0)
                throw new ArgumentOutOfRangeException(nameof(newAssetId), "New asset ID must be positive");
            
            return await _repository.ChangeAssetIdAsync(connectionString, currentAssetId, newAssetId, CancellationToken.None);
        }

        /// <summary>
        /// Gets a single asset by ID for management
        /// </summary>
        /// <param name="assetId">Asset ID</param>
        /// <returns>Asset data or null if not found</returns>
        public async Task<AssetData?> GetAssetByIdAsync(long assetId)
        {
            var connectionString = _config.ConnectionString;
            
            if (string.IsNullOrWhiteSpace(connectionString))
                return null;

            try
            {
                using var conn = new NpgsqlConnection(connectionString);
                await conn.OpenAsync().ConfigureAwait(false);

                const string sql = @"
                    select a.asset_id, a.name, a.description, a.asset_type_id, a.price, a.price_in_tix,
                           u.user_name, a.owner_user_id, a.last_updated, a.thumbnail_url,
                           a.high_res_thumbnail_url,
                           a.on_sale, a.limited_unique, a.limited_quantity,
                           a.limited_remaining, a.limited_until
                    from assets a
                    left join users u on a.owner_user_id = u.user_id
                    where a.asset_id = @asset_id and a.asset_image = false";

                using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("asset_id", assetId);

                using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
                if (await reader.ReadAsync().ConfigureAwait(false))
                {
                    return new AssetData
                    {
                        Id = reader.GetInt64(0),
                        Name = reader.GetString(1),
                        Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        AssetTypeId = reader.GetInt32(3),
                        Type = GetAssetTypeName(reader.GetInt32(3)),
                        Price = FormatRobuxPrice(reader.GetInt64(4)),
                        TixPrice = reader.GetInt64(5).ToString(),
                        Creator = reader.IsDBNull(6) ? "Unknown" : reader.GetString(6),
                        CreatorId = reader.IsDBNull(7) ? 0 : reader.GetInt64(7),
                        UpdatedTime = FormatRelativeTime(reader.GetDateTime(8)),
                        ThumbnailUrl = reader.IsDBNull(9) ? GetDefaultThumbnailUrl(reader.GetInt32(3)) : reader.GetString(9),
                        HighResThumbnailUrl = reader.IsDBNull(10) ? null : reader.GetString(10),
                        PutOnSale = !reader.IsDBNull(11) && reader.GetBoolean(11),
                        IsLimited = !reader.IsDBNull(12) && reader.GetBoolean(12),
                        LimitedQuantity = reader.IsDBNull(13) ? (long?)null : reader.GetInt64(13),
                        LimitedRemaining = reader.IsDBNull(14) ? (long?)null : reader.GetInt64(14),
                        LimitedUntil = reader.IsDBNull(15) ? (DateTime?)null : reader.GetDateTime(15),
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting asset by ID: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Updates an existing asset in the database
        /// </summary>
        /// <param name="asset">The asset data to update</param>
        /// <returns>True if update was successful, false otherwise</returns>
        public async Task<bool> UpdateAssetAsync(AssetUpdateData asset)
        {
            if (asset == null)
                throw new ArgumentNullException(nameof(asset));

            var connectionString = _configuration.GetConnectionString("Default");
            
            try
            {
                using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();
                
                const string sql = @"
                    UPDATE assets 
                    SET name = @name, 
                        description = @description, 
                        price = @robuxPrice, 
                        price_in_tix = @tixPrice, 
                        on_sale = @putOnSale,
                        limited_unique = @isLimited,
                        limited_quantity = @limitedQuantity,
                        limited_remaining = @limitedRemaining,
                        limited_until = @limitedUntil,
                        last_updated = CURRENT_TIMESTAMP
                    WHERE asset_id = @assetId";

                using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@name", asset.Name);
                cmd.Parameters.AddWithValue("@description", asset.Description);
                cmd.Parameters.AddWithValue("@robuxPrice", asset.RobuxPrice);
                cmd.Parameters.AddWithValue("@tixPrice", asset.TixPrice);
                cmd.Parameters.AddWithValue("@putOnSale", asset.PutOnSale);
                cmd.Parameters.AddWithValue("@isLimited", asset.IsLimited);
                cmd.Parameters.AddWithValue("@limitedQuantity", (object?)asset.LimitedQuantity ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@limitedRemaining", (object?)asset.LimitedRemaining ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@limitedUntil", (object?)asset.LimitedUntil ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@assetId", asset.Id);

                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating asset {asset.Id}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Searches assets with filtering and sorting options
        /// </summary>
        public async Task<List<AssetData>> SearchAssetsAsync(AssetSearchParameters searchParams)
        {
            var results = new List<AssetData>();
            var connectionString = _config.ConnectionString;
            
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                System.Diagnostics.Debug.WriteLine("Database connection string is empty");
                return results;
            }

            try
            {
                using var conn = new NpgsqlConnection(connectionString);
                await conn.OpenAsync().ConfigureAwait(false);
                var sql = BuildSearchSql(searchParams, out var parameters);
                using var cmd = new NpgsqlCommand(sql, conn);
                foreach (var param in parameters)
                {
                    cmd.Parameters.AddWithValue(param.Key, param.Value);
                }

                using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var asset = new AssetData
                    {
                        Id = reader.GetInt64(0),
                        Name = reader.GetString(1),
                        Type = GetAssetTypeName(reader.GetInt32(2)),
                        Price = FormatRobuxPrice(reader.GetInt64(3)),
                        TixPrice = reader.GetInt64(4).ToString(),
                        Creator = reader.GetString(5),
                        UpdatedTime = FormatRelativeTime(reader.GetDateTime(6)),
                        ThumbnailUrl = reader.IsDBNull(7) ? GetDefaultThumbnailUrl(reader.GetInt32(2)) : reader.GetString(7),
                        HighResThumbnailUrl = reader.IsDBNull(8) ? null : reader.GetString(8)
                    };
                    results.Add(asset);
                }
                
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error searching assets: {ex.Message}");
            }

            return results;
        }

        /// <summary>
        /// Gets asset type ID from text representation
        /// </summary>
        public int GetAssetTypeIdFromText(string assetTypeText)
        {
            switch (assetTypeText)
            {
                case "Decal (13)":
                    return 13;
                case "T-Shirt (2)":
                    return 2;
                case "Model (10)":
                    return 10;
                case "Mesh (4)":
                    return 4;
                case "Place (9)":
                    return 9;
                case "Shirt (11)":
                    return 11;
                case "Pants (12)":
                    return 12;
                case "Audio (3)":
                    return 3;
                case "Plugin (38)":
                    return 38;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Gets content type based on file extension
        /// </summary>
        public string GetContentType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            switch (extension)
            {
                case ".png":
                    return "image/png";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".bmp":
                    return "image/bmp";
                default:
                    return "application/octet-stream";
            }
        }

        /// <summary>
        /// Checks if a file format is supported for the given asset type
        /// </summary>
        public bool IsFileFormatSupported(string filePath, int assetTypeId)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return false;

            switch (assetTypeId)
            {
                case 1:  // Decal
                case 2:  // T-Shirt
                case 11: // Shirt
                case 12: // Pants
                    return FileUtilities.IsValidImageFile(filePath);
                
                case 10:  // Model
                case 38:  // Plugin
                    var modelExt = Path.GetExtension(filePath).ToLowerInvariant();
                    return modelExt == ".rbxm" || modelExt == ".rbxmx";
                
                case 4:  // Mesh
                    var meshExt = Path.GetExtension(filePath).ToLowerInvariant();
                    return meshExt == ".mesh";
                
                case 3:  // Audio
                    var audioExt = Path.GetExtension(filePath).ToLowerInvariant();
                    return audioExt == ".mp3" || audioExt == ".ogg" || audioExt == ".wav";
                
                case 9:  // Place
                    var placeExt = Path.GetExtension(filePath).ToLowerInvariant();
                    return placeExt == ".rbxl";
                
                default:
                    return false;
            }
        }

        /// <summary>
        /// Uploads an image-based asset with UI feedback and optional custom asset ID assignment
        /// </summary>
        public async Task<ImageBasedAssetUploadResult> UploadImageBasedAssetAsync(
            string selectedFilePath,
            string assetName,
            string assetIdText,
            string robuxPriceText,
            string tixPriceText,
            string assetTypeText,
            bool putOnSale = false,
            long? ownerUserId = null)
        {
            try
            {
                var validation = ValidateAssetUpload(
                    selectedFilePath,
                    assetName,
                    assetIdText,
                    robuxPriceText,
                    tixPriceText,
                    assetTypeText);

                if (!validation.IsValid)
                {
                    throw new ArgumentException(validation.ErrorMessage);
                }

                if (string.IsNullOrWhiteSpace(_config.ConnectionString))
                {
                    throw new InvalidOperationException("Database connection string not configured.");
                }

                var finalOwnerUserId = ownerUserId ?? _config.DefaultOwnerUserId;
                var fileBytes = await Task.Run(() => File.ReadAllBytes(selectedFilePath));
                var contentType = GetContentType(selectedFilePath);
                var fileName = Path.GetFileName(selectedFilePath);
                var newAssetId = await UploadImageBasedAssetInternalAsync(
                    _config.ConnectionString,
                    finalOwnerUserId,
                    validation.AssetName,
                    fileName,
                    contentType,
                    fileBytes,
                    validation.AssetTypeId,
                    validation.RobuxPrice,
                    validation.TixPrice,
                    putOnSale,
                    _config);

                if (validation.AssetId.HasValue && validation.AssetId.Value != newAssetId)
                {
                    try
                    {
                        newAssetId = await ChangeAssetIdAsync(_config.ConnectionString, newAssetId, validation.AssetId.Value);
                    }
                    catch (InvalidOperationException ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to change asset ID: {ex.Message}");
                        throw new InvalidOperationException($"Failed to assign custom asset ID {validation.AssetId.Value}: {ex.Message}", ex);
                    }
                }

                return new ImageBasedAssetUploadResult
                {
                    AssetId = newAssetId,
                    AssetName = validation.AssetName,
                    AssetType = assetTypeText
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error uploading asset: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Validates asset upload parameters
        /// </summary>
        public AssetUploadValidationResult ValidateAssetUpload(
            string filePath,
            string assetName,
            string assetIdText,
            string robuxPriceText,
            string tixPriceText,
            string assetTypeText)
        {
            var result = new AssetUploadValidationResult();

            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                result.IsValid = false;
                result.ErrorMessage = "Please select a valid image file.";
                return result;
            }

            int assetTypeId = GetAssetTypeIdFromText(assetTypeText);
            if (assetTypeId == 0)
            {
                result.IsValid = false;
                result.ErrorMessage = "Please select an asset type.";
                return result;
            }

            if (!IsFileFormatSupported(filePath, assetTypeId))
            {
                result.IsValid = false;
                result.ErrorMessage = "Unsupported File";
                return result;
            }

            if (string.IsNullOrWhiteSpace(assetName))
            {
                result.IsValid = false;
                result.ErrorMessage = "Please enter an asset name.";
                return result;
            }

            if (!string.IsNullOrWhiteSpace(assetIdText) && long.TryParse(assetIdText.Trim(), out var parsedId))
            {
                result.AssetId = parsedId;
            }

            var robuxPriceTextClean = string.IsNullOrWhiteSpace(robuxPriceText) ? "0" : robuxPriceText.Trim();
            if (!long.TryParse(robuxPriceTextClean, out var robuxPrice) || robuxPrice < 0)
            {
                result.IsValid = false;
                result.ErrorMessage = "Please enter a valid Robux price (non-negative number).";
                return result;
            }

            var tixPriceTextClean = string.IsNullOrWhiteSpace(tixPriceText) ? "0" : tixPriceText.Trim();
            if (!long.TryParse(tixPriceTextClean, out var tixPrice) || tixPrice < 0)
            {
                result.IsValid = false;
                result.ErrorMessage = "Please enter a valid Tix price (non-negative number).";
                return result;
            }

            result.RobuxPrice = robuxPrice;
            result.TixPrice = tixPrice;
            result.AssetTypeId = assetTypeId;
            result.IsValid = true;
            result.AssetName = assetName.Trim();
            return result;
        }

        /// <summary>
        /// Replaces an existing asset's file with proper validation for image-based assets
        /// </summary>
        /// <param name="assetId">Asset ID to replace</param>
        /// <param name="newFilePath">New file path</param>
        /// <param name="assetName">Updated asset name (optional)</param>
        /// <returns>True if replacement was successful, false otherwise</returns>
        public async Task<bool> ReplaceAssetAsync(long assetId, string newFilePath, string? assetName = null)
        {
            if (assetId <= 0)
                throw new ArgumentOutOfRangeException(nameof(assetId), "Asset ID must be positive");
            
            if (string.IsNullOrWhiteSpace(newFilePath) || !File.Exists(newFilePath))
                throw new ArgumentException("Valid file path is required", nameof(newFilePath));

            try
            {
                if (string.IsNullOrWhiteSpace(_config.ConnectionString))
                    throw new InvalidOperationException("Database connection string not configured.");

                using var connection = new NpgsqlConnection(_config.ConnectionString);
                await connection.OpenAsync();
                const string getAssetSql = "SELECT asset_type_id, name FROM assets WHERE asset_id = @assetId";
                using var getAssetCmd = new NpgsqlCommand(getAssetSql, connection);
                getAssetCmd.Parameters.AddWithValue("@assetId", assetId);

                int assetTypeId;
                string currentAssetName;
                
                using (var reader = await getAssetCmd.ExecuteReaderAsync())
                {
                    if (!await reader.ReadAsync())
                        throw new InvalidOperationException($"Asset with ID {assetId} not found.");
                    
                    assetTypeId = reader.GetInt32(0);
                    currentAssetName = reader.GetString(1);
                }

                if (!IsFileFormatSupported(newFilePath, assetTypeId))
                {
                    throw new InvalidOperationException($"File format not supported for asset type {assetTypeId}. Supported formats: {GetSupportedFormats(assetTypeId)}");
                }

                if (IsImageBasedAsset(assetTypeId))
                {
                    if (!FileUtilities.IsValidImageFile(newFilePath))
                    {
                        throw new InvalidOperationException("The selected file is not a valid image file or is corrupted.");
                    }
                }

                var fileBytes = await Task.Run(() => File.ReadAllBytes(newFilePath));
                var contentType = GetContentType(newFilePath);
                var fileName = Path.GetFileName(newFilePath);
                bool replacementSuccess;
                switch (assetTypeId)
                {
                    case 2: // T-Shirt
                        replacementSuccess = await ReplaceTShirtAsset(
                            connection, assetId, fileBytes, contentType, fileName, _config);
                        break;
                    case 11: // Shirt
                        replacementSuccess = await ReplaceShirtAsset(
                            connection, assetId, fileBytes, contentType, fileName, _config);
                        break;
                    case 12: // Pants
                        replacementSuccess = await ReplacePantsAsset(
                            connection, assetId, fileBytes, contentType, fileName, _config);
                        break;
                    case 1: // Decal
                        replacementSuccess = await ReplaceDecalAsset(
                            connection, assetId, fileBytes, contentType, fileName, _config);
                        break;
                    case 10: // Model
                        replacementSuccess = await ReplaceModelAsset(
                            connection, assetId, fileBytes, contentType, fileName, _config);
                        break;
                    default:
                        throw new NotSupportedException($"Asset replacement not supported for type {assetTypeId}");
                }

                if (replacementSuccess)
                {
                    if (!string.IsNullOrWhiteSpace(assetName) && assetName != currentAssetName)
                    {
                        const string updateNameSql = "UPDATE assets SET name = @name, last_updated = CURRENT_TIMESTAMP WHERE asset_id = @assetId";
                        using var updateNameCmd = new NpgsqlCommand(updateNameSql, connection);
                        updateNameCmd.Parameters.AddWithValue("@name", assetName.Trim());
                        updateNameCmd.Parameters.AddWithValue("@assetId", assetId);
                        await updateNameCmd.ExecuteNonQueryAsync();
                    }

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error replacing asset {assetId}: {ex.Message}");
                throw new InvalidOperationException($"Failed to replace asset: {ex.Message}", ex);
            }
        }

        #region Private Helper Methods

        private async Task<long> UploadImageBasedAssetInternalAsync(
            string connectionString,
            long ownerUserId,
            string assetName,
            string fileName,
            string contentType,
            byte[] fileBytes,
            int assetTypeId,
            long robuxPrice,
            long tixPrice,
            bool putOnSale,
            AssetUploadConfiguration config)
        {
            long newAssetId;
            switch (assetTypeId)
            {
                case 2: // T-Shirt
                    newAssetId = await _tShirtService.CreateTShirtAsync(
                        connectionString,
                        ownerUserId,
                        assetName,
                        fileName,
                        contentType,
                        fileBytes,
                        config.AssetsDirectory,
                        config.ThumbnailsOutputDirectory,
                        config.ThumbnailUrl,
                        config.TshirtTemplatePath,
                        config.TshirtTemplateHighResPath,
                        config.PublicBaseUrl,
                        config.PublicBaseUrl,
                        CancellationToken.None);
                    break;
                case 11: // Shirt
                    newAssetId = await _shirtService.CreateShirtAsync(
                        connectionString,
                        ownerUserId,
                        assetName,
                        fileName,
                        contentType,
                        fileBytes,
                        config.AssetsDirectory,
                        config.PublicBaseUrl,
                        config.ThumbnailsOutputDirectory,
                        config.ThumbnailUrl,
                        config.PublicBaseUrl,
                        null); // arbiterBaseUrl
                    break;
                case 12: // Pants
                    newAssetId = await _pantsService.CreatePantsAsync(
                        connectionString,
                        ownerUserId,
                        assetName,
                        fileName,
                        contentType,
                        fileBytes,
                        config.AssetsDirectory,
                        config.PublicBaseUrl,
                        config.ThumbnailsOutputDirectory,
                        config.ThumbnailUrl,
                        config.PublicBaseUrl,
                        null); // arbiterBaseUrl
                    break;
                case 13: // Decal
                    newAssetId = await _decalService.CreateDecalAsync(
                        connectionString,
                        ownerUserId,
                        assetName,
                        fileName,
                        contentType,
                        fileBytes,
                        config.AssetsDirectory,
                        config.ThumbnailsOutputDirectory,
                        config.ThumbnailUrl,
                        config.PublicBaseUrl,
                        CancellationToken.None);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported asset type for image-based upload: {assetTypeId}");
            }

            if (robuxPrice > 0 || tixPrice > 0)
            {
                await _repository.UpdateAssetPricesAsync(connectionString, newAssetId, robuxPrice, tixPrice);
            }
            else if (putOnSale)
            {
                await _repository.UpdateAssetPricesAsync(connectionString, newAssetId, 0, 0);
            }

            return newAssetId;
        }

        private AssetUploadConfiguration GetUploadConfiguration()
        {
            var connectionString = _configuration.GetConnectionString("Default");
            var assetsDirectory = _configuration["Assets:Directory"] ?? @"C:\cdn\assets";
            var thumbnailUrl = _configuration["Thumbnails:Url"] ?? "http://localhost/thumbnails";
            var tshirtTemplatePath = _configuration["Templates:TShirt"] ?? @"C:\templates\tshirt.png";
            var tshirtTemplateHighResPath = _configuration["Templates:TShirtHighRes"] ?? @"C:\templates\tshirt_highres.png";
            var publicBaseUrl = _configuration["Website:PublicBaseUrl"] ?? "http://localhost:5077";
            
            long defaultOwnerUserId = 1;
            var defaultOwnerUserIdStr = _configuration["Assets:DefaultOwnerUserId"];
            if (!string.IsNullOrWhiteSpace(defaultOwnerUserIdStr) && 
                long.TryParse(defaultOwnerUserIdStr.Trim(), out var parsedOwnerId) && 
                parsedOwnerId > 0)
            {
                defaultOwnerUserId = parsedOwnerId;
            }
            
            return new AssetUploadConfiguration
            {
                ConnectionString = connectionString,
                AssetsDirectory = assetsDirectory,
                ThumbnailsOutputDirectory = Path.Combine(assetsDirectory, "thumbnails"),
                ThumbnailUrl = thumbnailUrl,
                TshirtTemplatePath = tshirtTemplatePath,
                TshirtTemplateHighResPath = tshirtTemplateHighResPath,
                BaseUrl = publicBaseUrl,
                PublicBaseUrl = publicBaseUrl,
                DefaultOwnerUserId = defaultOwnerUserId
            };
        }

        private string BuildSearchSql(AssetSearchParameters p, out Dictionary<string, object> parameters)
        {
            parameters = new Dictionary<string, object>();
            var conditions = new List<string>();
            var paramIndex = 0;
            var sql = @"
                select a.asset_id, a.name, a.asset_type_id, a.price, a.price_in_tix,
                       u.user_name, a.last_updated, a.thumbnail_url,
                       a.high_res_thumbnail_url
                from assets a
                left join users u on a.owner_user_id = u.user_id
                where a.asset_type_id != 9 and a.asset_image = false";

            if (!string.IsNullOrWhiteSpace(p.AssetIdQuery) && long.TryParse(p.AssetIdQuery.Trim(), out var assetId))
            {
                conditions.Add("a.asset_id = @param" + paramIndex);
                parameters.Add("@param" + paramIndex, assetId);
                paramIndex++;
            }
            else if (!string.IsNullOrWhiteSpace(p.SearchQuery))
            {
                var keyword = p.SearchQuery.Trim();
                
                if (keyword.StartsWith("\"") && keyword.EndsWith("\""))
                {
                    var exactMatch = keyword.Substring(1, keyword.Length - 2);
                    conditions.Add("(LOWER(a.name) = LOWER(@param" + paramIndex + "))");
                    parameters.Add("@param" + paramIndex, exactMatch);
                    paramIndex++;
                }
                else if (keyword.Length < 4)
                {
                    conditions.Add("(LOWER(a.name) LIKE LOWER(@param" + paramIndex + "))");
                    parameters.Add("@param" + paramIndex, "%" + keyword + "%");
                    paramIndex++;
                }
                else
                {
                    conditions.Add("(to_tsvector('english', a.name) @@ plainto_tsquery('english', @param" + paramIndex + "))");
                    parameters.Add("@param" + paramIndex, keyword);
                    paramIndex++;
                }
            }

            if (p.AssetTypeFilter != "All Types")
            {
                var typeId = GetAssetTypeIdFromFilter(p.AssetTypeFilter);
                if (typeId > 0)
                {
                    conditions.Add("a.asset_type_id = @param" + paramIndex);
                    parameters.Add("@param" + paramIndex, typeId);
                    paramIndex++;
                }
            }

            var dateCondition = GetDateFilterCondition(p.DateFilter, paramIndex, parameters);
            if (!string.IsNullOrEmpty(dateCondition))
            {
                conditions.Add(dateCondition);
                paramIndex++;
            }

            if (conditions.Count > 0)
            {
                sql += " AND " + string.Join(" AND ", conditions);
            }

            sql += " " + GetOrderByClause(p.SortFilter);
            sql += " LIMIT @limit OFFSET @offset";
            parameters.Add("@limit", Math.Min(p.Limit, 100));
            parameters.Add("@offset", p.Offset);

            return sql;
        }

        private int GetAssetTypeIdFromFilter(string filterName)
        {
            return filterName switch
            {
                "Decal" => 1,
                "T-Shirt" => 2,
                "Model" => 10,
                "Mesh" => 4,
                "Shirt" => 11,
                "Pants" => 12,
                "Audio" => 3,
                "Plugin" => 38,
                _ => 0
            };
        }

        private string GetDateFilterCondition(string dateFilter, int paramIndex, Dictionary<string, object> parameters)
        {
            var now = DateTime.UtcNow;
            var startDate = dateFilter switch
            {
                "Today" => now.Date,
                "This Week" => now.AddDays(-7),
                "This Month" => now.AddMonths(-1),
                "This Year" => now.AddYears(-1),
                _ => (DateTime?)null
            };

            if (startDate.HasValue)
            {
                parameters.Add("@param" + paramIndex, startDate.Value);
                return "a.last_updated >= @param" + paramIndex;
            }

            return string.Empty;
        }

        private string GetOrderByClause(string sortFilter)
        {
            return sortFilter switch
            {
                "Most Recent" => "ORDER BY a.last_updated DESC",
                "The Oldest" => "ORDER BY a.last_updated ASC",
                "Name A-Z" => "ORDER BY LOWER(a.name) ASC",
                "Name Z-A" => "ORDER BY LOWER(a.name) DESC",
                "Asset ID" => "ORDER BY a.asset_id DESC",
                _ => "ORDER BY a.last_updated DESC"
            };
        }

        private string GetAssetTypeName(int assetTypeId)
        {
            return assetTypeId switch
            {
                1 => "Decal",
                2 => "T-Shirt",
                3 => "Audio",
                10 => "Model",
                4 => "Mesh",
                9 => "Place",
                11 => "Shirt",
                12 => "Pants",
                38 => "Plugin",
                _ => "Unknown"
            };
        }

        private string FormatRobuxPrice(long robuxPrice)
        {
            if (robuxPrice > 0)
                return robuxPrice.ToString();
            return "Free";
        }

        private string FormatRelativeTime(DateTime dateTime)
        {
            var span = DateTime.UtcNow - dateTime;
            
            if (span.TotalMinutes < 1)
                return "Just now";
            if (span.TotalMinutes < 60)
                return $"{(int)span.TotalMinutes} minute{((int)span.TotalMinutes != 1 ? "s" : "")} ago";
            if (span.TotalHours < 24)
                return $"{(int)span.TotalHours} hour{((int)span.TotalHours != 1 ? "s" : "")} ago";
            if (span.TotalDays < 30)
                return $"{(int)span.TotalDays} day{((int)span.TotalDays != 1 ? "s" : "")} ago";
            
            return dateTime.ToString("MMM dd, yyyy");
        }

        private bool IsImageBasedAsset(int assetTypeId)
        {
            return assetTypeId == 1 || // Decal
                   assetTypeId == 2 || // T-Shirt
                   assetTypeId == 11 || // Shirt
                   assetTypeId == 12;   // Pants
        }

        private string GetSupportedFormats(int assetTypeId)
        {
            switch (assetTypeId)
            {
                case 1:
                case 2:
                case 11:
                case 12:
                    return "PNG, JPG, JPEG, BMP, GIF, TIFF, ICO";
                case 3:
                case 38:
                    return ".rbxm, .rbxmx";
                case 4:
                    return ".mesh";
                case 9:
                    return ".rbxl";
                default:
                    return "Unknown";
            }
        }

        private async Task<bool> ReplaceTShirtAsset(NpgsqlConnection connection, long assetId, byte[] fileBytes, string contentType, string fileName, AssetUploadConfiguration config)
        {
            try
            {
                const string sql = @"
                    UPDATE assets 
                    SET asset_image = true, 
                        content_type = @contentType,
                        file_name = @fileName,
                        file_size = @fileSize,
                        last_updated = CURRENT_TIMESTAMP
                    WHERE asset_id = @assetId";

                using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@contentType", contentType);
                cmd.Parameters.AddWithValue("@fileName", fileName);
                cmd.Parameters.AddWithValue("@fileSize", fileBytes.Length);
                cmd.Parameters.AddWithValue("@assetId", assetId);

                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                
                if (rowsAffected > 0)
                {
                    var assetFilePath = Path.Combine(config.AssetsDirectory, $"{assetId}");
                    await Task.Run(() => File.WriteAllBytes(assetFilePath, fileBytes));
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error replacing T-Shirt asset: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> ReplaceShirtAsset(NpgsqlConnection connection, long assetId, byte[] fileBytes, string contentType, string fileName, AssetUploadConfiguration config)
        {
            try
            {
                const string sql = @"
                    UPDATE assets 
                    SET asset_image = true, 
                        content_type = @contentType,
                        file_name = @fileName,
                        file_size = @fileSize,
                        last_updated = CURRENT_TIMESTAMP
                    WHERE asset_id = @assetId";

                using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@contentType", contentType);
                cmd.Parameters.AddWithValue("@fileName", fileName);
                cmd.Parameters.AddWithValue("@fileSize", fileBytes.Length);
                cmd.Parameters.AddWithValue("@assetId", assetId);

                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                
                if (rowsAffected > 0)
                {
                    var assetFilePath = Path.Combine(config.AssetsDirectory, $"{assetId}");
                    await Task.Run(() => File.WriteAllBytes(assetFilePath, fileBytes));
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error replacing Shirt asset: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> ReplacePantsAsset(NpgsqlConnection connection, long assetId, byte[] fileBytes, string contentType, string fileName, AssetUploadConfiguration config)
        {
            try
            {
                const string sql = @"
                    UPDATE assets 
                    SET asset_image = true, 
                        content_type = @contentType,
                        file_name = @fileName,
                        file_size = @fileSize,
                        last_updated = CURRENT_TIMESTAMP
                    WHERE asset_id = @assetId";

                using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@contentType", contentType);
                cmd.Parameters.AddWithValue("@fileName", fileName);
                cmd.Parameters.AddWithValue("@fileSize", fileBytes.Length);
                cmd.Parameters.AddWithValue("@assetId", assetId);

                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                
                if (rowsAffected > 0)
                {
                    var assetFilePath = Path.Combine(config.AssetsDirectory, $"{assetId}");
                    await Task.Run(() => File.WriteAllBytes(assetFilePath, fileBytes));
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error replacing Pants asset: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> ReplaceModelAsset(NpgsqlConnection connection, long assetId, byte[] fileBytes, string contentType, string fileName, AssetUploadConfiguration config)
        {
            try
            {
                var (isValid, errorMessage) = AssetValidationHelper.ValidateModelContent(fileBytes);
                if (!isValid)
                    throw new InvalidOperationException(errorMessage ?? "Model validation failed.");

                string contentHash;
                using (var sha = SHA256.Create())
                {
                    var hashBytes = sha.ComputeHash(fileBytes);
                    var sb = new StringBuilder(hashBytes.Length * 2);
                    foreach (var b in hashBytes)
                        sb.Append(b.ToString("x2"));
                    contentHash = sb.ToString();
                }

                var ext = ".rbxm";
                var assetFilePath = Path.Combine(config.AssetsDirectory, "asset", contentHash + ext);

                var dir = Path.GetDirectoryName(assetFilePath);
                if (!string.IsNullOrWhiteSpace(dir))
                    Directory.CreateDirectory(dir);

                await Task.Run(() => File.WriteAllBytes(assetFilePath, fileBytes));

                string? oldHash = null;
                const string getHashSql = "SELECT content_hash FROM assets WHERE asset_id = @assetId";
                using (var getHashCmd = new NpgsqlCommand(getHashSql, connection))
                {
                    getHashCmd.Parameters.AddWithValue("@assetId", assetId);
                    var hashResult = await getHashCmd.ExecuteScalarAsync();
                    oldHash = hashResult == null || hashResult == DBNull.Value ? null : Convert.ToString(hashResult);
                }

                if (!string.IsNullOrWhiteSpace(oldHash))
                {
                    try
                    {
                        var repo = new AssetsRepository();
                        await repo.AppendVersionHistoryAsync(config.ConnectionString, assetId, oldHash);
                    }
                    catch
                    {
                        // Version history failure should not block upload
                    }
                }

                const string sql = @"
                    UPDATE assets 
                    SET content_hash = @contentHash,
                        file_extension = @fileExtension,
                        content_type = @contentType,
                        last_updated = CURRENT_TIMESTAMP
                    WHERE asset_id = @assetId";

                using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@contentHash", contentHash);
                cmd.Parameters.AddWithValue("@fileExtension", ext);
                cmd.Parameters.AddWithValue("@contentType", contentType);
                cmd.Parameters.AddWithValue("@assetId", assetId);

                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error replacing Model asset: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> ReplaceDecalAsset(NpgsqlConnection connection, long assetId, byte[] fileBytes, string contentType, string fileName, AssetUploadConfiguration config)
        {
            try
            {
                const string sql = @"
                    UPDATE assets 
                    SET asset_image = true, 
                        content_type = @contentType,
                        file_name = @fileName,
                        file_size = @fileSize,
                        last_updated = CURRENT_TIMESTAMP
                    WHERE asset_id = @assetId";

                using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@contentType", contentType);
                cmd.Parameters.AddWithValue("@fileName", fileName);
                cmd.Parameters.AddWithValue("@fileSize", fileBytes.Length);
                cmd.Parameters.AddWithValue("@assetId", assetId);

                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                
                if (rowsAffected > 0)
                {
                    var assetFilePath = Path.Combine(config.AssetsDirectory, $"{assetId}");
                    await Task.Run(() => File.WriteAllBytes(assetFilePath, fileBytes));
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error replacing Decal asset: {ex.Message}");
                return false;
            }
        }

        private string GetDefaultThumbnailUrl(int assetTypeId)
        {
            var thumbnailUrl = _configuration["Thumbnails:Url"] ?? "http://localhost/thumbnails";
            if (!thumbnailUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && 
                !thumbnailUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                thumbnailUrl = "http://" + thumbnailUrl;
            }
            
            return assetTypeId switch
            {
                1 => $"{thumbnailUrl}/asset/decal.png",
                2 => $"{thumbnailUrl}/asset/tshirt.png",
                3 => _configuration["AudioThumbnailUrl"] ?? "/images/audio.png",
                10 => $"{thumbnailUrl}/asset/model.png",
                4 => $"{thumbnailUrl}/asset/mesh.png",
                11 => $"{thumbnailUrl}/asset/shirt.png",
                12 => $"{thumbnailUrl}/asset/pants.png",
                38 => _configuration["PluginThumbnailUrl"] ?? "/images/plugin.png",
                _ => _configuration["DefaultThumbnailUrl"] ?? $"{thumbnailUrl}/asset/default.png"
            };
        }

        #endregion
    }
}
