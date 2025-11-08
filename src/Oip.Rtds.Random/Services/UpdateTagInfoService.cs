using Oip.Rtds.Base;
using Oip.Rtds.Grpc;
using Oip.Rtds.Random.Settings;

namespace Oip.Rtds.Random.Services;

/// <summary>
/// Service for updating tag information and formulas from RTDS data source
/// Handles tag cache updates and formula calculations
/// </summary>
public class UpdateTagInfoService(
    RtdsService.RtdsServiceClient client,
    ILogger<UpdateTagInfoService> logger,
    FormulaManager formulaManager,
    TagCacheService tagCacheService)
{
    /// <summary>
    /// Fetches tags from RTDS service and updates tag cache and formulas
    /// Processes all tags from the configured interface
    /// </summary>
    /// <param name="stoppingToken">Cancellation token to stop the operation</param>
    /// <returns>Task representing the asynchronous operation</returns>
    /// <exception cref="RpcException">Thrown when gRPC communication fails</exception>
    /// <exception cref="InvalidOperationException">Thrown when tag processing fails</exception>
    public async Task DoWork(CancellationToken stoppingToken)
    {
        var tags = client.GetTags(new GetTagsRequest()
        {
            InterfaceId = AppSettings.Instance.InterfaceId
        });
        tagCacheService.UpdateTags(tags.Tags);
        foreach (TagResponse tag in tags.Tags)
        {
            logger.LogInformation("Tag: {Name}", tag.Name);
            formulaManager.UpdateFormulas(tag.Id, tag.ValueCalculation, tag.TimeCalculation, tag.ErrorCalculation);
        }
    }
}
