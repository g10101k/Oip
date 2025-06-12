using Dapper;
using Octonica.ClickHouseClient;
using Oip.Rtds.Data.Entities;
using Oip.Rtds.Data.Enums;
using Oip.Rtds.Data.Settings;

namespace Oip.Rtds.Data.Contexts;

/// <summary>
/// Provides a context for interacting with the real-time storage (RTS) using ClickHouse.
/// </summary>
public sealed class RtdsContext : IDisposable, IAsyncDisposable
{
    private readonly ClickHouseConnection _connection;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="RtdsContext"/> class using the provided application settings.
    /// Opens the ClickHouse connection immediately.
    /// </summary>
    /// <param name="appSettings">Application settings containing the RTS connection string.</param>
    public RtdsContext(AppSettings appSettings)
    {
        _connection = new ClickHouseConnection(appSettings.RtsConnectionString);
        _connection.Open();
    }

  

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