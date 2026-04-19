namespace Oip.Data.Services;

/// <summary>
/// Configures allowed field mappings and paging limits for server-side table queries.
/// </summary>
public class TableQueryOptions
{
    /// <summary>
    /// Maps incoming field names to entity property paths.
    /// </summary>
    public Dictionary<string, string> FieldMappings { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Property paths used by the global filter.
    /// </summary>
    public List<string> GlobalFilterFields { get; } = new();

    /// <summary>
    /// Default page size used when the request does not provide one.
    /// </summary>
    public int DefaultRows { get; set; } = 25;

    /// <summary>
    /// Maximum allowed page size.
    /// </summary>
    public int MaxRows { get; set; } = 200;

    /// <summary>
    /// Resolves an incoming field to the underlying entity property path.
    /// </summary>
    public string ResolveField(string fieldName)
    {
        return FieldMappings.TryGetValue(fieldName, out var mappedField) ? mappedField : fieldName;
    }
}
