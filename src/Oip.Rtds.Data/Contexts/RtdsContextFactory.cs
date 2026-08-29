using Microsoft.Extensions.Logging;

namespace Oip.Rtds.Data.Contexts;

/// <summary>
/// Creates <see cref="RtdsContext"/> instances outside of a dependency injection scope,
/// for components that own a long-lived ClickHouse connection.
/// </summary>
/// <param name="appSettings">Application settings containing connection string.</param>
/// <param name="logger">Logger passed to the created contexts.</param>
public sealed class RtdsContextFactory(IRtdsAppSettings appSettings, ILogger<RtdsContext> logger)
{
    /// <summary>
    /// Creates a new context. The caller owns the instance and must dispose it.
    /// </summary>
    /// <returns>A new <see cref="RtdsContext"/>.</returns>
    public RtdsContext Create() => new(appSettings, logger);
}
