namespace Oip.UiTest.Pages;

/// <summary>
/// The module registry, administrators only: the modules known to the application and the extension modules
/// registered from a manifest.
/// </summary>
internal class ModuleRegistryPage(IWebDriver driver) : BasePage(driver)
{
    /// <summary>
    /// Path of the page.
    /// </summary>
    public const string Path = "/modules";

    private static readonly By ManifestUrlInput = By.Id("oip-app-modules-manifest-url");
    private static readonly By RegisterButton = By.Id("oip-app-modules-register");
    private static readonly By RefreshButton = By.CssSelector("p-toolbar p-button[icon='pi pi-refresh'] button");
    private static readonly By DeleteRowButton = By.CssSelector("p-button[icon='pi pi-trash']");
    private static readonly By Rows = By.CssSelector("p-table tbody tr td:nth-child(2)");

    /// <summary>
    /// Opens the page and waits for the registry.
    /// </summary>
    public ModuleRegistryPage Open()
    {
        Navigate(Path);
        WaitForAppInteractive();
        Wait.UntilFindElement(Rows);
        return this;
    }

    /// <summary>
    /// Whether the registry lists a module with the given name.
    /// </summary>
    /// <param name="name">Module name.</param>
    public bool Contains(string name) => ExistsNow(Row(name));

    /// <summary>
    /// Registers an extension module from its manifest and waits until the registry lists it.
    /// </summary>
    /// <param name="manifestUrl">Address of the manifest.</param>
    /// <param name="name">Module name from the manifest.</param>
    public void Register(string manifestUrl, string name)
    {
        Wait.UntilFindElement(ManifestUrlInput).SendKeys(manifestUrl);
        new Toasts(Driver).ExpectSuccess(() => Wait.UntilClick(RegisterButton));
        Wait.UntilFindElement(Row(name));
    }

    /// <summary>
    /// Reloads the registry with the refresh button, without reloading the page.
    /// </summary>
    public void Refresh()
    {
        Wait.UntilClick(RefreshButton);
        // The button is disabled while the request runs.
        Wait.Until(d => d.FindElement(RefreshButton).Enabled);
    }

    /// <summary>
    /// Deletes the module, confirming it, and waits until the registry no longer lists it.
    /// </summary>
    /// <param name="name">Module name.</param>
    public void Delete(string name)
    {
        new Toasts(Driver).ExpectSuccess(() =>
        {
            Wait.UntilFindElement(Row(name)).FindElement(DeleteRowButton).Click();
            new ConfirmDialog(Driver).Accept();
        });
        Wait.UntilDisappear(Row(name));
    }

    private static By Row(string name) => By.XPath($"//p-table//tbody/tr[td[2][normalize-space()='{name}']]");
}
