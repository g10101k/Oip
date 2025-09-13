using OpenQA.Selenium.Support.Extensions;

namespace Oip.UiTest.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IWebDriver"/> instances.
/// </summary>
public static class WebDriverExtension
{
    /// <summary>
    /// Scrolls the specified element into view.
    /// </summary>
    /// <param name="driver">The web driver instance.</param>
    /// <param name="element">The web element to scroll to.</param>
    /// <return>The original web element.</return>
    public static IWebElement ScrollToElement(this IWebDriver driver, IWebElement element)
    {
        driver.ExecuteJavaScript("arguments[0].scrollIntoView(true);", element);
        return element;
    }

    public static bool ExistsNow(this IWebElement e, By locator)
    {
        var driver = ((IWrapsDriver)e).WrappedDriver;
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMicroseconds(1);
        var exists = e.FindElements(locator).Count != 0;
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(BaseTest.StandardTimeOutInSeconds);
        return exists;
    }
}