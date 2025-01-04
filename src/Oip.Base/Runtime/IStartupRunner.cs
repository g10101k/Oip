namespace Oip.Base.Runtime;

/// <summary>
/// Startup Runner interface
/// </summary>
public interface IStartupRunner
{
    /// <summary>
    /// Start async
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task StartupAsync(CancellationToken cancellationToken = default);
}