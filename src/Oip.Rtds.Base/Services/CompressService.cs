using System.Globalization;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Oip.Rtds.Grpc;

namespace Oip.Rtds.Base.Services;

/// <summary>
/// Service for compressing and filtering data before writing to RTDS
/// </summary>
public class CompressService(TagCacheService tagCacheService, ILogger<CompressService> logger)
{
    /// <summary>
    /// Compresses and filters calculation results based on compression settings
    /// </summary>
    /// <param name="calculateResults">Collection of calculation results to process</param>
    /// <returns>WriteDataRequest containing filtered tags for writing to RTDS</returns>
    public async Task<WriteDataRequest> CompressFilterData(IEnumerable<CalculateResult> calculateResults)
    {
        var enumerable = calculateResults as CalculateResult[] ?? calculateResults.ToArray();
        var writeDataRequest = new WriteDataRequest();

        foreach (var result in enumerable)
        {
            try
            {
                if (!tagCacheService.TryGetTag(result.TagId, out var tag) || tag is null)
                    throw new InvalidOperationException($"Tag with id {result.TagId} not found");

                if (ShouldWriteValue(result, tag))
                {
                    writeDataRequest.Tags.Add(PrepareDataSend(result, tag));
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error processing tag with ID {TagId}", result.TagId);
            }
        }

        return writeDataRequest;
    }

    /// <summary>
    /// Determines whether a value should be written to RTDS based on the compression settings
    /// </summary>
    /// <param name="calculateResult">The calculated result to check</param>
    /// <param name="tag">The tag associated with the result</param>
    /// <returns><c>true</c> if the value should be written, <c>false</c> otherwise</returns>
    public static bool ShouldWriteValue(CalculateResult calculateResult, TagResponse tag)
    {
        if (!tag.Compressing || tag.ValueTime is null)
            return true;

        var lastTime = tag.ValueTime.ToDateTimeOffset();
        var deltaTime = calculateResult.Time - lastTime;

        // Skip if the time difference is too small
        if (deltaTime.TotalMilliseconds < tag.CompressionMinTime)
            return false;

        if (deltaTime.TotalMilliseconds > tag.CompressionMaxTime)
            return true;

        return HasSignificantChange(calculateResult, tag);
    }

    /// <summary>
    /// Determines whether the value has moved far enough from the last stored one to be worth writing.
    /// Numeric tags use the deadband of the calculated error, the remaining types have no meaningful
    /// distance, so any change counts.
    /// </summary>
    /// <param name="calculateResult">The calculated result to check</param>
    /// <param name="tag">The tag associated with the result</param>
    /// <returns><c>true</c> when the value changed significantly, <c>false</c> otherwise</returns>
    private static bool HasSignificantChange(CalculateResult calculateResult, TagResponse tag)
    {
        var lastValue = tag.GetValue();

        if (!TagTypeMap.IsNumeric(tag.ValueType))
        {
            var last = TagTypeMap.ConvertValue(lastValue, tag.ValueType);
            var current = TagTypeMap.ConvertValue(calculateResult.Value, tag.ValueType);
            return last is byte[] lastBytes && current is byte[] currentBytes
                ? !lastBytes.SequenceEqual(currentBytes)
                : !Equals(last, current);
        }

        var deltaValue = Math.Abs(Convert.ToDouble(lastValue, CultureInfo.InvariantCulture) -
                                  Convert.ToDouble(calculateResult.Value, CultureInfo.InvariantCulture));
        return deltaValue > calculateResult.Error;
    }

    /// <summary>
    /// Prepares data for sending to the server
    /// </summary>
    /// <param name="calculateResult">The calculated result to send</param>
    /// <param name="tagResponse">The tag associated with the result</param>
    /// <returns>A <see cref="WriteDataTag"/> containing the prepared data</returns>
    private static WriteDataTag PrepareDataSend(CalculateResult calculateResult, TagResponse tagResponse)
    {
        var writeDataTag = new WriteDataTag
        {
            Id = tagResponse.Id,
            Time = Timestamp.FromDateTimeOffset(calculateResult.Time)
        };
        writeDataTag.SetValue(tagResponse.ValueType, calculateResult.Value);
        return writeDataTag;
    }
}
