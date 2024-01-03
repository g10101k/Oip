namespace Oip.Core.HostedServices;

public interface IScopedBackgroundService
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}