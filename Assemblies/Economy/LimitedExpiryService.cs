using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace Economy
{
    public sealed class LimitedExpiryService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(60);

        public LimitedExpiryService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var connStr = _configuration.GetConnectionString("Default");
                    if (!string.IsNullOrWhiteSpace(connStr))
                    {
                        await ExpireLimitedItemsAsync(connStr, stoppingToken).ConfigureAwait(false);
                    }
                }
                catch
                {
                }

                await Task.Delay(Interval, stoppingToken).ConfigureAwait(false);
            }
        }

        private static async Task ExpireLimitedItemsAsync(string connectionString, CancellationToken ct)
        {
            using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);

            const string sql = @"UPDATE assets
                                 SET on_sale = false, last_updated = now()
                                 WHERE limited_until IS NOT NULL
                                   AND limited_until <= now()
                                   AND on_sale = true
                                   AND coalesce(limited_remaining, 0) > 0";
            using var cmd = new NpgsqlCommand(sql, conn);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        }
    }
}
