using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Assets;
using Common;
using Npgsql;
using Microsoft.Extensions.Configuration;

namespace Control_Panel.Functions
{
    /// <summary>
    /// Control Panel specific asset search result (wrapper for universal AssetData)
    /// </summary>
    public class AssetSearchResult
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

    public class AssetService
    {
        private readonly TShirtAssetService _tShirtService;
        private readonly ShirtAssetService _shirtService;
        private readonly PantsAssetService _pantsService;
        private readonly AssetsRepository _repository;
        private readonly AssetManagementService _managementService;

        public AssetService()
        {
            _tShirtService = new TShirtAssetService();
            _shirtService = new ShirtAssetService();
            _pantsService = new PantsAssetService();
            _repository = new AssetsRepository();
            
            // Create configuration from Properties.Settings for AssetManagementService
            var controlPanelConfig = GetConfigurationValues();
            var universalConfig = new AssetUploadConfiguration
            {
                ConnectionString = controlPanelConfig.ConnectionString,
                AssetsDirectory = controlPanelConfig.AssetsDirectory,
                ThumbnailsOutputDirectory = controlPanelConfig.ThumbnailsOutputDirectory,
                ThumbnailUrl = controlPanelConfig.ThumbnailUrl,
                TshirtTemplatePath = controlPanelConfig.TshirtTemplatePath,
                TshirtTemplateHighResPath = controlPanelConfig.TshirtTemplateHighResPath,
                BaseUrl = controlPanelConfig.BaseUrl,
                PublicBaseUrl = controlPanelConfig.PublicBaseUrl,
                DefaultOwnerUserId = controlPanelConfig.DefaultOwnerUserId
            };
            
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["ConnectionStrings:Default"] = universalConfig.ConnectionString,
                    ["Assets:Directory"] = universalConfig.AssetsDirectory,
                    ["Thumbnails:Url"] = universalConfig.ThumbnailUrl,
                    ["Templates:TShirt"] = universalConfig.TshirtTemplatePath,
                    ["Templates:TShirtHighRes"] = universalConfig.TshirtTemplateHighResPath,
                    ["Website:PublicBaseUrl"] = universalConfig.PublicBaseUrl,
                    ["Assets:DefaultOwnerUserId"] = universalConfig.DefaultOwnerUserId.ToString()
                })
                .Build();
            
            _managementService = new AssetManagementService(configuration, universalConfig);
        }

        /// <summary>
        /// Uploads an image-based asset with the specified parameters
        /// </summary>
        public async Task<long> UploadImageBasedAssetAsync(
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
            string cdnAssetsRoot,
            string thumbnailsRoot,
            string thumbnailBaseUrl,
            string tshirtTemplatePath,
            string tshirtTemplateHighResPath,
            string baseUrl,
            string publicAssetBaseUrl)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string is required", nameof(connectionString));
            
            if (ownerUserId <= 0)
                throw new ArgumentOutOfRangeException(nameof(ownerUserId), "Owner user ID must be positive");
            
            if (string.IsNullOrWhiteSpace(assetName))
                throw new ArgumentException("Asset name is required", nameof(assetName));
            
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name is required", nameof(fileName));
            
            if (fileBytes == null || fileBytes.Length == 0)
                throw new ArgumentException("File bytes are required", nameof(fileBytes));

            long newAssetId;
            switch (assetTypeId)
            {
                case 2: // T-Shirt
                    newAssetId = await UploadTShirt(
                        connectionString,
                        ownerUserId,
                        assetName,
                        fileName,
                        contentType,
                        fileBytes,
                        cdnAssetsRoot,
                        thumbnailsRoot,
                        thumbnailBaseUrl,
                        tshirtTemplatePath,
                        tshirtTemplateHighResPath,
                        baseUrl,
                        publicAssetBaseUrl);
                    break;
                case 11: // Shirt
                    newAssetId = await UploadShirt(
                        connectionString,
                        ownerUserId,
                        assetName,
                        fileName,
                        contentType,
                        fileBytes,
                        cdnAssetsRoot,
                        thumbnailsRoot,
                        thumbnailBaseUrl,
                        baseUrl,
                        publicAssetBaseUrl);
                    break;
                case 12: // Pants
                    newAssetId = await UploadPants(
                        connectionString,
                        ownerUserId,
                        assetName,
                        fileName,
                        contentType,
                        fileBytes,
                        cdnAssetsRoot,
                        thumbnailsRoot,
                        thumbnailBaseUrl,
                        baseUrl,
                        publicAssetBaseUrl);
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

        /// <summary>
        /// Uploads a T-Shirt asset
        /// </summary>
        private async Task<long> UploadTShirt(
            string connectionString,
            long ownerUserId,
            string assetName,
            string fileName,
            string contentType,
            byte[] fileBytes,
            string cdnAssetsRoot,
            string thumbnailsRoot,
            string thumbnailBaseUrl,
            string tshirtTemplatePath,
            string tshirtTemplateHighResPath,
            string baseUrl,
            string publicAssetBaseUrl)
        {

            return await _tShirtService.CreateTShirtAsync(
                connectionString,
                ownerUserId,
                assetName,
                fileName,
                contentType,
                fileBytes,
                cdnAssetsRoot,
                thumbnailsRoot ?? string.Empty,
                thumbnailBaseUrl ?? string.Empty,
                tshirtTemplatePath ?? string.Empty,
                tshirtTemplateHighResPath ?? string.Empty,
                publicAssetBaseUrl,
                publicAssetBaseUrl,
                CancellationToken.None);
        }

        /// <summary>
        /// Uploads a Shirt asset
        /// </summary>
        private async Task<long> UploadShirt(
            string connectionString,
            long ownerUserId,
            string assetName,
            string fileName,
            string contentType,
            byte[] fileBytes,
            string cdnAssetsRoot,
            string thumbnailsRoot,
            string thumbnailBaseUrl,
            string baseUrl,
            string publicAssetBaseUrl)
        {
            return await _shirtService.CreateShirtAsync(
                connectionString,
                ownerUserId,
                assetName,
                fileName,
                contentType,
                fileBytes,
                cdnAssetsRoot,
                publicAssetBaseUrl,
                thumbnailsRoot ?? string.Empty,
                thumbnailBaseUrl ?? string.Empty,
                publicAssetBaseUrl,
                null); // arbiterBaseUrl
        }

        /// <summary>
        /// Uploads a Pants asset
        /// </summary>
        private async Task<long> UploadPants(
            string connectionString,
            long ownerUserId,
            string assetName,
            string fileName,
            string contentType,
            byte[] fileBytes,
            string cdnAssetsRoot,
            string thumbnailsRoot,
            string thumbnailBaseUrl,
            string baseUrl,
            string publicAssetBaseUrl)
        {
            return await _pantsService.CreatePantsAsync(
                connectionString,
                ownerUserId,
                assetName,
                fileName,
                contentType,
                fileBytes,
                cdnAssetsRoot,
                publicAssetBaseUrl,
                thumbnailsRoot ?? string.Empty,
                thumbnailBaseUrl ?? string.Empty,
                publicAssetBaseUrl,
                null); // arbiterBaseUrl
        }

        /// <summary>
        /// Gets asset type ID from text representation
        /// </summary>
        public int GetAssetTypeIdFromText(string assetTypeText)
        {
            return _managementService.GetAssetTypeIdFromText(assetTypeText);
        }

        /// <summary>
        /// Gets content type based on file extension
        /// </summary>
        public string GetContentType(string filePath)
        {
            return _managementService.GetContentType(filePath);
        }

        /// <summary>
        /// Checks if a file format is supported for the given asset type
        /// </summary>
        public bool IsFileFormatSupported(string filePath, int assetTypeId)
        {
            return _managementService.IsFileFormatSupported(filePath, assetTypeId);
        }

        /// <summary>
        /// Gets configuration values for asset upload
        /// </summary>
        /// <returns>Configuration values object</returns>
        public AssetUploadConfiguration GetConfigurationValues()
        {
            var settings = Properties.Settings.Default;
            long defaultOwnerUserId = 1;
            if (!string.IsNullOrWhiteSpace(settings.DefaultOwnerUserId) && 
                long.TryParse(settings.DefaultOwnerUserId.Trim(), out var parsedOwnerId) && 
                parsedOwnerId > 0)
            {
                defaultOwnerUserId = parsedOwnerId;
            }
            
            return new AssetUploadConfiguration
            {
                ConnectionString = settings.DatabaseConnectionString,
                AssetsDirectory = settings.AssetsDirectory ?? @"C:\cdn\assets",
                ThumbnailsOutputDirectory = Path.Combine(settings.AssetsDirectory ?? @"C:\cdn\assets", "thumbnails"),
                ThumbnailUrl = settings.ThumbnailUrl ?? "http://localhost/thumbnails",
                TshirtTemplatePath = settings.TshirtTemplatePath ?? @"C:\templates\tshirt.png",
                TshirtTemplateHighResPath = settings.TshirtTemplateHighResPath ?? @"C:\templates\tshirt_highres.png",
                BaseUrl = GetBaseUrl(),
                PublicBaseUrl = settings.PublicBaseUrl,
                DefaultOwnerUserId = defaultOwnerUserId
            };
        }

        /// <summary>
        /// Gets the base URL for the website
        /// </summary>
        /// <returns>Base URL</returns>
        private string GetBaseUrl()
        {
            var settings = Properties.Settings.Default;
            var host = settings.WebsiteHost ?? "localhost";
            var port = settings.WebsitePort ?? "5077";
            return $"http://{host}:{port}";
        }

        /// <summary>
        /// Changes an asset's ID to a custom value
        /// </summary>
        public async Task<long> ChangeAssetIdAsync(string connectionString, long currentAssetId, long newAssetId)
        {
            return await _managementService.ChangeAssetIdAsync(connectionString, currentAssetId, newAssetId);
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
            return await _managementService.UploadImageBasedAssetAsync(
                selectedFilePath,
                assetName,
                assetIdText,
                robuxPriceText,
                tixPriceText,
                assetTypeText,
                putOnSale,
                ownerUserId);
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
            return _managementService.ValidateAssetUpload(
                filePath,
                assetName,
                assetIdText,
                robuxPriceText,
                tixPriceText,
                assetTypeText);
        }

        /// <summary>
        /// Searches assets with filtering and sorting options
        /// </summary>
        public async Task<List<AssetSearchResult>> SearchAssetsAsync(AssetSearchParameters searchParams)
        {
            var universalParams = new Assets.AssetSearchParameters
            {
                SearchQuery = searchParams.SearchQuery,
                AssetIdQuery = searchParams.AssetIdQuery,
                AssetTypeFilter = searchParams.AssetTypeFilter,
                DateFilter = searchParams.DateFilter,
                SortFilter = searchParams.SortFilter,
                Limit = searchParams.Limit,
                Offset = searchParams.Offset
            };
            
            var universalResults = await _managementService.SearchAssetsAsync(universalParams);
            return universalResults.Select(u => new AssetSearchResult
            {
                Id = u.Id,
                Name = u.Name,
                Description = u.Description,
                AssetTypeId = u.AssetTypeId,
                Type = u.Type,
                Price = u.Price,
                TixPrice = u.TixPrice,
                Creator = u.Creator,
                CreatorId = u.CreatorId,
                UpdatedTime = u.UpdatedTime,
                ThumbnailUrl = u.ThumbnailUrl,
                HighResThumbnailUrl = u.HighResThumbnailUrl,
                IsLimited = u.IsLimited,
                PutOnSale = u.PutOnSale,
                LimitedQuantity = u.LimitedQuantity,
                LimitedRemaining = u.LimitedRemaining,
                LimitedUntil = u.LimitedUntil
            }).ToList();
        }

        /// <summary>
        /// Gets a single asset by ID for management
        /// </summary>
        /// <param name="assetId">Asset ID</param>
        /// <returns>Asset search result or null if not found</returns>
        public async Task<AssetSearchResult?> GetAssetByIdAsync(long assetId)
        {
            var universalAsset = await _managementService.GetAssetByIdAsync(assetId);
            if (universalAsset == null)
                return null;
                
            return new AssetSearchResult
            {
                Id = universalAsset.Id,
                Name = universalAsset.Name,
                Description = universalAsset.Description,
                AssetTypeId = universalAsset.AssetTypeId,
                Type = universalAsset.Type,
                Price = universalAsset.Price,
                TixPrice = universalAsset.TixPrice,
                Creator = universalAsset.Creator,
                CreatorId = universalAsset.CreatorId,
                UpdatedTime = universalAsset.UpdatedTime,
                ThumbnailUrl = universalAsset.ThumbnailUrl,
                HighResThumbnailUrl = universalAsset.HighResThumbnailUrl,
                IsLimited = universalAsset.IsLimited,
                PutOnSale = universalAsset.PutOnSale,
                LimitedQuantity = universalAsset.LimitedQuantity,
                LimitedRemaining = universalAsset.LimitedRemaining,
                LimitedUntil = universalAsset.LimitedUntil
            };
        }


        /// <summary>
        /// Updates an existing asset in the database
        /// </summary>
        /// <param name="asset">The asset data to update</param>
        /// <returns>True if update was successful, false otherwise</returns>
        public async Task<bool> UpdateAssetAsync(AssetSearchResult asset)
        {
            if (asset == null)
                throw new ArgumentNullException(nameof(asset));

            var universalAsset = new AssetUpdateData
            {
                Id = asset.Id,
                Name = asset.Name,
                Description = asset.Description,
                RobuxPrice = ParsePrice(asset.Price),
                TixPrice = ParseTixPrice(asset.TixPrice),
                PutOnSale = asset.PutOnSale,
                IsLimited = asset.IsLimited,
                LimitedQuantity = asset.LimitedQuantity,
                LimitedRemaining = asset.LimitedRemaining,
                LimitedUntil = asset.LimitedUntil
            };

            return await _managementService.UpdateAssetAsync(universalAsset);
        }

        /// <summary>
        /// Parses price string to long value
        /// </summary>
        private long ParsePrice(string price)
        {
            if (string.IsNullOrWhiteSpace(price) || 
                price.Equals("Free", StringComparison.OrdinalIgnoreCase) ||
                price.Equals("Not for sale", StringComparison.OrdinalIgnoreCase))
                return 0;
                
            return long.TryParse(price, out var result) ? result : 0;
        }

        /// <summary>
        /// Parses Tix price string to long value
        /// </summary>
        private long ParseTixPrice(string tixPrice)
        {
            if (string.IsNullOrWhiteSpace(tixPrice) || 
                tixPrice.Equals("Free", StringComparison.OrdinalIgnoreCase) ||
                tixPrice.Equals("Not for sale", StringComparison.OrdinalIgnoreCase))
                return 0;
                
            return long.TryParse(tixPrice.Replace("Tix", "").Trim(), out var result) ? result : 0;
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
            return await _managementService.ReplaceAssetAsync(assetId, newFilePath, assetName);
        }
    }
}

