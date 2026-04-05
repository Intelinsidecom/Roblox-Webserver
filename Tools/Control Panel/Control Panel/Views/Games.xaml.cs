using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using Control_Panel.Properties;
using ControlPanel.Functions;

namespace Control_Panel
{
    /// <summary>
    /// Interaction logic for Games.xaml
    /// </summary>
    public partial class GamesView : UserControl
    {
        private readonly GamesService _gamesService;
        private ObservableCollection<GamesService.GameServerInfo> _servers;
        private bool _isInitialized = false;
        private DispatcherTimer _autoRefreshTimer;
        private GamesService.AuthenticationTicketInfo? _currentTicket;

        public GamesView()
        {
            InitializeComponent();
            
            _gamesService = new GamesService();
            _servers = new ObservableCollection<GamesService.GameServerInfo>();
            ServersDataGrid.ItemsSource = _servers;
            CreateServerPanel.Visibility = Visibility.Visible;
            ServerDetailsHeader.Visibility = Visibility.Collapsed;
            ServerInfoPanel.Visibility = Visibility.Collapsed;
            TimingInfoPanel.Visibility = Visibility.Collapsed;
            AuthenticationPanel.Visibility = Visibility.Collapsed;
            ServerActionButtons.Visibility = Visibility.Collapsed;
            _ = LoadServersAsync();
            InitializeAutoRefreshTimer();
            ServersDataGrid.SelectedItem = null;
            this.Unloaded += GamesView_Unloaded;
        }

        /// <summary>
        /// Cleanup timer when control is unloaded
        /// </summary>
        private void GamesView_Unloaded(object sender, RoutedEventArgs e)
        {
            _autoRefreshTimer?.Stop();
            _autoRefreshTimer = null;
        }

        /// <summary>
        /// Initialize the auto-refresh timer to update every minute
        /// </summary>
        private void InitializeAutoRefreshTimer()
        {
            _autoRefreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1)
            };
            _autoRefreshTimer.Tick += AutoRefreshTimer_Tick;
            _autoRefreshTimer.Start();
        }

        /// <summary>
        /// Timer tick event handler for auto-refresh
        /// </summary>
        private async void AutoRefreshTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                await LoadServersAsync();
                UpdateStatus("Auto-refreshed server list", "info");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Auto-refresh failed: {ex.Message}", "warning");
            }
        }

        /// <summary>
        /// Update the status bar in the main window
        /// </summary>
        private void UpdateStatus(string message, string type = "info")
        {
            try
            {
                var parentWindow = Window.GetWindow(this);
                if (parentWindow != null)
                {
                    var statusTextBlock = FindChild<TextBlock>(parentWindow, "StatusTextBlock");
                    if (statusTextBlock != null)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            statusTextBlock.Text = message;
                            var color = type.ToLower() switch
                            {
                                "info" => new SolidColorBrush(Colors.White),
                                "success" => new SolidColorBrush(Colors.White),
                                "warning" => new SolidColorBrush(Colors.Yellow),
                                "error" => new SolidColorBrush(Colors.Red),
                                _ => new SolidColorBrush(Colors.White)
                            };
                            
                            statusTextBlock.Foreground = color;
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating status: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Helper method to find child controls by name
        /// </summary>
        private static T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T && (child as FrameworkElement)?.Name == childName)
                {
                    return (T)child;
                }

                var childOfChild = FindChild<T>(child, childName);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }
            
        private async Task LoadServersAsync()
        {
            try
            {
                var servers = await _gamesService.GetAllGameServersAsync();
                
                var updatedServers = new List<GamesService.GameServerInfo>();
                foreach (var server in servers)
                {
                    var webPlayerCount = await _gamesService.GetPlayerCountFromWebApiAsync(server.GameId);
                    if (webPlayerCount.HasValue)
                    {
                        server.PlayerCount = webPlayerCount.Value;
                    }
                    updatedServers.Add(server);
                }
                
                Dispatcher.Invoke(() =>
                {
                    var selectedServer = ServersDataGrid.SelectedItem as GamesService.GameServerInfo;
                    bool selectedServerStillExists = false;
                    
                    _servers.Clear();
                    foreach (var server in updatedServers)
                    {
                        _servers.Add(server);
                        if (selectedServer != null && server.GameId == selectedServer.GameId)
                        {
                            selectedServerStillExists = true;
                        }
                    }
                    
                    if (!selectedServerStillExists && _currentTicket != null)
                    {
                        ClearTicketDisplay();
                    }
                    
                    UpdateStatistics();
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Failed to load servers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        private void UpdateStatistics()
        {
            var totalServers = _servers.Count;
            var totalPlayers = _servers.Sum(s => s.PlayerCount);
            var runningServers = _servers.Count(s => s.Status.Equals("running", StringComparison.OrdinalIgnoreCase));
            var startingServers = _servers.Count(s => s.Status.Equals("starting", StringComparison.OrdinalIgnoreCase));
            TotalServersText.Text = totalServers.ToString();
            TotalPlayersText.Text = totalPlayers.ToString();
            RunningServersText.Text = runningServers.ToString();
            ExpiredServersText.Text = startingServers.ToString();
        }

        private void ServersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedServer = ServersDataGrid.SelectedItem as GamesService.GameServerInfo;
            if (selectedServer != null)
            {
                UpdateServerDetails(selectedServer);
                KillServerButton.IsEnabled = true;
                RefreshServerButton.IsEnabled = true;
                CreateServerPanel.Visibility = Visibility.Collapsed;
                ServerDetailsHeader.Visibility = Visibility.Visible;
                ServerInfoPanel.Visibility = Visibility.Visible;
                TimingInfoPanel.Visibility = Visibility.Visible;
                CloseServerDetailsButton.Visibility = Visibility.Visible;
                AuthenticationPanel.Visibility = Visibility.Visible;
                ServerActionButtons.Visibility = Visibility.Visible;
                
                if (_currentTicket != null && (!string.IsNullOrEmpty(_currentTicket.GameId) && _currentTicket.GameId != selectedServer.GameId))
                {
                    ClearTicketDisplay();
                }
            }
            else
            {
                ClearServerDetails();
                ClearTicketDisplay();
                KillServerButton.IsEnabled = false;
                RefreshServerButton.IsEnabled = false;
                CreateServerPanel.Visibility = Visibility.Visible;
                ServerDetailsHeader.Visibility = Visibility.Collapsed;
                ServerInfoPanel.Visibility = Visibility.Collapsed;
                TimingInfoPanel.Visibility = Visibility.Collapsed;
                CloseServerDetailsButton.Visibility = Visibility.Collapsed;
                AuthenticationPanel.Visibility = Visibility.Collapsed;
                ServerActionButtons.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateServerDetails(GamesService.GameServerInfo server)
        {
            DetailGameIdText.Text = server.GameId;
            DetailPlaceIdText.Text = server.PlaceId.ToString();
            DetailPortText.Text = server.Port.ToString();
            DetailPlayersText.Text = $"{server.PlayerCount}/{server.MaxPlayers}";
            DetailPrivateServerText.Text = string.IsNullOrEmpty(server.PrivateServerId) ? "No" : server.PrivateServerId;
            DetailStartTimeText.Text = server.StartTime.ToString("yyyy-MM-dd HH:mm:ss");
            DetailExpirationText.Text = server.Expiration.ToString("yyyy-MM-dd HH:mm:ss");
            DetailLastActivityText.Text = server.LastActivityTime.ToString("yyyy-MM-dd HH:mm:ss");
            DetailInactivityTimeoutText.Text = server.InactivityTimeout.ToString(@"hh\:mm\:ss");
        }

        private void ClearServerDetails()
        {
            DetailGameIdText.Text = "N/A";
            DetailPlaceIdText.Text = "N/A";
            DetailPortText.Text = "N/A";
            DetailPlayersText.Text = "N/A";
            DetailPrivateServerText.Text = "N/A";
            DetailStartTimeText.Text = "N/A";
            DetailExpirationText.Text = "N/A";
            DetailLastActivityText.Text = "N/A";
            DetailInactivityTimeoutText.Text = "N/A";
        }

        private void ClearTicketDisplay()
        {
            _currentTicket = null;
            TicketInfoPanel.Visibility = Visibility.Collapsed;
            JoinScriptUrlTextBox.Text = string.Empty;
            AuthTicketTextBox.Text = string.Empty;
            AuthUrlTextBox.Text = string.Empty;
            TicketExpirationText.Text = "N/A";
            CommandLineArgsTextBox.Text = string.Empty;
        }


        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await LoadServersAsync();
                UpdateStatus("Server list refreshed successfully", "success");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Failed to refresh servers: {ex.Message}", "error");
            }
        }

        private async void KillAllButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                $"Are you sure you want to kill all {_servers.Count} running servers?",
                "Kill All Servers",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var success = await _gamesService.KillAllGameServersAsync();
                    if (success)
                    {
                        await LoadServersAsync();
                        ClearServerDetails();
                        UpdateStatus($"All {_servers.Count} servers killed successfully", "success");
                    }
                    else
                    {
                        UpdateStatus("Some servers could not be killed", "warning");
                    }
                }
                catch (Exception ex)
                {
                    UpdateStatus($"Failed to kill servers: {ex.Message}", "error");
                }
            }
        }

        private async void KillServerButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedServer = ServersDataGrid.SelectedItem as GamesService.GameServerInfo;
            if (selectedServer == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to kill server {selectedServer.GameId}?",
                "Kill Server",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var response = await _gamesService.StopGameServerAsync(selectedServer.GameId);
                    if (response != null)
                    {
                        await LoadServersAsync();
                        ClearServerDetails();
                        UpdateStatus($"Server {selectedServer.GameId} killed successfully", "success");
                    }
                    else
                    {
                        UpdateStatus($"Failed to kill server {selectedServer.GameId}", "error");
                    }
                }
                catch (Exception ex)
                {
                    UpdateStatus($"Failed to kill server: {ex.Message}", "error");
                }
            }
        }

        private async void RefreshServerButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedServer = ServersDataGrid.SelectedItem as GamesService.GameServerInfo;
            if (selectedServer == null) return;

            try
            {
                var status = await _gamesService.GetGameServerStatusAsync(selectedServer.GameId);
                if (status != null)
                {
                    var updatedServer = new GamesService.GameServerInfo
                    {
                        GameId = status.GameId,
                        PlaceId = status.PlaceId,
                        Port = status.Port,
                        MaxPlayers = status.MaxPlayers,
                        PlayerCount = status.PlayerCount,
                        Status = status.Status,
                        StartTime = status.StartTime,
                        Expiration = status.Expiration,
                        BaseUrl = status.BaseUrl,
                        PrivateServerId = status.PrivateServerId,
                        LastActivityTime = status.LastActivityTime,
                        InactivityTimeout = TimeSpan.Parse("01:00:00")
                    };
                    
                    var index = _servers.ToList().FindIndex(s => s.GameId == selectedServer.GameId);
                    if (index >= 0)
                    {
                        _servers[index] = updatedServer;
                        UpdateServerDetails(updatedServer);
                        UpdateStatistics();
                    }
                    UpdateStatus($"Server {selectedServer.GameId} status refreshed", "success");
                }
                else
                {
                    UpdateStatus($"Failed to get status for server {selectedServer.GameId}", "error");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Failed to refresh server status: {ex.Message}", "error");
            }
        }

        private async void CreateServerButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(CreatePlaceIdTextBox.Text, out int placeId) || placeId <= 0)
            {
                UpdateStatus("Please enter a valid Place ID (positive number)", "warning");
                return;
            }

            if (!int.TryParse(CreateExpirationTextBox.Text, out int expirationMinutes) || expirationMinutes < 0)
            {
                UpdateStatus("Please enter a valid expiration time (0 or positive number of minutes)", "warning");
                return;
            }

            if (!int.TryParse(CreateMaxPlayersTextBox.Text, out int maxPlayers) || maxPlayers <= 0 || maxPlayers > 100)
            {
                UpdateStatus("Please enter a valid max players count (1-100)", "warning");
                return;
            }

            try
            {
                var request = new GamesService.StartGameServerRequest
                {
                    PlaceId = placeId,
                    MaxPlayers = maxPlayers,
                    MaxInactive = expirationMinutes,
                    BaseUrl = Settings.Default.PublicBaseUrl
                };

                var response = await _gamesService.StartGameServerAsync(request);
                if (response != null)
                {
                    await LoadServersAsync();
                    
                    var newServer = _servers.FirstOrDefault(s => s.GameId == response.GameId);
                    if (newServer != null)
                    {
                        ServersDataGrid.SelectedItem = newServer;
                    }

                    UpdateStatus($"Server Successfully Created ({response.GameId})", "success");
                }
                else
                {
                    UpdateStatus("Failed to create server", "error");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Failed to create server: {ex.Message}", "error");
            }
        }

        private void CloseServerDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            ServersDataGrid.SelectedItem = null;
        }

        #region Authentication Ticket Methods

        /// <summary>
        /// Generate authentication ticket for the selected server
        /// </summary>
        private async void GenerateTicketButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedServer = ServersDataGrid.SelectedItem as GamesService.GameServerInfo;
                if (selectedServer == null)
                {
                    UpdateStatus("Please select a server first", "error");
                    return;
                }

                GenerateTicketButton.IsEnabled = false;
                GenerateTicketButton.Content = "Generating...";
                UpdateStatus("Generating authentication ticket...", "info");

                // Use the new server-specific ticket creation method
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Calling CreateAuthenticationTicketForServerAsync with selectedServer.GameId='{selectedServer?.GameId}'");
                var ticket = await CreateAuthenticationTicketForServerAsync(selectedServer.PlaceId, selectedServer.GameId, selectedServer.Port);
                if (ticket != null)
                {
                    _currentTicket = ticket;
                    DisplayTicketInfo(ticket);
                    UpdateStatus($"Authentication ticket generated for server {selectedServer.GameId}", "success");
                }
                else
                {
                    UpdateStatus("Failed to generate authentication ticket", "error");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error generating ticket: {ex.Message}", "error");
            }
            finally
            {
                GenerateTicketButton.IsEnabled = true;
                GenerateTicketButton.Content = "Generate Authentication Ticket";
            }
        }

        /// <summary>
        /// Create an authentication ticket for a specific game server
        /// </summary>
        private async Task<GamesService.AuthenticationTicketInfo?> CreateAuthenticationTicketForServerAsync(long placeId, string gameId, int serverPort)
        {
            return await _gamesService.CreateAuthenticationTicketForServerAsync(placeId, gameId, serverPort);
        }

        /// <summary>
        /// Create an authentication ticket for the specified place
        /// </summary>
        private async Task<GamesService.AuthenticationTicketInfo?> CreateAuthenticationTicketAsync(long placeId)
        {
            return await _gamesService.CreateAuthenticationTicketAsync(placeId);
        }

        /// <summary>
        /// Display the ticket information in the UI
        /// </summary>
        private void DisplayTicketInfo(GamesService.AuthenticationTicketInfo ticket)
        {
            TicketInfoPanel.Visibility = Visibility.Visible;
            JoinScriptUrlTextBox.Text = ticket.JoinScriptUrl;
            AuthTicketTextBox.Text = ticket.TicketToken;
            AuthUrlTextBox.Text = ticket.AuthenticationUrl;
            TicketExpirationText.Text = ticket.ExpiresAt.ToString("yyyy-MM-dd HH:mm:ss");
            var commandLine = $"--authenticationUrl \"{ticket.AuthenticationUrl}\" --authenticationTicket \"{ticket.TicketToken}\" --joinScriptUrl \"{ticket.JoinScriptUrl}\"";
            CommandLineArgsTextBox.Text = commandLine;
        }

        /// <summary>
        /// Copy join script URL to clipboard
        /// </summary>
        private void CopyJoinScriptButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(JoinScriptUrlTextBox.Text);
                UpdateStatus("Join Script URL copied to clipboard", "success");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Failed to copy to clipboard: {ex.Message}", "error");
            }
        }

        /// <summary>
        /// Copy authentication ticket to clipboard
        /// </summary>
        private void CopyAuthTicketButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(AuthTicketTextBox.Text);
                UpdateStatus("Authentication Ticket copied to clipboard", "success");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Failed to copy to clipboard: {ex.Message}", "error");
            }
        }

        /// <summary>
        /// Copy authentication URL to clipboard
        /// </summary>
        private void CopyAuthUrlButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(AuthUrlTextBox.Text);
                UpdateStatus("Authentication URL copied to clipboard", "success");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Failed to copy to clipboard: {ex.Message}", "error");
            }
        }

        /// <summary>
        /// Copy command line arguments to clipboard
        /// </summary>
        private void CopyCommandLineButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(CommandLineArgsTextBox.Text);
                UpdateStatus("Command line arguments copied to clipboard", "success");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Failed to copy to clipboard: {ex.Message}", "error");
            }
        }

        #endregion
    }


    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status.ToLower() switch
                {
                    "running" => new SolidColorBrush(Colors.Green),
                    "starting" => new SolidColorBrush(Colors.Orange),
                    "expired" => new SolidColorBrush(Colors.Red),
                    "stopped" => new SolidColorBrush(Colors.Gray),
                    _ => new SolidColorBrush(Colors.Black)
                };
            }
            return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AutoKillToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool autoKillEnabled)
            {
                return autoKillEnabled ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AutoKillToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool autoKillEnabled)
            {
                return autoKillEnabled ? "ON" : "OFF";
            }
            return "?";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
