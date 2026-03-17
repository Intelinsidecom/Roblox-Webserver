using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ControlPanel.Functions;
using Control_Panel.Views;
using Users;

namespace Control_Panel
{
    /// <summary>
    /// Interaction logic for UsersView.xaml
    /// </summary>
    public partial class UsersView : UserControl
    {
        private UserSearchService userSearchService;

        public UsersView()
        {
            InitializeComponent();
            
            if (!DatabaseConnectionWindow.EnsureDatabaseAccessible(Window.GetWindow(this)))
            {
                return;
            }
            
            string connectionString = GetConnectionString();
            userSearchService = new UserSearchService(connectionString);
            InitializePlaceholders();
        }

        /// <summary>
        /// Sets the username search field and triggers a search
        /// </summary>
        /// <param name="username">The username to search for</param>
        public void SearchForUser(string username)
        {
            if (!string.IsNullOrEmpty(username))
            {
                ClearPlaceholder(UsernameSearchTextBox);
                UsernameSearchTextBox.Text = username;
                UsernameSearchTextBox_TextChanged(UsernameSearchTextBox, null);
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

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            UsernameSearchTextBox.Clear();
            IdSearchTextBox.Clear();
            ResultsContainer.Visibility = Visibility.Collapsed;
            
            SetPlaceholder(UsernameSearchTextBox);
            SetPlaceholder(IdSearchTextBox);
        }

        private async void UsernameSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (UsernameSearchTextBox.Text == UsernameSearchTextBox.Tag as string)
            {
                ResultsContainer.Visibility = Visibility.Collapsed;
                return;
            }
            
            string searchTerm = UsernameSearchTextBox.Text.Trim();
            if (string.IsNullOrEmpty(searchTerm))
            {
                ResultsContainer.Visibility = Visibility.Collapsed;
                return;
            }

            try
            {
                var results = await userSearchService.SearchUsersByUsernameAsync(searchTerm);
                DisplaySearchResults(results, searchTerm, "username");
            }
            catch (Exception ex)
            {
                DisplayError($"Error searching users: {ex.Message}");
            }
        }

        private async void IdSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ResultsContainer.Visibility = Visibility.Collapsed;
        }

        private async void IdSearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                await HandleUserIdSearchAsync();
            }
        }

        private async Task HandleUserIdSearchAsync()
        {
            ResultsContainer.Visibility = Visibility.Collapsed;
            string userIdText = IdSearchTextBox.Text.Trim();
            if (string.IsNullOrEmpty(userIdText) || userIdText == IdSearchTextBox.Tag as string)
            {
                return;
            }

            if (!long.TryParse(userIdText, out long userId))
            {
                UpdateStatusInMainWindow($"Invalid user ID format: {userIdText}");
                return;
            }

            try
            {
                string connectionString = GetConnectionString();
                bool userExists = await UserQueries.UserExistsAsync(connectionString, userId);

                if (!userExists)
                {
                    UpdateStatusInMainWindow($"User ID - {userId} Not Found");
                    return;
                }

                UserManagementWindow.OpenUserManagement((int)userId);
            }
            catch (Exception ex)
            {
                UpdateStatusInMainWindow($"Error checking user existence: {ex.Message}");
            }
        }

        private void UpdateStatusInMainWindow(string status)
        {
            try
            {
                var mainWindow = Window.GetWindow(this) as Main;
                if (mainWindow?.ViewLoader != null)
                {
                    mainWindow.ViewLoader.UpdateStatus(status);
                }
            }
            catch (Exception ex)
            {
                DisplayError(status);
                System.Diagnostics.Debug.WriteLine($"Failed to update status bar: {ex.Message}");
            }
        }

        private void DisplaySearchResults(List<UserSearchResult> results, string searchTerm, string searchType)
        {
            if (results == null || results.Count == 0)
            {
                ResultsContainer.Visibility = Visibility.Visible;
                ResultsContainer.ItemsSource = new List<object> { 
                    new TextBlock {
                        Text = "No users found",
                        Style = (Style)FindResource("DashboardSubtitle"),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 50, 0, 0)
                    }
                };
                return;
            }

            ResultsContainer.Visibility = Visibility.Visible;
            ResultsContainer.ItemsSource = results;
        }

        private void DisplayError(string errorMessage)
        {
            ResultsContainer.Visibility = Visibility.Visible;
            ResultsContainer.ItemsSource = new List<object> { 
                new TextBlock {
                    Text = errorMessage,
                    Style = (Style)FindResource("DashboardSubtitle"),
                    Foreground = Brushes.Red,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 50, 0, 0)
                }
            };
        }
        
        private void InitializePlaceholders()
        {
            SetPlaceholder(UsernameSearchTextBox);
            SetPlaceholder(IdSearchTextBox);
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
        
        private void MoreInfoButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null && button.Tag != null)
            {
                int userId;
                if (int.TryParse(button.Tag.ToString(), out userId))
                {
                    UserManagementWindow.OpenUserManagement(userId);
                }
                else
                {
                    MessageBox.Show("Invalid user ID format.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        private void CreateUserButton_Click(object sender, RoutedEventArgs e)
        {
            UserCreateWindow createWindow = new UserCreateWindow();
            createWindow.Owner = Window.GetWindow(this);
            createWindow.ShowDialog();
        }
    }
}
