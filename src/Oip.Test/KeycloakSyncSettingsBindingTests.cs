using Microsoft.Extensions.Configuration;

namespace Oip.Test;

/// <summary>
/// Guards the configuration section names in the shipped <c>appsettings.json</c> files against drifting apart from
/// the property names they are bound to. A section that no longer binds silently falls back to code defaults.
/// </summary>
[TestFixture]
public class KeycloakSyncSettingsBindingTests
{
    [Test]
    public void UsersAppSettingsBindKeycloakSyncSection()
    {
        var settings = Bind<Oip.Users.Base.Settings.AppSettings>("Oip.Users");

        Assert.That(settings.KeycloakSync.BatchSize, Is.EqualTo(100));
    }

    [Test]
    public void ShellAppSettingsBindKeycloakSyncSection()
    {
        var settings = Bind<Oip.Settings.AppSettings>("Oip");

        Assert.That(settings.KeycloakSync.BatchSize, Is.EqualTo(100));
    }

    [Test]
    public void ShellAppSettingsBindObjectStorageSections()
    {
        var settings = Bind<Oip.Settings.AppSettings>("Oip");

        Assert.That(settings.UserPhotoStorage.BucketName, Is.EqualTo("oip-user-photos"));
        Assert.That(settings.DiscussionAttachmentStorage.BucketName, Is.EqualTo("oip-discussion-attachments"));
    }

    [TestCase("Oip")]
    [TestCase("Oip.Users")]
    public void AppSettingsBindSecurityServiceSection(string project)
    {
        var settings = Bind<Oip.Settings.AppSettings>(project);

        Assert.That(settings.SecurityService.ClientId, Is.EqualTo("oip-backend"));
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
