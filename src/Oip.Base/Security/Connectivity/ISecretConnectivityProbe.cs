namespace Oip.Base.Security.Connectivity;

/// <summary>
/// Checks at startup whether an external dependency accepts the credentials this service is configured with,
/// so that a mismatched secret is reported by name instead of surfacing later as an opaque connection error.
/// </summary>
public interface ISecretConnectivityProbe
{
    /// <summary>
    /// Name of the probed dependency, for example <c>Redis</c>.
    /// </summary>
    string Dependency { get; }

    /// <summary>
    /// Configuration keys an operator has to look at when the probe reports an authentication failure.
    /// </summary>
    string ConfigHint { get; }

    /// <summary>
    /// Probes the dependency.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token for the operation.</param>
    /// <returns>The outcome of the probe.</returns>
    Task<SecretConnectivityResult> ProbeAsync(CancellationToken cancellationToken = default);
}
