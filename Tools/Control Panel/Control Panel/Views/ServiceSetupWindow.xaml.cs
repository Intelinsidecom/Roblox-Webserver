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
                ApiHostTextBox.Text = Settings.Default.ApiHost ?? "";
                ApiPortTextBox.Text = Settings.Default.ApiPort ?? "";
                CdnHostTextBox.Text = Settings.Default.CdnHost ?? "";
                CdnPortTextBox.Text = Settings.Default.CdnPort ?? "";
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
                urls["API"] = $"http://{ApiHostTextBox.Text.Trim()}:{ApiPortTextBox.Text.Trim()}";
                urls["CDN"] = $"http://{CdnHostTextBox.Text.Trim()}:{CdnPortTextBox.Text.Trim()}";
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
            ApiStatusText.Text = "Testing...";
            ApiStatusText.Foreground = (Brush)FindResource("SubtleText");
            CdnStatusText.Text = "Testing...";
            CdnStatusText.Foreground = (Brush)FindResource("SubtleText");
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
                catch (Exception)
                {
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
                catch (Exception)
                {
                    testResults.Add("✗ Website: Connection failed");
                }
                
                WebsiteStatusText.Text = testResults[1];
                WebsiteStatusText.Foreground = testResults[1].StartsWith("✓") ? 
                    (Brush)FindResource("Success") : (Brush)FindResource("Error");
                ApiStatusText.Text = "Testing API...";
                ApiStatusText.Foreground = (Brush)FindResource("SubtleText");
                await Task.Delay(100);
                
                try
                {
                    var response = await httpClient.GetAsync($"{serviceUrls["API"]}/api/health");
                    if (response.IsSuccessStatusCode)
                    {
                        testResults.Add("✓ API: Connected");
                    }
                    else
                    {
                        testResults.Add($"✗ API: HTTP {response.StatusCode}");
                    }
                }
                catch (HttpRequestException)
                {
                    testResults.Add("✗ API: Connection failed");
                }
                catch (TaskCanceledException)
                {
                    testResults.Add("✗ API: Connection failed");
                }
                catch (Exception)
                {
                    testResults.Add("✗ API: Connection failed");
                }
                
                ApiStatusText.Text = testResults[2];
                ApiStatusText.Foreground = testResults[2].StartsWith("✓") ? 
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
                catch (Exception)
                {
                    testResults.Add("✗ CDN: Connection failed");
                }

                CdnStatusText.Text = testResults[3];
                CdnStatusText.Foreground = testResults[3].StartsWith("✓") ? 
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
                
                int apiPort;
                if (!int.TryParse(ApiPortTextBox.Text.Trim(), out apiPort) || apiPort < 1 || apiPort > 65535)
                {
                    StatusTextBlock.Text = "Invalid API port (must be 1-65535)";
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
                
                Settings.Default.ArbiterHost = ArbiterHostTextBox.Text.Trim();
                Settings.Default.ArbiterPort = ArbiterPortTextBox.Text.Trim();
                Settings.Default.WebsiteHost = WebsiteHostTextBox.Text.Trim();
                Settings.Default.WebsitePort = WebsitePortTextBox.Text.Trim();
                Settings.Default.ApiHost = ApiHostTextBox.Text.Trim();
                Settings.Default.ApiPort = ApiPortTextBox.Text.Trim();
                Settings.Default.CdnHost = CdnHostTextBox.Text.Trim();
                Settings.Default.CdnPort = CdnPortTextBox.Text.Trim();
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
