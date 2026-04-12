using System.Text.Json;

namespace Oip.Data.Dtos;

/// <summary>
/// Represents a server-side table query request coming from PrimeNG p-table lazy loading.
/// </summary>
public class TableQueryRequest
{
    /// <summary>
    /// Number of records to skip from the beginning of the result set.
    /// </summary>
    public int First { get; set; }

    /// <summary>
    /// Number of rows to return.
    /// </summary>
    public int Rows { get; set; } = 25;

    /// <summary>
    /// Requested field for sorting.
    /// </summary>
    public string? SortField { get; set; }

    /// <summary>
    /// Sort direction, where values less than zero mean descending.
    /// </summary>
    public int SortOrder { get; set; } = 1;

    /// <summary>
    /// Optional global filter value applied across configured fields.
    /// </summary>
    public string? GlobalFilter { get; set; }

    /// <summary>
    /// Per-column filters keyed by field name.
    /// </summary>
    public Dictionary<string, JsonElement> Filters { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// Represents a PrimeNG column filter definition.
/// </summary>
public class TableColumnFilter
{
    /// <summary>
    /// Logical operator used to combine constraints.
    /// </summary>
    public string? Operator { get; set; }

    /// <summary>
    /// Legacy single-value filter payload.
    /// </summary>
    public JsonElement? Value { get; set; }

    /// <summary>
    /// Legacy single-value match mode.
    /// </summary>
    public string? MatchMode { get; set; }

    /// <summary>
    /// Multi-constraint filter payload used by PrimeNG menus.
    /// </summary>
    public List<TableFilterConstraint> Constraints { get; set; } = new();
}

/// <summary>
/// Represents a single column filter constraint.
/// </summary>
public class TableFilterConstraint
{
    /// <summary>
    /// Constraint value.
    /// </summary>
    public JsonElement? Value { get; set; }

    /// <summary>
    /// Match mode that should be applied to the value.
    /// </summary>
    public string? MatchMode { get; set; }
}
