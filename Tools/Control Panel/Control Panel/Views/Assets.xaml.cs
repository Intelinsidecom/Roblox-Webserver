using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.Win32;
using System.Configuration;
using Assets;
using Control_Panel.Functions;
using AssetService = Control_Panel.Functions.AssetService;
using ControlPanel.Functions;

namespace Control_Panel
{
    /// <summary>
    /// Represents an asset item for display in catalog
    /// </summary>
    public class AssetItem
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Price { get; set; }
        public string TixPrice { get; set; }
        public string Creator { get; set; }
        public string UpdatedTime { get; set; }
        public string ThumbnailUrl { get; set; }

        public AssetItem()
        {
        }

        public AssetItem(long id, string name, string type, string price, string tixPrice, string creator, string updatedTime, string thumbnailUrl)
        {
            Id = id;
            Name = name;
            Type = type;
            Price = price;
            TixPrice = tixPrice;
            Creator = creator;
            UpdatedTime = updatedTime;
            ThumbnailUrl = thumbnailUrl;
        }
    }

    public partial class AssetsView : UserControl
    {
        private string _selectedFilePath;
        private readonly AssetService _assetService;
        private SimpleViewLoader _viewLoader;
        private ObservableCollection<AssetItem> _assetItems;
        private readonly AssetSearchParameters _searchParams;
        private CancellationTokenSource _searchCancellationTokenSource;
        private System.Timers.Timer _searchDebounceTimer;
        private const int SearchDebounceMs = 300;
        private readonly UserSearchService _userSearchService;

        public AssetsView()
        {
            InitializeComponent();
            _assetService = new AssetService();
            _assetItems = new ObservableCollection<AssetItem>();
            _searchParams = new AssetSearchParameters();
            _searchCancellationTokenSource = new CancellationTokenSource();
            string connectionString = GetConnectionString();
            _userSearchService = new UserSearchService(connectionString);
            AssetItemsControl.ItemsSource = _assetItems;
            LoadAssetsData();
            LoadConfigurationValues();
            InitializePlaceholders();
            this.Loaded += AssetsView_Loaded;
        }

        private void AssetsView_Loaded(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this);
            if (parentWindow is Main mainWindow)
            {
                _viewLoader = mainWindow.ViewLoader;
            }
        }

        private void LoadAssetsData()
        {
            if (!_assetItems.Any())
            {
                _ = PerformSearchAsync();
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            _ = PerformSearchAsync();
        }

        private void AssetUploadTabButton_Click(object sender, RoutedEventArgs e)
        {
            AssetUploadTab.Visibility = Visibility.Visible;
            AssetConfigTab.Visibility = Visibility.Collapsed;
            AssetSearchTab.Visibility = Visibility.Collapsed;
            AssetUploadTabButton.Background = (System.Windows.Media.Brush)FindResource("AccentPrimary");
            AssetUploadTabButton.Foreground = System.Windows.Media.Brushes.White;
            AssetUploadTabButton.BorderBrush = null;
            AssetConfigTabButton.Background = System.Windows.Media.Brushes.Transparent;
            AssetConfigTabButton.BorderBrush = (System.Windows.Media.Brush)FindResource("SubtleText");
            AssetConfigTabButton.Foreground = (System.Windows.Media.Brush)FindResource("Foreground");
            AssetSearchTabButton.Background = System.Windows.Media.Brushes.Transparent;
            AssetSearchTabButton.BorderBrush = (System.Windows.Media.Brush)FindResource("SubtleText");
            AssetSearchTabButton.Foreground = (System.Windows.Media.Brush)FindResource("Foreground");
        }

        private async void AssetSearchTabButton_Click(object sender, RoutedEventArgs e)
        {
            AssetUploadTab.Visibility = Visibility.Collapsed;
            AssetConfigTab.Visibility = Visibility.Collapsed;
            AssetSearchTab.Visibility = Visibility.Visible;
            AssetUploadTabButton.Background = System.Windows.Media.Brushes.Transparent;
            AssetUploadTabButton.BorderBrush = (System.Windows.Media.Brush)FindResource("SubtleText");
            AssetUploadTabButton.Foreground = (System.Windows.Media.Brush)FindResource("Foreground");
            AssetConfigTabButton.Background = System.Windows.Media.Brushes.Transparent;
            AssetConfigTabButton.BorderBrush = (System.Windows.Media.Brush)FindResource("SubtleText");
            AssetConfigTabButton.Foreground = (System.Windows.Media.Brush)FindResource("Foreground");
            AssetSearchTabButton.Background = (System.Windows.Media.Brush)FindResource("AccentPrimary");
            AssetSearchTabButton.Foreground = System.Windows.Media.Brushes.White;
            AssetSearchTabButton.BorderBrush = null;
            _ = PerformSearchAsync();
        }

        private void AssetConfigTabButton_Click(object sender, RoutedEventArgs e)
        {
            AssetUploadTab.Visibility = Visibility.Collapsed;
            AssetConfigTab.Visibility = Visibility.Visible;
            AssetSearchTab.Visibility = Visibility.Collapsed;
            AssetUploadTabButton.Background = System.Windows.Media.Brushes.Transparent;
            AssetUploadTabButton.BorderBrush = (System.Windows.Media.Brush)FindResource("SubtleText");
            AssetUploadTabButton.Foreground = (System.Windows.Media.Brush)FindResource("Foreground");
            AssetConfigTabButton.Background = (System.Windows.Media.Brush)FindResource("AccentPrimary");
            AssetConfigTabButton.Foreground = System.Windows.Media.Brushes.White;
            AssetConfigTabButton.BorderBrush = null;
            AssetSearchTabButton.Background = System.Windows.Media.Brushes.Transparent;
            AssetSearchTabButton.BorderBrush = (System.Windows.Media.Brush)FindResource("SubtleText");
            AssetSearchTabButton.Foreground = (System.Windows.Media.Brush)FindResource("Foreground");
        }

        private void LoadConfigurationValues()
        {
            var settings = Properties.Settings.Default;
            AssetsDirectoryTextBox.Text = settings.AssetsDirectory ?? string.Empty;
            CDNUrlTextBox.Text = settings.ThumbnailUrl ?? string.Empty;
            TshirtTemplatePathTextBox.Text = settings.TshirtTemplatePath ?? string.Empty;
            TshirtTemplateHighResPathTextBox.Text = settings.TshirtTemplateHighResPath ?? string.Empty;
            PublicBaseUrlTextBox.Text = settings.PublicBaseUrl ?? string.Empty;
            DefaultOwnerUserIdTextBox.Text = settings.DefaultOwnerUserId ?? "1";
        }

        /// <summary>
        /// Ensures that a URL has HTTP or HTTPS protocol. If not present, adds HTTP:// by default.
        /// </summary>
        /// <param name="url">The URL to validate and fix</param>
        /// <returns>URL with proper protocol</returns>
        private string EnsureUrlProtocol(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return url;

            url = url.Trim();
            
            if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
                url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return url;
            }
            return $"http://{url}";
        }

        private void SaveConfiguration()
        {
            try
            {
                var settings = Properties.Settings.Default;
                settings.AssetsDirectory = AssetsDirectoryTextBox.Text;
                settings.ThumbnailUrl = EnsureUrlProtocol(CDNUrlTextBox.Text);
                settings.TshirtTemplatePath = TshirtTemplatePathTextBox.Text;
                settings.TshirtTemplateHighResPath = TshirtTemplateHighResPathTextBox.Text;
                settings.PublicBaseUrl = EnsureUrlProtocol(PublicBaseUrlTextBox.Text);
                settings.DefaultOwnerUserId = DefaultOwnerUserIdTextBox.Text;
                settings.Save();
                settings.Reload();
                CDNUrlTextBox.Text = settings.ThumbnailUrl ?? string.Empty;
                PublicBaseUrlTextBox.Text = settings.PublicBaseUrl ?? string.Empty;
                ConfigStatusText.Text = "Configuration saved successfully!";
                ConfigStatusText.Foreground = (System.Windows.Media.Brush)FindResource("AccentGreen");
            }
            catch (Exception ex)
            {
                ConfigStatusText.Text = $"Error saving configuration: {ex.Message}";
                ConfigStatusText.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private void SaveConfigButton_Click(object sender, RoutedEventArgs e)
        {
            SaveConfiguration();
        }

        private void BrowseAssetsDirectory_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderDialog.Description = "Select Assets Directory";
            folderDialog.ShowNewFolderButton = true;
            
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                AssetsDirectoryTextBox.Text = folderDialog.SelectedPath;
            }
        }

        private void BrowseTshirtTemplate_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select T-Shirt Template Image",
                Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All files (*.*)|*.*",
                FilterIndex = 1
            };

            if (openFileDialog.ShowDialog() == true)
            {
                TshirtTemplatePathTextBox.Text = openFileDialog.FileName;
            }
        }

        private void BrowseTshirtTemplateHighRes_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select T-Shirt High-Res Template Image",
                Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All files (*.*)|*.*",
                FilterIndex = 1
            };

            if (openFileDialog.ShowDialog() == true)
            {
                TshirtTemplateHighResPathTextBox.Text = openFileDialog.FileName;
            }
        }



        private void AssetTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = AssetTypeComboBox.SelectedItem as ComboBoxItem;
            if (selectedItem?.Content?.ToString() is string assetTypeText)
            {
                bool showItemOptions = assetTypeText == "T-Shirt (2)" || 
                                     assetTypeText == "Shirt (11)" || 
                                     assetTypeText == "Pants (12)";
                PriceFieldsPanel.Visibility = showItemOptions ? Visibility.Visible : Visibility.Collapsed;
                ItemOptionsPanel.Visibility = showItemOptions ? Visibility.Visible : Visibility.Collapsed;
                LimitedFieldsPanel.Visibility = showItemOptions ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void IsLimitedCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            LimitedFieldsGrid.IsEnabled = true;
        }

        private void IsLimitedCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            LimitedFieldsGrid.IsEnabled = false;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            string assetTypeContent = (AssetTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Unknown";
            string filter = "All files (*.*)|*.*";
            string title = "Select File";
            
            switch (assetTypeContent)
            {
                case "Decal (1)":
                case "T-Shirt (2)":
                case "Shirt (11)":
                case "Pants (12)":
                    filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All files (*.*)|*.*";
                    title = $"Select {assetTypeContent.Split('(')[0].Trim()} Image";
                    break;
                default:
                    title = "Select File";
                    break;
            }

            var openFileDialog = new OpenFileDialog
            {
                Title = title,
                Filter = filter,
                FilterIndex = 1
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _selectedFilePath = openFileDialog.FileName;
                SelectedFileTextBox.Text = _selectedFilePath;
            }
        }

        private async void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AssetTypeComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Please select an asset type before uploading.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(_selectedFilePath))
                {
                    MessageBox.Show("Please select a file before uploading.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string assetTypeContent = (AssetTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Unknown";
                string assetTypeName = assetTypeContent.Split('(')[0].Trim();
                _viewLoader?.UpdateStatus("Starting upload...");
                string customAssetIdText = AssetIdTextBox.Text?.Trim();
                bool hasCustomAssetId = !string.IsNullOrWhiteSpace(customAssetIdText);
                _viewLoader?.UpdateStatus("Processing file...");
                await System.Threading.Tasks.Task.Delay(100);
                bool putOnSale = PutOnSaleCheckBox.IsChecked == true;
                var result = await _assetService.UploadImageBasedAssetAsync(
                    _selectedFilePath,
                    AssetNameTextBox.Text?.Trim(),
                    customAssetIdText,
                    RobuxPriceTextBox.Text?.Trim(),
                    TixPriceTextBox.Text?.Trim(),
                    assetTypeContent,
                    putOnSale);

                if (hasCustomAssetId)
                {
                    _viewLoader?.UpdateStatus($"Assigning custom asset ID {customAssetIdText}...");
                    await System.Threading.Tasks.Task.Delay(100);
                }

                string successMessage = $"{assetTypeName} Asset: {result.AssetId} Uploaded successfully";
                _viewLoader?.UpdateStatus(successMessage);
                await System.Threading.Tasks.Task.Delay(2000);
                ClearForm();
                _viewLoader?.UpdateStatus("Ready");
            }
            catch (Exception ex)
            {
                _viewLoader?.UpdateStatus(ex.Message);
                
                if (ex.Message != "Unsupported File")
                {
                    MessageBox.Show(ex.Message, "Upload Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ClearForm()
        {
            AssetIdTextBox.Clear();
            AssetNameTextBox.Clear();
            AssetDescriptionTextBox.Clear();
            RobuxPriceTextBox.Text = "0";
            TixPriceTextBox.Text = "0";
            SelectedFileTextBox.Clear();
            _selectedFilePath = null;
            IsLimitedCheckBox.IsChecked = false;
            PutOnSaleCheckBox.IsChecked = true;
            LimitedFieldsGrid.IsEnabled = false;
            AssetTypeComboBox.SelectedIndex = -1;
            ConfigStatusText.Text = "Click Save to apply configuration changes.";
            ConfigStatusText.Foreground = (System.Windows.Media.Brush)FindResource("Foreground");
        }

        private async void AssetSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_searchDebounceTimer != null)
            {
                _searchDebounceTimer.Stop();
                _searchDebounceTimer.Dispose();
            }
            
            if (AssetSearchTextBox.Text == AssetSearchTextBox.Tag as string)
            {
                _searchParams.SearchQuery = string.Empty;
                await PerformSearchAsync();
                return;
            }
            
            _searchDebounceTimer = new System.Timers.Timer(SearchDebounceMs);
            _searchDebounceTimer.AutoReset = false;
            _searchDebounceTimer.Elapsed += async (s, args) =>
            {
                Dispatcher.Invoke(async () =>
                {
                    if (sender is TextBox textBox && _searchParams != null)
                    {
                        _searchParams.SearchQuery = textBox.Text;
                        await PerformSearchAsync();
                    }
                });
            };
            
            _searchDebounceTimer.Start();
        }

        private async void AssetIdSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_searchDebounceTimer != null)
            {
                _searchDebounceTimer.Stop();
                _searchDebounceTimer.Dispose();
            }
            
            if (AssetIdSearchTextBox.Text == AssetIdSearchTextBox.Tag as string)
            {
                _searchParams.AssetIdQuery = string.Empty;
                await PerformSearchAsync();
                return;
            }
            
            _searchDebounceTimer = new System.Timers.Timer(SearchDebounceMs);
            _searchDebounceTimer.AutoReset = false;
            _searchDebounceTimer.Elapsed += async (s, args) =>
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    if (!string.IsNullOrWhiteSpace(AssetIdSearchTextBox.Text) && 
                        AssetIdSearchTextBox.Text != AssetIdSearchTextBox.Tag as string)
                    {
                        _searchParams.AssetIdQuery = AssetIdSearchTextBox.Text.Trim();
                    }
                    else
                    {
                        _searchParams.AssetIdQuery = string.Empty;
                    }
                });
                
                await PerformSearchAsync();
            };
            
            _searchDebounceTimer.Start();
        }


        private async void AssetIdSearchTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                string assetIdText = AssetIdSearchTextBox.Text.Trim();
                
                if (!string.IsNullOrWhiteSpace(assetIdText) && long.TryParse(assetIdText, out long assetId))
                {
                    try
                    {
                        var assetManagementWindow = new AssetManagementWindow(assetId);
                        assetManagementWindow.Owner = Window.GetWindow(this);
                        assetManagementWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        assetManagementWindow.Show();
                        AssetIdSearchTextBox.Clear();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error opening asset {assetId}: {ex.Message}");
                        try
                        {
                            Control_Panel.ConsoleWindowManager.GlobalConsole.WriteLine($"Error opening asset {assetId}: {ex.Message}", "ERROR");
                        }
                        catch
                        {
                            // Ignore console errors
                        }
                        
                        MessageBox.Show($"Failed to open asset {assetId}: {ex.Message}", "Error", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else if (!string.IsNullOrWhiteSpace(assetIdText))
                {
                    MessageBox.Show("Please enter a valid asset ID", "Invalid Input", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show("Please enter a valid numeric asset ID", "Invalid Input", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                
                e.Handled = true;
            }
        }

        private async void AssetTypeFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem && _searchParams != null)
            {
                _searchParams.AssetTypeFilter = selectedItem.Content.ToString() ?? "All Types";
                await PerformSearchAsync();
            }
        }

        private async void DateFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem && _searchParams != null)
            {
                _searchParams.DateFilter = selectedItem.Content.ToString() ?? "Any Time";
                await PerformSearchAsync();
            }
        }

        private async void SortFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem && _searchParams != null)
            {
                _searchParams.SortFilter = selectedItem.Content.ToString() ?? "Most Recent";
                await PerformSearchAsync();
            }
        }

        /// <summary>
        /// Performs asset search with current parameters
        /// </summary>
        private async Task PerformSearchAsync()
        {
            if (_searchParams == null || _assetService == null)
                return;

            try
            {
                _searchCancellationTokenSource?.Cancel();
                _searchCancellationTokenSource = new CancellationTokenSource();
                _viewLoader?.UpdateStatus("Searching assets...");
                _assetItems.Clear();
                var results = await _assetService.SearchAssetsAsync(_searchParams);
                Dispatcher.Invoke(() =>
                {
                    foreach (var result in results)
                    {
                        _assetItems.Add(new AssetItem(
                            result.Id,
                            result.Name,
                            result.Type,
                            result.Price,
                            result.TixPrice,
                            result.Creator,
                            result.UpdatedTime,
                            result.ThumbnailUrl
                        ));
                    }

                    _viewLoader?.UpdateStatus($"Found {results.Count} assets");
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    _viewLoader?.UpdateStatus($"Search error: {ex.Message}");
                });
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
        
        private void InitializePlaceholders()
        {
            SetPlaceholder(AssetSearchTextBox);
            SetPlaceholder(AssetIdSearchTextBox);
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
        /// Handles creator name click to navigate to user management
        /// </summary>
        private async void CreatorLinkButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string creatorName)
            {
                try
                {
                    var searchResults = await _userSearchService.SearchUsersByUsernameAsync(creatorName, 1);
                    
                    if (searchResults.Count > 0)
                    {
                        var userId = (int)searchResults[0].Id;
                        Views.UserManagementWindow.OpenUserManagement(userId);
                    }
                    else
                    {
                        MessageBox.Show($"User '{creatorName}' not found in the database.", "User Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error opening user management for {creatorName}: {ex.Message}");
                    MessageBox.Show($"Error opening user management: {ex.Message}", "Navigation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Handles catalog item click to open Asset Management view
        /// </summary>
        private void CatalogItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is long assetId)
                {
                    OpenAssetManagementView(assetId);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error opening asset management: {ex.Message}");
                MessageBox.Show($"Error opening asset management: {ex.Message}", "Navigation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Opens the Asset Management view and loads the specified asset
        /// </summary>
        private void OpenAssetManagementView(long assetId)
        {
            try
            {
                var assetManagementWindow = new AssetManagementWindow();
                assetManagementWindow.LoadAsset(assetId);
                assetManagementWindow.Show();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating Asset Management window: {ex.Message}");
                MessageBox.Show($"Error opening Asset Management: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


    }
}
