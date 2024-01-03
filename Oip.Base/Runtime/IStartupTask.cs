namespace Oip.Base.Runtime;

public interface IStartupTask
{
    int Order { get; }
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}