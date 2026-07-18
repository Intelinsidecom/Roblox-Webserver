using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api.Data;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("marketplace")]
    public class MarketplaceController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

        public MarketplaceController(AppDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        [HttpGet("productinfo")]
        public async Task<IActionResult> GetProductInfo(long assetId)
        {
            if (assetId <= 0)
            {
                return BadRequest(new { error = "Invalid asset ID" });
            }

            try
            {
                var connection = _dbContext.Database.GetDbConnection();
                await connection.OpenAsync();
                
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT 
                        a.asset_id as ""AssetId"",
                        a.name as ""Name"",
                        a.description as ""Description"",
                        a.asset_type_id as ""AssetTypeId"",
                        a.created_at as ""Created"",
                        a.last_updated as ""Updated"",
                        a.on_sale as ""IsForSale"",
                        a.price as ""Price"",
                        u.user_id as ""CreatorId"",
                        u.user_name as ""CreatorName""
                    FROM assets a
                    LEFT JOIN users u ON a.owner_user_id = u.user_id
                    WHERE a.asset_id = @assetId";
                
                var parameter = command.CreateParameter();
                parameter.ParameterName = "@assetId";
                parameter.Value = assetId;
                command.Parameters.Add(parameter);
                
                    using var reader = await command.ExecuteReaderAsync();
                    
                    if (await reader.ReadAsync())
                    {
                        var assetTypeId = reader.GetInt32(3);
                        var isForSale = reader.GetBoolean(6);
                        var price = reader.IsDBNull(7) ? (int?)null : reader.GetInt32(7);
                        var creatorId = reader.IsDBNull(8) ? 0 : reader.GetInt64(8);
                        var creatorName = reader.IsDBNull(9) ? "Unknown" : reader.GetString(9);
                        var assetIdResult = reader.GetInt64(0);
                        var name = reader.GetString(1);
                        var description = reader.IsDBNull(2) ? null : reader.GetString(2);
                        var created = reader.GetDateTime(4);
                        var updated = reader.GetDateTime(5);

                        reader.Close();

                        var limCmd = connection.CreateCommand();
                    limCmd.CommandText = @"SELECT limited_unique, limited_quantity, limited_remaining, limited_until, recent_average_price
                                           FROM assets WHERE asset_id = @assetId";
                    var limParam = limCmd.CreateParameter();
                    limParam.ParameterName = "@assetId";
                    limParam.Value = assetId;
                    limCmd.Parameters.Add(limParam);

                    bool isLimitedUnique = false;
                    bool isLimited = false;
                    long? limitedQuantity = null;
                    long? limitedRemaining = null;
                    long rap = 0;

                    using (var limReader = await limCmd.ExecuteReaderAsync())
                    {
                        if (await limReader.ReadAsync())
                        {
                            isLimitedUnique = !limReader.IsDBNull(0) && limReader.GetBoolean(0);
                            limitedQuantity = limReader.IsDBNull(1) ? null : (long?)limReader.GetInt64(1);
                            limitedRemaining = limReader.IsDBNull(2) ? null : (long?)limReader.GetInt64(2);
                            rap = limReader.IsDBNull(4) ? 0 : limReader.GetInt64(4);
                            isLimited = isLimitedUnique || (limitedQuantity.HasValue && limitedQuantity.Value > 0);
                        }
                    }

                    dynamic result = new
                    {
                        AssetId = assetIdResult,
                        Name = name,
                        Description = description,
                        AssetTypeId = assetTypeId,
                        Created = created,
                        Updated = updated,
                        IsForSale = isForSale,
                        Price = price,
                        Creator = new
                        {
                            Id = creatorId,
                            Name = creatorName,
                            Type = "User"
                        },
                        IsLimited = isLimited,
                        IsLimitedUnique = isLimitedUnique,
                        Quantity = limitedQuantity,
                        Remaining = limitedRemaining,
                        OriginalPrice = price,
                        RAP = rap
                    };

                    if (result.AssetTypeId == 9)
                    {
                        using var placeCommand = connection.CreateCommand();
                        placeCommand.CommandText = @"
                            SELECT 
                                u.universe_id,
                                a.max_visitor_count
                            FROM assets a
                            LEFT JOIN universes u ON a.asset_id = ANY(u.place_ids)
                            WHERE a.asset_id = @assetId";
                        
                        var placeParam = placeCommand.CreateParameter();
                        placeParam.ParameterName = "@assetId";
                        placeParam.Value = assetId;
                        placeCommand.Parameters.Add(placeParam);
                        
                        using var placeReader = await placeCommand.ExecuteReaderAsync();
                        if (await placeReader.ReadAsync())
                        {
                            var universeId = placeReader.IsDBNull(0) ? 0 : placeReader.GetInt64(0);
                            var maxPlayers = placeReader.IsDBNull(1) ? 0 : placeReader.GetInt32(1);
                            
                            placeReader.Close();

                            var arbiterUrl = _configuration["ArbiterUrl"] ?? "http://localhost:5000";
                            var playing = 0;

                            try
                            {
                                var response = await _httpClient.GetAsync($"{arbiterUrl}/api/gameservers/players/{assetId}?live=false");
                                if (response.IsSuccessStatusCode)
                                {
                                    var content = await response.Content.ReadAsStringAsync();
                                    using var doc = JsonDocument.Parse(content);
                                    var root = doc.RootElement;
                                    playing = root.TryGetProperty("totalPlayerCount", out var count) ? count.GetInt32() : 0;
                                }
                            }
                            catch
                            {
                                playing = 0;
                            }

                            result = new
                            {
                                result.AssetId,
                                result.Name,
                                result.Description,
                                result.AssetTypeId,
                                result.Created,
                                result.Updated,
                                result.IsForSale,
                                result.Price,
                                result.Creator,
                                UniverseId = universeId,
                                MaxPlayers = maxPlayers,
                                Visits = 0,
                                Favorites = 0,
                                Playing = playing
                            };
                        }
                    }

                    var jsonOptions = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = null
                    };
                    
                    return new JsonResult(result, jsonOptions);
                }
                else
                {
                    return NotFound(new { error = "Asset not found" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// POST /marketplace/validatepurchase?receipt={receipt}
        /// Validates a purchase receipt/ticket
        /// Called by MarketplaceService to verify client purchase claims
        /// </summary>
        [HttpPost("validatepurchase")]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data", "application/json")]
        public async Task<IActionResult> ValidatePurchase([FromQuery] string receipt)
        {
            try
            {
                if (string.IsNullOrEmpty(receipt) && Request.HasFormContentType)
                {
                    receipt = Request.Form["receipt"].FirstOrDefault();
                }

                if (string.IsNullOrEmpty(receipt))
                {
                    return BadRequest(new { error = "Receipt parameter is required" });
                }

                var response = new
                {
                    isValid = true,
                    receipt = receipt,
                    playerId = 0,
                    productId = 0,
                    placeId = 0,
                    currencyType = 1, // Robux
                    currencySpent = 0,
                    message = "Purchase validated (placeholder)",
                    validatedAt = DateTime.UtcNow
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    isValid = true,
                    receipt = receipt ?? "unknown",
                    error = ex.Message,
                    message = "Validation error - allowing purchase (placeholder)"
                });
            }
        }

        /// <summary>
        /// GET /marketplace/validatepurchase
        /// Alternative GET endpoint for validating purchases
        /// </summary>
        [HttpGet("validatepurchase")]
        public async Task<IActionResult> ValidatePurchaseGet([FromQuery] string receipt)
        {
            return await ValidatePurchase(receipt);
        }

        /// <summary>
        /// POST /marketplace/validatepurchaseticket
        /// Alternative endpoint name used by some client versions
        /// </summary>
        [HttpPost("validatepurchaseticket")]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data", "application/json")]
        public async Task<IActionResult> ValidatePurchaseTicket()
        {
            string receipt = null;
            
            if (Request.HasFormContentType)
            {
                receipt = Request.Form["receipt"].FirstOrDefault();
            }
            else
            {
                receipt = Request.Query["receipt"].FirstOrDefault();
            }

            return await ValidatePurchase(receipt);
        }
    }
}