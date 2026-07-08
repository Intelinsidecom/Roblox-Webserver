using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using Control_Panel.Properties;
using ControlPanel.Functions;
using Games;

namespace Control_Panel
{
    /// <summary>
    /// Interaction logic for Games.xaml
    /// </summary>
    /// <summary>
    /// Represents a game card item for display in the Games tab
    /// </summary>
    public class GameCardItem
    {
        public long UniverseId { get; set; }
        public long PlaceId { get; set; }
        public string Name { get; set; }
        public string SecondaryText { get; set; }
        public string Creator { get; set; }
        public long CreatorUserId { get; set; }
        public string ThumbnailUrl { get; set; }
        public int PlayerCount { get; set; }
        public double UpVotePercent { get; set; }
        public int VisitCount { get; set; }

        public double UpVoteBarWidth => Math.Max(0, Math.Min(149, 149 * (UpVotePercent / 100.0)));
    }

    public partial class GamesView : UserControl
    {
        private readonly GamesService _gamesService;
        private ObservableCollection<GamesService.GameServerInfo> _servers;
        private bool _isInitialized = false;
        private DispatcherTimer _autoRefreshTimer;
        private GamesService.AuthenticationTicketInfo? _currentTicket;
        private readonly string _connectionString;
        private readonly ObservableCollection<GameCardItem> _gameItems;
        private int _sortFilter = 1;
        private int _timeFilter = 0;
        private int _genreFilter = 1;
        private string _searchKeyword = string.Empty;
        private CancellationTokenSource _searchCancellationTokenSource;
        private System.Timers.Timer _searchDebounceTimer;
        private const int SearchDebounceMs = 300;
        private const int GamePageSize = 20;
        private int _gameOffset = 0;
        private bool _hasMoreGames = true;
        private bool _isLoadingMore = false;

        public GamesView()
        {
            InitializeComponent();

            _connectionString = GetConnectionString();
            _gameItems = new ObservableCollection<GameCardItem>();
            _searchCancellationTokenSource = new CancellationTokenSource();
            GameCardsItemsControl.ItemsSource = _gameItems;

            _gamesService = new GamesService();
            _servers = new ObservableCollection<GamesService.GameServerInfo>();
            ServersDataGrid.ItemsSource = _servers;
            CreateServerPanel.Visibility = Visibility.Visible;
            ServerDetailsHeader.Visibility = Visibility.Collapsed;
            ServerInfoPanel.Visibility = Visibility.Collapsed;
            TimingInfoPanel.Visibility = Visibility.Collapsed;
            AuthenticationPanel.Visibility = Visibility.Collapsed;
            ServerActionButtons.Visibility = Visibility.Collapsed;
            _ = LoadGamesAsync();
            _ = LoadServersAsync();
            InitializeAutoRefreshTimer();
            ServersDataGrid.SelectedItem = null;
            this.Unloaded += GamesView_Unloaded;
            InitializePlaceholders();
        }

        private string GetConnectionString()
        {
            var connectionString = Properties.Settings.Default.DatabaseConnectionString;
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("Database connection string is not configured in application settings.");
            return connectionString;
        }

        private async Task LoadGamesAsync(bool reset = true)
        {
            if (_isLoadingMore) return;

            try
            {
                _isLoadingMore = true;

                if (reset)
                {
                    _searchCancellationTokenSource?.Cancel();
                    _searchCancellationTokenSource = new CancellationTokenSource();
                    _gameOffset = 0;
                    _hasMoreGames = true;
                }
                var token = _searchCancellationTokenSource.Token;

                List<GamesQueries.GameEntry> results;
                if (!string.IsNullOrWhiteSpace(_searchKeyword))
                {
                    results = await GamesQueries.SearchPublicGamesAsync(
                        _searchKeyword, _gameOffset, GamePageSize + 1, _connectionString, token);
                }
                else
                {
                    results = await GamesQueries.GetPublicGamesAsync(
                        _sortFilter, _timeFilter, _genreFilter, 183, _gameOffset, GamePageSize + 1, _connectionString, token);
                }

                Dispatcher.Invoke(() =>
                {
                    if (reset)
                        _gameItems.Clear();

                    var count = 0;
                    foreach (var game in results)
                    {
                        count++;
                        if (count > GamePageSize)
                        {
                            _hasMoreGames = true;
                            continue;
                        }
                        _gameItems.Add(new GameCardItem
                        {
                            UniverseId = game.UniverseId,
                            PlaceId = game.PlaceId,
                            Name = game.Name,
                            SecondaryText = $"{game.VisitCount:N0} visits",
                            Creator = game.CreatorName,
                            CreatorUserId = game.CreatorUserId,
                            ThumbnailUrl = game.ThumbnailUrl,
                            PlayerCount = game.Playing,
                            UpVotePercent = game.VotePercentage,
                            VisitCount = game.VisitCount
                        });
                    }
                    _hasMoreGames = count > GamePageSize;
                    _gameOffset = _gameItems.Count;
                });
            }
            catch (OperationCanceledException)
            {
                // Search was cancelled, ignore
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    UpdateStatus($"Failed to load games: {ex.Message}", "error");
                });
            }
            finally
            {
                _isLoadingMore = false;
            }
        }

        private void GameCard_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is GameCardItem game)
            {
                var placeWindow = new PlaceSelectWindow(game.UniverseId, game.Name);
                placeWindow.Owner = Window.GetWindow(this);
                placeWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                placeWindow.Show();
            }
        }

        private void CreatorLink_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is long userId)
            {
                if (userId > 0)
                {
                    Views.UserManagementWindow.OpenUserManagement((int)userId);
                }
            }
        }

        private async void GameSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_searchDebounceTimer != null)
            {
                _searchDebounceTimer.Stop();
                _searchDebounceTimer.Dispose();
            }

            if (sender is TextBox textBox)
            {
                if (textBox.Text == textBox.Tag as string)
                {
                    _searchKeyword = string.Empty;
                    await LoadGamesAsync();
                    return;
                }

                _searchDebounceTimer = new System.Timers.Timer(SearchDebounceMs);
                _searchDebounceTimer.AutoReset = false;
                _searchDebounceTimer.Elapsed += async (s, args) =>
                {
                    Dispatcher.Invoke(async () =>
                    {
                        _searchKeyword = textBox.Text;
                        await LoadGamesAsync();
                    });
                };
                _searchDebounceTimer.Start();
            }
        }

        private void GameIdSearchTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                string gameIdText = GameIdSearchTextBox.Text.Trim();
                if (!string.IsNullOrWhiteSpace(gameIdText) && long.TryParse(gameIdText, out long universeId) && universeId > 0)
                {
                    try
                    {
                        var placeWindow = new PlaceSelectWindow(universeId, $"Universe {universeId}");
                        placeWindow.Owner = Window.GetWindow(this);
                        placeWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        placeWindow.Show();
                        GameIdSearchTextBox.Clear();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to open universe {universeId}: {ex.Message}", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void GameTypeFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                _genreFilter = selectedItem.Content.ToString() switch
                {
                    "Town & City" => 2,
                    "Fantasy" => 3,
                    "Sci-Fi" => 4,
                    "Ninja" => 5,
                    "Scary" => 6,
                    "Pirate" => 7,
                    "Adventure" => 8,
                    "Sports" => 9,
                    "Funny" => 10,
                    "Wild West" => 11,
                    "War" => 12,
                    "Skate Park" => 13,
                    "Tutorial" => 14,
                    "RPG" => 15,
                    "FPS" => 16,
                    "Fighting" => 17,
                    "Building" => 18,
                    "Military" => 19,
                    "Naval" => 20,
                    "Medieval" => 21,
                    _ => 1
                };
                await LoadGamesAsync();
            }
        }

        private async void GameDateFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                _timeFilter = selectedItem.Content.ToString() switch
                {
                    "Today" => 1,
                    "This Week" => 2,
                    "This Month" => 3,
                    "This Year" => 4,
                    _ => 0
                };
                await LoadGamesAsync();
            }
        }

        private async void GameSortFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                _sortFilter = selectedItem.Content.ToString() switch
                {
                    "The Oldest" => 16,
                    "Name A-Z" => 3,
                    "Name Z-A" => 9,
                    "Most Players" => 11,
                    _ => 1
                };
                await LoadGamesAsync();
            }
        }

        private void GamesScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (GamesTab.Visibility != Visibility.Visible) return;
            if (_isLoadingMore || !_hasMoreGames) return;

            var scrollViewer = (ScrollViewer)sender;
            if (scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight - 200)
            {
                _ = LoadGamesAsync(false);
            }
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
                var updatedServers = servers.ToList();
                
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
                await LoadGamesAsync();
                UpdateStatus("Refreshed successfully", "success");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Failed to refresh: {ex.Message}", "error");
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

        private void GameServersTabButton_Click(object sender, RoutedEventArgs e)
        {
            GameServersTab.Visibility = Visibility.Visible;
            GamesTab.Visibility = Visibility.Collapsed;
            GameServersTabButton.Background = (System.Windows.Media.Brush)Application.Current.Resources["AccentPrimary"];
            GameServersTabButton.Foreground = System.Windows.Media.Brushes.White;
            GamesTabButton.Background = System.Windows.Media.Brushes.Transparent;
            GamesTabButton.Foreground = (System.Windows.Media.Brush)Application.Current.Resources["Foreground"];
        }

        private void GamesTabButton_Click(object sender, RoutedEventArgs e)
        {
            GameServersTab.Visibility = Visibility.Collapsed;
            GamesTab.Visibility = Visibility.Visible;
            GamesTabButton.Background = (System.Windows.Media.Brush)Application.Current.Resources["AccentPrimary"];
            GamesTabButton.Foreground = System.Windows.Media.Brushes.White;
            GameServersTabButton.Background = System.Windows.Media.Brushes.Transparent;
            GameServersTabButton.Foreground = (System.Windows.Media.Brush)Application.Current.Resources["Foreground"];
        }

        private void InitializePlaceholders()
        {
            SetPlaceholder(GameSearchTextBox);
            SetPlaceholder(GameIdSearchTextBox);
        }

        private void SetPlaceholder(TextBox textBox)
        {
            if (string.IsNullOrEmpty(textBox.Text))
            {
                textBox.Text = textBox.Tag as string;
                textBox.Foreground = (Brush)FindResource("SubtleText");
            }
        }

        private void ClearPlaceholder(TextBox textBox)
        {
            if (textBox.Text == textBox.Tag as string)
            {
                textBox.Text = "";
                textBox.Foreground = (Brush)FindResource("Foreground");
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                ClearPlaceholder(textBox);
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                if (string.IsNullOrEmpty(textBox.Text))
                {
                    SetPlaceholder(textBox);
                }
            }
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
