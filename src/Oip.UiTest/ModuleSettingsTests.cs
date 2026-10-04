using Oip.UiTest.Pages;

namespace Oip.UiTest;

/// <summary>
/// Module instance settings are loaded and saved by BaseModuleComponent, which every module is built on.
/// </summary>
[Order(8)]
internal class ModuleSettingsTests : BaseTest
{
    private const string FolderLabel = "#ModuleSettingsFolder";
    private const string WeatherLabel = "#ModuleSettingsWeather";
    private const string IframeLabel = "#ModuleSettingsIframe";
    private const string IframeModule = "IframeModule";

    /// <summary>
    /// Differs from the default of five days of WeatherModuleSettings.
    /// </summary>
    private const int DayCount = 3;

    private readonly By _folder = SideMenu.Folder(FolderLabel);

    private static By InFolder(string menuLabel) => SideMenu.ItemInFolder(FolderLabel, menuLabel);

    [OneTimeSetUp]
    public void CreateModuleInstances()
    {
        SignInAs(TestSetup.Admin);
        if (Menu.Contains(_folder))
            Menu.Delete(_folder);

        Menu.CreateRootFolder(FolderLabel);
        Menu.CreateModuleInstance(_folder, WeatherLabel, SideMenu.WeatherForecastModule);
        Menu.CreateModuleInstance(_folder, IframeLabel, IframeModule);
    }

    [OneTimeTearDown]
    public void DeleteModuleInstances()
    {
        SignInAs(TestSetup.Admin);
        if (Menu.Contains(_folder))
            Menu.Delete(_folder);
    }

    [Test]
    public void WeatherSettingsAreKeptAfterReload()
    {
        SignInAs(TestSetup.Admin);
        Menu.Open(InFolder(WeatherLabel));
        var weather = new WeatherForecastModulePage(Driver);

        weather.OpenSettings().SaveDayCount(DayCount);
        Menu.Reload();

        Assert.Multiple(() =>
        {
            Assert.That(weather.OpenSettings().DayCount, Is.EqualTo(DayCount.ToString()));
            Assert.That(weather.OpenContent().RowCount, Is.EqualTo(DayCount),
                "The forecast is expected for the number of days from the settings");
        });
    }

    [Test]
    public void IframeSettingsAreKeptAfterReload()
    {
        // A same-origin static file: the frame loads without network access and without a second copy of the app.
        var url = $"{TestSetup.BaseUrl}/assets/favicon.svg";
        SignInAs(TestSetup.Admin);
        Menu.Open(InFolder(IframeLabel));
        var iframe = new IframeModulePage(Driver);

        iframe.OpenSettings().SaveUrl(url);
        Menu.Reload();

        Assert.Multiple(() =>
        {
            Assert.That(iframe.OpenSettings().Url, Is.EqualTo(url));
            Assert.That(iframe.OpenContent().FrameSource, Is.EqualTo(url));
        });
    }
}
