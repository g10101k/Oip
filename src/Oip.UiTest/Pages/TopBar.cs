namespace Oip.UiTest.Pages;

/// <summary>
/// The application top bar.
/// </summary>
internal class TopBar(IWebDriver driver) : BasePage(driver)
{
    private static readonly By LogoutButton = By.Id("oip-app-topbar-logout-button");
    private static readonly By ThemeButton = By.Id("oip-app-topbar-theme-button");
    private static readonly By Avatar = By.Id("oip-app-topbar-user-avatar");
    private static readonly By ProfileButton = By.XPath("//button[.//p-avatar[@id='oip-app-topbar-user-avatar']]");
    private static readonly By AvatarImage = By.CssSelector("#oip-app-topbar-user-avatar img");
    private static readonly By DarkThemeRoot = By.CssSelector("html.app-dark");

    private readonly Toasts _toasts = new(driver);

    /// <summary>
    /// Clicks the logout button and confirms the sign-out.
    /// </summary>
    public void Logout()
    {
        Click(LogoutButton);

        // Clicking the logout button opens a confirmation dialog;
        // the actual sign-out only happens after confirming.
        new ConfirmDialog(Driver).Accept();
        TestSetup.CurrentUser = null;
    }

    /// <summary>
    /// Caption of the logout button, translated to the interface language. On wide windows the top bar
    /// shows the button as an icon only, so the caption is read from the DOM rather than from the rendered text.
    /// </summary>
    public string LogoutText => Wait.UntilFindElement(LogoutButton).GetDomProperty("textContent")?.Trim() ?? string.Empty;

    /// <summary>
    /// Whether the dark theme is applied to the page.
    /// </summary>
    public bool IsDarkTheme => ExistsNow(DarkThemeRoot);

    /// <summary>
    /// Switches between the light and the dark theme and waits until the page follows.
    /// </summary>
    public void ToggleTheme()
    {
        var wasDark = IsDarkTheme;
        Click(ThemeButton);
        Wait.Until(_ => IsDarkTheme != wasDark);
    }

    /// <summary>
    /// Source of the user photo in the avatar, or null while the avatar shows initials.
    /// </summary>
    public string? AvatarImageSource => ExistsNow(AvatarImage) ? Driver.FindElement(AvatarImage).GetAttribute("src") : null;

    /// <summary>
    /// Clicks the profile button with the user avatar.
    /// </summary>
    public void OpenProfile() => Click(ProfileButton);

    /// <summary>
    /// Text of the avatar: the user initials while there is no photo.
    /// </summary>
    public string AvatarText => Wait.UntilFindElement(Avatar).Text.Trim();

    /// <summary>
    /// Opens the module tab with the given id (<c>content</c>, <c>settings</c> or <c>security</c>).
    /// </summary>
    /// <param name="tabId">Id of the tab.</param>
    public void OpenModuleTab(string tabId)
    {
        Click(ModuleTab(tabId));
        Wait.Until(d => d.FindElement(ModuleTab(tabId)).GetAttribute("aria-selected") == "true");
    }

    /// <summary>
    /// Checks whether the module tab with the given id (<c>content</c>, <c>settings</c> or <c>security</c>)
    /// is shown in the top bar right now.
    /// </summary>
    /// <param name="tabId">Id of the tab.</param>
    public bool HasModuleTab(string tabId) => ExistsNow(ModuleTab(tabId));

    /// <summary>
    /// Waits until the module tab with the given id is shown in the top bar.
    /// </summary>
    /// <param name="tabId">Id of the tab.</param>
    public void WaitForModuleTab(string tabId) => Wait.UntilFindElement(ModuleTab(tabId));

    /// <summary>
    /// Clicks a top bar button. Toasts cover the right part of the top bar, so they are closed first;
    /// a toast that shows up right before the click is closed on the next attempt.
    /// </summary>
    /// <param name="button">The button locator.</param>
    private void Click(By button) => Wait.Until(d =>
    {
        _toasts.CloseAll();
        d.FindElement(button).Click();
    });

    // PrimeNG p-tab replaces the id from the template (oip-app-topbar-tab-...) with a generated
    // "pn_id_N_tab_<value>", so only the suffix built from the tab value is stable.
    private static By ModuleTab(string tabId) =>
        By.CssSelector($".layout-topbar-tabs [role='tab'][id$='_tab_{tabId}']");
}
