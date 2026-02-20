using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Npgsql;

namespace Control_Panel
{
    /// <summary>
    /// SQL Console window for executing SQL commands
    /// </summary>
    public partial class SqlConsoleWindow : Window
    {
        private string _consoleName;
        private string _connectionString;
        
        public SqlConsoleWindow(string consoleName)
        {
            _consoleName = consoleName;
            InitializeComponent();
            this.Icon = System.Windows.Application.Current.MainWindow?.Icon ?? 
                new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/ControlPanel.ico"));
            InitializeTheme();
            UpdateWindowTitle();
            LoadConnectionString();
        }
        
        private void UpdateWindowTitle()
        {
            Title = $"SQL Console - {_consoleName}";
        }
        
        private void InitializeTheme()
        {
            try
            {
                var themeSettings = ControlPanel.Functions.ThemeManager.LoadThemeSettings();
                ControlPanel.Functions.ThemeManager.ApplyThemeToWindow(
                    this, 
                    themeSettings.Theme, 
                    themeSettings.ColorScheme, 
                    themeSettings.BackgroundColor);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load theme for SQL console: {ex.Message}");
            }
        }
        
        private void LoadConnectionString()
        {
            try
            {
                _connectionString = DatabaseUtilities.GetConnectionString();
                if (string.IsNullOrEmpty(_connectionString))
                {
                    WriteError("Database connection string not found. Please configure the connection in app.config or Website/appsettings.json");
                    ExecuteButton.IsEnabled = false;
                }
                else
                {
                    WriteSuccess($"Database connection loaded: {MaskConnectionString(_connectionString)}");
                }
            }
            catch (Exception ex)
            {
                WriteError($"Failed to load database connection: {ex.Message}");
                ExecuteButton.IsEnabled = false;
            }
        }
        
        private string MaskConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return connectionString;

            try
            {
                var masked = connectionString;
                var passwordIndex = connectionString.IndexOf("password=", StringComparison.OrdinalIgnoreCase);
                if (passwordIndex >= 0)
                {
                    var afterPassword = connectionString.Substring(passwordIndex + 9);
                    var semicolonIndex = afterPassword.IndexOf(';');
                    var passwordPart = semicolonIndex >= 0 ? afterPassword.Substring(0, semicolonIndex) : afterPassword;
                    masked = connectionString.Replace(passwordPart, new string('*', passwordPart.Length));
                }
                return masked;
            }
            catch
            {
                return "***MASKED***";
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
                if (ConsoleScrollViewer != null)
                {
                    ConsoleScrollViewer.ScrollToEnd();
                }
                
                UpdateStatus($"Last: {message.Substring(0, Math.Min(message.Length, 30))}...");
            });
        }
        
        public void WriteError(string message)
        {
            WriteLine($"{message}", "ERROR");
        }
        
        public void WriteWarning(string message)
        {
            WriteLine($"{message}", "WARNING");
        }
        
        public void WriteSQL(string sql)
        {
            WriteLine($"SQL: {sql}", "SQL");
        }
        
        public void WriteSuccess(string message)
        {
            WriteLine($"{message}", "SUCCESS");
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
        
        private void ClearOutputButton_Click(object sender, RoutedEventArgs e)
        {
            ConsoleTextBox.Clear();
            UpdateStatus("Console cleared");
        }
        
        private void ClearInputButton_Click(object sender, RoutedEventArgs e)
        {
            SqlInputTextBox.Clear();
            SqlInputTextBox.Focus();
            UpdateStatus("Input cleared");
        }
        
        private async void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            var sql = SqlInputTextBox.Text.Trim();
            
            if (string.IsNullOrEmpty(sql))
            {
                WriteError("No SQL command to execute");
                return;
            }
            
            if (sql == "Enter SQL command here...")
            {
                WriteError("Please enter a valid SQL command");
                return;
            }
            
            ExecuteButton.IsEnabled = false;
            UpdateStatus("Executing...");
            
            try
            {
                await System.Threading.Tasks.Task.Run(() => ExecuteSql(sql));
            }
            catch (Exception ex)
            {
                WriteError($"Execution failed: {ex.Message}");
            }
            finally
            {
                Dispatcher.Invoke(() => ExecuteButton.IsEnabled = true);
                UpdateStatus("Ready");
            }
        }
        
        private void ExecuteSql(string sql)
        {
            try
            {
                WriteSQL(sql);
                
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    
                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        var sqlUpper = sql.ToUpper().Trim();
                        if (sqlUpper.StartsWith("SELECT") || sqlUpper.StartsWith("WITH") || 
                            sqlUpper.StartsWith("SHOW") || sqlUpper.StartsWith("DESCRIBE") || 
                            sqlUpper.StartsWith("EXPLAIN"))
                        {
                            using (var reader = cmd.ExecuteReader())
                            {
                                var resultCount = 0;
                                while (reader.Read())
                                {
                                    resultCount++;
                                    var values = new object[reader.FieldCount];
                                    reader.GetValues(values);
                                    var row = string.Join(" | ", values);
                                    WriteLine($"Row {resultCount}: {row}");
                                }
                                
                                if (resultCount == 0)
                                {
                                    WriteSuccess("Query executed successfully. No rows returned.");
                                }
                                else
                                {
                                    WriteSuccess($"Query executed successfully. {resultCount} row(s) returned.");
                                }
                            }
                        }
                        else
                        {
                            var rowsAffected = cmd.ExecuteNonQuery();
                            WriteSuccess($"Command executed successfully. {rowsAffected} row(s) affected.");
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                WriteError($"PostgreSQL Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    WriteError($"Inner Exception: {ex.InnerException.Message}");
                }
            }
            catch (Exception ex)
            {
                WriteError($"General Error: {ex.Message}");
            }
        }
        
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Text files (*.txt)|*.txt|Log files (*.log)|*.log|SQL files (*.sql)|*.sql|All files (*.*)|*.*",
                    FileName = $"sql_console_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
                };
                
                if (saveFileDialog.ShowDialog() == true)
                {
                    var content = $"SQL Console Output - {_consoleName} - {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
                                 $"{'=', 60}\n\n" +
                                 ConsoleTextBox.Text;
                    
                    File.WriteAllText(saveFileDialog.FileName, content);
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
            base.OnClosed(e);
        }
        
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
        
        private void SqlInputTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SqlInputTextBox.Text == "Enter SQL command here...")
            {
                SqlInputTextBox.Clear();
            }
        }
        
        private void SqlInputTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SqlInputTextBox.Text))
            {
                SqlInputTextBox.Text = "Enter SQL command here...";
            }
        }
    }
}
