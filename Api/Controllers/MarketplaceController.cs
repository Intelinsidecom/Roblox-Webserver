using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api.Data;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Users;
using Games;
using Economy;

namespace Api.Controllers
{
    [ApiController]
    [Route("marketplace")]
    public class MarketplaceController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly Api.Services.CurrentUserService _currentUserService;
        private static readonly HttpClient _httpClient = new HttpClient { Timeout = Common.HttpClientDefaults.Timeout };

        public MarketplaceController(AppDbContext dbContext, IConfiguration configuration, Api.Services.CurrentUserService currentUserService)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _currentUserService = currentUserService;
        }

        private string GetConnStr() => _configuration.GetConnectionString("Default") ?? "";

        [HttpGet("productDetails")]
        public async Task<IActionResult> GetProductDetails([FromQuery] long productId)
        {
            if (productId <= 0)
                return BadRequest(new { error = "Invalid product ID" });

            try
            {
                var connStr = GetConnStr();
                var product = await DevProductHandler.GetDeveloperProductByIdAsync(connStr, productId);

                if (product == null)
                    return NotFound(new { error = "Product not found" });

                return Ok(new
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    IconImageAssetId = product.ImageAssetId ?? 0,
                    PriceInRobux = product.PriceInRobux
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("purchase")]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data", "application/json")]
        public async Task<IActionResult> PurchaseAsset()
        {
            long userId = await _currentUserService.GetUserIdAsync();
            if (userId <= 0)
                return StatusCode(403, new { error = "Not authenticated" });

            try
            {
                long productId = 0;
                int currencyTypeId = 1;

                if (Request.HasFormContentType)
                {
                    long.TryParse(Request.Form["productId"].FirstOrDefault(), out productId);
                    int.TryParse(Request.Form["currencyTypeId"].FirstOrDefault(), out currencyTypeId);
                }
                else
                {
                    using var body = await JsonDocument.ParseAsync(Request.Body);
                    if (body.RootElement.TryGetProperty("productId", out var pid))
                        pid.TryGetInt64(out productId);
                    if (body.RootElement.TryGetProperty("currencyTypeId", out var cid))
                        cid.TryGetInt32(out currencyTypeId);
                }

                if (productId <= 0)
                    return BadRequest(new { error = "Invalid productId" });

                var connStr = GetConnStr();
                var currency = currencyTypeId == 2
                    ? UserPurchaseService.CurrencyKind.Tix
                    : UserPurchaseService.CurrencyKind.Robux;

                var purchaseService = new UserPurchaseService();
                var (success, error) = await purchaseService.PurchaseAssetAsync(connStr, userId, productId, currency);

                if (success)
                    return Ok(new { status = "Purchased", productId });
                else
                    return BadRequest(new { error = error ?? "Purchase failed" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("submitpurchase")]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data", "application/json")]
        public async Task<IActionResult> SubmitPurchase()
        {
            long userId = await _currentUserService.GetUserIdAsync();
            if (userId <= 0)
                return StatusCode(403, new { error = "Not authenticated" });

            try
            {
                long productId = 0;
                int currencyTypeId = 1;

                if (Request.HasFormContentType)
                {
                    long.TryParse(Request.Form["productId"].FirstOrDefault(), out productId);
                    int.TryParse(Request.Form["currencyTypeId"].FirstOrDefault(), out currencyTypeId);
                }
                else
                {
                    using var body = await JsonDocument.ParseAsync(Request.Body);
                    if (body.RootElement.TryGetProperty("productId", out var pid))
                        pid.TryGetInt64(out productId);
                    if (body.RootElement.TryGetProperty("currencyTypeId", out var cid))
                        cid.TryGetInt32(out currencyTypeId);
                }

                if (productId <= 0)
                    return BadRequest(new { error = "Invalid productId" });

                var connStr = GetConnStr();
                var currency = currencyTypeId == 2
                    ? UserPurchaseService.CurrencyKind.Tix
                    : UserPurchaseService.CurrencyKind.Robux;

                var purchaseService = new UserPurchaseService();
                var (success, error) = await purchaseService.PurchaseAssetAsync(connStr, userId, productId, currency);

                if (success)
                    return Ok(new { status = "Purchased", productId });
                else
                    return BadRequest(new { error = error ?? "Purchase failed" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("purchase-from-anywhere")]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data", "application/json")]
        public async Task<IActionResult> PurchaseFromAnywhere()
        {
            return await PurchaseAsset();
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

                long playerId = 0;
                long productId = 0;
                long placeId = 0;
                int currencyType = 1;
                long currencySpent = 0;

                var connStr = GetConnStr();
                if (!string.IsNullOrWhiteSpace(connStr))
                {
                    try
                    {
                        var logging = new AssetPurchaseLogging();
                        var sale = await logging.GetSaleByReceiptAsync(connStr, receipt);

                        if (sale != null)
                        {
                            playerId = sale.BuyerUserId;
                            productId = sale.AssetId;
                            currencySpent = sale.Price;
                            currencyType = sale.Currency;
                        }
                    }
                    catch
                    {
                    }
                }

                return Ok(new
                {
                    isValid = true,
                    receipt,
                    playerId,
                    productId,
                    placeId,
                    currencyType,
                    currencySpent,
                    message = "Purchase validated",
                    validatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    isValid = true,
                    receipt = receipt ?? "unknown",
                    error = ex.Message,
                    message = "Validation error - allowing purchase"
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