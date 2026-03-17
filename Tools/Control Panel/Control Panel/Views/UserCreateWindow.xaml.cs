using System;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using Control_Panel;
using ControlPanel.Functions;

namespace Control_Panel.Views
{
    public partial class UserCreateWindow : Window
    {
        private readonly UserManagementService _userManagementService;
        
        public UserCreateWindow()
        {
            InitializeComponent();
            
            if (!DatabaseConnectionWindow.EnsureDatabaseAccessible(this, () => InitializeComponent()))
            {
                return;
            }
            
            var connectionString = DatabaseUtilities.GetConnectionString();
            _userManagementService = new UserManagementService(connectionString);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private async void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
            {
                ShowStatus("Please enter a username.", false);
                return;
            }

            if (string.IsNullOrWhiteSpace(PasswordTextBox.Text))
            {
                ShowStatus("Please enter a password.", false);
                return;
            }

            if (GenderComboBox.SelectedItem == null)
            {
                ShowStatus("Please select a gender.", false);
                return;
            }

            try
            {
                if (!DatabaseConnectionWindow.EnsureDatabaseAccessible(this, () => CreateButton_Click(sender, (RoutedEventArgs)e)))
                {
                    return;
                }

                string selectedGender = (GenderComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString()?.ToLowerInvariant() ?? "unknown";
                var result = await _userManagementService.CreateUserAsync(
                    UsernameTextBox.Text,
                    PasswordTextBox.Text,
                    selectedGender
                );

                if (result.success)
                {
                    ShowStatus($"User {result.userId} Created Successfully", true);
                }
                else
                {
                    ShowStatus("User Creation Failed", false);
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"User Creation Failed", false);
                ConsoleWindow.Instance?.WriteError($"Error creating user: {ex.Message}");
            }
        }

        private void RandomPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            PasswordTextBox.Text = GenerateRandomPassword(12);
        }

        private string GenerateRandomPassword(int length)
        {
            const string lowerCase = "abcdefghijklmnopqrstuvwxyz";
            const string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string numbers = "0123456789";
            const string specialChars = "!@#$%^&*()";

            string allChars = lowerCase + upperCase + numbers + specialChars;
            Random random = new Random();
            char[] password = new char[length];

            for (int i = 0; i < length; i++)
            {
                password[i] = allChars[random.Next(allChars.Length)];
            }

            return new string(password);
        }

        private void ShowStatus(string message, bool isSuccess)
        {
            StatusText.Text = message;
            StatusText.Foreground = isSuccess ? 
                System.Windows.Media.Brushes.Green : 
                System.Windows.Media.Brushes.Red;
            StatusText.Visibility = Visibility.Visible;
        }
    }
}
