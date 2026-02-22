using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using ControlPanel.Functions;
using Control_Panel.Properties;

namespace Control_Panel
{
    /// <summary>
    /// Interaction logic for DatabaseConnectionWindow.xaml
    /// </summary>
    public partial class DatabaseConnectionWindow : Window
    {
        private bool _isLoadingTheme = false;
        
        /// <summary>
        /// Static method to check if database is accessible and open configuration window if not.
        /// Returns true if database is accessible, false if configuration window was opened.
        /// </summary>
        /// <param name="owner">Owner window for the database configuration window</param>
        /// <param name="retryAction">Optional action to retry after database configuration</param>
        public static bool EnsureDatabaseAccessible(Window owner = null, Action retryAction = null)
        {
            if (DatabaseUtilities.IsDatabaseAccessible())
            {
                return true;
            }
            
            var databaseWindow = new DatabaseConnectionWindow();
            if (owner != null)
            {
                databaseWindow.Owner = owner;
            }
            
            if (retryAction != null)
            {
                databaseWindow.Closed += (sender, e) => retryAction();
            }
            
            databaseWindow.Show();
            return false;
        }
        
        public DatabaseConnectionWindow()
        {
            try
            {
                InitializeComponent();
                this.Icon = System.Windows.Application.Current.MainWindow?.Icon ?? 
                    new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/ControlPanel.ico"));
                ThemeManager.InitializeThemeForWindow(this);
                this.Loaded += DatabaseConnectionWindow_Loaded;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DatabaseConnectionWindow initialization failed: {ex.Message}");
                try
                {
                    InitializeComponent();
                    this.Icon = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/ControlPanel.ico"));
                    this.Loaded += DatabaseConnectionWindow_Loaded;
                }
                catch (Exception initEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Minimal DatabaseConnectionWindow initialization failed: {initEx.Message}");
                    throw;
                }
            }
        }
        
        private void DatabaseConnectionWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.PropertyChanged += Settings_PropertyChanged;
        }
        
        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_isLoadingTheme) return;
            
            if (e.PropertyName == "Theme" || e.PropertyName == "ColorScheme" || e.PropertyName == "BackgroundColor")
            {
                ThemeManager.InitializeThemeForWindow(this);
            }
        }
        
        private string BuildConnectionString()
        {
            string server = ServerTextBox.Text.Trim();
            string database = DatabaseTextBox.Text.Trim();
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;
            int portValue;
            int port = int.TryParse(PortTextBox.Text.Trim(), out portValue) ? portValue : 5432;
            
            if (string.IsNullOrWhiteSpace(server) || 
                string.IsNullOrWhiteSpace(database) || 
                string.IsNullOrWhiteSpace(username))
            {
                return null;
            }
            
            return $"Host={server};Port={port};Database={database};Username={username};Password={password};SslMode=Disable;";
        }
        
        private void TestConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = BuildConnectionString();
            
            if (string.IsNullOrEmpty(connectionString))
            {
                StatusTextBlock.Text = "Please fill in all required fields";
                StatusTextBlock.Foreground = (Brush)FindResource("Error");
                return;
            }
            
            try
            {
                var dbQueries = new ControlPanel.Functions.DatabaseQueries(connectionString);
                
                if (dbQueries.TestConnection())
                {
                    StatusTextBlock.Text = "Connection successful!";
                    StatusTextBlock.Foreground = (Brush)FindResource("Success");
                    SaveConnectionButton.IsEnabled = true;
                }
                else
                {
                    StatusTextBlock.Text = "Connection failed - check your settings";
                    StatusTextBlock.Foreground = (Brush)FindResource("Error");
                    SaveConnectionButton.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Error: {ex.Message}";
                StatusTextBlock.Foreground = (Brush)FindResource("Error");
                SaveConnectionButton.IsEnabled = false;
            }
        }
        
        private void SaveConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = BuildConnectionString();
            
            if (string.IsNullOrEmpty(connectionString))
            {
                StatusTextBlock.Text = "Please fill in all required fields";
                StatusTextBlock.Foreground = (Brush)FindResource("Error");
                return;
            }
            
            try
            {
                App.SaveConnectionString(connectionString);
                StatusTextBlock.Text = "Connection saved successfully!";
                StatusTextBlock.Foreground = (Brush)FindResource("Success");
                CloseAllWindowsAndOpenNext();
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Error saving connection: {ex.Message}";
                StatusTextBlock.Foreground = (Brush)FindResource("Error");
                System.Diagnostics.Debug.WriteLine($"Save connection failed: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Save connection stack trace: {ex.StackTrace}");
            }
        }
        
        private void CloseAllWindowsAndOpenNext()
        {
            try
            {
                var currentWindow = this;
                var otherWindows = new List<Window>();
                
                foreach (Window window in Application.Current.Windows)
                {
                    if (window != currentWindow)
                    {
                        otherWindows.Add(window);
                    }
                }
                
                bool servicesConfigured = AreServicesConfigured();
                
                Window nextWindow;
                
                if (!servicesConfigured)
                {
                    nextWindow = new ServiceSetupWindow();
                }
                else
                {
                    nextWindow = new Main();
                }
                
                try
                {
                    var themeSettings = ControlPanel.Functions.ThemeManager.LoadThemeSettings();
                    ControlPanel.Functions.ThemeManager.ApplyThemeToWindow(nextWindow, themeSettings.Theme, themeSettings.ColorScheme, themeSettings.BackgroundColor);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to apply theme to {nextWindow.GetType().Name}: {ex.Message}");
                }
                
                nextWindow.Show();
                
                if (!(nextWindow is Main))
                {
                    nextWindow.Closed += (sender, e) => {
                        if (Application.Current.Windows.Count <= 1)
                        {
                            Application.Current.Shutdown();
                        }
                    };
                }
                
                System.Threading.Tasks.Task.Delay(100).ContinueWith(_ => {
                    Application.Current.Dispatcher.Invoke(() => {
                        foreach (Window window in otherWindows)
                        {
                            try
                            {
                                window.Close();
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error closing window {window.GetType().Name}: {ex.Message}");
                            }
                        }
                        
                        try
                        {
                            currentWindow.Close();
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error closing current window: {ex.Message}");
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error transitioning to next window: {ex.Message}");
                MessageBox.Show($"Error transitioning to next window: {ex.Message}", "Error", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
