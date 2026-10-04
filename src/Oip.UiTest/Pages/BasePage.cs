using OpenQA.Selenium.Interactions;

namespace Oip.UiTest.Pages;

/// <summary>
/// Base class for page objects. Gives access to the driver and the common waiting helpers.
/// </summary>
internal abstract class BasePage
{
    /// <summary>
    /// The application wrapper while it is blocked by BlockLoaderComponent: a full screen overlay is up
    /// and the wrapper is marked <c>inert</c>, so clicks and context menus never reach the menu below it.
    /// </summary>
    private static readonly By BlockedLayoutWrapper = By.CssSelector(".layout-wrapper[inert]");

    /// <summary>
    /// How long the blocker must stay away before the application counts as interactive.
    /// </summary>
    private const int InteractiveSettleMilliseconds = 500;

    /// <summary>
    /// Creates a page object bound to the given driver.
    /// </summary>
    /// <param name="driver">The web driver the page works with.</param>
    protected BasePage(IWebDriver driver)
    {
        Driver = driver;
        Wait = new Waiter(driver);
        Actions = new Actions(driver);
    }

    /// <summary>
    /// The web driver the page works with.
    /// </summary>
    protected IWebDriver Driver { get; }

    /// <summary>
    /// Waiter bound to <see cref="Driver"/>.
    /// </summary>
    protected Waiter Wait { get; }

    /// <summary>
    /// Mouse and keyboard actions bound to <see cref="Driver"/>.
    /// </summary>
    protected Actions Actions { get; }

    /// <summary>
    /// Path of the current page, without the query string.
    /// </summary>
    public string CurrentPath => new Uri(Driver.Url).AbsolutePath;

    /// <summary>
    /// Loads the given path of the application in the browser, as if the user typed the address.
    /// </summary>
    /// <param name="path">Path relative to the application root, starting with a slash.</param>
    public void Navigate(string path) => Driver.Navigate().GoToUrl($"{TestSetup.BaseUrl}{path}");

    /// <summary>
    /// Reloads the page, as the browser refresh button does, and waits until the application is interactive.
    /// </summary>
    public void Reload()
    {
        Driver.Navigate().Refresh();
        WaitForAppInteractive();
    }

    /// <summary>
    /// Waits until the browser shows the given path. Redirects of the guards happen after the page has
    /// loaded, so the address has to be polled.
    /// </summary>
    /// <param name="path">Expected path, without the query string.</param>
    public void WaitForPath(string path)
    {
        try
        {
            Wait.Until(_ => CurrentPath == path, 1, 30);
        }
        catch (TimeoutException e)
        {
            throw new AssertionException($"Expected the browser to show '{path}', but it shows '{CurrentPath}'.", e);
        }
    }

    /// <summary>
    /// Waits until a guard redirects the browser away from the given path and returns the new path.
    /// </summary>
    /// <param name="path">Path the browser was sent to.</param>
    public string WaitForRedirectFrom(string path)
    {
        Wait.Until(_ => CurrentPath != path, 1, 30);
        return CurrentPath;
    }

    /// <summary>
    /// Waits until the page has received the response to a request whose address contains the given text.
    /// Resource timing entries appear only once the response is complete.
    /// </summary>
    /// <param name="urlPart">Part of the request address.</param>
    public void WaitForResponse(string urlPart) =>
        Wait.Until(d => (bool)((IJavaScriptExecutor)d).ExecuteScript(
            "return performance.getEntriesByType('resource').some(e => e.name.includes(arguments[0]));", urlPart));

    /// <summary>
    /// Checks whether an element exists on the page right now, without waiting for it.
    /// </summary>
    /// <param name="locator">The locator used to find the element.</param>
    /// <returns>True if the element exists, otherwise false.</returns>
    public bool ExistsNow(By locator) => FindAllNow(Driver, locator).Count != 0;

    /// <summary>
    /// Finds the elements inside the given element or page right now. Without it an empty result takes the whole
    /// implicit wait, so checking that something is absent would take seconds.
    /// </summary>
    /// <param name="context">The element or page to search in.</param>
    /// <param name="locator">The locator used to find the elements.</param>
    protected IReadOnlyList<IWebElement> FindAllNow(ISearchContext context, By locator)
    {
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMicroseconds(1);
        try
        {
            return context.FindElements(locator);
        }
        finally
        {
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(BaseTest.StandardTimeOutInSeconds);
        }
    }

    /// <summary>
    /// Waits until BlockLoaderComponent has released the application.
    /// The blocker outlives the router navigation - it also covers module rights, settings and
    /// extension loading registered in ModuleLoadingService - and it appears 150 ms after the
    /// transition starts, so a single check right after a click can pass before it even shows up.
    /// </summary>
    public void WaitForAppInteractive()
    {
        Wait.Until(_ =>
        {
            for (var elapsed = 0; elapsed < InteractiveSettleMilliseconds; elapsed += 100)
            {
                if (ExistsNow(BlockedLayoutWrapper))
                    return false;
                Thread.Sleep(100);
            }

            return true;
        });
    }
}
