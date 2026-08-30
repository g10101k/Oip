using Oip.Rtds.Base;
using Oip.Rtds.Grpc;

namespace Oip.Rtds.Test;

[TestFixture]
public class TagTypeMapTests
{
    private static IEnumerable<TagTypes> AllTagTypes => Enum.GetValues<TagTypes>();

    [Test]
    public void EveryTagType_IsMapped()
    {
        foreach (var tagType in AllTagTypes)
        {
            Assert.DoesNotThrow(() => TagTypeMap.GetTableSuffix(tagType), $"table suffix of {tagType}");
            Assert.DoesNotThrow(() => TagTypeMap.GetClickHouseType(tagType), $"ClickHouse type of {tagType}");
            Assert.DoesNotThrow(() => TagTypeMap.GetValueCodec(tagType), $"codec of {tagType}");
            Assert.DoesNotThrow(() => TagTypeMap.GetClrType(tagType), $"CLR type of {tagType}");
            Assert.DoesNotThrow(() => TagTypeMap.GetCsharpTypeName(tagType), $"C# type name of {tagType}");
            Assert.DoesNotThrow(() => TagTypeMap.GetDefaultValue(tagType), $"default value of {tagType}");
        }
    }

    [Test]
    public void TableSuffixes_AreUnique()
    {
        var suffixes = AllTagTypes.Select(TagTypeMap.GetTableSuffix).ToList();

        Assert.That(suffixes, Is.Unique);
    }

    [Test]
    public void DefaultValue_HasTheClrTypeOfTheTag()
    {
        foreach (var tagType in AllTagTypes)
            Assert.That(TagTypeMap.GetDefaultValue(tagType), Is.TypeOf(TagTypeMap.GetClrType(tagType)),
                $"default value of {tagType}");
    }

    [TestCase(TagTypes.Float32, 1.5, typeof(float))]
    [TestCase(TagTypes.Float64, 1.5, typeof(double))]
    [TestCase(TagTypes.Int8, 7, typeof(sbyte))]
    [TestCase(TagTypes.Int16, 7, typeof(short))]
    [TestCase(TagTypes.Int32, 7, typeof(int))]
    [TestCase(TagTypes.Int64, 7, typeof(long))]
    [TestCase(TagTypes.Uint8, 7, typeof(byte))]
    [TestCase(TagTypes.Uint16, 7, typeof(ushort))]
    [TestCase(TagTypes.Uint32, 7, typeof(uint))]
    [TestCase(TagTypes.Uint64, 7, typeof(ulong))]
    [TestCase(TagTypes.Digital, 3, typeof(byte))]
    [TestCase(TagTypes.Boolean, true, typeof(bool))]
    [TestCase(TagTypes.String, "text", typeof(string))]
    public void ConvertValue_ProducesTheClrTypeOfTheTag(TagTypes tagType, object value, Type expected)
    {
        Assert.That(TagTypeMap.ConvertValue(value, tagType), Is.TypeOf(expected));
    }

    [Test]
    public void ConvertValue_KeepsFloat64Precision()
    {
        const double value = 123.45;

        Assert.That(TagTypeMap.ConvertValue(value, TagTypes.Float64), Is.EqualTo(value));
    }

    [Test]
    public void ConvertValue_OfNull_ReturnsTheDefaultValue()
    {
        Assert.That(TagTypeMap.ConvertValue(null, TagTypes.String), Is.EqualTo(string.Empty));
        Assert.That(TagTypeMap.ConvertValue(null, TagTypes.Int32), Is.EqualTo(0));
    }

    [Test]
    public void ConvertTo_ConvertsBetweenDateTimeAndDateTimeOffset()
    {
        var offset = new DateTimeOffset(2026, 8, 30, 10, 0, 0, TimeSpan.Zero);

        Assert.That(TagTypeMap.ConvertTo(offset, typeof(DateTime)), Is.EqualTo(offset.UtcDateTime));
        Assert.That(TagTypeMap.ConvertTo(offset.UtcDateTime, typeof(DateTimeOffset)), Is.EqualTo(offset));
    }
}
