using Microsoft.Extensions.Logging;
using Oip.Rtds.Base;
using Oip.Rtds.Data.Contexts;
using Oip.Rtds.Data.Repositories;
using Oip.Rtds.Grpc;

namespace Oip.Rtds.Services;

/// <summary>
/// Service for managing tags
/// </summary>
/// <param name="tagRepository">Repository for tag operations</param>
/// <param name="rtdsRepository">Repository for RTDS operations</param>
/// <param name="logger">Logger instance for logging operations</param>
public class TagService(TagRepository tagRepository, RtdsRepository rtdsRepository, ILogger<TagService> logger)
{
    /// <summary>
    /// Retrieves tags by interface ID.
    /// </summary>
    /// <param name="request">The request containing the interface ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>A response containing the list of tags.</returns>
    public async Task<GetTagsResponse> GetTagsByInterfaceId(GetTagsRequest request,
        CancellationToken cancellationToken = default)
    {
        var tagEntities = await tagRepository.GetTagsByInterfaceIdAsync(request.InterfaceId, cancellationToken);
        var tags = tagEntities.Select(x => new TagResponse()
        {
            Id = x.Id,
            Name = x.Name ?? string.Empty,
            Compressing = x.Compressing,
            CompressionMaxTime = x.CompressionMaxTime ?? UInt32.MinValue,
            CompressionMinTime = x.CompressionMinTime ?? UInt32.MinValue,
            InstrumentTag = x.InstrumentTag ?? string.Empty,
            InterfaceId = x.InterfaceId ?? 0,
            DigitalSet = x.DigitalSet ?? string.Empty,
            ErrorCalculation = x.ErrorCalculation ?? string.Empty,
            TimeCalculation = x.TimeCalculation ?? string.Empty,
            ValueCalculation = x.ValueCalculation ?? string.Empty,
            ValueType = x.ValueType,
        });
        var response = new GetTagsResponse();
        response.Tags.AddRange(tags);
        return response;
    }


    /// <summary>
    /// Writes data to tags
    /// </summary>
    /// <param name="request">The request containing tag data to write</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Response indicating success or failure</returns>
    public async Task<WriteDataResponse> WriteData(WriteDataRequest request,
        CancellationToken cancellationToken = default)
    {
        // The storage type is the one the tag was declared with, not the one the value arrived in: a client
        // may send a whole number for a Float64 tag, and every type ends up in the table of its own tag type.
        var tagIds = request.Tags.Select(x => x.Id).Distinct().ToList();
        var valueTypes = await tagRepository.GetValueTypesAsync(tagIds, cancellationToken);

        var values = new List<InsertValueDto>(request.Tags.Count);
        foreach (var tag in request.Tags)
        {
            if (!valueTypes.TryGetValue(tag.Id, out var valueType))
            {
                logger.LogWarning("Skipping a value for unknown tag {TagId}", tag.Id);
                continue;
            }

            var value = tag.GetValue();
            if (value is null)
            {
                logger.LogWarning("Skipping a value for tag {TagId} because it carries no value", tag.Id);
                continue;
            }

            try
            {
                values.Add(new InsertValueDto(tag.Id, valueType, tag.Time.ToDateTimeOffset(),
                    TagTypeMap.ConvertValue(value, valueType), tag.Status));
            }
            catch (Exception e) when (e is InvalidCastException or FormatException or OverflowException
                                          or NotSupportedException)
            {
                logger.LogWarning(e, "Skipping a value for tag {TagId} that does not fit its type {ValueType}",
                    tag.Id, valueType);
            }
        }

        await rtdsRepository.InsertValues(values, cancellationToken);
        return new WriteDataResponse()
        {
            Success = true
        };
    }
}
