namespace Oip.UiTest;

/// <summary>
/// Reads the browser console of the shared session. Requires the browser log preference set in <see cref="TestSetup"/>.
/// </summary>
internal class BrowserConsole(IWebDriver driver)
{
    /// <summary>
    /// Drops the entries collected so far, so the next <see cref="ReadErrors"/> only reports new ones.
    /// </summary>
    public void Clear() => driver.Manage().Logs.GetLog(LogType.Browser);

    /// <summary>
    /// Returns the error entries logged since the previous read and removes them from the log.
    /// </summary>
    public IReadOnlyList<string> ReadErrors() =>
        driver.Manage().Logs.GetLog(LogType.Browser)
            .Where(entry => entry.Level == LogLevel.Severe)
            .Select(entry => entry.Message)
            .ToList();
}
