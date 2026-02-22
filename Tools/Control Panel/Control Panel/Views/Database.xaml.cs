using System;
using System.Windows;
using System.Windows.Controls;

namespace Control_Panel
{
    /// <summary>
    /// Interaction logic for Database.xaml
    /// </summary>
    public partial class Database : UserControl
    {
        public Database()
        {
            InitializeComponent();
        }

        private async void MigrationButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var mainWindow = Window.GetWindow(this) as Main;
                MigrateButton.IsEnabled = false;
                MigrationStatusText.Text = "Checking migration folder...";
                mainWindow?.ViewLoader.UpdateStatus("Database migration: Checking migration folder...");
                string migrationFolder = DatabaseUtilities.GetSavedMigrationFolder();

                if (string.IsNullOrEmpty(migrationFolder))
                {
                    MigrationStatusText.Text = "Please select migration folder...";
                    mainWindow?.ViewLoader.UpdateStatus("Database migration: Please select migration folder...");
                    migrationFolder = DatabaseUtilities.SelectMigrationFolder();
                    
                    if (string.IsNullOrEmpty(migrationFolder))
                    {
                        MigrationStatusText.Text = "Migration cancelled";
                        mainWindow?.ViewLoader.UpdateStatus("Database migration: Cancelled");
                        MigrateButton.IsEnabled = true;
                        return;
                    }
                }

                var migrationConsole = ConsoleWindowManager.GetReservedConsole("Database Migration");
                ConsoleWindowManager.ShowReservedConsole("Database Migration");
                MigrationStatusText.Text = "Running migration...";
                mainWindow?.ViewLoader.UpdateStatus("Database migration: Running migration...");
                var result = await System.Threading.Tasks.Task.Run(() => 
                    DatabaseUtilities.PerformMigration(migrationFolder));

                if (result.Success)
                {
                    MigrationStatusText.Text = "Success";
                    MigrationStatusText.Foreground = System.Windows.Media.Brushes.Green;
                    mainWindow?.ViewLoader.UpdateStatus("Database migration: Success");
                }
                else
                {
                    MigrationStatusText.Text = "Error";
                    MigrationStatusText.Foreground = System.Windows.Media.Brushes.Red;
                    mainWindow?.ViewLoader.UpdateStatus("Database migration: Error");
                }
            }
            catch (Exception ex)
            {
                var migrationConsole = ConsoleWindowManager.GetReservedConsole("Database Migration");
                migrationConsole.WriteError($"Migration failed: {ex.Message}");
                MigrationStatusText.Text = "Error";
                MigrationStatusText.Foreground = System.Windows.Media.Brushes.Red;
                var mainWindow = Window.GetWindow(this) as Main;
                mainWindow?.ViewLoader.UpdateStatus("Database migration: Error");
            }
            finally
            {
                MigrateButton.IsEnabled = true;
            }
        }

        private async void WipeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show(
                    "Are you sure you want to wipe all database data? This action cannot be undone.",
                    "Confirm Database Wipe",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                {
                    WipeStatusText.Text = "Cancelled";
                    return;
                }

                WipeButton.IsEnabled = false;
                WipeStatusText.Text = "Wiping database...";
                var wipeConsole = ConsoleWindowManager.GetReservedConsole("Database Wipe");
                ConsoleWindowManager.ShowReservedConsole("Database Wipe");
                var success = await System.Threading.Tasks.Task.Run(() => 
                    DatabaseUtilities.PerformDatabaseWipe());

                if (success)
                {
                    WipeStatusText.Text = "Success";
                    WipeStatusText.Foreground = System.Windows.Media.Brushes.Green;
                }
                else
                {
                    WipeStatusText.Text = "Error";
                    WipeStatusText.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
            catch (Exception ex)
            {
                var wipeConsole = ConsoleWindowManager.GetReservedConsole("Database Wipe");
                wipeConsole.WriteError($"Database wipe failed: {ex.Message}");
                WipeStatusText.Text = "Error";
                WipeStatusText.Foreground = System.Windows.Media.Brushes.Red;
            }
            finally
            {
                WipeButton.IsEnabled = true;
            }
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var mainWindow = Window.GetWindow(this) as Main;
                ExportButton.IsEnabled = false;
                ExportStatusText.Text = "Selecting export location...";
                mainWindow?.ViewLoader.UpdateStatus("Database export: Selecting export location...");
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "SQL files (*.sql)|*.sql|All files (*.*)|*.*",
                    Title = "Export Database",
                    FileName = $"database_export_{DateTime.Now:yyyyMMdd_HHmmss}.sql"
                };

                if (saveFileDialog.ShowDialog() != true)
                {
                    ExportStatusText.Text = "Cancelled";
                    mainWindow?.ViewLoader.UpdateStatus("Database export: Cancelled");
                    ExportButton.IsEnabled = true;
                    return;
                }

                ExportStatusText.Text = "Exporting database...";
                mainWindow?.ViewLoader.UpdateStatus("Database export: Exporting database...");
                var success = await System.Threading.Tasks.Task.Run(() => 
                    DatabaseUtilities.PerformDatabaseExport(saveFileDialog.FileName));

                if (success)
                {
                    ExportStatusText.Text = "Success";
                    ExportStatusText.Foreground = System.Windows.Media.Brushes.Green;
                    mainWindow?.ViewLoader.UpdateStatus("Database export: Success");
                }
                else
                {
                    ExportStatusText.Text = "Error";
                    ExportStatusText.Foreground = System.Windows.Media.Brushes.Red;
                    mainWindow?.ViewLoader.UpdateStatus("Database export: Error");
                    ConsoleWindowManager.ShowReservedConsole("Database Export");
                }
            }
            catch (Exception ex)
            {
                var exportConsole = ConsoleWindowManager.GetReservedConsole("Database Export");
                exportConsole.WriteError($"Database export failed: {ex.Message}");
                ConsoleWindowManager.ShowReservedConsole("Database Export");
                ExportStatusText.Text = "Error";
                ExportStatusText.Foreground = System.Windows.Media.Brushes.Red;
                var mainWindow = Window.GetWindow(this) as Main;
                mainWindow?.ViewLoader.UpdateStatus("Database export: Error");
            }
            finally
            {
                ExportButton.IsEnabled = true;
            }
        }

        private void SqlConsoleButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SqlConsoleStatusText.Text = "Opening SQL Console...";
                var sqlConsole = new SqlConsoleWindow("");
                App.ShowWindowWithShutdownHandling(sqlConsole);
                SqlConsoleStatusText.Text = "SQL Console Opened";
            }
            catch (Exception ex)
            {
                SqlConsoleStatusText.Text = "Error";
                SqlConsoleStatusText.Foreground = System.Windows.Media.Brushes.Red;
                var mainWindow = Window.GetWindow(this) as Main;
                mainWindow?.ViewLoader.UpdateStatus("SQL Console: Error");
                MessageBox.Show($"Failed to open SQL Console: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var firstWarningResult = MessageBox.Show(
                    "WARNING: This will permanently delete all current database data and replace it with the imported file.\n\n" +
                    "This action cannot be undone.\n\n" +
                    "Do you want to continue with the import?",
                    "Database Import Warning",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (firstWarningResult != MessageBoxResult.Yes)
                {
                    ImportStatusText.Text = "Cancelled";
                    return;
                }

                ImportButton.IsEnabled = false;
                ImportStatusText.Text = "Selecting import file...";
                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "SQL dump files (*.sql)|*.sql",
                    Title = "Import Database",
                    CheckFileExists = true,
                    CheckPathExists = true
                };

                if (openFileDialog.ShowDialog() != true)
                {
                    ImportStatusText.Text = "Cancelled";
                    ImportButton.IsEnabled = true;
                    return;
                }

                ImportStatusText.Text = "Importing database...";

                var importConsole = ConsoleWindowManager.GetReservedConsole("Database Import");
                ConsoleWindowManager.ShowReservedConsole("Database Import");
                var success = await System.Threading.Tasks.Task.Run(() => 
                    DatabaseUtilities.PerformDatabaseImport(openFileDialog.FileName));

                if (success)
                {
                    ImportStatusText.Text = "Success";
                    ImportStatusText.Foreground = System.Windows.Media.Brushes.Green;
                }
                else
                {
                    ImportStatusText.Text = "Error";
                    ImportStatusText.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
            catch (Exception ex)
            {
                var importConsole = ConsoleWindowManager.GetReservedConsole("Database Import");
                importConsole.WriteError($"Database import failed: {ex.Message}");
                ImportStatusText.Text = "Error";
                ImportStatusText.Foreground = System.Windows.Media.Brushes.Red;
            }
            finally
            {
                ImportButton.IsEnabled = true;
            }
        }
    }
}
