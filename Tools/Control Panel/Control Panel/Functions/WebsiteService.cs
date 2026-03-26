using System;
using System.Threading.Tasks;
using Npgsql;
using Control_Panel;

namespace ControlPanel.Functions
{
    public class WebsiteService
    {
        private readonly string _connectionString;

        public WebsiteService(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<WebsiteSettings> GetWebsiteSettingsAsync()
        {
            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();
                
                using var cmd = new NpgsqlCommand(@"
                    SELECT global_message, lockdown_mode_enabled, lockdown_mode_reason 
                    FROM website_settings 
                    LIMIT 1", conn);
                
                using var reader = await cmd.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    return new WebsiteSettings
                    {
                        GlobalMessage = reader["global_message"] as string,
                        MaintenanceModeEnabled = reader["lockdown_mode_enabled"] as bool? ?? false,
                        MaintenanceModeReason = reader["lockdown_mode_reason"] as string
                    };
                }
                
                return new WebsiteSettings
                {
                    GlobalMessage = null,
                    MaintenanceModeEnabled = false,
                    MaintenanceModeReason = null
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve website settings: {ex.Message}", ex);
            }
        }

        public async Task UpdateGlobalMessageAsync(string message)
        {
            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();
                
                using var cmd = new NpgsqlCommand(@"
                    UPDATE website_settings 
                    SET global_message = @message", conn);
                
                cmd.Parameters.AddWithValue("message", (object)message ?? DBNull.Value);
                
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update global message: {ex.Message}", ex);
            }
        }

        public async Task UpdateMaintenanceModeAsync(bool enabled, string reason = null)
        {
            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();
                
                using var cmd = new NpgsqlCommand(@"
                    UPDATE website_settings 
                    SET lockdown_mode_enabled = @enabled, 
                        lockdown_mode_reason = @reason", conn);
                
                cmd.Parameters.AddWithValue("enabled", enabled);
                cmd.Parameters.AddWithValue("reason", (object)reason ?? DBNull.Value);
                
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update Maintenance mode: {ex.Message}", ex);
            }
        }

        public async Task<bool> IsMaintenanceModeEnabledAsync()
        {
            try
            {
                var settings = await GetWebsiteSettingsAsync();
                return settings.MaintenanceModeEnabled;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to check Maintenance mode status: {ex.Message}", ex);
            }
        }
    }

    public class WebsiteSettings
    {
        public string GlobalMessage { get; set; }
        public bool MaintenanceModeEnabled { get; set; }
        public string MaintenanceModeReason { get; set; }
    }
}
