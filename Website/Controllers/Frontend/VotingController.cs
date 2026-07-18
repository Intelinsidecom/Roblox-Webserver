using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Games;

namespace Website.Controllers.Frontend;

/// <summary>
/// Controller for handling voting operations on games/assets
/// </summary>
[ApiController]
[Route("voting")]
public class VotingController : ControllerBase
{
    private readonly VotingService _votingService;
    private readonly string _connectionString;

    public VotingController(IConfiguration configuration)
    {
        var connectionString = DatabaseUtilities.GetConnectionString(configuration);
        _connectionString = connectionString;
        _votingService = new VotingService(connectionString);
    }

    /// <summary>
    /// Gets vote counts for a specific game/place
    /// </summary>
    /// <param name="id">The place/asset ID</param>
    /// <returns>Vote data for the place</returns>
    [HttpGet("api/v1/places/{id}/votes")]
    public async Task<IActionResult> GetPlaceVotes(long id)
    {
        try
        {
            var (upvotes, downvotes) = await _votingService.GetAssetVotesAsync(id);
            
            return Ok(new 
            { 
                data = new 
                { 
                    assetId = id,
                    upVotes = upvotes,
                    downVotes = downvotes,
                    totalVotes = upvotes + downvotes,
                    voteRatio = (upvotes + downvotes) > 0 ? Math.Round((double)upvotes / (upvotes + downvotes) * 100, 2) : 0
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { errors = new[] { new { code = 0, message = "Internal server error" } } });
        }
    }

    /// <summary>
    /// Gets vote counts for multiple games
    /// </summary>
    /// <param name="universeIds">Comma-separated list of universe IDs</param>
    /// <returns>Vote data for each universe</returns>
    [HttpGet("api/v1/games/votes")]
    public async Task<IActionResult> GetGameVotes([FromQuery] string universeIds)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(universeIds))
                return BadRequest(new { errors = new[] { new { code = 1, message = "universeIds parameter is required" } } });

            var ids = universeIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => long.TryParse(id.Trim(), out var parsed) ? parsed : (long?)null)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToList();

            if (!ids.Any())
                return BadRequest(new { errors = new[] { new { code = 2, message = "Invalid universe IDs provided" } } });

            // Convert universe IDs to asset IDs (assuming universe root place is the asset to vote on)
            // This is a simplified approach - in a real implementation you'd map universes to their root place assets
            var voteData = await _votingService.GetBatchVotesAsync(ids);

            var result = ids.Select(id => {
                var hasVotes = voteData.TryGetValue(id, out var votes);
                return new
                {
                    universeId = id,
                    upVotes = hasVotes ? votes.upvotes : 0,
                    downVotes = hasVotes ? votes.downvotes : 0,
                    totalVotes = hasVotes ? votes.upvotes + votes.downvotes : 0,
                    voteRatio = hasVotes && (votes.upvotes + votes.downvotes) > 0 
                        ? Math.Round((double)votes.upvotes / (votes.upvotes + votes.downvotes) * 100, 2) 
                        : 0
                };
            });

            return Ok(new { data = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { errors = new[] { new { code = 0, message = "Internal server error" } } });
        }
    }

    /// <summary>
    /// Records a user's vote on a game
    /// </summary>
    /// <param name="universeId">The universe ID</param>
    /// <param name="request">Vote request containing vote type</param>
    /// <returns>Updated vote data</returns>
    [HttpPatch("api/v1/games/{universeId}/user-votes")]
    public async Task<IActionResult> VoteOnGame(long universeId, [FromBody] VoteRequest request)
    {
        try
        {
            // Get user ID from session/authentication
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { errors = new[] { new { code = 4, message = "Authentication required" } } });

            if (request == null)
                return BadRequest(new { errors = new[] { new { code = 1, message = "Request body is required" } } });

            // Validate user can vote (you would add additional validation here)
            if (!await CanUserVoteAsync(userId.Value, universeId))
            {
                return Forbid();
            }

            // Record the vote
            await _votingService.VoteAsync(userId.Value, universeId, request.Vote);

            // Get updated vote counts
            var (upvotes, downvotes) = await _votingService.GetAssetVotesAsync(universeId);

            return Ok(new 
            { 
                data = new 
                { 
                    universeId = universeId,
                    upVotes = upvotes,
                    downVotes = downvotes,
                    totalVotes = upvotes + downvotes,
                    voteRatio = (upvotes + downvotes) > 0 ? Math.Round((double)upvotes / (upvotes + downvotes) * 100, 2) : 0,
                    userVote = request.Vote
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { errors = new[] { new { code = 0, message = "Internal server error" } } });
        }
    }

    /// <summary>
    /// Removes a user's vote from a game
    /// </summary>
    /// <param name="universeId">The universe ID</param>
    /// <returns>Updated vote data</returns>
    [HttpDelete("api/v1/games/{universeId}/user-votes")]
    public async Task<IActionResult> RemoveVoteFromGame(long universeId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { errors = new[] { new { code = 4, message = "Authentication required" } } });

            await _votingService.RemoveVoteAsync(userId.Value, universeId);

            // Get updated vote counts
            var (upvotes, downvotes) = await _votingService.GetAssetVotesAsync(universeId);

            return Ok(new 
            { 
                data = new 
                { 
                    universeId = universeId,
                    upVotes = upvotes,
                    downVotes = downvotes,
                    totalVotes = upvotes + downvotes,
                    voteRatio = (upvotes + downvotes) > 0 ? Math.Round((double)upvotes / (upvotes + downvotes) * 100, 2) : 0,
                    userVote = (bool?)null
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { errors = new[] { new { code = 0, message = "Internal server error" } } });
        }
    }

    /// <summary>
    /// Simple voting endpoint for JavaScript frontend
    /// </summary>
    /// <param name="assetId">The asset ID to vote on</param>
    /// <param name="vote">True for upvote, false for downvote</param>
    /// <returns>Vote result in format expected by frontend</returns>
    [HttpPost("vote")]
    public async Task<IActionResult> VoteAsset([FromQuery] long assetId, [FromQuery] string vote)
    {
        try
        {
            bool? voteValue = null;
            if (!string.IsNullOrEmpty(vote) && vote.ToLower() != "null")
            {
                if (bool.TryParse(vote, out bool parsedVote))
                {
                    voteValue = parsedVote;
                }
                else
                {
                    return BadRequest(new { errors = new[] { new { code = 1, message = "Invalid vote value. Use true, false, or null." } } });
                }
            }
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Ok(new { Success = false, ModalType = "GuestUser" });
            }

            if (!await CanUserVoteAsync(userId.Value, assetId))
            {
                return Ok(new { Success = false, ModalType = "PlayGame" });
            }

            if (voteValue.HasValue)
            {
                await _votingService.VoteAsync(userId.Value, assetId, voteValue.Value);
            }
            else
            {
                await _votingService.RemoveVoteAsync(userId.Value, assetId);
            }

            var (upvotes, downvotes) = await _votingService.GetAssetVotesAsync(assetId);
            var userVote = await _votingService.GetUserVoteAsync(userId.Value, assetId);

            return Ok(new { 
                Success = true, 
                Model = new {
                    UpVotes = upvotes,
                    DownVotes = downvotes,
                    UserVote = userVote
                }
            });
        }
        catch (Exception ex)
        {
            return Ok(new { Success = false, ModalType = "Error" });
        }
    }

    /// <summary>
    /// Gets a user's current vote on a game
    /// </summary>
    /// <param name="universeId">The universe ID</param>
    /// <returns>User's vote or null if no vote</returns>
    [HttpGet("api/v1/games/{universeId}/user-votes")]
    public async Task<IActionResult> GetUserVote(long universeId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { errors = new[] { new { code = 4, message = "Authentication required" } } });

            var userVote = await _votingService.GetUserVoteAsync(userId.Value, universeId);
            var (upvotes, downvotes) = await _votingService.GetAssetVotesAsync(universeId);

            return Ok(new 
            { 
                data = new 
                { 
                    universeId = universeId,
                    upVotes = upvotes,
                    downVotes = downvotes,
                    totalVotes = upvotes + downvotes,
                    voteRatio = (upvotes + downvotes) > 0 ? Math.Round((double)upvotes / (upvotes + downvotes) * 100, 2) : 0,
                    userVote = userVote
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { errors = new[] { new { code = 0, message = "Internal server error" } } });
        }
    }

    [HttpPost("studio/vote")]
    public async Task<IActionResult> StudioVote()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Ok(new { status = false, ModalType = "GuestUser" });

            var assetId = GetFormOrQuery<long>("assetId");
            if (assetId <= 0)
                return BadRequest(new { status = false, error = "Invalid request" });

            var vote = GetFormOrQuery<bool?>("vote") ?? false;
            await _votingService.VoteAsync(userId.Value, assetId, vote);
            return Ok(new { status = true });
        }
        catch
        {
            return Ok(new { status = false });
        }
    }

    [HttpPost("studio/unvote")]
    public async Task<IActionResult> StudioUnvote()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Ok(new { status = false, ModalType = "GuestUser" });

            var assetId = GetFormOrQuery<long>("assetId");
            if (assetId <= 0)
                return BadRequest(new { status = false, error = "Invalid request" });

            await _votingService.RemoveVoteAsync(userId.Value, assetId);
            return Ok(new { status = true });
        }
        catch
        {
            return Ok(new { status = false });
        }
    }

    private T GetFormOrQuery<T>(string key)
    {
        var value = Request.HasFormContentType
            ? Request.Form[key].FirstOrDefault()
            : Request.Query[key].FirstOrDefault();
        if (value == null) return default;
        try { return (T)Convert.ChangeType(value, typeof(T)); }
        catch { return default; }
    }

    private long? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
        {
            return null;
        }
        return userId;
    }

    private async Task<bool> CanUserVoteAsync(long userId, long assetId)
    {
        var universeId = await GamesRepository.GetUniverseIdFromPlaceIdAsync(_connectionString, assetId);
        if (!universeId.HasValue) return false;
        
        return await VisitTracking.HasVisitedAsync(userId, universeId.Value, _connectionString);
    }
}

public class VoteRequest
{
    public bool Vote { get; set; }
}

public class StudioVoteRequest
{
    public long assetId { get; set; }
    public bool vote { get; set; }
}
