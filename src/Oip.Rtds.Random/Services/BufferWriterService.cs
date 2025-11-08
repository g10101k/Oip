using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Oip.Rtds.Base;
using Oip.Rtds.Grpc;

namespace Oip.Rtds.Random.Services;

public class BufferWriterService(
    TagCacheService tagCacheService,
    RtdsService.RtdsServiceClient client,
    ILogger<BufferWriterService> logger)
{
    /// <summary>
    /// Writes data to the server based on compression settings.
    /// </summary>
    /// <param name="calculateResults">The results to be written to the server.</param>
    /// <exception cref="InvalidOperationException">Thrown when the tag is not found or invalid.</exception>
    public void WriteToService(IEnumerable<CalculateResult> calculateResults)
    {
        var enumerable = calculateResults as CalculateResult[] ?? calculateResults.ToArray();

        var writeDataRequest = new WriteDataRequest();

        foreach (var calculateResult in enumerable)
        {
            try
            {
                if (!tagCacheService.TryGetTag(calculateResult.TagId, out var tag) || tag is null)
                    throw new InvalidOperationException($"Tag with id {calculateResult.TagId} not found");

                if (ShouldWriteValue(calculateResult, tag))
                {
                    writeDataRequest.Tags.Add(PrepareDataSend(calculateResult, tag));
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error processing tag with ID {TagId}", calculateResult.TagId);
            }
        }

        try
        {
            client.WriteDataAsync(writeDataRequest);
        }
        catch (RpcException ex)
        {
            logger.LogError(ex, "Error while sending data to RTDS");
        }
    }

    /// <summary>
    /// Determines whether a value should be written to RTDS based on the compression settings.
    /// </summary>
    /// <param name="calculateResult">The calculated result to check.</param>
    /// <param name="tag">The tag associated with the result.</param>
    /// <returns><c>true</c> if the value should be written, <c>false</c> otherwise.</returns>
    private static bool ShouldWriteValue(CalculateResult calculateResult, TagResponse tag)
    {
        if (!tag.Compressing)
            return true;

        var lastTime = tag.ValueTime.ToDateTimeOffset();
        var deltaTime = calculateResult.Time - lastTime;
        var deltaValue = Math.Abs(Convert.ToDouble(tag.Value) - Convert.ToDouble(calculateResult.Value));

        // Skip if the time difference is too small
        if (deltaTime.TotalMilliseconds < tag.CompressionMinTime)
            return false;

        // Write if the time difference exceeds the maximum or the value change exceeds the error threshold
        return deltaTime.TotalMilliseconds > tag.CompressionMaxTime || deltaValue > calculateResult.Error;
    }

    /// <summary>
    /// Prepares data for sending to the server.
    /// </summary>
    /// <param name="calculateResult">The calculated result to send.</param>
    /// <param name="tagResponse">The tag associated with the result.</param>
    /// <returns>A <see cref="WriteDataTag"/> containing the prepared data.</returns>
    private static WriteDataTag PrepareDataSend(CalculateResult calculateResult, TagResponse tagResponse)
    {
        return new WriteDataTag
        {
            Id = tagResponse.Id,
            Value = calculateResult.Value.ToString(),
            Time = Timestamp.FromDateTimeOffset(calculateResult.Time)
        };
    }
}