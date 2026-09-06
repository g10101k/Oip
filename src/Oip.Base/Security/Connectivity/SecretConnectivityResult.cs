namespace Oip.Base.Security.Connectivity;

/// <summary>
/// Outcome of a single secret connectivity probe.
/// </summary>
public enum SecretConnectivityStatus
{
    /// <summary>
    /// The dependency accepted the configured credentials.
    /// </summary>
    Ok = 0,

    /// <summary>
    /// The dependency rejected the configured credentials.
    /// </summary>
    AuthenticationFailed = 1,

    /// <summary>
    /// The dependency could not be reached, so the credentials were never checked.
    /// </summary>
    Unavailable = 2,

    /// <summary>
    /// The dependency is not configured for this service.
    /// </summary>
    Skipped = 3
}

/// <summary>
/// Result of a single secret connectivity probe.
/// </summary>
/// <param name="Status">Outcome of the probe.</param>
/// <param name="Message">Operator facing description of the outcome.</param>
public sealed record SecretConnectivityResult(SecretConnectivityStatus Status, string Message)
{
    /// <summary>
    /// Creates a result for a dependency that accepted the credentials.
    /// </summary>
    /// <param name="dependency">Name of the dependency.</param>
    public static SecretConnectivityResult Ok(string dependency) =>
        new(SecretConnectivityStatus.Ok, $"{dependency} accepted the configured credentials.");

    /// <summary>
    /// Creates a result for a dependency that is not configured.
    /// </summary>
    /// <param name="dependency">Name of the dependency.</param>
    /// <param name="reason">Reason the probe was skipped.</param>
    public static SecretConnectivityResult Skipped(string dependency, string reason) =>
        new(SecretConnectivityStatus.Skipped, $"{dependency} probe skipped: {reason}");
}
