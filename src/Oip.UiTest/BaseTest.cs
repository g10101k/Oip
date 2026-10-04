using OpenQA.Selenium.Interactions;
using Oip.UiTest.Pages;

namespace Oip.UiTest;

/// <summary>
/// Base class for UI tests. Provides common functionality and setup.
/// </summary>
internal class BaseTest
{
    private static readonly By LayoutSidebar = By.ClassName("layout-sidebar");

    private KeycloakLoginPage? _loginPage;
    private TopBar? _topBar;
    private SideMenu? _menu;

    /// <summary>
    /// The WebDriver instance used for browser automation. Shared across all tests, see <see cref="TestSetup"/>.
    /// </summary>
    internal IWebDriver Driver => TestSetup.GlobalDriver;

    /// <summary>
    /// The Waiter instance (extends WebDriverWait) used to wait for specific conditions on the web page.
    /// </summary>
    protected Waiter Wait => TestSetup.GlobalWait;

    /// <summary>
    /// Provides methods to perform user interactions such as mouse movements, keyboard actions, and context menu interactions.
    /// </summary>
    protected Actions Actions => TestSetup.GlobalActions!;

    /// <summary>
    /// The Keycloak sign-in form.
    /// </summary>
    protected KeycloakLoginPage LoginPage => _loginPage ??= new KeycloakLoginPage(Driver);

    /// <summary>
    /// The application top bar.
    /// </summary>
    protected TopBar TopBar => _topBar ??= new TopBar(Driver);

    /// <summary>
    /// The sidebar menu.
    /// </summary>
    protected SideMenu Menu => _menu ??= new SideMenu(Driver);

    /// <summary>
    /// The browser console of the shared session.
    /// </summary>
    protected BrowserConsole Console => new(Driver);

    /// <summary>
    /// The default timeout in seconds for WebDriverWait operations.
    /// </summary>
    internal const int StandardTimeOutInSeconds = 15;

    /// <summary>
    /// Signs out of the shared browser session and signs back in as the given user.
    /// The session is shared by the whole assembly, so a test that switches to <see cref="TestSetup.User"/>
    /// must switch back to <see cref="TestSetup.Admin"/> when it is done.
    /// </summary>
    /// <param name="user">The user to sign in with.</param>
    protected void SignInAs(TestUser user)
    {
        // Start from the application root: a test that failed halfway may have left the browser anywhere,
        // and logging out of a page the next user cannot open would return them to it after signing in.
        Menu.Navigate("/");
        Wait.Until(_ => LoginPage.IsFormShown() || Menu.ExistsNow(LayoutSidebar), 1, 30);

        if (!LoginPage.IsFormShown())
        {
            if (TestSetup.CurrentUser == user)
                return;

            TopBar.Logout();
            LoginPage.WaitForForm();
        }

        LoginPage.SignIn(user);
        Menu.WaitForAppInteractive();
    }

    /// <summary>
    /// Cross platform Ctrl+A
    /// </summary>
    internal void CtrlA()
    {
        var capabilities = ((WebDriver)Driver).Capabilities;
        string platformName = (string)capabilities.GetCapability("platformName")!;

        string cmdCtrl = platformName.Contains("mac") ? Keys.Meta : Keys.Control;

        new Actions(Driver)
            .KeyDown(cmdCtrl)
            .SendKeys("a");
    }

    /// <summary>
    /// Checks if a checkbox is currently checked.
    /// </summary>
    /// <param name="checkboxElement">The IWebElement representing the checkbox.</param>
    /// <returns>True if the checkbox is checked; otherwise, false.</returns>
    internal bool IsCheckboxChecked(IWebElement checkboxElement)
    {
        return checkboxElement.FindElement(By.ClassName("p-checkbox")).GetAttribute("class")
            ?.Contains("p-checkbox-checked") == true;
    }

    /// <summary>
    /// Sends key presses and verifies that the entered value matches.
    /// </summary>
    /// <param name="locator">The input field locator</param>
    /// <param name="text">The text to enter</param>
    /// <param name="milliseconds">Interval between retries</param>
    /// <param name="maxAttempts">Maximum number of retries</param>
    protected void SendKeyWithCheck(By locator, string text, int milliseconds = 300,
        int maxAttempts = 15)
    {
        _ = Wait.Until(d =>
        {
            var element = d.FindElement(locator);
            element.Click();
            element.Clear();
            element.SendKeys(text);
            return element.GetAttribute("value") == text;
        }, TimeSpan.FromMilliseconds(milliseconds), maxAttempts);
    }
}