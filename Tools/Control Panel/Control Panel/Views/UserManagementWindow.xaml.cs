using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
            EmailVerifiedBadge.Visibility = Visibility.Collapsed;
            GenderText.Text = "";
            RobuxBalanceText.Text = "";
            TixBalanceText.Text = "";
            StatusText.Text = "";
            CreatedText.Text = "";
            MembershipText.Text = "";
            CollapseSocialFields();
            RobuxAmountTextBox.Text = "0";
            TixAmountTextBox.Text = "0";
        }
        
        private void CollapseSocialFields()
        {
            FacebookLabel.Visibility = Visibility.Collapsed;
            FacebookText.Visibility = Visibility.Collapsed;
            TwitterLabel.Visibility = Visibility.Collapsed;
            TwitterText.Visibility = Visibility.Collapsed;
            GooglePlusLabel.Visibility = Visibility.Collapsed;
            GooglePlusText.Visibility = Visibility.Collapsed;
            YouTubeLabel.Visibility = Visibility.Collapsed;
            YouTubeText.Visibility = Visibility.Collapsed;
            TwitchLabel.Visibility = Visibility.Collapsed;
            TwitchText.Visibility = Visibility.Collapsed;
            SocialNetworksLabel.Visibility = Visibility.Collapsed;
            SocialNetworksText.Visibility = Visibility.Collapsed;
        }
        
        private void UpdateUIWithUserData()
        {
            if (_currentUser == null) return;
            
            UserIdText.Text = _currentUser.UserId.ToString();
            UsernameText.Text = _currentUser.Username ?? "Unknown";
            RobuxBalanceText.Text = _currentUser.RobuxBalanceFormatted;
            TixBalanceText.Text = _currentUser.TixBalanceFormatted;
            
            var email = _currentUser.Email;
            if (string.IsNullOrEmpty(email))
            {
                EmailText.Text = "Not set";
                EmailVerifiedBadge.Visibility = Visibility.Collapsed;
            }
            else
            {
                EmailText.Text = email;
                if (_currentUser.EmailVerified)
                {
                    EmailVerifiedBadge.Text = "(Verified)";
                    EmailVerifiedBadge.Foreground = Brushes.Green;
                }
                else
                {
                    EmailVerifiedBadge.Text = "(Not Verified)";
                    EmailVerifiedBadge.Foreground = Brushes.Red;
                }
                EmailVerifiedBadge.Visibility = Visibility.Visible;
            }
            
            GenderText.Text = _currentUser.GenderText;
            StatusText.Text = _currentUser.StatusText;
            CreatedText.Text = _currentUser.CreatedDateFormatted;
            MembershipText.Text = _currentUser.MembershipText;

            bool hasAnySocial = !string.IsNullOrEmpty(_currentUser.Facebook)
                || !string.IsNullOrEmpty(_currentUser.Twitter)
                || !string.IsNullOrEmpty(_currentUser.GooglePlus)
                || !string.IsNullOrEmpty(_currentUser.YouTube)
                || !string.IsNullOrEmpty(_currentUser.Twitch);

            SetSocialField(FacebookLabel, FacebookText, _currentUser.Facebook);
            SetSocialField(TwitterLabel, TwitterText, _currentUser.Twitter);
            SetSocialField(GooglePlusLabel, GooglePlusText, _currentUser.GooglePlus);
            SetSocialField(YouTubeLabel, YouTubeText, _currentUser.YouTube);
            SetSocialField(TwitchLabel, TwitchText, _currentUser.Twitch);

            if (hasAnySocial)
            {
                SocialNetworksLabel.Visibility = Visibility.Visible;
                SocialNetworksText.Visibility = Visibility.Visible;
                SocialNetworksText.Text = SocialNetworksVisibilityText(_currentUser.SocialNetworksVisibility);
            }
            else
            {
                SocialNetworksLabel.Visibility = Visibility.Collapsed;
                SocialNetworksText.Visibility = Visibility.Collapsed;
            }
        }
        
        private void SetSocialField(TextBlock label, TextBlock value, string text)
        {
            bool hasValue = !string.IsNullOrWhiteSpace(text);
            label.Visibility = hasValue ? Visibility.Visible : Visibility.Collapsed;
            value.Visibility = hasValue ? Visibility.Visible : Visibility.Collapsed;
            if (hasValue) value.Text = text;
        }
        
        private string SocialNetworksVisibilityText(short visibility)
        {
            return visibility switch
            {
                6 => "Everyone",
                5 => "Friends, Following and Followers",
                4 => "Friends and Following",
                3 => "Friends",
                0 => "No one",
                _ => "Everyone"
            };
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
                EmailVerifiedBadge.Visibility = Visibility.Collapsed;
                GenderText.Text = "Loading...";
                StatusText.Text = "Loading...";
                CreatedText.Text = "Loading...";
                MembershipText.Text = "Loading...";
                CollapseSocialFields();
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
                    var cacheRepo = new Avatar.AvatarThumbnailCacheRepository();
                    await cacheRepo.WipeUserCacheAsync(connectionString, _currentUser.UserId);

                    // Clean up 3D avatar files on disk (model directories + map files)
                    await ClearUser3DAvatarCacheAsync(connectionString, _currentUser.UserId);
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
        
        private async Task ClearUser3DAvatarCacheAsync(string connectionString, long userId)
        {
            try
            {
                var solutionRoot = DatabaseUtilities.GetSolutionRoot();
                var websiteJsonPath = Path.Combine(solutionRoot, "Website", "appsettings.json");
                string avatar3dDir;

                if (File.Exists(websiteJsonPath))
                {
                    var json = File.ReadAllText(websiteJsonPath);
                    var match = System.Text.RegularExpressions.Regex.Match(json,
                        @"""Avatar3DDirectory""\s*:\s*""([^""]+)""");
                    avatar3dDir = match.Success ? match.Groups[1].Value : null;
                }
                else
                {
                    avatar3dDir = null;
                }

                if (string.IsNullOrWhiteSpace(avatar3dDir))
                    avatar3dDir = Path.Combine(solutionRoot, "CDN", "Assets", "3DAvatar");

                var mapsDir = Path.Combine(avatar3dDir, "maps");
                if (!Directory.Exists(mapsDir))
                    return;

                var mapFiles = Directory.GetFiles(mapsDir, $"{userId}_*.txt");
                if (mapFiles.Length == 0)
                    return;

                var repo = new Avatar.Avatar3DThumbnailCacheRepository();

                foreach (var mapFile in mapFiles)
                {
                    var modelHash = (await File.ReadAllTextAsync(mapFile)).Trim();
                    if (string.IsNullOrWhiteSpace(modelHash))
                        continue;

                    // Delete from SQL cache
                    await repo.DeleteByModelHashAsync(connectionString, modelHash);

                    // Delete the 3D avatar directory from disk
                    var avatarDir = Path.Combine(avatar3dDir, modelHash);
                    if (Directory.Exists(avatarDir))
                        Directory.Delete(avatarDir, recursive: true);

                    // Delete the map file
                    File.Delete(mapFile);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing 3D avatar cache for user {userId}: {ex.Message}");
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
        
        private async void EditMembershipButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null) return;

            var membershipWindow = new MembershipConfigWindow();
            membershipWindow.Owner = this;
            membershipWindow.SetCurrentMembership(_currentUser.MembershipType);

            if (membershipWindow.ShowDialog() == true)
            {
                try
                {
                    if (!DatabaseConnectionWindow.EnsureDatabaseAccessible(this, () => EditMembershipButton_Click(sender, (RoutedEventArgs)e)))
                    {
                        return;
                    }

                    var connectionString = DatabaseUtilities.GetConnectionString();
                    var service = new ControlPanel.Functions.UserManagementService(connectionString);
                    var success = await service.SetMembershipAsync(_currentUser.UserId, membershipWindow.MembershipStatus);

                    if (success)
                    {
                        await LoadUserDataAsync();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating membership: {ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        private async void EditSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null) return;

            var settingsWindow = new UserSettingsWindow(_currentUser.UserId);
            settingsWindow.Owner = this;
            settingsWindow.ShowDialog();
            await LoadUserDataAsync();
        }

        private async void AddRobuxButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null) return;

            if (!long.TryParse(RobuxAmountTextBox.Text, out long amount) || amount == 0)
            {
                MessageBox.Show("Please enter a valid non-zero number for Robux amount. Use a negative value to subtract.", "Invalid Input", 
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
            
            if (!long.TryParse(TixAmountTextBox.Text, out long amount) || amount == 0)
            {
                MessageBox.Show("Please enter a valid non-zero number for Tix amount. Use a negative value to subtract.", "Invalid Input", 
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
