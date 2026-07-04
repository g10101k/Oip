namespace Oip.Data.Dtos;

/// <summary>
/// Column description returned by an extension-aware table endpoint.
/// </summary>
public class ExtensionTableColumnDto
{
    /// <summary>
    /// API field name.
    /// </summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// Display header.
    /// </summary>
    public string Header { get; set; } = string.Empty;

    /// <summary>
    /// Field value type.
    /// </summary>
    public ExtensionFieldType Type { get; set; }

    /// <summary>
    /// Whether this is a fixed base entity column.
    /// </summary>
    public bool IsBase { get; set; }

    /// <summary>
    /// Whether the column is visible.
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Whether the column can be sorted.
    /// </summary>
    public bool IsSortable { get; set; } = true;

    /// <summary>
    /// Whether the column can be filtered.
    /// </summary>
    public bool IsFilterable { get; set; } = true;

    /// <summary>
    /// Display order.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Optional width.
    /// </summary>
    public string? Width { get; set; }

    /// <summary>
    /// Optional display format.
    /// </summary>
    public string? Format { get; set; }

    /// <summary>
    /// Discrete field options.
    /// </summary>
    public List<ExtensionFieldOptionDto> Options { get; set; } = [];
}
