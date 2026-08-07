using System;
using System.Windows;
using System.Windows.Media;
using ControlPanel.Functions;
using Control_Panel.Properties;
using Users;

namespace Control_Panel.Views
{
    public partial class FeedBroadcastWindow : Window
    {
        private readonly UserManagementService _userManagementService;
        private readonly long _posterUserId;

        public FeedBroadcastWindow()
        {
            InitializeComponent();

            if (!DatabaseConnectionWindow.EnsureDatabaseAccessible(this, () => InitializeComponent()))
                return;

            _userManagementService = new UserManagementService(DatabaseUtilities.GetConnectionString());

            long.TryParse(Settings.Default.DefaultOwnerUserId, out _posterUserId);
            if (_posterUserId <= 0) _posterUserId = 1;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void PostButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!DatabaseConnectionWindow.EnsureDatabaseAccessible(this,
                    () => PostButton_Click(sender, (RoutedEventArgs)e)))
                    return;

                var message = MessageTextBox.Text?.Trim();
                if (string.IsNullOrEmpty(message))
                {
                    ShowStatus("Message cannot be empty.", false);
                    return;
                }

                PostButton.IsEnabled = false;
                var userIds = await _userManagementService.GetAllUserIdsAsync();
                int count = 0;
                foreach (var userId in userIds)
                {
                    try
                    {
                        await UserQueries.InsertFeedEntryAsync(
                            DatabaseUtilities.GetConnectionString(),
                            userId,
                            message,
                            feedType: 0,
                            posterUserId: _posterUserId);
                        count++;
                    }
                    catch (Exception ex)
                    {
                        ConsoleWindow.Instance?.WriteError(
                            $"Feed broadcast failed for user {userId}: {ex.Message}");
                    }
                }

                ShowStatus($"Posted to {count} of {userIds.Count} users' feeds.", count > 0);
            }
            catch (Exception ex)
            {
                ShowStatus($"Broadcast failed: {ex.Message}", false);
                ConsoleWindow.Instance?.WriteError($"Feed broadcast error: {ex.Message}");
            }
            finally
            {
                PostButton.IsEnabled = true;
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
