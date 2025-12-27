using Microsoft.Extensions.Logging;
using Octonica.ClickHouseClient;
using Oip.Rtds.Grpc;

namespace Oip.Rtds.Data.Contexts;

/// <summary>
/// ClickHouse database context for real-time data storage operations.
/// </summary>
public sealed class RtdsContext : IDisposable, IAsyncDisposable
{
    private readonly ILogger<RtdsContext> _logger;
    private readonly ClickHouseConnection _connection;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="RtdsContext"/> class using the provided application settings.
    /// Opens the ClickHouse connection immediately.
    /// </summary>
    /// <param name="appSettings">Application settings containing connection string</param>
    /// <param name="logger">Logger instance for logging operations</param>
    public RtdsContext(IRtdsAppSettings appSettings, ILogger<RtdsContext> logger)
    {
        _logger = logger;
        _connection = new ClickHouseConnection(appSettings.RtsConnectionString);
        _connection.Open();
    }

    /// <summary>
    /// Finalizer for releasing unmanaged resources.
    /// </summary>
    ~RtdsContext()
    {
        Dispose(false);
    }

    /// <summary>
    /// Creates the database if it doesn't exist.
    /// </summary>
    /// <param name="databaseName">The name of the database to create.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task EnsureDatabaseCreatedAsync(string databaseName = "data")
    {
        var sql = $"CREATE DATABASE IF NOT EXISTS {databaseName}";
        await using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Asynchronously creates a new tag table in the RTDS using ClickHouse.
    /// </summary>
    /// <param name="valueType">The type of value stored in the tag table.</param>
    /// <param name="statusType">The type of status stored in the tag table.</param>
    /// <exception cref="InvalidOperationException">Thrown when a table with the same name already exists.</exception>
    public async Task CreateTagTableAsync(string valueType, string statusType)
    {
        await EnsureDatabaseCreatedAsync(); // Ensure database exists before creating table
        var sql = string.Format(QueryConstants.CreateIntTagValue, valueType, statusType);
        await using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Inserts a collection of numeric values into the tag table
    /// </summary>
    /// <param name="values">List of value DTOs containing data to insert</param>
    /// <returns>Task representing the asynchronous insert operation</returns>
    public async Task InsertValues(List<InsertValueDto<double>> values)
    {
        try
        {
            var valuesForQuery = values.Aggregate("",
                (current, value) =>
                    current + string.Format(
                        $",({value.Id}, '{value.Time.UtcDateTime:yyyy-MM-dd HH:mm:ss.fff}', {value.Value}, '{value.Status}')"));
            valuesForQuery = valuesForQuery.Remove(0, 1);

            var commandText = string.Format(QueryConstants.InsertIntoQuery, values[0].ValueType, valuesForQuery);
            await using var cmd = _connection.CreateCommand();
            cmd.CommandText = commandText;
            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while inserting values");
        }
    }

    #region IDisposable Support

    /// <summary>
    /// Releases the unmanaged and managed resources used by the <see cref="RtdsContext"/> instance.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the managed and optionally unmanaged resources used by the <see cref="RtdsContext"/>.
    /// </summary>
    /// <param name="disposing">
    /// True to release both managed and unmanaged resources; false to release only unmanaged resources.
    /// </param>
    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _connection.Close();
            _connection.Dispose();
        }

        _disposed = true;
    }

    /// <summary>
    /// Asynchronously releases the unmanaged and managed resources used by the <see cref="RtdsContext"/> instance.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
        _disposed = true;
    }

    /// <summary>
    /// Performs the core asynchronous logic for releasing managed resources.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous dispose operation.</returns>
    private async ValueTask DisposeAsyncCore()
    {
        await _connection.CloseAsync();
        await _connection.DisposeAsync();
    }

    #endregion
}

/// <summary>
/// Represents a data transfer object for inserting tag values
/// </summary>
/// <typeparam name="T">Type of the value data</typeparam>
/// <param name="Id">Tag identifier</param>
/// <param name="ValueType">Type of the tag value</param>
/// <param name="Time">Timestamp of the value</param>
/// <param name="Value">The actual value data</param>
/// <param name="Status">Status of the tag value</param>
public record InsertValueDto<T>(uint Id, TagTypes ValueType, DateTimeOffset Time, T Value, TagValueStatus Status);