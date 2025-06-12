namespace Oip.Rtds.Data.Enums;

/// <summary>
/// Represents the status of a tag value.
/// </summary>
public enum TagValueStatus
{
    /// <summary>
    /// The value is valid and good.
    /// </summary>
    Good = 0,

    /// <summary>
    /// The value is questionable; may be from a backup source or have quality issues.
    /// </summary>
    Questionable = -1,

    /// <summary>
    /// The value was substituted (e.g., manually or by a system process).
    /// </summary>
    Substituted = -2,

    /// <summary>
    /// The value was interpolated between two known data points.
    /// </summary>
    Interpolated = -3,

    /// <summary>
    /// No data is available for the requested time or range.
    /// </summary>
    NoData = -4,

    /// <summary>
    /// The data source or interface returned a bad or corrupt value.
    /// </summary>
    Bad = -5,

    /// <summary>
    /// The PI Server or interface was shut down.
    /// </summary>
    Shutdown = -6,

    /// <summary>
    /// A calculation failed, e.g., in a formula tag.
    /// </summary>
    CalcFailed = -7,

    /// <summary>
    /// Data was expected but did not arrive in time.
    /// </summary>
    Timeout = -8,

    /// <summary>
    /// The value is outside of the configured low or high range.
    /// </summary>
    OutOfRange = -9
}
