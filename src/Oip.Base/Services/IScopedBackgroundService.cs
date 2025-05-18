namespace Oip.Base.Services;

public interface IScopedBackgroundService
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}