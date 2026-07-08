using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Assets;
using Games;
using Users;
using Control_Panel.Properties;
using Control_Panel.Functions;
using ControlPanel.Functions;

namespace Control_Panel
{
    public partial class PlaceManagementWindow : Window
    {
        private long _placeId;
        private long _universeId;
        private long _rootPlaceId;
        private readonly AssetMetadataRepository _metadataRepo;
        private AssetRecord _currentPlace;
        private string _creatorName;
        private bool _isHighResThumbnail = false;
        private string _connectionString;

        public PlaceManagementWindow()
        {
            InitializeComponent();
            ThemeManager.InitializeThemeForWindow(this);
            Settings.Default.PropertyChanged += Settings_PropertyChanged;
            _connectionString = GetConnectionString();
            _metadataRepo = new AssetMetadataRepository();
        }

        public PlaceManagementWindow(long placeId) : this()
        {
            _placeId = placeId;
            Loaded += async (sender, e) => await LoadPlaceDataAsync();
            Closing += (sender, e) => ClearPlaceData();
        }

        private async Task LoadPlaceDataAsync()
        {
            try
            {
                SetLoadingState(true);
                _currentPlace = await _metadataRepo.GetPlaceByIdAsync(_connectionString, _placeId);

                if (_currentPlace == null)
                {
                    MessageBox.Show($"Place with ID {_placeId} not found.", "Place Not Found",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    this.Close();
                    return;
                }

                var universeId = await GamesRepository.GetUniverseIdFromPlaceIdAsync(_connectionString, _placeId);
                if (universeId.HasValue)
                {
                    _universeId = universeId.Value;
                    _rootPlaceId = await GamesQueries.GetFirstPlaceIdFromUniverseAsync(_universeId, _connectionString);
                }

                UpdateUIWithPlaceData();
                await LoadThumbnailImageAsync();
                await LoadAdditionalDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading place data: {ex.Message}", "Error",
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
                LoadUniverseDataAsync(),
                LoadCreatorNameAsync()
            );
        }

        private async Task LoadVersionHistoryAsync()
        {
            try
            {
                var versions = await VersionHistory.GetAssetVersionHistoryAsync(_connectionString, _placeId);
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
                VersionHistoryStatus.Text = "Error loading version history";
                System.Diagnostics.Debug.WriteLine($"Error loading version history: {ex.Message}");
            }
        }

        private async Task LoadUniverseDataAsync()
        {
            try
            {
                if (_universeId <= 0) return;

                var stats = await GamesRepository.GetUniverseStatsAsync(_connectionString, _universeId);
                if (stats != null)
                {
                    VisitCountText.Text = stats.VisitCount.ToString("N0");
                    UpdatedText.Text = stats.LastUpdated?.ToString("MMM dd, yyyy") ?? "Unknown";
                    CreatedText.Text = stats.CreatedAt?.ToString("MMM dd, yyyy") ?? "Unknown";
                    var total = stats.Upvotes + stats.Downvotes;
                    LikeRatioText.Text = total > 0
                        ? $"{stats.Upvotes * 100.0 / total:F1}% ({stats.Upvotes} up, {stats.Downvotes} down)"
                        : "No votes";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading universe data: {ex.Message}");
            }
        }

        private async Task LoadCreatorNameAsync()
        {
            try
            {
                if (_currentPlace == null || _currentPlace.OwnerUserId <= 0) return;

                _creatorName = await Users.UserQueries.GetUserNameByIdAsync(_connectionString, _currentPlace.OwnerUserId) ?? "Unknown";
                CreatorText.Text = _creatorName;
            }
            catch
            {
                _creatorName = "Unknown";
            }
        }

        private void ClearPlaceData()
        {
            _currentPlace = null;
            PlaceThumbnailImage.Source = null;
            AssetIdText.Text = "";
            PlaceNameText.Text = "";
            PlaceDescriptionTextBox.Text = "";
            UniverseIdText.Text = "";
            RootPlaceText.Text = "";
            CreatorText.Text = "";
            UpdatedText.Text = "";
            GenreText.Text = "";
            AllowCommentsText.Text = "";
            MaxPlayersText.Text = "";
            AccessText.Text = "";
            PrivateServersText.Text = "";
            PaidAccessText.Text = "";
            CopyingText.Text = "";
            DeviceText.Text = "";
            ServerFillText.Text = "";
            LikeRatioText.Text = "";
            VisitCountText.Text = "";
            CreatedText.Text = "";
            ViewThumbnailsButton.IsEnabled = false;
            VersionHistoryListView.ItemsSource = null;
            VersionHistoryStatus.Text = "No version history loaded";
            RevertVersionButton.IsEnabled = false;
        }

        private void UpdateUIWithPlaceData()
        {
            if (_currentPlace == null) return;

            AssetIdText.Text = _currentPlace.AssetId.ToString();
            PlaceNameText.Text = _currentPlace.Name ?? "Unknown";
            PlaceDescriptionTextBox.Text = _currentPlace.Description ?? "";
            UniverseIdText.Text = _universeId > 0 ? _universeId.ToString() : "N/A";
            RootPlaceText.Text = (_universeId > 0 && _placeId == _rootPlaceId) ? "Yes" : "No";
            CreatorText.Text = _currentPlace.OwnerUserId.ToString();
            GenreText.Text = AssetGenreNames.GetGenreLabel(_currentPlace.Genre);
            AllowCommentsText.Text = _currentPlace.AllowComments ? "Yes" : "No";
            MaxPlayersText.Text = _currentPlace.MaxVisitorCount.ToString();
            AccessText.Text = GetAccessTypeText(_currentPlace.AccessType);
            PrivateServersText.Text = GetPrivateServersText(
                _currentPlace.PrivateServersAllowed,
                _currentPlace.IsPrivateServersFree,
                _currentPlace.PrivateServersPrice);
            PaidAccessText.Text = GetPaidAccessText(
                _currentPlace.PaidAccessEnabled,
                _currentPlace.PaidAccessPrice);
            CopyingText.Text = _currentPlace.IsCopyingAllowed ? "Allowed" : "Not Allowed";
            DeviceText.Text = GetDeviceCompatibilityText(_currentPlace.DeviceCompatibility);
            ServerFillText.Text = GetServerFillTypeText(_currentPlace.ServerFillType);
            Title = $"Place Management - ID: {_currentPlace.AssetId}";
            ViewCreatorButton.IsEnabled = _currentPlace.OwnerUserId > 0;
            EditPlaceButton.IsEnabled = _currentPlace.OwnerUserId > 0;
            ViewThumbnailsButton.IsEnabled = _universeId > 0;
        }

        private void SetLoadingState(bool isLoading)
        {
            if (isLoading)
            {
                AssetIdText.Text = "Loading...";
                PlaceNameText.Text = "Loading...";
                PlaceDescriptionTextBox.Text = "Loading...";
                UniverseIdText.Text = "Loading...";
                RootPlaceText.Text = "Loading...";
                CreatorText.Text = "Loading...";
                UpdatedText.Text = "Loading...";
                GenreText.Text = "Loading...";
                AllowCommentsText.Text = "Loading...";
                MaxPlayersText.Text = "Loading...";
                AccessText.Text = "Loading...";
                PrivateServersText.Text = "Loading...";
                PaidAccessText.Text = "Loading...";
                CopyingText.Text = "Loading...";
                DeviceText.Text = "Loading...";
                ServerFillText.Text = "Loading...";
                LikeRatioText.Text = "Loading...";
                VisitCountText.Text = "Loading...";
                CreatedText.Text = "Loading...";
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
                    if (!string.IsNullOrWhiteSpace(_currentPlace?.PlaceGeneratedIconHighResUrl))
                        thumbnailUrl = _currentPlace.PlaceGeneratedIconHighResUrl;
                    else if (!string.IsNullOrWhiteSpace(_currentPlace?.PlaceCustomIconHighResUrl))
                        thumbnailUrl = _currentPlace.PlaceCustomIconHighResUrl;
                    else if (!string.IsNullOrWhiteSpace(_currentPlace?.HighResThumbnailUrl))
                        thumbnailUrl = _currentPlace.HighResThumbnailUrl;
                }

                if (string.IsNullOrEmpty(thumbnailUrl))
                    thumbnailUrl = _currentPlace?.ThumbnailUrl ?? "";

                if (!string.IsNullOrEmpty(thumbnailUrl))
                {
                    try
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(thumbnailUrl);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        PlaceThumbnailImage.Source = bitmap;
                    }
                    catch
                    {
                        ShowWebsiteNotActiveMessage();
                    }
                }
                else
                {
                    ShowCdnNotActiveMessage();
                }
            }
            catch
            {
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
                FontWeight = FontWeights.Medium
            };
            var border = (Border)PlaceThumbnailImage.Parent;
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
                FontWeight = FontWeights.Medium
            };
            var border = (Border)PlaceThumbnailImage.Parent;
            border.Child = textBlock;
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadPlaceDataAsync();
        }

        private async void ThumbnailToggleButton_Click(object sender, RoutedEventArgs e)
        {
            _isHighResThumbnail = !_isHighResThumbnail;
            ThumbnailToggleButton.Content = _isHighResThumbnail ? "High-Res" : "Low-Res";
            await LoadThumbnailImageAsync();
        }

        private async void ViewCreatorButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPlace == null || _currentPlace.OwnerUserId <= 0) return;

            Views.UserManagementWindow.OpenUserManagement((int)_currentPlace.OwnerUserId);
        }

        private async void EditPlaceButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPlace == null) return;

            var editWindow = new PlaceEditWindow(_currentPlace.AssetId);
            editWindow.Owner = this;
            editWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            editWindow.ShowDialog();
        }

        private void ViewThumbnailsButton_Click(object sender, RoutedEventArgs e)
        {
            if (_universeId <= 0) return;

            var carousel = new ThumbnailCarousel(_universeId, _connectionString)
            {
                Owner = this
            };
            carousel.Show();
        }

        private void VersionHistoryListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RevertVersionButton.IsEnabled = VersionHistoryListView.SelectedItem != null;
        }

        private async void RevertVersionButton_Click(object sender, RoutedEventArgs e)
        {
            if (VersionHistoryListView.SelectedItem is PlaceVersionEntry selectedVersion)
            {
                var result = MessageBox.Show(
                    $"Revert place to version {selectedVersion.Version} from {selectedVersion.Date}?\n\nHash: {selectedVersion.File_Hash}",
                    "Confirm Version Revert", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes) return;

                try
                {
                    RevertVersionButton.IsEnabled = false;
                    VersionHistoryStatus.Text = "Reverting...";

                    bool success = await VersionHistory.RevertToVersionAsync(_connectionString, _placeId, selectedVersion.Version);

                    if (success)
                    {
                        MessageBox.Show($"Place reverted to version {selectedVersion.Version} successfully.", "Revert Successful",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadVersionHistoryAsync();
                    }
                    else
                    {
                        MessageBox.Show("Failed to revert place to selected version.", "Revert Failed",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error reverting place: {ex.Message}", "Error",
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

        private string GetConnectionString()
        {
            var connectionString = Properties.Settings.Default.DatabaseConnectionString;
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("Database connection string is not configured in application settings.");
            return connectionString;
        }

        private string GetAccessTypeText(int accessType)
        {
            return accessType switch
            {
                1 => "Everyone (Public)",
                2 => "Friends Only",
                _ => "Unknown"
            };
        }

        private string GetPrivateServersText(bool allowed, bool isFree, int price)
        {
            if (!allowed) return "Not Allowed";
            if (isFree) return "Allowed (Free)";
            return $"Allowed ({price} Robux)";
        }

        private string GetPaidAccessText(bool enabled, int price)
        {
            if (!enabled) return "Disabled";
            return price > 0 ? $"Enabled ({price} Robux)" : "Enabled (Free)";
        }

        private string GetDeviceCompatibilityText(string deviceJson)
        {
            if (string.IsNullOrWhiteSpace(deviceJson) || deviceJson == "[]")
                return "None";

            var devices = new List<string>();
            try
            {
                var ids = System.Text.Json.JsonSerializer.Deserialize<int[]>(deviceJson);
                if (ids == null) return "Unknown";
                foreach (var id in ids)
                {
                    devices.Add(id switch
                    {
                        1 => "Computer",
                        2 => "Tablet",
                        3 => "Phone",
                        4 => "Console",
                        _ => $"Unknown ({id})"
                    });
                }
            }
            catch
            {
                return "Unknown";
            }

            return string.Join(", ", devices);
        }

        private string GetServerFillTypeText(int fillType)
        {
            return fillType switch
            {
                0 => "Standard",
                1 => "Prefer Friends",
                _ => $"Unknown ({fillType})"
            };
        }

        public void LoadPlace(long placeId)
        {
            _placeId = placeId;
            _ = LoadPlaceDataAsync();
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Theme" || e.PropertyName == "ColorScheme" || e.PropertyName == "BackgroundColor")
            {
                ThemeManager.InitializeThemeForWindow(this);
            }
        }
    }
}
