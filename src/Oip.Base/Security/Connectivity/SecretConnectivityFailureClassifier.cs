namespace Oip.Base.Security.Connectivity;

/// <summary>
/// Tells a rejected credential apart from an unreachable dependency, so that a probe can point at the
/// configuration key instead of reporting a generic connection failure.
/// </summary>
public static class SecretConnectivityFailureClassifier
{
    private static readonly string[] AuthenticationMarkers =
    [
        "NOAUTH",
        "WRONGPASS",
        "invalid password",
        "invalid username-password pair",
        "AuthenticationFailure",
        "InvalidAccessKeyId",
        "SignatureDoesNotMatch",
        "AccessDenied",
        "Access Denied",
        "Unauthorized",
        "does not match"
    ];

    /// <summary>
    /// Returns whether the exception, or any of its inner exceptions, reports rejected credentials.
    /// </summary>
    /// <param name="exception">The exception raised by the probe.</param>
    /// <returns><c>true</c> when the dependency rejected the credentials.</returns>
    public static bool IsAuthenticationFailure(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            var message = current.Message;
            if (AuthenticationMarkers.Any(marker => message.Contains(marker, StringComparison.OrdinalIgnoreCase)))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Builds the result for a failed probe.
    /// </summary>
    /// <param name="dependency">Name of the probed dependency.</param>
    /// <param name="configHint">Configuration keys to check.</param>
    /// <param name="exception">The exception raised by the probe.</param>
    /// <returns>The classified result.</returns>
    public static SecretConnectivityResult Classify(string dependency, string configHint, Exception exception)
    {
        return IsAuthenticationFailure(exception)
            ? new SecretConnectivityResult(SecretConnectivityStatus.AuthenticationFailed,
                $"{dependency} rejected the configured credentials. Check {configHint} against the credentials " +
                $"{dependency} was started with. ({exception.Message})")
            : new SecretConnectivityResult(SecretConnectivityStatus.Unavailable,
                $"{dependency} could not be reached, the credentials were not checked. ({exception.Message})");
    }
}
