using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ControlPanel.Functions;

namespace Control_Panel
{
    /// <summary>
    /// Interaction logic for DashboardView.xaml
    /// </summary>
    public partial class DashboardView : UserControl
    {
        private DashboardService _dashboardService;
        private DashboardData _dashboardData;
        private System.Windows.Threading.DispatcherTimer _refreshTimer;
        
        public DashboardView()
        {
            InitializeComponent();
            InitializeDashboard();
            InitializeRefreshTimer();
            LoadDashboardDataAsync();
        }
        
        private void InitializeDashboard()
        {
            try
            {
                var connectionString = Properties.Settings.Default.DatabaseConnectionString;
                
                if (string.IsNullOrEmpty(connectionString))
                {
                    ShowDatabaseNotConfiguredError();
                    return;
                }
                
                var dbQueries = new ControlPanel.Functions.DatabaseQueries(connectionString);
                if (!dbQueries.TestConnection())
                {
                    ShowDatabaseConnectionError();
                    return;
                }
                
                var settings = Properties.Settings.Default;
                var arbiterUrl = $"http://{settings.ArbiterHost}:{settings.ArbiterPort}";
                var frontendUrl = $"http://{settings.WebsiteHost}:{settings.WebsitePort}";
                var cdnUrl = $"http://{settings.CdnHost}:{settings.CdnPort}";
                
                _dashboardService = new DashboardService(connectionString, arbiterUrl, frontendUrl, cdnUrl);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize dashboard service: {ex.Message}", "Initialization Error", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
                
                ShowInitializationError(ex.Message);
            }
        }
        
        private async void LoadDashboardDataAsync()
        {
            await LoadDashboardData();
        }
        
        private async Task<DashboardData> LoadDashboardData()
        {
            try
            {
                if (_dashboardService != null)
                {
                    _dashboardData = await _dashboardService.GetDashboardDataAsync();
                    
                    if (_dashboardData != null)
                    {
                        if (!AreServicesConfigured())
                        {
                            ShowInitializationError("Services are not configured");
                            return _dashboardData;
                        }
                        
                        if (_dashboardData.ServerHealth == "Error" || !_dashboardData.IsHealthy)
                        {
                            var result = MessageBox.Show(
                                "Database connection is showing errors. Would you like to configure the database connection?",
                                "Database Connection Error",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning);
                                
                            if (result == MessageBoxResult.Yes)
                            {
                                ShowDatabaseConnectionError();
                                return _dashboardData;
                            }
                        }
                        
                        UpdateDashboardUI();
                    }
                }
                else
                {
                    ShowDatabaseNotConfiguredError();
                }
            }
            catch (Exception ex)
            {
                var result = MessageBox.Show(
                    $"Failed to load dashboard data: {ex.Message}\n\nWould you like to configure the database connection?",
                    "Dashboard Error",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                    
                if (result == MessageBoxResult.Yes)
                {
                    ShowDatabaseConnectionError();
                }
                else
                {
                    UpdateDashboardUIWithError(ex.Message);
                }
            }
            
            return _dashboardData;
        }
        
        private void UpdateDashboardUI()
        {
            if (_dashboardData == null) return;
            
            ServerHealthText.Text = _dashboardData.ServerHealth;
            ServerHealthText.Foreground = _dashboardData.IsHealthy ? 
                new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
            DatabaseSizeText.Text = _dashboardData.ArbiterStatus;
            DatabaseSizeText.Foreground = _dashboardData.ArbiterIsRunning ? 
                new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
            if (_dashboardData.ArbiterIsRunning)
            {
                RCCVersionText.Text = _dashboardData.RccVersion ?? "Unknown";
                RCCVersionText.Foreground = new SolidColorBrush(Colors.Gray);
                RCCStatusText.Text = _dashboardData.RccStatus;
                RCCStatusText.Foreground = new SolidColorBrush(Colors.Green);
            }
            else
            {
                RCCVersionText.Text = "RCC not active";
                RCCVersionText.Foreground = new SolidColorBrush(Colors.Gray);
                RCCStatusText.Text = _dashboardData.RccStatus;
                RCCStatusText.Foreground = new SolidColorBrush(Colors.Red);
            }
            UpdateFrontendUI();
        }
        
        private void UpdateFrontendUI()
        {
            WebsiteStatusText.Text = _dashboardData.WebsiteStatus;
            WebsiteStatusText.Foreground = _dashboardData.WebsiteIsOnline ? 
                new SolidColorBrush(Colors.Green) : 
                (_dashboardData.WebsiteStatus == "Unhealthy" ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Gray));
            if (_dashboardData.FrontendActiveUsers > 0 || !string.IsNullOrEmpty(_dashboardData.FrontendUserError))
            {
                ActiveUsersText.Text = string.IsNullOrEmpty(_dashboardData.FrontendUserError) 
                    ? _dashboardData.FrontendActiveUsers.ToString()
                    : $"Error: {_dashboardData.FrontendUserError}";
                ActiveUsersText.Foreground = string.IsNullOrEmpty(_dashboardData.FrontendUserError)
                    ? new SolidColorBrush(Colors.Gray)
                    : new SolidColorBrush(Colors.Red);
            }
            else
            {
                ActiveUsersText.Text = _dashboardData.ActiveUsers.ToString();
                ActiveUsersText.Foreground = new SolidColorBrush(Colors.Gray);
            }
            if (_dashboardData.CdnIsOnline || !string.IsNullOrEmpty(_dashboardData.CdnErrorMessage))
            {
                CdnStatusText.Text = string.IsNullOrEmpty(_dashboardData.CdnErrorMessage)
                    ? _dashboardData.CdnStatus
                    : _dashboardData.CdnErrorMessage;
                CdnStatusText.Foreground = string.IsNullOrEmpty(_dashboardData.CdnErrorMessage)
                    ? (_dashboardData.CdnStatus == "Unhealthy" ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Green))
                    : new SolidColorBrush(Colors.Red);
            }
            else
            {
                CdnStatusText.Text = _dashboardData.CdnStatus;
                CdnStatusText.Foreground = _dashboardData.CdnStatus == "Unhealthy" 
                    ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Gray);
            }
            
            if (_dashboardData.ApiIsOnline)
            {
                var responseTimeText = _dashboardData.ApiResponseTime.TotalMilliseconds > 0 
                    ? $"({_dashboardData.ApiResponseTime.TotalMilliseconds:F0}ms)"
                    : "";
                APIResponseText.Text = $"{_dashboardData.ApiStatus} {responseTimeText}".Trim();
                APIResponseText.Foreground = new SolidColorBrush(Colors.Green);
            }
            else
            {
                APIResponseText.Text = string.IsNullOrEmpty(_dashboardData.ApiErrorMessage)
                    ? _dashboardData.ApiStatus
                    : $"Error: {_dashboardData.ApiErrorMessage}";
                APIResponseText.Foreground = _dashboardData.ApiStatus == "Unhealthy" 
                    ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Gray);
            }
        }
        
        private void InitializeRefreshTimer()
        {
            _refreshTimer = new System.Windows.Threading.DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromMinutes(1); // Refresh every 1 minute
            _refreshTimer.Tick += RefreshTimer_Tick;
            _refreshTimer.Start();
        }
        
        private async void RefreshTimer_Tick(object sender, EventArgs e)
        {
            ClearStatusFields();
            await LoadDashboardData();
        }
        
        private void ClearStatusFields()
        {
            ServerHealthText.Text = "Checking...";
            DatabaseSizeText.Text = "Checking...";
            RCCStatusText.Text = "Checking...";
            RCCVersionText.Text = "Checking...";
            WebsiteStatusText.Text = "Checking...";
            ActiveUsersText.Text = "Checking...";
            CdnStatusText.Text = "Checking...";
            APIResponseText.Text = "Checking...";
            var neutralColor = new SolidColorBrush(Colors.Gray);
            ServerHealthText.Foreground = neutralColor;
            DatabaseSizeText.Foreground = neutralColor;
            RCCStatusText.Foreground = neutralColor;
            RCCVersionText.Foreground = neutralColor;
            WebsiteStatusText.Foreground = neutralColor;
            ActiveUsersText.Foreground = neutralColor;
            CdnStatusText.Foreground = neutralColor;
            APIResponseText.Foreground = neutralColor;
        }
        
        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            ClearStatusFields();
            await LoadDashboardData();
        }
        
        private void UpdateDashboardUIWithError(string errorMessage)
        {
            ServerHealthText.Text = "Error";
            ServerHealthText.Foreground = new SolidColorBrush(Colors.Red);
        }
        
        private void ShowDatabaseNotConfiguredError()
        {
            try
            {
                ServerHealthText.Text = "Database not configured";
                ServerHealthText.Foreground = new SolidColorBrush(Colors.Orange);
                var databaseWindow = new DatabaseConnectionWindow();
                databaseWindow.Owner = Window.GetWindow(this);
                databaseWindow.Closed += async (sender, e) => {
                    await System.Threading.Tasks.Task.Run(() => {
                        Dispatcher.Invoke(() => {
                            ClearStatusFields();
                            LoadDashboardData().ConfigureAwait(false);
                        });
                    });
                };
                
                databaseWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to open database configuration: {ex.Message}");
                MessageBox.Show(
                    "Failed to open database configuration. Please restart the application.",
                    "Configuration Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        
        private void ShowDatabaseConnectionError()
        {
            try
            {
                ServerHealthText.Text = "Database connection failed";
                ServerHealthText.Foreground = new SolidColorBrush(Colors.Red);
                
                MessageBox.Show(
                    "Database connection failed. Please restart the application to configure the database connection.",
                    "Database Connection Error", 
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to show database error: {ex.Message}");
            }
        }
        
        private void ShowInitializationError(string message)
        {
            try
            {
                ServerHealthText.Text = "Initialization failed";
                ServerHealthText.Foreground = new SolidColorBrush(Colors.Red);
                
                MessageBox.Show(
                    $"Dashboard initialization failed: {message}\n\nPlease restart the application to resolve configuration issues.",
                    "Initialization Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to show initialization error: {ex.Message}");
            }
        }
        
        private bool AreServicesConfigured()
        {
            try
            {
                var settings = Properties.Settings.Default;
                
                if (string.IsNullOrEmpty(settings.ArbiterHost) || string.IsNullOrEmpty(settings.ArbiterPort) ||
                    string.IsNullOrEmpty(settings.WebsiteHost) || string.IsNullOrEmpty(settings.WebsitePort) ||
                    string.IsNullOrEmpty(settings.ApiHost) || string.IsNullOrEmpty(settings.ApiPort) ||
                    string.IsNullOrEmpty(settings.CdnHost) || string.IsNullOrEmpty(settings.CdnPort))
                {
                    return false;
                }
                
                int port;
                if (!int.TryParse(settings.ArbiterPort, out port) || port < 1 || port > 65535 ||
                    !int.TryParse(settings.WebsitePort, out port) || port < 1 || port > 65535 ||
                    !int.TryParse(settings.ApiPort, out port) || port < 1 || port > 65535 ||
                    !int.TryParse(settings.CdnPort, out port) || port < 1 || port > 65535)
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
        
    }
}
