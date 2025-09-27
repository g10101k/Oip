namespace Oip.Rtds.Data.Enums;

/// <summary>
/// Defines the data types supported for tags
/// </summary>
/// <remarks>
/// These types correspond to standard RTDS point types
/// </remarks>
public enum TagTypes
{
    /// <summary>
    /// 32-bit floating point number (single precision)
    /// </summary>
    Float32 = 0,
        
    /// <summary>
    /// 64-bit floating point number (double precision)
    /// </summary>
    Float64 = 1,
        
    /// <summary>
    /// 16-bit signed integer
    /// </summary>
    Int16 = 2,
        
    /// <summary>
    /// 32-bit signed integer
    /// </summary>
    Int32 = 3,
        
    /// <summary>
    /// Digital (boolean) value (0/1)
    /// </summary>
    Digital = 4,
        
    /// <summary>
    /// String/text data
    /// </summary>
    String = 5,
        
    /// <summary>
    /// Binary large object (blob) - for arbitrary binary data
    /// </summary>
    Blob = 6
}