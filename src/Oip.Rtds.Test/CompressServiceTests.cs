using System.Globalization;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Moq;
using Oip.Rtds.Base;
using Oip.Rtds.Base.Services;
using Oip.Rtds.Grpc;
using Xunit;

namespace Oip.Rtds.Test;

public class CompressServiceTests
{
    private readonly Mock<TagCacheService> _tagCacheServiceMock;
    private readonly Mock<ILogger<CompressService>> _loggerMock;
    private readonly CompressService _compressService;

    public CompressServiceTests()
    {
        _tagCacheServiceMock = new Mock<TagCacheService>();
        _loggerMock = new Mock<ILogger<CompressService>>();
        _compressService = new CompressService(_tagCacheServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CompressFilterData_WhenNoCalculateResults_ReturnsEmptyRequest()
    {
        // Arrange
        var calculateResults = Enumerable.Empty<CalculateResult>();

        // Act
        var result = await _compressService.CompressFilterData(calculateResults);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Tags);
    }

    [Fact]
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
        Assert.NotNull(result);
        Assert.Empty(result.Tags);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error processing tag with ID 1")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }

    [Fact]
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
            Value = "50.0",
            ValueTime = Timestamp.FromDateTimeOffset(now.AddMinutes(-1))
        };

        var tag2 = new TagResponse
        {
            Id = 2,
            Compressing = false,
            Value = "150.0",
            ValueTime = Timestamp.FromDateTimeOffset(now.AddMinutes(-1))
        };

        SetupTagCache(1, tag1);
        SetupTagCache(2, tag2);

        // Act
        var result = await _compressService.CompressFilterData(calculateResults);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Tags.Count);
        Assert.Contains(result.Tags, t => t.Id == 1 && t.Value == "100");
        Assert.Contains(result.Tags, t => t.Id == 2 && t.Value == "200");
    }

    [Fact]
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
            Value = "99.9", // Small change
            ValueTime = Timestamp.FromDateTimeOffset(now.AddMilliseconds(-500)) // Less than min time
        };

        SetupTagCache(1, tag);

        // Act
        var result = await _compressService.CompressFilterData(new[] { calculateResult });

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Tags);
    }

    [Fact]
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
            Value = "99.9", // Small change
            ValueTime = Timestamp.FromDateTimeOffset(now.AddMilliseconds(-6000)) // More than max time
        };

        SetupTagCache(1, tag);

        // Act
        var result = await _compressService.CompressFilterData(new[] { calculateResult });

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Tags);
        Assert.Equal(1, (int)result.Tags[0].Id);
        Assert.Equal("100", result.Tags[0].Value);
    }

    [Fact]
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
            Value = "100.0",
            ValueTime = Timestamp.FromDateTimeOffset(now.AddMilliseconds(-2000)) // Within time range
        };

        SetupTagCache(1, tag);

        // Act
        var result = await _compressService.CompressFilterData(new[] { calculateResult });

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Tags);
        Assert.Equal(1, (int)result.Tags[0].Id);
        Assert.Equal("110", result.Tags[0].Value);
    }

    [Fact]
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
            Value = "100.0",
            ValueTime = Timestamp.FromDateTimeOffset(now.AddMilliseconds(-2000)) // Within time range
        };

        SetupTagCache(1, tag);

        // Act
        var result = await _compressService.CompressFilterData(new[] { calculateResult });

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Tags);
    }

    [Fact]
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
            Value = "150.0",
            ValueTime = Timestamp.FromDateTimeOffset(now.AddMinutes(-1))
        };
        SetupTagCache(2, tag2);

        // Act
        var result = await _compressService.CompressFilterData(calculateResults);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Tags); // Only second tag should be processed
        Assert.Equal(2, (int)result.Tags[0].Id);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error processing tag with ID 1")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }

    [Theory]
    [InlineData(false, 500, 1000, 99.9, 100.0, 1.0, 1, true)] 
    [InlineData(true, 500, 1000, 99.9, 100.0, 1.0, 200, false)] // Time diff < min time
    [InlineData(true, 1000, 5000, 99.9, 100.0, 1.0, 2000, false)] // Value change < error
    [InlineData(true, 1000, 5000, 90.0, 100.0, 1.0, 2000, true)] // Value change > error
    [InlineData(true, 1000, 5000, 99.9, 100.0, 0.05, 2000, true)] // Value change > error (small error)
    [InlineData(true, 1000, 5000, 99.9, 100.0, 1.0, 6000, true)] // Time diff > max time
    public void ShouldWriteValue_WithVariousScenarios_ReturnsExpectedResult(
        bool compressing,
        uint compressionMinTime,
        uint compressionMaxTime,
        double lastValue,
        double currentValue,
        double error,
        uint valueTimeOffset, // новый параметр - смещение от now в миллисекундах
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
            Value = lastValue.ToString(CultureInfo.InvariantCulture),
            ValueTime = Timestamp.FromDateTimeOffset(now.AddMilliseconds(-valueTimeOffset))
        };

        // Act
        var result = CompressService.ShouldWriteValue(calculateResult, tag);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    private void SetupTagCache(uint tagId, TagResponse tagResponse)
    {
        _tagCacheServiceMock
            .Setup(x => x.TryGetTag(tagId, out It.Ref<TagResponse?>.IsAny))
            .Callback((uint id, out TagResponse? tag) => { tag = tagResponse; })
            .Returns(true);
    }

    // Helper class to test private method
    private static class CompressServiceTestsAccessor
    {
        public static WriteDataTag PrepareDataSend(CalculateResult calculateResult, TagResponse tagResponse)
        {
            var compressService = new CompressService(Mock.Of<TagCacheService>(), Mock.Of<ILogger<CompressService>>());
            var method = typeof(CompressService).GetMethod("PrepareDataSend",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            return (WriteDataTag)method!.Invoke(compressService, new object[] { calculateResult, tagResponse })!;
        }
    }
}