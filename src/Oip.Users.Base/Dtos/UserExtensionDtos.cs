using Oip.Data.Dtos;

namespace Oip.Users.Base.Dtos;

/// <summary>
/// User table row with dynamic extension values.
/// </summary>
public class UserExtensionTableRowDto : ExtensionTableRowDto
{
    /// <summary>
    /// User id.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// E-mail.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// First name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Whether the user is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Creation date and time.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>
/// Request to create a user extension field.
/// </summary>
public class CreateUserExtensionFieldRequest
{
    /// <summary>
    /// API field name.
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// Optional database column name. If omitted, field name is used.
    /// </summary>
    public string? DbColumn { get; set; }

    /// <summary>
    /// Field type.
    /// </summary>
    public ExtensionFieldType Type { get; set; }

    /// <summary>
    /// Discrete options for select/tag fields.
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
    /// Whether the field can be sorted.
    /// </summary>
    public bool IsSortable { get; set; } = true;

    /// <summary>
    /// Whether the field can be filtered.
    /// </summary>
    public bool IsFilterable { get; set; } = true;

    /// <summary>
    /// Display order.
    /// </summary>
    public int Order { get; set; }
}

/// <summary>
/// Request to update user extension field metadata.
/// </summary>
public class UpdateUserExtensionFieldRequest : CreateUserExtensionFieldRequest;

/// <summary>
/// Request to update one user's extension values.
/// </summary>
public class UpdateUserExtensionValuesRequest
{
    /// <summary>
    /// Values keyed by metadata field name.
    /// </summary>
    public Dictionary<string, object?> Values { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
