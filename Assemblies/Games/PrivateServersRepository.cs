using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Games;

public enum PrivateServerStatusType
{
    Active = 1,
    Inactive = 2,
    Canceled = 3
}

public sealed class PrivateServerInfo
{
    public long PrivateServerId { get; set; }
    public long UniverseId { get; set; }
    public long PlaceId { get; set; }
    public long OwnerUserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AccessCode { get; set; } = string.Empty;
    public bool Active { get; set; }
    public bool AutoRenew { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public PrivateServerStatusType GetStatusType(DateTime? utcNow = null)
    {
        var now = utcNow ?? DateTime.UtcNow;
        if (!Active)
            return PrivateServerStatusType.Canceled;
        if (ExpiresAt <= now)
            return PrivateServerStatusType.Inactive;
        return PrivateServerStatusType.Active;
    }
}

public static class PrivateServersRepository
{
    public const int MaxNameLength = 50;
    public const int SubscriptionDays = 30;

    private static PrivateServerInfo MapReader(NpgsqlDataReader reader)
    {
        return new PrivateServerInfo
        {
            PrivateServerId = reader.GetInt64(0),
            UniverseId = reader.GetInt64(1),
            PlaceId = reader.GetInt64(2),
            OwnerUserId = reader.GetInt64(3),
            Name = reader.GetString(4),
            AccessCode = reader.GetString(5),
            Active = reader.GetBoolean(6),
            AutoRenew = reader.GetBoolean(7),
            ExpiresAt = reader.GetDateTime(8),
            CreatedAt = reader.GetDateTime(9),
            UpdatedAt = reader.GetDateTime(10)
        };
    }

    private const string SelectColumns = @"
        private_server_id, universe_id, place_id, owner_user_id, name, access_code,
        active, auto_renew, expires_at, created_at, updated_at";

    public static string? ValidateName(string? name)
    {
        var trimmed = name?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
            return "The name of a VIP Server cannot be blank.";
        if (trimmed.Length > MaxNameLength)
            return $"The name of a VIP Server can be no more than {MaxNameLength} characters.";
        return null;
    }

    /// <summary>
    /// Creates a VIP server, debiting the Robux price from the buyer within one
    /// transaction when price > 0. Returns the created server or an error message.
    /// </summary>
    public static async Task<(PrivateServerInfo? Server, string? Error, long NewBalance)> PurchaseAsync(
        string connectionString,
        long universeId,
        long placeId,
        long ownerUserId,
        string name,
        int price,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("connectionString is required", nameof(connectionString));
        if (universeId <= 0 || placeId <= 0 || ownerUserId <= 0)
            return (null, "Invalid game or user", 0);

        var nameError = ValidateName(name);
        if (nameError != null)
            return (null, nameError, 0);

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var tx = conn.BeginTransaction();

        try
        {
            long newBalance = 0;
            if (price > 0)
            {
                const string balanceSql = "select coalesce(robux_balance, 0) from users where user_id = @uid for update";
                using var balCmd = new NpgsqlCommand(balanceSql, conn, tx);
                balCmd.Parameters.AddWithValue("uid", ownerUserId);
                var balObj = await balCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                if (balObj == null || balObj == DBNull.Value)
                {
                    await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    return (null, "User not found", 0);
                }
                newBalance = balObj is long l ? l : Convert.ToInt64(balObj);

                if (newBalance < price)
                {
                    await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    return (null, "Insufficient funds", newBalance);
                }

                using var debitCmd = new NpgsqlCommand(
                    "update users set robux_balance = robux_balance - @p where user_id = @uid", conn, tx);
                debitCmd.Parameters.AddWithValue("p", price);
                debitCmd.Parameters.AddWithValue("uid", ownerUserId);
                await debitCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                newBalance -= price;
            }

            const string insertSql = @"
                insert into private_servers (universe_id, place_id, owner_user_id, name, access_code)
                values (@universeId, @placeId, @ownerUserId, @name, @accessCode)
                returning " + SelectColumns;

            using var insCmd = new NpgsqlCommand(insertSql, conn, tx);
            insCmd.Parameters.AddWithValue("universeId", universeId);
            insCmd.Parameters.AddWithValue("placeId", placeId);
            insCmd.Parameters.AddWithValue("ownerUserId", ownerUserId);
            insCmd.Parameters.AddWithValue("name", name.Trim());
            insCmd.Parameters.AddWithValue("accessCode", Guid.NewGuid().ToString("N"));

            PrivateServerInfo server;
            await using (var reader = await insCmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
            {
                if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    return (null, "Failed to create VIP server", newBalance);
                }
                server = MapReader(reader);
            }

            await tx.CommitAsync(cancellationToken).ConfigureAwait(false);
            return (server, null, newBalance);
        }
        catch (Exception ex)
        {
            try { await tx.RollbackAsync(cancellationToken).ConfigureAwait(false); } catch { }
            return (null, ex.Message, 0);
        }
    }

    public static async Task<PrivateServerInfo?> GetByIdAsync(
        string connectionString, long privateServerId, CancellationToken cancellationToken = default)
    {
        if (privateServerId <= 0)
            return null;

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string sql = $"select {SelectColumns} from private_servers where private_server_id = @id";
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", privateServerId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        return await reader.ReadAsync(cancellationToken).ConfigureAwait(false) ? MapReader(reader) : null;
    }

    public static async Task<PrivateServerInfo?> GetByAccessCodeAsync(
        string connectionString, string accessCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accessCode))
            return null;

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string sql = $"select {SelectColumns} from private_servers where access_code = @code";
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("code", accessCode.Trim());

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        return await reader.ReadAsync(cancellationToken).ConfigureAwait(false) ? MapReader(reader) : null;
    }

    /// <summary>
    /// Lists a user's VIP servers for a universe, newest first, paged.
    /// </summary>
    public static async Task<(List<PrivateServerInfo> Servers, int TotalPages)> GetForUserInUniverseAsync(
        string connectionString, long ownerUserId, long universeId, int page, int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var servers = new List<PrivateServerInfo>();
        if (ownerUserId <= 0 || universeId <= 0)
            return (servers, 1);
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        int total;
        using (var countCmd = new NpgsqlCommand(
            "select count(*) from private_servers where owner_user_id = @uid and universe_id = @universeId", conn))
        {
            countCmd.Parameters.AddWithValue("uid", ownerUserId);
            countCmd.Parameters.AddWithValue("universeId", universeId);
            total = Convert.ToInt32(await countCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false));
        }

        var sql = $@"
            select {SelectColumns}
            from private_servers
            where owner_user_id = @uid and universe_id = @universeId
            order by created_at desc
            limit @limit offset @offset";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("uid", ownerUserId);
        cmd.Parameters.AddWithValue("universeId", universeId);
        cmd.Parameters.AddWithValue("limit", pageSize);
        cmd.Parameters.AddWithValue("offset", (page - 1) * pageSize);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            servers.Add(MapReader(reader));
        }

        var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
        return (servers, totalPages);
    }

    public static async Task<bool> RenameAsync(
        string connectionString, long privateServerId, long requesterUserId, string name,
        CancellationToken cancellationToken = default)
    {
        var nameError = ValidateName(name);
        if (nameError != null)
            return false;

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"
            update private_servers
            set name = @name, updated_at = now()
            where private_server_id = @id and owner_user_id = @uid";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", privateServerId);
        cmd.Parameters.AddWithValue("uid", requesterUserId);
        cmd.Parameters.AddWithValue("name", name.Trim());

        return await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) > 0;
    }

    public static async Task<bool> CancelAsync(
        string connectionString, long privateServerId, long requesterUserId,
        CancellationToken cancellationToken = default)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"
            update private_servers
            set active = false, auto_renew = false, updated_at = now()
            where private_server_id = @id and owner_user_id = @uid";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", privateServerId);
        cmd.Parameters.AddWithValue("uid", requesterUserId);

        return await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) > 0;
    }

    /// <summary>
    /// Renews a VIP server owned by requesterUserId, debiting price Robux when
    /// price > 0. Extends an unexpired subscription by 30 days; reactivates an
    /// expired/cancelled one for a fresh 30 days.
    /// </summary>
    public static async Task<(bool Success, string? Error, long NewBalance)> RenewAsync(
        string connectionString, long privateServerId, long requesterUserId, int price,
        CancellationToken cancellationToken = default)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var tx = conn.BeginTransaction();

        try
        {
            DateTime currentExpiry;
            using (var ownCmd = new NpgsqlCommand(
                @"select expires_at from private_servers
                  where private_server_id = @id and owner_user_id = @uid for update", conn, tx))
            {
                ownCmd.Parameters.AddWithValue("id", privateServerId);
                ownCmd.Parameters.AddWithValue("uid", requesterUserId);
                var obj = await ownCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                if (obj == null || obj == DBNull.Value)
                {
                    await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    return (false, "VIP server not found", 0);
                }
                currentExpiry = (DateTime)obj;
            }

            long newBalance = 0;
            if (price > 0)
            {
                using var balCmd = new NpgsqlCommand(
                    "select coalesce(robux_balance, 0) from users where user_id = @uid for update", conn, tx);
                balCmd.Parameters.AddWithValue("uid", requesterUserId);
                var balObj = await balCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                newBalance = balObj == null || balObj == DBNull.Value ? 0 : (balObj is long l ? l : Convert.ToInt64(balObj));

                if (newBalance < price)
                {
                    await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    return (false, "Insufficient funds", newBalance);
                }

                using var debitCmd = new NpgsqlCommand(
                    "update users set robux_balance = robux_balance - @p where user_id = @uid", conn, tx);
                debitCmd.Parameters.AddWithValue("p", price);
                debitCmd.Parameters.AddWithValue("uid", requesterUserId);
                await debitCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                newBalance -= price;
            }

            var baseExpiry = currentExpiry > DateTime.UtcNow ? currentExpiry : DateTime.UtcNow;
            const string renewSql = @"
                update private_servers
                set expires_at = @expiresAt, active = true, auto_renew = true, updated_at = now()
                where private_server_id = @id and owner_user_id = @uid";

            using var renewCmd = new NpgsqlCommand(renewSql, conn, tx);
            renewCmd.Parameters.AddWithValue("expiresAt", baseExpiry.AddDays(SubscriptionDays));
            renewCmd.Parameters.AddWithValue("id", privateServerId);
            renewCmd.Parameters.AddWithValue("uid", requesterUserId);

            if (await renewCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) == 0)
            {
                await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
                return (false, "VIP server not found", newBalance);
            }

            await tx.CommitAsync(cancellationToken).ConfigureAwait(false);
            return (true, null, newBalance);
        }
        catch (Exception ex)
        {
            try { await tx.RollbackAsync(cancellationToken).ConfigureAwait(false); } catch { }
            return (false, ex.Message, 0);
        }
    }

    public static async Task<bool> IsWhitelistedAsync(
        string connectionString, long privateServerId, long userId,
        CancellationToken cancellationToken = default)
    {
        if (privateServerId <= 0 || userId <= 0)
            return false;

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"
            select 1 from private_server_whitelist
            where private_server_id = @id and user_id = @uid limit 1";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", privateServerId);
        cmd.Parameters.AddWithValue("uid", userId);

        return await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false) != null;
    }

    /// <summary>
    /// Whether userId may join this server: owner or explicitly whitelisted.
    /// Expiry/active checks are the caller's responsibility.
    /// </summary>
    public static Task<bool> CanJoinAsync(
        string connectionString, PrivateServerInfo server, long userId,
        CancellationToken cancellationToken = default)
    {
        if (server == null || userId <= 0)
            return Task.FromResult(false);
        if (server.OwnerUserId == userId)
            return Task.FromResult(true);
        return IsWhitelistedAsync(connectionString, server.PrivateServerId, userId, cancellationToken);
    }

    public static async Task<(bool Success, string? Error)> WhitelistAddAsync(
        string connectionString, long privateServerId, long requesterUserId, long targetUserId,
        CancellationToken cancellationToken = default)
    {
        if (targetUserId <= 0)
            return (false, "User not found");
        if (targetUserId == requesterUserId)
            return (false, "You already have access to your own VIP server");

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"
            with owner_check as (
                select 1 from private_servers
                where private_server_id = @id and owner_user_id = @requester
            )
            insert into private_server_whitelist (private_server_id, user_id)
            select @id, @target
            where exists (select 1 from owner_check)
            on conflict (private_server_id, user_id) do nothing";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", privateServerId);
        cmd.Parameters.AddWithValue("requester", requesterUserId);
        cmd.Parameters.AddWithValue("target", targetUserId);

        return await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) > 0
            ? (true, null)
            : (false, "Could not add user (already whitelisted or you do not own this server)");
    }

    public static async Task<bool> WhitelistRemoveAsync(
        string connectionString, long privateServerId, long requesterUserId, long targetUserId,
        CancellationToken cancellationToken = default)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"
            delete from private_server_whitelist w
            using private_servers s
            where w.private_server_id = s.private_server_id
              and s.owner_user_id = @requester
              and w.private_server_id = @id
              and w.user_id = @target";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", privateServerId);
        cmd.Parameters.AddWithValue("requester", requesterUserId);
        cmd.Parameters.AddWithValue("target", targetUserId);

        return await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) > 0;
    }

    public static async Task<List<(long UserId, string UserName)>> GetWhitelistAsync(
        string connectionString, long privateServerId, CancellationToken cancellationToken = default)
    {
        var result = new List<(long, string)>();

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"
            select w.user_id, u.user_name
            from private_server_whitelist w
            left join users u on u.user_id = w.user_id
            where w.private_server_id = @id
            order by w.created_at asc";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", privateServerId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            result.Add((reader.GetInt64(0), reader.IsDBNull(1) ? "Unknown" : reader.GetString(1)));
        }
        return result;
    }
}
