using System;
using System.Windows;
using System.Windows.Controls;
using Control_Panel;
using ControlPanel.Functions;

namespace ControlPanel.Functions
{
    /// <summary>
    /// Simple view loader for loading XAML views into a ContentControl
    /// </summary>
    public class SimpleViewLoader
    {
        private readonly ContentControl _viewContainer;
        private readonly TextBlock _statusTextBlock;
        
        public SimpleViewLoader(ContentControl viewContainer, TextBlock statusTextBlock)
        {
            if (viewContainer == null)
                throw new ArgumentNullException(nameof(viewContainer));
            if (statusTextBlock == null)
                throw new ArgumentNullException(nameof(statusTextBlock));
                
            _viewContainer = viewContainer;
            _statusTextBlock = statusTextBlock;
        }
        
        /// <summary>
        /// Load a view by name into the container
        /// </summary>
        /// <param name="viewName">Name of the view to load</param>
        public void LoadView(string viewName)
        {
            try
            {
                _statusTextBlock.Text = $"Loading {viewName}...";
                
                UserControl view = null;
                
                switch (viewName)
                {
                    case "Dashboard":
                        view = new DashboardView();
                        break;
                    case "Database":
                        view = new Database();
                        break;
                    default:
                        _statusTextBlock.Text = $"Unknown view: {viewName}";
                        return;
                }
                
                if (view != null)
                {
                    ApplyThemeToUserControl(view);
                    
                    _viewContainer.Content = view;
                    _statusTextBlock.Text = $"Switched to {viewName} view";
                    var parentWindow = Window.GetWindow(_viewContainer);
                    if (parentWindow != null)
                    {
                        ThemeManager.InitializeThemeForWindow(parentWindow);
                    }
                }
            }
            catch (Exception ex)
            {
                _statusTextBlock.Text = $"Error loading {viewName}: {ex.Message}";
                MessageBox.Show($"Failed to load {viewName} view: {ex.Message}", "View Loading Error", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// Update the status text in the main window
        /// </summary>
        /// <param name="status">The status message to display</param>
        public void UpdateStatus(string status)
        {
            if (_statusTextBlock != null)
            {
                _statusTextBlock.Text = status;
            }
        }
        
        /// <summary>
        /// Apply theme resources to a user control
        /// </summary>
        /// <param name="userControl">The user control to apply theme to</param>
        private void ApplyThemeToUserControl(UserControl userControl)
        {
            try
            {
                var themeSettings = ThemeManager.LoadThemeSettings();
                userControl.Resources.MergedDictionaries.Clear();

                LoadThemeResource(userControl, themeSettings.Theme);
                LoadColorResource(userControl, themeSettings.ColorScheme);
                LoadBackgroundResourceOnly(userControl, themeSettings.BackgroundColor);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to apply theme to {userControl.GetType().Name}: {ex.Message}");
                LoadThemeResource(userControl, "DefaultTheme");
                LoadColorResource(userControl, "Blue");
                LoadBackgroundResourceOnly(userControl, "White");
            }
        }
        
        private void LoadThemeResource(UserControl userControl, string theme)
        {
            string themeFile;
            switch (theme)
            {
                case "DefaultTheme":
                    themeFile = "Styles/Themes/DefaultTheme.xaml";
                    break;
                case "WPTheme":
                    themeFile = "Styles/Themes/WPTheme.xaml";
                    break;
                default:
                    themeFile = "Styles/Themes/DefaultTheme.xaml";
                    break;
            }

            try
            {
                var themeDict = new ResourceDictionary();
                themeDict.Source = new Uri(themeFile, UriKind.Relative);
                userControl.Resources.MergedDictionaries.Add(themeDict);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load theme {themeFile}: {ex.Message}");
                var fallbackDict = new ResourceDictionary();
                fallbackDict.Source = new Uri("Styles/Themes/DefaultTheme.xaml", UriKind.Relative);
                userControl.Resources.MergedDictionaries.Add(fallbackDict);
            }
        }

        private void LoadColorResource(UserControl userControl, string color)
        {
            try
            {
                var colorDict = new ResourceDictionary();
                colorDict.Source = new Uri($"Styles/Colors/{color}.xaml", UriKind.Relative);
                userControl.Resources.MergedDictionaries.Add(colorDict);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load color {color}: {ex.Message}");
            }
        }

        private void LoadBackgroundResourceOnly(UserControl userControl, string background)
        {
            try
            {
                var backgroundDict = ThemeManager.CreateBackgroundResourceDictionary(background);
                userControl.Resources.MergedDictionaries.Add(backgroundDict);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load background {background}: {ex.Message}");
            }
        }
    }
}
