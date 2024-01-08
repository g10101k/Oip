using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Oip.Core.HostedServices;

namespace Oip.Base.Services;

/// <summary>
/// Executed the specified worker within a scoped-lifetime scope.
/// </summary>
public class ScopedBackgroundService<TWorker> : BackgroundService where TWorker : IScopedBackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ScopedBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var worker =
            (IScopedBackgroundService)ActivatorUtilities.GetServiceOrCreateInstance<TWorker>(scope.ServiceProvider);
        await worker.ExecuteAsync(stoppingToken);
    }
}