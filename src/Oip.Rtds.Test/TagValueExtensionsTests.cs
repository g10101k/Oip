using Oip.Rtds.Base;
using Oip.Rtds.Grpc;

namespace Oip.Rtds.Test;

[TestFixture]
public class TagValueExtensionsTests
{
    /// <summary>
    /// The narrow types ride in the widest wire case that fits them, so the value that comes back off the wire
    /// is only expected to be equal to what went in, not to already have the CLR type of the tag.
    /// </summary>
    private static IEnumerable<TestCaseData> ValueCases()
    {
        yield return new TestCaseData(TagTypes.Float32, 1.5, 1.5f);
        yield return new TestCaseData(TagTypes.Float64, 1.5, 1.5d);
        yield return new TestCaseData(TagTypes.Int8, 7, 7);
        yield return new TestCaseData(TagTypes.Int16, 7, 7);
        yield return new TestCaseData(TagTypes.Int32, 7, 7);
        yield return new TestCaseData(TagTypes.Int64, 7, 7L);
        yield return new TestCaseData(TagTypes.Uint8, 7, 7u);
        yield return new TestCaseData(TagTypes.Uint16, 7, 7u);
        yield return new TestCaseData(TagTypes.Uint32, 7, 7u);
        yield return new TestCaseData(TagTypes.Uint64, 7, 7ul);
        yield return new TestCaseData(TagTypes.Digital, 3, 3u);
        yield return new TestCaseData(TagTypes.Boolean, true, true);
        yield return new TestCaseData(TagTypes.String, "text", "text");
    }

    [TestCaseSource(nameof(ValueCases))]
    public void SetValue_ThenGetValue_RoundTripsTheValue(TagTypes tagType, object value, object expected)
    {
        var tag = new WriteDataTag { Id = 1 };

        tag.SetValue(tagType, value);

        Assert.That(tag.GetValue(), Is.EqualTo(expected));
        Assert.That(TagTypeMap.ConvertValue(tag.GetValue(), tagType),
            Is.TypeOf(TagTypeMap.GetClrType(tagType)));
    }

    [Test]
    public void SetValue_OfBlob_RoundTripsTheBytes()
    {
        var tag = new WriteDataTag { Id = 1 };
        var bytes = new byte[] { 1, 2, 3 };

        tag.SetValue(TagTypes.Blob, bytes);

        Assert.That(tag.GetValue(), Is.EqualTo(bytes));
    }

    [Test]
    public void SetValue_OfDateTime_RoundTripsTheTimestamp()
    {
        var tag = new TagResponse { Id = 1 };
        var time = new DateTimeOffset(2026, 8, 30, 10, 0, 0, TimeSpan.Zero);

        tag.SetValue(TagTypes.DateTime, time);

        Assert.That(tag.GetValue(), Is.EqualTo(time));
    }

    [Test]
    public void GetValue_WhenNoValueIsSet_ReturnsNull()
    {
        Assert.That(new WriteDataTag { Id = 1 }.GetValue(), Is.Null);
        Assert.That(new TagResponse { Id = 1 }.GetValue(), Is.Null);
    }
}
