using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Control_Panel;
using ControlPanel.Functions;

namespace Control_Panel.Views
{
    public partial class UserManagementWindow : Window
    {
        private readonly int _userId;
        private readonly UserManagementService _userManagementService;
        private UserData _currentUser;
        private bool _isHeadshotMode = false;
        
        public UserManagementWindow()
        {
            InitializeComponent();
            
            AvatarToggleButton.Content = _isHeadshotMode ? "Headshot" : "Full Body";
            
            if (!DatabaseConnectionWindow.EnsureDatabaseAccessible(this, () => LoadUserDataAsync()))
            {
                return;
            }
            
            var connectionString = DatabaseUtilities.GetConnectionString();
            _userManagementService = new UserManagementService(connectionString);
        }
        
        public UserManagementWindow(int userId) : this()
        {
            _userId = userId;
            Loaded += async (sender, e) => await LoadUserDataAsync();
            Closing += (sender, e) => ClearUserData();
        }
        
        private async Task LoadUserDataAsync()
        {
            try
            {
                SetLoadingState(true);
                _currentUser = await _userManagementService.GetUserByIdAsync(_userId);
                
                if (_currentUser == null)
                {
                    MessageBox.Show($"User with ID {_userId} not found.", "User Not Found", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    this.Close();
                    return;
                }
                
                UpdateUIWithUserData();
                await LoadAvatarImageAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading user data: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SetLoadingState(false);
            }
        }
        
        private void ClearUserData()
        {
            _currentUser = null;
            AvatarImage.Source = null;
            UserIdText.Text = "";
            UsernameText.Text = "";
            EmailText.Text = "";
            RobuxBalanceText.Text = "";
            TixBalanceText.Text = "";
            StatusText.Text = "";
            CreatedText.Text = "";
            RobuxAmountTextBox.Text = "0";
            TixAmountTextBox.Text = "0";
        }
        
        private void UpdateUIWithUserData()
        {
            if (_currentUser == null) return;
            
            UserIdText.Text = _currentUser.UserId.ToString();
            UsernameText.Text = _currentUser.Username ?? "Unknown";
            RobuxBalanceText.Text = _currentUser.RobuxBalanceFormatted;
            TixBalanceText.Text = _currentUser.TixBalanceFormatted;
            EmailText.Text = _currentUser.Email ?? "Not set";
            StatusText.Text = _currentUser.StatusText;
            CreatedText.Text = _currentUser.CreatedDateFormatted;
        }
        
        private void SetLoadingState(bool isLoading)
        {
            if (isLoading)
            {
                UserIdText.Text = "Loading...";
                UsernameText.Text = "Loading...";
                RobuxBalanceText.Text = "Loading...";
                TixBalanceText.Text = "Loading...";
                EmailText.Text = "Loading...";
                StatusText.Text = "Loading...";
                CreatedText.Text = "Loading...";
            }
        }
        
        private async Task LoadAvatarImageAsync()
        {
            try
            {
                var settings = Properties.Settings.Default;
                string websiteHost = !string.IsNullOrEmpty(settings.WebsiteHost) ? settings.WebsiteHost : "localhost";
                string websitePort = !string.IsNullOrEmpty(settings.WebsitePort) ? settings.WebsitePort : "5000";
                string baseUrl = websiteHost.StartsWith("http") ? websiteHost : $"http://{websiteHost}";
                if (!string.IsNullOrEmpty(websitePort) && websitePort != "80" && websitePort != "443")
                {
                    baseUrl += $":{websitePort}";
                }

                string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                string avatarUrl;
                
                if (_isHeadshotMode)
                {
                    avatarUrl = $"{baseUrl}/headshot-thumbnail/image?userId={_userId}&width=420&height=420&t={timestamp}";
                }
                else
                {
                    avatarUrl = $"{baseUrl}/control-panel/avatar?userId={_userId}&width=420&height=420&t={timestamp}";
                }
                
                if (!string.IsNullOrEmpty(avatarUrl))
                {
                    try
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(avatarUrl);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        AvatarImage.Source = bitmap;
                        
                        ConsoleWindow.Instance?.WriteLine($"[Avatar Debug] Successfully loaded avatar image");
                    }
                    catch (Exception imgEx)
                    {
                        ConsoleWindow.Instance?.WriteError($"[Avatar Debug] Failed to load image from website: {imgEx.Message}");
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
                ConsoleWindow.Instance?.WriteError($"[Avatar Debug] Failed to load avatar: {ex.Message}");
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
            
            var border = (System.Windows.Controls.Border)AvatarImage.Parent;
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
            
            var border = (System.Windows.Controls.Border)AvatarImage.Parent;
            border.Child = textBlock;
        }
        
        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadUserDataAsync();
        }
        
        private async void ResetAvatarButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null) return;
            
            var result = MessageBox.Show($"Are you sure you want to reset {_currentUser.Username}'s avatar? " +
                "This will remove all equipped items and reset body colors to defaults.", 
                "Confirm Avatar Reset", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result != MessageBoxResult.Yes)
                return;
            
            try
            {
                if (!DatabaseConnectionWindow.EnsureDatabaseAccessible(this, () => ResetAvatarButton_Click(sender, (RoutedEventArgs)e)))
                {
                    return;
                }
                
                var connectionString = DatabaseUtilities.GetConnectionString();
                var success = await Users.UserQueries.ResetAvatarAsync(connectionString, _currentUser.UserId);
                
                if (success)
                {
                    await Thumbnails.ThumbnailQueries.ClearUserThumbnailUrlsAsync(connectionString, _currentUser.UserId);
                }
                
                if (success)
                {
                    MessageBox.Show($"Successfully reset {_currentUser.Username}'s avatar!", 
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadAvatarImageAsync();
                    await LoadUserDataAsync();
                }
                else
                {
                    MessageBox.Show("Failed to reset avatar. User may not exist.", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error resetting avatar: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async void ResetPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null) return;
            
            var passwordWindow = new PasswordResetWindow();
            passwordWindow.Owner = this;
            
            if (passwordWindow.ShowDialog() == true)
            {
                try
                {
                    var newPassword = passwordWindow.Password;
                    var success = await _userManagementService.ResetUserPasswordAsync(_currentUser.UserId, newPassword);
                    
                    if (success)
                    {
                        MessageBox.Show($"Password updated successfully!\n\nNew password: {newPassword}\n\n" +
                            "Please provide this password to the user and advise them to change it.", 
                            "Password Update Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to update password. Please try again.", 
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating password: {ex.Message}", 
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        private void BanUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null) return;
            
            MessageBox.Show("Ban User functionality to be implemented.", "Coming Soon", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private void ViewLogsButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null) return;
            MessageBox.Show($"View logs functionality for user {_currentUser.Username} to be implemented.", 
                "Coming Soon", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private async void AddRobuxButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null) return;
            
            if (!long.TryParse(RobuxAmountTextBox.Text, out long amount) || amount < 0)
            {
                MessageBox.Show("Please enter a valid positive number for Robux amount.", "Invalid Input", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            try
            {
                if (!DatabaseConnectionWindow.EnsureDatabaseAccessible(this, () => AddRobuxButton_Click(sender, (RoutedEventArgs)e)))
                {
                    return;
                }
                
                var connectionString = DatabaseUtilities.GetConnectionString();
                var success = await Users.UserQueries.IncrementCurrencyByIdAsync(connectionString, _currentUser.UserId, "robux", amount);
                
                if (success)
                {
                    await LoadUserDataAsync();
                    RobuxAmountTextBox.Text = "0";
                }
                else
                {
                    MessageBox.Show("Failed to add Robux. User may not exist.", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding Robux: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async void AddTixButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null) return;
            
            if (!long.TryParse(TixAmountTextBox.Text, out long amount) || amount < 0)
            {
                MessageBox.Show("Please enter a valid positive number for Tix amount.", "Invalid Input", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            try
            {
                if (!DatabaseConnectionWindow.EnsureDatabaseAccessible(this, () => AddTixButton_Click(sender, (RoutedEventArgs)e)))
                {
                    return;
                }
                
                var connectionString = DatabaseUtilities.GetConnectionString();
                var success = await Users.UserQueries.IncrementCurrencyByIdAsync(connectionString, _currentUser.UserId, "tix", amount);
                
                if (success)
                {
                    await LoadUserDataAsync();
                    TixAmountTextBox.Text = "0";
                }
                else
                {
                    MessageBox.Show("Failed to add Tix. User may not exist.", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding Tix: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        
        private async void AvatarToggleButton_Click(object sender, RoutedEventArgs e)
        {
            _isHeadshotMode = !_isHeadshotMode;
            AvatarToggleButton.Content = _isHeadshotMode ? "Headshot" : "Full Body";
            await LoadAvatarImageAsync();
        }
        
        /// <summary>
        /// Opens the User Management window for the specified user ID
        /// </summary>
        /// <param name="userId">The ID of the user to manage</param>
        /// <returns>The UserManagementWindow instance</returns>
        public static UserManagementWindow OpenUserManagement(int userId)
        {
            if (userId <= 0)
            {
                MessageBox.Show("Invalid user ID provided.", "Invalid Input", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }
            
            var window = new UserManagementWindow(userId);
            window.Show();
            return window;
        }
    }
    
    public static class StringExtensions
    {
        public static string Capitalize(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;
            
            return char.ToUpper(str[0]) + str.Substring(1).ToLower();
        }
    }
}
