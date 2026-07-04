namespace Oip.Data.Dtos;

/// <summary>
/// Supported physical extension field value types.
/// </summary>
public enum ExtensionFieldType
{
    /// <summary>
    /// Text value.
    /// </summary>
    Text = 0,

    /// <summary>
    /// Numeric value.
    /// </summary>
    Number = 1,

    /// <summary>
    /// Boolean value.
    /// </summary>
    Boolean = 2,

    /// <summary>
    /// Date value.
    /// </summary>
    Date = 3,

    /// <summary>
    /// Date and time value.
    /// </summary>
    DateTime = 4,

    /// <summary>
    /// Discrete option value.
    /// </summary>
    Select = 5
}
