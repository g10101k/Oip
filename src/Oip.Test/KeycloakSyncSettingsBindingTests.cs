using Microsoft.Extensions.Configuration;

namespace Oip.Test;

/// <summary>
/// Guards the configuration section names of the Keycloak synchronization settings against drifting apart from the
/// property names they are bound to.
/// </summary>
[TestFixture]
public class KeycloakSyncSettingsBindingTests
{
    [Test]
    public void UsersAppSettingsBindSharedSecret()
    {
        var settings = Bind<Oip.Users.Base.Settings.AppSettings>("Oip.Users");

        Assert.That(settings.KeycloakSync.SharedSecret, Is.Not.Empty);
    }

    [Test]
    public void ShellAppSettingsBindSharedSecret()
    {
        var settings = Bind<Oip.Settings.AppSettings>("Oip");

        Assert.That(settings.KeycloakSync.SharedSecret, Is.Not.Empty);
    }

    [TestCase("Oip")]
    [TestCase("Oip.Users")]
    public void AppSettingsBindObjectStorageSecret(string project)
    {
        var configuration = LoadAppSettings(project);

        Assert.That(configuration["UserPhotoStorage:SecretKey"], Is.Not.Empty);
    }

    private static TSettings Bind<TSettings>(string project) where TSettings : new()
    {
        var settings = new TSettings();
        LoadAppSettings(project).Bind(settings);
        return settings;
    }

    private static IConfiguration LoadAppSettings(string project)
    {
        var path = Path.Combine(FindRepositoryRoot(), "src", project, "appsettings.json");
        Assert.That(File.Exists(path), Is.True, $"'{path}' not found.");

        return new ConfigurationBuilder().AddJsonFile(path).Build();
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "src", "Oip.sln")))
            directory = directory.Parent;

        Assert.That(directory, Is.Not.Null, "Repository root not found.");
        return directory!.FullName;
    }
}
