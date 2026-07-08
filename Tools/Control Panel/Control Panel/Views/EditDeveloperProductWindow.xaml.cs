using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Control_Panel.Functions;
using ControlPanel.Functions;
using Control_Panel.Properties;
using Games;
using Common;
using Microsoft.Win32;

namespace Control_Panel
{
    public partial class EditDeveloperProductWindow : Window
    {
        private readonly string _connectionString;
        private readonly long _universeId;
        private readonly long _productId;
        private string _selectedImagePath;
        private long? _existingImageAssetId;
        private long _jsonDeveloperProductId;

        public EditDeveloperProductWindow(string connectionString, long universeId, long productId)
        {
            InitializeComponent();
            ThemeManager.InitializeThemeForWindow(this);
            _connectionString = connectionString;
            _universeId = universeId;
            _productId = productId;
            Loaded += async (sender, e) => await LoadProductDataAsync();
        }

        private async Task LoadProductDataAsync()
        {
            try
            {
                var products = await DevProductHandler.GetDeveloperProductsByUniverse(_connectionString, _universeId);
                var product = products.FirstOrDefault(p => p.Id == _productId);
                if (product == null)
                {
                    MessageBox.Show("Developer product not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    DialogResult = false;
                    Close();
                    return;
                }

                ProductNameTextBox.Text = product.Name;
                ProductDescriptionTextBox.Text = product.Description ?? "";
                ProductPriceRobuxTextBox.Text = product.PriceInRobux.ToString();
                ProductPriceTixTextBox.Text = product.PriceInTix.ToString();
                _existingImageAssetId = product.ImageAssetId;

                await LoadJsonDeveloperProductIdAsync(product);

                await LoadProductImageAsync(product.ImageUrl, product.ImageAssetId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading product data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                DialogResult = false;
                Close();
            }
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
                }
                catch { }
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
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
                SaveButton.IsEnabled = false;

                long? excludeProductId = _jsonDeveloperProductId > 0 ? _jsonDeveloperProductId : (long?)null;
                var isUnique = await DevProductHandler.IsDeveloperProductNameUniqueAsync(_connectionString, _universeId, name, excludeProductId);
                if (!isUnique)
                {
                    MessageBox.Show("A developer product with this name already exists in this universe.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    SaveButton.IsEnabled = true;
                    return;
                }

                long? imageAssetId = _existingImageAssetId;
                if (!string.IsNullOrWhiteSpace(_selectedImagePath))
                {
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
                        SaveButton.IsEnabled = true;
                        return;
                    }

                    var assetId = await DevProductHandler.CreateDeveloperProductImageAsset(
                        _connectionString, fileName, imageUrl, fileHash, createdBy);

                    imageAssetId = assetId;
                }

                var updatedInUniverse = await DevProductHandler.UpdateDeveloperProductInUniverseAsync(
                    _connectionString, _universeId, _jsonDeveloperProductId, name, description, priceRobux, priceTix, imageAssetId);

                if (!updatedInUniverse)
                {
                    MessageBox.Show("Failed to update developer product in universe.", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                try
                {
                    await DevProductHandler.UpdateDeveloperProductInDatabaseAsync(
                        _connectionString, _productId, name, description, priceRobux, priceTix, imageAssetId);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Warning: Failed to update database record: {ex.Message}");
                }

                if (imageAssetId.HasValue && _existingImageAssetId.HasValue && imageAssetId.Value != _existingImageAssetId.Value)
                {
                    try
                    {
                        await DevProductHandler.UpdateDeveloperProductAssetLink(
                            _connectionString, imageAssetId.Value, _productId);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Warning: Failed to link asset: {ex.Message}");
                    }
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating developer product: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SaveButton.IsEnabled = true;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async Task LoadJsonDeveloperProductIdAsync(Games.DevProductHandler.DeveloperProduct product)
        {
            try
            {
                var entries = await GamesRepository.GetUniverseDeveloperProductsAsync(_connectionString, _universeId);
                if (entries == null) return;

                var targetName = product.Name?.Trim();
                foreach (var entry in entries)
                {
                    if (!entry.TryGetProperty("name", out var nameEl) || nameEl.ValueKind == System.Text.Json.JsonValueKind.Null) continue;
                    if (!string.Equals(nameEl.GetString()?.Trim(), targetName, StringComparison.OrdinalIgnoreCase)) continue;

                    if (entry.TryGetProperty("developerProductId", out var idEl) && idEl.ValueKind == System.Text.Json.JsonValueKind.Number)
                    {
                        _jsonDeveloperProductId = idEl.GetInt64();
                        return;
                    }
                }
            }
            catch { }
        }

        private async Task LoadProductImageAsync(string imageUrl, long? imageAssetId)
        {
            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imageUrl);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    ProductImagePreview.Source = bitmap;
                    return;
                }
                catch { }
            }

            if (imageAssetId.HasValue)
            {
                try
                {
                    var assetInfo = await DevProductHandler.GetDeveloperProductAssetAsync(_connectionString, imageAssetId.Value);
                    if (assetInfo.HasValue)
                    {
                        var cdnPath = Common.CDNUtilities.GetCDNAssetsPath("dev-product-icons");
                        var localPath = System.IO.Path.Combine(cdnPath, $"{assetInfo.Value.contentHash}.png");
                        if (File.Exists(localPath))
                        {
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(localPath, UriKind.Absolute);
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                            ProductImagePreview.Source = bitmap;
                        }
                    }
                }
                catch { }
            }
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
