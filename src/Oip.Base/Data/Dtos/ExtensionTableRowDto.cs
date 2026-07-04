namespace Oip.Data.Dtos;

/// <summary>
/// Base row shape for extension-aware tables.
/// </summary>
public class ExtensionTableRowDto
{
    /// <summary>
    /// Extension values keyed by metadata field name.
    /// </summary>
    public Dictionary<string, object?> ExtensionValues { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
