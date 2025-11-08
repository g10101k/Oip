using Oip.Rtds.Base;
using Oip.Rtds.Base.Services;

namespace Oip.Rtds.RandomInterface.Services;

/// <summary>
/// Service that handles tag data collection, evaluation of formulas, and writing results to a buffer service.
/// </summary>
public class TagWorkerService(
    TagCacheService tagCacheService,
    FormulaManager formulaManager,
    BufferWriterService bufferWriterService)
{
    /// <summary>
    /// Collects tag data, evaluates formulas for each tag, and writes the results to a buffer.
    /// </summary>
    public async Task CollectAndEvaluate()
    {
        var tags = tagCacheService.Tags;
        if (tags.Count == 0)
            return;

        var tasks = tags
            .Select(tag => formulaManager.Evaluate(tag.Id, double.MaxValue, tag.DoubleValue, DateTimeOffset.Now))
            .ToArray();

        var result = await Task.WhenAll(tasks);

        await bufferWriterService.WriteToService(result);
    }
}