using Oip.Rtds.Grpc;
using Oip.Rtds.Random.Settings;

namespace Oip.Rtds.Random.Services;

public class RandomInterfaceScoped(RtdsService.RtdsServiceClient client, ILogger<RandomInterfaceScoped> logger)
{
    public void DoWork()
    {
        var tags = client.GetTags(new GetTagsRequest()
        {
            InterfaceId = AppSettings.Instance.InterfaceId
        });
        
        foreach (var tag in tags.Tags)
            logger.LogInformation($"Tag: {tag.Name}");
    }
}