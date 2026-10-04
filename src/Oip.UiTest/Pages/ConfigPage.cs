namespace Oip.UiTest.Pages;

/// <summary>
/// The configuration page: profile photo, interface language, date and time formats, first day of the week
/// and time zone. The interface settings are kept in the browser local storage.
/// </summary>
internal class ConfigPage(IWebDriver driver) : BasePage(driver)
{
    /// <summary>
    /// Path of the page.
    /// </summary>
    public const string Path = "/config";

    private const string LayoutConfigKey = "layoutConfig";

    private static readonly By LanguageSelect = By.CssSelector("[qa-id='oip-app-config-language-select']");
    private static readonly By DateFormatSelect = By.CssSelector("[qa-id='oip-app-config-date-format-select']");
    private static readonly By TimeFormatSelect = By.CssSelector("[qa-id='oip-app-config-time-format-select']");
    private static readonly By FirstDayOfWeekSelect = By.CssSelector("[qa-id='oip-app-config-first-day-of-week-select']");
    private static readonly By TimeZoneSelect = By.CssSelector("[qa-id='oip-app-config-timezone-select']");

    /// <summary>
    /// Index of Monday and Sunday among the first day options, which start from Monday.
    /// </summary>
    public const int MondayIndex = 0;

    /// <inheritdoc cref="MondayIndex"/>
    public const int SundayIndex = 6;

    /// <summary>
    /// Opens the page.
    /// </summary>
    public ConfigPage Open()
    {
        Navigate(Path);
        WaitForAppInteractive();
        Wait.UntilFindElement(LanguageSelect);
        return this;
    }

    /// <summary>
    /// Language select.
    /// </summary>
    public PrimeSelect Language => new(Driver, LanguageSelect);

    /// <summary>
    /// Date format select.
    /// </summary>
    public PrimeSelect DateFormat => new(Driver, DateFormatSelect);

    /// <summary>
    /// Time format select.
    /// </summary>
    public PrimeSelect TimeFormat => new(Driver, TimeFormatSelect);

    /// <summary>
    /// First day of the week select. The options are named in the interface language,
    /// so pick them with <see cref="PrimeSelect.ChooseAt"/>.
    /// </summary>
    public PrimeSelect FirstDayOfWeek => new(Driver, FirstDayOfWeekSelect);

    /// <summary>
    /// Time zone select.
    /// </summary>
    public PrimeSelect TimeZone => new(Driver, TimeZoneSelect);

    /// <summary>
    /// Reads the interface settings saved in the browser, to restore them after a test.
    /// </summary>
    public string? ReadSavedSettings() =>
        (string?)((IJavaScriptExecutor)Driver).ExecuteScript($"return localStorage.getItem('{LayoutConfigKey}');");

    /// <summary>
    /// Puts back the interface settings read by <see cref="ReadSavedSettings"/> and reloads the page to apply them.
    /// </summary>
    /// <param name="settings">Saved settings, or null if there were none.</param>
    public void RestoreSavedSettings(string? settings)
    {
        ((IJavaScriptExecutor)Driver).ExecuteScript(
            settings is null
                ? $"localStorage.removeItem('{LayoutConfigKey}');"
                : $"localStorage.setItem('{LayoutConfigKey}', arguments[0]);",
            settings);
        Reload();
    }
}
