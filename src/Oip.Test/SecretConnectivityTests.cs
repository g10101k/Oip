using Oip.Base.Security.Connectivity;

namespace Oip.Test;

/// <summary>
/// Tests for <see cref="SecretConnectivityFailureClassifier"/> and <see cref="DelegateSecretConnectivityProbe"/>.
/// </summary>
[TestFixture]
public class SecretConnectivityTests
{
    [Test]
    public void RejectedCredentialsAreReportedAsAuthenticationFailure()
    {
        var result = SecretConnectivityFailureClassifier.Classify("Redis", "the password",
            new InvalidOperationException("WRONGPASS invalid username-password pair"));

        Assert.That(result.Status, Is.EqualTo(SecretConnectivityStatus.AuthenticationFailed));
        Assert.That(result.Message, Does.Contain("the password"));
    }

    [Test]
    public void RejectedCredentialsAreFoundInInnerException()
    {
        var result = SecretConnectivityFailureClassifier.Classify("MinIO", "the key pair",
            new InvalidOperationException("request failed", new Exception("The AWS Access Key Id you provided " +
                                                                         "does not exist: InvalidAccessKeyId")));

        Assert.That(result.Status, Is.EqualTo(SecretConnectivityStatus.AuthenticationFailed));
    }

    [Test]
    public void UnreachableDependencyIsNotReportedAsAuthenticationFailure()
    {
        var result = SecretConnectivityFailureClassifier.Classify("Redis", "the password",
            new TimeoutException("Connection to 10.0.0.1:6379 timed out"));

        Assert.That(result.Status, Is.EqualTo(SecretConnectivityStatus.Unavailable));
    }

    [Test]
    public async Task DelegateProbeReportsSuccess()
    {
        var probe = new DelegateSecretConnectivityProbe("MinIO", "the key pair", _ => Task.CompletedTask);

        var result = await probe.ProbeAsync();

        Assert.That(result.Status, Is.EqualTo(SecretConnectivityStatus.Ok));
    }

    [Test]
    public async Task DelegateProbeClassifiesRejectedCredentials()
    {
        var probe = new DelegateSecretConnectivityProbe("MinIO", "the key pair",
            _ => throw new InvalidOperationException("SignatureDoesNotMatch"));

        var result = await probe.ProbeAsync();

        Assert.That(result.Status, Is.EqualTo(SecretConnectivityStatus.AuthenticationFailed));
        Assert.That(result.Message, Does.Contain("the key pair"));
    }
}
