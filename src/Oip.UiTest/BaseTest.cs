using OpenQA.Selenium.Interactions;

namespace Oip.UiTest;

/// <summary>
/// Base class for UI tests. Provides common functionality and setup.
/// </summary>
internal class BaseTest
{
    /// <summary>
    /// The base URL for the application under test.
    /// </summary>
    protected const string BaseUrl = "https://localhost:50002";

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
    /// Represents the By locator for the sign-in button on the unauthorized page.
    /// </summary>
    protected By OipSignInButton => By.Id("oip-unauthorized-error-sign-in-button");

    /// <summary>
    /// The Keycloak username field locator.
    /// </summary>
    protected By KeycloakUsername => By.Id("username");

    /// <summary>
    /// The Keycloak password field locator.
    /// </summary>
    protected By KeycloakPassword => By.Id("password");

    /// <summary>
    /// The Keycloak login button element.
    /// </summary>
    protected By KeycloakLoginButton => By.Id("kc-login");

    /// <summary>
    /// The locator for the Keycloak login error alert (custom "oip" theme no longer renders
    /// the stock "input-error-username" element, only this alert banner).
    /// </summary>
    protected By KeycloakErrorUserName => By.CssSelector(".oip-alert.oip-alert-error");

    /// <summary>
    /// The locator for the input field used when creating a new menu item.
    /// </summary>
    protected By OipMenuCreateItemLabel => By.Id("oip-menu-item-create-label");

    /// <summary>
    /// The By locator for the "Save" button in the create item menu.
    /// </summary>
    protected By OipMenuItemCreateSaveButton => By.Id("oip-menu-item-create-save");

    /// <summary>
    /// The container element for the OIP menu.
    /// </summary>
    protected By OipMenuContainer => By.ClassName("layout-sidebar");

    /// <summary>
    /// The module selector for creating a new menu item.
    /// </summary>
    protected By OipMenuItemCreateModule => By.Id("oip-menu-item-create-module");

    protected string RootFolderName => "#RootFolder";

    protected By OipMenuItemCreateLabel => By.Id("oip-menu-item-create-label");

    protected By OipAppTopBarLogoutButton => By.Id("oip-app-topbar-logout-button");

    /// <summary>
    /// The accept/confirm button of any PrimeNG ConfirmDialog (e.g. the logout confirmation
    /// opened by <see cref="OipAppTopBarLogoutButton"/>, or the delete confirmation in the menu).
    /// </summary>
    protected By ConfirmDialogAcceptButton => By.CssSelector(".p-confirmdialog-accept-button");

    /// <summary>
    /// The default timeout in seconds for WebDriverWait operations.
    /// </summary>
    internal const int StandardTimeOutInSeconds = 15;

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
    /// Navigates to the specified module instance.
    /// </summary>
    /// <param name="moduleName">The name of the module to navigate to.</param>
    internal void GoToModuleInstance(string moduleName)
    {
        var scrollContainer = Wait.Until(d => d.FindElement(By.ClassName("layout-sidebar")));
        scrollContainer.FindElement(By.XPath($"//span[text()='{moduleName}']")).Click();
    }

    /// <summary>
    /// Checks if an element exists on the page.
    /// </summary>
    /// <param name="locator">The locator used to find the element.</param>
    /// <returns>True if the element exists, otherwise false.</returns>
    internal bool ExistsNow(By locator)
    {
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMicroseconds(1);
        var exists = Driver.FindElements(locator).Count != 0;
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(StandardTimeOutInSeconds);
        return exists;
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