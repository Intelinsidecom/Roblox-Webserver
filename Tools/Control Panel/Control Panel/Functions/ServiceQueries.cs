using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.Json;
using Control_Panel;

namespace ControlPanel.Functions
{
    public class ServiceQueries
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _serviceType;

        public ServiceQueries(string baseUrl, string serviceType)
        {
            if (string.IsNullOrEmpty(baseUrl))
                throw new ArgumentNullException(nameof(baseUrl));
                
            if (string.IsNullOrEmpty(serviceType))
                throw new ArgumentNullException(nameof(serviceType));
                
            _baseUrl = baseUrl;
            _serviceType = serviceType;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(8);
        }

        public async Task<ServiceStatus> GetServiceStatusAsync()
        {
            var status = new ServiceStatus
            {
                LastChecked = DateTime.Now,
                ServiceType = _serviceType
            };

            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/");
                
                if (response.IsSuccessStatusCode)
                {
                    status.IsOnline = true;
                    status.Status = "Healthy";
                    status.ResponseTime = response.Headers.Date != null ? 
                        DateTime.Now - response.Headers.Date.Value : TimeSpan.Zero;
                    status.StatusCode = (int)response.StatusCode;
                    
                    if (_serviceType == "API Service")
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        if (content.Contains("\"errors\"") && content.Contains("\"code\""))
                        {
                            status.Status = "Healthy - API Format";
                        }
                    }
                }
                else
                {
                    status.IsOnline = false;
                    status.Status = "Unhealthy";
                    status.StatusCode = (int)response.StatusCode;
                }
            }
            catch (HttpRequestException)
            {
                status.IsOnline = false;
                status.Status = "Unhealthy";
                
                try
                {
                    var consoleWindow = Control_Panel.ConsoleWindow.Instance;
                    consoleWindow.WriteError($"{_serviceType} HTTP request failed");
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine($"{_serviceType} HTTP request failed");
                }
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                status.IsOnline = false;
                status.Status = "Unhealthy";
                
                try
                {
                    var consoleWindow = Control_Panel.ConsoleWindow.Instance;
                    consoleWindow.WriteWarning($"{_serviceType} status check timed out after 8 seconds");
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine($"{_serviceType} status check timed out");
                }
            }
            catch (Exception)
            {
                status.IsOnline = false;
                status.Status = "Unhealthy";
                
                try
                {
                    var consoleWindow = Control_Panel.ConsoleWindow.Instance;
                    consoleWindow.WriteError($"{_serviceType} status check failed");
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine($"{_serviceType} status check failed");
                }
            }

            return status;
        }

        public async Task<ServiceInfo> GetServiceInfoAsync()
        {
            var info = new ServiceInfo
            {
                LastChecked = DateTime.Now,
                ServiceType = _serviceType
            };

            try
            {
                string endpoint = _serviceType == "API Service" ? "/swagger" : "/health";
                
                var response = await _httpClient.GetAsync($"{_baseUrl}{endpoint}");
                
                if (response.IsSuccessStatusCode)
                {
                    info.IsHealthy = true;
                    info.Status = "Running";
                    info.StatusCode = (int)response.StatusCode;
                    
                    if (_serviceType == "API Service")
                    {
                        info.ServiceDescription = "Swagger documentation available";
                    }
                    else
                    {
                        info.ServiceDescription = "Service responding";
                    }
                }
                else
                {
                    var basicResponse = await _httpClient.GetAsync($"{_baseUrl}/");
                    if (basicResponse.IsSuccessStatusCode)
                    {
                        info.IsHealthy = true;
                        info.Status = "Running";
                        info.StatusCode = (int)basicResponse.StatusCode;
                        info.ServiceDescription = "Service responding";
                    }
                    else
                    {
                        info.IsHealthy = false;
                        info.Status = "Unhealthy";
                        info.StatusCode = (int)basicResponse.StatusCode;
                    }
                }
            }
            catch (Exception)
            {
                info.IsHealthy = false;
                info.Status = "Error";
            }

            return info;
        }

        public async Task<WebsiteStatus> GetWebsiteStatusAsync()
        {
            var status = new WebsiteStatus
            {
                LastChecked = DateTime.Now
            };

            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/");
                
                if (response.IsSuccessStatusCode)
                {
                    status.IsOnline = true;
                    status.Status = "Healthy";
                    status.ResponseTime = response.Headers.Date != null ? 
                        DateTime.Now - response.Headers.Date.Value : TimeSpan.Zero;
                    status.StatusCode = (int)response.StatusCode;
                }
                else
                {
                    status.IsOnline = false;
                    status.Status = "Unhealthy";
                    status.StatusCode = (int)response.StatusCode;
                    status.ErrorMessage = response.ReasonPhrase;
                }
            }
            catch (HttpRequestException ex)
            {
                status.IsOnline = false;
                status.Status = "Unhealthy";
                status.ErrorMessage = ex.Message;
                
                try
                {
                    var consoleWindow = Control_Panel.ConsoleWindow.Instance;
                }
                catch
                {
                    Debug.WriteLine($"Website status error: {ex.Message}");
                }
            }
            catch (TaskCanceledException ex)
            {
                status.IsOnline = false;
                status.Status = "Unhealthy";
                status.ErrorMessage = "Request timed out after 8 seconds";
                
                try
                {
                    var consoleWindow = Control_Panel.ConsoleWindow.Instance;
                }
                catch
                {
                    Debug.WriteLine($"Website status timeout: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                status.IsOnline = false;
                status.Status = "Unhealthy";
                status.ErrorMessage = ex.Message;

                try
                {
                    var consoleWindow = Control_Panel.ConsoleWindow.Instance;
                }
                catch
                {
                    Debug.WriteLine($"Website status error: {ex.Message}");
                }
            }

            return status;
        }

        public async Task<ApiStatus> GetApiStatusAsync()
        {
            var status = new ApiStatus
            {
                LastChecked = DateTime.Now
            };

            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/ping");
                
                var stopwatch = Stopwatch.StartNew();
                stopwatch.Start();
                
                if (response.IsSuccessStatusCode)
                {
                    status.IsOnline = true;
                    status.Status = "Healthy";
                    status.ResponseTime = stopwatch.Elapsed;
                    status.StatusCode = (int)response.StatusCode;
                }
                else
                {
                    status.IsOnline = false;
                    status.Status = "Unhealthy";
                    status.StatusCode = (int)response.StatusCode;
                    status.ErrorMessage = response.ReasonPhrase;
                }
                
                stopwatch.Stop();
            }
            catch (HttpRequestException ex)
            {
                status.IsOnline = false;
                status.Status = "Unhealthy";
                status.ErrorMessage = ex.Message;
            }
            catch (TaskCanceledException ex)
            {
                status.IsOnline = false;
                status.Status = "Unhealthy";
                status.ErrorMessage = "Request timed out after 8 seconds";
            }
            catch (Exception ex)
            {
                status.IsOnline = false;
                status.Status = "Unhealthy";
                status.ErrorMessage = ex.Message;
            }

            return status;
        }

        public async Task<UserStatistics> GetUserStatisticsAsync()
        {
            var stats = new UserStatistics
            {
                LastChecked = DateTime.Now
            };

            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/stats/users");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    // TODO - actually add api in website for active users, i should pull out ecsv2 for that, but not in webserver, in my private version if i make one.
                    stats.ActiveUsers = ParseActiveUsersFromResponse(content);
                    stats.TotalUsers = ParseTotalUsersFromResponse(content);
                    stats.IsSuccessful = true;
                }
                else
                {
                    stats.IsSuccessful = false;
                    stats.ErrorMessage = $"API returned status {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                stats.IsSuccessful = false;
                stats.ErrorMessage = ex.Message;
            }

            return stats;
        }

        public async Task<CdnStatus> GetCdnStatusAsync()
        {
            var status = new CdnStatus
            {
                ServiceUrl = _baseUrl,
                LastChecked = DateTime.Now
            };

            try
            {
                var stopwatch = Stopwatch.StartNew();
                var response = await _httpClient.GetAsync(_baseUrl);
                stopwatch.Stop();

                status.ResponseTime = stopwatch.Elapsed;
                status.IsOnline = response.IsSuccessStatusCode;
                
                if (response.IsSuccessStatusCode)
                {
                    status.Status = "Healthy";
                }
                else
                {
                    status.Status = "Unhealthy";
                    status.ErrorMessage = "Unhealthy";
                }
            }
            catch (HttpRequestException ex)
            {
                status.IsOnline = false;
                status.Status = "Unhealthy";
                status.ErrorMessage = "Unhealthy";
                status.ResponseTime = TimeSpan.Zero;
            }
            catch (TaskCanceledException ex)
            {
                status.IsOnline = false;
                status.Status = "Unhealthy";
                status.ErrorMessage = "Unhealthy";
                status.ResponseTime = TimeSpan.Zero;
            }
            catch (Exception ex)
            {
                status.IsOnline = false;
                status.Status = "Unhealthy";
                status.ErrorMessage = "Unhealthy";
                status.ResponseTime = TimeSpan.Zero;
            }

            return status;
        }

        private long ParseActiveUsersFromResponse(string content)
        {
            return 0;
        }

        private long ParseTotalUsersFromResponse(string content)
        {
            return 0;
        }

        public async Task<WebsiteServiceStatus> GetWebsiteServiceStatusAsync()
        {
            var websiteStatus = await GetWebsiteStatusAsync();
            var apiStatus = await GetApiStatusAsync();
            var cdnStatus = await GetCdnStatusAsync();
            var rccLogStatus = await GetServiceStatusAsync(); // Use generic service status for RCC Log

            var serviceStatus = new WebsiteServiceStatus
            {
                LastChecked = DateTime.Now,
                WebsiteIsOnline = websiteStatus.IsOnline,
                ApiIsOnline = apiStatus.IsOnline,
                CdnIsOnline = cdnStatus.IsOnline,
                RccLogIsOnline = rccLogStatus.IsOnline,
                WebsiteStatusCode = websiteStatus.StatusCode,
                ApiStatusCode = apiStatus.StatusCode,
                CdnStatusCode = cdnStatus.StatusCode,
                RccLogStatusCode = rccLogStatus.StatusCode,
                WebsiteResponseTime = websiteStatus.ResponseTime,
                ApiResponseTime = apiStatus.ResponseTime,
                CdnResponseTime = cdnStatus.ResponseTime,
                RccLogResponseTime = rccLogStatus.ResponseTime,
                WebsiteErrorMessage = websiteStatus.ErrorMessage,
                ApiErrorMessage = apiStatus.ErrorMessage,
                CdnErrorMessage = cdnStatus.ErrorMessage,
                RccLogErrorMessage = rccLogStatus.ErrorMessage
            };

            var healthyServices = 0;
            if (websiteStatus.IsOnline) healthyServices++;
            if (apiStatus.IsOnline) healthyServices++;
            if (cdnStatus.IsOnline) healthyServices++;
            if (rccLogStatus.IsOnline) healthyServices++;

            if (healthyServices == 4)
            {
                serviceStatus.Status = "Healthy";
                serviceStatus.OverallIsOnline = true;
            }
            else if (healthyServices == 0)
            {
                serviceStatus.Status = "Unhealthy";
                serviceStatus.OverallIsOnline = false;
            }
            else
            {
                serviceStatus.Status = "Partially";
                serviceStatus.OverallIsOnline = false;
            }

            return serviceStatus;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    public class ServiceStatus
    {
        public bool IsOnline { get; set; }
        public string Status { get; set; } = "Unknown";
        public string ServiceType { get; set; }
        public DateTime LastChecked { get; set; }
        public TimeSpan ResponseTime { get; set; }
        public int StatusCode { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class ServiceInfo
    {
        public bool IsHealthy { get; set; }
        public string Status { get; set; } = "Unknown";
        public string ServiceType { get; set; }
        public string ServiceDescription { get; set; }
        public DateTime LastChecked { get; set; }
        public int StatusCode { get; set; }
    }

    public class RccLogStatus : ServiceStatus
    {
        public RccLogStatus() { ServiceType = "RCC Log Service"; }
    }

    public class ApiServiceStatus : ServiceStatus
    {
        public ApiServiceStatus() { ServiceType = "API Service"; }
    }

    public class RccLogServiceInfo : ServiceInfo
    {
        public RccLogServiceInfo() { ServiceType = "RCC Log Service"; }
    }

    public class ApiServiceInfo : ServiceInfo
    {
        public ApiServiceInfo() { ServiceType = "API Service"; }
    }

    public class WebsiteStatus
    {
        public bool IsOnline { get; set; }
        public string Status { get; set; } = "Unknown";
        public string ErrorMessage { get; set; }
        public int StatusCode { get; set; }
        public TimeSpan ResponseTime { get; set; }
        public DateTime LastChecked { get; set; }
    }

    public class ApiStatus
    {
        public bool IsOnline { get; set; }
        public string Status { get; set; } = "Unknown";
        public string ErrorMessage { get; set; }
        public int StatusCode { get; set; }
        public TimeSpan ResponseTime { get; set; }
        public DateTime LastChecked { get; set; }
    }

    public class UserStatistics
    {
        public bool IsSuccessful { get; set; }
        public long ActiveUsers { get; set; }
        public long TotalUsers { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime LastChecked { get; set; }
    }

    public class CdnStatus
    {
        public bool IsOnline { get; set; }
        public string Status { get; set; } = "Unknown";
        public TimeSpan ResponseTime { get; set; }
        public string ErrorMessage { get; set; }
        public string ServiceUrl { get; set; }
        public DateTime LastChecked { get; set; }
        public int StatusCode { get; set; }
    }

    public class WebsiteServiceStatus
    {
        public bool OverallIsOnline { get; set; }
        public string Status { get; set; } = "Unknown";
        public DateTime LastChecked { get; set; }
        public bool WebsiteIsOnline { get; set; }
        public bool ApiIsOnline { get; set; }
        public bool CdnIsOnline { get; set; }
        public bool RccLogIsOnline { get; set; }
        public int WebsiteStatusCode { get; set; }
        public int ApiStatusCode { get; set; }
        public int CdnStatusCode { get; set; }
        public int RccLogStatusCode { get; set; }
        public TimeSpan WebsiteResponseTime { get; set; }
        public TimeSpan ApiResponseTime { get; set; }
        public TimeSpan CdnResponseTime { get; set; }
        public TimeSpan RccLogResponseTime { get; set; }
        public string WebsiteErrorMessage { get; set; }
        public string ApiErrorMessage { get; set; }
        public string CdnErrorMessage { get; set; }
        public string RccLogErrorMessage { get; set; }
    }

    public class FrontendQueries : ServiceQueries
    {
        public FrontendQueries(string baseUrl) : base(baseUrl, "Frontend Service") { }
    }

    public class CdnQueries : ServiceQueries
    {
        public CdnQueries(string baseUrl) : base(baseUrl, "CDN Service") { }
    }
}
