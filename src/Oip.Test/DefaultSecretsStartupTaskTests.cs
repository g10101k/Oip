using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Oip.Base.Security.DefaultSecrets;
using Oip.Base.Settings;

namespace Oip.Test;

/// <summary>
/// Tests for <see cref="DefaultSecretsStartupTask"/>.
/// </summary>
[TestFixture]
public class DefaultSecretsStartupTaskTests
{
    [Test]
    public void ProductionWithFailOnDefaultSecretsRefusesToStart()
    {
        var task = CreateTask(Environments.Production, new DefaultSecretsOptions { FailOnDefaultSecrets = true },
            KnownDefaultSecrets.KeycloakClientSecret);

        var exception = Assert.ThrowsAsync<InvalidOperationException>(() => task.ExecuteAsync());

        Assert.That(exception!.Message, Does.Contain("SecurityService:ClientSecret"));
    }

    [Test]
    public void DevelopmentOnlyWarns()
    {
        var task = CreateTask(Environments.Development, new DefaultSecretsOptions { FailOnDefaultSecrets = true },
            KnownDefaultSecrets.KeycloakClientSecret);

        Assert.DoesNotThrowAsync(() => task.ExecuteAsync());
    }

    [Test]
    public void ProductionWithoutFailOnDefaultSecretsOnlyLogs()
    {
        var task = CreateTask(Environments.Production, new DefaultSecretsOptions(),
            KnownDefaultSecrets.KeycloakClientSecret);

        Assert.DoesNotThrowAsync(() => task.ExecuteAsync());
    }

    [Test]
    public void ValidationCanBeDisabled()
    {
        var task = CreateTask(Environments.Production,
            new DefaultSecretsOptions { FailOnDefaultSecrets = true, ValidateDefaultSecrets = false },
            KnownDefaultSecrets.KeycloakClientSecret);

        Assert.DoesNotThrowAsync(() => task.ExecuteAsync());
    }

    private static DefaultSecretsStartupTask CreateTask(string environmentName, DefaultSecretsOptions options,
        string clientSecret)
    {
        ISettings settings = new Oip.Settings.AppSettings
        {
            SecurityService = new SecurityServiceSettings
            {
                ClientSecret = clientSecret,
                AdminPassword = "real-admin-password"
            },
            UserPhotoStorage = new() { SecretKey = "real-photo-secret" },
            DiscussionAttachmentStorage = new() { SecretKey = "real-attachment-secret" }
        };

        var configuration = new ConfigurationBuilder().Build();
        var validator = new DefaultSecretsValidator(settings, configuration, Options.Create(options));

        var environment = new Mock<IHostEnvironment>();
        environment.SetupGet(x => x.EnvironmentName).Returns(environmentName);
        environment.SetupGet(x => x.ApplicationName).Returns("Oip.Test");

        return new DefaultSecretsStartupTask(validator, environment.Object, Options.Create(options),
            NullLogger<DefaultSecretsStartupTask>.Instance);
    }
}
