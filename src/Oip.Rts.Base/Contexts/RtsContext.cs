using Octonica.ClickHouseClient;
using Dapper;
using Oip.Rts.Base.Settings;

namespace Oip.Rts.Base.Contexts;

/// <summary>
/// Real time storage context
/// </summary>
public class RtsContext : IDisposable, IAsyncDisposable
{
    private readonly ClickHouseConnection _connection;
    private bool _disposed;

    /// <summary>
    /// .ctor
    /// </summary>
    /// <param name="appSettings"></param>
    public RtsContext(AppSettings appSettings)
    {
        _connection = new ClickHouseConnection(appSettings.RtsConnectionString);
        _connection.Open();
    }

    /// <summary>
    /// Get ta
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public List<Tag> GetTagsByFilter(string filter)
    {
        return _connection.Query<Tag>(
            QueryConstants.SelectFromDefaultTagWhereNameLikeFilter,
            new { filter }
        ).ToList();
    }

    public void AddTag(Tag tag)
    {
        // Реализация добавления тега
    }

    // Реализация IDisposable
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
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

    // Реализация IAsyncDisposable
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
        _disposed = true;
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        await _connection.CloseAsync();
        await _connection.DisposeAsync();
    }
}