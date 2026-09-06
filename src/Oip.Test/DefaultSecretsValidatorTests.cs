using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Oip.Base.Security.DefaultSecrets;
using Oip.Base.Settings;

namespace Oip.Test;

/// <summary>
/// Tests for <see cref="DefaultSecretsValidator"/> and <see cref="SecretSettingsScanner"/>.
/// </summary>
[TestFixture]
public class DefaultSecretsValidatorTests
{
    [Test]
    public void ScannerFindsSecretsThroughNestedSettings()
    {
        var settings = CreateSettings();

        var descriptors = SecretSettingsScanner.Scan(settings);

        Assert.That(descriptors.Select(x => x.ConfigKey), Does.Contain("SecurityService:ClientSecret"));
        Assert.That(descriptors.Select(x => x.ConfigKey), Does.Contain("UserPhotoStorage:SecretKey"));
    }

    [Test]
    public void ShippedDefaultIsReported()
    {
        var settings = CreateSettings();
        settings.SecurityService.ClientSecret = KnownDefaultSecrets.KeycloakClientSecret;

        var report = Validate(settings);

        var finding = report.Findings.SingleOrDefault(x => x.ConfigKey == "SecurityService:ClientSecret");
        Assert.That(finding, Is.Not.Null);
        Assert.That(finding!.Kind, Is.EqualTo(DefaultSecretFindingKind.ShippedDefault));
    }

    [Test]
    public void EmptyRequiredSecretIsReportedAsMissing()
    {
        var settings = CreateSettings();
        settings.SecurityService.ClientSecret = string.Empty;

        var report = Validate(settings);

        var finding = report.Findings.SingleOrDefault(x => x.ConfigKey == "SecurityService:ClientSecret");
        Assert.That(finding, Is.Not.Null);
        Assert.That(finding!.Kind, Is.EqualTo(DefaultSecretFindingKind.Missing));
    }

    [Test]
    public void EmptyOptionalSecretIsNotReported()
    {
        var settings = CreateSettings();
        settings.SecurityService.AdminPassword = string.Empty;

        var report = Validate(settings);

        Assert.That(report.Findings.Any(x => x.ConfigKey == "SecurityService:AdminPassword"), Is.False);
    }

    [Test]
    public void OverriddenSecretIsNotReported()
    {
        var settings = CreateSettings();

        var report = Validate(settings);

        Assert.That(report.HasFindings, Is.False, string.Join(", ", report.Findings.Select(x => x.ConfigKey)));
        Assert.That(report.CheckedCount, Is.GreaterThan(0));
    }

    [Test]
    public void IgnoredKeysAreSkipped()
    {
        var settings = CreateSettings();
        settings.SecurityService.ClientSecret = KnownDefaultSecrets.KeycloakClientSecret;

        var report = Validate(settings,
            new DefaultSecretsOptions { IgnoredSecretKeys = ["SecurityService:ClientSecret"] });

        Assert.That(report.HasFindings, Is.False);
    }

    [Test]
    public void DefaultHintUsesEnvironmentVariableSeparator()
    {
        var finding = new DefaultSecretFinding("SecurityService:ClientSecret",
            DefaultSecretFindingKind.ShippedDefault, null);

        Assert.That(finding.Describe("Oip.Test"), Does.Contain("SecurityService__ClientSecret"));
    }

    [Test]
    public void RawConfigurationKeyIsReported()
    {
        var settings = CreateSettings();
        var configuration = CreateConfiguration(
            new Dictionary<string, string> { ["SmtpSettings:SmtpPassword"] = KnownDefaultSecrets.SmtpPassword });

        var report = new DefaultSecretsValidator(settings, configuration,
            Options.Create(new DefaultSecretsOptions())).Validate();

        Assert.That(report.Findings.Select(x => x.ConfigKey), Does.Contain("SmtpSettings:SmtpPassword"));
    }

    private static DefaultSecretsReport Validate(ISettings settings, DefaultSecretsOptions options = null) =>
        new DefaultSecretsValidator(settings, CreateConfiguration(new Dictionary<string, string>()),
                Options.Create(options ?? new DefaultSecretsOptions()))
            .Validate();

    private static IConfiguration CreateConfiguration(IDictionary<string, string> values) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

    private static Oip.Settings.AppSettings CreateSettings() => new()
    {
        SecurityService = new SecurityServiceSettings
        {
            ClientSecret = "real-client-secret",
            AdminPassword = "real-admin-password"
        },
        UserPhotoStorage = new() { SecretKey = "real-photo-secret" },
        DiscussionAttachmentStorage = new() { SecretKey = "real-attachment-secret" }
    };
}
