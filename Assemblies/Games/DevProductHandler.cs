using System;
using System.Data;
using Npgsql;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Text.Json;
using Common;

namespace Games
{
    /// <summary>
    /// Handles database operations for developer products and their assets
    /// </summary>
    public static class DevProductHandler
    {
        /// <summary>
        /// Processes and saves a developer product image
        /// </summary>
        public static async Task<(string imageUrl, string fileHash)> ProcessDeveloperProductImageAsync(
            Stream fileStream,
            string fileName,
            string contentType,
            string baseUrl)
        {
            if (fileStream == null || fileStream.Length == 0)
                throw new ArgumentException("No file uploaded", nameof(fileStream));

            if (!contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Invalid file type. Please upload an image file.", nameof(contentType));

            // Process the uploaded image
            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();

            // Generate hash for the uploaded file
            var fileHash = HashingUtilities.GenerateFileHash(fileBytes);

            // Resize to 256x256
            var resized256 = ResizeImage(fileBytes, 256, 256);

            // Save file to CDN dev-product-icons directory
            var imageFileName = $"{fileHash}.png";
            CDNUtilities.SaveToCDN("dev-product-icons", imageFileName, resized256);

            // Generate CDN URL
            var imageUrl = $"{baseUrl.TrimEnd('/')}/dev-product-icons/{imageFileName}";

            return (imageUrl, fileHash);
        }

        /// <summary>
        /// Resizes an image to the specified dimensions
        /// </summary>
        private static byte[] ResizeImage(byte[] imageBytes, int width, int height)
        {
            using var inputStream = new MemoryStream(imageBytes);
            using var outputStream = new MemoryStream();
            
            using var image = System.Drawing.Image.FromStream(inputStream);
            
            // Calculate aspect ratio
            var aspectRatio = (double)image.Width / image.Height;
            var newWidth = width;
            var newHeight = height;
            
            if (aspectRatio > 1) // Landscape
            {
                newHeight = (int)(width / aspectRatio);
            }
            else // Portrait or square
            {
                newWidth = (int)(height * aspectRatio);
            }
            
            using var resizedImage = new System.Drawing.Bitmap(newWidth, newHeight);
            using var graphics = System.Drawing.Graphics.FromImage(resizedImage);
            
            graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            
            graphics.DrawImage(image, 0, 0, newWidth, newHeight);
            
            resizedImage.Save(outputStream, System.Drawing.Imaging.ImageFormat.Png);
            return outputStream.ToArray();
        }

        /// <summary>
        /// Creates an asset record for a developer product image
        /// </summary>
        public static async Task<long> CreateDeveloperProductImageAsset(string connectionString, string fileName, string imageUrl, string fileHash, long createdBy)
        {
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            // For now, we'll create a temporary record without a developer_product_id
            // The product_id will be updated when the developer product is created
            var sql = @"
                INSERT INTO developer_product_assets (developer_product_id, asset_name, asset_type, file_name, content_hash, 
                                                   thumbnail_url, created_by)
                VALUES (NULL, @assetName, @assetType, @fileName, @contentHash, 
                        @thumbnailUrl, @createdBy)
                RETURNING id";

            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@assetName", fileName);
            command.Parameters.AddWithValue("@assetType", "Image");
            command.Parameters.AddWithValue("@fileName", fileName);
            command.Parameters.AddWithValue("@contentHash", fileHash);
            command.Parameters.AddWithValue("@thumbnailUrl", imageUrl);
            command.Parameters.AddWithValue("@createdBy", createdBy);

            var assetId = await command.ExecuteScalarAsync();
            return Convert.ToInt64(assetId);
        }

        /// <summary>
        /// Updates a developer product asset to link it to a developer product
        /// </summary>
        public static async Task UpdateDeveloperProductAssetLink(string connectionString, long assetId, long developerProductId)
        {
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            var sql = @"
                UPDATE developer_product_assets 
                SET developer_product_id = @developerProductId, updated_at = CURRENT_TIMESTAMP
                WHERE id = @assetId";

            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@developerProductId", developerProductId);
            command.Parameters.AddWithValue("@assetId", assetId);

            await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Creates a developer product record
        /// </summary>
        public static async Task<long> CreateDeveloperProduct(string connectionString, long universeId, string name, string description, int priceInRobux, int priceInTix, long? imageAssetId)
        {
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            var sql = @"
                INSERT INTO developer_products (universe_id, name, description, price_in_robux, price_in_tix, image_asset_id)
                VALUES (@universeId, @name, @description, @priceInRobux, @priceInTix, @imageAssetId)
                RETURNING id";

            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@universeId", universeId);
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@description", string.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description);
            command.Parameters.AddWithValue("@priceInRobux", priceInRobux);
            command.Parameters.AddWithValue("@priceInTix", priceInTix);
            command.Parameters.AddWithValue("@imageAssetId", imageAssetId ?? (object)DBNull.Value);

            var productId = await command.ExecuteScalarAsync();
            return Convert.ToInt64(productId);
        }

        /// <summary>
        /// Gets all developer products for a place
        /// </summary>
        public static async Task<System.Collections.Generic.List<DeveloperProduct>> GetDeveloperProductsByUniverse(string connectionString, long universeId)
        {
            var products = new System.Collections.Generic.List<DeveloperProduct>();

            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            var sql = @"
                SELECT dp.id, dp.universe_id, dp.name, dp.description, dp.price_in_robux, dp.price_in_tix, dp.image_asset_id, 
                       dp.created_at, dp.updated_at,
                       dpa.thumbnail_url as image_url
                FROM developer_products dp
                LEFT JOIN developer_product_assets dpa ON dp.image_asset_id = dpa.id
                WHERE dp.universe_id = @universeId
                ORDER BY dp.created_at DESC";

            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@universeId", universeId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                products.Add(new DeveloperProduct
                {
                    Id = reader.GetInt64(reader.GetOrdinal("id")),
                    UniverseId = reader.GetInt64(reader.GetOrdinal("universe_id")),
                    Name = reader.GetString(reader.GetOrdinal("name")),
                    Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description")),
                    PriceInRobux = reader.GetInt32(reader.GetOrdinal("price_in_robux")),
                    PriceInTix = reader.GetInt32(reader.GetOrdinal("price_in_tix")),
                    ImageAssetId = reader.IsDBNull(reader.GetOrdinal("image_asset_id")) ? null : (long?)reader.GetInt64(reader.GetOrdinal("image_asset_id")),
                    ImageUrl = reader.IsDBNull(reader.GetOrdinal("image_url")) ? null : reader.GetString(reader.GetOrdinal("image_url")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                    UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at"))
                });
            }

            return products;
        }

        /// <summary>
        /// Checks if a developer product name is unique within a universe.
        /// Optionally excludes a specific product ID (for update scenarios).
        /// </summary>
        public static async Task<bool> IsDeveloperProductNameUniqueAsync(string connectionString, long universeId, string name, long? productIdToExclude = null)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (universeId <= 0)
                throw new ArgumentOutOfRangeException(nameof(universeId));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("name is required", nameof(name));

            using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();

            const string sql = @"SELECT developer_products 
                                   FROM universes 
                                   WHERE universe_id = @universeId";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("universeId", universeId);

            var result = await cmd.ExecuteScalarAsync();
            
            if (result == null || result == DBNull.Value)
                return true; // no products yet, so name is unique

            var developerProductsJson = result.ToString();
            if (string.IsNullOrWhiteSpace(developerProductsJson) || developerProductsJson == "[]")
                return true;

            var allDeveloperProducts = JsonSerializer.Deserialize<List<JsonElement>>(developerProductsJson);
            if (allDeveloperProducts == null || allDeveloperProducts.Count == 0)
                return true;

            var targetName = name.Trim();

            foreach (var product in allDeveloperProducts)
            {
                try
                {
                    if (productIdToExclude.HasValue &&
                        product.TryGetProperty("developerProductId", out var idElement) &&
                        idElement.ValueKind != JsonValueKind.Null &&
                        idElement.GetInt64() == productIdToExclude.Value)
                    {
                        continue;
                    }

                    if (!product.TryGetProperty("name", out var nameElement) || nameElement.ValueKind == JsonValueKind.Null)
                        continue;

                    var existingName = nameElement.GetString();
                    if (existingName == null)
                        continue;

                    if (string.Equals(existingName.Trim(), targetName, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
                catch
                {
                    // Ignore malformed entries and continue checking others
                }
            }

            return true;
        }

        /// <summary>
        /// Gets paginated developer products for a universe using the JSON-based universe storage
        /// </summary>
        public static async Task<(List<JsonElement>? products, int totalCount)> GetUniverseDeveloperProductsPaginatedAsync(string connectionString, long universeId, int page = 1, int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (universeId <= 0)
                throw new ArgumentOutOfRangeException(nameof(universeId));
            if (page < 1)
                throw new ArgumentOutOfRangeException(nameof(page));
            if (pageSize < 1)
                throw new ArgumentOutOfRangeException(nameof(pageSize));

            using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();

            const string sql = @"SELECT developer_products 
                                   FROM universes 
                                   WHERE universe_id = @universeId";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("universeId", universeId);

            var result = await cmd.ExecuteScalarAsync();
            
            if (result == null || result == DBNull.Value)
                return (new List<JsonElement>(), 0);

            var developerProductsJson = result.ToString();
            if (string.IsNullOrWhiteSpace(developerProductsJson) || developerProductsJson == "[]")
                return (new List<JsonElement>(), 0);

            var allDeveloperProducts = JsonSerializer.Deserialize<List<JsonElement>>(developerProductsJson);
            if (allDeveloperProducts == null)
                return (new List<JsonElement>(), 0);

            var totalCount = allDeveloperProducts.Count;
            var skip = (page - 1) * pageSize;
            var pagedProducts = allDeveloperProducts.Skip(skip).Take(pageSize).ToList();

            return (pagedProducts, totalCount);
        }

        /// <summary>
        /// Updates an existing developer product in the universe storage
        /// </summary>
        public static async Task<bool> UpdateDeveloperProductInUniverseAsync(string connectionString, long universeId, long productId, string name, string description, int priceInRobux, int priceInTix, long? imageAssetId)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));
            if (universeId <= 0)
                throw new ArgumentOutOfRangeException(nameof(universeId));
            if (productId <= 0)
                throw new ArgumentOutOfRangeException(nameof(productId));

            using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();

            // Get current universe developer products
            const string getSql = @"SELECT developer_products 
                                   FROM universes 
                                   WHERE universe_id = @universeId";

            using var getCmd = new NpgsqlCommand(getSql, conn);
            getCmd.Parameters.AddWithValue("universeId", universeId);

            var result = await getCmd.ExecuteScalarAsync();
            
            if (result == null || result == DBNull.Value)
                return false;

            var developerProductsJson = result.ToString();
            if (string.IsNullOrWhiteSpace(developerProductsJson) || developerProductsJson == "[]")
                return false;

            var allDeveloperProducts = JsonSerializer.Deserialize<List<JsonElement>>(developerProductsJson);
            if (allDeveloperProducts == null)
                return false;

            // Find and update the specific product
            var productIndex = -1;
            for (int i = 0; i < allDeveloperProducts.Count; i++)
            {
                if (allDeveloperProducts[i].TryGetProperty("developerProductId", out var idElement) && 
                    idElement.ValueKind != JsonValueKind.Null && 
                    idElement.GetInt64() == productId)
                {
                    productIndex = i;
                    break;
                }
            }

            if (productIndex == -1)
                return false;

            // Create updated product object
            var existingProduct = allDeveloperProducts[productIndex];
            var updatedProduct = new
            {
                developerProductId = productId,
                universeId = universeId,
                name = name,
                description = description ?? "",
                priceInRobux = priceInRobux,
                priceInTix = priceInTix,
                imageAssetId = imageAssetId,
                creatorUserId = existingProduct.TryGetProperty("creatorUserId", out var creatorElement) && creatorElement.ValueKind != JsonValueKind.Null ? creatorElement.GetInt64() : 0,
                createdAt = existingProduct.TryGetProperty("createdAt", out var createdElement) && createdElement.ValueKind != JsonValueKind.Null ? createdElement.GetDateTime() : DateTime.UtcNow,
                updatedAt = DateTime.UtcNow,
                placeId = existingProduct.TryGetProperty("placeId", out var placeElement) && placeElement.ValueKind != JsonValueKind.Null ? placeElement.GetInt64() : 0
            };

            // Replace the old product with the updated one
            allDeveloperProducts[productIndex] = JsonSerializer.SerializeToElement(updatedProduct);

            // Update the universe storage
            const string updateSql = @"UPDATE universes 
                                       SET developer_products = @developerProducts::jsonb 
                                       WHERE universe_id = @universeId";

            using var updateCmd = new NpgsqlCommand(updateSql, conn);
            updateCmd.Parameters.AddWithValue("developerProducts", JsonSerializer.Serialize(allDeveloperProducts));
            updateCmd.Parameters.AddWithValue("universeId", universeId);

            var rowsAffected = await updateCmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        /// <summary>
        /// Updates the database record for a developer product
        /// </summary>
        public static async Task<bool> UpdateDeveloperProductInDatabaseAsync(string connectionString, long productId, string name, string description, int priceInRobux, int priceInTix, long? imageAssetId)
        {
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            var sql = @"
                UPDATE developer_products 
                SET name = @name, 
                    description = @description, 
                    price_in_robux = @priceInRobux, 
                    price_in_tix = @priceInTix, 
                    image_asset_id = @imageAssetId,
                    updated_at = CURRENT_TIMESTAMP
                WHERE id = @productId";

            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@description", string.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description);
            command.Parameters.AddWithValue("@priceInRobux", priceInRobux);
            command.Parameters.AddWithValue("@priceInTix", priceInTix);
            command.Parameters.AddWithValue("@imageAssetId", imageAssetId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@productId", productId);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        /// <summary>
        /// Gets developer product asset information from the database
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="assetId">The asset ID to retrieve</param>
        /// <returns>Tuple containing thumbnailUrl, fileName, contentHash, or null if not found</returns>
        public static async Task<(string thumbnailUrl, string fileName, string contentHash)?> GetDeveloperProductAssetAsync(string connectionString, long assetId)
        {
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            var sql = @"
                SELECT thumbnail_url, file_name, content_hash
                FROM developer_product_assets 
                WHERE id = @assetId";

            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@assetId", assetId);

            using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return null;
            }

            var thumbnailUrl = reader.GetString(reader.GetOrdinal("thumbnail_url"));
            var fileName = reader.GetString(reader.GetOrdinal("file_name"));
            var contentHash = reader.GetString(reader.GetOrdinal("content_hash"));

            return (thumbnailUrl, fileName, contentHash);
        }

        /// <summary>
        /// Extracts ticket price from a JsonElement representing a developer product
        /// </summary>
        public static int GetTicketPriceFromJson(JsonElement product)
        {
            if (product.TryGetProperty("priceInTix", out var ticketElement) && ticketElement.ValueKind != JsonValueKind.Null)
            {
                return ticketElement.GetInt32();
            }
            
            // Fallback to 0 if not found
            return 0;
        }

        /// <summary>
        /// Model representing a developer product
        /// </summary>
        public class DeveloperProduct
        {
            public long Id { get; set; }
            public long UniverseId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public int PriceInRobux { get; set; }
            public int PriceInTix { get; set; }
            public long? ImageAssetId { get; set; }
            public string ImageUrl { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }
    }
}