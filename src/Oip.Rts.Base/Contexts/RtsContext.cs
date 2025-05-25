using Octonica.ClickHouseClient;
using Dapper;
using Oip.Rts.Base.Settings;

namespace Oip.Rts.Base.Contexts;

/// Real time storage context
public class RtsContext : IDisposable, IAsyncDisposable
{
    private readonly ClickHouseConnection _connection;

    public RtsContext(AppSettings appSettings)
    {
        _connection = new ClickHouseConnection(appSettings.RtsConnectionString);
        _connection.Open();
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.CloseAsync();
        await _connection.DisposeAsync();
    }

    public void AddTag(Tag tag)
    {
    }

    public List<Tag> GetTagsByFilter(string filter)
    {
        return _connection.Query<Tag>("SELECT * FROM default.Tag WHERE Name like @filter", new { filter }).ToList();
    }
}