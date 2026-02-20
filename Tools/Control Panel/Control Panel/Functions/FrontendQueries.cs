using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;
using Control_Panel;

namespace ControlPanel.Functions
{
    public class FrontendQueries
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public FrontendQueries(string baseUrl)
        {
            if (string.IsNullOrEmpty(baseUrl))
                throw new ArgumentNullException(nameof(baseUrl));
                
            _baseUrl = baseUrl;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
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
                status.ErrorMessage = "Request timed out after 10 seconds";
                
                // Log error to console
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
                status.ErrorMessage = "Request timed out after 10 seconds";
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

        private long ParseActiveUsersFromResponse(string content)
        {
            return 0;
        }

        private long ParseTotalUsersFromResponse(string content)
        {
            return 0;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
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

}
