using System;
using System.Windows;
using ControlPanel.Functions;

namespace Control_Panel
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main : Window
    {
        private SimpleViewLoader _viewLoader;
        
        public SimpleViewLoader ViewLoader => _viewLoader;
        
        public Main()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            
            InitializeComponent();
            ThemeManager.InitializeThemeForWindow(this);
            StatusTextBlock.Text = $"Ready - v{version}";
            _viewLoader = new SimpleViewLoader(ViewContainer, StatusTextBlock);
            _viewLoader.LoadView("Dashboard");
        }
        
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }
        
        public void SwitchToView(string viewName)
        {
            _viewLoader.LoadView(viewName);
        }
        
        public void OpenSettings()
        {
            GlobalSettingsOverlay.Show();
        }
        
        public void ShowAbout()
        {
            AboutMenuItem_Click(this, new RoutedEventArgs());
        }
        
        private void ConsoleMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var consoleWindow = ConsoleWindow.Instance;
            App.ShowWindowWithShutdownHandling(consoleWindow);
            consoleWindow.WriteLine("Console window opened", "SYSTEM");
        }
        
        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            GlobalSettingsOverlay.Show();
        }
        
        private void ServiceSetupMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ServiceSetupWindow serviceSetupWindow = new ServiceSetupWindow();
            App.ShowWindowWithShutdownHandling(serviceSetupWindow);
        }
        
        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var version = assembly.GetName().Version;
                var fileVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion;
                
                string versionInfo = $"Control Panel v{version}\n\n";
                versionInfo += $"Build Date: {System.IO.File.GetLastWriteTime(assembly.Location):yyyy-MM-dd HH:mm:ss}\n\n";
                versionInfo += "Roblox Webserver Management Tool\n\n© 2026 Roblox Webserver Project";
                
                MessageBox.Show(versionInfo, "About Control Panel", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting version info: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
