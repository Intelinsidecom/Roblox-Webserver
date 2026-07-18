using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Control_Panel.Functions;
using ControlPanel.Functions;
using Control_Panel.Properties;
using Games;
using Microsoft.Win32;

namespace Control_Panel
{
    public partial class CreateDeveloperProductWindow : Window
    {
        private readonly string _connectionString;
        private readonly long _universeId;
        private string _selectedImagePath;

        public CreateDeveloperProductWindow(string connectionString, long universeId)
        {
            InitializeComponent();
            ThemeManager.InitializeThemeForWindow(this);
            _connectionString = connectionString;
            _universeId = universeId;
        }

        private void SelectImageButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select Developer Product Image",
                Filter = "Image Files (*.png;*.jpg;*.jpeg;*.gif;*.bmp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp"
            };
            if (dialog.ShowDialog() == true)
            {
                _selectedImagePath = dialog.FileName;
                SelectedImageIndicator.Text = System.IO.Path.GetFileName(_selectedImagePath);
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(_selectedImagePath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    ProductImagePreview.Source = bitmap;
                    ImagePreviewBorder.Visibility = Visibility.Visible;
                }
                catch (Exception ex) { Console.WriteLine($"[ERROR] SelectImageButton_Click preview: {ex}"); }
            }
        }

        private async void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            var name = ProductNameTextBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Product name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var description = ProductDescriptionTextBox.Text?.Trim() ?? "";

            if (!int.TryParse(ProductPriceRobuxTextBox.Text?.Trim(), out var priceRobux) || priceRobux < 0)
            {
                MessageBox.Show("Price In Robux must be a non-negative number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(ProductPriceTixTextBox.Text?.Trim(), out var priceTix) || priceTix < 0)
            {
                MessageBox.Show("Price In Tickets must be a non-negative number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                CreateButton.IsEnabled = false;

                var isUnique = await DevProductHandler.IsDeveloperProductNameUniqueAsync(_connectionString, _universeId, name);
                if (!isUnique)
                {
                    MessageBox.Show("A developer product with this name already exists in this universe.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    CreateButton.IsEnabled = true;
                    return;
                }

                if (string.IsNullOrWhiteSpace(_selectedImagePath))
                {
                    MessageBox.Show("Please select an image for the developer product.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    CreateButton.IsEnabled = true;
                    return;
                }

                long? imageAssetId = null;

                var baseUrl = Settings.Default.ThumbnailUrl;
                if (string.IsNullOrWhiteSpace(baseUrl))
                    baseUrl = $"http://{Settings.Default.WebsiteHost}:{Settings.Default.WebsitePort}";

                var fileBytes = File.ReadAllBytes(_selectedImagePath);
                using var fileStream = new MemoryStream(fileBytes);
                var contentType = GetContentType(_selectedImagePath);
                var fileName = System.IO.Path.GetFileName(_selectedImagePath);

                var (imageUrl, fileHash) = await DevProductHandler.ProcessDeveloperProductImageAsync(
                    fileStream, fileName, contentType, baseUrl);

                var defaultOwnerStr = Settings.Default.DefaultOwnerUserId;
                if (!long.TryParse(defaultOwnerStr, out var createdBy) || createdBy <= 0)
                {
                    MessageBox.Show("Default Owner User ID not configured (set in Assets tab).", "Configuration Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    CreateButton.IsEnabled = true;
                    return;
                }

                var assetId = await DevProductHandler.CreateDeveloperProductImageAsset(
                    _connectionString, fileName, imageUrl, fileHash, createdBy);

                imageAssetId = assetId;

                var productId = await GamesRepository.GenerateUniverseDeveloperProductIdAsync(_connectionString);

                var developerProduct = new
                {
                    developerProductId = productId,
                    universeId = _universeId,
                    name = name,
                    description = description,
                    priceInRobux = priceRobux,
                    priceInTix = priceTix,
                    imageAssetId = imageAssetId,
                    creatorUserId = createdBy,
                    createdAt = DateTime.UtcNow
                };

                var developerProductJson = JsonSerializer.SerializeToElement(developerProduct);
                var addedToUniverse = await GamesRepository.AddDeveloperProductToUniverseAsync(
                    _connectionString, _universeId, developerProductJson);

                if (!addedToUniverse)
                {
                    MessageBox.Show("Failed to add developer product to universe.", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                try
                {
                    await DevProductHandler.CreateDeveloperProduct(
                        _connectionString, _universeId, name, description, priceRobux, priceTix, imageAssetId);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Warning: Failed to create database record: {ex.Message}");
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating developer product: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                CreateButton.IsEnabled = true;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private static string GetContentType(string filePath)
        {
            var ext = System.IO.Path.GetExtension(filePath)?.ToLowerInvariant();
            switch (ext)
            {
                case ".png": return "image/png";
                case ".jpg": return "image/jpeg";
                case ".jpeg": return "image/jpeg";
                case ".gif": return "image/gif";
                case ".bmp": return "image/bmp";
                case ".webp": return "image/webp";
                default: return "image/png";
            }
        }
    }
}
