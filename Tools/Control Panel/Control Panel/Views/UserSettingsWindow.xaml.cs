using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ControlPanel.Functions;
using Users;

namespace Control_Panel.Views
{
    public partial class UserSettingsWindow : Window
    {
        private readonly long _userId;
        private readonly string _connectionString;
        private readonly UserManagementService _userManagementService;
        private string _originalGender = "none";
        private DateTime? _originalBirthday;

        public UserSettingsWindow()
        {
            InitializeComponent();

            if (!DatabaseConnectionWindow.EnsureDatabaseAccessible(this, () => InitializeComponent()))
                return;

            _connectionString = DatabaseUtilities.GetConnectionString();
            _userManagementService = new UserManagementService(_connectionString);
        }

        public UserSettingsWindow(long userId) : this()
        {
            _userId = userId;
            Loaded += async (sender, e) => await LoadSettingsAsync();
        }

        private async Task LoadSettingsAsync()
        {
            try
            {
                SetLoadingState(true);

                var accountInfo = await UserQueries.GetAccountInfoAsync(_connectionString, _userId);
                var settings = await UserQueries.GetSettingsAsync(_connectionString, _userId);
                var statusText = await UserQueries.GetUserStatusTextAsync(_connectionString, _userId);
                var description = await _userManagementService.GetUserDescriptionAsync(_userId);
                var twoStep = await UserQueries.GetTwoStepEnabledAsync(_connectionString, _userId);
                var (pinEnabled, _) = await UserQueries.GetAccountPinAsync(_connectionString, _userId);
                var restrictions = await UserQueries.GetAccountRestrictionsEnabledAsync(_connectionString, _userId);

                UpdateEmailSection(accountInfo);

                StatusTextBox.Text = statusText ?? "";
                DescriptionTextBox.Text = description ?? "";

                var gender = await _userManagementService.GetGenderAsync(_userId);
                var birthday = await _userManagementService.GetBirthdayAsync(_userId);
                _originalGender = gender ?? "none";
                _originalBirthday = birthday;
                SelectGender(_originalGender);
                PopulateBirthday(_originalBirthday);

                FacebookTextBox.Text = settings.SocialFacebookUrl ?? "";
                TwitterTextBox.Text = settings.SocialTwitterUrl ?? "";
                GooglePlusTextBox.Text = settings.SocialGoogleplusUrl ?? "";
                YouTubeTextBox.Text = settings.SocialYoutubeUrl ?? "";
                TwitchTextBox.Text = settings.SocialTwitchUrl ?? "";
                SelectSocialVisibility(settings.SocialNetworksVisibility);

                TwoStepText.Text = twoStep ? "Enabled" : "Disabled";
                PinText.Text = pinEnabled ? "Enabled" : "Disabled";
                RestrictionsText.Text = restrictions ? "Enabled" : "Disabled";

                SelectComboByTag(AppChatPrivacyComboBox, settings.AppChatPrivacy);
                SelectComboByTag(GameChatPrivacyComboBox, settings.GameChatPrivacy);
                SelectComboByTag(PrivateMessagePrivacyComboBox, settings.PrivateMessagePrivacy);
                SelectComboByTag(PrivateServerInvitePrivacyComboBox, settings.PrivateServerInvitePrivacy);
                SelectComboByTag(FollowMePrivacyComboBox, settings.FollowMePrivacy);

                NewsletterText.Text = settings.ReceiveNewsletter ? "Enabled" : "Disabled";
            }
            catch (Exception ex)
            {
                ShowStatus($"Failed to load settings: {ex.Message}", false);
                ConsoleWindow.Instance?.WriteError($"Error loading settings for user {_userId}: {ex.Message}");
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        private void UpdateEmailSection(AccountInfo accountInfo)
        {
            var email = accountInfo?.Email;
            if (string.IsNullOrEmpty(email))
            {
                EmailText.Text = "Not set";
                EmailVerifiedBadge.Visibility = Visibility.Collapsed;
                VerifyEmailButton.Visibility = Visibility.Collapsed;
                UnverifyEmailButton.Visibility = Visibility.Collapsed;
                EditEmailButton.Visibility = Visibility.Visible;
                return;
            }

            EmailText.Text = email;
            if (accountInfo != null && accountInfo.EmailVerified)
            {
                EmailVerifiedBadge.Text = "(Verified)";
                EmailVerifiedBadge.Foreground = Brushes.Green;
                VerifyEmailButton.Visibility = Visibility.Collapsed;
                UnverifyEmailButton.Visibility = Visibility.Visible;
            }
            else
            {
                EmailVerifiedBadge.Text = "(Not Verified)";
                EmailVerifiedBadge.Foreground = Brushes.Red;
                VerifyEmailButton.Visibility = Visibility.Visible;
                UnverifyEmailButton.Visibility = Visibility.Collapsed;
            }
            EmailVerifiedBadge.Visibility = Visibility.Visible;
            EditEmailButton.Visibility = Visibility.Visible;
        }

        private async Task LoadEmailAsync()
        {
            var accountInfo = await UserQueries.GetAccountInfoAsync(_connectionString, _userId);
            UpdateEmailSection(accountInfo);
        }

        private async void VerifyEmailButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!DatabaseConnectionWindow.EnsureDatabaseAccessible(this,
                    () => VerifyEmailButton_Click(sender, (RoutedEventArgs)e)))
                    return;

                await EmailQueries.MarkEmailVerifiedAsync(_connectionString, _userId);
                await LoadEmailAsync();
                ShowStatus("Email verified.", true);
            }
            catch (Exception ex)
            {
                ShowStatus($"Failed to verify email: {ex.Message}", false);
                ConsoleWindow.Instance?.WriteError($"Error verifying email for user {_userId}: {ex.Message}");
            }
        }

        private async void UnverifyEmailButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!DatabaseConnectionWindow.EnsureDatabaseAccessible(this,
                    () => UnverifyEmailButton_Click(sender, (RoutedEventArgs)e)))
                    return;

                await _userManagementService.SetEmailVerifiedAsync(_userId, false);
                await LoadEmailAsync();
                ShowStatus("Email unverified.", true);
            }
            catch (Exception ex)
            {
                ShowStatus($"Failed to unverify email: {ex.Message}", false);
                ConsoleWindow.Instance?.WriteError($"Error unverifying email for user {_userId}: {ex.Message}");
            }
        }

        private async void EditEmailButton_Click(object sender, RoutedEventArgs e)
        {
            var currentEmail = EmailText.Text == "Not set" ? "" : EmailText.Text;
            var emailWindow = new EmailEditWindow(currentEmail);
            emailWindow.Owner = this;

            if (emailWindow.ShowDialog() == true)
            {
                try
                {
                    if (!DatabaseConnectionWindow.EnsureDatabaseAccessible(this,
                        () => EditEmailButton_Click(sender, (RoutedEventArgs)e)))
                        return;

                    await EmailQueries.UpdateEmailAsync(_connectionString, _userId, emailWindow.Email);
                    await LoadEmailAsync();
                    ShowStatus("Email updated.", true);
                }
                catch (Exception ex)
                {
                    ShowStatus($"Failed to update email: {ex.Message}", false);
                    ConsoleWindow.Instance?.WriteError($"Error updating email for user {_userId}: {ex.Message}");
                }
            }
        }

        private void SelectSocialVisibility(short visibility)
        {
            foreach (ComboBoxItem item in SocialVisibilityComboBox.Items)
            {
                if (item.Tag != null && Convert.ToInt16(item.Tag) == visibility)
                {
                    SocialVisibilityComboBox.SelectedItem = item;
                    return;
                }
            }
            SocialVisibilityComboBox.SelectedIndex = 0;
        }

        private short SelectedSocialVisibility()
        {
            if (SocialVisibilityComboBox.SelectedItem is ComboBoxItem item && item.Tag != null)
                return Convert.ToInt16(item.Tag);
            return 6;
        }

        private void SelectGender(string gender)
        {
            foreach (ComboBoxItem item in GenderComboBox.Items)
            {
                if (item.Tag != null && string.Equals(item.Tag.ToString(), gender, StringComparison.OrdinalIgnoreCase))
                {
                    GenderComboBox.SelectedItem = item;
                    return;
                }
            }
            GenderComboBox.SelectedIndex = 0;
        }

        private string SelectedGender()
        {
            if (GenderComboBox.SelectedItem is ComboBoxItem item && item.Tag != null)
                return item.Tag.ToString();
            return "none";
        }

        private void PopulateBirthday(DateTime? birthday)
        {
            string[] monthNames =
            {
                "January", "February", "March", "April", "May", "June",
                "July", "August", "September", "October", "November", "December"
            };

            BirthMonthComboBox.Items.Clear();
            for (int i = 0; i < monthNames.Length; i++)
            {
                var item = new ComboBoxItem { Content = monthNames[i], Tag = (i + 1).ToString() };
                BirthMonthComboBox.Items.Add(item);
            }

            BirthDayComboBox.Items.Clear();
            for (int day = 1; day <= 31; day++)
            {
                BirthDayComboBox.Items.Add(new ComboBoxItem { Content = day.ToString(), Tag = day.ToString() });
            }

            BirthYearComboBox.Items.Clear();
            for (int year = 1900; year <= DateTime.UtcNow.Year; year++)
            {
                BirthYearComboBox.Items.Add(new ComboBoxItem { Content = year.ToString(), Tag = year.ToString() });
            }

            SelectComboByTag(BirthMonthComboBox, (birthday?.Month ?? 1).ToString());
            SelectComboByTag(BirthDayComboBox, (birthday?.Day ?? 1).ToString());
            SelectComboByTag(BirthYearComboBox, (birthday?.Year ?? 2000).ToString());
        }

        private static void SelectComboByTag(ComboBox combo, string tag)
        {
            foreach (ComboBoxItem item in combo.Items)
            {
                if (item.Tag != null && item.Tag.ToString() == tag)
                {
                    combo.SelectedItem = item;
                    return;
                }
            }
            if (combo.Items.Count > 0)
                combo.SelectedIndex = 0;
        }

        private DateTime? SelectedBirthday()
        {
            int month = GetSelectedTag(BirthMonthComboBox, 1);
            int day = GetSelectedTag(BirthDayComboBox, 1);
            int year = GetSelectedTag(BirthYearComboBox, 2000);
            try
            {
                return new DateTime(year, month, day);
            }
            catch
            {
                return null;
            }
        }

        private static int GetSelectedTag(ComboBox combo, int fallback)
        {
            if (combo.SelectedItem is ComboBoxItem item && item.Tag != null &&
                int.TryParse(item.Tag.ToString(), out int value))
                return value;
            return fallback;
        }

        private static string GetSelectedComboTag(ComboBox combo, string fallback)
        {
            if (combo.SelectedItem is ComboBoxItem item && item.Tag != null)
                return item.Tag.ToString();
            return fallback;
        }

        private async void SaveInfoButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!DatabaseConnectionWindow.EnsureDatabaseAccessible(this,
                    () => SaveInfoButton_Click(sender, (RoutedEventArgs)e)))
                    return;

                await _userManagementService.UpdateUserDescriptionAsync(_userId, DescriptionTextBox.Text);
                await UserQueries.UpdateUserStatusTextAsync(_connectionString, _userId, StatusTextBox.Text);

                var newGender = SelectedGender();
                if (!string.Equals(newGender, _originalGender, StringComparison.OrdinalIgnoreCase))
                {
                    await _userManagementService.UpdateGenderAsync(_userId, newGender);
                    _originalGender = newGender;
                }

                var newBirthday = SelectedBirthday();
                if (newBirthday != _originalBirthday)
                {
                    await _userManagementService.UpdateBirthdayAsync(_userId, newBirthday);
                    _originalBirthday = newBirthday;
                }

                ShowStatus("Account info saved.", true);
            }
            catch (Exception ex)
            {
                ShowStatus($"Failed to save account info: {ex.Message}", false);
                ConsoleWindow.Instance?.WriteError($"Error saving account info for user {_userId}: {ex.Message}");
            }
        }

        private async void SaveSocialButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!DatabaseConnectionWindow.EnsureDatabaseAccessible(this,
                    () => SaveSocialButton_Click(sender, (RoutedEventArgs)e)))
                    return;

                await UserQueries.SetSocialNetworksAsync(_connectionString, _userId,
                    FacebookTextBox.Text, TwitterTextBox.Text, GooglePlusTextBox.Text,
                    YouTubeTextBox.Text, TwitchTextBox.Text, SelectedSocialVisibility());

                ShowStatus("Social networks saved.", true);
            }
            catch (Exception ex)
            {
                ShowStatus($"Failed to save social networks: {ex.Message}", false);
                ConsoleWindow.Instance?.WriteError($"Error saving social networks for user {_userId}: {ex.Message}");
            }
        }

        private async void SavePrivacyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!DatabaseConnectionWindow.EnsureDatabaseAccessible(this,
                    () => SavePrivacyButton_Click(sender, (RoutedEventArgs)e)))
                    return;

                await UserQueries.SetAppChatPrivacyAsync(_connectionString, _userId,
                    GetSelectedComboTag(AppChatPrivacyComboBox, "Friends"));
                await UserQueries.SetGameChatPrivacyAsync(_connectionString, _userId,
                    GetSelectedComboTag(GameChatPrivacyComboBox, "AllUsers"));
                await UserQueries.SetPrivateMessagePrivacyAsync(_connectionString, _userId,
                    GetSelectedComboTag(PrivateMessagePrivacyComboBox, "Friends"));
                await UserQueries.SetPrivateServerInvitePrivacyAsync(_connectionString, _userId,
                    GetSelectedComboTag(PrivateServerInvitePrivacyComboBox, "Friends"));
                await UserQueries.SetFollowMePrivacyAsync(_connectionString, _userId,
                    GetSelectedComboTag(FollowMePrivacyComboBox, "Friends"));

                ShowStatus("Privacy settings saved.", true);
            }
            catch (Exception ex)
            {
                ShowStatus($"Failed to save privacy settings: {ex.Message}", false);
                ConsoleWindow.Instance?.WriteError($"Error saving privacy settings for user {_userId}: {ex.Message}");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void SetLoadingState(bool isLoading)
        {
            if (isLoading)
            {
                EmailText.Text = "Loading...";
                EmailVerifiedBadge.Visibility = Visibility.Collapsed;
                VerifyEmailButton.Visibility = Visibility.Collapsed;
                UnverifyEmailButton.Visibility = Visibility.Collapsed;
                EditEmailButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                SaveInfoButton.IsEnabled = true;
                SaveSocialButton.IsEnabled = true;
                SavePrivacyButton.IsEnabled = true;
            }
        }

        private void ShowStatus(string message, bool isSuccess)
        {
            StatusText.Text = message;
            StatusText.Foreground = isSuccess ? Brushes.Green : Brushes.Red;
            StatusText.Visibility = Visibility.Visible;
        }
    }
}
