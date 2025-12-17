using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Thumbnails;
using Avatar;

namespace Website.Services;

public sealed class AvatarThumbnailRefreshService
{
    private static readonly ConcurrentDictionary<long, DateTime> _lastWarmTimes = new();

    private readonly IThumbnailService _thumbnailService;
    private readonly IConfiguration _configuration;

    public AvatarThumbnailRefreshService(IThumbnailService thumbnailService, IConfiguration configuration)
    {
        _thumbnailService = thumbnailService;
        _configuration = configuration;
    }

    public async Task WarmAvatarAndHeadshotAsync(long userId, CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
            throw new ArgumentOutOfRangeException(nameof(userId));

        // Per-user debounce: avoid spamming RCC with redundant thumbnail renders
        // when callers rapidly toggle avatar state (e.g., equip/unequip spam).
        // This is in addition to the hash-based early-exit below and ensures
        // we do not re-render thumbnails more often than necessary even when
        // avatar_state_hash flips back and forth between previously seen
        // configurations whose hashed thumbnails are already cached.
        var now = DateTime.UtcNow;
        if (_lastWarmTimes.TryGetValue(userId, out var last) &&
            (now - last) < TimeSpan.FromSeconds(2))
        {
            return;
        }
        _lastWarmTimes[userId] = now;

        var connStr = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connStr))
            return;

        string? currentStateHash = null;
        string? lastThumbnailHash = null;

        try
        {
            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sql = "select avatar_state_hash, last_avatar_thumbnail from users where user_id = @id";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", userId);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                currentStateHash = reader.IsDBNull(0) ? null : reader.GetString(0);
                lastThumbnailHash = reader.IsDBNull(1) ? null : reader.GetString(1);
            }
        }
        catch
        {
        }

        if (!string.IsNullOrEmpty(currentStateHash) &&
            string.Equals(currentStateHash, lastThumbnailHash, StringComparison.Ordinal))
        {
            return;
        }

        var baseUrl = _configuration["Thumbnails:ThumbnailUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            // When called outside of an HTTP context, we rely solely on configured base URL.
            // If it is not configured, we cannot build absolute URLs, so bail out quietly.
            return;
        }

        // If this avatar configuration has already been rendered and stored in the
        // global avatar thumbnail cache, reuse it instead of opening a new RCC
        // render. This covers cases where the user toggles back to a previously
        // seen outfit (same config hash) even though last_avatar_thumbnail may
        // refer to a different, more recent configuration.
        try
        {
            var configBuilder = new AvatarRenderConfigBuilder();
            var config = await configBuilder
                .BuildAvatarRenderConfigAsync(connStr!, userId, "avatar", 420, 420, cancellationToken)
                .ConfigureAwait(false);

            var cacheRepo = new AvatarThumbnailCacheRepository();
            var (found, fileName) = await cacheRepo.TryGetAsync(connStr!, config.configHash, cancellationToken).ConfigureAwait(false);
            if (found && !string.IsNullOrWhiteSpace(fileName))
            {
                var cachedUrl = CombineUrl(baseUrl!, fileName!);
                await ThumbnailQueries.SetUserThumbnailUrlAsync(connStr!, userId, cachedUrl, cancellationToken).ConfigureAwait(false);

                // Update last_avatar_thumbnail to the current avatar_state_hash so
                // subsequent warm calls for this configuration can short-circuit
                // on the hash equality check above.
                try
                {
                    await using var conn = new NpgsqlConnection(connStr);
                    await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

                    const string updateSql = "update users set last_avatar_thumbnail = avatar_state_hash where user_id = @id";
                    await using var cmd = new NpgsqlCommand(updateSql, conn);
                    cmd.Parameters.AddWithValue("id", userId);
                    await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                }
                catch
                {
                }

                // We have fully satisfied the warm request from cache; no RCC call.
                return;
            }
        }
        catch
        {
            // Cache is best-effort; fall back to rendering below.
        }

        // Best-effort: render avatar and headshot and persist URLs. Any failure should not
        // break the caller since this is an optimization for smoother loading.
        try
        {
            var avatarSave = await _thumbnailService.RenderAvatarAsync("avatar", userId, cancellationToken: cancellationToken).ConfigureAwait(false);
            var avatarUrl = CombineUrl(baseUrl!, avatarSave.FileName);
            await ThumbnailQueries.SetUserThumbnailUrlAsync(connStr!, userId, avatarUrl, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            // Ignore avatar failures.
        }

        try
        {
            var headshotSave = await _thumbnailService.RenderAvatarAsync("headshot", userId, cancellationToken: cancellationToken).ConfigureAwait(false);
            var headshotUrl = CombineUrl(baseUrl!, headshotSave.FileName);
            await ThumbnailQueries.SetUserHeadshotUrlAsync(connStr!, userId, headshotUrl, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            // Ignore headshot failures.
        }

        try
        {
            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string updateSql = "update users set last_avatar_thumbnail = avatar_state_hash where user_id = @id";
            await using var cmd = new NpgsqlCommand(updateSql, conn);
            cmd.Parameters.AddWithValue("id", userId);
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
        }
    }

    private static string CombineUrl(string baseUrl, string relative)
    {
        if (string.IsNullOrEmpty(baseUrl)) return relative;
        if (string.IsNullOrEmpty(relative)) return baseUrl;
        var trimmedBase = baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/";
        return trimmedBase + relative.TrimStart('/');
    }
}
