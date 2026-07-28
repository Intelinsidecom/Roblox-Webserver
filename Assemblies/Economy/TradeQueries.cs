using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Economy
{
    public class TradeQueries
    {
        private const double TradeFee = 0.3;

        private readonly string _connectionString;

        public TradeQueries(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        private async Task<NpgsqlConnection> OpenAsync(CancellationToken ct = default)
        {
            var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);
            return conn;
        }

        public async Task<long> CreateTradeAsync(
            long senderId, long receiverId, string tradeJson,
            CancellationToken ct = default)
        {
            var doc = JsonDocument.Parse(tradeJson);
            var root = doc.RootElement;

            long senderRobux = 0;
            long receiverRobux = 0;
            var senderItems = new List<(long UserAssetId, long AssetId)>();
            var receiverItems = new List<(long UserAssetId, long AssetId)>();

            if (root.TryGetProperty("AgentOfferList", out var agentList))
            {
                foreach (var agent in agentList.EnumerateArray())
                {
                    var agentId = agent.GetProperty("AgentID").GetInt64();
                    var isSender = agentId == senderId;

                    if (agent.TryGetProperty("OfferRobux", out var robuxProp))
                    {
                        var robux = robuxProp.GetInt64();
                        if (isSender) senderRobux = robux;
                        else receiverRobux = robux;
                    }

                    if (agent.TryGetProperty("OfferList", out var offerList))
                    {
                        foreach (var item in offerList.EnumerateArray())
                        {
                            var userAssetIdStr = item.GetProperty("UserAssetID").GetString();
                            if (!long.TryParse(userAssetIdStr, out var userAssetId)) continue;
                            var assetId = item.TryGetProperty("AssetId", out var aId)
                                ? (aId.ValueKind == JsonValueKind.String
                                    ? (long.TryParse(aId.GetString(), out var aIdParsed) ? aIdParsed : 0)
                                    : aId.GetInt64())
                                : 0;
                            if (isSender)
                                senderItems.Add((userAssetId, assetId));
                            else
                                receiverItems.Add((userAssetId, assetId));
                        }
                    }
                }
            }

            await using var conn = await OpenAsync(ct).ConfigureAwait(false);
            using var tx = conn.BeginTransaction();

            try
            {
                foreach (var items in new[] { senderItems, receiverItems })
                {
                    for (int i = 0; i < items.Count; i++)
                    {
                        if (items[i].AssetId == 0)
                        {
                            using var lookupCmd = new NpgsqlCommand(@"
                                SELECT asset_id FROM user_assets WHERE user_asset_id = @id", conn, tx);
                            lookupCmd.Parameters.AddWithValue("id", items[i].UserAssetId);
                            var result = await lookupCmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
                            if (result != null && result is not DBNull)
                                items[i] = (items[i].UserAssetId, (long)result);
                        }
                    }
                }

                using var tradeCmd = new NpgsqlCommand(@"
                    INSERT INTO trades (sender_id, receiver_id, status, sender_robux, receiver_robux)
                    VALUES (@sender, @receiver, 'Open', @senderRobux, @receiverRobux)
                    RETURNING id", conn, tx);
                tradeCmd.Parameters.AddWithValue("sender", senderId);
                tradeCmd.Parameters.AddWithValue("receiver", receiverId);
                tradeCmd.Parameters.AddWithValue("senderRobux", senderRobux);
                tradeCmd.Parameters.AddWithValue("receiverRobux", receiverRobux);
                var tradeId = (long)(await tradeCmd.ExecuteScalarAsync(ct).ConfigureAwait(false))!;

                foreach (var (userAssetId, assetId) in senderItems)
                {
                    long? serialNumber = await GetSerialNumberForUserAssetAsync(
                        conn, tx, userAssetId, assetId, senderId, ct).ConfigureAwait(false);

                    using var itemCmd = new NpgsqlCommand(@"
                        INSERT INTO trade_items (trade_id, user_asset_id, asset_id, agent_id, side, serial_number)
                        VALUES (@tradeId, @userAssetId, @assetId, @agentId, 'offer', @serialNumber)
                        ON CONFLICT DO NOTHING", conn, tx);
                    itemCmd.Parameters.AddWithValue("tradeId", tradeId);
                    itemCmd.Parameters.AddWithValue("userAssetId", userAssetId);
                    itemCmd.Parameters.AddWithValue("assetId", assetId);
                    itemCmd.Parameters.AddWithValue("agentId", senderId);
                    itemCmd.Parameters.AddWithValue("serialNumber", (object?)serialNumber ?? DBNull.Value);
                    await itemCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                }

                foreach (var (userAssetId, assetId) in receiverItems)
                {
                    long? serialNumber = await GetSerialNumberForUserAssetAsync(
                        conn, tx, userAssetId, assetId, receiverId, ct).ConfigureAwait(false);

                    using var itemCmd = new NpgsqlCommand(@"
                        INSERT INTO trade_items (trade_id, user_asset_id, asset_id, agent_id, side, serial_number)
                        VALUES (@tradeId, @userAssetId, @assetId, @agentId, 'request', @serialNumber)
                        ON CONFLICT DO NOTHING", conn, tx);
                    itemCmd.Parameters.AddWithValue("tradeId", tradeId);
                    itemCmd.Parameters.AddWithValue("userAssetId", userAssetId);
                    itemCmd.Parameters.AddWithValue("assetId", assetId);
                    itemCmd.Parameters.AddWithValue("agentId", receiverId);
                    itemCmd.Parameters.AddWithValue("serialNumber", (object?)serialNumber ?? DBNull.Value);
                    await itemCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                }

                using var histCmd = new NpgsqlCommand(@"
                    INSERT INTO trade_history (trade_id, action, actor_id)
                    VALUES (@tradeId, 'created', @actorId)", conn, tx);
                histCmd.Parameters.AddWithValue("tradeId", tradeId);
                histCmd.Parameters.AddWithValue("actorId", senderId);
                await histCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

                tx.Commit();
                return tradeId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task<JsonDocument?> GetTradeByIdAsync(
            long tradeId, long requestUserId,
            CancellationToken ct = default)
        {
            await using var conn = await OpenAsync(ct).ConfigureAwait(false);

            using var tradeCmd = new NpgsqlCommand(@"
                SELECT id, sender_id, receiver_id, status, sender_robux, receiver_robux,
                       created_at, expires_at
                FROM trades WHERE id = @id", conn);
            tradeCmd.Parameters.AddWithValue("id", tradeId);
            await using var reader = await tradeCmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
            if (!await reader.ReadAsync(ct).ConfigureAwait(false))
                return null;

            var dbTradeId = reader.GetInt64(0);
            var senderId = reader.GetInt64(1);
            var receiverId = reader.GetInt64(2);
            var status = reader.GetString(3);
            var senderRobux = reader.GetInt64(4);
            var receiverRobux = reader.GetInt64(5);
            var createdAt = reader.GetDateTime(6);
            var expiresAt = reader.GetDateTime(7);
            await reader.CloseAsync().ConfigureAwait(false);

            var agentOffers = new List<Dictionary<string, object?>>();
            foreach (var agentId in new[] { senderId, receiverId })
            {
                var offerList = new List<Dictionary<string, object?>>();
                long offerRobux = agentId == senderId ? senderRobux : receiverRobux;

                using var itemsCmd = new NpgsqlCommand(@"
                    SELECT ti.user_asset_id, ti.asset_id, ti.side,
                           a.name, a.recent_average_price, a.price,
                           ti.serial_number,
                           (SELECT COUNT(*) FROM asset_serials WHERE asset_id = ti.asset_id) AS serial_total
                    FROM trade_items ti
                    JOIN assets a ON a.asset_id = ti.asset_id
                    WHERE ti.trade_id = @tradeId AND ti.agent_id = @agentId", conn);
                itemsCmd.Parameters.AddWithValue("tradeId", dbTradeId);
                itemsCmd.Parameters.AddWithValue("agentId", agentId);
                await using var itemReader = await itemsCmd.ExecuteReaderAsync(ct).ConfigureAwait(false);

                var rawItems = new List<(long UserAssetId, long AssetId, string Name, long? AvgPrice, long? Price, long? Serial, long SerialTotal)>();
                while (await itemReader.ReadAsync(ct).ConfigureAwait(false))
                {
                    rawItems.Add((
                        itemReader.GetInt64(0),
                        itemReader.GetInt64(1),
                        itemReader.IsDBNull(3) ? "Unknown" : itemReader.GetString(3),
                        itemReader.IsDBNull(4) ? (long?)null : itemReader.GetInt64(4),
                        itemReader.IsDBNull(5) ? (long?)null : itemReader.GetInt64(5),
                        itemReader.IsDBNull(6) ? (long?)null : itemReader.GetInt64(6),
                        itemReader.IsDBNull(7) ? 0 : itemReader.GetInt64(7)
                    ));
                }
                await itemReader.CloseAsync().ConfigureAwait(false);

                foreach (var raw in rawItems)
                {
                    var assetId = raw.AssetId;
                    var name = raw.Name;
                    var avgPrice = raw.AvgPrice;
                    var price = raw.Price;
                    var serialNumber = raw.Serial;
                    var serialTotal = raw.SerialTotal;

                    if (!serialNumber.HasValue)
                    {
                        using var serialLookup = new NpgsqlCommand(@"
                            SELECT serial_number FROM asset_serials
                            WHERE asset_id = @assetId AND owner_user_id = @agentId
                            LIMIT 1", conn);
                        serialLookup.Parameters.AddWithValue("assetId", assetId);
                        serialLookup.Parameters.AddWithValue("agentId", agentId);
                        var sr = await serialLookup.ExecuteScalarAsync(ct).ConfigureAwait(false);
                        if (sr != null && sr is not DBNull)
                            serialNumber = (long)sr;
                    }

                    var slug = System.Text.RegularExpressions.Regex.Replace(name, @"[\W_]+", "-")
                        .Trim('-').ToLowerInvariant();
                    if (string.IsNullOrEmpty(slug)) slug = "unnamed";

                    offerList.Add(new Dictionary<string, object?>
                    {
                        ["UserAssetID"] = raw.UserAssetId.ToString(),
                        ["AssetId"] = assetId,
                        ["Name"] = name,
                        ["ItemLink"] = $"/catalog/{assetId}/{slug}",
                        ["ImageLink"] = $"/asset-thumbnail/image?assetId={assetId}&height=110&width=110",
                        ["AveragePrice"] = avgPrice.HasValue ? (object)avgPrice.Value : "---",
                        ["OriginalPrice"] = price.HasValue ? (object)price.Value : "---",
                        ["SerialNumber"] = serialNumber.HasValue ? (object)serialNumber.Value : "",
                        ["SerialNumberTotal"] = serialTotal > 0 ? (object)serialTotal : ""
                    });
                }

                long offerValue = 0;
                foreach (var item in offerList)
                {
                    if (item["AveragePrice"] is long rap) offerValue += rap;
                    else if (item["OriginalPrice"] is long price) offerValue += price;
                }
                if (offerRobux > 0) offerValue += offerRobux;

                agentOffers.Add(new Dictionary<string, object?>
                {
                    ["AgentID"] = agentId,
                    ["OfferList"] = offerList,
                    ["OfferRobux"] = offerRobux,
                    ["OfferValue"] = offerValue
                });
            }

            var isActive = status == "Open" || status == "Pending";
            var result = new Dictionary<string, object?>
            {
                ["AgentOfferList"] = agentOffers,
                ["IsActive"] = isActive,
                ["TradeStatus"] = status,
                ["StatusType"] = status,
                ["Expiration"] = $"/Date({new DateTimeOffset(expiresAt).ToUnixTimeMilliseconds()})/",
                ["TradeSessionID"] = dbTradeId,
                ["SenderID"] = senderId,
                ["ReceiverID"] = receiverId
            };

            var json = JsonSerializer.Serialize(result);
            return JsonDocument.Parse(json);
        }

        public async Task<List<Dictionary<string, object?>>> GetUserTradesAsync(
            long userId, string statusType, int startIndex,
            CancellationToken ct = default)
        {
            await using var conn = await OpenAsync(ct).ConfigureAwait(false);

            string whereClause;
            switch (statusType)
            {
                case "inbound":
                    whereClause = "t.receiver_id = @userId AND t.status IN ('Open', 'Pending')";
                    break;
                case "outbound":
                    whereClause = "t.sender_id = @userId AND t.status IN ('Open', 'Pending')";
                    break;
                case "completed":
                    whereClause = "(t.sender_id = @userId OR t.receiver_id = @userId) AND t.status = 'Finished'";
                    break;
                case "inactive":
                    whereClause = "(t.sender_id = @userId OR t.receiver_id = @userId) AND t.status IN ('Expired', 'Rejected', 'Declined', 'Countered')";
                    break;
                default:
                    whereClause = "(t.sender_id = @userId OR t.receiver_id = @userId) AND t.status IN ('Open', 'Pending')";
                    break;
            }

            using var cmd = new NpgsqlCommand($@"
                SELECT t.id, t.sender_id, t.receiver_id, t.status,
                       t.created_at, t.expires_at,
                       COALESCE(sender_u.user_name, '') as sender_name,
                       COALESCE(receiver_u.user_name, '') as receiver_name
                FROM trades t
                LEFT JOIN users sender_u ON sender_u.user_id = t.sender_id
                LEFT JOIN users receiver_u ON receiver_u.user_id = t.receiver_id
                WHERE {whereClause}
                ORDER BY t.created_at DESC
                OFFSET @offset LIMIT 25", conn);
            cmd.Parameters.AddWithValue("userId", userId);
            cmd.Parameters.AddWithValue("offset", startIndex);

            var results = new List<Dictionary<string, object?>>();
            await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);

            while (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                var tradeId = reader.GetInt64(0);
                var senderId = reader.GetInt64(1);
                var receiverId = reader.GetInt64(2);
                var status = reader.GetString(3);
                var createdAt = reader.GetDateTime(4);
                var expiresAt = reader.GetDateTime(5);
                var senderName = reader.GetString(6);
                var receiverName = reader.GetString(7);

                var partnerId = senderId == userId ? receiverId : senderId;
                var partnerName = senderId == userId ? receiverName : senderName;

                var expiresStr = (status == "Open" || status == "Pending")
                    ? FormatExpires(expiresAt)
                    : "";

                var statusAddon = "";
                if (status == "Open" && senderId == userId) statusAddon = " (You)";
                if (status == "Pending" && senderId == userId) statusAddon = " (You)";

                results.Add(new Dictionary<string, object?>
                {
                    ["TradeSessionID"] = tradeId,
                    ["TradePartnerID"] = partnerId,
                    ["TradePartner"] = partnerName,
                    ["Date"] = createdAt.ToString("MM/dd/yyyy"),
                    ["Expires"] = expiresStr,
                    ["Status"] = status,
                    ["StatusAddon"] = statusAddon
                });
            }

            return results;
        }

        public async Task<int> GetUserTradeCountAsync(
            long userId, string statusType,
            CancellationToken ct = default)
        {
            await using var conn = await OpenAsync(ct).ConfigureAwait(false);

            string whereClause;
            switch (statusType)
            {
                case "inbound":
                    whereClause = "receiver_id = @userId AND status IN ('Open', 'Pending')";
                    break;
                case "outbound":
                    whereClause = "sender_id = @userId AND status IN ('Open', 'Pending')";
                    break;
                case "completed":
                    whereClause = "(sender_id = @userId OR receiver_id = @userId) AND status = 'Finished'";
                    break;
                case "inactive":
                    whereClause = "(sender_id = @userId OR receiver_id = @userId) AND status IN ('Expired', 'Rejected', 'Declined', 'Countered')";
                    break;
                default:
                    whereClause = "(sender_id = @userId OR receiver_id = @userId) AND status IN ('Open', 'Pending')";
                    break;
            }

            using var cmd = new NpgsqlCommand($@"
                SELECT COUNT(*) FROM trades WHERE {whereClause}", conn);
            cmd.Parameters.AddWithValue("userId", userId);
            var result = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
            return result == null || result is DBNull ? 0 : Convert.ToInt32(result);
        }

        public async Task<bool> AcceptTradeAsync(
            long tradeId, long userId, string tradeJson,
            CancellationToken ct = default)
        {
            await using var conn = await OpenAsync(ct).ConfigureAwait(false);
            using var tx = conn.BeginTransaction();

            try
            {
                using var lockCmd = new NpgsqlCommand(@"
                    SELECT id, sender_id, receiver_id, status, sender_robux, receiver_robux
                    FROM trades WHERE id = @id FOR UPDATE", conn, tx);
                lockCmd.Parameters.AddWithValue("id", tradeId);
                await using var lockReader = await lockCmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
                if (!await lockReader.ReadAsync(ct).ConfigureAwait(false))
                {
                    tx.Rollback();
                    return false;
                }

                var senderId = lockReader.GetInt64(1);
                var receiverId = lockReader.GetInt64(2);
                var status = lockReader.GetString(3);
                var senderRobux = lockReader.GetInt64(4);
                var receiverRobux = lockReader.GetInt64(5);
                await lockReader.CloseAsync().ConfigureAwait(false);

                if (status != "Open")
                {
                    tx.Rollback();
                    return false;
                }

                if (userId != receiverId)
                {
                    tx.Rollback();
                    return false;
                }

                using var updateCmd = new NpgsqlCommand(@"
                    UPDATE trades SET status = 'Pending', updated_at = now()
                    WHERE id = @id", conn, tx);
                updateCmd.Parameters.AddWithValue("id", tradeId);
                await updateCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

                await TransferItemsAsync(conn, tx, tradeId, senderId, receiverId, ct).ConfigureAwait(false);

                using var finishCmd = new NpgsqlCommand(@"
                    UPDATE trades SET status = 'Finished', updated_at = now()
                    WHERE id = @id", conn, tx);
                finishCmd.Parameters.AddWithValue("id", tradeId);
                await finishCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

                if (senderRobux > 0)
                {
                    using var debitCmd = new NpgsqlCommand(@"
                        UPDATE users SET robux = GREATEST(robux - @amount, 0)
                        WHERE user_id = @userId", conn, tx);
                    debitCmd.Parameters.AddWithValue("amount", senderRobux);
                    debitCmd.Parameters.AddWithValue("userId", senderId);
                    await debitCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

                    var receiverGets = senderRobux - (long)Math.Ceiling(senderRobux * TradeFee);
                    using var creditCmd = new NpgsqlCommand(@"
                        UPDATE users SET robux = robux + @amount
                        WHERE user_id = @userId", conn, tx);
                    creditCmd.Parameters.AddWithValue("amount", receiverGets);
                    creditCmd.Parameters.AddWithValue("userId", receiverId);
                    await creditCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                }

                if (receiverRobux > 0)
                {
                    using var debitCmd = new NpgsqlCommand(@"
                        UPDATE users SET robux = GREATEST(robux - @amount, 0)
                        WHERE user_id = @userId", conn, tx);
                    debitCmd.Parameters.AddWithValue("amount", receiverRobux);
                    debitCmd.Parameters.AddWithValue("userId", receiverId);
                    await debitCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

                    var senderGets = receiverRobux - (long)Math.Ceiling(receiverRobux * TradeFee);
                    using var creditCmd = new NpgsqlCommand(@"
                        UPDATE users SET robux = robux + @amount
                        WHERE user_id = @userId", conn, tx);
                    creditCmd.Parameters.AddWithValue("amount", senderGets);
                    creditCmd.Parameters.AddWithValue("userId", senderId);
                    await creditCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                }

                using var histCmd = new NpgsqlCommand(@"
                    INSERT INTO trade_history (trade_id, action, actor_id)
                    VALUES (@tradeId, 'accepted', @actorId)", conn, tx);
                histCmd.Parameters.AddWithValue("tradeId", tradeId);
                histCmd.Parameters.AddWithValue("actorId", userId);
                await histCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

                tx.Commit();
                return true;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task<bool> DeclineTradeAsync(
            long tradeId, long userId,
            CancellationToken ct = default)
        {
            await using var conn = await OpenAsync(ct).ConfigureAwait(false);
            using var tx = conn.BeginTransaction();

            try
            {
                using var checkCmd = new NpgsqlCommand(@"
                    SELECT sender_id, receiver_id, status
                    FROM trades WHERE id = @id FOR UPDATE", conn, tx);
                checkCmd.Parameters.AddWithValue("id", tradeId);
                await using var reader = await checkCmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
                if (!await reader.ReadAsync(ct).ConfigureAwait(false))
                {
                    tx.Rollback();
                    return false;
                }

                var senderId = reader.GetInt64(0);
                var receiverId = reader.GetInt64(1);
                var status = reader.GetString(2);
                await reader.CloseAsync().ConfigureAwait(false);

                if (status != "Open")
                {
                    tx.Rollback();
                    return false;
                }

                if (userId != receiverId && userId != senderId)
                {
                    tx.Rollback();
                    return false;
                }

                using var updateCmd = new NpgsqlCommand(@"
                    UPDATE trades SET status = 'Declined', updated_at = now()
                    WHERE id = @id", conn, tx);
                updateCmd.Parameters.AddWithValue("id", tradeId);
                await updateCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

                using var histCmd = new NpgsqlCommand(@"
                    INSERT INTO trade_history (trade_id, action, actor_id)
                    VALUES (@tradeId, 'declined', @actorId)", conn, tx);
                histCmd.Parameters.AddWithValue("tradeId", tradeId);
                histCmd.Parameters.AddWithValue("actorId", userId);
                await histCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

                tx.Commit();
                return true;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task<long> CounterTradeAsync(
            long originalTradeId, long userId, string tradeJson,
            CancellationToken ct = default)
        {
            // Create the new trade FIRST (with swapped roles) so the original
            // is untouched if this fails. The counter-initiator becomes the sender.
            await using var outerConn = await OpenAsync(ct).ConfigureAwait(false);
            using var outerTx = outerConn.BeginTransaction();
            try
            {
                // Read original trade to determine role swap
                using var checkCmd = new NpgsqlCommand(@"
                    SELECT sender_id, receiver_id, status
                    FROM trades WHERE id = @id FOR UPDATE", outerConn, outerTx);
                checkCmd.Parameters.AddWithValue("id", originalTradeId);
                await using var reader = await checkCmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
                if (!await reader.ReadAsync(ct).ConfigureAwait(false))
                {
                    await outerTx.RollbackAsync(ct).ConfigureAwait(false);
                    return 0;
                }

                var origSenderId = reader.GetInt64(0);
                var origReceiverId = reader.GetInt64(1);
                var status = reader.GetString(2);
                await reader.CloseAsync().ConfigureAwait(false);

                if (status != "Open")
                {
                    await outerTx.RollbackAsync(ct).ConfigureAwait(false);
                    return 0;
                }

                // Counter-initiator becomes the new sender; the other party is receiver
                var newSenderId = userId;
                var newReceiverId = userId == origSenderId ? origReceiverId : origSenderId;

                // Create new trade (opens its own connection/transaction)
                var newTradeId = await CreateTradeAsync(
                    newSenderId, newReceiverId, tradeJson, ct).ConfigureAwait(false);
                if (newTradeId == 0)
                {
                    await outerTx.RollbackAsync(ct).ConfigureAwait(false);
                    return 0;
                }

                // Mark original as Countered and record history
                using var updateCmd = new NpgsqlCommand(@"
                    UPDATE trades SET status = 'Countered', updated_at = now()
                    WHERE id = @id", outerConn, outerTx);
                updateCmd.Parameters.AddWithValue("id", originalTradeId);
                await updateCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

                using var histCmd = new NpgsqlCommand(@"
                    INSERT INTO trade_history (trade_id, action, actor_id)
                    VALUES (@tradeId, 'countered', @actorId)", outerConn, outerTx);
                histCmd.Parameters.AddWithValue("tradeId", originalTradeId);
                histCmd.Parameters.AddWithValue("actorId", userId);
                await histCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

                await outerTx.CommitAsync(ct).ConfigureAwait(false);
                return newTradeId;
            }
            catch
            {
                await outerTx.RollbackAsync(ct).ConfigureAwait(false);
                throw;
            }
        }

        public async Task<(bool IsValid, long? SenderId, long? ReceiverId)> ValidateTradeOwnershipAsync(
            long tradeId, long userId,
            CancellationToken ct = default)
        {
            await using var conn = await OpenAsync(ct).ConfigureAwait(false);
            using var cmd = new NpgsqlCommand(@"
                SELECT sender_id, receiver_id FROM trades WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("id", tradeId);
            await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
            if (!await reader.ReadAsync(ct).ConfigureAwait(false))
                return (false, null, null);

            var senderId = reader.GetInt64(0);
            var receiverId = reader.GetInt64(1);
            var isOwner = senderId == userId || receiverId == userId;
            return (isOwner, senderId, receiverId);
        }

        private async Task TransferItemsAsync(
            NpgsqlConnection conn, NpgsqlTransaction tx,
            long tradeId, long senderId, long receiverId,
            CancellationToken ct)
        {
            using var offerCmd = new NpgsqlCommand(@"
                SELECT user_asset_id, asset_id, serial_number FROM trade_items
                WHERE trade_id = @tradeId AND agent_id = @agentId AND side = 'offer'", conn, tx);
            offerCmd.Parameters.AddWithValue("tradeId", tradeId);
            offerCmd.Parameters.AddWithValue("agentId", senderId);
            await using var offerReader = await offerCmd.ExecuteReaderAsync(ct).ConfigureAwait(false);

            var items = new List<(long UserAssetId, long AssetId, long? SerialNumber)>();
            while (await offerReader.ReadAsync(ct).ConfigureAwait(false))
            {
                var serial = offerReader.IsDBNull(2) ? (long?)null : offerReader.GetInt64(2);
                items.Add((offerReader.GetInt64(0), offerReader.GetInt64(1), serial));
            }
            await offerReader.CloseAsync().ConfigureAwait(false);

            foreach (var (userAssetId, assetId, serialNumber) in items)
            {
                using var transferCmd = new NpgsqlCommand(@"
                    UPDATE user_assets SET user_id = @newOwner
                    WHERE user_asset_id = @userAssetId AND user_id = @oldOwner", conn, tx);
                transferCmd.Parameters.AddWithValue("newOwner", receiverId);
                transferCmd.Parameters.AddWithValue("userAssetId", userAssetId);
                transferCmd.Parameters.AddWithValue("oldOwner", senderId);
                await transferCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

                if (serialNumber.HasValue)
                {
                    using var serialCmd = new NpgsqlCommand(@"
                        UPDATE asset_serials SET owner_user_id = @newOwner
                        WHERE asset_id = @assetId AND serial_number = @serialNumber", conn, tx);
                    serialCmd.Parameters.AddWithValue("newOwner", receiverId);
                    serialCmd.Parameters.AddWithValue("assetId", assetId);
                    serialCmd.Parameters.AddWithValue("serialNumber", serialNumber.Value);
                    await serialCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                }
            }

            using var reqCmd = new NpgsqlCommand(@"
                SELECT user_asset_id, asset_id, serial_number FROM trade_items
                WHERE trade_id = @tradeId AND agent_id = @agentId AND side = 'request'", conn, tx);
            reqCmd.Parameters.AddWithValue("tradeId", tradeId);
            reqCmd.Parameters.AddWithValue("agentId", receiverId);
            await using var reqReader = await reqCmd.ExecuteReaderAsync(ct).ConfigureAwait(false);

            var reqItems = new List<(long UserAssetId, long AssetId, long? SerialNumber)>();
            while (await reqReader.ReadAsync(ct).ConfigureAwait(false))
            {
                var serial = reqReader.IsDBNull(2) ? (long?)null : reqReader.GetInt64(2);
                reqItems.Add((reqReader.GetInt64(0), reqReader.GetInt64(1), serial));
            }
            await reqReader.CloseAsync().ConfigureAwait(false);

            foreach (var (userAssetId, assetId, serialNumber) in reqItems)
            {
                using var transferCmd = new NpgsqlCommand(@"
                    UPDATE user_assets SET user_id = @newOwner
                    WHERE user_asset_id = @userAssetId AND user_id = @oldOwner", conn, tx);
                transferCmd.Parameters.AddWithValue("newOwner", senderId);
                transferCmd.Parameters.AddWithValue("userAssetId", userAssetId);
                transferCmd.Parameters.AddWithValue("oldOwner", receiverId);
                await transferCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

                if (serialNumber.HasValue)
                {
                    using var serialCmd = new NpgsqlCommand(@"
                        UPDATE asset_serials SET owner_user_id = @newOwner
                        WHERE asset_id = @assetId AND serial_number = @serialNumber", conn, tx);
                    serialCmd.Parameters.AddWithValue("newOwner", senderId);
                    serialCmd.Parameters.AddWithValue("assetId", assetId);
                    serialCmd.Parameters.AddWithValue("serialNumber", serialNumber.Value);
                    await serialCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                }
            }
        }

        private static async Task<long?> GetSerialNumberForUserAssetAsync(
            NpgsqlConnection conn, NpgsqlTransaction tx,
            long userAssetId, long assetId, long ownerId,
            CancellationToken ct)
        {
            using var cmd = new NpgsqlCommand(@"
                SELECT serial_number FROM asset_serials
                WHERE asset_id = @assetId AND owner_user_id = @ownerId", conn, tx);
            cmd.Parameters.AddWithValue("assetId", assetId);
            cmd.Parameters.AddWithValue("ownerId", ownerId);
            var result = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
            return result == null || result is DBNull ? null : (long)result;
        }

        private static string FormatExpires(DateTime expiresAt)
        {
            var remaining = expiresAt - DateTime.UtcNow;
            if (remaining.TotalDays > 1)
                return $"in {Math.Floor(remaining.TotalDays)} days";
            if (remaining.TotalHours > 1)
                return $"in {Math.Floor(remaining.TotalHours)} hours";
            return "soon";
        }
    }
}
