using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Control_Panel.Properties;
using ControlPanel.Functions;

namespace Control_Panel
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {

            this.ShutdownMode = ShutdownMode.OnLastWindowClose;

            LoadThemeResourcesSynchronously();
            ShowInitialWindow();
        }
        
        private void LoadThemeResourcesSynchronously()
        {
            try
            {
                Application.Current.Resources.MergedDictionaries.Clear();
                var themeDict = new ResourceDictionary();
                themeDict.Source = new Uri("pack://application:,,,/Styles/Themes/DefaultTheme.xaml", UriKind.Absolute);
                Application.Current.Resources.MergedDictionaries.Add(themeDict);
                var themeSettings = ControlPanel.Functions.ThemeManager.LoadThemeSettings();
                var colorDict = new ResourceDictionary();
                colorDict.Source = new Uri($"pack://application:,,,/Styles/Colors/{themeSettings.ColorScheme}.xaml", UriKind.Absolute);
                Application.Current.Resources.MergedDictionaries.Add(colorDict);
                var backgroundDict = ControlPanel.Functions.ThemeManager.CreateBackgroundResourceDictionary(themeSettings.BackgroundColor);
                Application.Current.Resources.MergedDictionaries.Add(backgroundDict);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Critical error loading theme resources: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                LoadMinimalFallbackResources();
            }
        }
        
        private void ShowInitialWindow()
        {
            Window initialWindow = DetermineInitialWindow();
            if (initialWindow != null)
            {
                ShowWindowWithShutdownHandling(initialWindow);
            }
        }
        
        private void LoadMinimalFallbackResources()
        {
            try
            {
                Application.Current.Resources.MergedDictionaries.Clear();
                
                var fallbackDict = new ResourceDictionary();
                fallbackDict.Add("Background", new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255)));
                fallbackDict.Add("Foreground", new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(27, 27, 27)));
                fallbackDict.Add("AccentPrimary", new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 175, 240)));
                fallbackDict.Add("AccentBlue", new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 175, 240)));
                fallbackDict.Add("SubtleText", new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(136, 136, 136)));
                
                Application.Current.Resources.MergedDictionaries.Add(fallbackDict);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Critical failure: Cannot load fallback resources: {ex.Message}");
                throw new InvalidOperationException("Failed to load any theme resources", ex);
            }
        }
        
        private Window DetermineInitialWindow()
        {
            if (Application.Current.Windows.Count > 1)
            {
                return null;
            }

            string connectionString = LoadConnectionString();
            bool servicesConfigured = AreServicesConfigured();
            
            if (!servicesConfigured)
            {
                return new ServiceSetupWindow();
            }
            
            if (string.IsNullOrEmpty(connectionString))
            {
                return new DatabaseConnectionWindow();
            }
            
            try
            {
                if (string.IsNullOrWhiteSpace(connectionString) || connectionString.Length < 10)
                {
                    return new DatabaseConnectionWindow();
                }
                
                var dbQueries = new ControlPanel.Functions.DatabaseQueries(connectionString);
                if (dbQueries.TestConnection())
                {
                    return new Main();
                }
                
                return new DatabaseConnectionWindow();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database connection test threw exception: {ex.Message}");
                return new DatabaseConnectionWindow();
            }
        }
        
        /// <summary>
        /// Shows a window and subscribes to its closed event for proper shutdown handling
        /// Only applies to non-main windows since Main handles its own shutdown
        /// </summary>
        public static void ShowWindowWithShutdownHandling(Window window)
        {
            if (window != null)
            {
                try
                {
                    var themeSettings = ControlPanel.Functions.ThemeManager.LoadThemeSettings();
                    ControlPanel.Functions.ThemeManager.ApplyThemeToWindow(window, themeSettings.Theme, themeSettings.ColorScheme, themeSettings.BackgroundColor);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to apply theme to {window.GetType().Name}: {ex.Message}");
                }
                
                try
                {
                    if (!(window is Main))
                    {
                        window.Show();
                        window.Closed += (sender, e) => {
                            if (Current.Windows.Count == 0)
                            {
                                Current.Shutdown();
                            }
                        };
                    }
                    else
                    {
                        window.Show();
                    }
                }
                catch (InvalidOperationException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Cannot show window {window.GetType().Name}: {ex.Message}");
                }
            }
        }
        
        private void Application_Exit(object sender, ExitEventArgs e)
        {
        }
        
        private bool AreServicesConfigured()
        {
            try
            {
                var arbiterHost = Settings.Default.ArbiterHost;
                var arbiterPort = Settings.Default.ArbiterPort;
                var websiteHost = Settings.Default.WebsiteHost;
                var websitePort = Settings.Default.WebsitePort;
                var apiHost = Settings.Default.ApiHost;
                var apiPort = Settings.Default.ApiPort;
                var cdnHost = Settings.Default.CdnHost;
                var cdnPort = Settings.Default.CdnPort;
                
                if (string.IsNullOrEmpty(arbiterHost) || string.IsNullOrEmpty(arbiterPort) ||
                    string.IsNullOrEmpty(websiteHost) || string.IsNullOrEmpty(websitePort) ||
                    string.IsNullOrEmpty(apiHost) || string.IsNullOrEmpty(apiPort) ||
                    string.IsNullOrEmpty(cdnHost) || string.IsNullOrEmpty(cdnPort))
                {
                    return false;
                }
                
                int port;
                if (!int.TryParse(arbiterPort, out port) || port < 1 || port > 65535 ||
                    !int.TryParse(websitePort, out port) || port < 1 || port > 65535 ||
                    !int.TryParse(apiPort, out port) || port < 1 || port > 65535 ||
                    !int.TryParse(cdnPort, out port) || port < 1 || port > 65535)
                {
                    return false;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking service configuration: {ex.Message}");
                return false;
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
                using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\ControlPanel"))
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
        
        public static void SaveConnectionString(string connectionString)
        {
            System.Diagnostics.Debug.WriteLine($"Attempting to save connection string: {connectionString.Substring(0, Math.Min(50, connectionString.Length))}...");
            
            try
            {
                Settings.Default.DatabaseConnectionString = connectionString;
                Settings.Default.Save();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save to app settings: {ex.Message}");
            }
            
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\ControlPanel"))
                {
                    key?.SetValue("ConnectionString", connectionString);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save to registry: {ex.Message}");
                throw new Exception("Failed to save connection string to both settings and registry", ex);
            }
        }
        
        public static void ApplyThemeToAllWindows(string theme, string color)
        {
            foreach (Window window in Current.Windows)
            {
                ThemeManager.ApplyThemeToWindow(window, theme, color, "White");
            }
        }
        
        public static void ApplyThemeToWindow(Window window, string theme, string color, string background)
        {
            ThemeManager.ApplyThemeToWindow(window, theme, color, background);
        }
    }
}
