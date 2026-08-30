using System.Collections.Concurrent;
using System.Data;
using Microsoft.Extensions.Logging;
using Octonica.ClickHouseClient;
using Oip.Rtds.Base;
using Oip.Rtds.Grpc;

namespace Oip.Rtds.Data.Contexts;

/// <summary>
/// ClickHouse database context for real-time data storage operations.
/// </summary>
public sealed class RtdsContext : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Databases already created by this process, keyed by connection string and database name.
    /// <c>CREATE DATABASE IF NOT EXISTS</c> is idempotent but still costs a round trip on every tag creation.
    /// </summary>
    private static readonly ConcurrentDictionary<string, bool> CreatedDatabases = new();

    private readonly ILogger<RtdsContext> _logger;
    private readonly string _connectionString;
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
        _connectionString = appSettings.RtsConnectionString;
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
    /// Creates the database if it doesn't exist. Skipped once this process has created it.
    /// </summary>
    /// <param name="databaseName">The name of the database to create.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task EnsureDatabaseCreatedAsync(string databaseName = "data",
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{_connectionString}|{databaseName}";
        if (CreatedDatabases.ContainsKey(cacheKey))
            return;

        var connection = await GetOpenConnectionAsync(cancellationToken);
        var sql = $"CREATE DATABASE IF NOT EXISTS {databaseName}";
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        await cmd.ExecuteNonQueryAsync(cancellationToken);
        CreatedDatabases[cacheKey] = true;
    }

    /// <summary>
    /// Asynchronously creates the value table of the given tag type, if it does not exist yet.
    /// </summary>
    /// <param name="tagType">The type of the values stored in the table.</param>
    /// <param name="statusType">The type of status stored in the tag table.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    public async Task CreateTagTableAsync(TagTypes tagType, string statusType,
        CancellationToken cancellationToken = default)
    {
        await EnsureDatabaseCreatedAsync(cancellationToken: cancellationToken); // Ensure database exists before creating table
        var connection = await GetOpenConnectionAsync(cancellationToken);
        var tableSuffix = TagTypeMap.GetTableSuffix(tagType);
        var sql = string.Format(QueryConstants.CreateTagValueTable, tableSuffix,
            TagTypeMap.GetClickHouseType(tagType), statusType, TagTypeMap.GetValueCodec(tagType),
            QueryConstants.DeduplicationWindow);
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        await cmd.ExecuteNonQueryAsync(cancellationToken);

        // The table may predate insert deduplication, in which case CREATE IF NOT EXISTS left the setting disabled.
        await using var alterCmd = connection.CreateCommand();
        alterCmd.CommandText = string.Format(QueryConstants.EnableDeduplication, tableSuffix,
            QueryConstants.DeduplicationWindow);
        await alterCmd.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <summary>
    /// Inserts a collection of values into the tag value table.
    /// All values must share the same <see cref="InsertValueDto.ValueType"/>.
    /// </summary>
    /// <param name="values">List of value DTOs containing data to insert</param>
    /// <param name="deduplicationToken">
    /// Token identifying the batch. Reusing it for a retry of the same batch lets ClickHouse discard the second
    /// copy of a block it has already written, so a retry after a lost response does not duplicate rows.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Task representing the asynchronous insert operation</returns>
    public async Task InsertValues(List<InsertValueDto> values, string deduplicationToken,
        CancellationToken cancellationToken = default)
    {
        if (values.Count == 0)
            return;

        var connection = await GetOpenConnectionAsync(cancellationToken);
        var commandText = string.Format(QueryConstants.InsertIntoQuery,
            TagTypeMap.GetTableSuffix(values[0].ValueType), deduplicationToken);
        await using var writer = await connection.CreateColumnWriterAsync(commandText, cancellationToken);

        var valueColumnType = writer.GetFieldType(writer.GetOrdinal("Value"));
        var ids = new uint[values.Count];
        var times = new DateTimeOffset[values.Count];
        // A typed array rather than object[]: the column writer needs the values in the type of the column,
        // and a mismatch has to fail here instead of being coerced silently.
        var columnValues = Array.CreateInstance(valueColumnType, values.Count);
        var statuses = new string[values.Count];

        for (var i = 0; i < values.Count; i++)
        {
            var value = values[i];
            ids[i] = value.Id;
            times[i] = value.Time;
            columnValues.SetValue(TagTypeMap.ConvertTo(value.Value, valueColumnType), i);
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
/// Represents a data transfer object for inserting tag values.
/// The value is carried as an object because a batch mixes tags of every supported type;
/// it is stored in the CLR type <see cref="TagTypeMap.GetClrType"/> returns for <paramref name="ValueType"/>.
/// </summary>
/// <param name="Id">Tag identifier</param>
/// <param name="ValueType">Type of the tag value</param>
/// <param name="Time">Timestamp of the value</param>
/// <param name="Value">The actual value data</param>
/// <param name="Status">Status of the tag value</param>
public record InsertValueDto(uint Id, TagTypes ValueType, DateTimeOffset Time, object Value, TagValueStatus Status);
