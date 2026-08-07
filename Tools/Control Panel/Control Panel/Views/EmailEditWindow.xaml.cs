using System.Windows;
using System.Windows.Media;

namespace Control_Panel.Views
{
    public partial class EmailEditWindow : Window
    {
        public string Email { get; private set; }

        public EmailEditWindow(string currentEmail)
        {
            InitializeComponent();

            EmailTextBox.Text = currentEmail ?? "";
            EmailTextBox.Focus();
            EmailTextBox.CaretIndex = EmailTextBox.Text.Length;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var email = EmailTextBox.Text?.Trim();
            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Email cannot be empty.", "Invalid Input",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Email = email;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
