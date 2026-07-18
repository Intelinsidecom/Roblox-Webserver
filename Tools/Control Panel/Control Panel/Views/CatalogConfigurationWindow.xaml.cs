using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Assets;
using Npgsql;

namespace Control_Panel
{
    public partial class CatalogConfigurationWindow : Window
    {
        private readonly AssetsRepository _repository;
        private readonly string _connectionString;

        public CatalogConfigurationWindow()
        {
            InitializeComponent();
            _repository = new AssetsRepository();
            _connectionString = Properties.Settings.Default.DatabaseConnectionString;

            if (string.IsNullOrEmpty(_connectionString))
            {
                MessageBox.Show("Database connection string is not configured.", "Configuration Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            Loaded += async (s, e) => await LoadFeaturedItemsAsync();
        }

        private async Task LoadFeaturedItemsAsync()
        {
            try
            {
                var featured = await _repository.GetFeaturedAssetsAsync(_connectionString);

                var textBoxes = new[] { FeaturedId1, FeaturedId2, FeaturedId3, FeaturedId4 };
                for (int i = 0; i < textBoxes.Length; i++)
                {
                    textBoxes[i].Text = string.Empty;
                }

                foreach (var (assetId, name, rank) in featured)
                {
                    if (rank >= 1 && rank <= 4)
                    {
                        textBoxes[rank - 1].Text = assetId.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load featured items: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var textBoxes = new[] { FeaturedId1, FeaturedId2, FeaturedId3, FeaturedId4 };
                var newSlots = new Dictionary<int, long>();

                for (int i = 0; i < textBoxes.Length; i++)
                {
                    var text = textBoxes[i].Text?.Trim();
                    if (string.IsNullOrEmpty(text))
                        continue;

                    if (!long.TryParse(text, out var assetId) || assetId <= 0)
                    {
                        MessageBox.Show($"Slot {i + 1}: '{text}' is not a valid asset ID. Please enter a positive number or leave empty.", "Validation Error",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    newSlots[i + 1] = assetId;
                }

                var oldFeatured = await _repository.GetFeaturedAssetsAsync(_connectionString);

                foreach (var (oldId, _, oldRank) in oldFeatured)
                {
                    long newIdForRank;
                    bool hasNewId = newSlots.TryGetValue(oldRank, out newIdForRank);
                    if (!hasNewId || newIdForRank != oldId)
                    {
                        await _repository.UpdateAssetFeaturedRankAsync(_connectionString, oldId, 0);
                    }
                }

                foreach (var (rank, assetId) in newSlots)
                {
                    await _repository.UpdateAssetFeaturedRankAsync(_connectionString, assetId, rank);
                }

                MessageBox.Show("Featured items saved successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save featured items: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
