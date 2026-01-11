using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Moq;
using Oip.Rtds.Base;
using Oip.Rtds.Base.Services;
using Oip.Rtds.Grpc;

namespace Oip.Rtds.Test;

[TestFixture]
public class CompressServiceTests
{
    private Mock<TagCacheService> _tagCacheServiceMock = null!;
    private Mock<ILogger<CompressService>> _loggerMock = null!;
    private CompressService _compressService = null!;

    [SetUp]
    public void SetUp()
    {
        _tagCacheServiceMock = new Mock<TagCacheService>();
        _loggerMock = new Mock<ILogger<CompressService>>();
        _compressService = new CompressService(_tagCacheServiceMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task CompressFilterData_WhenNoCalculateResults_ReturnsEmptyRequest()
    {
        // Arrange
        var calculateResults = Enumerable.Empty<CalculateResult>();

        // Act
        var result = await _compressService.CompressFilterData(calculateResults);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Tags, Is.Empty);
    }

    [Test]
    public async Task CompressFilterData_WhenTagNotFound_LogsErrorAndContinues()
    {
        // Arrange
        var calculateResults = new[]
        {
            new CalculateResult { TagId = 1, Value = 100.0, Time = DateTimeOffset.UtcNow, Error = 1.0 }
        };

        _tagCacheServiceMock.Setup(x => x.TryGetTag(1, out It.Ref<TagResponse?>.IsAny))
            .Returns(false);

        // Act
        var result = await _compressService.CompressFilterData(calculateResults);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Tags, Is.Empty);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error processing tag with ID 1")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }

    [Test]
    public async Task CompressFilterData_WhenCompressionDisabled_WritesAllValues()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var calculateResults = new[]
        {
            new CalculateResult() { TagId = 1, Value = 100.0, Time = now, Error = 1.0 },
            new CalculateResult() { TagId = 2, Value = 200.0, Time = now, Error = 1.0 }
        };

        var tag1 = new TagResponse
        {
            Id = 1,
            Compressing = false,
            DoubleValue = 50.0,
            ValueTime = Timestamp.FromDateTimeOffset(now.AddMinutes(-1))
        };

        var tag2 = new TagResponse
        {
            Id = 2,
            Compressing = false,
            DoubleValue = 150.0,
            ValueTime = Timestamp.FromDateTimeOffset(now.AddMinutes(-1))
        };

        SetupTagCache(1, tag1);
        SetupTagCache(2, tag2);

        // Act
        var result = await _compressService.CompressFilterData(calculateResults);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Tags, Has.Count.EqualTo(2));
        Assert.That(result.Tags, Has.Some.Matches<WriteDataTag>(t => t.Id == 1 && Math.Abs(t.DoubleValue - 100.0d) < 0.001));
        Assert.That(result.Tags, Has.Some.Matches<WriteDataTag>(t => t.Id == 2 && Math.Abs(t.DoubleValue - 200) < 0.001));
    }

    [Test]
    public async Task CompressFilterData_WhenTimeDifferenceBelowMinTime_DoesNotWriteValue()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var calculateResult = new CalculateResult
        {
            TagId = 1,
            Value = 100.0,
            Time = now,
            Error = 1.0
        };

        var tag = new TagResponse
        {
            Id = 1,
            Compressing = true,
            CompressionMinTime = 1000, // 1 second
            CompressionMaxTime = 5000, // 5 seconds
            DoubleValue = 99.9, // Small change
            ValueTime = Timestamp.FromDateTimeOffset(now.AddMilliseconds(-500)) // Less than min time
        };

        SetupTagCache(1, tag);

        // Act
        var result = await _compressService.CompressFilterData(new[] { calculateResult });

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Tags, Is.Empty);
    }

    [Test]
    public async Task CompressFilterData_WhenTimeDifferenceExceedsMaxTime_WritesValue()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var calculateResult = new CalculateResult
        {
            TagId = 1,
            Value = 100.0,
            Time = now,
            Error = 1.0
        };

        var tag = new TagResponse
        {
            Id = 1,
            Compressing = true,
            CompressionMinTime = 1000,
            CompressionMaxTime = 5000,
            DoubleValue = 99.9, // Small change
            ValueTime = Timestamp.FromDateTimeOffset(now.AddMilliseconds(-6000)) // More than max time
        };

        SetupTagCache(1, tag);

        // Act
        var result = await _compressService.CompressFilterData(new[] { calculateResult });

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Tags, Has.Count.EqualTo(1));
        Assert.That(result.Tags[0].Id, Is.EqualTo(1));
        Assert.That(result.Tags[0].DoubleValue, Is.EqualTo(100));
    }

    [Test]
    public async Task CompressFilterData_WhenValueChangeExceedsError_WritesValue()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var calculateResult = new CalculateResult
        {
            TagId = 1,
            Value = 110.0, // Large change
            Time = now,
            Error = 5.0
        };

        var tag = new TagResponse
        {
            Id = 1,
            Compressing = true,
            CompressionMinTime = 1000,
            CompressionMaxTime = 5000,
            DoubleValue = 100.0,
            ValueTime = Timestamp.FromDateTimeOffset(now.AddMilliseconds(-2000)) // Within time range
        };

        SetupTagCache(1, tag);

        // Act
        var result = await _compressService.CompressFilterData(new[] { calculateResult });

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Tags, Has.Count.EqualTo(1));
        Assert.That(result.Tags[0].Id, Is.EqualTo(1));
        Assert.That(result.Tags[0].DoubleValue, Is.EqualTo(110));
    }

    [Test]
    public async Task CompressFilterData_WhenValueChangeBelowError_DoesNotWriteValue()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var calculateResult = new CalculateResult
        {
            TagId = 1,
            Value = 100.5, // Small change
            Time = now,
            Error = 1.0
        };

        var tag = new TagResponse
        {
            Id = 1,
            Compressing = true,
            CompressionMinTime = 1000,
            CompressionMaxTime = 5000,
            DoubleValue = 100.0,
            ValueTime = Timestamp.FromDateTimeOffset(now.AddMilliseconds(-2000)) // Within time range
        };

        SetupTagCache(1, tag);

        // Act
        var result = await _compressService.CompressFilterData(new[] { calculateResult });

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Tags, Is.Empty);
    }

    [Test]
    public async Task CompressFilterData_WhenExceptionOccurs_LogsErrorAndContinuesProcessing()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var calculateResults = new[]
        {
            new CalculateResult { TagId = 1, Value = 100.0, Time = now, Error = 1.0 },
            new CalculateResult { TagId = 2, Value = 200.0, Time = now, Error = 1.0 }
        };

        // First tag will throw exception
        _tagCacheServiceMock.Setup(x => x.TryGetTag(1, out It.Ref<TagResponse?>.IsAny))
            .Throws(new InvalidOperationException("Test exception"));

        // Second tag should be processed normally
        var tag2 = new TagResponse
        {
            Id = 2,
            Compressing = false,
            DoubleValue = 150.0,
            ValueTime = Timestamp.FromDateTimeOffset(now.AddMinutes(-1))
        };
        SetupTagCache(2, tag2);

        // Act
        var result = await _compressService.CompressFilterData(calculateResults);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Tags, Has.Count.EqualTo(1)); // Only second tag should be processed
        Assert.That(result.Tags[0].Id, Is.EqualTo(2));

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error processing tag with ID 1")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }

    [TestCase(false, 500u, 1000u, 99.9, 100.0, 1.0, 1u, true)]
    [TestCase(true, 500u, 1000u, 99.9, 100.0, 1.0, 200u, false)] // Time diff < min time
    [TestCase(true, 1000u, 5000u, 99.9, 100.0, 1.0, 2000u, false)] // Value change < error
    [TestCase(true, 1000u, 5000u, 90.0, 100.0, 1.0, 2000u, true)] // Value change > error
    [TestCase(true, 1000u, 5000u, 99.9, 100.0, 0.05, 2000u, true)] // Value change > error (small error)
    [TestCase(true, 1000u, 5000u, 99.9, 100.0, 1.0, 6000u, true)] // Time diff > max time
    public void ShouldWriteValue_WithVariousScenarios_ReturnsExpectedResult(
        bool compressing,
        uint compressionMinTime,
        uint compressionMaxTime,
        double lastValue,
        double currentValue,
        double error,
        uint valueTimeOffset, 
        bool expectedResult)
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var calculateResult = new CalculateResult
        {
            TagId = 1,
            Value = currentValue,
            Time = now,
            Error = error
        };

        var tag = new TagResponse
        {
            Id = 1,
            Compressing = compressing,
            CompressionMinTime = compressionMinTime,
            CompressionMaxTime = compressionMaxTime,
            DoubleValue = lastValue,
            ValueTime = Timestamp.FromDateTimeOffset(now.AddMilliseconds(-valueTimeOffset))
        };

        // Act
        var result = CompressService.ShouldWriteValue(calculateResult, tag);

        // Assert
        Assert.That(result, Is.EqualTo(expectedResult));
    }

    private void SetupTagCache(uint tagId, TagResponse tagResponse)
    {
        _tagCacheServiceMock
            .Setup(x => x.TryGetTag(tagId, out It.Ref<TagResponse?>.IsAny))
            .Callback((uint id, out TagResponse? tag) => { tag = tagResponse; })
            .Returns(true);
    }
}