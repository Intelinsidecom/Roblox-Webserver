using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Control_Panel.Functions;
using ControlPanel.Functions;
using Assets;
using Control_Panel.Properties;
using Games;

namespace Control_Panel
{
    public partial class AssetManagementWindow : Window
    {
        private long _assetId;
        private readonly Control_Panel.Functions.AssetService _assetService;
        private readonly AssetsRepository _assetsRepository;
        private AssetSearchResult _currentAsset;
        private readonly UserSearchService _userSearchService;
        private bool _isHighResThumbnail = false;
        private string _connectionString;
        private System.Windows.Media.MediaPlayer _mediaPlayer;
        private bool _isAudioPlaying = false;

        public AssetManagementWindow()
        {
            InitializeComponent();
            ThemeManager.InitializeThemeForWindow(this);
            Settings.Default.PropertyChanged += Settings_PropertyChanged;
            _connectionString = GetConnectionString();
            _assetService = new Control_Panel.Functions.AssetService();
            _assetsRepository = new AssetsRepository();
            _userSearchService = new UserSearchService(_connectionString);
        }

        public AssetManagementWindow(long assetId) : this()
        {
            _assetId = assetId;
            Loaded += async (sender, e) => await LoadAssetDataAsync();
            Closing += (sender, e) => ClearAssetData();
        }

        private async Task LoadAssetDataAsync()
        {
            try
            {
                SetLoadingState(true);
                _currentAsset = await _assetService.GetAssetByIdAsync(_assetId);

                if (_currentAsset == null)
                {
                    MessageBox.Show($"Asset with ID {_assetId} not found.", "Asset Not Found",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    this.Close();
                    return;
                }

                if (_currentAsset.AssetTypeId == 9)
                {
                    MessageBox.Show("Places cannot be opened in Asset Management.",
                        "Unsupported Asset Type", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                    return;
                }

                UpdateUIWithAssetData();
                await LoadThumbnailImageAsync();
                await LoadAdditionalDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading asset data: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        private async Task LoadAdditionalDataAsync()
        {
            await Task.WhenAll(
                LoadVersionHistoryAsync(),
                LoadAssetSettingsAsync()
            );
        }

        private async Task LoadVersionHistoryAsync()
        {
            try
            {
                var versions = await _assetService.GetAssetVersionHistoryAsync(_connectionString, _assetId);
                VersionHistoryListView.ItemsSource = versions;

                if (versions == null || versions.Count == 0)
                {
                    VersionHistoryStatus.Text = "No version history available";
                    RevertVersionButton.IsEnabled = false;
                }
                else
                {
                    VersionHistoryStatus.Text = $"{versions.Count} version(s) found";
                    RevertVersionButton.IsEnabled = VersionHistoryListView.SelectedItem != null;
                }
            }
            catch (Exception ex)
            {
                VersionHistoryStatus.Text = $"Error loading version history";
                System.Diagnostics.Debug.WriteLine($"Error loading version history: {ex.Message}");
            }
        }

        private async Task LoadAssetSettingsAsync()
        {
            try
            {
                var settingsTask = _assetService.GetAssetSettingsAsync(_connectionString, _assetId);
                var favoritesTask = _assetService.GetFavoriteCountAsync(_connectionString, _assetId);

                await Task.WhenAll(settingsTask, favoritesTask);

                var settings = await settingsTask;
                AllowCommentsText.Text = settings.allowComments ? "Yes" : "No";
                AllowCopyingText.Text = settings.isCopyingAllowed ? "Yes" : "No";
                GenreText.Text = AssetGenreNames.GetGenreLabel(settings.genre);
                FavoritesCountText.Text = (await favoritesTask).ToString("N0");
            }
            catch (Exception ex)
            {
                AllowCommentsText.Text = "Error";
                AllowCopyingText.Text = "Error";
                GenreText.Text = "Error";
                FavoritesCountText.Text = "Error";
                System.Diagnostics.Debug.WriteLine($"Error loading asset settings: {ex.Message}");
            }
        }

        private void ClearAssetData()
        {
            ResetAudioPlayback();
            _currentAsset = null;
            AssetThumbnailImage.Source = null;
            AudioPlayButtonBorder.Visibility = Visibility.Collapsed;
            AssetIdText.Text = "";
            AssetNameText.Text = "";
            AssetDescriptionTextBox.Text = "";
            AssetTypeText.Text = "";
            CreatorText.Text = "";
            UpdatedText.Text = "";
            RobuxPriceText.Text = "";
            TixPriceText.Text = "";
            OnSaleText.Text = "";
            AllowCommentsText.Text = "";
            AllowCopyingText.Text = "";
            GenreText.Text = "";
            FavoritesCountText.Text = "-";
            VersionHistoryListView.ItemsSource = null;
            VersionHistoryStatus.Text = "No version history loaded";
            RevertVersionButton.IsEnabled = false;
        }

        private void UpdateUIWithAssetData()
        {
            if (_currentAsset == null) return;

            AssetIdText.Text = _currentAsset.Id.ToString();
            AssetNameText.Text = _currentAsset.Name ?? "Unknown";
            AssetDescriptionTextBox.Text = _currentAsset.Description ?? "";
            AssetTypeText.Text = _currentAsset.Type ?? "Unknown";
            CreatorText.Text = _currentAsset.Creator ?? "Unknown";
            UpdatedText.Text = _currentAsset.UpdatedTime ?? "Unknown";
            RobuxPriceText.Text = _currentAsset.Price ?? "Not for sale";
            TixPriceText.Text = _currentAsset.TixPrice ?? "Not for sale";
            OnSaleText.Text = _currentAsset.PutOnSale.ToString();
            Title = $"Asset Management - ID: {_currentAsset.Id}";

            AudioPlayButtonBorder.Visibility = _currentAsset.AssetTypeId == 3
                ? Visibility.Visible
                : Visibility.Collapsed;
            ResetAudioPlayback();
        }

        private void SetLoadingState(bool isLoading)
        {
            if (isLoading)
            {
                AssetIdText.Text = "Loading...";
                AssetNameText.Text = "Loading...";
                AssetDescriptionTextBox.Text = "Loading...";
                AssetTypeText.Text = "Loading...";
                CreatorText.Text = "Loading...";
                UpdatedText.Text = "Loading...";
                RobuxPriceText.Text = "Loading...";
                TixPriceText.Text = "Loading...";
                OnSaleText.Text = "Loading...";
                AllowCommentsText.Text = "Loading...";
                AllowCopyingText.Text = "Loading...";
                GenreText.Text = "Loading...";
                FavoritesCountText.Text = "Loading...";
                VersionHistoryStatus.Text = "Loading...";
            }
        }

        private async Task LoadThumbnailImageAsync()
        {
            try
            {
                string thumbnailUrl = "";

                if (_isHighResThumbnail)
                {
                    if (_currentAsset?.HighResThumbnailUrl != null && !string.IsNullOrWhiteSpace(_currentAsset.HighResThumbnailUrl))
                    {
                        thumbnailUrl = _currentAsset.HighResThumbnailUrl;
                    }
                    else
                    {
                        if (_currentAsset?.ThumbnailUrl != null && !string.IsNullOrWhiteSpace(_currentAsset.ThumbnailUrl))
                        {
                            thumbnailUrl = _currentAsset.ThumbnailUrl;
                        }
                        else
                        {
                            thumbnailUrl = Properties.Settings.Default.DefaultThumbnailUrl ?? string.Empty;
                        }
                    }
                }
                else
                {
                    if (_currentAsset?.ThumbnailUrl != null && !string.IsNullOrWhiteSpace(_currentAsset.ThumbnailUrl))
                    {
                        thumbnailUrl = _currentAsset.ThumbnailUrl;
                    }
                    else
                    {
                        thumbnailUrl = Properties.Settings.Default.DefaultThumbnailUrl ?? string.Empty;
                    }
                }

                if (!string.IsNullOrEmpty(thumbnailUrl))
                {
                    try
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(thumbnailUrl);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        AssetThumbnailImage.Source = bitmap;
                    }
                    catch (Exception imgEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to load image from URL, trying default thumbnail: {imgEx.Message}");
                        var defaultUrl = Properties.Settings.Default.DefaultThumbnailUrl ?? string.Empty;
                        if (!string.IsNullOrEmpty(defaultUrl) && defaultUrl != thumbnailUrl)
                        {
                            try
                            {
                                var fallbackBitmap = new BitmapImage();
                                fallbackBitmap.BeginInit();
                                fallbackBitmap.UriSource = new Uri(defaultUrl);
                                fallbackBitmap.CacheOption = BitmapCacheOption.OnLoad;
                                fallbackBitmap.EndInit();
                                AssetThumbnailImage.Source = fallbackBitmap;
                            }
                            catch
                            {
                                ShowWebsiteNotActiveMessage();
                            }
                        }
                        else
                        {
                            ShowWebsiteNotActiveMessage();
                        }
                    }
                }
                else
                {
                    ShowCdnNotActiveMessage();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load asset thumbnail: {ex.Message}");
                ShowWebsiteNotActiveMessage();
            }
        }

        private void ShowCdnNotActiveMessage()
        {
            var textBlock = new TextBlock
            {
                Text = "CDN is not Active",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = System.Windows.Media.Brushes.Gray,
                FontSize = 14,
                FontWeight = System.Windows.FontWeights.Medium
            };

            var border = (System.Windows.Controls.Border)AssetThumbnailImage.Parent;
            border.Child = textBlock;
        }

        private void ShowWebsiteNotActiveMessage()
        {
            var textBlock = new TextBlock
            {
                Text = "Website and CDN is not Active",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = System.Windows.Media.Brushes.Red,
                FontSize = 14,
                FontWeight = System.Windows.FontWeights.Medium
            };

            var border = (System.Windows.Controls.Border)AssetThumbnailImage.Parent;
            border.Child = textBlock;
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadAssetDataAsync();
        }

        private void EditAssetButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentAsset == null) return;

            try
            {
                bool wasSaved = AssetEditWindow.EditAsset(_currentAsset.Id);

                if (wasSaved)
                {
                    _ = LoadAssetDataAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening asset edit window: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteAssetButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentAsset == null) return;

            var result = MessageBox.Show($"Are you sure you want to delete asset '{_currentAsset.Name}' (ID: {_currentAsset.Id})? " +
                "This action cannot be undone.", "Confirm Asset Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            MessageBox.Show("Asset deletion functionality to be implemented.", "Coming Soon",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void ViewCreatorButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentAsset == null) return;

            try
            {
                var searchResults = await _userSearchService.SearchUsersByUsernameAsync(_currentAsset.Creator, 1);

                if (searchResults.Count > 0)
                {
                    var userId = (int)searchResults[0].Id;
                    Views.UserManagementWindow.OpenUserManagement(userId);
                }
                else
                {
                    MessageBox.Show($"User '{_currentAsset.Creator}' not found in the database.", "User Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening user management: {ex.Message}", "Navigation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ThumbnailToggleButton_Click(object sender, RoutedEventArgs e)
        {
            _isHighResThumbnail = !_isHighResThumbnail;
            ThumbnailToggleButton.Content = _isHighResThumbnail ? "High-Res" : "Thumbnail";
            _ = LoadThumbnailImageAsync();
        }

        private async void RevertVersionButton_Click(object sender, RoutedEventArgs e)
        {
            if (VersionHistoryListView.SelectedItem is PlaceVersionEntry selectedVersion)
            {
                var result = MessageBox.Show(
                    $"Revert asset to version {selectedVersion.Version} from {selectedVersion.Date}?\n\nHash: {selectedVersion.File_Hash}",
                    "Confirm Version Revert", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                    return;

                try
                {
                    RevertVersionButton.IsEnabled = false;
                    VersionHistoryStatus.Text = "Reverting...";

                    bool success = await _assetService.RevertAssetToVersionAsync(_connectionString, _assetId, selectedVersion.Version);

                    if (success)
                    {
                        MessageBox.Show($"Asset reverted to version {selectedVersion.Version} successfully.", "Revert Successful",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadVersionHistoryAsync();
                    }
                    else
                    {
                        MessageBox.Show("Failed to revert asset to selected version.", "Revert Failed",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error reverting asset: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    RevertVersionButton.IsEnabled = VersionHistoryListView.SelectedItem != null;
                    if (VersionHistoryStatus.Text == "Reverting...")
                        VersionHistoryStatus.Text = "Revert failed";
                }
            }
        }

        private void VersionHistoryListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RevertVersionButton.IsEnabled = VersionHistoryListView.SelectedItem != null;
        }

        private string GetConnectionString()
        {
            var connectionString = Properties.Settings.Default.DatabaseConnectionString;

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Database connection string is not configured in application settings.");
            }

            return connectionString;
        }

        /// <summary>
        /// Loads a specific asset into the Asset Management view
        /// </summary>
        /// <param name="assetId">The ID of the asset to load</param>
        public void LoadAsset(long assetId)
        {
            _assetId = assetId;
            _ = LoadAssetDataAsync();
        }

        /// <summary>
        /// Handles settings property changes to apply theme immediately when settings are saved
        /// </summary>
        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Theme" || e.PropertyName == "ColorScheme" || e.PropertyName == "BackgroundColor")
            {
                ThemeManager.InitializeThemeForWindow(this);
            }
        }

        private void AudioPlayButton_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;

            try
            {
                if (_currentAsset == null || _currentAsset.AssetTypeId != 3) return;

                if (_isAudioPlaying && _mediaPlayer != null)
                {
                    _mediaPlayer.Pause();
                    _isAudioPlaying = false;
                    AudioPlayIcon.Text = "\u25B6";
                    AudioPlayButtonBorder.Background = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromArgb(128, 0, 0, 0));
                    return;
                }

                if (!_isAudioPlaying && _mediaPlayer != null)
                {
                    _mediaPlayer.Play();
                    _isAudioPlaying = true;
                    AudioPlayIcon.Text = "\u23F8";
                    AudioPlayButtonBorder.Background = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromArgb(204, 76, 175, 80));
                    return;
                }

                var host = Settings.Default.WebsiteHost ?? "localhost";
                var port = Settings.Default.WebsitePort ?? "5077";
                var url = $"http://{host}:{port}/asset/?id={_currentAsset.Id}";

                _mediaPlayer = new System.Windows.Media.MediaPlayer();
                _mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
                _mediaPlayer.Open(new Uri(url));
                _mediaPlayer.Play();
                _isAudioPlaying = true;
                AudioPlayIcon.Text = "\u23F8";
                AudioPlayButtonBorder.Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(204, 76, 175, 80));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error playing audio: {ex.Message}");
                ResetAudioPlayback();
            }
        }

        private void MediaPlayer_MediaEnded(object sender, EventArgs e)
        {
            Dispatcher.Invoke(ResetAudioPlayback);
        }

        private void ResetAudioPlayback()
        {
            _mediaPlayer?.Close();
            _mediaPlayer = null;
            _isAudioPlaying = false;
            AudioPlayIcon.Text = "\u25B6";
            AudioPlayButtonBorder.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromArgb(128, 0, 0, 0));
        }
    }
}
