using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Control_Panel.Properties;
using ControlPanel.Functions;
using Games;
using Npgsql;

namespace Control_Panel
{
    public class UniverseDevProductDisplayItem
    {
        public long Id { get; set; }
        public string Name { get; set; } = "";
        public string Price { get; set; } = "";
    }

    public partial class UniverseEditWindow : Window
    {
        private readonly long _universeId;
        private readonly string _connectionString;
        private UniverseInfo? _currentUniverse;

        public UniverseEditWindow(long universeId)
        {
            InitializeComponent();
            ThemeManager.InitializeThemeForWindow(this);

            _universeId = universeId;
            _connectionString = GetConnectionString();

            Loaded += async (sender, e) => await LoadUniverseDataAsync();
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
            AvatarSettingsContent.Visibility = Visibility.Collapsed;
            PlacesContent.Visibility = Visibility.Collapsed;
            DevProductsContent.Visibility = Visibility.Collapsed;

            if (tab == BasicSettingsTab)
                BasicSettingsContent.Visibility = Visibility.Visible;
            else if (tab == AvatarSettingsTab)
                AvatarSettingsContent.Visibility = Visibility.Visible;
            else if (tab == PlacesTab)
                PlacesContent.Visibility = Visibility.Visible;
            else if (tab == DevProductsTab)
                DevProductsContent.Visibility = Visibility.Visible;
        }

        private async Task LoadUniverseDataAsync()
        {
            try
            {
                IsEnabled = false;

                _currentUniverse = await GamesRepository.GetUniverseAsync(_connectionString, _universeId);
                if (_currentUniverse == null)
                {
                    MessageBox.Show($"Universe {_universeId} not found.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }

                Title = $"Configure Game - {_currentUniverse.Name}";
                UniverseNameTextBox.Text = _currentUniverse.Name ?? "";

                switch (_currentUniverse.PrivacyLevel)
                {
                    case 1: PublicRadio.IsChecked = true; break;
                    case 2: FriendsRadio.IsChecked = true; break;
                    case 3: PrivateRadio.IsChecked = true; break;
                    default: PublicRadio.IsChecked = true; break;
                }

                StudioAccessToApisCheckBox.IsChecked = _currentUniverse.Studio_Access_To_APIs;

                await LoadDevProductsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading universe: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
            finally
            {
                IsEnabled = true;
            }
        }

        private async Task LoadDevProductsAsync()
        {
            try
            {
                var products = await DevProductHandler.GetDeveloperProductsByUniverse(_connectionString, _universeId);
                DevProductsListView.ItemsSource = products.Select(p => new UniverseDevProductDisplayItem
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = $"R$ {p.PriceInRobux}"
                }).ToList();
            }
            catch (Exception ex)
            {
                DevProductsListView.ItemsSource = new List<UniverseDevProductDisplayItem>();
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUniverse == null) return;

            var name = UniverseNameTextBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Name cannot be empty.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int privacyLevel = 1;
            if (FriendsRadio.IsChecked == true) privacyLevel = 2;
            else if (PrivateRadio.IsChecked == true) privacyLevel = 3;

            bool studioAccessToApis = StudioAccessToApisCheckBox.IsChecked == true;

            try
            {
                SaveButton.IsEnabled = false;
                SaveButton.Content = "Saving...";

                await GameCreationService.UpdateUniverseNameAsync(_universeId, _connectionString, name);
                await GameCreationService.UpdateUniversePrivacyAsync(_universeId, _connectionString, privacyLevel);

                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();
                const string updateStudioSql = @"UPDATE universes SET Studio_Access_To_APIs = @studioAccess WHERE universe_id = @universeId";
                using (var cmd = new NpgsqlCommand(updateStudioSql, conn))
                {
                    cmd.Parameters.AddWithValue("studioAccess", studioAccessToApis);
                    cmd.Parameters.AddWithValue("universeId", _universeId);
                    await cmd.ExecuteNonQueryAsync();
                }

                MessageBox.Show("Game settings saved successfully.", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SaveButton.IsEnabled = true;
                SaveButton.Content = "Save";
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void EditDevProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is long productId)
            {
                var window = new EditDeveloperProductWindow(_connectionString, _universeId, productId);
                if (window.ShowDialog() == true)
                {
                    _ = LoadDevProductsAsync();
                }
            }
        }
    }
}
