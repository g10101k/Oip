using System.Data;
using System.Globalization;
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
    private readonly SemaphoreSlim _connectionGate = new(1, 1);
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="RtdsContext"/> class using the provided application settings.
    /// The connection is opened lazily on the first operation.
    /// </summary>
    /// <param name="appSettings">Application settings containing connection string</param>
    /// <param name="logger">Logger instance for logging operations</param>
    public RtdsContext(IRtdsAppSettings appSettings, ILogger<RtdsContext> logger)
    {
        _logger = logger;
        _connection = new ClickHouseConnection(appSettings.RtsConnectionString);
    }

    /// <summary>
    /// Returns an open ClickHouse connection, opening or reopening it when required.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>An open connection.</returns>
    private async Task<ClickHouseConnection> GetOpenConnectionAsync(CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_connection.State == ConnectionState.Open)
            return _connection;

        await _connectionGate.WaitAsync(cancellationToken);
        try
        {
            if (_connection.State == ConnectionState.Broken)
            {
                _logger.LogWarning("ClickHouse connection is broken, reopening");
                await _connection.CloseAsync();
            }

            if (_connection.State != ConnectionState.Open)
                await _connection.OpenAsync(cancellationToken);

            return _connection;
        }
        finally
        {
            _connectionGate.Release();
        }
    }

    /// <summary>
    /// Creates the database if it doesn't exist.
    /// </summary>
    /// <param name="databaseName">The name of the database to create.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task EnsureDatabaseCreatedAsync(string databaseName = "data",
        CancellationToken cancellationToken = default)
    {
        var connection = await GetOpenConnectionAsync(cancellationToken);
        var sql = $"CREATE DATABASE IF NOT EXISTS {databaseName}";
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <summary>
    /// Asynchronously creates a new tag table in the RTDS using ClickHouse.
    /// </summary>
    /// <param name="valueType">The type of value stored in the tag table.</param>
    /// <param name="statusType">The type of status stored in the tag table.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <exception cref="InvalidOperationException">Thrown when a table with the same name already exists.</exception>
    public async Task CreateTagTableAsync(string valueType, string statusType,
        CancellationToken cancellationToken = default)
    {
        await EnsureDatabaseCreatedAsync(cancellationToken: cancellationToken); // Ensure database exists before creating table
        var connection = await GetOpenConnectionAsync(cancellationToken);
        var sql = string.Format(QueryConstants.CreateIntTagValue, valueType, statusType);
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <summary>
    /// Inserts a collection of numeric values into the tag table.
    /// All values must share the same <see cref="InsertValueDto{T}.ValueType"/>.
    /// </summary>
    /// <param name="values">List of value DTOs containing data to insert</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Task representing the asynchronous insert operation</returns>
    public async Task InsertValues(List<InsertValueDto<double>> values,
        CancellationToken cancellationToken = default)
    {
        if (values.Count == 0)
            return;

        var connection = await GetOpenConnectionAsync(cancellationToken);
        var commandText = string.Format(QueryConstants.InsertIntoQuery, values[0].ValueType);
        await using var writer = await connection.CreateColumnWriterAsync(commandText, cancellationToken);

        var valueColumnType = writer.GetFieldType(writer.GetOrdinal("Value"));
        var ids = new uint[values.Count];
        var times = new DateTimeOffset[values.Count];
        var columnValues = new object[values.Count];
        var statuses = new string[values.Count];

        for (var i = 0; i < values.Count; i++)
        {
            var value = values[i];
            ids[i] = value.Id;
            times[i] = value.Time;
            columnValues[i] = Convert.ChangeType(value.Value, valueColumnType, CultureInfo.InvariantCulture);
            statuses[i] = value.Status.ToString();
        }

        await writer.WriteTableAsync(new object[] { ids, times, columnValues, statuses }, values.Count,
            cancellationToken);
        await writer.EndWriteAsync(cancellationToken);
    }

    #region IDisposable Support

    /// <summary>
    /// Releases the managed resources used by the <see cref="RtdsContext"/> instance.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _connection.Dispose();
        _connectionGate.Dispose();
        _disposed = true;
    }

    /// <summary>
    /// Asynchronously releases the managed resources used by the <see cref="RtdsContext"/> instance.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        await _connection.DisposeAsync();
        _connectionGate.Dispose();
        _disposed = true;
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
