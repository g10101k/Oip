using Oip.Rtds.Data.Contexts;
using Oip.Rtds.Data.Repositories;
using Oip.Rtds.Grpc;
using TagTypes = Oip.Rtds.Grpc.TagTypes;

namespace Oip.Rtds.Services;

/// <summary>
/// Service for managing tags
/// </summary>
/// <param name="tagRepository">Repository for tag operations</param>
/// <param name="rtdsRepository">Repository for RTDS operations</param>
public class TagService(TagRepository tagRepository, RtdsRepository rtdsRepository)
{
    /// <summary>
    /// Retrieves tags by interface ID.
    /// </summary>
    /// <param name="request">The request containing the interface ID.</param>
    /// <returns>A response containing the list of tags.</returns>
    public async Task<GetTagsResponse> GetTagsByInterfaceId(GetTagsRequest request)
    {
        var tags = tagRepository.GetTagsByInterfaceId(request.InterfaceId).Select(x => new TagResponse()
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
        return await Task.FromResult(response);
    }


    /// <summary>
    /// Writes data to tags
    /// </summary>
    /// <param name="request">The request containing tag data to write</param>
    /// <returns>Response indicating success or failure</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an invalid value type is provided</exception>
    public async Task<WriteDataResponse> WriteData(WriteDataRequest request)
    {
        var t = request.Tags.Where(x => x.ValueCase is WriteDataTag.ValueOneofCase.DoubleValue)
            .Select(x => new InsertValueDto<double>(x.Id, TagTypes.Float32, x.Time.ToDateTimeOffset(),
                x.DoubleValue, TagValueStatus.Good)).ToList();

        await rtdsRepository.InsertValues(t);
        return new WriteDataResponse()
        {
            Success = true
        };
    }
}