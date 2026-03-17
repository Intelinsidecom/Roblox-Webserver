using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ControlPanel.Functions;

namespace Control_Panel
{
    /// <summary>
    /// Interaction logic for Website.xaml
    /// </summary>
    public partial class WebsiteView : UserControl
    {
        private readonly WebsiteService _websiteService;
        private WebsiteSettings _currentSettings;
        private ServiceQueries _frontendQueries;
        private ServiceQueries _cdnQueries;
        private ServiceQueries _apiServiceQueries;
        private ServiceQueries _rccLogQueries;
        private ServiceStatus _websiteStatus;
        private ServiceStatus _cdnStatus;
        private ServiceStatus _apiStatus;
        private ServiceStatus _rccLogStatus;
        private System.Windows.Threading.DispatcherTimer _refreshTimer;

        public WebsiteView()
        {
            InitializeComponent();
            var connectionString = Control_Panel.Properties.Settings.Default.DatabaseConnectionString;
            _websiteService = new WebsiteService(connectionString);
            InitializeServiceQueries();
            MaintenanceReasonTextBox.Height = 35;
            LoadWebsiteSettingsAsync();
            InitializeRefreshTimer();
            LoadServiceDataAsync();
        }

        private void InitializeServiceQueries()
        {
            try
            {
                var websiteHost = Properties.Settings.Default.WebsiteHost;
                var websitePort = Properties.Settings.Default.WebsitePort;
                var cdnHost = Properties.Settings.Default.CdnHost;
                var cdnPort = Properties.Settings.Default.CdnPort;
                var apiServiceHost = Properties.Settings.Default.ApiServiceHost;
                var apiServicePort = Properties.Settings.Default.ApiServicePort;
                var rccLogHost = Properties.Settings.Default.RccLogHost;
                var rccLogPort = Properties.Settings.Default.RccLogPort;
                if (!string.IsNullOrEmpty(websiteHost) && !string.IsNullOrEmpty(websitePort))
                {
                    var frontendUrl = $"http://{websiteHost}:{websitePort}";
                    _frontendQueries = new ServiceQueries(frontendUrl, "Frontend Service");
                }
                
                if (!string.IsNullOrEmpty(cdnHost) && !string.IsNullOrEmpty(cdnPort))
                {
                    var cdnUrl = $"http://{cdnHost}:{cdnPort}";
                    _cdnQueries = new ServiceQueries(cdnUrl, "CDN Service");
                }
                
                if (!string.IsNullOrEmpty(apiServiceHost) && !string.IsNullOrEmpty(apiServicePort))
                {
                    var apiUrl = $"http://{apiServiceHost}:{apiServicePort}";
                    _apiServiceQueries = new ServiceQueries(apiUrl, "API Service");
                }
                
                if (!string.IsNullOrEmpty(rccLogHost) && !string.IsNullOrEmpty(rccLogPort))
                {
                    var rccLogUrl = $"http://{rccLogHost}:{rccLogPort}";
                    _rccLogQueries = new ServiceQueries(rccLogUrl, "RCC Log Service");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize service queries: {ex.Message}", "Initialization Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                ShowServiceInitializationError(ex.Message);
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
            ClearServiceStatusFields();
            await LoadServiceData();
        }

        private async void LoadServiceDataAsync()
        {
            await LoadServiceData();
        }

        private async Task LoadServiceData()
        {
            try
            {
                var tasks = new List<Task>();

                if (_frontendQueries != null)
                {
                    tasks.Add(GetWebsiteStatusAsync());
                }
                
                if (_cdnQueries != null)
                {
                    tasks.Add(GetCdnStatusAsync());
                }
                
                if (_apiServiceQueries != null)
                {
                    tasks.Add(GetApiStatusAsync());
                }
                
                if (_rccLogQueries != null)
                {
                    tasks.Add(GetRccLogStatusAsync());
                }
                
                if (tasks.Count == 0)
                {
                    ShowServiceNotConfiguredError();
                    return;
                }
                
                await Task.WhenAll(tasks);
                UpdateServiceStatusUI();
            }
            catch (Exception ex)
            {
                UpdateServiceStatusUIWithError(ex.Message);
            }
        }

        private async Task GetWebsiteStatusAsync()
        {
            try
            {
                if (_frontendQueries != null)
                {
                    var websiteStatus = await _frontendQueries.GetWebsiteStatusAsync();
                    _websiteStatus = new ServiceStatus
                    {
                        IsOnline = websiteStatus.IsOnline,
                        Status = websiteStatus.IsOnline ? "Healthy" : "Unhealthy",
                        ResponseTime = websiteStatus.ResponseTime,
                        LastChecked = DateTime.Now
                    };
                }
            }
            catch (Exception ex)
            {
                _websiteStatus = new ServiceStatus
                {
                    IsOnline = false,
                    Status = "Error",
                    LastChecked = DateTime.Now
                };
            }
        }
        
        private async Task GetCdnStatusAsync()
        {
            try
            {
                if (_cdnQueries != null)
                {
                    _cdnStatus = await _cdnQueries.GetServiceStatusAsync();
                }
            }
            catch (Exception ex)
            {
                _cdnStatus = new ServiceStatus
                {
                    IsOnline = false,
                    Status = "Error",
                    LastChecked = DateTime.Now
                };
            }
        }
        
        private async Task GetApiStatusAsync()
        {
            try
            {
                if (_apiServiceQueries != null)
                {
                    _apiStatus = await _apiServiceQueries.GetServiceStatusAsync();
                }
            }
            catch (Exception ex)
            {
                _apiStatus = new ServiceStatus
                {
                    IsOnline = false,
                    Status = "Error",
                    LastChecked = DateTime.Now
                };
            }
        }
        
        private async Task GetRccLogStatusAsync()
        {
            try
            {
                if (_rccLogQueries != null)
                {
                    _rccLogStatus = await _rccLogQueries.GetServiceStatusAsync();
                }
            }
            catch (Exception ex)
            {
                _rccLogStatus = new ServiceStatus
                {
                    IsOnline = false,
                    Status = "Error",
                    LastChecked = DateTime.Now
                };
            }
        }

        private void UpdateServiceStatusUI()
        {
            UpdateWebsiteStatusUI();
            UpdateCdnStatusUI();
            UpdateApiStatusUI();
            UpdateRccLogStatusUI();
        }
        
        private void UpdateWebsiteStatusUI()
        {
            if (_websiteStatus != null)
            {
                WebsiteStatusText.Text = _websiteStatus.IsOnline ? "Healthy" : "Unhealthy";
                WebsiteStatusText.Foreground = _websiteStatus.IsOnline ? 
                    new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
            }
        }
        
        private void UpdateCdnStatusUI()
        {
            if (_cdnStatus != null)
            {
                CdnStatusText.Text = _cdnStatus.IsOnline ? "Healthy" : "Unhealthy";
                CdnStatusText.Foreground = _cdnStatus.IsOnline ? 
                    new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
            }
        }
        
        private void UpdateApiStatusUI()
        {
            if (_apiStatus != null)
            {
                ApiServiceStatusText.Text = _apiStatus.IsOnline ? "Healthy" : "Unhealthy";
                ApiServiceStatusText.Foreground = _apiStatus.IsOnline ? 
                    new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
            }
        }
        
        private void UpdateRccLogStatusUI()
        {
            if (_rccLogStatus != null)
            {
                RccLogStatusText.Text = _rccLogStatus.Status;
                RccLogStatusText.Foreground = _rccLogStatus.IsOnline ? 
                    new SolidColorBrush(Colors.Green) : 
                    (_rccLogStatus.Status == "Unhealthy" ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Gray));
            }
        }

        private void ClearServiceStatusFields()
        {
            WebsiteStatusText.Text = "Checking...";
            CdnStatusText.Text = "Checking...";
            ApiServiceStatusText.Text = "Checking...";
            RccLogStatusText.Text = "Checking...";
            
            var neutralColor = new SolidColorBrush(Colors.Gray);
            WebsiteStatusText.Foreground = neutralColor;
            CdnStatusText.Foreground = neutralColor;
            ApiServiceStatusText.Foreground = neutralColor;
            RccLogStatusText.Foreground = neutralColor;
        }

        private void ShowServiceNotConfiguredError()
        {
            WebsiteStatusText.Text = "Services not configured";
            WebsiteStatusText.Foreground = new SolidColorBrush(Colors.Orange);
        }

        private void ShowServiceInitializationError(string message)
        {
            WebsiteStatusText.Text = "Initialization failed";
            WebsiteStatusText.Foreground = new SolidColorBrush(Colors.Red);
        }

        private void UpdateServiceStatusUIWithError(string errorMessage)
        {
            WebsiteStatusText.Text = "Error";
            WebsiteStatusText.Foreground = new SolidColorBrush(Colors.Red);
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            ClearServiceStatusFields();
            await LoadServiceData();
        }

        private async void SaveMessageButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var mainWindow = Window.GetWindow(this) as Main;
                var statusTextBlock = mainWindow?.FindName("StatusTextBlock") as System.Windows.Controls.TextBlock;

                if (statusTextBlock != null)
                {
                    statusTextBlock.Text = "Saving global message...";
                }
                
                var message = GlobalMessageTextBox.Text.Trim();
                await _websiteService.UpdateGlobalMessageAsync(string.IsNullOrEmpty(message) ? null : message);

                if (statusTextBlock != null)
                {
                    statusTextBlock.Text = "Global message saved successfully!";
                }

                if (statusTextBlock != null)
                {
                    var timer = new System.Windows.Threading.DispatcherTimer
                    {
                        Interval = TimeSpan.FromSeconds(3)
                    };
                    timer.Tick += (s, args) =>
                    {
                        statusTextBlock.Text = "Ready";
                        timer.Stop();
                    };
                    timer.Start();
                }
            }
            catch (Exception ex)
            {
                if (Control_Panel.DatabaseUtilities.IsDatabaseError(ex))
                {
                    var result = MessageBox.Show(
                        "Database issue happened, are you sure you migrated? (Clicking Yes if database is working wont do anything)",
                        "Database Issue",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        var dbConnectionWindow = new DatabaseConnectionWindow();
                        dbConnectionWindow.Owner = Window.GetWindow(this);
                        dbConnectionWindow.ShowDialog();
                    }
                    else
                    {
                        var mainWindow = Window.GetWindow(this) as Main;
                        mainWindow?.SwitchToView("Database");
                    }
                }
                else
                {
                    var mainWindow = Window.GetWindow(this) as Main;
                    var statusTextBlock = mainWindow?.FindName("StatusTextBlock") as System.Windows.Controls.TextBlock;
                    
                    if (statusTextBlock != null)
                    {
                        statusTextBlock.Text = $"Failed to save global message: {ex.Message}";
                        var timer = new System.Windows.Threading.DispatcherTimer
                        {
                            Interval = TimeSpan.FromSeconds(5)
                        };
                        timer.Tick += (s, args) =>
                        {
                            statusTextBlock.Text = "Ready";
                            timer.Stop();
                        };
                        timer.Start();
                    }
                    else
                    {
                        MessageBox.Show($"Failed to save global message: {ex.Message}", "Error", 
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void MaintenanceButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var mainWindow = Window.GetWindow(this) as Main;
                var statusTextBlock = mainWindow?.FindName("StatusTextBlock") as System.Windows.Controls.TextBlock;

                if (statusTextBlock != null)
                {
                    statusTextBlock.Text = "Updating Maintenance mode...";
                }
                
                var newMaintenanceState = !_currentSettings.MaintenanceModeEnabled;
                var reason = MaintenanceReasonTextBox.Text.Trim();
                
                
                await _websiteService.UpdateMaintenanceModeAsync(newMaintenanceState, reason);

                _currentSettings.MaintenanceModeEnabled = newMaintenanceState;
                _currentSettings.MaintenanceModeReason = reason;
                UpdateMaintenanceUI();

                if (statusTextBlock != null)
                {
                    statusTextBlock.Text = $"Maintenance mode {(newMaintenanceState ? "activated" : "deactivated")} successfully!";
                }

                if (statusTextBlock != null)
                {
                    var timer = new System.Windows.Threading.DispatcherTimer
                    {
                        Interval = TimeSpan.FromSeconds(3)
                    };
                    timer.Tick += (s, args) =>
                    {
                        statusTextBlock.Text = "Ready";
                        timer.Stop();
                    };
                    timer.Start();
                }
            }
            catch (Exception ex)
            {
                if (Control_Panel.DatabaseUtilities.IsDatabaseError(ex))
                {
                    var result = MessageBox.Show(
                        "Database issue happened, are you sure you migrated?  (Clicking Yes if database is working wont do anything)",
                        "Database Issue",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        var dbConnectionWindow = new DatabaseConnectionWindow();
                        dbConnectionWindow.Owner = Window.GetWindow(this);
                        dbConnectionWindow.ShowDialog();
                    }
                    else
                    {
                        var mainWindow = Window.GetWindow(this) as Main;
                        mainWindow?.SwitchToView("Database");
                    }
                }
                else
                {
                    var mainWindow = Window.GetWindow(this) as Main;
                    var statusTextBlock = mainWindow?.FindName("StatusTextBlock") as System.Windows.Controls.TextBlock;
                    
                    if (statusTextBlock != null)
                    {
                        statusTextBlock.Text = $"Failed to update Maintenance mode: {ex.Message}";
                        var timer = new System.Windows.Threading.DispatcherTimer
                        {
                            Interval = TimeSpan.FromSeconds(5)
                        };
                        timer.Tick += (s, args) =>
                        {
                            statusTextBlock.Text = "Ready";
                            timer.Stop();
                        };
                        timer.Start();
                    }
                    else
                    {
                        MessageBox.Show($"Failed to update Maintenance mode: {ex.Message}", "Error", 
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void LoadWebsiteSettingsAsync()
        {
            try
            {
                _currentSettings = await _websiteService.GetWebsiteSettingsAsync();
                Dispatcher.Invoke(() =>
                {
                    GlobalMessageTextBox.Text = _currentSettings.GlobalMessage ?? string.Empty;
                    MaintenanceReasonTextBox.Text = _currentSettings.MaintenanceModeReason ?? string.Empty;
                    UpdateMaintenanceUI();
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    if (Control_Panel.DatabaseUtilities.IsDatabaseError(ex))
                    {
                        var result = MessageBox.Show(
                            "Database issue happened, are you sure you migrated?",
                            "Database Issue",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);
                        
                        if (result == MessageBoxResult.No)
                        {
                            var mainWindow = Window.GetWindow(this) as Main;
                            mainWindow?.SwitchToView("Database");
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Failed to load website settings: {ex.Message}", "Error", 
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
            }
        }

        private void UpdateMaintenanceUI()
        {
            if (_currentSettings.MaintenanceModeEnabled)
            {
                MaintenanceButton.Content = "Deactivate";
                MaintenanceStatusText.Text = "Active";
                MaintenanceStatusText.Foreground = System.Windows.Media.Brushes.Green;
            }
            else
            {
                MaintenanceButton.Content = "Activate";
                MaintenanceStatusText.Text = "Inactive";
                MaintenanceStatusText.Foreground = System.Windows.Media.Brushes.Red;
            }

            MaintenanceReasonTextBox.IsEnabled = true;
            MaintenanceReasonTextBox.Height = 35;
        }
    }
}
