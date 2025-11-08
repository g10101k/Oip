using Oip.Rtds.Data.Repositories;
using Oip.Rtds.Grpc;

namespace Oip.Rts.Services;

public class TagService
{
    private readonly TagRepository _tagRepository;

    public TagService(TagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    /// <summary>
    /// Retrieves tags by interface ID.
    /// </summary>
    /// <param name="request">The request containing the interface ID.</param>
    /// <return>A response containing the list of tags.</return>
    public async Task<GetTagsResponse> GetTagsByInterfaceId(GetTagsRequest request)
    {
        var tags = _tagRepository.GetTagsByInterfaceId(request.InterfaceId).Select(x => new TagResponse()
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
}