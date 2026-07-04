namespace Oip.Data.Dtos;

/// <summary>
/// Global metadata for a physical extension field.
/// </summary>
public class ExtensionFieldMetadataDto
{
    /// <summary>
    /// Metadata identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Base entity code, for example User.
    /// </summary>
    public string EntityCode { get; set; } = string.Empty;

    /// <summary>
    /// Extension table schema.
    /// </summary>
    public string TableSchema { get; set; } = string.Empty;

    /// <summary>
    /// Extension table name.
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// API field name.
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// Physical database column name.
    /// </summary>
    public string DbColumn { get; set; } = string.Empty;

    /// <summary>
    /// Field value type.
    /// </summary>
    public ExtensionFieldType Type { get; set; }

    /// <summary>
    /// Optional discrete options encoded in metadata.
    /// </summary>
    public List<ExtensionFieldOptionDto> Options { get; set; } = [];

    /// <summary>
    /// Whether a value is required.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Whether the field is visible by default.
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Whether the field can be sorted by the table API.
    /// </summary>
    public bool IsSortable { get; set; } = true;

    /// <summary>
    /// Whether the field can be filtered by the table API.
    /// </summary>
    public bool IsFilterable { get; set; } = true;

    /// <summary>
    /// Default display order.
    /// </summary>
    public int Order { get; set; }
}
