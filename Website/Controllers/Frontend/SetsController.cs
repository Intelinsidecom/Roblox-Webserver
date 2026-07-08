using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace RobloxWebserver.Controllers;

[ApiController]
[Route("sets")]
public class SetsController : Controller
{
    private readonly IConfiguration _configuration;

    public SetsController(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new System.ArgumentNullException(nameof(configuration));
    }

    private string? ConnectionString => _configuration.GetConnectionString("Default");

    [HttpGet("get-roblox-sets")]
    [AllowAnonymous]
    public IActionResult GetRobloxSets()
    {
        var sets = new List<RobloxSetEntry>
        {
            new() { Id = 464140, Name = "Game Stuff" },
            new() { Id = 464138, Name = "Baseplates" },
            new() { Id = 464131, Name = "Weapons" },
            new() { Id = 463266, Name = "Vehicles" },
            new() { Id = 458339, Name = "Bricks" },
            new() { Id = 360380, Name = "Basic Building" },
            new() { Id = 360378, Name = "Advanced Building" },
            new() { Id = 360375, Name = "House Kit" },
            new() { Id = 360372, Name = "House Interior Kit" },
            new() { Id = 360371, Name = "Landscape" },
            new() { Id = 360369, Name = "Castle Kit" },
            new() { Id = 360365, Name = "Castle Interior Kit" },
            new() { Id = 360363, Name = "Space Kit" },
            new() { Id = 360362, Name = "Fun Machines" },
            new() { Id = 360360, Name = "Deadly Machines" }
        };

        return Ok(sets);
    }

    [HttpGet("get-by-creator")]
    [Authorize]
    public async Task<IActionResult> GetByCreator([FromQuery] long userid, CancellationToken cancellationToken = default)
    {
        var connStr = ConnectionString;
        if (string.IsNullOrWhiteSpace(connStr))
            return Ok(new List<RobloxSetEntry>());

        var sets = new List<RobloxSetEntry>();

        try
        {
            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync(cancellationToken);

            const string sql = @"SELECT asset_id, name FROM assets WHERE owner_user_id = @uid AND asset_type_id = 10 ORDER BY name ASC";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("uid", userid);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                sets.Add(new RobloxSetEntry
                {
                    Id = reader.GetInt64(0),
                    Name = reader.IsDBNull(1) ? "Unnamed" : reader.GetString(1)
                });
            }
        }
        catch
        {
            return Ok(new List<RobloxSetEntry>());
        }

        return Ok(sets);
    }

    [HttpGet("get-subscribed")]
    [Authorize]
    public async Task<IActionResult> GetSubscribed([FromQuery] long userid, CancellationToken cancellationToken = default)
    {
        var connStr = ConnectionString;
        if (string.IsNullOrWhiteSpace(connStr))
            return Ok(new List<RobloxSetEntry>());

        var sets = new List<RobloxSetEntry>();

        try
        {
            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync(cancellationToken);

            const string sql = @"SELECT a.asset_id, a.name FROM user_assets ua JOIN assets a ON a.asset_id = ua.asset_id WHERE ua.user_id = @uid AND a.asset_type_id = 10 ORDER BY ua.created_at DESC";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("uid", userid);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                sets.Add(new RobloxSetEntry
                {
                    Id = reader.GetInt64(0),
                    Name = reader.IsDBNull(1) ? "Unnamed" : reader.GetString(1)
                });
            }
        }
        catch
        {
            return Ok(new List<RobloxSetEntry>());
        }

        return Ok(sets);
    }

    [HttpGet("{setId}/items")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSetItems(long setId, [FromQuery] int page = 1, [FromQuery] int num = 30, CancellationToken cancellationToken = default)
    {
        var connStr = ConnectionString;
        if (string.IsNullOrWhiteSpace(connStr))
            return Ok(new { Results = new List<object>(), TotalResults = 0L });

        num = System.Math.Clamp(num, 1, 100);
        page = System.Math.Max(1, page);
        var offset = (page - 1) * num;

        try
        {
            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync(cancellationToken);

            const string countSql = @"SELECT COUNT(*) FROM assets WHERE asset_type_id = 10 AND (is_place = false OR is_place IS NULL)";
            await using var countCmd = new NpgsqlCommand(countSql, conn);
            var totalResults = (long)(await countCmd.ExecuteScalarAsync(cancellationToken) ?? 0L);

            const string sql = @"SELECT a.asset_id, a.name, a.asset_type_id, a.owner_user_id,
                                        a.thumbnail_url, a.upvotes, a.downvotes, u.user_name
                                 FROM assets a
                                 LEFT JOIN users u ON u.user_id = a.owner_user_id
                                 WHERE a.asset_type_id = 10 AND (a.is_place = false OR a.is_place IS NULL)
                                 ORDER BY a.created_at DESC
                                 OFFSET @offset LIMIT @limit";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("offset", offset);
            cmd.Parameters.AddWithValue("limit", num);

            var results = new List<object>();
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                results.Add(new
                {
                    Asset = new
                    {
                        Id = reader.GetInt64(0),
                        Name = reader.IsDBNull(1) ? "" : reader.GetString(1),
                        TypeId = reader.GetInt32(2),
                        IsEndorsed = false
                    },
                    Creator = new
                    {
                        Id = reader.GetInt64(3),
                        Name = reader.IsDBNull(7) ? "" : reader.GetString(7),
                        Type = 1
                    },
                    Thumbnail = new
                    {
                        Final = true,
                        Url = reader.IsDBNull(4) ? "" : reader.GetString(4),
                        RetryUrl = (string?)null,
                        UserId = 0L,
                        EndpointType = "Avatar"
                    },
                    Voting = new
                    {
                        ShowVotes = true,
                        UpVotes = reader.IsDBNull(5) ? 0L : reader.GetInt64(5),
                        DownVotes = reader.IsDBNull(6) ? 0L : reader.GetInt64(6),
                        CanVote = false,
                        UserVote = (bool?)null,
                        HasVoted = false,
                        ReasonForNotVoteable = "InvalidAssetOrUser"
                    }
                });
            }

            return Ok(new { Results = results, TotalResults = totalResults });
        }
        catch (System.Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

public sealed class RobloxSetEntry
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
}
