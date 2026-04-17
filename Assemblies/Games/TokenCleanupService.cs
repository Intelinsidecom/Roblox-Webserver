using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Games
{
    /// <summary>
    /// Background service that periodically cleans up expired tokens from the database.
    /// Runs every 5 minutes to remove stale game tickets and old sessions.
    /// </summary>
    public class TokenCleanupService : BackgroundService
    {
        private readonly TokenService _tokenService;
        private readonly ILogger<TokenCleanupService> _logger;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5);

        public TokenCleanupService(
            TokenService tokenService,
            ILogger<TokenCleanupService> logger)
        {
            _tokenService = tokenService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _tokenService.CleanupExpiredTicketsAsync();
                    await _tokenService.CleanupInactiveSessionsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during token cleanup");
                }

                try
                {
                    await Task.Delay(_cleanupInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }
}

