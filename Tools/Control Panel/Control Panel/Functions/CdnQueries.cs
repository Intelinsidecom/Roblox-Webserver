using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace ControlPanel.Functions
{
    public class CdnStatus
    {
        public bool IsOnline { get; set; }
        public string Status { get; set; } = "Unknown";
        public TimeSpan ResponseTime { get; set; }
        public string ErrorMessage { get; set; }
        public string ServiceUrl { get; set; }
        public DateTime LastChecked { get; set; }
    }

    public class CdnQueries
    {
        private readonly string _cdnUrl;
        private readonly HttpClient _httpClient;

        public CdnQueries(string cdnUrl)
        {
            if (cdnUrl == null)
                throw new ArgumentNullException(nameof(cdnUrl));
            
            _cdnUrl = cdnUrl;
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
        }

        public async Task<CdnStatus> GetCdnStatusAsync()
        {
            var status = new CdnStatus
            {
                ServiceUrl = _cdnUrl,
                LastChecked = DateTime.Now
            };

            try
            {
                var stopwatch = Stopwatch.StartNew();
                var response = await _httpClient.GetAsync(_cdnUrl);
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

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
