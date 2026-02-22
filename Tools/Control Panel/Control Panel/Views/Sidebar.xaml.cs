using System;
using System.Windows;
using System.Windows.Controls;
using ControlPanel.Functions;

namespace Control_Panel
{
    /// <summary>
    /// Interaction logic for Sidebar.xaml
    /// </summary>
    public partial class Sidebar : UserControl
    {
        public Sidebar()
        {
            InitializeComponent();
            this.Loaded += Sidebar_Loaded;
        }
        
        private void Sidebar_Loaded(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                ThemeManager.InitializeThemeForWindow(parentWindow);
            }
        }
        
        private void ShowFeatureComingSoon(string featureName)
        {
            MessageBox.Show($"{featureName} clicked - Feature coming soon!", "Control Panel", 
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as Main;
            if (mainWindow != null)
            {
                mainWindow.SwitchToView("Dashboard");
            }
        }
        
        private void DatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as Main;
            if (mainWindow != null)
            {
                mainWindow.SwitchToView("Database");
            }
        }
        
        private void UsersButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as Main;
            if (mainWindow != null)
            {
                mainWindow.SwitchToView("Users");
            }
        }
        
        private void GamesButton_Click(object sender, RoutedEventArgs e)
        {
            ShowFeatureComingSoon("Games");
        }
        
        private void AssetsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowFeatureComingSoon("Assets");
        }
        
        private void LogsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowFeatureComingSoon("Logs");
        }
        
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as Main;
            if (mainWindow != null)
            {
                mainWindow.OpenSettings();
            }
        }
        
        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as Main;
            if (mainWindow != null)
            {
                mainWindow.ShowAbout();
            }
        }
    }
}
