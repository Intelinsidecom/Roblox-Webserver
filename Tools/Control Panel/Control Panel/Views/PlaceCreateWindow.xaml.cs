using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Control_Panel.Properties;
using ControlPanel.Functions;
using Games;
using Assets;
using Users;
using Thumbnails;
using Webserver.Common;
using Microsoft.Win32;

namespace Control_Panel
{
    public partial class PlaceCreateWindow : Window
    {
        private readonly string _connectionString;
        private string? _selectedPlaceFilePath;

        public PlaceCreateWindow()
        {
            InitializeComponent();
            ThemeManager.InitializeThemeForWindow(this);
            Settings.Default.PropertyChanged += Settings_PropertyChanged;
            _connectionString = GetConnectionString();

            for (int i = 1; i <= 100; i++)
            {
                MaxPlayersComboBox.Items.Add(new ComboBoxItem
                {
                    Content = i.ToString(),
                    IsSelected = i == 8
                });
            }

            SellGameAccessCheckBox.Checked += (s, e) => PricingPanel.IsEnabled = true;
            SellGameAccessCheckBox.Unchecked += (s, e) => PricingPanel.IsEnabled = false;
            AllowPrivateServersCheckBox.Checked += (s, e) => PrivateServerDetails.IsEnabled = true;
            AllowPrivateServersCheckBox.Unchecked += (s, e) => PrivateServerDetails.IsEnabled = false;
            PrivateServerPaidRadio.Checked += (s, e) => PrivateServerPricePanel.IsEnabled = true;
            PrivateServerFreeRadio.Checked += (s, e) => PrivateServerPricePanel.IsEnabled = false;
            PriceTextBox.TextChanged += (s, e) => UpdatePricing();
            PrivateServerPriceTextBox.TextChanged += (s, e) => UpdatePrivateServerPricing();
        }

        private string GetConnectionString()
        {
            var cs = Properties.Settings.Default.DatabaseConnectionString;
            if (string.IsNullOrEmpty(cs))
                throw new InvalidOperationException("Database connection string is not configured in application settings.");
            return cs;
        }

        private void Tab_Click(object sender, RoutedEventArgs e)
        {
            var tab = sender as RadioButton;
            if (tab == null) return;

            BasicSettingsContent.Visibility = Visibility.Collapsed;
            AccessContent.Visibility = Visibility.Collapsed;
            PermissionsContent.Visibility = Visibility.Collapsed;

            if (tab == BasicSettingsTab)
                BasicSettingsContent.Visibility = Visibility.Visible;
            else if (tab == AccessTab)
                AccessContent.Visibility = Visibility.Visible;
            else if (tab == PermissionsTab)
                PermissionsContent.Visibility = Visibility.Visible;
        }

        private void UpdatePricing()
        {
            if (int.TryParse(PriceTextBox.Text, out int price) && price > 0)
            {
                int fee = (int)Math.Round(price * 0.3);
                int profit = price - fee;
                MarketplaceFeeText.Text = fee.ToString();
                ProfitText.Text = profit.ToString();
            }
            else
            {
                MarketplaceFeeText.Text = "0";
                ProfitText.Text = "0";
            }
        }

        private void UpdatePrivateServerPricing()
        {
            if (int.TryParse(PrivateServerPriceTextBox.Text, out int price) && price > 0)
            {
                int fee = (int)Math.Round(price * 0.3);
                int profit = price - fee;
                PrivateServerFeeText.Text = fee.ToString();
                PrivateServerProfitText.Text = profit.ToString();
            }
            else
            {
                PrivateServerFeeText.Text = "0";
                PrivateServerProfitText.Text = "0";
            }
        }

        private void BrowsePlaceFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select Place File",
                Filter = "Roblox Place Files (*.rbxl)|*.rbxl|All Files (*.*)|*.*",
                FilterIndex = 1
            };

            if (dialog.ShowDialog() == true)
            {
                _selectedPlaceFilePath = dialog.FileName;
                PlaceFilePathText.Text = Path.GetFileName(dialog.FileName);
                PlaceFilePathText.Foreground = (System.Windows.Media.Brush)FindResource("Foreground");
            }
        }

        private async void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            var placeName = PlaceNameTextBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(placeName))
            {
                MessageBox.Show("Please enter a name for the place.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(_selectedPlaceFilePath) || !File.Exists(_selectedPlaceFilePath))
            {
                MessageBox.Show("Please select a valid .rbxl place file.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!long.TryParse(Settings.Default.DefaultOwnerUserId, out var creatorUserId) || creatorUserId <= 0)
            {
                MessageBox.Show("Default Owner User ID is not configured in settings.", "Configuration Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                CreateButton.IsEnabled = false;
                CreateButton.Content = "Creating...";

                var creatorUserName = await UserQueries.GetUserNameByIdAsync(_connectionString, creatorUserId)
                                     ?? "Player";

                var assetsRoot = Settings.Default.AssetsDirectory;
                var universeInfo = await GameCreationService.CreateUniverseWithRootPlaceAsync(
                    _connectionString,
                    creatorUserId,
                    creatorUserName,
                    assetsRoot: string.IsNullOrWhiteSpace(assetsRoot) ? null : assetsRoot,
                    starterPlacePath: _selectedPlaceFilePath,
                    enableCreationCooldown: false,
                    thumbnailService: null,
                    configuration: null,
                    cancellationToken: CancellationToken.None,
                    customName: placeName);

                if (universeInfo == null || universeInfo.RootPlaceId <= 0)
                {
                    MessageBox.Show("Failed to create place.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var deviceIds = new List<int>();
                if (DeviceComputerCheckBox.IsChecked == true) deviceIds.Add(1);
                if (DeviceTabletCheckBox.IsChecked == true) deviceIds.Add(2);
                if (DevicePhoneCheckBox.IsChecked == true) deviceIds.Add(3);
                if (DeviceConsoleCheckBox.IsChecked == true) deviceIds.Add(4);
                var deviceJson = System.Text.Json.JsonSerializer.Serialize(deviceIds);

                var gearIds = new List<int>();
                if (GearMeleeCheckBox.IsChecked == true) gearIds.Add(1);
                if (GearPowerUpsCheckBox.IsChecked == true) gearIds.Add(2);
                if (GearRangedCheckBox.IsChecked == true) gearIds.Add(3);
                if (GearNavigationCheckBox.IsChecked == true) gearIds.Add(4);
                if (GearExplosivesCheckBox.IsChecked == true) gearIds.Add(5);
                if (GearMusicalCheckBox.IsChecked == true) gearIds.Add(6);
                if (GearSocialCheckBox.IsChecked == true) gearIds.Add(7);
                if (GearTransportCheckBox.IsChecked == true) gearIds.Add(8);
                if (GearBuildingCheckBox.IsChecked == true) gearIds.Add(9);
                var gearJson = System.Text.Json.JsonSerializer.Serialize(gearIds);

                int genreId = AssetGenreNames.GetGenreIdFromString(
                    (GenreComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "All");

                int maxPlayers = 8;
                if (MaxPlayersComboBox.SelectedItem is ComboBoxItem maxItem)
                    int.TryParse(maxItem.Content?.ToString(), out maxPlayers);

                int accessType = (AccessComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() == "Friends" ? 2 : 1;

                int serverFill = ServerFillAutomaticRadio.IsChecked == true ? 0
                    : ServerFillEmptyRadio.IsChecked == true ? 1 : 2;

                bool paidAccess = SellGameAccessCheckBox.IsChecked == true;
                int paidAccessPrice = 0;
                if (paidAccess && int.TryParse(PriceTextBox.Text, out int pPrice))
                    paidAccessPrice = pPrice;

                bool psAllowed = AllowPrivateServersCheckBox.IsChecked == true;
                bool psFree = PrivateServerFreeRadio.IsChecked == true;
                int psPrice = 0;
                if (psAllowed && !psFree && int.TryParse(PrivateServerPriceTextBox.Text, out int psP))
                    psPrice = psP;

                bool isAllGenres = AllGenresGearRadio.IsChecked == true;

                var settings = new PlaceSettingsData
                {
                    PlaceId = universeInfo.RootPlaceId,
                    Name = placeName,
                    Description = PlaceDescriptionTextBox.Text ?? "",
                    Genre = genreId,
                    DeviceCompatibility = deviceJson,
                    MaxVisitorCount = maxPlayers,
                    ServerFillType = serverFill,
                    AccessType = accessType,
                    PrivateServersAllowed = psAllowed,
                    PrivateServersFree = psFree,
                    PrivateServersPrice = psPrice,
                    PaidAccessEnabled = paidAccess,
                    PaidAccessPrice = paidAccessPrice,
                    IsCopyingAllowed = AllowCopyingCheckBox.IsChecked == true,
                    IsAllGenresAllowed = isAllGenres,
                    AllowedGearTypes = gearJson,
                    AllowPlaceToBeCopiedInGame = false,
                    AllowPlaceToBeUpdatedInGame = false,
                    CustomIcon = false,
                    GeneratedIcon = true,
                    PlaceCustomThumbnail = false,
                    PlaceVideoThumbnail = false,
                    PlaceAutoGeneratedThumbnail = true
                };

                await GameCreationService.UpdatePlaceAllSettingsAsync(_connectionString, settings);

                var rootPlaceId = universeInfo.RootPlaceId;
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var placeAssetHash = await GamesRepository.GetPlaceAssetHashAsync(_connectionString, rootPlaceId, CancellationToken.None);
                        if (!string.IsNullOrWhiteSpace(placeAssetHash))
                        {
                        var thumbnailService = new ThumbnailService(null);
                        var thumbnailBaseUrl = "https://cdn.freblx.xyz/";

                            await PlaceThumbnail.GeneratePlaceThumbnailAsync(thumbnailService, _connectionString, rootPlaceId, placeAssetHash, thumbnailBaseUrl, placeName: placeName, cancellationToken: CancellationToken.None);
                            await PlaceThumbnail.GenerateAutoGeneratedThumbnailAsync(thumbnailService, _connectionString, rootPlaceId, placeAssetHash, thumbnailBaseUrl, CancellationToken.None);
                        }
                    }
                    catch (Exception thumbEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Thumbnail generation failed (non-fatal): {thumbEx.Message}");
                    }
                });

                MessageBox.Show(
                    $"Place created successfully!\n\nUniverse ID: {universeInfo.UniverseId}\nPlace ID: {universeInfo.RootPlaceId}",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating place: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                CreateButton.IsEnabled = true;
                CreateButton.Content = "Create Place";
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
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
