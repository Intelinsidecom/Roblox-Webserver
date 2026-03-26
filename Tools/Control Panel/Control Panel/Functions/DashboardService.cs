using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using Control_Panel;
using Control_Panel.Properties;

namespace ControlPanel.Functions
{
    public class DashboardService
    {
        private readonly string _connectionString;
        private readonly ArbiterQueries _arbiterQueries;
        private readonly ServiceQueries _frontendQueries;
        private readonly ServiceQueries _cdnQueries;
        private readonly ServiceQueries _rccLogQueries;
        private readonly ServiceQueries _apiServiceQueries;

        public DashboardService(string connectionString, string arbiterUrl = null, string frontendUrl = null, string cdnUrl = null, string rccLogUrl = null, string apiServiceUrl = null)
        {
            _connectionString = connectionString;
            
            if (string.IsNullOrEmpty(arbiterUrl))
            {
                throw new ArgumentException("Arbiter URL is required and cannot be null or empty", nameof(arbiterUrl));
            }
            
            if (string.IsNullOrEmpty(frontendUrl))
            {
                throw new ArgumentException("Frontend URL is required and cannot be null or empty", nameof(frontendUrl));
            }
            
            if (string.IsNullOrEmpty(cdnUrl))
            {
                throw new ArgumentException("CDN URL is required and cannot be null or empty", nameof(cdnUrl));
            }
            
            _arbiterQueries = new ArbiterQueries();
            _frontendQueries = new ServiceQueries(frontendUrl, "Frontend Service");
            _cdnQueries = new ServiceQueries(cdnUrl, "CDN Service");
            
            if (!string.IsNullOrEmpty(rccLogUrl))
            {
                _rccLogQueries = new ServiceQueries(rccLogUrl, "RCC Log Service");
            }
            
            if (!string.IsNullOrEmpty(apiServiceUrl))
            {
                _apiServiceQueries = new ServiceQueries(apiServiceUrl, "API Service");
            }
        }

        public async Task<DashboardData> GetDashboardDataAsync()
        {
            var data = new DashboardData();
            
            try
            {
                var tasks = new List<Task>();
                var dbTask = GetDatabaseDataAsync(data);
                tasks.Add(dbTask);
                var arbiterTask = GetArbiterStatusWithTimeoutAsync();
                var frontendTask = GetFrontendDataWithTimeoutAsync(data);
                tasks.Add(arbiterTask);
                tasks.Add(frontendTask);
                
                var rccLogTask = _rccLogQueries != null ? GetRccLogDataWithTimeoutAsync(data) : null;
                if (rccLogTask != null)
                {
                    tasks.Add(rccLogTask);
                }
                
                var apiServiceTask = _apiServiceQueries != null ? GetApiServiceDataWithTimeoutAsync(data) : null;
                if (apiServiceTask != null)
                {
                    tasks.Add(apiServiceTask);
                }

                try
                {
                    await Task.WhenAll(tasks);
                }
                catch
                {
                }
                
                var arbiterStatus = await arbiterTask;
                data.ArbiterStatus = arbiterStatus.Status;
                data.ArbiterIsRunning = arbiterStatus.IsRunning;
                data.RccStatus = arbiterStatus.Status;
                data.RccVersion = arbiterStatus.Version;
                data.LastUpdated = DateTime.Now;
                
                if (rccLogTask != null)
                {
                    await rccLogTask;
                }
                
                if (apiServiceTask != null)
                {
                    await apiServiceTask;
                }
            }
            catch (Exception ex)
            {
                data.IsHealthy = false;
                data.ErrorMessage = ex.Message;
                data.LastUpdated = DateTime.Now;

                try
                {
                    var consoleWindow = Control_Panel.ConsoleWindow.Instance;
                    consoleWindow.WriteError($"Dashboard update failed: {ex.Message}");
                    consoleWindow.WriteError($"Stack trace: {ex.StackTrace}");
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine($"Dashboard error: {ex.Message}");
                }
            }
            
            return data;
        }

        private async Task GetDatabaseDataAsync(DashboardData data)
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    
                    data.ServerHealth = await CheckServerHealthAsync(conn);
                    data.IsHealthy = data.ServerHealth == "Healthy";
                    data.ActiveUsers = await GetActiveUsersCountAsync(conn);
                }
            }
            catch (Exception ex)
            {
                data.ServerHealth = "Error";
                data.IsHealthy = false;
                data.ActiveUsers = 0;
                
                try
                {
                    var consoleWindow = Control_Panel.ConsoleWindow.Instance;
                    consoleWindow.WriteError($"Database data collection failed: {ex.Message}");
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine($"Database data error: {ex.Message}");
                }
            }
        }

        private async Task<ArbiterStatus> GetArbiterStatusWithTimeoutAsync()
        {
            try
            {
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(10)); // 10 second timeout
                var statusTask = GetArbiterStatusAsync();
                var completedTask = await Task.WhenAny(statusTask, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    try
                    {
                        var consoleWindow = Control_Panel.ConsoleWindow.Instance;
                        consoleWindow.WriteWarning("Arbiter status check timed out after 10 seconds");
                    }
                    catch
                    {
                        System.Diagnostics.Debug.WriteLine("Arbiter status check timed out");
                    }
                    
                    return new ArbiterStatus 
                    { 
                        IsRunning = false, 
                        Status = "Timeout", 
                        LastChecked = DateTime.Now,
                        Error = "Request timed out after 10 seconds"
                    };
                }
                
                return await statusTask;
            }
            catch (Exception ex)
            {
                try
                {
                    var consoleWindow = Control_Panel.ConsoleWindow.Instance;
                    consoleWindow.WriteError($"Arbiter status check failed: {ex.Message}");
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine($"Arbiter status error: {ex.Message}");
                }
                
                return new ArbiterStatus 
                { 
                    IsRunning = false, 
                    Status = "Error", 
                    LastChecked = DateTime.Now,
                    Error = ex.Message
                };
            }
        }

        private async Task GetFrontendDataWithTimeoutAsync(DashboardData data)
        {
            try
            {
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(8));
                var frontendTask = GetFrontendDataAsync(data);
                var completedTask = await Task.WhenAny(frontendTask, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    data.WebsiteServiceStatus = new WebsiteServiceStatus
                    {
                        Status = "Unhealthy",
                        OverallIsOnline = false,
                        LastChecked = DateTime.Now
                    };
                    data.FrontendActiveUsers = -1;
                    data.FrontendUserError = "";
                    
                    try
                    {
                        var consoleWindow = Control_Panel.ConsoleWindow.Instance;
                        consoleWindow.WriteWarning("Frontend data collection timed out after 8 seconds");
                    }
                    catch
                    {
                        System.Diagnostics.Debug.WriteLine("Frontend data collection timed out");
                    }
                }
                else
                {
                    await frontendTask;
                }
            }
            catch (Exception ex)
            {
                data.WebsiteServiceStatus = new WebsiteServiceStatus
                {
                    Status = "Unhealthy",
                    OverallIsOnline = false,
                    LastChecked = DateTime.Now
                };
                data.FrontendActiveUsers = -1;
                data.FrontendUserError = "";
                try
                {
                    var consoleWindow = Control_Panel.ConsoleWindow.Instance;
                    consoleWindow.WriteError($"Frontend data collection failed: {ex.Message}");
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine($"Frontend data error: {ex.Message}");
                }
            }
        }

        private async Task GetRccLogDataWithTimeoutAsync(DashboardData data)
        {
            try
            {
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(8));
                var rccLogTask = GetRccLogDataAsync(data);
                
                var completedTask = await Task.WhenAny(rccLogTask, timeoutTask);
                
                if (completedTask == timeoutTask)
                {
                    data.RccLogStatus = "Unhealthy";
                    data.RccLogIsOnline = false;
                    data.RccLogErrorMessage = "";
                    
                    try
                    {
                        var consoleWindow = Control_Panel.ConsoleWindow.Instance;
                        consoleWindow.WriteWarning("RCC Log Service data collection timed out after 8 seconds");
                    }
                    catch
                    {
                        System.Diagnostics.Debug.WriteLine("RCC Log Service data collection timed out");
                    }
                }
                else
                {
                    await rccLogTask;
                }
            }
            catch (Exception ex)
            {
                data.RccLogStatus = "Unhealthy";
                data.RccLogIsOnline = false;
                data.RccLogErrorMessage = "";
                try
                {
                    var consoleWindow = Control_Panel.ConsoleWindow.Instance;
                    consoleWindow.WriteError($"RCC Log Service data collection failed: {ex.Message}");
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine($"RCC Log Service data error: {ex.Message}");
                }
            }
        }

        private async Task GetApiServiceDataWithTimeoutAsync(DashboardData data)
        {
            try
            {
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(8)); // 8 second timeout
                var apiServiceTask = GetApiServiceDataAsync(data);
                
                var completedTask = await Task.WhenAny(apiServiceTask, timeoutTask);
                
                if (completedTask == timeoutTask)
                {
                    data.ApiServiceStatus = "Unhealthy";
                    data.ApiServiceIsOnline = false;
                    data.ApiServiceErrorMessage = "";
                    
                    try
                    {
                        var consoleWindow = Control_Panel.ConsoleWindow.Instance;
                        consoleWindow.WriteWarning("API Service data collection timed out after 8 seconds");
                    }
                    catch
                    {
                        System.Diagnostics.Debug.WriteLine("API Service data collection timed out");
                    }
                }
                else
                {
                    await apiServiceTask;
                }
            }
            catch (Exception ex)
            {
                data.ApiServiceStatus = "Unhealthy";
                data.ApiServiceIsOnline = false;
                data.ApiServiceErrorMessage = "";
                try
                {
                    var consoleWindow = Control_Panel.ConsoleWindow.Instance;
                    consoleWindow.WriteError($"API Service data collection failed: {ex.Message}");
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine($"API Service data error: {ex.Message}");
                }
            }
        }

        private async Task<string> CheckServerHealthAsync(NpgsqlConnection conn)
        {
            try
            {
                using (var cmd = new NpgsqlCommand("SELECT 1", conn))
                {
                    await cmd.ExecuteScalarAsync();
                    return "Healthy";
                }
            }
            catch
            {
                return "Unhealthy";
            }
        }

        private async Task<long> GetActiveUsersCountAsync(NpgsqlConnection conn)
        {
            try
            {
                string sql = @"
                    SELECT COUNT(*) FROM users 
                    WHERE last_activity > NOW() - INTERVAL '7 days'";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    var result = await cmd.ExecuteScalarAsync();
                    return Convert.ToInt64(result);
                }
            }
            catch
            {
                return 0;
            }
        }

        private async Task<ArbiterStatus> GetArbiterStatusAsync()
        {
            try
            {
                var status = await _arbiterQueries.GetArbiterStatusAsync();
                string version = status.Version;
                try
                {
                    var versionData = await _arbiterQueries.GetArbiterVersionAsync();
                    if (versionData.IsSuccessful && !string.IsNullOrEmpty(versionData.RccVersion))
                    {
                        version = versionData.RccVersion;
                    }
                }
                catch (Exception versionEx)
                {
                    try
                    {
                        var consoleWindow = Control_Panel.ConsoleWindow.Instance;
                        consoleWindow.WriteError($"Failed to get arbiter version: {versionEx.Message}");
                    }
                    catch
                    {
                        System.Diagnostics.Debug.WriteLine($"Arbiter version error: {versionEx.Message}");
                    }
                }
                
                if (!string.IsNullOrEmpty(version))
                {
                    status.Version = version;
                }
                
                try
                {
                    var consoleWindow = Control_Panel.ConsoleWindow.Instance;
                    if (!string.IsNullOrEmpty(status.Error))
                    {
                        consoleWindow.WriteWarning($"Arbiter error: {status.Error}");
                    }
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine($"Arbiter status: {status.Status}");
                }
                
                return status;
            }
            catch (Exception ex)
            {
                try
                {
                    var consoleWindow = Control_Panel.ConsoleWindow.Instance;
                    consoleWindow.WriteError($"Failed to get arbiter status: {ex.Message}");
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine($"Arbiter status error: {ex.Message}");
                }
                
                return new ArbiterStatus 
                { 
                    IsRunning = false, 
                    Status = "Error", 
                    LastChecked = DateTime.Now 
                };
            }
        }

        private async Task GetFrontendDataAsync(DashboardData data)
        {
            try
            {
                var websiteServiceStatus = await _frontendQueries.GetWebsiteServiceStatusAsync();
                data.WebsiteServiceStatus = websiteServiceStatus;
                
                var userStats = await _frontendQueries.GetUserStatisticsAsync();
                if (userStats.IsSuccessful)
                {
                    data.FrontendActiveUsers = userStats.ActiveUsers;
                }
                else
                {
                    data.FrontendActiveUsers = -1;
                    data.FrontendUserError = "";
                }

            }
            catch (Exception ex)
            {
                data.WebsiteServiceStatus = new WebsiteServiceStatus
                {
                    Status = "Unhealthy",
                    OverallIsOnline = false,
                    LastChecked = DateTime.Now
                };
                data.FrontendActiveUsers = -1;
                data.FrontendUserError = "";
                
                try
                {
                    var consoleWindow = Control_Panel.ConsoleWindow.Instance;
                    consoleWindow.WriteError($"Frontend data collection failed: {ex.Message}");
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine($"Frontend data error: {ex.Message}");
                }
            }
        }

        private async Task GetRccLogDataAsync(DashboardData data)
        {
            try
            {
                var rccLogStatus = await _rccLogQueries.GetServiceStatusAsync();
                data.RccLogStatus = rccLogStatus.Status;
                data.RccLogIsOnline = rccLogStatus.IsOnline;
                data.RccLogResponseTime = rccLogStatus.ResponseTime;
                
                if (!string.IsNullOrEmpty(rccLogStatus.Status) && rccLogStatus.Status != "Healthy")
                {
                    data.RccLogErrorMessage = rccLogStatus.Status;
                }
            }
            catch (Exception ex)
            {
                data.RccLogStatus = "Unhealthy";
                data.RccLogIsOnline = false;
                data.RccLogErrorMessage = "";
                
                try
                {
                    var consoleWindow = Control_Panel.ConsoleWindow.Instance;
                    consoleWindow.WriteError($"RCC Log Service data collection failed: {ex.Message}");
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine($"RCC Log Service data error: {ex.Message}");
                }
            }
        }

        private async Task GetApiServiceDataAsync(DashboardData data)
        {
            try
            {
                var apiServiceStatus = await _apiServiceQueries.GetServiceStatusAsync();
                data.ApiServiceStatus = apiServiceStatus.Status;
                data.ApiServiceIsOnline = apiServiceStatus.IsOnline;
                data.ApiServiceResponseTime = apiServiceStatus.ResponseTime;
                
                if (!string.IsNullOrEmpty(apiServiceStatus.Status) && apiServiceStatus.Status != "Healthy")
                {
                    data.ApiServiceErrorMessage = apiServiceStatus.Status;
                }
            }
            catch (Exception ex)
            {
                data.ApiServiceStatus = "Unhealthy";
                data.ApiServiceIsOnline = false;
                data.ApiServiceErrorMessage = "";
                
                try
                {
                    var consoleWindow = Control_Panel.ConsoleWindow.Instance;
                    consoleWindow.WriteError($"API Service data collection failed: {ex.Message}");
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine($"API Service data error: {ex.Message}");
                }
            }
        }
    }

    public class DashboardData
    {
        public bool IsHealthy { get; set; }
        public string ServerHealth { get; set; } = "Unknown";
        public string ErrorMessage { get; set; }
        public DateTime LastUpdated { get; set; }
        public long ActiveUsers { get; set; }
        public string ArbiterStatus { get; set; }
        public bool ArbiterIsRunning { get; set; }
        public string RccStatus { get; set; }
        public string RccVersion { get; set; }
        public WebsiteServiceStatus WebsiteServiceStatus { get; set; }
        public long FrontendActiveUsers { get; set; }
        public string FrontendUserError { get; set; }
        public string RccLogStatus { get; set; } = "Unknown";
        public bool RccLogIsOnline { get; set; }
        public TimeSpan RccLogResponseTime { get; set; }
        public string RccLogErrorMessage { get; set; }
        public string ApiServiceStatus { get; set; } = "Unknown";
        public bool ApiServiceIsOnline { get; set; }
        public TimeSpan ApiServiceResponseTime { get; set; }
        public string ApiServiceErrorMessage { get; set; }
    }
}
