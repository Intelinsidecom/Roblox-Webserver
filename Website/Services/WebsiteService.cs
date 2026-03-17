using System;
using System.Threading.Tasks;
using Npgsql;

namespace Website.Services
{
    public class WebsiteService
    {
        private readonly string _connectionString;

        public WebsiteService(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<string> GetGlobalMessageAsync()
        {
            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();
                
                using var cmd = new NpgsqlCommand(@"
                    SELECT global_message 
                    FROM website_settings 
                    LIMIT 1", conn);
                
                var result = await cmd.ExecuteScalarAsync();
                return result as string;
            }
            catch (Exception ex)
            {
                // Log error but don't throw - global message failure shouldn't break the site
                Console.WriteLine($"Failed to retrieve global message: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> IsMaintenanceModeEnabledAsync()
        {
            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();
                
                using var cmd = new NpgsqlCommand(@"
                    SELECT lockdown_mode_enabled 
                    FROM website_settings 
                    LIMIT 1", conn);
                
                var result = await cmd.ExecuteScalarAsync();
                return result as bool? ?? false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to check Maintenance mode: {ex.Message}");
                return false;
            }
        }

        public async Task<string> GetMaintenanceReasonAsync()
        {
            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();
                
                using var cmd = new NpgsqlCommand(@"
                    SELECT lockdown_mode_reason 
                    FROM website_settings 
                    LIMIT 1", conn);
                
                var result = await cmd.ExecuteScalarAsync();
                return result as string;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to retrieve Maintenance reason: {ex.Message}");
                return null;
            }
        }
    }
}
