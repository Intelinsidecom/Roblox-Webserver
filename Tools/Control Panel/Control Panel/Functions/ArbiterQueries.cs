using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ControlPanel.Functions
{
    public class ArbiterQueries
    {
        private readonly string _baseUrl;
        private readonly HttpClient _httpClient;

        public ArbiterQueries()
        {
            var settings = Control_Panel.Properties.Settings.Default;
            var host = settings.ArbiterHost ?? "localhost";
            var port = settings.ArbiterPort ?? "5000";
            _baseUrl = $"http://{host}:{port}".TrimEnd('/');
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        public async Task<ArbiterVersionData> GetArbiterVersionAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/version");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    
                    var jsonOptions = new JsonSerializerOptions 
                    { 
                        PropertyNameCaseInsensitive = true,
                        IgnoreNullValues = true
                    };
                    
                    try
                    {
                        var versionData = JsonSerializer.Deserialize<ArbiterVersionResponse>(json, jsonOptions);
                        
                        if (versionData?.arbiter != null && versionData?.rcc != null)
                        {
                            return new ArbiterVersionData 
                            { 
                                ArbiterName = versionData.arbiter.name,
                                ArbiterVersion = versionData.arbiter.version,
                                RccUrl = versionData.rcc.url,
                                RccVersion = versionData.rcc.version,
                                EnvironmentCount = versionData.rcc.environmentCount,
                                Error = versionData.rcc.error,
                                IsSuccessful = true
                            };
                        }
                        else
                        {
                            return new ArbiterVersionData 
                            { 
                                IsSuccessful = false, 
                                Error = "Invalid response format" 
                            };
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        return new ArbiterVersionData 
                        { 
                            IsSuccessful = false, 
                            Error = $"JSON parsing failed: {jsonEx.Message}" 
                        };
                    }
                }
                else
                {
                    return new ArbiterVersionData 
                    { 
                        IsSuccessful = false, 
                        Error = $"HTTP {response.StatusCode}" 
                    };
                }
            }
            catch (HttpRequestException ex)
            {
                return new ArbiterVersionData 
                { 
                    IsSuccessful = false, 
                    Error = $"Connection Error: {ex.Message}" 
                };
            }
            catch (TaskCanceledException)
            {
                return new ArbiterVersionData 
                { 
                    IsSuccessful = false, 
                    Error = "Timeout" 
                };
            }
            catch (Exception ex)
            {
                return new ArbiterVersionData 
                { 
                    IsSuccessful = false, 
                    Error = $"Error: {ex.Message}" 
                };
            }
        }

        public async Task<ArbiterStatus> GetArbiterStatusAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/status");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var jsonOptions = new JsonSerializerOptions 
                    { 
                        PropertyNameCaseInsensitive = true,
                        IgnoreNullValues = true
                    };
                    
                    try
                    {
                        var statusData = JsonSerializer.Deserialize<ArbiterStatusResponse>(json, jsonOptions);
                        
                        if (statusData?.ok == true && statusData.rcc != null)
                        {
                            return new ArbiterStatus 
                            { 
                                IsRunning = true,
                                Status = "Healthy", // Arbiter API is responding
                                LastChecked = DateTime.Now,
                                Version = statusData.rcc.version,
                                ServiceUrl = statusData.rcc.url,
                                Error = statusData.rcc.error
                            };
                        }
                        else
                        {
                            return new ArbiterStatus 
                            { 
                                IsRunning = false, 
                                Status = "Unknown Response", 
                                LastChecked = DateTime.Now 
                            };
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        try
                        {
                            var consoleWindow = Control_Panel.ConsoleWindow.Instance;
                            consoleWindow.WriteError($"JSON parsing failed: {jsonEx.Message}");
                            consoleWindow.WriteError($"Response content: {json.Substring(0, Math.Min(json.Length, 200))}...");
                        }
                        catch
                        {
                            System.Diagnostics.Debug.WriteLine($"JSON error: {jsonEx.Message}");
                        }
                        
                        return ParseArbiterStatusWithRegex(json);
                    }
                }
                else
                {
                    return new ArbiterStatus 
                    { 
                        IsRunning = false, 
                        Status = "Unhealthy", 
                        LastChecked = DateTime.Now 
                    };
                }
            }
            catch (HttpRequestException ex)
            {
                try
                {
                    var consoleWindow = Control_Panel.ConsoleWindow.Instance;
                    consoleWindow.WriteError($"Arbiter connection failed: {ex.Message}");
                    consoleWindow.WriteError($"Base URL: {_baseUrl}");
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine($"Arbiter connection error: {ex.Message}");
                }
                
                return new ArbiterStatus 
                { 
                    IsRunning = false, 
                    Status = "Unhealthy", 
                    LastChecked = DateTime.Now 
                };
            }
            catch (TaskCanceledException)
            {
                try
                {
                    var consoleWindow = Control_Panel.ConsoleWindow.Instance;
                    consoleWindow.WriteError($"Arbiter request timed out after 10 seconds");
                    consoleWindow.WriteError($"Base URL: {_baseUrl}");
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("Arbiter request timed out");
                }
                
                return new ArbiterStatus 
                { 
                    IsRunning = false, 
                    Status = "Unhealthy", 
                    LastChecked = DateTime.Now 
                };
            }
            catch (Exception ex)
            {
                try
                {
                    var consoleWindow = Control_Panel.ConsoleWindow.Instance;
                    consoleWindow.WriteError($"Arbiter request failed: {ex.Message}");
                    consoleWindow.WriteError($"Base URL: {_baseUrl}");
                    consoleWindow.WriteError($"Stack trace: {ex.StackTrace}");
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine($"Arbiter general error: {ex.Message}");
                }
                
                return new ArbiterStatus 
                { 
                    IsRunning = false, 
                    Status = "Unhealthy", 
                    LastChecked = DateTime.Now 
                };
            }
        }

        public bool TestConnection()
        {
            try
            {
                var task = GetArbiterStatusAsync();
                var result = task.Wait(TimeSpan.FromSeconds(5));
                return result && task.Result.IsRunning;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        private ArbiterStatus ParseArbiterStatusWithRegex(string json)
        {
            try
            {
                var okMatch = Regex.Match(json, @"""ok""\s*:\s*(true|false)", RegexOptions.IgnoreCase);
                var reachableMatch = Regex.Match(json, @"""reachable""\s*:\s*(true|false)", RegexOptions.IgnoreCase);
                var urlMatch = Regex.Match(json, @"""url""\s*:\s*""([^""]+)""", RegexOptions.IgnoreCase);
                var versionMatch = Regex.Match(json, @"""version""\s*:\s*""([^""]+)""", RegexOptions.IgnoreCase);
                var errorMatch = Regex.Match(json, @"""error""\s*:\s*""([^""]*)""", RegexOptions.IgnoreCase);
                bool isOk = okMatch.Success && bool.Parse(okMatch.Groups[1].Value);
                bool isReachable = reachableMatch.Success && bool.Parse(reachableMatch.Groups[1].Value);
                string url = urlMatch.Success ? urlMatch.Groups[1].Value : "Unknown";
                string version = versionMatch.Success ? versionMatch.Groups[1].Value : "Unknown";
                string error = errorMatch.Success ? errorMatch.Groups[1].Value : null;


                return new ArbiterStatus
                {
                    IsRunning = isOk, // Arbiter API is running
                    Status = isOk ? "Healthy" : "Unhealthy", // Based on arbiter API status only
                    LastChecked = DateTime.Now,
                    Version = version,
                    ServiceUrl = url,
                    Error = error
                };
            }
            catch (Exception ex)
            {
                try
                {
                    var consoleWindow = Control_Panel.ConsoleWindow.Instance;
                    consoleWindow.WriteError($"Regex parsing failed: {ex.Message}");
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine($"Regex error: {ex.Message}");
                }

                return new ArbiterStatus
                {
                    IsRunning = false,
                    Status = "Parse Error",
                    LastChecked = DateTime.Now,
                    Error = ex.Message
                };
            }
        }
    }

    public class ArbiterVersionData
    {
        public bool IsSuccessful { get; set; }
        public string ArbiterName { get; set; }
        public string ArbiterVersion { get; set; }
        public string RccUrl { get; set; }
        public string RccVersion { get; set; }
        public int EnvironmentCount { get; set; }
        public string Error { get; set; }
    }

    public class ArbiterVersionResponse
    {
        public ArbiterVersionInfo arbiter { get; set; }
        public RccVersionInfo rcc { get; set; }
    }

    public class ArbiterVersionInfo
    {
        public string name { get; set; }
        public string version { get; set; }
    }

    public class RccVersionInfo
    {
        public string url { get; set; }
        public string version { get; set; }
        public int environmentCount { get; set; }
        public string error { get; set; }
    }

    public class ArbiterStatus
    {
        public bool IsRunning { get; set; }
        public string Status { get; set; } = "Unknown";
        public DateTime LastChecked { get; set; }
        public string Version { get; set; }
        public string ServiceUrl { get; set; }
        public string Error { get; set; }
    }

    public class ArbiterStatusResponse
    {
        public bool ok { get; set; }
        public ArbiterRccStatus rcc { get; set; }
    }

    public class ArbiterRccStatus
    {
        public string url { get; set; }
        public bool reachable { get; set; }
        public string error { get; set; }
        public string version { get; set; }
    }
}
