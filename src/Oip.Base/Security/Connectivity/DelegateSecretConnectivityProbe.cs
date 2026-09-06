namespace Oip.Base.Security.Connectivity;

/// <summary>
/// Probe built from a callback that performs the smallest authenticated request the dependency offers.
/// Modules that own a client - the MinIO clients, for example - register one of these instead of
/// pulling their client library into <c>Oip.Base</c>.
/// </summary>
/// <param name="dependency">Name of the probed dependency.</param>
/// <param name="configHint">Configuration keys to check when the credentials are rejected.</param>
/// <param name="probe">Callback issuing an authenticated request.</param>
public sealed class DelegateSecretConnectivityProbe(
    string dependency,
    string configHint,
    Func<CancellationToken, Task> probe) : ISecretConnectivityProbe
{
    /// <inheritdoc />
    public string Dependency { get; } = dependency;

    /// <inheritdoc />
    public string ConfigHint { get; } = configHint;

    /// <inheritdoc />
    public async Task<SecretConnectivityResult> ProbeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await probe(cancellationToken);
            return SecretConnectivityResult.Ok(Dependency);
        }
        catch (Exception exception)
        {
            return SecretConnectivityFailureClassifier.Classify(Dependency, ConfigHint, exception);
        }
    }
}
