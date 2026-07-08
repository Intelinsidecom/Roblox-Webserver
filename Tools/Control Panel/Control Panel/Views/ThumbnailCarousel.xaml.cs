using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Games;
using ControlPanel.Functions;

namespace Control_Panel
{
    public partial class ThumbnailCarousel : Window
    {
        private readonly long _universeId;
        private readonly string _connectionString;
        private readonly GamesService _gamesService;
        private List<WideThumbnailItem> _items = new List<WideThumbnailItem>();
        private int _currentIndex;

        public ThumbnailCarousel(long universeId, string connectionString)
        {
            InitializeComponent();
            ThemeManager.InitializeThemeForWindow(this);
            _universeId = universeId;
            _connectionString = connectionString;
            _gamesService = new GamesService();
            Loaded += async (sender, e) => await LoadAsync();
            Closing += (sender, e) => ClearData();
        }

        private async Task LoadAsync()
        {
            try
            {
                if (_universeId <= 0)
                {
                    ShowEmpty();
                    return;
                }

                var placeIds = await GamesQueries.GetUniversePlaceIdsAsync(_universeId, _connectionString);
                if (placeIds == null || placeIds.Count == 0)
                {
                    ShowEmpty();
                    return;
                }

                _items = await _gamesService.FetchWideThumbnailsAsync(placeIds, _connectionString);

                if (_items.Count == 0)
                {
                    ShowEmpty();
                    return;
                }

                _currentIndex = 0;
                ShowImage(_currentIndex);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading carousel: {ex.Message}");
                ShowEmpty();
            }
        }

        private void ShowImage(int index)
        {
            if (index < 0 || index >= _items.Count)
            {
                ShowEmpty();
                return;
            }

            EmptyStateText.Visibility = Visibility.Collapsed;
            CarouselImage.Visibility = Visibility.Visible;

            var item = _items[index];
            try
            {
                CarouselImage.Source = !string.IsNullOrWhiteSpace(item.Url)
                    ? new BitmapImage(new Uri(item.Url))
                    : null;
            }
            catch
            {
                CarouselImage.Source = null;
            }

            CounterText.Text = $"{index + 1} / {_items.Count}";
            BackButton.IsEnabled = index > 0;
            NextButton.IsEnabled = index < _items.Count - 1;
        }

        private void ShowEmpty()
        {
            _items.Clear();
            _currentIndex = 0;
            CarouselImage.Source = null;
            CarouselImage.Visibility = Visibility.Collapsed;
            EmptyStateText.Visibility = Visibility.Visible;
            CounterText.Text = "0 / 0";
            BackButton.IsEnabled = false;
            NextButton.IsEnabled = false;
        }

        private void ClearData()
        {
            _items.Clear();
            CarouselImage.Source = null;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentIndex > 0)
            {
                _currentIndex--;
                ShowImage(_currentIndex);
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentIndex < _items.Count - 1)
            {
                _currentIndex++;
                ShowImage(_currentIndex);
            }
        }
    }
}
