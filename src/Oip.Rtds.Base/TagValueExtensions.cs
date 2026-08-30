using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Oip.Rtds.Grpc;

namespace Oip.Rtds.Base;

/// <summary>
/// Reads and writes the value carried by the <c>value</c> oneof of the tag messages,
/// so that callers work with a CLR value instead of switching over the oneof cases themselves.
/// </summary>
public static class TagValueExtensions
{
    /// <summary>
    /// Returns the value carried by the tag, or <c>null</c> when no value case is set.
    /// </summary>
    /// <param name="tag">The tag to read the value from.</param>
    /// <returns>The value, or <c>null</c> when the tag carries none.</returns>
    public static object? GetValue(this WriteDataTag tag) => tag.ValueCase switch
    {
        WriteDataTag.ValueOneofCase.DoubleValue => tag.DoubleValue,
        WriteDataTag.ValueOneofCase.FloatValue => tag.FloatValue,
        WriteDataTag.ValueOneofCase.Int32Value => tag.Int32Value,
        WriteDataTag.ValueOneofCase.Int64Value => tag.Int64Value,
        WriteDataTag.ValueOneofCase.Uint32Value => tag.Uint32Value,
        WriteDataTag.ValueOneofCase.Uint64Value => tag.Uint64Value,
        WriteDataTag.ValueOneofCase.BoolValue => tag.BoolValue,
        WriteDataTag.ValueOneofCase.StringValue => tag.StringValue,
        WriteDataTag.ValueOneofCase.BytesValue => tag.BytesValue.ToByteArray(),
        WriteDataTag.ValueOneofCase.TimestampValue => tag.TimestampValue.ToDateTimeOffset(),
        _ => null
    };

    /// <summary>
    /// Returns the last known value of the tag, or <c>null</c> when no value case is set.
    /// </summary>
    /// <param name="tag">The tag to read the value from.</param>
    /// <returns>The value, or <c>null</c> when the tag carries none.</returns>
    public static object? GetValue(this TagResponse tag) => tag.ValueCase switch
    {
        TagResponse.ValueOneofCase.DoubleValue => tag.DoubleValue,
        TagResponse.ValueOneofCase.FloatValue => tag.FloatValue,
        TagResponse.ValueOneofCase.Int32Value => tag.Int32Value,
        TagResponse.ValueOneofCase.Int64Value => tag.Int64Value,
        TagResponse.ValueOneofCase.Uint32Value => tag.Uint32Value,
        TagResponse.ValueOneofCase.Uint64Value => tag.Uint64Value,
        TagResponse.ValueOneofCase.BoolValue => tag.BoolValue,
        TagResponse.ValueOneofCase.StringValue => tag.StringValue,
        TagResponse.ValueOneofCase.BytesValue => tag.BytesValue.ToByteArray(),
        TagResponse.ValueOneofCase.TimestampValue => tag.TimestampValue.ToDateTimeOffset(),
        _ => null
    };

    /// <summary>
    /// Sets the value case matching <paramref name="tagType"/>, converting the value to that type first.
    /// </summary>
    /// <param name="tag">The tag to write the value to.</param>
    /// <param name="tagType">The declared type of the tag.</param>
    /// <param name="value">The value to write.</param>
    /// <exception cref="NotSupportedException">Thrown when the tag type is not supported.</exception>
    public static void SetValue(this WriteDataTag tag, TagTypes tagType, object? value)
    {
        var converted = TagTypeMap.ConvertValue(value, tagType);
        switch (tagType)
        {
            case TagTypes.Float32:
                tag.FloatValue = (float)converted;
                break;
            case TagTypes.Float64:
                tag.DoubleValue = (double)converted;
                break;
            case TagTypes.Int8:
                tag.Int32Value = (sbyte)converted;
                break;
            case TagTypes.Int16:
                tag.Int32Value = (short)converted;
                break;
            case TagTypes.Int32:
                tag.Int32Value = (int)converted;
                break;
            case TagTypes.Int64:
                tag.Int64Value = (long)converted;
                break;
            case TagTypes.Uint8:
            case TagTypes.Digital:
                tag.Uint32Value = (byte)converted;
                break;
            case TagTypes.Uint16:
                tag.Uint32Value = (ushort)converted;
                break;
            case TagTypes.Uint32:
                tag.Uint32Value = (uint)converted;
                break;
            case TagTypes.Uint64:
                tag.Uint64Value = (ulong)converted;
                break;
            case TagTypes.Boolean:
                tag.BoolValue = (bool)converted;
                break;
            case TagTypes.DateTime:
                tag.TimestampValue = Timestamp.FromDateTimeOffset((DateTimeOffset)converted);
                break;
            case TagTypes.String:
                tag.StringValue = (string)converted;
                break;
            case TagTypes.Blob:
                tag.BytesValue = ByteString.CopyFrom((byte[])converted);
                break;
            default:
                throw new NotSupportedException($"Unsupported TagType: {tagType}");
        }
    }

    /// <summary>
    /// Sets the value case matching <paramref name="tagType"/>, converting the value to that type first.
    /// </summary>
    /// <param name="tag">The tag to write the value to.</param>
    /// <param name="tagType">The declared type of the tag.</param>
    /// <param name="value">The value to write.</param>
    /// <exception cref="NotSupportedException">Thrown when the tag type is not supported.</exception>
    public static void SetValue(this TagResponse tag, TagTypes tagType, object? value)
    {
        var converted = TagTypeMap.ConvertValue(value, tagType);
        switch (tagType)
        {
            case TagTypes.Float32:
                tag.FloatValue = (float)converted;
                break;
            case TagTypes.Float64:
                tag.DoubleValue = (double)converted;
                break;
            case TagTypes.Int8:
                tag.Int32Value = (sbyte)converted;
                break;
            case TagTypes.Int16:
                tag.Int32Value = (short)converted;
                break;
            case TagTypes.Int32:
                tag.Int32Value = (int)converted;
                break;
            case TagTypes.Int64:
                tag.Int64Value = (long)converted;
                break;
            case TagTypes.Uint8:
            case TagTypes.Digital:
                tag.Uint32Value = (byte)converted;
                break;
            case TagTypes.Uint16:
                tag.Uint32Value = (ushort)converted;
                break;
            case TagTypes.Uint32:
                tag.Uint32Value = (uint)converted;
                break;
            case TagTypes.Uint64:
                tag.Uint64Value = (ulong)converted;
                break;
            case TagTypes.Boolean:
                tag.BoolValue = (bool)converted;
                break;
            case TagTypes.DateTime:
                tag.TimestampValue = Timestamp.FromDateTimeOffset((DateTimeOffset)converted);
                break;
            case TagTypes.String:
                tag.StringValue = (string)converted;
                break;
            case TagTypes.Blob:
                tag.BytesValue = ByteString.CopyFrom((byte[])converted);
                break;
            default:
                throw new NotSupportedException($"Unsupported TagType: {tagType}");
        }
    }
}
