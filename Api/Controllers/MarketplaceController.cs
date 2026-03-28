using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api.Data;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("marketplace")]
    public class MarketplaceController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public MarketplaceController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
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
                    dynamic result = new
                    {
                        AssetId = reader.GetInt64(0),
                        Name = reader.GetString(1),
                        Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                        AssetTypeId = reader.GetInt32(3),
                        Created = reader.GetDateTime(4),
                        Updated = reader.GetDateTime(5),
                        IsForSale = reader.GetBoolean(6),
                        Price = reader.IsDBNull(7) ? (int?)null : reader.GetInt32(7),
                        Creator = new
                        {
                            Id = reader.IsDBNull(8) ? 0 : reader.GetInt64(8),
                            Name = reader.IsDBNull(9) ? "Unknown" : reader.GetString(9),
                            Type = "User"
                        }
                    };

                    if (result.AssetTypeId == 9)
                    {
                        reader.Close();
                        
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
                            
                            using var playerCommand = connection.CreateCommand();
                            playerCommand.CommandText = @"
                                SELECT COUNT(*) 
                                FROM game_presence 
                                WHERE placeid = @assetId";
                            
                            var playerParam = playerCommand.CreateParameter();
                            playerParam.ParameterName = "@assetId";
                            playerParam.Value = assetId;
                            playerCommand.Parameters.Add(playerParam);
                            
                            var playingCount = await playerCommand.ExecuteScalarAsync();
                            var playing = playingCount != null ? Convert.ToInt32(playingCount) : 0;

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
                                Visits = 0, // No visits column available
                                Favorites = 0, // No favorites count column available
                                Playing = playing
                            };
                        }
                    }

                    return Ok(result);
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
    }
}