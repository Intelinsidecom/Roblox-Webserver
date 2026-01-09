using Npgsql;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Common;

public static class DatabaseUtilities
{
    /// <summary>
    /// Creates and opens a database connection with standard configuration
    /// </summary>
    public static async Task<NpgsqlConnection> CreateAndOpenConnectionAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required", nameof(connectionString));

        var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        return conn;
    }

    /// <summary>
    /// Executes a command and returns the number of affected rows
    /// </summary>
    public static async Task<int> ExecuteNonQueryAsync(string connectionString, string sql, CancellationToken cancellationToken = default, params (string parameterName, object value)[] parameters)
    {
        using var conn = await CreateAndOpenConnectionAsync(connectionString, cancellationToken);
        using var cmd = new NpgsqlCommand(sql, conn);
        
        foreach (var (name, val) in parameters)
        {
            cmd.Parameters.AddWithValue(name, val ?? DBNull.Value);
        }
        
        return await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes a command and returns the first column of the first row
    /// </summary>
    public static async Task<object> ExecuteScalarAsync(string connectionString, string sql, CancellationToken cancellationToken = default, params (string parameterName, object value)[] parameters)
    {
        using var conn = await CreateAndOpenConnectionAsync(connectionString, cancellationToken);
        using var cmd = new NpgsqlCommand(sql, conn);
        
        foreach (var (name, val) in parameters)
        {
            cmd.Parameters.AddWithValue(name, val ?? DBNull.Value);
        }
        
        return await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes a command and returns a data reader
    /// </summary>
    public static async Task<IDataReader> ExecuteReaderAsync(string connectionString, string sql, CancellationToken cancellationToken = default, params (string parameterName, object value)[] parameters)
    {
        var conn = await CreateAndOpenConnectionAsync(connectionString, cancellationToken);
        var cmd = new NpgsqlCommand(sql, conn);
        
        foreach (var (name, val) in parameters)
        {
            cmd.Parameters.AddWithValue(name, val ?? DBNull.Value);
        }
        
        return await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Validates that a record exists and throws if not found
    /// </summary>
    public static async Task EnsureRecordExistsAsync(string connectionString, string sql, string errorMessage, CancellationToken cancellationToken = default, params (string parameterName, object value)[] parameters)
    {
        var rowsAffected = await ExecuteNonQueryAsync(connectionString, sql, cancellationToken, parameters);
        if (rowsAffected == 0)
            throw new InvalidOperationException(errorMessage);
    }
}
