using Oip.Base.Services;
using Oip.Rtds.Base;
using Oip.Rtds.Base.Services;

namespace Oip.Rtds.RandomInterface.Services;

/// <summary>
/// A hosted service that periodically collects and evaluates tags using a specified interval
/// </summary>
public class TagWorkerHostedService(IServiceScopeFactory scopeFactory, ILogger<TagWorkerService> logger)
    : PeriodicBackgroundService<TagWorkerService>(scopeFactory, logger);
    
/// <summary>
/// Service that handles tag data collection, evaluation of formulas, and writing results to a buffer service.
/// </summary>
public class TagWorkerService(
    TagCacheService tagCacheService,
    FormulaManager formulaManager,
    BufferWriterService bufferWriterService) : IPeriodicalService
{
    public int Interval => 5;

    /// <summary>
    /// Collects tag data, evaluates formulas for each tag, and writes the results to a buffer.
    /// </summary>
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var tags = tagCacheService.Tags;
        if (tags.Count == 0)
            return;

        var result = tags
            .Select(tag => formulaManager.Evaluate(tag.Id, double.MaxValue, tag.DoubleValue, DateTimeOffset.Now))
            .ToArray();

        await bufferWriterService.WriteToService(result);
    }
}