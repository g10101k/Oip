using Octonica.ClickHouseClient;
using Oip.Rtds.Data.Settings;

namespace Oip.Rtds.Data.Contexts;

/// <summary>
/// ClickHouse database context for real-time data storage operations.
/// </summary>
/// <remarks>
/// Manages database connections and provides methods for tag table creation.
/// Handles both synchronous and asynchronous resource cleanup.
/// </remarks>
public sealed class RtdsContext : IDisposable, IAsyncDisposable
{
    private readonly ClickHouseConnection _connection;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="RtdsContext"/> class using the provided application settings.
    /// Opens the ClickHouse connection immediately.
    /// </summary>
    /// <param name="appSettings">Application configuration containing connection string.</param>
    /// <remarks>
    /// Immediately opens a connection to the ClickHouse database.
    /// </remarks>
    public RtdsContext(AppSettings appSettings)
    {
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
    /// Asynchronously creates a new tag table in the RTDS using ClickHouse.
    /// </summary>
    /// <param name="tableName">The name of the tag table to create.</param>
    /// <param name="valueType">The type of value stored in the tag table.</param>
    /// <param name="statusType">The type of status stored in the tag table.</param>
    /// <param name="partition">The partition for the tag table.</param>
    /// <exception cref="InvalidOperationException">Thrown when a table with the same name already exists.</exception>
    public async Task CreateTagTableAsync(string tableName, string valueType, string statusType, string partition)
    {
        var sql = string.Format(QueryConstants.CreateTagTableSql, tableName, valueType, statusType, partition);
        await using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;
        await cmd.ExecuteNonQueryAsync();
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