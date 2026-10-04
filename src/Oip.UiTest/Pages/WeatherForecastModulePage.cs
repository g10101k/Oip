namespace Oip.UiTest.Pages;

/// <summary>
/// An instance of the weather forecast module: a table of forecasts and the number of days in the settings.
/// </summary>
internal class WeatherForecastModulePage(IWebDriver driver) : BasePage(driver)
{
    private static readonly By DayCountInput = By.Id("dayCount");
    private static readonly By SaveSettingsButton = By.Id("weather-forecast-module-settings-save-button");
    private static readonly By Rows = By.CssSelector("p-table tbody tr");
    private static readonly By FirstDateCell = By.CssSelector("p-table tbody tr:first-child td:first-child");

    // The empty message is a single cell spanning the table, a forecast row has a cell per column.
    private static readonly By ForecastCell = By.CssSelector("p-table tbody tr td:nth-child(4)");
    private static readonly By RefreshButton = By.CssSelector("p-toolbar p-button[icon='pi pi-refresh'] button");
    private static readonly By DateFilterButton = By.CssSelector("p-table thead th:first-child .p-datatable-column-filter-button");
    private static readonly By FilterOverlay = By.CssSelector(".p-datatable-filter-overlay");
    private static readonly By FilterDateInput = By.CssSelector(".p-datatable-filter-overlay p-datepicker input");
    private static readonly By CalendarWeekdays = By.CssSelector(".p-datepicker-panel .p-datepicker-weekday-cell");

    /// <summary>
    /// Opens the content tab and waits for the forecasts.
    /// </summary>
    public WeatherForecastModulePage OpenContent()
    {
        new TopBar(Driver).OpenModuleTab("content");

        // The demo endpoint fails one request in four on purpose and leaves the table empty, so ask again.
        // The refresh button is disabled while a request is running, extra clicks do nothing.
        Wait.Until(_ =>
        {
            if (ExistsNow(ForecastCell))
                return true;
            Driver.FindElement(RefreshButton).Click();
            return false;
        }, seconds: 3);
        return this;
    }

    /// <summary>
    /// Opens the settings tab.
    /// </summary>
    public WeatherForecastModulePage OpenSettings()
    {
        new TopBar(Driver).OpenModuleTab("settings");
        Wait.UntilFindElement(DayCountInput);
        return this;
    }

    /// <summary>
    /// Number of forecast days in the settings form.
    /// </summary>
    public string DayCount => Wait.UntilFindElement(DayCountInput).GetAttribute("value") ?? string.Empty;

    /// <summary>
    /// Changes the number of forecast days and saves the settings.
    /// </summary>
    /// <param name="dayCount">Number of days.</param>
    public void SaveDayCount(int dayCount)
    {
        var input = Wait.UntilFindElement(DayCountInput);
        input.Clear();
        input.SendKeys(dayCount.ToString());
        new Toasts(Driver).ExpectSuccess(() => Wait.UntilClick(SaveSettingsButton));
    }

    /// <summary>
    /// Number of rows in the forecast table.
    /// </summary>
    public int RowCount => Wait.UntilFindElements(Rows).Count;

    /// <summary>
    /// Date of the first forecast as the table shows it.
    /// </summary>
    public string FirstDate => Wait.UntilFindElementText(FirstDateCell).Trim();

    /// <summary>
    /// Opens the calendar of the date column filter and returns the weekday captions in the order the
    /// calendar shows them. Closes the filter before returning.
    /// </summary>
    public IReadOnlyList<string> GetCalendarWeekdays()
    {
        Wait.UntilClick(DateFilterButton);
        Wait.UntilClick(FilterDateInput);
        var weekdays = Wait.Until(d =>
        {
            var cells = d.FindElements(CalendarWeekdays);
            return cells.Count == 7 ? cells.Select(cell => cell.Text.Trim()).ToList() : null;
        });

        // Escape closes the calendar, the second one the filter overlay.
        Actions.SendKeys(Keys.Escape).Perform();
        Actions.SendKeys(Keys.Escape).Perform();
        Wait.UntilDisappear(FilterOverlay);
        return weekdays;
    }
}
