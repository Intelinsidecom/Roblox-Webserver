using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Threading.Tasks;
using Games;
using Common;
using Users;
using Api.Services;

namespace Api.Controllers
{
    [ApiController]
    [Route("v1/developer-products")]
    [Route("developerproducts")]
    public class DeveloperProductsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly CurrentUserService _currentUserService;

        public DeveloperProductsController(IConfiguration configuration, CurrentUserService currentUserService)
        {
            _configuration = configuration;
            _currentUserService = currentUserService;
        }


        [HttpGet("list")]
        public async Task<IActionResult> GetDeveloperProductsList([FromQuery] long page = 1, [FromQuery] long? placeId = null, [FromQuery] long? universeId = null)
        {
            try
            {
                if (page < 1 || page > 5)
                {
                    page = 1;
                }

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);

                if (universeId is null && placeId is not null)
                {
                    var resolved = await GamesRepository.GetUniverseIdFromPlaceIdAsync(connectionString, placeId.Value);
                    if (!resolved.HasValue)
                    {
                        return Ok(new
                        {
                            FinalPage = true,
                            DeveloperProducts = new object[] { },
                            PageSize = 0
                        });
                    }
                    universeId = resolved.Value;
                }
                else if (universeId is null)
                {
                    return BadRequest(new { error = "You must provide a valid placeId or universeId." });
                }

                var (products, totalCount) = await DevProductHandler.GetUniverseDeveloperProductsPaginatedAsync(connectionString, universeId.Value, (int)page, 5);
                var productList = new System.Collections.Generic.List<object>();

                if (products != null)
                {
                    foreach (var product in products)
                    {
                        try
                        {
                            var productId = product.TryGetProperty("developerProductId", out var idElement)
                                ? idElement.GetInt64() : 0;
                            var name = product.TryGetProperty("name", out var nameElement)
                                ? nameElement.GetString() : "";
                            var description = product.TryGetProperty("description", out var descElement)
                                ? descElement.GetString() : "";

                            long? iconImageAssetId = null;
                            if (product.TryGetProperty("imageAssetId", out var imgElement) &&
                                imgElement.ValueKind != JsonValueKind.Null)
                            {
                                iconImageAssetId = imgElement.GetInt64();
                            }

                            var priceInRobux = product.TryGetProperty("priceInRobux", out var robuxElement)
                                ? robuxElement.GetInt32() : 0;

                            productList.Add(new
                            {
                                ProductId = productId,
                                DeveloperProductId = productId,
                                Name = name,
                                Description = description,
                                IconImageAssetId = iconImageAssetId ?? 0,
                                displayName = name,
                                displayDescription = description,
                                displayIcon = (int?)null,
                                PriceInRobux = priceInRobux,
                            });
                        }
                        catch
                        {
                        }
                    }
                }

                return Ok(new
                {
                    FinalPage = productList.Count < 5 || page >= 5,
                    DeveloperProducts = productList,
                    PageSize = productList.Count
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    FinalPage = true,
                    DeveloperProducts = new object[] { },
                    PageSize = 0,
                    error = ex.Message
                });
            }
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddDeveloperProduct([FromQuery] long universeId)
        {
            try
            {
                var userId = await _currentUserService.GetUserIdAsync();
                if (userId <= 0)
                    return Ok(new { success = false, message = "Authentication required" });

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var ownerId = await GamesRepository.GetUniverseOwnerAsync(connectionString, universeId);
                if (ownerId == null || ownerId.Value != userId)
                    return Ok(new { success = false, message = "You do not own this universe" });

                using var reader = new System.IO.StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();
                var data = JsonSerializer.Deserialize<System.Text.Json.JsonElement>(body);

                var name = StringUtilities.SanitizeString(data.TryGetProperty("Name", out var nameEl) ? nameEl.GetString() ?? "" : "", 100);
                var description = StringUtilities.SanitizeString(data.TryGetProperty("Description", out var descEl) ? descEl.GetString() ?? "" : "", 1000);
                var priceInRobuxStr = data.TryGetProperty("PriceInRobux", out var priceEl) ? priceEl.GetString() : "0";
                if (!int.TryParse(priceInRobuxStr, out var priceInRobux) || priceInRobux < 1)
                    return Ok(new { success = false, message = "PriceInRobux must be a positive integer" });
                if (priceInRobux > 1000000)
                    return Ok(new { success = false, message = "PriceInRobux cannot exceed 1,000,000" });
                if (string.IsNullOrWhiteSpace(name))
                    return Ok(new { success = false, message = "Product name is required" });

                var productId = await GamesRepository.GenerateUniverseDeveloperProductIdAsync(connectionString);

                var developerProduct = new
                {
                    developerProductId = productId,
                    universeId = universeId,
                    name = name,
                    description = description,
                    priceInRobux = priceInRobux,
                    priceInTix = 0,
                    imageAssetId = (long?)null,
                    createdAt = DateTime.UtcNow
                };

                var developerProductJson = JsonSerializer.SerializeToElement(developerProduct);
                var addedToUniverse = await GamesRepository.AddDeveloperProductToUniverseAsync(
                    connectionString, universeId, developerProductJson);

                if (!addedToUniverse)
                    return Ok(new { success = false, message = "Failed to add developer product" });

                try
                {
                    await DevProductHandler.CreateDeveloperProduct(
                        connectionString, universeId, name, description, priceInRobux, 0, null);
                }
                catch { }

                return Ok(new { success = true, productId = productId });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateDeveloperProduct([FromQuery] long universeId)
        {
            try
            {
                var userId = await _currentUserService.GetUserIdAsync();
                if (userId <= 0)
                    return Ok(new { success = false, message = "Authentication required" });

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var ownerId = await GamesRepository.GetUniverseOwnerAsync(connectionString, universeId);
                if (ownerId == null || ownerId.Value != userId)
                    return Ok(new { success = false, message = "You do not own this universe" });

                using var reader = new System.IO.StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();
                var data = JsonSerializer.Deserialize<System.Text.Json.JsonElement>(body);

                var productIdStr = data.TryGetProperty("ProductId", out var idEl) ? idEl.GetString() : "";
                long.TryParse(productIdStr, out var productId);
                if (productId <= 0)
                    return Ok(new { success = false, message = "Invalid product ID" });

                var name = StringUtilities.SanitizeString(data.TryGetProperty("Name", out var nameEl) ? nameEl.GetString() ?? "" : "", 100);
                var description = StringUtilities.SanitizeString(data.TryGetProperty("Description", out var descEl) ? descEl.GetString() ?? "" : "", 1000);
                var priceInRobuxStr = data.TryGetProperty("PriceInRobux", out var priceEl) ? priceEl.GetString() : "0";
                if (!int.TryParse(priceInRobuxStr, out var priceInRobux) || priceInRobux < 1)
                    return Ok(new { success = false, message = "PriceInRobux must be a positive integer" });
                if (priceInRobux > 1000000)
                    return Ok(new { success = false, message = "PriceInRobux cannot exceed 1,000,000" });
                if (string.IsNullOrWhiteSpace(name))
                    return Ok(new { success = false, message = "Product name is required" });
                var iconStr = data.TryGetProperty("IconImageAssetId", out var iconEl) ? iconEl.GetString() : "";
                long.TryParse(iconStr, out var iconId);

                long? imageAssetId = iconId > 0 ? iconId : null;

                var updatedInUniverse = await DevProductHandler.UpdateDeveloperProductInUniverseAsync(
                    connectionString, universeId, productId, name, description, priceInRobux, 0, imageAssetId);

                if (!updatedInUniverse)
                    return Ok(new { success = false, message = "Failed to update developer product in universe" });

                try
                {
                    await DevProductHandler.UpdateDeveloperProductInDatabaseAsync(
                        connectionString, productId, name, description, priceInRobux, 0, imageAssetId);
                }
                catch { }

                return Ok(new { success = true, productId = productId });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetDeveloperProduct(long productId)
        {
            try
            {
                if (productId <= 0)
                {
                    return BadRequest(new { error = "Invalid product ID" });
                }

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);

                using var conn = new Npgsql.NpgsqlConnection(connectionString);
                await conn.OpenAsync();

                const string sql = @"
                    SELECT id, universe_id, name, description, price_in_robux, price_in_tix, image_asset_id 
                    FROM developer_products 
                    WHERE id = @productId";

                using var cmd = new Npgsql.NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("productId", productId);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var id = reader.GetInt64(0);
                    var universeId = reader.GetInt64(1);
                    var name = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    var description = reader.IsDBNull(3) ? "" : reader.GetString(3);
                    var priceInRobux = reader.IsDBNull(4) ? 0 : reader.GetInt32(4);
                    var priceInTix = reader.IsDBNull(5) ? 0 : reader.GetInt32(5);
                    long? imageAssetId = reader.IsDBNull(6) ? null : reader.GetInt64(6);

                    return Ok(new
                    {
                        ProductId = id,
                        DeveloperProductId = id,
                        Name = name,
                        Description = description,
                        PriceInRobux = priceInRobux,
                        PriceInTix = priceInTix,
                        ImageAssetId = imageAssetId,
                        UniverseId = universeId,
                        IconImageAssetId = imageAssetId ?? 0
                    });
                }

                return NotFound(new { error = "Developer product not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
            }
        }
    }
}
