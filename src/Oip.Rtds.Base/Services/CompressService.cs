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
        var deltaValue = Math.Abs(Convert.ToDouble(tag.DoubleValue, CultureInfo.InvariantCulture) -
                                  Convert.ToDouble(calculateResult.Value));

        // Skip if the time difference is too small
        if (deltaTime.TotalMilliseconds < tag.CompressionMinTime)
            return false;

        // Write if the time difference exceeds the maximum or the value change exceeds the error threshold
        return deltaTime.TotalMilliseconds > tag.CompressionMaxTime || deltaValue > calculateResult.Error;
    }

    /// <summary>
    /// Prepares data for sending to the server
    /// </summary>
    /// <param name="calculateResult">The calculated result to send</param>
    /// <param name="tagResponse">The tag associated with the result</param>
    /// <returns>A <see cref="WriteDataTag"/> containing the prepared data</returns>
    private static WriteDataTag PrepareDataSend(CalculateResult calculateResult, TagResponse tagResponse)
    {
        return new WriteDataTag
        {
            Id = tagResponse.Id,
            DoubleValue = (double)calculateResult.Value,
            Time = Timestamp.FromDateTimeOffset(calculateResult.Time)
        };
    }
}