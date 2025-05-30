using Octonica.ClickHouseClient;
using Dapper;
using Oip.Rts.Base.Enums;
using Oip.Rts.Base.Settings;

namespace Oip.Rts.Base.Contexts;

/// <summary>
/// Provides a context for interacting with the real-time storage (RTS) using ClickHouse.
/// </summary>
public sealed class RtsContext : IDisposable, IAsyncDisposable
{
    private readonly ClickHouseConnection _connection;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="RtsContext"/> class using the provided application settings.
    /// Opens the ClickHouse connection immediately.
    /// </summary>
    /// <param name="appSettings">Application settings containing the RTS connection string.</param>
    public RtsContext(AppSettings appSettings)
    {
        _connection = new ClickHouseConnection(appSettings.RtsConnectionString);
        _connection.Open();
    }

    /// <summary>
    /// Retrieves a list of <see cref="TagEntity"/> objects from the database that match the specified filter.
    /// </summary>
    /// <param name="filter">A string filter to apply to tag names (e.g., "Sensor%").</param>
    /// <returns>A list of <see cref="TagEntity"/> objects matching the filter.</returns>
    public List<TagEntity> GetTagsByFilter(string filter)
    {
        return _connection.Query<TagEntity>(
            QueryConstants.SelectFromDefaultTagWhereNameLikeFilter,
            new { filter }
        ).ToList();
    }

    /// <summary>
    /// Adds a tag to the database.
    /// </summary>
    /// <param name="tag">The tag to add.</param>
    public void AddTag(TagEntity tag)
    {
        CheckTagName(tag.Name);
        var nextId = _connection.ExecuteScalar<uint?>(QueryConstants.GetMaxTagIdSql) ?? 0;
        tag.TagId = nextId;
        _connection.Execute(QueryConstants.InsertTagSql, new
        {
            tag.TagId,
            tag.Name,
            tag.ValueType,
            tag.Source,
            tag.Descriptor,
            tag.EngUnits,
            tag.InstrumentTag,
            tag.Archiving,
            tag.Compressing,
            tag.ExcDev,
            tag.ExcMin,
            tag.ExcMax,
            tag.CompDev,
            tag.CompMin,
            tag.CompMax,
            tag.Zero,
            tag.Span,
            tag.Location1,
            tag.Location2,
            tag.Location3,
            tag.Location4,
            tag.Location5,
            tag.ExDesc,
            tag.Scan,
            tag.DigitalSet,
            tag.Step,
            tag.Future,
            tag.UserInt1,
            tag.UserInt2,
            tag.UserInt3,
            tag.UserInt4,
            tag.UserInt5,
            tag.UserReal1,
            tag.UserReal2,
            tag.UserReal3,
            tag.UserReal4,
            tag.UserReal5,
            tag.CreationDate,
            tag.Creator
        });

        CreateTagTableAsync(tag).Wait();
    }

    private void CheckTagName(string tagName)
    {
        var q = _connection.ExecuteScalar<int>(QueryConstants.CheckTagNameSql, new { tagName });
        if (q > 0)
            throw new InvalidOperationException($"Tag {tagName} already exists");
    }

    public async Task CreateTagTableAsync(TagEntity tag)
    {
        var tableName = $"{tag.TagId:D6}";
        var valueType = GetClickHouseTypeFromTagType(tag.ValueType);
        var statusType = GenerateClickHouseEnum8<TagValueStatus>();
        var sql = string.Format(QueryConstants.CreateTagTableSql, tableName, valueType, statusType, tag.Partition);
        await using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;
        await cmd.ExecuteNonQueryAsync();
    }

    private static string GenerateClickHouseEnum8<TEnum>() where TEnum : Enum
    {
        var values = Enum.GetValues(typeof(TEnum)).Cast<TEnum>();
        var entries = values.Select(v => $"'{v}' = {(int)(object)v}");
        return "Enum8(" + string.Join(", ", entries) + ")";
    }

    private static string GetClickHouseTypeFromTagType(TagTypes pointType)
    {
        return pointType switch
        {
            TagTypes.Float32 => "Float32",
            TagTypes.Float64 => "Float64",
            TagTypes.Int16 => "Int32",
            TagTypes.Int32 => "Int32",
            TagTypes.Digital => "UInt8",
            TagTypes.String => "String",
            TagTypes.Blob => "String", // Можно также использовать FixedString(N) или Base64
            _ => throw new NotSupportedException($"Unsupported TagType: {pointType}")
        };
    }


    #region IDisposable Support

    /// <summary>
    /// Releases the unmanaged and managed resources used by the <see cref="RtsContext"/> instance.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the managed and optionally unmanaged resources used by the <see cref="RtsContext"/>.
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
    /// Asynchronously releases the unmanaged and managed resources used by the <see cref="RtsContext"/> instance.
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