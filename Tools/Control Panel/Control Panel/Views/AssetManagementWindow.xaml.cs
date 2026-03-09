using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Control_Panel.Functions;
using ControlPanel.Functions;
using Assets;
using Control_Panel.Properties;

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
        
        public AssetManagementWindow()
        {
            InitializeComponent();
            ThemeManager.InitializeThemeForWindow(this);
            Settings.Default.PropertyChanged += Settings_PropertyChanged;
            var connectionString = GetConnectionString();
            _assetService = new Control_Panel.Functions.AssetService();
            _assetsRepository = new AssetsRepository();
            _userSearchService = new UserSearchService(connectionString);
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
        
        private void ClearAssetData()
        {
            _currentAsset = null;
            AssetThumbnailImage.Source = null;
            AssetIdText.Text = "";
            AssetNameText.Text = "";
            AssetDescriptionTextBox.Text = "";
            AssetTypeText.Text = "";
            CreatorText.Text = "";
            UpdatedText.Text = "";
            RobuxPriceText.Text = "";
            TixPriceText.Text = "";
            OnSaleText.Text = "";
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
                            ShowCdnNotActiveMessage();
                            return;
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
                        ShowCdnNotActiveMessage();
                        return;
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
                        System.Diagnostics.Debug.WriteLine($"Failed to load image from website: {imgEx.Message}");
                        ShowWebsiteNotActiveMessage();
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
    }
}
