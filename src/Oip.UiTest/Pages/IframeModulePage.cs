namespace Oip.UiTest.Pages;

/// <summary>
/// An instance of the iframe module: shows the site from its settings in a frame.
/// </summary>
internal class IframeModulePage(IWebDriver driver) : BasePage(driver)
{
    private static readonly By UrlInput = By.CssSelector("[qa-id='iframe-module-settings-site-url-input']");
    private static readonly By SaveSettingsButton = By.CssSelector("[qa-id='iframe-module-settings-save-button']");
    private static readonly By Frame = By.CssSelector("iframe[title='Main iframe']");

    /// <summary>
    /// Opens the content tab.
    /// </summary>
    public IframeModulePage OpenContent()
    {
        new TopBar(Driver).OpenModuleTab("content");
        Wait.UntilFindElement(Frame);
        return this;
    }

    /// <summary>
    /// Opens the settings tab.
    /// </summary>
    public IframeModulePage OpenSettings()
    {
        new TopBar(Driver).OpenModuleTab("settings");
        Wait.UntilFindElement(UrlInput);
        return this;
    }

    /// <summary>
    /// Site address in the settings form.
    /// </summary>
    public string Url => Wait.UntilFindElement(UrlInput).GetAttribute("value") ?? string.Empty;

    /// <summary>
    /// Changes the site address and saves the settings.
    /// </summary>
    /// <param name="url">Address of the site.</param>
    public void SaveUrl(string url)
    {
        var input = Wait.UntilFindElement(UrlInput);
        input.Clear();
        input.SendKeys(url);
        new Toasts(Driver).ExpectSuccess(() => Wait.UntilClick(SaveSettingsButton));
    }

    /// <summary>
    /// Address the frame loads, or null if it has none.
    /// </summary>
    public string? FrameSource => Wait.UntilFindElement(Frame).GetAttribute("src");
}
