using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Control_Panel;
using Control_Panel.Properties;
using System.Text.Json;

namespace ControlPanel.Functions
{
    public class ThemeConfig
    {
        public string Theme { get; set; }
        public string ColorScheme { get; set; }
        public string BackgroundColor { get; set; }
    }

    public static class ThemeManager
    {
        private const string THEME_CONFIG_FOLDER = "ControlPanel";
        private const string THEME_CONFIG_FILE = "theme.json";
        private static bool _isApplyingTheme = false;

        public static void InitializeThemeForWindow(Window window)
        {
            if (_isApplyingTheme) return;
            
            _isApplyingTheme = true;
            try
            {
                var themeSettings = LoadThemeSettings();
                ApplyThemeToWindow(window, themeSettings.Theme, themeSettings.ColorScheme, themeSettings.BackgroundColor);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to initialize theme for {window.GetType().Name}: {ex.Message}");
                ApplyThemeToWindow(window, "DefaultTheme", "Blue", "White");
            }
            finally
            {
                _isApplyingTheme = false;
            }
        }

        public static ThemeSettings LoadThemeSettings()
        {
            var settings = new ThemeSettings();
            LoadFromApplicationSettings(ref settings);
            return settings;
        }

        public static void ApplyThemeToWindow(Window window, string theme, string color, string background)
        {
            try
            {
                window.Resources.MergedDictionaries.Clear();

                LoadThemeResource(window, theme);
                LoadColorResource(window, color);
                LoadBackgroundResourceOnly(window, background);
                ApplyThemeToUserControlsInWindow(window, theme, color, background);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to apply theme to {window.GetType().Name}: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply theme to all UserControls within a window (for live preview)
        /// </summary>
        private static void ApplyThemeToUserControlsInWindow(Window window, string theme, string color, string background)
        {
            try
            {
                var userControls = FindVisualChildren<UserControl>(window);
                
                foreach (var userControl in userControls)
                {
                    if (userControl is SettingsOverlay)
                        continue;
                        
                    ApplyThemeToUserControl(userControl, theme, color, background);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to apply theme to UserControls in {window.GetType().Name}: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply theme to a specific UserControl
        /// </summary>
        public static void ApplyThemeToUserControl(UserControl userControl, string theme, string color, string background)
        {
            try
            {
                userControl.Resources.MergedDictionaries.Clear();

                LoadThemeResourceToUserControl(userControl, theme);
                LoadColorResourceToUserControl(userControl, color);
                LoadBackgroundResourceOnlyToUserControl(userControl, background);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to apply theme to {userControl.GetType().Name}: {ex.Message}");
            }
        }

        /// <summary>
        /// Find all visual children of a specific type
        /// </summary>
        private static System.Collections.Generic.IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = System.Windows.Media.VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private static void LoadThemeResource(Window window, string theme)
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
                window.Resources.MergedDictionaries.Add(themeDict);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load theme {themeFile}: {ex.Message}");
                var fallbackDict = new ResourceDictionary();
                fallbackDict.Source = new Uri("Styles/Themes/DefaultTheme.xaml", UriKind.Relative);
                window.Resources.MergedDictionaries.Add(fallbackDict);
            }
        }

        private static void LoadThemeResourceToUserControl(UserControl userControl, string theme)
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
                System.Diagnostics.Debug.WriteLine($"Failed to load theme {themeFile} for UserControl: {ex.Message}");
                var fallbackDict = new ResourceDictionary();
                fallbackDict.Source = new Uri("Styles/Themes/DefaultTheme.xaml", UriKind.Relative);
                userControl.Resources.MergedDictionaries.Add(fallbackDict);
            }
        }

        private static void LoadColorResource(Window window, string color)
        {
            try
            {
                var colorDict = new ResourceDictionary();
                colorDict.Source = new Uri($"Styles/Colors/{color}.xaml", UriKind.Relative);
                window.Resources.MergedDictionaries.Add(colorDict);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load color {color}: {ex.Message}");
            }
        }

        private static void LoadColorResourceToUserControl(UserControl userControl, string color)
        {
            try
            {
                var colorDict = new ResourceDictionary();
                colorDict.Source = new Uri($"Styles/Colors/{color}.xaml", UriKind.Relative);
                userControl.Resources.MergedDictionaries.Add(colorDict);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load color {color} for UserControl: {ex.Message}");
            }
        }

        private static void LoadBackgroundResource(Window window, string background)
        {
            LoadBackgroundResourceOnly(window, background);
        }

        public static ResourceDictionary CreateBackgroundResourceDictionary(string background)
        {
            var backgroundDict = new ResourceDictionary();
            
            if (background == "Dark")
            {
                backgroundDict.Add("Background", new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(27, 27, 27)));
                backgroundDict.Add("BackgroundSecondary", new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(37, 37, 37)));
                backgroundDict.Add("Foreground", new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(248, 248, 248)));
                backgroundDict.Add("ForegroundLight", new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255)));
                backgroundDict.Add("SubtleText", new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(170, 170, 170)));
                backgroundDict.Add("BorderColor", new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(51, 51, 51)));
                backgroundDict.Add("SidebarBackground", new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(37, 37, 37)));
                backgroundDict.Add("SidebarHover", new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(74, 74, 74)));
                backgroundDict.Add("SidebarPressed", new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(90, 90, 90)));
            }
            else
            {
                backgroundDict.Add("Background", new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255)));
                backgroundDict.Add("BackgroundSecondary", new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(248, 248, 248)));
                backgroundDict.Add("Foreground", new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(27, 27, 27)));
                backgroundDict.Add("ForegroundLight", new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0)));
                backgroundDict.Add("SubtleText", new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(136, 136, 136)));
                backgroundDict.Add("BorderColor", new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(224, 224, 224)));
                backgroundDict.Add("SidebarBackground", new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(248, 248, 248)));
                backgroundDict.Add("SidebarHover", new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(224, 224, 224)));
                backgroundDict.Add("SidebarPressed", new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(208, 208, 208)));
            }
            
            return backgroundDict;
        }

        private static void LoadBackgroundResourceOnly(Window window, string background)
        {
            try
            {
                var backgroundDict = CreateBackgroundResourceDictionary(background);
                window.Resources.MergedDictionaries.Add(backgroundDict);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load background {background}: {ex.Message}");
            }
        }

        private static void LoadBackgroundResourceOnlyToUserControl(UserControl userControl, string background)
        {
            try
            {
                var backgroundDict = CreateBackgroundResourceDictionary(background);
                userControl.Resources.MergedDictionaries.Add(backgroundDict);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load background {background} for UserControl: {ex.Message}");
            }
        }

        private static void LoadFromApplicationSettings(ref ThemeSettings settings)
        {
            try
            {
                Settings.Default.Reload();
                settings.Theme = Settings.Default.Theme ?? "DefaultTheme";
                settings.ColorScheme = Settings.Default.ColorScheme ?? "Blue";
                settings.BackgroundColor = Settings.Default.BackgroundColor ?? "White";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load theme from settings: {ex.Message}");
                settings.Theme = "DefaultTheme";
                settings.ColorScheme = "Blue";
                settings.BackgroundColor = "White";
            }
        }

        public static void SaveToSettings(string theme, string color, string background)
        {
            if (_isApplyingTheme) return;
            
            try
            {
                Settings.Default.Theme = theme;
                Settings.Default.ColorScheme = color;
                Settings.Default.BackgroundColor = background;
                Settings.Default.Save();
                Settings.Default.Reload();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save theme to settings: {ex.Message}");
            }
        }
    }

    public class ThemeSettings
    {
        public string Theme { get; set; } = "DefaultTheme";
        public string ColorScheme { get; set; } = "Blue";
        public string BackgroundColor { get; set; } = "White";
    }
}
