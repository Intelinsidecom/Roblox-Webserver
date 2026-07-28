using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Website.Services;

public class PresenceUpdateService : BackgroundService
{
    private readonly ILogger<PresenceUpdateService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly PresenceTracker _tracker;
    private readonly string? _connectionString;

    private static readonly TimeSpan FlushInterval = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan WriteThrottle = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan StaleThreshold = TimeSpan.FromMinutes(5);

    public PresenceUpdateService(
        ILogger<PresenceUpdateService> logger,
        IServiceProvider serviceProvider,
        PresenceTracker tracker,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _tracker = tracker;
        _connectionString = configuration.GetConnectionString("Default");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(FlushInterval, stoppingToken).ConfigureAwait(false);
                await FlushAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error flushing presence data");
            }
        }
    }

    private async Task FlushAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
            return;

        var now = DateTime.UtcNow;
        var entries = _tracker.GetAll();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        foreach (var (userId, entry) in entries)
        {
            var timeSinceLastRequest = now - entry.LastRequestTime;
            if (timeSinceLastRequest > StaleThreshold)
                continue;

            var timeSinceLastWrite = now - entry.LastDbWriteTime;
            if (timeSinceLastWrite < WriteThrottle)
                continue;

            try
            {
                using var cmd = new NpgsqlCommand(@"
                    UPDATE users
                    SET last_activity = NOW(),
                        in_studio = CASE WHEN in_game THEN false ELSE @isStudio END
                    WHERE user_id = @userId", conn);
                cmd.Parameters.AddWithValue("userId", userId);
                cmd.Parameters.AddWithValue("isStudio", entry.IsStudio);
                await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

                _tracker.MarkFlushed(userId, now);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to flush presence for user {UserId}", userId);
            }
        }
    }
}
