using Oip.Base.Services;

namespace Oip.Rtds.RandomInterface.Services;

/// <summary>
/// A hosted service that periodically collects and evaluates tags using a specified interval
/// </summary>
public class TagWorkerHostedService(IServiceScopeFactory scopeFactory, ILogger<TagWorkerService> logger)
    : PeriodicBackgroundService<TagWorkerService>(scopeFactory, logger);