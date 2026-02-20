using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Control_Panel.Properties;
using ControlPanel.Functions;

namespace Control_Panel
{
    /// <summary>
    /// Interaction logic for SettingsOverlay.xaml
    /// </summary>
    public partial class SettingsOverlay : UserControl
    {
        private string currentTheme = "DefaultTheme";
        private string currentColor = "Blue";
        private string currentBackground = "White";
        private bool isInitializing = true;
        
        public SettingsOverlay()
        {
            InitializeComponent();
            LoadPreferences();
        }
        
        public void Show()
        {
            LoadPreferences();
            GlobalSettingsOverlay.Visibility = Visibility.Visible;
            LoadCurrentTheme();
        }
        
        private void LoadPreferences()
        {
            try
            {
                var settings = ThemeManager.LoadThemeSettings();
                currentTheme = settings.Theme;
                currentColor = settings.ColorScheme;
                currentBackground = settings.BackgroundColor;
            }
            catch
            {
                currentTheme = "DefaultTheme";
                currentColor = "Blue";
                currentBackground = "White";
            }
        }
        
        private void SavePreferences()
        {
            ThemeManager.SaveToSettings(currentTheme, currentColor, currentBackground);
        }
        
        public void Hide()
        {
            GlobalSettingsOverlay.Visibility = Visibility.Collapsed;
        }
        
        private void LoadCurrentTheme()
        {
            isInitializing = true;
            DefaultThemeRadioButton.IsChecked = (currentTheme == "DefaultTheme");
            WPThemeRadioButton.IsChecked = (currentTheme == "WPTheme");
            BlueColorRadioButton.IsChecked = (currentColor == "Blue");
            PurpleColorRadioButton.IsChecked = (currentColor == "Purple");
            GreenColorRadioButton.IsChecked = (currentColor == "Green");
            OrangeColorRadioButton.IsChecked = (currentColor == "Orange");
            WhiteBackgroundRadioButton.IsChecked = (currentBackground == "White");
            DarkBackgroundRadioButton.IsChecked = (currentBackground == "Dark");
            isInitializing = false;
        }
        
        private void LoadTheme(string theme, string color)
        {
            foreach (Window window in App.Current.Windows)
            {
                ThemeManager.ApplyThemeToWindow(window, theme, color, currentBackground);
            }
        }
        
        private void GlobalSettingsOverlay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Hide();
        }
        
        private void CloseGlobalSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }
        
        private void ThemeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (DefaultThemeRadioButton == null) return;
            string selectedTheme = DefaultThemeRadioButton.IsChecked == true ? "DefaultTheme" : "WPTheme";
            LoadTheme(selectedTheme, currentColor);
            currentTheme = selectedTheme;
        }
        
        private void ColorRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (isInitializing || BlueColorRadioButton == null) return;
            string selectedColor = "Blue";
            if (PurpleColorRadioButton.IsChecked == true) selectedColor = "Purple";
            else if (GreenColorRadioButton.IsChecked == true) selectedColor = "Green";
            else if (OrangeColorRadioButton.IsChecked == true) selectedColor = "Orange";
            
            LoadTheme(currentTheme, selectedColor);
            currentColor = selectedColor;
        }
        
        private void BackgroundColorRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (isInitializing || WhiteBackgroundRadioButton == null) return;
                
            string selectedBackground = "White";
            if (DarkBackgroundRadioButton.IsChecked == true) selectedBackground = "Dark";
            
            currentBackground = selectedBackground;
            LoadTheme(currentTheme, currentColor);
        }
        
        private void ApplySettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SavePreferences();
            Hide();
        }
        
        private void CancelSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }
}
