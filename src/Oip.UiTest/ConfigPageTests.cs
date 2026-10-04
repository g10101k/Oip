using System.Text.RegularExpressions;
using Oip.UiTest.Pages;

namespace Oip.UiTest;

/// <summary>
/// The interface settings of the configuration page. They are kept in the browser local storage of the shared
/// session, so the fixture puts the settings it found back when it is done.
/// </summary>
[Order(9)]
internal class ConfigPageTests : BaseTest
{
    private const string FolderLabel = "#ConfigPageFolder";
    private const string WeatherLabel = "#ConfigPageWeather";

    /// <summary>
    /// A translation key shown instead of its text, e.g. <c>config.timeZone</c> or <c>app-info.title</c>.
    /// Skips e-mail domains and date patterns such as <c>dd.MM.yyyy</c>.
    /// </summary>
    private static readonly Regex TranslationKey = new(@"(?<![@\w.-])[a-z][a-zA-Z-]{2,}\.[a-z][a-zA-Z]{2,}\b");

    private readonly By _folder = SideMenu.Folder(FolderLabel);
    private readonly By _weather = SideMenu.ItemInFolder(FolderLabel, WeatherLabel);
    private string? _savedSettings;

    private ConfigPage Config => new(Driver);
    private WeatherForecastModulePage Weather => new(Driver);

    [OneTimeSetUp]
    public void SaveSettingsAndCreateModuleInstance()
    {
        SignInAs(TestSetup.Admin);
        _savedSettings = Config.ReadSavedSettings();

        // The weather module shows dates in the configured format and has a date picker in its column filter.
        if (Menu.Contains(_folder))
            Menu.Delete(_folder);
        Menu.CreateRootFolder(FolderLabel);
        Menu.CreateModuleInstance(_folder, WeatherLabel, SideMenu.WeatherForecastModule);
    }

    [OneTimeTearDown]
    public void RestoreSettingsAndDeleteModuleInstance()
    {
        SignInAs(TestSetup.Admin);
        Config.RestoreSavedSettings(_savedSettings);
        if (Menu.Contains(_folder))
            Menu.Delete(_folder);
    }

    [Test]
    public void LanguageChangesInterface()
    {
        SignInAs(TestSetup.Admin);
        var config = Config.Open();
        (string Language, string Logout)[] languages = [("English", "Logout"), ("Русский", "Выход")];
        // Start with the language that is not selected yet, so that every step really switches the language.
        if (config.Language.SelectedText == languages[0].Language)
            Array.Reverse(languages);

        foreach (var (language, logout) in languages)
        {
            config.Language.Choose(language);
            Assert.That(() => TopBar.LogoutText, Is.EqualTo(logout).After(5).Seconds.PollEvery(200).MilliSeconds,
                $"The interface is not switched to {language}");

            config.Reload();
            Assert.Multiple(() =>
            {
                Assert.That(config.Language.SelectedText, Is.EqualTo(language), "The language is not kept after a reload");
                Assert.That(() => TopBar.LogoutText, Is.EqualTo(logout).After(5).Seconds.PollEvery(200).MilliSeconds);
                Assert.That(TranslationKey.Matches(Driver.FindElement(By.TagName("body")).Text).Select(m => m.Value),
                    Is.Empty, $"Untranslated keys on the configuration page in {language}");
            });
        }
    }

    [TestCase("dd.MM.yyyy", "HH:mm", @"^\d{2}\.\d{2}\.\d{4} \d{2}:\d{2}$")]
    [TestCase("yyyy-MM-dd", "HH:mm:ss", @"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}$")]
    public void DateTimeFormatAppliesToDates(string dateFormat, string timeFormat, string expectedDate)
    {
        SignInAs(TestSetup.Admin);
        var config = Config.Open();

        config.DateFormat.Choose(dateFormat);
        config.TimeFormat.Choose(timeFormat);
        config.Reload();

        Assert.Multiple(() =>
        {
            Assert.That(config.DateFormat.SelectedText, Is.EqualTo(dateFormat), "The date format is not kept after a reload");
            Assert.That(config.TimeFormat.SelectedText, Is.EqualTo(timeFormat), "The time format is not kept after a reload");
        });
        Menu.Open(_weather);
        Assert.That(Weather.OpenContent().FirstDate, Does.Match(expectedDate));
    }

    [Test]
    public void FirstDayOfWeekAppliesToCalendars()
    {
        SignInAs(TestSetup.Admin);
        Config.Open().FirstDayOfWeek.ChooseAt(ConfigPage.MondayIndex);
        Menu.Open(_weather);
        var mondayFirst = Weather.OpenContent().GetCalendarWeekdays();

        var config = Config.Open();
        config.FirstDayOfWeek.ChooseAt(ConfigPage.SundayIndex);
        config.Reload();
        Menu.Open(_weather);
        var sundayFirst = Weather.OpenContent().GetCalendarWeekdays();

        // Weekday captions depend on the interface language, so compare the order rather than the names.
        Assert.That(sundayFirst, Is.EqualTo(mondayFirst.Skip(6).Concat(mondayFirst.Take(6))),
            "The calendar is expected to start from Sunday, also after a reload");
    }

    [Test]
    public void TimeZoneIsKeptAfterReload()
    {
        SignInAs(TestSetup.Admin);
        var config = Config.Open();
        var timeZone = config.TimeZone.SelectedText == "Asia/Tokyo" ? "America/New_York" : "Asia/Tokyo";

        config.TimeZone.Choose(timeZone, filter: true);
        config.Reload();

        Assert.That(config.TimeZone.SelectedText, Is.EqualTo(timeZone));
    }

    [Test]
    public void DarkThemeIsKeptAfterReload()
    {
        SignInAs(TestSetup.Admin);
        Config.Open();
        if (TopBar.IsDarkTheme)
            TopBar.ToggleTheme();

        TopBar.ToggleTheme();
        Assert.That(TopBar.IsDarkTheme, Is.True, "The dark theme is not applied");

        Menu.Reload();
        Assert.That(() => TopBar.IsDarkTheme, Is.True.After(5).Seconds.PollEvery(200).MilliSeconds,
            "The dark theme is not kept after a reload");
    }
}
