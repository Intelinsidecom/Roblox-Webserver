using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;
using Control_Panel.Properties;

namespace Control_Panel
{
    public class DatabaseUtilities
    {
        private static readonly string SettingsKey = @"SOFTWARE\Control Panel\RobloxWebserver";
        private static readonly string MigrationFolderKey = "MigrationFolder";

        public class MigrationResult
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public List<string> Logs { get; set; } = new List<string>();
        }

        public static string GetSavedMigrationFolder()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(SettingsKey))
                {
                    if (key != null)
                    {
                        var folder = key.GetValue(MigrationFolderKey) as string;
                        if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder))
                        {
                            return folder;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error reading saved migration folder: {ex.Message}");
            }
            return null;
        }

        public static void SaveMigrationFolder(string folderPath)
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(SettingsKey))
                {
                    key?.SetValue(MigrationFolderKey, folderPath);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error saving migration folder: {ex.Message}");
            }
        }

        public static string SelectMigrationFolder()
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select Database SQL Migration Folder";
                folderDialog.ShowNewFolderButton = false;
                var defaultPath = Path.Combine(GetSolutionRoot(), "Database", "Sql");
                if (Directory.Exists(defaultPath))
                {
                    folderDialog.SelectedPath = defaultPath;
                }
                else
                {
                    var savedPath = GetSavedMigrationFolder();
                    if (!string.IsNullOrEmpty(savedPath))
                    {
                        folderDialog.SelectedPath = savedPath;
                    }
                }

                while (true)
                {
                    var result = folderDialog.ShowDialog();
                    if (result != DialogResult.OK || string.IsNullOrEmpty(folderDialog.SelectedPath))
                    {
                        return null;
                    }

                    var selectedPath = folderDialog.SelectedPath;
                    var sqlFiles = Directory.GetFiles(selectedPath, "*.sql", SearchOption.TopDirectoryOnly);
                    if (sqlFiles.Length == 0)
                    {
                        MessageBox.Show(
                            "The selected folder does not contain any SQL migration files (*.sql).\n\nPlease select a folder that contains SQL migration files.",
                            "No SQL Files Found",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        continue; // Show dialog again
                    }

                    SaveMigrationFolder(selectedPath);
                    return selectedPath;
                }
            }
        }

        public static string GetSolutionRoot()
        {
            var exeDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var projectDir = Path.GetFullPath(Path.Combine(exeDir, "..", ".."));
            return Path.GetFullPath(Path.Combine(projectDir, "..", ".."));
        }

        public static bool IsDatabaseAccessible()
        {
            try
            {
                var connectionString = GetConnectionString();
                if (string.IsNullOrEmpty(connectionString))
                {
                    return false;
                }

                using (var conn = new Npgsql.NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    return conn.State == System.Data.ConnectionState.Open;
                }
            }
            catch
            {
                return false;
            }
        }

        public static void EnsureDatabaseAccessible()
        {
            if (!IsDatabaseAccessible())
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
                    {
                        window.Close();
                    }
                    
                    var dbConnectionWindow = new DatabaseConnectionWindow();
                    dbConnectionWindow.Show();
                });
            }
        }

        public static string GetConnectionString()
        {
            try
            {
                var settingsConnectionString = Settings.Default.DatabaseConnectionString;
                if (!string.IsNullOrEmpty(settingsConnectionString))
                {
                    return settingsConnectionString;
                }

                var appConfigConnectionString = ConfigurationManager.ConnectionStrings["Default"]?.ConnectionString;
                if (!string.IsNullOrEmpty(appConfigConnectionString))
                {
                    return appConfigConnectionString;
                }

                var websiteDir = Path.Combine(GetSolutionRoot(), "Website");
                if (Directory.Exists(websiteDir))
                {
                    var appSettingsPath = Path.Combine(websiteDir, "appsettings.json");
                    if (File.Exists(appSettingsPath))
                    {
                        var json = File.ReadAllText(appSettingsPath);
                        var connectionMatch = System.Text.RegularExpressions.Regex.Match(json, @"""ConnectionStrings""\s*:\s*\{[^}]*""Default""\s*:\s*""([^""]+)""");
                        if (connectionMatch.Success)
                        {
                            return connectionMatch.Groups[1].Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error getting connection string: {ex.Message}");
            }
            
            return null;
        }

        public static MigrationResult PerformMigration(string migrationFolder)
        {
            EnsureDatabaseAccessible();
            
            var result = new MigrationResult();
            
            try
            {
                LogMessage("=== Database Migration Started ===");
                LogMessage($"Migration folder: {migrationFolder}");
                
                var connectionString = GetConnectionString();
                if (string.IsNullOrEmpty(connectionString))
                {
                    LogError("Database connection string not found");
                    LogMessage("Please configure the connection in Database Connection Window or Website/appsettings.json");
                    result.Success = false;
                    result.Message = "Database connection string not found. Please configure the connection in app.config or Website/appsettings.json";
                    return result;
                }

                LogMessage($"Database connection: {MaskConnectionString(connectionString)}");

                if (!Directory.Exists(migrationFolder))
                {
                    LogError($"Migration folder not found: {migrationFolder}");
                    result.Success = false;
                    result.Message = $"Migration folder not found: {migrationFolder}";
                    return result;
                }

                var sqlFiles = Directory.GetFiles(migrationFolder, "*.sql", SearchOption.TopDirectoryOnly);
                if (sqlFiles.Length == 0)
                {
                    LogError("No SQL migration files found in the selected folder");
                    result.Success = false;
                    result.Message = "No SQL migration files found in the selected folder";
                    return result;
                }

                LogMessage($"Found {sqlFiles.Length} SQL migration files in folder");
                Array.Sort(sqlFiles);
                
                var executedMigrations = GetExecutedMigrations(connectionString);
                var pendingMigrations = new List<string>();
                var totalPendingStatements = 0;
                
                foreach (var file in sqlFiles)
                {
                    var fileName = Path.GetFileName(file);
                    var migrationName = Path.GetFileNameWithoutExtension(file);
                    
                    if (!executedMigrations.Contains(migrationName))
                    {
                        pendingMigrations.Add(fileName);
                        
                        try
                        {
                            var content = File.ReadAllText(file);
                            var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                            var statementCount = lines.Count(l => 
                                !l.Trim().StartsWith("--") && 
                                !l.Trim().StartsWith("/*") &&
                                !l.Trim().StartsWith("*") &&
                                !l.Trim().StartsWith("*/") &&
                                !string.IsNullOrWhiteSpace(l.Trim()) &&
                                l.Trim().EndsWith(";"));
                            
                            totalPendingStatements += statementCount;
                        }
                        catch (Exception ex)
                        {
                            LogError($"Failed to analyze {fileName}: {ex.Message}");
                        }
                    }
                }
                
                if (pendingMigrations.Count == 0)
                {
                    LogSuccess("Database is already up to date - no pending migrations");
                    result.Success = true;
                    result.Message = "Database is already up to date";
                    return result;
                }
                
                LogMessage($"Executing {pendingMigrations.Count} pending migrations with {totalPendingStatements} total statements");
                
                result = RunMigrationsManually(connectionString, migrationFolder, pendingMigrations.ToArray());
                
                if (result.Success)
                {
                    LogSuccess("Database migration completed successfully");
                    result.Message = "Database migration completed successfully";
                }
                else
                {
                    LogError($"Migration failed: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                LogError($"Migration failed with exception: {ex.Message}");
                result.Success = false;
                result.Message = $"Migration failed: {ex.Message}";
            }

            LogMessage("=== Database Migration Completed ===");
            return result;
        }

        private static MigrationResult RunMigrationsManually(string connectionString, string migrationFolder, string[] pendingFiles)
        {
            var result = new MigrationResult();
            
            try
            {
                LogMessage($"Executing {pendingFiles.Length} pending migrations...");
                
                var executedCount = 0;
                var totalStatements = 0;
                
                foreach (var file in pendingFiles)
                {
                    var fileName = Path.GetFileName(file);
                    var migrationName = Path.GetFileNameWithoutExtension(file);
                    var fullPath = Path.Combine(migrationFolder, fileName);
                    
                    LogMessage($"Executing {migrationName}...");
                    
                    try
                    {
                        var content = File.ReadAllText(fullPath);
                        var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        var statementCount = lines.Count(l => 
                            !l.Trim().StartsWith("--") && 
                            !l.Trim().StartsWith("/*") &&
                            !l.Trim().StartsWith("*") &&
                            !l.Trim().StartsWith("*/") &&
                            !string.IsNullOrWhiteSpace(l.Trim()) &&
                            l.Trim().EndsWith(";"));
                        
                        totalStatements += statementCount;
                        
                        ExecuteSqlScript(connectionString, content);
                        MarkMigrationAsExecuted(connectionString, migrationName, content);
                        
                        executedCount++;
                        LogSuccess($" {migrationName} ({statementCount} statements)");
                    }
                    catch (Exception ex)
                    {
                        LogError($"Failed to execute {migrationName}: {ex.Message}");
                        result.Success = false;
                        result.Message = $"Migration {migrationName} failed: {ex.Message}";
                        return result;
                    }
                }
                
                LogSuccess($"Successfully executed {executedCount} migrations with {totalStatements} total statements");
                
                result.Success = true;
                result.Message = $"Successfully executed {executedCount} migrations with {totalStatements} SQL statements";
                result.Logs.Add($"Executed migrations: {executedCount}");
                result.Logs.Add($"Total statements: {totalStatements}");
            }
            catch (Exception ex)
            {
                LogError($"Migration execution error: {ex.Message}");
                result.Success = false;
                result.Message = $"Migration execution error: {ex.Message}";
            }

            return result;
        }

        private static HashSet<string> GetExecutedMigrations(string connectionString)
        {
            var executed = new HashSet<string>();
            
            try
            {
                using (var conn = new Npgsql.NpgsqlConnection(connectionString))
                {
                    conn.Open();
                        
                    // Check if schemaversions table exists
                    var checkTableCmd = new Npgsql.NpgsqlCommand(@"
                        SELECT COUNT(*) 
                        FROM information_schema.tables 
                        WHERE table_schema = 'public' AND table_name = 'schemaversions'", conn);
                        
                    var tableExists = Convert.ToInt64(checkTableCmd.ExecuteScalar()) > 0;
                        
                    if (tableExists)
                    {
                        // Get executed migrations
                        var getMigrationsCmd = new Npgsql.NpgsqlCommand(@"
                            SELECT scriptname 
                            FROM public.schemaversions 
                            ORDER BY applied", conn);
                            
                        using (var reader = getMigrationsCmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var scriptName = reader.GetString(0);
                                var migrationName = Path.GetFileNameWithoutExtension(scriptName);
                                executed.Add(migrationName);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Failed to check executed migrations: {ex.Message}");
            }
            
            return executed;
        }

        private static void ExecuteSqlScript(string connectionString, string sqlScript)
        {
            using (var conn = new Npgsql.NpgsqlConnection(connectionString))
            {
                conn.Open();

                var statements = SplitSqlStatements(sqlScript);
                
                foreach (var statement in statements)
                {
                    if (!string.IsNullOrWhiteSpace(statement.Trim()))
                    {
                        using (var cmd = new Npgsql.NpgsqlCommand(statement, conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        private static List<string> SplitSqlStatements(string script)
        {
            var statements = new List<string>();
            var lines = script.Split(new[] { '\r', '\n' }, StringSplitOptions.None);
            var currentStatement = "";
            var inDollarQuote = false;
            var dollarQuoteTag = "";
            
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                
                if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("--"))
                {
                    continue;
                }
                
                currentStatement += line + "\n";
                
                if (!inDollarQuote)
                {
                    var dollarMatch = System.Text.RegularExpressions.Regex.Match(trimmedLine, @"\$[\w]*\$");
                    if (dollarMatch.Success)
                    {
                        inDollarQuote = true;
                        dollarQuoteTag = dollarMatch.Value;
                    }
                }
                else
                {
                    if (trimmedLine.Contains(dollarQuoteTag))
                    {
                        inDollarQuote = false;
                        dollarQuoteTag = "";
                    }
                }
                
                if (!inDollarQuote && trimmedLine.EndsWith(";"))
                {
                    statements.Add(currentStatement.Trim());
                    currentStatement = "";
                }
            }
            
            if (!string.IsNullOrWhiteSpace(currentStatement.Trim()))
            {
                statements.Add(currentStatement.Trim());
            }
            
            return statements;
        }

        private static void MarkMigrationAsExecuted(string connectionString, string migrationName, string scriptContent)
        {
            try
            {
                using (var conn = new Npgsql.NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    
                    var insertCmd = new Npgsql.NpgsqlCommand(@"
                        INSERT INTO public.schemaversions (scriptname, applied) 
                        VALUES (@name, @applied)", conn);
                    
                    insertCmd.Parameters.AddWithValue("@name", migrationName + ".sql");
                    insertCmd.Parameters.AddWithValue("@applied", DateTime.UtcNow);
                    
                    insertCmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                LogError($"Failed to mark migration as executed: {ex.Message}");
            }
        }

        private static string MaskConnectionString(string connectionString)
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

        private static void LogMessageSafe(string message, string consoleName = "Database Import")
        {
            try
            {
                var console = ConsoleWindowManager.GetReservedConsole(consoleName);
                if (console != null)
                {
                    try
                    {
                        console.Dispatcher.Invoke(() =>
                        {
                            console.WriteLine(message, consoleName);
                        });
                    }
                    catch
                    {
                        System.Diagnostics.Debug.WriteLine($"[{consoleName}] {message}");
                    }
                }
                
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[{consoleName}] Failed to log message: {ex.Message}");
            }
        }

        private static void LogErrorSafe(string message, string consoleName = "Database Import")
        {
            try
            {
                var console = ConsoleWindowManager.GetReservedConsole(consoleName);
                if (console != null)
                {
                    try
                    {
                        console.Dispatcher.Invoke(() =>
                        {
                            console.WriteError(message);
                        });
                    }
                    catch
                    {
                        System.Diagnostics.Debug.WriteLine($"[{consoleName}] ERROR: {message}");
                    }
                }
                
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[{consoleName}] Failed to log error: {ex.Message}");
            }
        }

        private static void LogMessage(string message)
        {
            try
            {
                var migrationConsole = ConsoleWindowManager.GetReservedConsole("Database Migration");
                if (migrationConsole != null)
                {
                    migrationConsole.Dispatcher.Invoke(() =>
                    {
                        migrationConsole.WriteLine(message, "Database Migration");
                    });
                }
                
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Database Migration] Failed to log message: {ex.Message}");
            }
        }

        private static void LogSuccess(string message)
        {
            try
            {
                var migrationConsole = ConsoleWindowManager.GetReservedConsole("Database Migration");
                if (migrationConsole != null)
                {
                    migrationConsole.Dispatcher.Invoke(() =>
                    {
                        migrationConsole.WriteSuccess(message);
                    });
                }
                

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Database Migration] Failed to log success: {ex.Message}");
            }
        }

        private static void LogError(string message)
        {
            try
            {
                var migrationConsole = ConsoleWindowManager.GetReservedConsole("Database Migration");
                if (migrationConsole != null)
                {
                    migrationConsole.Dispatcher.Invoke(() =>
                    {
                        migrationConsole.WriteError(message);
                    });
                }
                
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Database Migration] Failed to log error: {ex.Message}");
            }
        }

        private static void LogSQL(string sql)
        {
            try
            {
                var migrationConsole = ConsoleWindowManager.GetReservedConsole("Database Migration");
                if (migrationConsole != null)
                {
                    migrationConsole.Dispatcher.Invoke(() =>
                    {
                        migrationConsole.WriteSQL(sql);
                    });
                }
                
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Database Migration] Failed to log SQL: {ex.Message}");
            }
        }

        public static bool PerformDatabaseWipe()
        {
            EnsureDatabaseAccessible();
            
            try
            {
                var connectionString = GetConnectionString();
                if (string.IsNullOrEmpty(connectionString))
                {
                    LogErrorSafe("Failed to get database connection string", "Database Wipe");
                    return false;
                }

                LogMessageSafe("Starting database wipe operation...", "Database Wipe");

                using (var conn = new Npgsql.NpgsqlConnection(connectionString))
                {
                    conn.Open();

                    // Get all table names
                    var getTablesCmd = new Npgsql.NpgsqlCommand(@"
                        SELECT tablename 
                        FROM pg_tables 
                        WHERE schemaname = 'public' 
                        AND tablename != 'schemaversions'", conn);

                    var tables = new List<string>();
                    using (var reader = getTablesCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tables.Add(reader.GetString(0));
                        }
                    }

                    var disableConstraintsCmd = new Npgsql.NpgsqlCommand("SET session_replication_role = replica;", conn);
                    disableConstraintsCmd.ExecuteNonQuery();

                    foreach (var table in tables)
                    {
                        LogMessageSafe($"Dropping table: {table}", "Database Wipe");
                        var dropCmd = new Npgsql.NpgsqlCommand($"DROP TABLE IF EXISTS \"{table}\" CASCADE;", conn);
                        dropCmd.ExecuteNonQuery();
                    }

                    var enableConstraintsCmd = new Npgsql.NpgsqlCommand("SET session_replication_role = DEFAULT;", conn);
                    enableConstraintsCmd.ExecuteNonQuery();
                    var clearSchemaCmd = new Npgsql.NpgsqlCommand("TRUNCATE TABLE schemaversions RESTART IDENTITY CASCADE;", conn);
                    clearSchemaCmd.ExecuteNonQuery();
                }

                LogMessageSafe("Database wipe completed successfully", "Database Wipe");
                return true;
            }
            catch (Exception ex)
            {
                LogErrorSafe($"Database wipe failed: {ex.Message}", "Database Wipe");
                return false;
            }
        }

        public static bool PerformDatabaseExport(string filePath)
        {
            EnsureDatabaseAccessible();
            
            try
            {
                var connectionString = GetConnectionString();
                if (string.IsNullOrEmpty(connectionString))
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        var exportConsole = ConsoleWindowManager.GetReservedConsole("Database Export");
                        exportConsole.WriteError("Failed to get database connection string");
                    });
                    return false;
                }

                var pgDumpPath = FindPgDumpExecutable();
                if (string.IsNullOrEmpty(pgDumpPath))
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        var exportConsole = ConsoleWindowManager.GetReservedConsole("Database Export");
                        exportConsole.WriteError("pg_dump executable not found. Please ensure PostgreSQL is installed and pg_dump is in PATH.");
                    });
                    return false;
                }

                var builder = new Npgsql.NpgsqlConnectionStringBuilder(connectionString);
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = pgDumpPath,
                    Arguments = $"--host={builder.Host} --port={builder.Port} --username={builder.Username} --dbname={builder.Database} --verbose --clean --create --no-owner --no-privileges --format=custom --file=\"{filePath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                startInfo.EnvironmentVariables["PGPASSWORD"] = builder.Password;

                using (var process = System.Diagnostics.Process.Start(startInfo))
                {
                    process.WaitForExit();
                    
                    if (process.ExitCode == 0)
                    {
                        return true;
                    }
                    else
                    {
                        var error = process.StandardError.ReadToEnd();
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            var exportConsole = ConsoleWindowManager.GetReservedConsole("Database Export");
                            exportConsole.WriteError($"Database export failed with exit code {process.ExitCode}: {error}");
                        });
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    var exportConsole = ConsoleWindowManager.GetReservedConsole("Database Export");
                    exportConsole.WriteError($"Database export failed: {ex.Message}");
                });
                return false;
            }
        }

        public static bool PerformDatabaseImport(string filePath)
        {
            EnsureDatabaseAccessible();
            
            try
            {
                if (!File.Exists(filePath))
                {
                    LogErrorSafe($"Import file not found: {filePath}");
                    return false;
                }

                var connectionString = GetConnectionString();
                if (string.IsNullOrEmpty(connectionString))
                {
                    LogErrorSafe("Failed to get database connection string");
                    return false;
                }

                LogMessageSafe($"Starting database import from: {filePath}");
                LogMessageSafe("Wiping Database.");

                var pgRestorePath = FindPgRestoreExecutable();
                if (string.IsNullOrEmpty(pgRestorePath))
                {
                    LogErrorSafe("pg_restore executable not found. Please ensure PostgreSQL is installed and pg_restore is in PATH.");
                    return false;
                }

                var builder = new Npgsql.NpgsqlConnectionStringBuilder(connectionString);
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = pgRestorePath,
                    Arguments = $"--host={builder.Host} --port={builder.Port} --username={builder.Username} --dbname={builder.Database} --verbose --clean --if-exists --no-owner --no-privileges \"{filePath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                startInfo.EnvironmentVariables["PGPASSWORD"] = builder.Password;

                LogMessageSafe("Running pg_restore command...");

                using (var process = System.Diagnostics.Process.Start(startInfo))
                {
                    var outputTask = System.Threading.Tasks.Task.Run(() =>
                    {
                        while (!process.StandardOutput.EndOfStream)
                        {
                            var line = process.StandardOutput.ReadLine();
                            if (!string.IsNullOrEmpty(line))
                            {
                                LogMessageSafe(line);
                            }
                        }
                    });

                    var errorTask = System.Threading.Tasks.Task.Run(() =>
                    {
                        while (!process.StandardError.EndOfStream)
                        {
                            var line = process.StandardError.ReadLine();
                            if (!string.IsNullOrEmpty(line))
                            {
                                LogMessageSafe($"pg_restore: {line}");
                            }
                        }
                    });

                    process.WaitForExit();
                    
                    System.Threading.Tasks.Task.WaitAll(outputTask, errorTask);
                    
                    if (process.ExitCode == 0)
                    {
                        LogMessageSafe("Database import completed successfully");
                        return true;
                    }
                    else
                    {
                        var error = process.StandardError.ReadToEnd();
                        LogErrorSafe($"Database import failed with exit code {process.ExitCode}: {error}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                LogErrorSafe($"Database import failed: {ex.Message}");
                return false;
            }
        }

        private static string FindPgDumpExecutable()
        {
            try
            {
                var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "where",
                    Arguments = "pg_dump",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                });
                
                process.WaitForExit();
                if (process.ExitCode == 0)
                {
                    var output = process.StandardOutput.ReadToEnd().Trim();
                    if (!string.IsNullOrEmpty(output))
                    {
                        return output.Split('\r', '\n')[0].Trim(); // Return first match
                    }
                }
            }
            catch
            {
            }

            var possiblePaths = new[]
            {
                @"C:\Program Files\PostgreSQL\*\bin\pg_dump.exe",
                @"C:\Program Files (x86)\PostgreSQL\*\bin\pg_dump.exe"
            };

            foreach (var path in possiblePaths)
            {
                try
                {
                    if (path.Contains("*"))
                    {
                        var basePath = path.Replace("*", "");
                        var parentDir = Path.GetDirectoryName(basePath);
                        if (Directory.Exists(parentDir))
                        {
                            var files = Directory.GetFiles(parentDir, "pg_dump.exe", SearchOption.AllDirectories);
                            if (files.Length > 0)
                                return files[0];
                        }
                    }
                    else
                    {
                        if (File.Exists(path))
                            return path;
                    }
                }
                catch
                {
                }
            }

            return "pg_dump.exe";
        }

        private static string FindPgRestoreExecutable()
        {
            try
            {
                var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "where",
                    Arguments = "pg_restore",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                });
                
                process.WaitForExit();
                if (process.ExitCode == 0)
                {
                    var output = process.StandardOutput.ReadToEnd().Trim();
                    if (!string.IsNullOrEmpty(output))
                    {
                        return output.Split('\r', '\n')[0].Trim(); // Return first match
                    }
                }
            }
            catch
            {
            }

            var possiblePaths = new[]
            {
                @"C:\Program Files\PostgreSQL\*\bin\pg_restore.exe",
                @"C:\Program Files (x86)\PostgreSQL\*\bin\pg_restore.exe"
            };

            foreach (var path in possiblePaths)
            {
                try
                {
                    if (path.Contains("*"))
                    {
                        var basePath = path.Replace("*", "");
                        var parentDir = Path.GetDirectoryName(basePath);
                        if (Directory.Exists(parentDir))
                        {
                            var files = Directory.GetFiles(parentDir, "pg_restore.exe", SearchOption.AllDirectories);
                            if (files.Length > 0)
                                return files[0];
                        }
                    }
                    else
                    {
                        if (File.Exists(path))
                            return path;
                    }
                }
                catch
                {
                }
            }

            return "pg_restore.exe";
        }
    }
}
