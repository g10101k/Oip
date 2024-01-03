namespace Oip.Base.Runtime;

public interface IStartupRunner
{
    Task StartupAsync(CancellationToken cancellationToken = default);
}