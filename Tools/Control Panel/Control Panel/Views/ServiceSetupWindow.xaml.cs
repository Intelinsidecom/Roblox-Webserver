using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ControlPanel.Functions;
using Control_Panel.Properties;

namespace Control_Panel
{
    /// <summary>
    /// Interaction logic for ServiceSetupWindow.xaml
    /// </summary>
    public partial class ServiceSetupWindow : Window
    {
        private bool _isLoadingTheme = false;
        
        public ServiceSetupWindow()
        {
            InitializeComponent();
            this.Icon = System.Windows.Application.Current.MainWindow?.Icon ?? 
                new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/ControlPanel.ico"));
            ThemeManager.InitializeThemeForWindow(this);
            
            this.Loaded += ServiceSetupWindow_Loaded;
        }
        
        private void ServiceSetupWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.PropertyChanged += Settings_PropertyChanged;
            LoadExistingSettings();
        }
        
        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_isLoadingTheme) return;
            
            if (e.PropertyName == "Theme" || e.PropertyName == "ColorScheme" || e.PropertyName == "BackgroundColor")
            {
                ThemeManager.InitializeThemeForWindow(this);
            }
        }
        
        private void LoadExistingSettings()
        {
            try
            {
                ArbiterHostTextBox.Text = Settings.Default.ArbiterHost ?? "";
                ArbiterPortTextBox.Text = Settings.Default.ArbiterPort ?? "";
                WebsiteHostTextBox.Text = Settings.Default.WebsiteHost ?? "";
                WebsitePortTextBox.Text = Settings.Default.WebsitePort ?? "";
                ApiServiceHostTextBox.Text = Settings.Default.ApiServiceHost ?? "";
                ApiServicePortTextBox.Text = Settings.Default.ApiServicePort ?? "";
                RccLogHostTextBox.Text = Settings.Default.RccLogHost ?? "";
                RccLogPortTextBox.Text = Settings.Default.RccLogPort ?? "";
                CdnHostTextBox.Text = Settings.Default.CdnHost ?? "";
                CdnPortTextBox.Text = Settings.Default.CdnPort ?? "";
                
                try
                {
                    SetupHostTextBox.Text = Settings.Default.SetupHost ?? "";
                    SetupPortTextBox.Text = Settings.Default.SetupPort ?? "";
                }
                catch
                {
                    SetupHostTextBox.Text = "localhost";
                    SetupPortTextBox.Text = "5192";
                }
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Error loading settings: {ex.Message}";
                StatusTextBlock.Foreground = (Brush)FindResource("Error");
            }
        }
        
        private Dictionary<string, string> GetServiceUrls()
        {
            var urls = new Dictionary<string, string>();
            
            try
            {
                urls["Arbiter"] = $"http://{ArbiterHostTextBox.Text.Trim()}:{ArbiterPortTextBox.Text.Trim()}";
                urls["Website"] = $"http://{WebsiteHostTextBox.Text.Trim()}:{WebsitePortTextBox.Text.Trim()}";
                urls["APIService"] = $"http://{ApiServiceHostTextBox.Text.Trim()}:{ApiServicePortTextBox.Text.Trim()}";
                urls["RccLog"] = $"http://{RccLogHostTextBox.Text.Trim()}:{RccLogPortTextBox.Text.Trim()}";
                urls["CDN"] = $"http://{CdnHostTextBox.Text.Trim()}:{CdnPortTextBox.Text.Trim()}";
                urls["Setup"] = $"http://{SetupHostTextBox.Text.Trim()}:{SetupPortTextBox.Text.Trim()}";
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Error building URLs: {ex.Message}";
                StatusTextBlock.Foreground = (Brush)FindResource("Error");
                return null;
            }
            
            return urls;
        }
        
        private async void TestServicesButton_Click(object sender, RoutedEventArgs e)
        {
            var serviceUrls = GetServiceUrls();
            
            if (serviceUrls == null)
            {
                return;
            }
            
            StatusTextBlock.Text = "Testing services...";
            StatusTextBlock.Foreground = (Brush)FindResource("AccentBlue");
            ArbiterStatusText.Text = "Testing...";
            ArbiterStatusText.Foreground = (Brush)FindResource("SubtleText");
            WebsiteStatusText.Text = "Testing...";
            WebsiteStatusText.Foreground = (Brush)FindResource("SubtleText");
            ApiServiceStatusText.Text = "Testing...";
            ApiServiceStatusText.Foreground = (Brush)FindResource("SubtleText");
            RccLogStatusText.Text = "Testing...";
            RccLogStatusText.Foreground = (Brush)FindResource("SubtleText");
            CdnStatusText.Text = "Testing CDN...";
            CdnStatusText.Foreground = (Brush)FindResource("SubtleText");
            SetupStatusText.Text = "Testing Setup...";
            SetupStatusText.Foreground = (Brush)FindResource("SubtleText");
            var testResults = new List<string>();
            var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(5);
            
            try
            {
                ArbiterStatusText.Text = "Testing Arbiter...";
                ArbiterStatusText.Foreground = (Brush)FindResource("SubtleText");
                await Task.Delay(100);
                
                try
                {
                    var response = await httpClient.GetAsync($"{serviceUrls["Arbiter"]}/status");
                    if (response.IsSuccessStatusCode)
                    {
                        testResults.Add("✓ Arbiter: Connected");
                    }
                    else
                    {
                        testResults.Add($"✗ Arbiter: HTTP {response.StatusCode}");
                    }
                }
                catch (HttpRequestException)
                {
                    testResults.Add("✗ Arbiter: Connection failed");
                }
                catch (TaskCanceledException)
                {
                    testResults.Add("✗ Arbiter: Connection failed");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] TestConnectionsButton_Click: {ex.Message}");
                    testResults.Add("✗ Arbiter: Connection failed");
                }
                
                ArbiterStatusText.Text = testResults[0];
                ArbiterStatusText.Foreground = testResults[0].StartsWith("✓") ? 
                    (Brush)FindResource("Success") : (Brush)FindResource("Error");
                WebsiteStatusText.Text = "Testing Website...";
                WebsiteStatusText.Foreground = (Brush)FindResource("SubtleText");
                await Task.Delay(100);
                
                try
                {
                    var response = await httpClient.GetAsync(serviceUrls["Website"]);
                    if (response.IsSuccessStatusCode)
                    {
                        testResults.Add("✓ Website: Connected");
                    }
                    else
                    {
                        testResults.Add($"✗ Website: HTTP {response.StatusCode}");
                    }
                }
                catch (HttpRequestException)
                {
                    testResults.Add("✗ Website: Connection failed");
                }
                catch (TaskCanceledException)
                {
                    testResults.Add("✗ Website: Connection failed");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] TestConnectionsButton_Click Website: {ex.Message}");
                    testResults.Add("✗ Website: Connection failed");
                }
                
                WebsiteStatusText.Text = testResults[1];
                WebsiteStatusText.Foreground = testResults[1].StartsWith("✓") ? 
                    (Brush)FindResource("Success") : (Brush)FindResource("Error");
                ApiServiceStatusText.Text = "Testing API Service...";
                ApiServiceStatusText.Foreground = (Brush)FindResource("SubtleText");
                await Task.Delay(100);
                
                try
                {
                    var response = await httpClient.GetAsync(serviceUrls["APIService"]);
                    if (response.IsSuccessStatusCode)
                    {
                        testResults.Add("✓ API Service: Connected");
                    }
                    else
                    {
                        testResults.Add($"✗ API Service: HTTP {response.StatusCode}");
                    }
                }
                catch (HttpRequestException)
                {
                    testResults.Add("✗ API Service: Connection failed");
                }
                catch (TaskCanceledException)
                {
                    testResults.Add("✗ API Service: Connection failed");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] TestConnectionsButton_Click API Service: {ex.Message}");
                    testResults.Add("✗ API Service: Connection failed");
                }
                
                ApiServiceStatusText.Text = testResults[2];
                ApiServiceStatusText.Foreground = testResults[2].StartsWith("✓") ? 
                    (Brush)FindResource("Success") : (Brush)FindResource("Error");
                RccLogStatusText.Text = "Testing Data Service...";
                RccLogStatusText.Foreground = (Brush)FindResource("SubtleText");
                await Task.Delay(100);
                
                try
                {
                    var response = await httpClient.GetAsync(serviceUrls["RccLog"]);
                    if (response.IsSuccessStatusCode)
                    {
                        testResults.Add("✓ Data Service: Connected");
                    }
                    else
                    {
                        testResults.Add($"✗ Data Service: HTTP {response.StatusCode}");
                    }
                }
                catch (HttpRequestException)
                {
                    testResults.Add("✗ Data Service: Connection failed");
                }
                catch (TaskCanceledException)
                {
                    testResults.Add("✗ Data Service: Connection failed");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] TestConnectionsButton_Click Data Service: {ex.Message}");
                    testResults.Add("✗ Data Service: Connection failed");
                }
                
                RccLogStatusText.Text = testResults[3];
                RccLogStatusText.Foreground = testResults[3].StartsWith("✓") ? 
                    (Brush)FindResource("Success") : (Brush)FindResource("Error");
                CdnStatusText.Text = "Testing CDN...";
                CdnStatusText.Foreground = (Brush)FindResource("SubtleText");
                await Task.Delay(100);
                
                try
                {
                    var response = await httpClient.GetAsync($"{serviceUrls["CDN"]}/health");
                    if (response.IsSuccessStatusCode)
                    {
                        testResults.Add("✓ CDN: Connected");
                    }
                    else
                    {
                        testResults.Add($"✗ CDN: HTTP {response.StatusCode}");
                    }
                }
                catch (HttpRequestException)
                {
                    testResults.Add("✗ CDN: Connection failed");
                }
                catch (TaskCanceledException)
                {
                    testResults.Add("✗ CDN: Connection failed");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] TestConnectionsButton_Click CDN: {ex.Message}");
                    testResults.Add("✗ CDN: Connection failed");
                }

                CdnStatusText.Text = testResults[4];
                CdnStatusText.Foreground = testResults[4].StartsWith("✓") ? 
                    (Brush)FindResource("Success") : (Brush)FindResource("Error");
                SetupStatusText.Text = "Testing Setup Service...";
                SetupStatusText.Foreground = (Brush)FindResource("SubtleText");
                await Task.Delay(100);
                
                try
                {
                    var response = await httpClient.GetAsync($"{serviceUrls["Setup"]}/cdn.txt");
                    if (response.IsSuccessStatusCode)
                    {
                        testResults.Add("✓ Setup Service: Connected");
                    }
                    else
                    {
                        testResults.Add($"✗ Setup Service: HTTP {response.StatusCode}");
                    }
                }
                catch (HttpRequestException)
                {
                    testResults.Add("✗ Setup Service: Connection failed");
                }
                catch (TaskCanceledException)
                {
                    testResults.Add("✗ Setup Service: Connection failed");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] TestConnectionsButton_Click Setup Service: {ex.Message}");
                    testResults.Add("✗ Setup Service: Connection failed");
                }

                SetupStatusText.Text = testResults[5];
                SetupStatusText.Foreground = testResults[5].StartsWith("✓") ? 
                    (Brush)FindResource("Success") : (Brush)FindResource("Error");
                var hasSuccessfulConnections = testResults.Any(r => r.StartsWith("✓"));
                var hasFailedConnections = testResults.Any(r => r.StartsWith("✗"));
                
                if (hasSuccessfulConnections && !hasFailedConnections)
                {
                    StatusTextBlock.Text = "All services connected successfully!";
                    StatusTextBlock.Foreground = (Brush)FindResource("Success");
                }
                else if (hasSuccessfulConnections && hasFailedConnections)
                {
                    StatusTextBlock.Text = "Some services connected, others failed";
                    StatusTextBlock.Foreground = (Brush)FindResource("Warning");
                }
                else
                {
                    StatusTextBlock.Text = "All services failed to connect";
                    StatusTextBlock.Foreground = (Brush)FindResource("Error");
                }
                
                SaveSettingsButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Error testing services: {ex.Message}";
                StatusTextBlock.Foreground = (Brush)FindResource("Error");
                SaveSettingsButton.IsEnabled = false;
            }
            finally
            {
                httpClient.Dispose();
            }
        }
        
        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int arbiterPort;
                if (!int.TryParse(ArbiterPortTextBox.Text.Trim(), out arbiterPort) || arbiterPort < 1 || arbiterPort > 65535)
                {
                    StatusTextBlock.Text = "Invalid Arbiter port (must be 1-65535)";
                    StatusTextBlock.Foreground = (Brush)FindResource("Error");
                    return;
                }
                
                int websitePort;
                if (!int.TryParse(WebsitePortTextBox.Text.Trim(), out websitePort) || websitePort < 1 || websitePort > 65535)
                {
                    StatusTextBlock.Text = "Invalid Website port (must be 1-65535)";
                    StatusTextBlock.Foreground = (Brush)FindResource("Error");
                    return;
                }
                
                int apiServicePort;
                if (!int.TryParse(ApiServicePortTextBox.Text.Trim(), out apiServicePort) || apiServicePort < 1 || apiServicePort > 65535)
                {
                    StatusTextBlock.Text = "Invalid API Service port (must be 1-65535)";
                    StatusTextBlock.Foreground = (Brush)FindResource("Error");
                    return;
                }
                
                int rccLogPort;
                if (!int.TryParse(RccLogPortTextBox.Text.Trim(), out rccLogPort) || rccLogPort < 1 || rccLogPort > 65535)
                {
                    StatusTextBlock.Text = "Invalid Data Service port (must be 1-65535)";
                    StatusTextBlock.Foreground = (Brush)FindResource("Error");
                    return;
                }
                
                int cdnPort;
                if (!int.TryParse(CdnPortTextBox.Text.Trim(), out cdnPort) || cdnPort < 1 || cdnPort > 65535)
                {
                    StatusTextBlock.Text = "Invalid CDN port (must be 1-65535)";
                    StatusTextBlock.Foreground = (Brush)FindResource("Error");
                    return;
                }
                
                int setupPort;
                if (!int.TryParse(SetupPortTextBox.Text.Trim(), out setupPort) || setupPort < 1 || setupPort > 65535)
                {
                    StatusTextBlock.Text = "Invalid Setup Service port (must be 1-65535)";
                    StatusTextBlock.Foreground = (Brush)FindResource("Error");
                    return;
                }
                
                Settings.Default.ArbiterHost = ArbiterHostTextBox.Text.Trim();
                Settings.Default.ArbiterPort = ArbiterPortTextBox.Text.Trim();
                Settings.Default.WebsiteHost = WebsiteHostTextBox.Text.Trim();
                Settings.Default.WebsitePort = WebsitePortTextBox.Text.Trim();
                Settings.Default.ApiServiceHost = ApiServiceHostTextBox.Text.Trim();
                Settings.Default.ApiServicePort = ApiServicePortTextBox.Text.Trim();
                Settings.Default.RccLogHost = RccLogHostTextBox.Text.Trim();
                Settings.Default.RccLogPort = RccLogPortTextBox.Text.Trim();
                Settings.Default.CdnHost = CdnHostTextBox.Text.Trim();
                Settings.Default.CdnPort = CdnPortTextBox.Text.Trim();
                
                try
                {
                    Settings.Default.SetupHost = SetupHostTextBox.Text.Trim();
                    Settings.Default.SetupPort = SetupPortTextBox.Text.Trim();
                }
                catch
                {
                    // Settings not available, ignore
                    ConsoleWindow.Instance?.WriteWarning("SetupHost/SetupPort settings not available, skipping save");
                }
                
                Settings.Default.Save();
                StatusTextBlock.Text = "Service settings saved successfully!";
                StatusTextBlock.Foreground = (Brush)FindResource("Success");
                
                // Just close this window
                this.Close();
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Error saving settings: {ex.Message}";
                StatusTextBlock.Foreground = (Brush)FindResource("Error");
            }
        }
        
        private string LoadConnectionString()
        {
            try
            {
                if (!string.IsNullOrEmpty(Settings.Default.DatabaseConnectionString))
                {
                    return Settings.Default.DatabaseConnectionString;
                }
            }
            catch
            {
            }
            
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\ControlPanel"))
                {
                    if (key != null)
                    {
                        var connectionString = key.GetValue("ConnectionString") as string;
                        if (!string.IsNullOrEmpty(connectionString))
                        {
                            return connectionString;
                        }
                    }
                }
            }
            catch
            {
            }
            
            return null;
        }
        
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (Window window in Application.Current.Windows)
            {
                window.Close();
            }
            Application.Current.Shutdown();
        }
    }
}
