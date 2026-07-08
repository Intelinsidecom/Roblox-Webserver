using Assets;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Games;

public class PlaceVersionEntry
{
    public int Version { get; set; }
    public string Date { get; set; } = string.Empty;
    public string File_Hash { get; set; } = string.Empty;
}

public static class VersionHistory
{
    public static async Task<int> GetNextVersionAsync(string connectionString, long placeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));
        if (placeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(placeId));

        using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"SELECT place_version_history FROM assets WHERE asset_id = @placeId";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("placeId", placeId);

        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        if (result == null || result == DBNull.Value)
            return 1;

        var json = result as string;
        if (string.IsNullOrWhiteSpace(json) || json == "[]")
            return 1;

        try
        {
            var entries = JsonSerializer.Deserialize<List<PlaceVersionEntry>>(json);
            if (entries == null || entries.Count == 0)
                return 1;

            var maxVersion = 0;
            foreach (var entry in entries)
            {
                if (entry.Version > maxVersion)
                    maxVersion = entry.Version;
            }
            return maxVersion + 1;
        }
        catch
        {
            return 1;
        }
    }

    public static async Task AddVersionEntryAsync(string connectionString, long placeId, string oldFileHash, CancellationToken cancellationToken = default, AssetService? assetService = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));
        if (placeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(placeId));
        if (string.IsNullOrWhiteSpace(oldFileHash))
            return;

        var nextVersion = await GetNextVersionAsync(connectionString, placeId, cancellationToken).ConfigureAwait(false);
        var now = DateTime.Now.ToString("M/d/yyyy h:mm:ss tt");

        var newEntry = new PlaceVersionEntry
        {
            Version = nextVersion,
            Date = now,
            File_Hash = oldFileHash
        };

        var entryJson = JsonSerializer.Serialize(newEntry);

        using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"UPDATE assets 
                              SET place_version_history = COALESCE(place_version_history, '[]'::jsonb) || @entry
                              WHERE asset_id = @placeId";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.Add(new NpgsqlParameter("entry", NpgsqlTypes.NpgsqlDbType.Jsonb) { Value = entryJson });
        cmd.Parameters.AddWithValue("placeId", placeId);

        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        if (assetService != null)
        {
            _ = Task.Run(async () => await assetService.RenderAssetThumbnailAsync(placeId, cancellationToken).ConfigureAwait(false));
        }
    }

    public static async Task<List<PlaceVersionEntry>> GetVersionHistoryAsync(string connectionString, long placeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));
        if (placeId <= 0)
            throw new ArgumentOutOfRangeException(nameof(placeId));

        using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"SELECT place_version_history FROM assets WHERE asset_id = @placeId";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("placeId", placeId);

        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        if (result == null || result == DBNull.Value)
            return new List<PlaceVersionEntry>();

        var json = result as string;
        if (string.IsNullOrWhiteSpace(json) || json == "[]")
            return new List<PlaceVersionEntry>();

        try
        {
            return JsonSerializer.Deserialize<List<PlaceVersionEntry>>(json) ?? new List<PlaceVersionEntry>();
        }
        catch
        {
            return new List<PlaceVersionEntry>();
        }
    }

    public static async Task<List<PlaceVersionEntry>> GetAssetVersionHistoryAsync(string connectionString, long assetId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));
        if (assetId <= 0)
            throw new ArgumentOutOfRangeException(nameof(assetId));

        using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"SELECT place_version_history FROM assets WHERE asset_id = @assetId";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("assetId", assetId);

        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        if (result == null || result == DBNull.Value)
            return new List<PlaceVersionEntry>();

        var json = result as string;
        if (string.IsNullOrWhiteSpace(json) || json == "[]")
            return new List<PlaceVersionEntry>();

        try
        {
            return JsonSerializer.Deserialize<List<PlaceVersionEntry>>(json) ?? new List<PlaceVersionEntry>();
        }
        catch
        {
            return new List<PlaceVersionEntry>();
        }
    }

    public static async Task<bool> RevertToVersionAsync(string connectionString, long assetId, int version, AssetService? assetService = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));
        if (assetId <= 0)
            throw new ArgumentOutOfRangeException(nameof(assetId));
        if (version <= 0)
            throw new ArgumentOutOfRangeException(nameof(version));

        using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"SELECT place_version_history FROM assets WHERE asset_id = @assetId";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("assetId", assetId);

        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        if (result == null || result == DBNull.Value)
            return false;

        var json = result as string;
        if (string.IsNullOrWhiteSpace(json) || json == "[]")
            return false;

        string? fileHash = null;
        try
        {
            using var doc = JsonDocument.Parse(json!);
            foreach (var entry in doc.RootElement.EnumerateArray())
            {
                if (entry.TryGetProperty("Version", out var verProp) &&
                    verProp.TryGetInt32(out var ver) &&
                    ver == version &&
                    entry.TryGetProperty("File_Hash", out var hashProp))
                {
                    fileHash = hashProp.GetString();
                    break;
                }
            }
        }
        catch
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(fileHash))
            return false;

        var repo = new AssetsRepository();
        await repo.UpdateAssetContentHashAsync(connectionString, assetId, fileHash, cancellationToken).ConfigureAwait(false);

        if (assetService != null)
        {
            _ = Task.Run(async () => await assetService.RenderAssetThumbnailAsync(assetId, cancellationToken).ConfigureAwait(false));
        }

        return true;
    }
}
