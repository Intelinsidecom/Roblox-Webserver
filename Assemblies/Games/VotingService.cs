using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Npgsql;

namespace Games;

/// <summary>
/// Service for handling voting operations on assets
/// </summary>
public class VotingService
{
    private readonly string _connectionString;

    public VotingService(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Gets the current vote counts for an asset
    /// </summary>
    public async Task<(long upvotes, long downvotes)> GetAssetVotesAsync(long assetId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT upvotes, downvotes 
            FROM assets 
            WHERE asset_id = @assetId";

        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@assetId", assetId);

        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return (reader.GetInt64(0), reader.GetInt64(1));
        }

        return (0, 0);
    }

    /// <summary>
    /// Gets a user's vote on an asset (true for upvote, false for downvote, null for no vote)
    /// </summary>
    public async Task<bool?> GetUserVoteAsync(long userId, long assetId, CancellationToken cancellationToken = default)
    {
        // Check for upvote first
        const string upvoteSql = @"
            SELECT 1 FROM user_upvoted_assets 
            WHERE user_id = @userId AND asset_id = @assetId LIMIT 1";

        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        
        using var upvoteCmd = new NpgsqlCommand(upvoteSql, conn);
        upvoteCmd.Parameters.AddWithValue("@userId", userId);
        upvoteCmd.Parameters.AddWithValue("@assetId", assetId);

        var upvoteResult = await upvoteCmd.ExecuteScalarAsync(cancellationToken);
        if (upvoteResult != null)
            return true;

        const string downvoteSql = @"
            SELECT 1 FROM user_downvoted_assets 
            WHERE user_id = @userId AND asset_id = @assetId LIMIT 1";

        using var downvoteCmd = new NpgsqlCommand(downvoteSql, conn);
        downvoteCmd.Parameters.AddWithValue("@userId", userId);
        downvoteCmd.Parameters.AddWithValue("@assetId", assetId);

        var downvoteResult = await downvoteCmd.ExecuteScalarAsync(cancellationToken);
        if (downvoteResult != null)
            return false;

        return null;
    }

    /// <summary>
    /// Records or updates a user's vote on an asset
    /// </summary>
    public async Task<bool> VoteAsync(long userId, long assetId, bool isUpvote, CancellationToken cancellationToken = default)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        using var transaction = conn.BeginTransaction();

        try
        {
            await RemoveExistingVoteAsync(conn, transaction, userId, assetId, cancellationToken);

            const string insertUpvoteSql = @"INSERT INTO user_upvoted_assets (user_id, asset_id) VALUES (@userId, @assetId)";
            const string insertDownvoteSql = @"INSERT INTO user_downvoted_assets (user_id, asset_id) VALUES (@userId, @assetId)";

            var insertSql = isUpvote ? insertUpvoteSql : insertDownvoteSql;
            
            using var cmd = new NpgsqlCommand(insertSql, conn, transaction);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@assetId", assetId);

            await cmd.ExecuteNonQueryAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Removes a user's vote from an asset
    /// </summary>
    public async Task<bool> RemoveVoteAsync(long userId, long assetId, CancellationToken cancellationToken = default)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        using var transaction = conn.BeginTransaction();

        try
        {
            var removed = await RemoveExistingVoteAsync(conn, transaction, userId, assetId, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return removed;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Gets vote statistics for multiple assets
    /// </summary>
    public async Task<Dictionary<long, (long upvotes, long downvotes)>> GetBatchVotesAsync(
        IEnumerable<long> assetIds, CancellationToken cancellationToken = default)
    {
        var ids = assetIds.ToList();
        if (!ids.Any())
            return new Dictionary<long, (long, long)>();

        const string sql = @"
            SELECT asset_id, upvotes, downvotes 
            FROM assets 
            WHERE asset_id = ANY(@assetIds)";

        using var conn = await DatabaseUtilities.CreateAndOpenConnectionAsync(_connectionString, cancellationToken);
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@assetIds", ids);

        var result = new Dictionary<long, (long, long)>();
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        
        while (await reader.ReadAsync(cancellationToken))
        {
            var assetId = reader.GetInt64(0);
            var upvotes = reader.GetInt64(1);
            var downvotes = reader.GetInt64(2);
            result[assetId] = (upvotes, downvotes);
        }

        return result;
    }

    /// <summary>
    /// Gets assets that a user has voted on
    /// </summary>
    public async Task<(IEnumerable<long> upvoted, IEnumerable<long> downvoted)> GetUserVotedAssetsAsync(
        long userId, CancellationToken cancellationToken = default)
    {
        const string upvoteSql = @"SELECT asset_id FROM user_upvoted_assets WHERE user_id = @userId";
        const string downvoteSql = @"SELECT asset_id FROM user_downvoted_assets WHERE user_id = @userId";

        using var conn = await DatabaseUtilities.CreateAndOpenConnectionAsync(_connectionString, cancellationToken);

        var upvoted = new List<long>();
        var downvoted = new List<long>();
        using var upvoteCmd = new NpgsqlCommand(upvoteSql, conn);
        upvoteCmd.Parameters.AddWithValue("@userId", userId);
        using var upvoteReader = await upvoteCmd.ExecuteReaderAsync(cancellationToken);
        while (await upvoteReader.ReadAsync(cancellationToken))
        {
            upvoted.Add(upvoteReader.GetInt64(0));
        }

        using var downvoteCmd = new NpgsqlCommand(downvoteSql, conn);
        downvoteCmd.Parameters.AddWithValue("@userId", userId);
        using var downvoteReader = await downvoteCmd.ExecuteReaderAsync(cancellationToken);
        while (await downvoteReader.ReadAsync(cancellationToken))
        {
            downvoted.Add(downvoteReader.GetInt64(0));
        }

        return (upvoted, downvoted);
    }

    private static async Task<bool> RemoveExistingVoteAsync(
        NpgsqlConnection conn, NpgsqlTransaction transaction, 
        long userId, long assetId, CancellationToken cancellationToken)
    {
        var removed = false;

        const string deleteUpvoteSql = @"
            DELETE FROM user_upvoted_assets 
            WHERE user_id = @userId AND asset_id = @assetId";

        using var deleteUpvoteCmd = new NpgsqlCommand(deleteUpvoteSql, conn, transaction);
        deleteUpvoteCmd.Parameters.AddWithValue("@userId", userId);
        deleteUpvoteCmd.Parameters.AddWithValue("@assetId", assetId);
        
        if (await deleteUpvoteCmd.ExecuteNonQueryAsync(cancellationToken) > 0)
            removed = true;

        const string deleteDownvoteSql = @"
            DELETE FROM user_downvoted_assets 
            WHERE user_id = @userId AND asset_id = @assetId";

        using var deleteDownvoteCmd = new NpgsqlCommand(deleteDownvoteSql, conn, transaction);
        deleteDownvoteCmd.Parameters.AddWithValue("@userId", userId);
        deleteDownvoteCmd.Parameters.AddWithValue("@assetId", assetId);
        
        if (await deleteDownvoteCmd.ExecuteNonQueryAsync(cancellationToken) > 0)
            removed = true;

        return removed;
    }
}
