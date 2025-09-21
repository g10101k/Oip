using Grpc.Core;
using Oip.Rtds.Data.Dtos;
using Oip.Rtds.Data.Repositories;
using Oip.Rtds.Grpc;
using Oip.Rts.Mappers;

namespace Oip.Rts.Services;

public class TagService
{
    private readonly TagRepository _tagRepository;

    public TagService(TagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public async Task<GetTagsResponse> GetTagsByInterfaceId(GetTagsRequest request)
    {
        var tags = _tagRepository.GetTagsByInterfaceId(request.InterfaceId).Select(TagMapper.ToTagResponse);
        var response = new GetTagsResponse();
        response.Tags.AddRange(tags);
        return response;
    }
}