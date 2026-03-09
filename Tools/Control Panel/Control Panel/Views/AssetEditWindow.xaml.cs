using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Control_Panel.Functions;
using ControlPanel.Functions;
using Control_Panel.Properties;

namespace Control_Panel
{
    public partial class AssetEditWindow : Window
    {
        private readonly AssetService _assetService;
        private AssetSearchResult _currentAsset;
        
        public AssetEditWindow()
        {
            InitializeComponent();
            ThemeManager.InitializeThemeForWindow(this);
            Settings.Default.PropertyChanged += Settings_PropertyChanged;
            
            _assetService = new AssetService();
        }
        
        public AssetEditWindow(long assetId) : this()
        {
            LoadAssetData(assetId);
        }
        
        private void LoadAssetData(long assetId)
        {
            try
            {
                var loadTask = System.Threading.Tasks.Task.Run(async () => 
                {
                    return await _assetService.GetAssetByIdAsync(assetId);
                });
                
                loadTask.Wait();
                _currentAsset = loadTask.Result;
                
                if (_currentAsset == null)
                {
                    MessageBox.Show($"Asset with ID {assetId} not found.", "Asset Not Found", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    this.Close();
                    return;
                }
                
                if (_currentAsset.AssetTypeId == 9)
                {
                    MessageBox.Show("Places cannot be edited in Asset Edit.\n\nPlaces are game environments and should be managed through the Games section.", 
                        "Unsupported Asset Type", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                    return;
                }
                
                PopulateFormFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading asset data: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }
        
        private void PopulateFormFields()
        {
            if (_currentAsset == null) return;

            HeaderTextBlock.Text = $"Asset Edit - ID {_currentAsset.Id}";
            AssetNameTextBox.Text = _currentAsset.Name ?? "";
            AssetDescriptionTextBox.Text = _currentAsset.Description ?? "";
            CreatorIdTextBox.Text = _currentAsset.CreatorId.ToString();
            RobuxPriceTextBox.Text = _currentAsset.Price ?? "0";
            TixPriceTextBox.Text = _currentAsset.TixPrice ?? "0";
            IsLimitedCheckBox.IsChecked = _currentAsset.IsLimited;
            PutOnSaleCheckBox.IsChecked = _currentAsset.PutOnSale;
            
            if (_currentAsset.IsLimited)
            {
                LimitedFieldsGrid.IsEnabled = true;
                LimitedQuantityTextBox.Text = _currentAsset.LimitedQuantity?.ToString() ?? "0";
                LimitedUntilDatePicker.SelectedDate = _currentAsset.LimitedUntil;
            }
            
        }
        
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentAsset == null) return;
            
            try
            {
                if (string.IsNullOrWhiteSpace(AssetNameTextBox.Text))
                {
                    MessageBox.Show("Asset name cannot be empty.", "Validation Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    AssetNameTextBox.Focus();
                    return;
                }
                
                if (!long.TryParse(CreatorIdTextBox.Text.Trim(), out long creatorId) || creatorId < 0)
                {
                    MessageBox.Show("Please enter a valid Creator ID (non-negative number).", "Validation Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    CreatorIdTextBox.Focus();
                    return;
                }
                
                bool hasFileReplacement = !string.IsNullOrWhiteSpace(SelectedFileTextBox.Text) && 
                                        File.Exists(SelectedFileTextBox.Text);

                if (hasFileReplacement && IsImageBasedAsset(_currentAsset.AssetTypeId))
                {
                    if (!Common.FileUtilities.IsValidImageFile(SelectedFileTextBox.Text))
                    {
                        MessageBox.Show("The selected file is not a valid image file or is corrupted.", "Validation Error", 
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        SelectedFileTextBox.Focus();
                        return;
                    }
                    
                    if (!_assetService.IsFileFormatSupported(SelectedFileTextBox.Text, _currentAsset.AssetTypeId))
                    {
                        string supportedFormats;
                        switch (_currentAsset.AssetTypeId)
                        {
                            case 1:
                            case 2:
                            case 11:
                            case 12:
                                supportedFormats = "PNG, JPG, JPEG, BMP, GIF, TIFF, ICO";
                                break;
                            case 9:
                                supportedFormats = ".rbxl";
                                break;
                            default:
                                supportedFormats = "Unknown formats";
                                break;
                        }
                        MessageBox.Show($"File format not supported for this asset type. Supported formats: {supportedFormats}", "Validation Error", 
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        SelectedFileTextBox.Focus();
                        return;
                    }
                }
                
                SaveButton.Content = "Saving...";
                SaveButton.IsEnabled = false;
                
                try
                {
                    _currentAsset.Name = AssetNameTextBox.Text.Trim();
                    _currentAsset.Description = AssetDescriptionTextBox.Text.Trim();
                    _currentAsset.Creator = creatorId.ToString();
                    _currentAsset.Price = RobuxPriceTextBox.Text.Trim();
                    _currentAsset.TixPrice = TixPriceTextBox.Text.Trim();
                    _currentAsset.IsLimited = IsLimitedCheckBox.IsChecked == true;
                    _currentAsset.PutOnSale = PutOnSaleCheckBox.IsChecked == true;
                    
                    if (_currentAsset.IsLimited)
                    {
                        if (long.TryParse(LimitedQuantityTextBox.Text.Trim(), out long quantity))
                        {
                            _currentAsset.LimitedQuantity = quantity;
                            _currentAsset.LimitedRemaining = quantity; // Set remaining equal to quantity for new items
                        }
                        _currentAsset.LimitedUntil = LimitedUntilDatePicker.SelectedDate;
                    }
                    else
                    {
                        _currentAsset.LimitedQuantity = null;
                        _currentAsset.LimitedRemaining = null;
                        _currentAsset.LimitedUntil = null;
                    }
                    
                    if (hasFileReplacement)
                    {
                        try
                        {
                            
                            bool replacementSuccess = await _assetService.ReplaceAssetAsync(
                                _currentAsset.Id, 
                                SelectedFileTextBox.Text, 
                                _currentAsset.Name);
                            
                            if (!replacementSuccess)
                            {
                                MessageBox.Show("Failed to replace asset file. Metadata will still be updated.", "Warning", 
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error replacing asset file: {ex.Message}\n\nMetadata will still be updated.", "Warning", 
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    
                    bool success = await _assetService.UpdateAssetAsync(_currentAsset);
                    
                    if (success)
                    {
                        MessageBox.Show("Asset updated successfully!", "Success", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        this.DialogResult = true;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Failed to update asset metadata. Please try again.", "Error", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                finally
                {
                    SaveButton.Content = "Save";
                    SaveButton.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving asset: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                
                SaveButton.Content = "Save";
                SaveButton.IsEnabled = true;
            }
        }
        
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
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
            if (_currentAsset == null) return;
            
            string filter;
            switch (_currentAsset.AssetTypeId)
            {
                case 1:
                case 2:
                case 11:
                case 12:
                    filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tiff;*.ico)|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tiff;*.ico|All files (*.*)|*.*";
                    break;
                case 9:
                    filter = "Roblox Place Files (*.rbxl)|*.rbxl|All files (*.*)|*.*";
                    break;
                default:
                    filter = "All files (*.*)|*.*";
                    break;
            }
            
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select Asset File",
                Filter = filter,
                FilterIndex = 1
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SelectedFileTextBox.Text = openFileDialog.FileName;
            }
        }
        
        /// <summary>
        /// Checks if an asset type is image-based
        /// </summary>
        /// <param name="assetTypeId">Asset type ID</param>
        /// <returns>True if image-based, false otherwise</returns>
        private bool IsImageBasedAsset(int assetTypeId)
        {
            return assetTypeId == 1 || // Decal
                   assetTypeId == 2 || // T-Shirt
                   assetTypeId == 11 || // Shirt
                   assetTypeId == 12;   // Pants
        }
        
        /// <summary>
        /// Opens the asset edit window for a specific asset
        /// </summary>
        /// <param name="assetId">The ID of the asset to edit</param>
        /// <returns>True if changes were saved, false if cancelled</returns>
        public static bool EditAsset(long assetId)
        {
            var editWindow = new AssetEditWindow(assetId);
            var result = editWindow.ShowDialog();
            return result == true;
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
