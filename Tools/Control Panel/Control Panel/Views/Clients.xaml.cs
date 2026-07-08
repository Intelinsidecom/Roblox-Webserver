using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using Npgsql;
using ControlPanel.Functions;
using Control_Panel.Properties;

namespace Control_Panel
{
    /// <summary>
    /// Interaction logic for ClientsView.xaml
    /// </summary>
    public partial class ClientsView : UserControl
    {
        private readonly HttpClient _httpClient;
        private readonly SetupService _setupService;
        
        private const string DefaultDbConnectionString = "Host=localhost;Database=postgres;Username=postgres;Password=password";
        private const string DefaultClientsInputFolder = "C:\\Clients";
        private const string DefaultSetupHost = "localhost";
        private const string DefaultSetupPort = "5192";
        private const string DefaultSetupServiceLocation = "C:\\SetupService";
        
        public ClientsView()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
            
            
            string dbConnectionString = DefaultDbConnectionString;
            try
            {
                dbConnectionString = Settings.Default.DatabaseConnectionString ?? DefaultDbConnectionString;
            }
            catch
            {
                ConsoleWindow.Instance?.WriteWarning("DatabaseConnectionString setting found, using defaults");
            }
            
            var dbConnection = new NpgsqlConnection(dbConnectionString);
            string clientsInputFolder = DefaultClientsInputFolder;
            _setupService = new SetupService(clientsInputFolder, dbConnection, null);
            this.Loaded += ClientsView_Loaded;
            this.Unloaded += ClientsView_Unloaded;
            BootstrapperTextBox.TextChanged += BootstrapperTextBox_TextChanged;
        }
        
        private async void ClientsView_Loaded(object sender, RoutedEventArgs e)
        {
            await RefreshServiceStatus();
            LoadCurrentConfiguration();
        }
        
        private void LoadCurrentConfiguration()
        {
            try
            {
                string setupHost = DefaultSetupHost;
                try
                {
                    setupHost = Settings.Default.SetupHost ?? DefaultSetupHost;
                }
                catch
                {
                    ConsoleWindow.Instance?.WriteWarning("SetupHost setting not found, using default");
                }
                SetupHostTextBox.Text = setupHost;
                
                string setupPort = DefaultSetupPort;
                try
                {
                    setupPort = Settings.Default.SetupPort ?? DefaultSetupPort;
                }
                catch
                {
                    ConsoleWindow.Instance?.WriteWarning("SetupPort setting not found, using default");
                }
                ServiceHostPortTextBox.Text = setupPort;
                
                string setupServiceLocation = DefaultSetupServiceLocation;
                try
                {
                    setupServiceLocation = Settings.Default.SetupServiceLocation ?? DefaultSetupServiceLocation;
                }
                catch
                {
                    ConsoleWindow.Instance?.WriteWarning("SetupServiceLocation setting not found, using default");
                }
                SetupServiceLocationTextBox.Text = setupServiceLocation;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading configuration: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async Task RefreshServiceStatus()
        {
            try
            {
                var version = await _setupService.GetClientVersionAsync("WindowsPlayer");
                ConsoleWindow.Instance?.WriteLine($"Database connection successful - Version: {version}");
            }
            catch (Exception ex)
            {
                ConsoleWindow.Instance?.WriteLine($"Setup service error: {ex.Message}");
            }
        }
        
        private void ClientsView_Unloaded(object sender, RoutedEventArgs e)
        {
            _httpClient?.Dispose();
            _setupService?.Dispose();
        }
        
        private void ClientUploadTabButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchTab("ClientUpload");
        }
        
        private void ClientConfigurationTabButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchTab("ClientConfiguration");
        }
        
        private void PlayerClientRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            UpdateBootstrapperLabel("Player");
        }
        
        private void StudioClientRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            UpdateBootstrapperLabel("Studio");
        }
        
        private void RccClientRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            UpdateBootstrapperLabel("RCC");
        }
        
        private void UpdateBootstrapperLabel(string clientType)
        {
            BootstrapperLabel.Text = $"Choose {clientType} Bootstrapper:";
        }
        
        private void SwitchTab(string tabName)
        {
            ClientUploadTab.Visibility = Visibility.Collapsed;
            ClientConfigurationTab.Visibility = Visibility.Collapsed;
            ResetTabButtonStyles();
            
            switch (tabName)
            {
                case "ClientUpload":
                    ClientUploadTab.Visibility = Visibility.Visible;
                    ClientUploadTabButton.Background = (System.Windows.Media.Brush)FindResource("AccentPrimary");
                    ClientUploadTabButton.Foreground = System.Windows.Media.Brushes.White;
                    ClientUploadTabButton.BorderBrush = null;
                    break;
                case "ClientConfiguration":
                    ClientConfigurationTab.Visibility = Visibility.Visible;
                    ClientConfigurationTabButton.Background = (System.Windows.Media.Brush)FindResource("AccentPrimary");
                    ClientConfigurationTabButton.Foreground = System.Windows.Media.Brushes.White;
                    ClientConfigurationTabButton.BorderBrush = null;
                    break;
            }
        }
        
        private void ResetTabButtonStyles()
        {
            ClientUploadTabButton.Background = System.Windows.Media.Brushes.Transparent;
            ClientUploadTabButton.BorderBrush = (System.Windows.Media.Brush)FindResource("SubtleText");
            ClientUploadTabButton.Foreground = (System.Windows.Media.Brush)FindResource("Foreground");
            
            ClientConfigurationTabButton.Background = System.Windows.Media.Brushes.Transparent;
            ClientConfigurationTabButton.BorderBrush = (System.Windows.Media.Brush)FindResource("SubtleText");
            ClientConfigurationTabButton.Foreground = (System.Windows.Media.Brush)FindResource("Foreground");
        }
        
        private void BrowseClientFolderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var folderDialog = new System.Windows.Forms.FolderBrowserDialog
                {
                    Description = "Choose the folder containing client files",
                    ShowNewFolderButton = false
                };
                
                if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    ClientFolderTextBox.Text = folderDialog.SelectedPath;
                    LoadClientVersions();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error browsing for client folder: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void BrowseBootstrapperButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Title = "Select Bootstrapper File",
                    Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*",
                    FilterIndex = 1
                };
                
                if (openFileDialog.ShowDialog() == true)
                {
                    BootstrapperTextBox.Text = openFileDialog.FileName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error browsing for bootstrapper file: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void LoadClientVersions()
        {
            try
            {
                ConsoleWindow.Instance?.WriteLine("Client type radio buttons initialized");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing client types: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void UploadClientsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var folderPath = ClientFolderTextBox.Text.Trim();
                var bootstrapperPath = BootstrapperTextBox.Text.Trim();
                string selectedClientType = null;
                
                if (PlayerClientRadioButton.IsChecked == true)
                    selectedClientType = "Player";
                else if (StudioClientRadioButton.IsChecked == true)
                    selectedClientType = "Studio";
                else if (RccClientRadioButton.IsChecked == true)
                    selectedClientType = "RCC";
                
                if (string.IsNullOrEmpty(folderPath))
                {
                    ShowUploadStatus("Please select a client folder.", false);
                    return;
                }
                
                if (!System.IO.Directory.Exists(folderPath))
                {
                    ShowUploadStatus("The selected folder does not exist.", false);
                    return;
                }
                
                if (string.IsNullOrEmpty(selectedClientType))
                {
                    ShowUploadStatus("Please select a client type to upload.", false);
                    return;
                }
                if (string.IsNullOrEmpty(bootstrapperPath))
                {
                    ShowUploadStatus("Please select a bootstrapper file.", false);
                    return;
                }
                
                var bootstrapperVersion = BootstrapperVersionInputTextBox.Text.Trim();
                if (string.IsNullOrEmpty(bootstrapperVersion))
                {
                    ShowUploadStatus("Please enter a bootstrapper version.", false);
                    return;
                }
                
                Dispatcher.Invoke(() => ShowUploadStatus("Uploading client...", true));
                
                var capturedBootstrapperVersion = Dispatcher.Invoke(() => BootstrapperVersionInputTextBox.Text.Trim());

                _ = Task.Run(async () =>
                {
                    try
                    {
                        UploadResult result;
                        
                        if (selectedClientType == "Player")
                        {
                            result = await _setupService.UploadPlayerClientAsync(folderPath, bootstrapperPath, capturedBootstrapperVersion);
                        }
                        else if (selectedClientType == "Studio")
                        {
                            result = await _setupService.UploadStudioClientAsync(folderPath, bootstrapperPath, capturedBootstrapperVersion);
                        }
                        else if (selectedClientType == "RCC")
                        {
                            result = await _setupService.UploadRccClientAsync(folderPath, capturedBootstrapperVersion);
                        }
                        else
                        {
                            result = new UploadResult
                            {
                                Success = false,
                                Error = $"Invalid client type: {selectedClientType}"
                            };
                        }
                        
                        Dispatcher.Invoke(() =>
                        {
                            if (result.Success)
                            {
                                var message = $"Upload completed successfully!\n\n" +
                                             $"Folder: {folderPath}\n" +
                                             $"Client Type: {selectedClientType}\n" +
                                             $"Client MD5 Hash: {result.UploadId}\n" +
                                             $"Files uploaded: {result.UploadedFiles.Count}" +
                                             (!string.IsNullOrEmpty(bootstrapperPath) ? "\nBootstrapper: Included" : "");
                                
                                ShowUploadStatus("Upload completed successfully!", true);
                                MessageBox.Show(message, "Upload Complete", 
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                ShowUploadStatus($"Upload failed: {result.Error}", false);
                                MessageBox.Show($"Upload failed: {result.Error}", "Upload Error", 
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            ShowUploadStatus($"Upload failed: {ex.Message}", false);
                            MessageBox.Show($"Error uploading clients: {ex.Message}\n\nStack trace:\n{ex.StackTrace}", "Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                ShowUploadStatus($"Upload failed: {ex.Message}", false);
                MessageBox.Show($"Error uploading clients: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void ShowUploadStatus(string message, bool isSuccess)
        {
            UploadStatusText.Text = message;
            UploadStatusText.Foreground = isSuccess ? 
                System.Windows.Media.Brushes.Green : 
                System.Windows.Media.Brushes.Red;
            UploadStatusText.Visibility = Visibility.Visible;
            
            if (isSuccess && message != "Uploading client...")
            {
                var timer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(5)
                };
                timer.Tick += (s, e) =>
                {
                    UploadStatusText.Visibility = Visibility.Collapsed;
                    timer.Stop();
                };
                timer.Start();
            }
        }
        
        private void BootstrapperTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var bootstrapperPath = BootstrapperTextBox.Text.Trim();
            
            if (string.IsNullOrEmpty(bootstrapperPath))
            {
                BootstrapperVersionText.Text = "No bootstrapper selected";
                return;
            }
            
            if (!File.Exists(bootstrapperPath))
            {
                BootstrapperVersionText.Text = "File not found";
                return;
            }
            
        }
        
        private void SeeBootstrapperVersionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var bootstrapperPath = BootstrapperTextBox.Text.Trim();
                
                if (string.IsNullOrEmpty(bootstrapperPath))
                {
                    MessageBox.Show("Choose bootstrapper first", "Warning", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                if (!File.Exists(bootstrapperPath))
                {
                    MessageBox.Show("Bootstrapper file not found: " + bootstrapperPath, "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                try
                {
                    var startInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = bootstrapperPath,
                        Arguments = "--version",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    };
                    
                    var process = System.Diagnostics.Process.Start(startInfo);
                    var output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    
                    var version = output.Trim();
                    if (!string.IsNullOrEmpty(version))
                    {
                        BootstrapperVersionText.Text = "Unable to get version";
                    }
                    else
                    {
                        BootstrapperVersionText.Text = version;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error running bootstrapper: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking bootstrapper file: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void DeployHistoryHyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            try
            {
                var setupHost = "localhost";
                var setupPort = "5192";
                
                try
                {
                    setupHost = Settings.Default.SetupHost ?? "localhost";
                    setupPort = Settings.Default.SetupPort ?? "5192";
                }
                catch
                {
                    ConsoleWindow.Instance?.WriteWarning("SetupHost/SetupPort settings not found, using defaults");
                }
                
                var deployHistoryUrl = $"http://{setupHost}:{setupPort}/DeployHistory.txt";
                
                try
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var history = await _setupService.GetDeployHistoryAsync();
                            ConsoleWindow.Instance?.WriteLine($"Deploy history fetched successfully ({history.Length} characters)");
                        }
                        catch (Exception ex)
                        {
                            ConsoleWindow.Instance?.WriteError($"Failed to fetch deploy history: {ex.Message}");
                        }
                    });
                }
                catch (Exception ex)
                {
                    ConsoleWindow.Instance?.WriteError($"Error starting deploy history fetch: {ex.Message}");
                }
                
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = deployHistoryUrl,
                    UseShellExecute = true
                });
                
                e.Handled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening deploy history: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = true;
            }
        }
        
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            _ = RefreshServiceStatus();
        }
        
        private void BrowseSetupServiceLocationButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var folderDialog = new System.Windows.Forms.FolderBrowserDialog
                {
                    Description = "Choose the setup service location folder",
                    ShowNewFolderButton = true
                };
                
                if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    SetupServiceLocationTextBox.Text = folderDialog.SelectedPath;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error browsing for setup service location: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void SaveConfigButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var setupHost = SetupHostTextBox.Text.Trim();
                var setupPort = ServiceHostPortTextBox.Text.Trim();
                var setupServiceLocation = SetupServiceLocationTextBox.Text.Trim();
                
                var parentWindow = Window.GetWindow(this);
                var statusTextBlock = FindChild<TextBlock>(parentWindow, "StatusTextBlock");
                
                if (string.IsNullOrEmpty(setupHost))
                {
                    if (statusTextBlock != null)
                    {
                        statusTextBlock.Text = "Please enter a setup host.";
                        statusTextBlock.Foreground = (Brush)FindResource("Error");
                    }
                    return;
                }
                
                if (string.IsNullOrEmpty(setupPort))
                {
                    if (statusTextBlock != null)
                    {
                        statusTextBlock.Text = "Please enter a setup port.";
                        statusTextBlock.Foreground = (Brush)FindResource("Error");
                    }
                    return;
                }
                
                if (string.IsNullOrEmpty(setupServiceLocation))
                {
                    if (statusTextBlock != null)
                    {
                        statusTextBlock.Text = "Please select a setup service location.";
                        statusTextBlock.Foreground = (Brush)FindResource("Error");
                    }
                    return;
                }
                
                if (!int.TryParse(setupPort, out int port) || port < 1 || port > 65535)
                {
                    if (statusTextBlock != null)
                    {
                        statusTextBlock.Text = "Invalid port number (must be 1-65535).";
                        statusTextBlock.Foreground = (Brush)FindResource("Error");
                    }
                    return;
                }
                
                Settings.Default.SetupHost = setupHost;
                Settings.Default.SetupPort = setupPort;
                Settings.Default.SetupServiceLocation = setupServiceLocation;
                Settings.Default.Save();
                
                if (statusTextBlock != null)
                {
                    statusTextBlock.Text = "Configuration saved successfully!";
                }
            }
            catch (Exception ex)
            {
                var parentWindow = Window.GetWindow(this);
                var statusTextBlock = FindChild<TextBlock>(parentWindow, "StatusTextBlock");
                
                if (statusTextBlock != null)
                {
                    statusTextBlock.Text = $"Error saving configuration: {ex.Message}";
                    statusTextBlock.Foreground = (Brush)FindResource("Error");
                }
            }
        }
        
        private void RevertClientButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var clientHash = ClientHashTextBox.Text.Trim();
                string selectedRevertType = null;
                
                if (RevertPlayerRadioButton.IsChecked == true)
                    selectedRevertType = "Player";
                else if (RevertStudioRadioButton.IsChecked == true)
                    selectedRevertType = "Studio";
                else if (RevertRccRadioButton.IsChecked == true)
                    selectedRevertType = "RCC";
                
                if (string.IsNullOrEmpty(clientHash))
                {
                    ShowRevertStatus("Please enter a client hash.", false);
                    return;
                }
                
                if (string.IsNullOrEmpty(selectedRevertType))
                {
                    ShowRevertStatus("Please select a revert type.", false);
                    return;
                }
                
                ShowRevertStatus("Reverting client...", true);
                
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var result = await _setupService.RevertClientAsync(selectedRevertType, clientHash);
                        
                        Dispatcher.Invoke(() =>
                        {
                            if (result.Success)
                            {
                                var message = $"Revert completed successfully!\n\n" +
                                             $"Client Hash: {clientHash}\n" +
                                             $"Revert Type: {selectedRevertType}\n" +
                                             $"Reverted Hash: {result.RevertedHash}\n" +
                                             $"Files reverted: {result.RevertedFiles.Count}";
                                
                                ShowRevertStatus("Revert completed successfully!", true);
                                MessageBox.Show(message, "Revert Complete", 
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                ShowRevertStatus($"Revert failed: {result.Error}", false);
                                MessageBox.Show($"Revert failed: {result.Error}", "Revert Error", 
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            ShowRevertStatus($"Revert failed: {ex.Message}", false);
                            MessageBox.Show($"Error reverting client: {ex.Message}", "Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                ShowRevertStatus($"Revert failed: {ex.Message}", false);
                MessageBox.Show($"Error reverting client: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void ShowRevertStatus(string message, bool isSuccess)
        {
            RevertStatusText.Text = message;
            RevertStatusText.Foreground = isSuccess ? 
                System.Windows.Media.Brushes.Green : 
                System.Windows.Media.Brushes.Red;
            RevertStatusText.Visibility = Visibility.Visible;
            
            if (isSuccess)
            {
                var timer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(5)
                };
                timer.Tick += (s, e) =>
                {
                    RevertStatusText.Visibility = Visibility.Collapsed;
                    timer.Stop();
                };
                timer.Start();
            }
        }
        
        /// <summary>
        /// Helper method to find child controls by name
        /// </summary>
        private static T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                
                if (child is T && (child as FrameworkElement)?.Name == childName)
                {
                    foundChild = (T)child;
                    return foundChild;
                }

                var childOfChild = FindChild<T>(child, childName);
                if (childOfChild != null)
                    return childOfChild;
            }

            return foundChild;
        }
    }
}
