using Oip.UiTest.Pages;

namespace Oip.UiTest;

[Order(13)]
internal class ModuleRegistryTests : BaseTest
{
    private const string Key = "uitest-extension";
    private const string Name = "UI test extension";

    private ModuleRegistryPage Registry => new(Driver);
    private BackendApi Api => new(Driver);

    [OneTimeSetUp]
    [OneTimeTearDown]
    public void DeleteTestModule()
    {
        SignInAs(TestSetup.Admin);
        var registry = Registry.Open();
        if (registry.Contains(Name))
            registry.Delete(Name);
    }

    [Test]
    public void RegisterExtensionModule()
    {
        SignInAs(TestSetup.Admin);
        using var server = new ManifestServer(Key, Name);
        var registry = Registry.Open();

        registry.Register(server.ManifestUrl, Name);
        registry.Reload();

        Assert.That(registry.Contains(Name), Is.True, "The registered module is not kept after a reload");
        registry.Delete(Name);
    }

    [Test]
    public void RefreshShowsModulesRegisteredMeanwhile()
    {
        SignInAs(TestSetup.Admin);
        using var server = new ManifestServer(Key, Name);
        var registry = Registry.Open();

        Api.Post("api/extension-modules/register-extension-module", new { manifestUrl = server.ManifestUrl });
        Assert.That(registry.Contains(Name), Is.False, "The registry is expected to show the state it loaded");
        registry.Refresh();

        Assert.That(() => registry.Contains(Name), Is.True.After(5).Seconds.PollEvery(200).MilliSeconds);
        registry.Delete(Name);
    }

    [Test]
    public void DeleteExtensionModule()
    {
        SignInAs(TestSetup.Admin);
        using var server = new ManifestServer(Key, Name);
        var registry = Registry.Open();
        if (!registry.Contains(Name))
            registry.Register(server.ManifestUrl, Name);

        registry.Delete(Name);
        registry.Reload();

        Assert.That(registry.Contains(Name), Is.False);
    }
}
