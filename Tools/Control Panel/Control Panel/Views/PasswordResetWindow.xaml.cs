using System;
using System.Windows;

namespace Control_Panel.Views
{
    public partial class PasswordResetWindow : Window
    {
        public string Password { get; private set; }
        
        public PasswordResetWindow()
        {
            InitializeComponent();
        }
        
        private void RandomPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var password = new char[12];
            
            for (int i = 0; i < password.Length; i++)
            {
                password[i] = chars[random.Next(chars.Length)];
            }
            
            PasswordTextBox.Text = new string(password);
        }
        
        private void SetPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            Password = PasswordTextBox.Text;
            DialogResult = true;
            Close();
        }
        
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
