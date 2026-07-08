using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Games;
using ControlPanel.Functions;

namespace Control_Panel
{
    public class PlaceCardItem
    {
        public long PlaceId { get; set; }
        public string Name { get; set; }
        public string ThumbnailUrl { get; set; }
        public bool IsRootPlace { get; set; }
        public string SecondaryText => IsRootPlace ? "Root Place" : $"Place #{PlaceId}";
        public string PlaceIdLabel => IsRootPlace ? $"Root Place (ID: {PlaceId})" : $"Place ID: {PlaceId}";
    }

    public partial class PlaceSelectWindow : Window
    {
        private readonly long _universeId;
        private readonly string _connectionString;
        private readonly string _universeName;
        private readonly ObservableCollection<PlaceCardItem> _placeItems;

        public PlaceSelectWindow(long universeId, string universeName)
        {
            InitializeComponent();
            ThemeManager.InitializeThemeForWindow(this);

            _universeId = universeId;
            _universeName = universeName;
            _connectionString = GetConnectionString();
            _placeItems = new ObservableCollection<PlaceCardItem>();
            PlaceItemsControl.ItemsSource = _placeItems;

            WindowTitleText.Text = $"Places - {universeName}";

            Loaded += async (sender, e) => await LoadPlacesAsync();
        }

        private string GetConnectionString()
        {
            var connectionString = Properties.Settings.Default.DatabaseConnectionString;
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("Database connection string is not configured in application settings.");
            return connectionString;
        }

        private async Task LoadPlacesAsync()
        {
            try
            {
                var placeIds = await GamesQueries.GetUniversePlaceIdsAsync(_universeId, _connectionString);

                var rootPlaceId = placeIds[0];
                var otherPlaceIds = placeIds.Skip(1).OrderBy(id => id).ToList();

                var orderedIds = new List<long> { rootPlaceId };
                orderedIds.AddRange(otherPlaceIds);

                var placeData = await GamesQueries.GetPlacesByIdsAsync(orderedIds, _connectionString);

                _placeItems.Clear();
                foreach (var place in placeData)
                {
                    _placeItems.Add(new PlaceCardItem
                    {
                        PlaceId = place.PlaceId,
                        Name = place.Name,
                        ThumbnailUrl = place.ThumbnailUrl,
                        IsRootPlace = place.PlaceId == rootPlaceId
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading places: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadPlacesAsync();
        }

        private void PlaceCard_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is long placeId)
            {
                var window = new PlaceManagementWindow(placeId);
                window.Owner = this;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                window.Show();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
