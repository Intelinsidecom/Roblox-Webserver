using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace Games;

public sealed class DatastoreEntry
{
    public long PlaceId { get; set; }
    public string Scope { get; set; } = "";
    public string Target { get; set; } = "";
    public string Key { get; set; } = "";
    public string? Value { get; set; }
    public double? SortKey { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class DataStoreQuery
{
    public string Scope { get; set; } = "";
    public string Target { get; set; } = "";
    public string Key { get; set; } = "";
}

public static class Datastore
{
    public const int MaxValueSize = 64 * 1024;
    public const int MaxPageSize = 100;

    private static readonly Regex QKeyRegex = new Regex(@"^qkeys\[(\d+)\]\.(scope|target|key)$", RegexOptions.Compiled);

    /// <summary>
    /// Handles POST /persistence/getV2. Returns { "data": [...] } where each entry is
    /// { "Value": &lt;raw json&gt;, "Scope", "Key", "Target" }.
    /// </summary>
    public static async Task<JObject> GetAsync(string connectionString, long placeId, IReadOnlyList<DataStoreQuery> queries, CancellationToken cancellationToken = default)
    {
        ValidateConnectionString(connectionString);

        var data = new JArray();
        if (queries.Count > 0)
        {
            using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            foreach (var query in queries)
            {
                const string sql = @"SELECT value
                                       FROM datastore_entries
                                      WHERE place_id = @placeId
                                        AND scope = @scope
                                        AND target = @target
                                        AND key = @key";

                using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("placeId", placeId);
                cmd.Parameters.AddWithValue("scope", query.Scope ?? "");
                cmd.Parameters.AddWithValue("target", query.Target ?? "");
                cmd.Parameters.AddWithValue("key", query.Key ?? "");

                var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                if (result == null || result == DBNull.Value)
                    continue;

                data.Add(new JObject
                {
                    ["Value"] = ParseJson(result.ToString()),
                    ["Scope"] = query.Scope ?? "",
                    ["Key"] = query.Key ?? "",
                    ["Target"] = query.Target ?? ""
                });
            }
        }

        return new JObject { ["data"] = data };
    }

    /// <summary>
    /// Handles POST /persistence/set. Supports plain set (expectedValueJson == null),
    /// set-if with a specific expected value, and set-if expecting the key to be absent
    /// (expectedValueJson == ""). Returns { "data": &lt;raw json&gt; } on success or
    /// { "error": ..., "currentValue": &lt;raw json&gt; } on CAS conflict.
    /// </summary>
    public static async Task<JObject> SetAsync(string connectionString, long placeId, string scope, string target, string key, string valueJson, string? expectedValueJson, bool isOrdered, CancellationToken cancellationToken = default)
    {
        ValidateConnectionString(connectionString);

        JToken valueToken;
        try
        {
            valueToken = JToken.Parse(valueJson);
        }
        catch (JsonException)
        {
            return Error("Value is not valid JSON");
        }

        if (valueJson.Length >= MaxValueSize)
            return Error("Value is too large");

        double? sortKey = null;
        if (isOrdered)
        {
            if (!TryGetInteger(valueToken, out var intValue))
                return Error("Value must be an integer for ordered data stores");
            sortKey = intValue;
        }

        bool expectNil = expectedValueJson != null && string.IsNullOrEmpty(expectedValueJson);
        JToken? expectedToken = null;
        if (expectedValueJson != null && !expectNil)
        {
            try
            {
                expectedToken = JToken.Parse(expectedValueJson);
            }
            catch (JsonException)
            {
                return Error("Expected value is not valid JSON");
            }
        }

        using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        if (expectedValueJson == null)
        {
            const string upsertSql = @"INSERT INTO datastore_entries (place_id, scope, target, key, value, sort_key, updated_at)
                                       VALUES (@placeId, @scope, @target, @key, @value::jsonb, @sortKey, now())
                                       ON CONFLICT (place_id, scope, target, key)
                                       DO UPDATE SET
                                           value = EXCLUDED.value,
                                           sort_key = EXCLUDED.sort_key,
                                           updated_at = now()";

            using var cmd = new NpgsqlCommand(upsertSql, conn);
            cmd.Parameters.AddWithValue("placeId", placeId);
            cmd.Parameters.AddWithValue("scope", scope ?? "");
            cmd.Parameters.AddWithValue("target", target ?? "");
            cmd.Parameters.AddWithValue("key", key ?? "");
            cmd.Parameters.AddWithValue("value", valueJson);
            cmd.Parameters.AddWithValue("sortKey", (object?)sortKey ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            return new JObject { ["data"] = valueToken };
        }

        bool won;
        if (expectNil)
        {
            const string insertSql = @"INSERT INTO datastore_entries (place_id, scope, target, key, value, sort_key, updated_at)
                                       VALUES (@placeId, @scope, @target, @key, @value::jsonb, @sortKey, now())
                                       ON CONFLICT (place_id, scope, target, key) DO NOTHING";

            using var cmd = new NpgsqlCommand(insertSql, conn);
            cmd.Parameters.AddWithValue("placeId", placeId);
            cmd.Parameters.AddWithValue("scope", scope ?? "");
            cmd.Parameters.AddWithValue("target", target ?? "");
            cmd.Parameters.AddWithValue("key", key ?? "");
            cmd.Parameters.AddWithValue("value", valueJson);
            cmd.Parameters.AddWithValue("sortKey", (object?)sortKey ?? DBNull.Value);
            won = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) > 0;
        }
        else
        {
            const string casSql = @"UPDATE datastore_entries
                                       SET value = @value::jsonb,
                                           sort_key = @sortKey,
                                           updated_at = now()
                                     WHERE place_id = @placeId
                                       AND scope = @scope
                                       AND target = @target
                                       AND key = @key
                                       AND value = @expected::jsonb";

            using var cmd = new NpgsqlCommand(casSql, conn);
            cmd.Parameters.AddWithValue("placeId", placeId);
            cmd.Parameters.AddWithValue("scope", scope ?? "");
            cmd.Parameters.AddWithValue("target", target ?? "");
            cmd.Parameters.AddWithValue("key", key ?? "");
            cmd.Parameters.AddWithValue("value", valueJson);
            cmd.Parameters.AddWithValue("sortKey", (object?)sortKey ?? DBNull.Value);
            cmd.Parameters.AddWithValue("expected", expectedValueJson!);
            won = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) > 0;
        }

        if (won)
            return new JObject { ["data"] = valueToken };

        var currentValue = await FetchValueAsync(conn, placeId, scope!, target!, key!, cancellationToken).ConfigureAwait(false);
        var current = currentValue != null ? ParseJson(currentValue) : JValue.CreateNull();
        return new JObject
        {
            ["error"] = "Value changed since expected",
            ["currentValue"] = current
        };
    }

    /// <summary>
    /// Handles POST /persistence/increment. Returns { "data": &lt;new number&gt; }.
    /// </summary>
    public static async Task<JObject> IncrementAsync(string connectionString, long placeId, string scope, string target, string key, long delta, CancellationToken cancellationToken = default)
    {
        ValidateConnectionString(connectionString);

        try
        {
            using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = @"INSERT INTO datastore_entries (place_id, scope, target, key, value, sort_key, updated_at)
                                 VALUES (@placeId, @scope, @target, @key, to_jsonb(@delta), NULL, now())
                                 ON CONFLICT (place_id, scope, target, key)
                                 DO UPDATE SET
                                     value = to_jsonb(COALESCE((datastore_entries.value #>> '{}')::numeric, 0) + @delta),
                                     sort_key = NULL,
                                     updated_at = now()
                                 RETURNING value";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("placeId", placeId);
            cmd.Parameters.AddWithValue("scope", scope ?? "");
            cmd.Parameters.AddWithValue("target", target ?? "");
            cmd.Parameters.AddWithValue("key", key ?? "");
            cmd.Parameters.AddWithValue("delta", delta);

            var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            if (result == null || result == DBNull.Value)
                return Error("Failed to increment value");

            return new JObject { ["data"] = ParseJson(result.ToString()) };
        }
        catch (PostgresException ex) when (ex.SqlState == "22P02" || ex.SqlState == "22023")
        {
            return Error("Cannot increment a non-numeric value");
        }
    }

    /// <summary>
    /// Handles POST /persistence/getSortedValues. Returns
    /// { "data": { "Entries": [ { "Target", "Value" }, ... ], "ExclusiveStartKey": "..." } }.
    /// </summary>
    public static async Task<JObject> GetSortedValuesAsync(string connectionString, long placeId, string scope, string key, int pageSize, bool ascending, double? inclusiveMin, double? inclusiveMax, string? exclusiveStartKey, CancellationToken cancellationToken = default)
    {
        ValidateConnectionString(connectionString);

        if (pageSize < 0)
            pageSize = 0;
        if (pageSize > MaxPageSize)
            pageSize = MaxPageSize;

        double? startSortKey = null;
        string? startTarget = null;
        if (!string.IsNullOrEmpty(exclusiveStartKey))
        {
            var decoded = DecodeCursor(exclusiveStartKey!);
            if (decoded == null)
                return Error("Invalid exclusive start key");
            startSortKey = decoded.Value.Value;
            startTarget = decoded.Value.Target;
        }

        var rows = new List<(string Target, double SortKey)>();
        using (var conn = new NpgsqlConnection(connectionString))
        {
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            var sql = new StringBuilder(@"SELECT target, sort_key
                                            FROM datastore_entries
                                           WHERE place_id = @placeId
                                             AND scope = @scope
                                             AND key = @key
                                             AND sort_key IS NOT NULL");

            var cmd = new NpgsqlCommand { Connection = conn };
            cmd.Parameters.AddWithValue("placeId", placeId);
            cmd.Parameters.AddWithValue("scope", scope ?? "");
            cmd.Parameters.AddWithValue("key", key ?? "");

            if (inclusiveMin.HasValue)
            {
                sql.Append(" AND sort_key >= @minValue");
                cmd.Parameters.AddWithValue("minValue", inclusiveMin.Value);
            }

            if (inclusiveMax.HasValue)
            {
                sql.Append(" AND sort_key <= @maxValue");
                cmd.Parameters.AddWithValue("maxValue", inclusiveMax.Value);
            }

            if (startSortKey.HasValue && startTarget != null)
            {
                if (ascending)
                    sql.Append(" AND (sort_key > @startSort OR (sort_key = @startSort AND target > @startTarget))");
                else
                    sql.Append(" AND (sort_key < @startSort OR (sort_key = @startSort AND target < @startTarget))");

                cmd.Parameters.AddWithValue("startSort", startSortKey.Value);
                cmd.Parameters.AddWithValue("startTarget", startTarget);
            }

            sql.Append(ascending ? " ORDER BY sort_key ASC, target ASC" : " ORDER BY sort_key DESC, target DESC");
            sql.Append(" LIMIT @pageSize");
            cmd.Parameters.AddWithValue("pageSize", pageSize);
            cmd.CommandText = sql.ToString();

            using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                rows.Add((reader.GetString(0), reader.GetDouble(1)));
            }
        }

        var entries = new JArray();
        foreach (var row in rows)
        {
            entries.Add(new JObject
            {
                ["Target"] = row.Target,
                ["Value"] = row.SortKey
            });
        }

        var data = new JObject { ["Entries"] = entries };
        if (rows.Count == pageSize && pageSize > 0)
        {
            var last = rows[rows.Count - 1];
            data["ExclusiveStartKey"] = EncodeCursor(last.Target, last.SortKey);
        }

        return new JObject { ["data"] = data };
    }

    /// <summary>
    /// Parses form keys of the form "qkeys[0].scope" / "qkeys[0].target" / "qkeys[0].key".
    /// </summary>
    public static List<DataStoreQuery> ParseQueries(IEnumerable<string> keys, Func<string, string?> getValue)
    {
        var dict = new SortedDictionary<int, DataStoreQuery>();
        foreach (var formKey in keys)
        {
            var match = QKeyRegex.Match(formKey);
            if (!match.Success)
                continue;

            if (!int.TryParse(match.Groups[1].Value, out var index))
                continue;

            if (!dict.TryGetValue(index, out var query))
            {
                query = new DataStoreQuery();
                dict[index] = query;
            }

            switch (match.Groups[2].Value)
            {
                case "scope": query.Scope = getValue(formKey) ?? ""; break;
                case "target": query.Target = getValue(formKey) ?? ""; break;
                case "key": query.Key = getValue(formKey) ?? ""; break;
            }
        }

        return new List<DataStoreQuery>(dict.Values);
    }

    private static async Task<string?> FetchValueAsync(NpgsqlConnection conn, long placeId, string scope, string target, string key, CancellationToken cancellationToken)
    {
        const string sql = @"SELECT value
                               FROM datastore_entries
                              WHERE place_id = @placeId
                                AND scope = @scope
                                AND target = @target
                                AND key = @key";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("placeId", placeId);
        cmd.Parameters.AddWithValue("scope", scope ?? "");
        cmd.Parameters.AddWithValue("target", target ?? "");
        cmd.Parameters.AddWithValue("key", key ?? "");

        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return result == null || result == DBNull.Value ? null : result.ToString();
    }

    private static bool TryGetInteger(JToken token, out long value)
    {
        value = 0;
        if (token.Type == JTokenType.Integer)
        {
            value = token.Value<long>();
            return true;
        }

        if (token.Type == JTokenType.Float)
        {
            var number = token.Value<double>();
            if (number >= long.MinValue && number <= long.MaxValue && number == Math.Floor(number))
            {
                value = (long)number;
                return true;
            }
        }

        return false;
    }

    private static JToken ParseJson(string json) => JToken.Parse(json);

    private static JObject Error(string message) => new JObject { ["error"] = message };

    private static string EncodeCursor(string target, double value)
    {
        var json = JsonConvert.SerializeObject(new { target, value });
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        return base64.TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static (string Target, double Value)? DecodeCursor(string cursor)
    {
        try
        {
            var base64 = cursor.Replace('-', '+').Replace('_', '/');
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
                default: break;
            }

            var json = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
            var obj = JObject.Parse(json);
            var target = (string?)obj["target"];
            var valueToken = obj["value"];
            if (target == null || valueToken == null || valueToken.Type != JTokenType.Float && valueToken.Type != JTokenType.Integer)
                return null;

            return (target, valueToken.Value<double>());
        }
        catch
        {
            return null;
        }
    }

    private static void ValidateConnectionString(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("connectionString is required", nameof(connectionString));
    }
}
