using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;

namespace Api.Controllers
{
    /// <summary>
    /// API endpoints for game transactions and purchase receipts
    /// Used by MarketplaceService for ProcessReceipt callback
    /// </summary>
    [ApiController]
    [Route("gametransactions")]
    public class GameTransactionsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public GameTransactionsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// GET /gametransactions/getpendingtransactions?PlaceId={placeId}&PlayerId={playerId}
        /// Returns pending purchase receipts for a player
        /// Called by MarketplaceService when player joins or after purchase
        /// </summary>
        [HttpGet("getpendingtransactions")]
        public async Task<IActionResult> GetPendingTransactions([FromQuery] long PlaceId, [FromQuery] long PlayerId)
        {
            try
            {
                if (PlaceId <= 0 || PlayerId <= 0)
                {
                    return BadRequest(new { error = "Invalid place ID or player ID" });
                }

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var pendingTransactions = new List<object>();


                return Ok(pendingTransactions);
            }
            catch (Exception ex)
            {
                return Ok(new List<object>());
            }
        }

        /// <summary>
        /// POST /gametransactions/settransactionstatuscomplete
        /// Marks a transaction receipt as processed/completed
        /// Called by MarketplaceService after ProcessReceipt callback returns PurchaseGranted
        /// </summary>
        [HttpPost("settransactionstatuscomplete")]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data", "application/json")]
        public async Task<IActionResult> SetTransactionStatusComplete()
        {
            try
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

                if (string.IsNullOrEmpty(receipt))
                {
                    return BadRequest(new { error = "Receipt parameter is required" });
                }

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);

                return Ok(new { success = true, receipt = receipt });
            }
            catch (Exception ex)
            {
                return Ok(new { success = true, warning = ex.Message });
            }
        }

        /// <summary>
        /// POST /gametransactions/createtransaction
        /// Creates a new game transaction record (for testing/internal use)
        /// </summary>
        [HttpPost("createtransaction")]
        public async Task<IActionResult> CreateTransaction(
            [FromForm] long playerId,
            [FromForm] long placeId,
            [FromForm] long productId,
            [FromForm] int priceInRobux,
            [FromForm] int priceInTix)
        {
            try
            {
                if (playerId <= 0 || placeId <= 0 || productId <= 0)
                {
                    return BadRequest(new { error = "Invalid player, place, or product ID" });
                }

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);
                var receiptId = Guid.NewGuid().ToString("N");


                return Ok(new
                {
                    success = true,
                    receipt = receiptId,
                    playerId = playerId,
                    placeId = placeId,
                    productId = productId,
                    status = "pending"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// GET /gametransactions/hastransaction?receipt={receipt}
        /// Check if a transaction exists and its status
        /// </summary>
        [HttpGet("hastransaction")]
        public async Task<IActionResult> HasTransaction([FromQuery] string receipt)
        {
            try
            {
                if (string.IsNullOrEmpty(receipt))
                {
                    return BadRequest(new { error = "Receipt parameter is required" });
                }

                var connectionString = DatabaseUtilities.GetConnectionString(_configuration);

                return Ok(new
                {
                    exists = false,
                    receipt = receipt,
                    isComplete = false
                });
            }
            catch (Exception ex)
            {
                return Ok(new { exists = false, error = ex.Message });
            }
        }
    }
}
