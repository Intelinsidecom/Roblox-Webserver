using System;
using System.Data;
using Npgsql;
using Control_Panel;

namespace ControlPanel.Functions
{
    public class DatabaseQueries
    {
        private string connectionString;

        public DatabaseQueries(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));
            
            if (connectionString.Length < 10)
                throw new ArgumentException("Connection string appears to be invalid (too short)", nameof(connectionString));
            
            if (!connectionString.Contains("Host=") && !connectionString.Contains("Server="))
                throw new ArgumentException("Connection string must contain Host or Server parameter", nameof(connectionString));
            
            if (!connectionString.Contains("Database="))
                throw new ArgumentException("Connection string must contain Database parameter", nameof(connectionString));
            
            this.connectionString = connectionString;
        }

        public bool TestConnection()
        {
            try
            {
                ConsoleWindow.Instance?.WriteLine("Testing database connection...", "DATABASE");
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    bool result = connection.State == ConnectionState.Open;
                    if (result)
                    {
                        ConsoleWindow.Instance?.WriteSuccess("Database connection successful");
                    }
                    else
                    {
                        ConsoleWindow.Instance?.WriteError("Database connection failed");
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                ConsoleWindow.Instance?.WriteError($"Database connection test failed: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Database connection test failed: {ex.Message}");
                return false;
            }
        }

        public DataTable ExecuteQuery(string query)
        {
            try
            {
                ConsoleWindow.Instance?.WriteSQL(query);
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);
                            ConsoleWindow.Instance?.WriteSuccess($"Query executed successfully. Rows returned: {dataTable.Rows.Count}");
                            return dataTable;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleWindow.Instance?.WriteError($"Query execution failed: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Query execution failed: {ex.Message}");
                return null;
            }
        }

        public int ExecuteNonQuery(string query)
        {
            try
            {
                ConsoleWindow.Instance?.WriteSQL(query);
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        int result = command.ExecuteNonQuery();
                        ConsoleWindow.Instance?.WriteSuccess($"Non-query executed successfully. Rows affected: {result}");
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleWindow.Instance?.WriteError($"Non-query execution failed: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Non-query execution failed: {ex.Message}");
                return -1;
            }
        }

        public object ExecuteScalar(string query)
        {
            try
            {
                ConsoleWindow.Instance?.WriteSQL(query);
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        object result = command.ExecuteScalar();
                        ConsoleWindow.Instance?.WriteSuccess($"Scalar query executed successfully. Result: {result ?? "NULL"}");
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleWindow.Instance?.WriteError($"Scalar query execution failed: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Scalar query execution failed: {ex.Message}");
                return null;
            }
        }
    }
}
