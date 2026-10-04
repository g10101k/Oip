namespace Oip.UiTest.Pages;

/// <summary>
/// The active authentication sessions, administrators only.
/// </summary>
internal class SessionsPage(IWebDriver driver) : BasePage(driver)
{
    /// <summary>
    /// Path of the page.
    /// </summary>
    public const string Path = "/sessions";

    private static readonly By Rows = By.CssSelector("p-table tbody tr td:nth-child(6)");
    private static readonly By TerminateButton = By.CssSelector("p-button[icon='pi pi-sign-out'] button");

    // The session of the current browser is marked with a tag in the user column.
    private static readonly By CurrentSessionRow = By.XPath("//p-table//tbody/tr[td[1]//p-tag]");

    /// <summary>
    /// Opens the page and waits for the sessions.
    /// </summary>
    public SessionsPage Open()
    {
        Navigate(Path);
        WaitForAppInteractive();
        Wait.UntilFindElement(Rows);
        return this;
    }

    /// <summary>
    /// Whether a session of a browser with the given user agent is listed.
    /// </summary>
    /// <param name="userAgent">User agent of the browser, or a unique part of it.</param>
    public bool Contains(string userAgent) => ExistsNow(Row(userAgent));

    /// <summary>
    /// Whether the terminate button of the current session is disabled.
    /// </summary>
    public bool IsCurrentSessionProtected =>
        !Wait.UntilFindElement(CurrentSessionRow).FindElement(TerminateButton).Enabled;

    /// <summary>
    /// Terminates the session of the browser with the given user agent, confirming it, and waits until
    /// it leaves the list.
    /// </summary>
    /// <param name="userAgent">User agent of the browser, or a unique part of it.</param>
    public void Terminate(string userAgent)
    {
        new Toasts(Driver).ExpectSuccess(() =>
        {
            Wait.UntilFindElement(Row(userAgent)).FindElement(TerminateButton).Click();
            new ConfirmDialog(Driver).Accept();
        });
        Wait.UntilDisappear(Row(userAgent));
    }

    private static By Row(string userAgent) =>
        By.XPath($"//p-table//tbody/tr[td[6][contains(normalize-space(), '{userAgent}')]]");
}
