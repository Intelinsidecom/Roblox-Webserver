using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Games
{
    /// <summary>
    /// Background service that periodically cleans up stale game presence entries
    /// to ensure player counts are accurate when users disconnect without proper cleanup
    /// </summary>
    public class GamePresenceCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<GamePresenceCleanupService> _logger;
        private readonly TimeSpan _cleanupInterval;
        private readonly TimeSpan _staleTimeout;

        public GamePresenceCleanupService(
            IServiceProvider serviceProvider,
            ILogger<GamePresenceCleanupService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _cleanupInterval = TimeSpan.FromSeconds(15);
            _staleTimeout = TimeSpan.FromSeconds(30);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_cleanupInterval, stoppingToken);
                    
                    if (stoppingToken.IsCancellationRequested)
                        break;

                    await CleanupStalePresencesAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during game presence cleanup");
                }
            }
        }

        private async Task CleanupStalePresencesAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var presenceService = scope.ServiceProvider.GetRequiredService<GamePresenceService>();

            try
            {
                await presenceService.CleanupStalePresenceAsync(_staleTimeout);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cleanup stale presences");
            }
        }
    }
}
