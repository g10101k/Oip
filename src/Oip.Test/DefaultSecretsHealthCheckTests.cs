using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Oip.Base.Controllers.Api;
using Oip.Base.Security.DefaultSecrets;
using Oip.Base.Settings;

namespace Oip.Test;

/// <summary>
/// Tests for <see cref="DefaultSecretsHealthCheck"/> and the report cache of <see cref="DefaultSecretsValidator"/>.
/// </summary>
[TestFixture]
public class DefaultSecretsHealthCheckTests
{
    [Test]
    public async Task FindingsAreReportedAsDegraded()
    {
        var result = await CheckAsync(KnownDefaultSecrets.KeycloakClientSecret);

        Assert.That(result.Status, Is.EqualTo(HealthStatus.Degraded));
        Assert.That(result.Data.Keys, Does.Contain("SecurityService:ClientSecret"));
        Assert.That(result.Description, Does.Contain("SecurityService:ClientSecret"));
    }

    [Test]
    public async Task OverriddenSecretsAreReportedAsHealthy()
    {
        var result = await CheckAsync("real-client-secret");

        Assert.That(result.Status, Is.EqualTo(HealthStatus.Healthy));
    }

    [Test]
    public void ReportIsComputedOnceAndShared()
    {
        var settings = CreateSettings(KnownDefaultSecrets.KeycloakClientSecret);
        var validator = CreateValidator(settings);

        var first = validator.Report;

        // The health check, the startup task and the controller must observe the same instance.
        settings.SecurityService.ClientSecret = "changed-after-startup";

        Assert.That(validator.Report, Is.SameAs(first));
        Assert.That(validator.Validate(), Is.Not.SameAs(first));
    }

    [Test]
    public void BannerIsRaisedOnlyInProduction()
    {
        var report = CreateValidator(CreateSettings(KnownDefaultSecrets.KeycloakClientSecret)).Report;

        Assert.That(DefaultSecretsReportResponse.Create(report, isProduction: true).ShowBanner, Is.True);
        Assert.That(DefaultSecretsReportResponse.Create(report, isProduction: false).ShowBanner, Is.False);
    }

    [Test]
    public void FindingsAreReportedOutsideProductionEvenWithoutTheBanner()
    {
        var report = CreateValidator(CreateSettings(KnownDefaultSecrets.KeycloakClientSecret)).Report;

        var response = DefaultSecretsReportResponse.Create(report, isProduction: false);

        Assert.That(response.HasFindings, Is.True);
        Assert.That(response.Findings.Select(x => x.ConfigKey), Does.Contain("SecurityService:ClientSecret"));
    }

    [Test]
    public void BannerIsNotRaisedWithoutFindings()
    {
        var report = CreateValidator(CreateSettings("real-client-secret")).Report;

        Assert.That(DefaultSecretsReportResponse.Create(report, isProduction: true).ShowBanner, Is.False);
    }

    [Test]
    public void DisabledValidationReportsNothing()
    {
        var validator = CreateValidator(CreateSettings(KnownDefaultSecrets.KeycloakClientSecret),
            new DefaultSecretsOptions { ValidateDefaultSecrets = false });

        Assert.That(validator.Report.HasFindings, Is.False);
        Assert.That(validator.Report.CheckedCount, Is.Zero);
    }

    private static async Task<HealthCheckResult> CheckAsync(string clientSecret)
    {
        var check = new DefaultSecretsHealthCheck(CreateValidator(CreateSettings(clientSecret)));
        var registration = new HealthCheckRegistration(DefaultSecretsHealthCheck.Name, check,
            HealthStatus.Degraded, [DefaultSecretsHealthCheck.Tag]);

        return await check.CheckHealthAsync(new HealthCheckContext { Registration = registration });
    }

    private static DefaultSecretsValidator CreateValidator(ISettings settings, DefaultSecretsOptions options = null) =>
        new(settings, new ConfigurationBuilder().Build(),
            Options.Create(options ?? new DefaultSecretsOptions()));

    private static Oip.Settings.AppSettings CreateSettings(string clientSecret) => new()
    {
        SecurityService = new SecurityServiceSettings
        {
            ClientSecret = clientSecret,
            AdminPassword = "real-admin-password"
        },
        UserPhotoStorage = new() { SecretKey = "real-photo-secret" },
        DiscussionAttachmentStorage = new() { SecretKey = "real-attachment-secret" }
    };
}
