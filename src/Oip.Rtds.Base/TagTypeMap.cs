using System.Globalization;
using Oip.Rtds.Grpc;

namespace Oip.Rtds.Base;

/// <summary>
/// Single source of truth for how a <see cref="TagTypes"/> value is represented: in ClickHouse storage,
/// in compiled formulas and in memory.
/// </summary>
public static class TagTypeMap
{
    /// <summary>
    /// Returns the suffix of the ClickHouse table holding values of the given tag type,
    /// i.e. <c>data.{suffix}TagValue</c>.
    /// Every tag type gets its own table even when two of them share a column type, so that a value
    /// can always be read back as the type it was written with.
    /// </summary>
    /// <param name="tagType">The tag type to get the table suffix for.</param>
    /// <returns>The table name suffix.</returns>
    /// <exception cref="NotSupportedException">Thrown when the tag type is not supported.</exception>
    public static string GetTableSuffix(TagTypes tagType) => tagType switch
    {
        TagTypes.Float32 => "Float32",
        TagTypes.Float64 => "Float64",
        TagTypes.Int8 => "Int8",
        TagTypes.Int16 => "Int16",
        TagTypes.Int32 => "Int32",
        TagTypes.Int64 => "Int64",
        TagTypes.Uint8 => "UInt8",
        TagTypes.Uint16 => "UInt16",
        TagTypes.Uint32 => "UInt32",
        TagTypes.Uint64 => "UInt64",
        TagTypes.Boolean => "Bool",
        TagTypes.Digital => "Digital",
        TagTypes.DateTime => "DateTime",
        TagTypes.String => "String",
        TagTypes.Blob => "Blob",
        _ => throw new NotSupportedException($"Unsupported TagType: {tagType}")
    };

    /// <summary>
    /// Returns the ClickHouse column type used to store values of the given tag type.
    /// </summary>
    /// <param name="tagType">The tag type to get the ClickHouse type for.</param>
    /// <returns>The ClickHouse type expression.</returns>
    /// <exception cref="NotSupportedException">Thrown when the tag type is not supported.</exception>
    public static string GetClickHouseType(TagTypes tagType) => tagType switch
    {
        TagTypes.Float32 => "Float32",
        TagTypes.Float64 => "Float64",
        TagTypes.Int8 => "Int8",
        TagTypes.Int16 => "Int16",
        TagTypes.Int32 => "Int32",
        TagTypes.Int64 => "Int64",
        TagTypes.Uint8 => "UInt8",
        TagTypes.Uint16 => "UInt16",
        TagTypes.Uint32 => "UInt32",
        TagTypes.Uint64 => "UInt64",
        TagTypes.Boolean => "Bool",
        TagTypes.Digital => "UInt8",
        TagTypes.DateTime => "DateTime64(3, 'UTC')",
        TagTypes.String => "String",
        TagTypes.Blob => "String",
        _ => throw new NotSupportedException($"Unsupported TagType: {tagType}")
    };

    /// <summary>
    /// Returns the compression codec expression for the Value column of the given tag type.
    /// Floating point series compress best with Gorilla, integer series with T64, timestamps with DoubleDelta,
    /// strings and binaries with plain ZSTD.
    /// </summary>
    /// <param name="tagType">The tag type to get the codec for.</param>
    /// <returns>The ClickHouse codec expression.</returns>
    /// <exception cref="NotSupportedException">Thrown when the tag type is not supported.</exception>
    public static string GetValueCodec(TagTypes tagType) => tagType switch
    {
        TagTypes.Float32 => "CODEC(Gorilla, ZSTD(1))",
        TagTypes.Float64 => "CODEC(Gorilla, ZSTD(1))",
        TagTypes.Int8 => "CODEC(T64, ZSTD(1))",
        TagTypes.Int16 => "CODEC(T64, ZSTD(1))",
        TagTypes.Int32 => "CODEC(T64, ZSTD(1))",
        TagTypes.Int64 => "CODEC(T64, ZSTD(1))",
        TagTypes.Uint8 => "CODEC(T64, ZSTD(1))",
        TagTypes.Uint16 => "CODEC(T64, ZSTD(1))",
        TagTypes.Uint32 => "CODEC(T64, ZSTD(1))",
        TagTypes.Uint64 => "CODEC(T64, ZSTD(1))",
        TagTypes.Boolean => "CODEC(ZSTD(1))",
        TagTypes.Digital => "CODEC(T64, ZSTD(1))",
        TagTypes.DateTime => "CODEC(DoubleDelta, ZSTD(1))",
        TagTypes.String => "CODEC(ZSTD(1))",
        TagTypes.Blob => "CODEC(ZSTD(1))",
        _ => throw new NotSupportedException($"Unsupported TagType: {tagType}")
    };

    /// <summary>
    /// Returns the CLR type a value of the given tag type is carried as through the write pipeline
    /// and passed to compiled formulas.
    /// </summary>
    /// <param name="tagType">The tag type to get the CLR type for.</param>
    /// <returns>The CLR type.</returns>
    /// <exception cref="NotSupportedException">Thrown when the tag type is not supported.</exception>
    public static Type GetClrType(TagTypes tagType) => tagType switch
    {
        TagTypes.Float32 => typeof(float),
        TagTypes.Float64 => typeof(double),
        TagTypes.Int8 => typeof(sbyte),
        TagTypes.Int16 => typeof(short),
        TagTypes.Int32 => typeof(int),
        TagTypes.Int64 => typeof(long),
        TagTypes.Uint8 => typeof(byte),
        TagTypes.Uint16 => typeof(ushort),
        TagTypes.Uint32 => typeof(uint),
        TagTypes.Uint64 => typeof(ulong),
        TagTypes.Boolean => typeof(bool),
        TagTypes.Digital => typeof(byte),
        TagTypes.DateTime => typeof(DateTimeOffset),
        TagTypes.String => typeof(string),
        TagTypes.Blob => typeof(byte[]),
        _ => throw new NotSupportedException($"Unsupported TagType: {tagType}")
    };

    /// <summary>
    /// Returns the C# type name used for the value parameter of a compiled formula of the given tag type.
    /// </summary>
    /// <param name="tagType">The tag type to get the C# type name for.</param>
    /// <returns>The C# type name.</returns>
    /// <exception cref="NotSupportedException">Thrown when the tag type is not supported.</exception>
    public static string GetCsharpTypeName(TagTypes tagType) => tagType switch
    {
        TagTypes.Float32 => "float",
        TagTypes.Float64 => "double",
        TagTypes.Int8 => "sbyte",
        TagTypes.Int16 => "short",
        TagTypes.Int32 => "int",
        TagTypes.Int64 => "long",
        TagTypes.Uint8 => "byte",
        TagTypes.Uint16 => "ushort",
        TagTypes.Uint32 => "uint",
        TagTypes.Uint64 => "ulong",
        TagTypes.Boolean => "bool",
        TagTypes.Digital => "byte",
        TagTypes.DateTime => "DateTimeOffset",
        TagTypes.String => "string",
        TagTypes.Blob => "byte[]",
        _ => throw new NotSupportedException($"Unsupported TagType: {tagType}")
    };

    /// <summary>
    /// Indicates whether values of the given tag type can be compared numerically,
    /// which is what deadband compression needs.
    /// </summary>
    /// <param name="tagType">The tag type to check.</param>
    /// <returns><c>true</c> for numeric types, <c>false</c> otherwise.</returns>
    public static bool IsNumeric(TagTypes tagType) => tagType switch
    {
        TagTypes.String or TagTypes.Blob or TagTypes.Boolean or TagTypes.DateTime => false,
        _ => true
    };

    /// <summary>
    /// Returns the value stored for a tag that has no value, used where the storage column is not nullable.
    /// An absent value is encoded by the status column rather than by the value itself.
    /// </summary>
    /// <param name="tagType">The tag type to get the placeholder for.</param>
    /// <returns>The placeholder value.</returns>
    public static object GetDefaultValue(TagTypes tagType) => tagType switch
    {
        TagTypes.String => string.Empty,
        TagTypes.Blob => Array.Empty<byte>(),
        TagTypes.DateTime => DateTimeOffset.UnixEpoch,
        TagTypes.Boolean => false,
        _ => System.Convert.ChangeType(0, GetClrType(tagType), CultureInfo.InvariantCulture)
    };

    /// <summary>
    /// Converts a value to the representation of the given tag type, so that a value produced by a formula
    /// or received over the wire is stored as the type the tag was declared with.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="tagType">The tag type to convert to.</param>
    /// <returns>The converted value.</returns>
    /// <exception cref="InvalidCastException">Thrown when the value cannot be converted to the tag type.</exception>
    public static object ConvertValue(object? value, TagTypes tagType)
    {
        if (value is null)
            return GetDefaultValue(tagType);

        return ConvertTo(value, GetClrType(tagType));
    }

    /// <summary>
    /// Converts a value to the requested CLR type, handling the conversions
    /// <see cref="System.Convert.ChangeType(object, Type)"/> does not support.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="target">The type to convert to.</param>
    /// <returns>The converted value.</returns>
    /// <exception cref="InvalidCastException">Thrown when the value cannot be converted to the target type.</exception>
    public static object ConvertTo(object value, Type target)
    {
        target = Nullable.GetUnderlyingType(target) ?? target;

        if (target.IsInstanceOfType(value))
            return value;

        return (value, Type.GetTypeCode(target)) switch
        {
            (DateTimeOffset offset, _) when target == typeof(DateTime) => offset.UtcDateTime,
            (DateTime dateTime, _) when target == typeof(DateTimeOffset) => new DateTimeOffset(
                DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)),
            (_, TypeCode.Object) => throw new InvalidCastException(
                $"Cannot convert a value of type {value.GetType()} to {target}"),
            (_, TypeCode.String) => System.Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty,
            _ => System.Convert.ChangeType(value, target, CultureInfo.InvariantCulture)
        };
    }
}
