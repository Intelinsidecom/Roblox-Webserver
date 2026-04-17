using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Threading.Tasks;
using Games;
using Common;

namespace Api.Controllers
{
    /// <summary>
    /// API endpoints for developer products - used by in-game MarketplaceService
    /// </summary>
    [ApiController]
    [Route("developerproducts")]
    public class DeveloperProductsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public DeveloperProductsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// GET /developerproducts/list?placeid={placeId}
        /// Returns developer products for a place in the format expected by StandardPages
        /// </summary>
        [HttpGet("list")]
        public async Task<IActionResult> GetDeveloperProductsList([FromQuery] long placeid)
        {
            try
            {
                if (placeid <= 0)
                {
                    return BadRequest(new { error = "Invalid place ID" });
                }

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var universeId = await GamesRepository.GetUniverseIdFromPlaceIdAsync(connectionString, placeid);
                if (!universeId.HasValue)
                {
                    return Ok(new
                    {
                        DeveloperProducts = new object[] { },
                        TotalCount = 0,
                        PlaceId = placeid
                    });
                }

                var products = await GamesRepository.GetUniverseDeveloperProductsAsync(connectionString, universeId.Value);
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
                            var priceInRobux = product.TryGetProperty("priceInRobux", out var robuxElement) 
                                ? robuxElement.GetInt32() : 0;
                            var priceInTix = product.TryGetProperty("priceInTix", out var tixElement) 
                                ? tixElement.GetInt32() : 0;
                            
                            long? imageAssetId = null;
                            if (product.TryGetProperty("imageAssetId", out var imgElement) && 
                                imgElement.ValueKind != JsonValueKind.Null)
                            {
                                imageAssetId = imgElement.GetInt64();
                            }

                            productList.Add(new
                            {
                                ProductId = productId,
                                DeveloperProductId = productId,
                                Name = name,
                                Description = description,
                                PriceInRobux = priceInRobux,
                                PriceInTix = priceInTix,
                                ImageAssetId = imageAssetId,
                                UniverseId = universeId.Value,
                                PlaceId = placeid,
                                IconImageAssetId = imageAssetId ?? 0
                            });
                        }
                        catch
                        {
                        }
                    }
                }

                return Ok(new
                {
                    data = productList,
                    DeveloperProducts = productList,
                    TotalCount = productList.Count,
                    PlaceId = placeid,
                    UniverseId = universeId.Value
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    data = new object[] { },
                    DeveloperProducts = new object[] { },
                    TotalCount = 0,
                    PlaceId = placeid,
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// GET /developerproducts/{productId}
        /// Get a single developer product by ID
        /// </summary>
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
