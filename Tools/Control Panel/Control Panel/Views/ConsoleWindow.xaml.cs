using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Control_Panel.Properties;
using ControlPanel.Functions;

namespace Control_Panel
{
    /// <summary>
    /// Interaction logic for ConsoleWindow.xaml
    /// </summary>
    public partial class ConsoleWindow : Window
    {
        private static readonly object _lock = new object();
        private string _consoleName;
        public static ConsoleWindow Instance
        {
            get
            {
                return ConsoleWindowManager.GlobalConsole;
            }
        }
        
        public ConsoleWindow(string consoleName)
        {
            _consoleName = consoleName;
            InitializeComponent();
            this.Icon = System.Windows.Application.Current.MainWindow?.Icon ?? 
                new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/ControlPanel.ico"));
            ThemeManager.InitializeThemeForWindow(this);
            UpdateWindowTitle();
            this.Loaded += ConsoleWindow_Loaded;
        }
        
        private ConsoleWindow()
        {
            _consoleName = "Global Console";
            InitializeComponent();
            this.Icon = System.Windows.Application.Current.MainWindow?.Icon ?? 
                new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/ControlPanel.ico"));
            ThemeManager.InitializeThemeForWindow(this);
            UpdateWindowTitle();
            this.Loaded += ConsoleWindow_Loaded;
        }
        
        private void UpdateWindowTitle()
        {
            Title = $"Console - {_consoleName}";
        }
        
        private void ConsoleWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.PropertyChanged += Settings_PropertyChanged;
        }
        
        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Theme" || e.PropertyName == "ColorScheme" || e.PropertyName == "BackgroundColor")
            {
                ThemeManager.InitializeThemeForWindow(this);
            }
        }
        
        public void WriteLine(string message)
        {
            WriteLine(message, null);
        }
        
        public void WriteLine(string message, string category)
        {
            Dispatcher.Invoke(() =>
            {
                string timestamp = DateTime.Now.ToString("HH:mm:ss");
                string formattedMessage;
                
                if (!string.IsNullOrEmpty(category))
                {
                    formattedMessage = $"[{timestamp}] [{category}] {message}";
                }
                else
                {
                    formattedMessage = $"[{timestamp}] {message}";
                }
                
                ConsoleTextBox.AppendText(formattedMessage + Environment.NewLine);
                ConsoleTextBox.ScrollToEnd();
                ConsoleTextBox.CaretIndex = ConsoleTextBox.Text.Length;
                ConsoleTextBox.Focus();
                
                if (ConsoleScrollViewer != null)
                {
                    ConsoleScrollViewer.ScrollToEnd();
                }
                
                UpdateStatus($"Last: {message.Substring(0, Math.Min(message.Length, 30))}...");
            });
        }
        
        public void WriteError(string message)
        {
            WriteLine($"ERROR: {message}", "ERROR");
        }
        
        public void WriteWarning(string message)
        {
            WriteLine($"WARNING: {message}", "WARNING");
        }
        
        public void WriteSQL(string sql)
        {
            WriteLine($"SQL: {sql}", "SQL");
        }
        
        public void WriteSuccess(string message)
        {
            WriteLine($"SUCCESS: {message}", "SUCCESS");
        }
        
        public void ClearOutput()
        {
            Dispatcher.Invoke(() =>
            {
                ConsoleTextBox.Clear();
                UpdateStatus("Console cleared");
            });
        }
        
        public string ConsoleName => _consoleName;
        
        private void UpdateStatus(string status)
        {
            StatusTextBlock.Text = $"| {status}";
        }
        
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ConsoleTextBox.Clear();
            UpdateStatus("Console cleared");
        }
        
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Text files (*.txt)|*.txt|Log files (*.log)|*.log|All files (*.*)|*.*",
                    FileName = $"console_output_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
                };
                
                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveFileDialog.FileName, ConsoleTextBox.Text);
                    UpdateStatus($"Saved to: {Path.GetFileName(saveFileDialog.FileName)}");
                }
            }
            catch (Exception ex)
            {
                WriteError($"Failed to save console output: {ex.Message}");
            }
        }
        
        protected override void OnClosed(EventArgs e)
        {
            if (!string.IsNullOrEmpty(_consoleName) && _consoleName != "Global Console")
            {
                ConsoleWindowManager.CloseReservedConsole(_consoleName);
            }
            else
            {
                lock (_lock)
                {
                }
            }
            base.OnClosed(e);
        }
    }
}
