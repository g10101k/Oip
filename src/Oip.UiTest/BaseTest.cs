using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace Oip.UiTest;

/// <summary>
/// Base class for UI tests. Provides common functionality and setup.
/// </summary>
internal class BaseTest
{
    /// <summary>
    /// The WebDriver instance used for browser automation.
    /// </summary>
    internal IWebDriver Driver;

    /// <summary>
    /// The base URL for the application under test.
    /// </summary>
    protected const string BaseUrl = "https://localhost:50000";

    /// <summary>
    /// The WebDriverWait instance used to wait for specific conditions on the web page.
    /// </summary>
    protected WebDriverWait Wait;

    /// <summary>
    /// Provides methods to perform user interactions such as mouse movements, keyboard actions, and context menu interactions.
    /// </summary>
    protected Actions Actions;

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
    /// The locator for the Keycloak error message related to the username field.
    /// </summary>
    protected By KeycloakErrorUserName => By.Id("input-error-username");

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
    protected By OipMenuContainer => By.CssSelector("div.menu-scroll-container");

    /// <summary>
    /// The module selector for creating a new menu item.
    /// </summary>
    protected By OipMenuItemCreateModule => By.Id("oip-menu-item-create-module");

    protected string RootFolderName => "#RootFolder";

    protected By OipMenuItemCreateLabel => By.Id("oip-menu-item-create-label");

    protected By OipAppTopBarLogoutButton => By.Id("oip-app-topbar-logout-button");

    /// <summary>
    /// The default timeout in seconds for WebDriverWait operations.
    /// </summary>
    internal const int StandardTimeOutInSeconds = 15;

    /// <summary>
    /// Base class for UI tests. Provides common functionality and setup.
    /// </summary>
    protected BaseTest()
    {
        var options = new ChromeOptions();
        options.AddArgument("--no-sandbox");
        options.AddArgument("--start-maximized");
        options.AddArgument("--disable-infobars");

        Driver = new ChromeDriver(options);
        Driver.Manage().Window.Maximize();
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(15);
        Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(15));
        Actions = new Actions(Driver);
    }

    /// <summary>
    /// Captures a screenshot and saves it to a file.
    /// </summary>
    /// <param name="testName">Name of the test case to include in the screenshot file name.</param>
    /// <param name="stepName">Name of the test step to include in the screenshot file name. Optional parameter.</param>
    public void TakeScreenshot(string testName, string stepName = "")
    {
        if (Driver is ITakesScreenshot screenshotDriver)
        {
            Screenshot screenshot = screenshotDriver.GetScreenshot();

            // Создаем директорию для скриншотов, если её нет
            string screenshotsDir = Path.Combine(Directory.GetCurrentDirectory(), "TestScreenshots");
            if (!Directory.Exists(screenshotsDir))
            {
                Directory.CreateDirectory(screenshotsDir);
            }

            // Формируем имя файла
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = string.IsNullOrEmpty(stepName)
                ? $"{testName}_{timestamp}.png"
                : $"{testName}_{stepName}_{timestamp}.png";

            string filePath = Path.Combine(screenshotsDir, fileName);
            screenshot.SaveAsFile(filePath);
        }
    }

    /// <summary>
    /// Pauses the execution for a specified number of milliseconds.
    /// </summary>
    /// <param name="seconds">The number of milliseconds to sleep. Defaults to 1000 (1 second).</param>
    internal void Sleep(int seconds = 1000) => Thread.Sleep(seconds);


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
    /// Performs initial setup before any tests are executed.
    /// </summary>
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        Driver.Navigate().GoToUrl($"{BaseUrl}/unauthorized");
        Sleep();
        Wait.Until(d => d.FindElement(OipSignInButton));

        Driver.FindElement(OipSignInButton).Click();

        Wait.Until(d => d.FindElement(KeycloakUsername));
        Driver.FindElement(KeycloakUsername).SendKeys("admin");
        Driver.FindElement(KeycloakPassword).SendKeys("P@ssw0rd");
        Driver.FindElement(KeycloakLoginButton).Click();
    }


    /// <summary>
    /// Performs cleanup after all tests have completed.
    /// </summary>
    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Driver.Quit();
        Driver.Dispose();
    }
}