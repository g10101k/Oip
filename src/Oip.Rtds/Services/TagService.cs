using Oip.Rtds.Data.Contexts;
using Oip.Rtds.Data.Repositories;
using Oip.Rtds.Grpc;
using TagTypes = Oip.Rtds.Grpc.TagTypes;

namespace Oip.Rts.Services;

/// <summary>
/// 
/// </summary>
/// <param name="tagRepository"></param>
/// <param name="rtdsRepository"></param>
public class TagService(TagRepository tagRepository, RtdsRepository rtdsRepository)
{
    /// <summary>
    /// Retrieves tags by interface ID.
    /// </summary>
    /// <param name="request">The request containing the interface ID.</param>
    /// <return>A response containing the list of tags.</return>
    public async Task<GetTagsResponse> GetTagsByInterfaceId(GetTagsRequest request)
    {
        var tags = tagRepository.GetTagsByInterfaceId(request.InterfaceId).Select(x => new TagResponse()
        {
            Id = x.Id,
            Name = x.Name,
            Compressing = x.Compressing,
            CompressionMaxTime = x.CompressionMaxTime ?? UInt32.MinValue,
            CompressionMinTime = x.CompressionMinTime ?? UInt32.MinValue,
            InstrumentTag = x.InstrumentTag ?? string.Empty,
            InterfaceId = x.InterfaceId!.Value,
            DigitalSet = x.DigitalSet ?? string.Empty,
            ErrorCalculation = x.ErrorCalculation ?? string.Empty,
            TimeCalculation = x.TimeCalculation ?? string.Empty,
            ValueCalculation = x.ValueCalculation ?? string.Empty,
            ValueType = (TagTypes)x.ValueType,
        });
        var response = new GetTagsResponse();
        response.Tags.AddRange(tags);
        return response;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
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