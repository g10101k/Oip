using OpenQA.Selenium.Interactions;
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

    /// <summary>
    /// Sets the date on a date picker element.
    /// </summary>
    /// <param name="datePicker">The IWebElement representing the date picker.</param>
    /// <param name="date">The date to set, as a string. The format must be compatible with the date picker's expected format.</param>
    public static void DatePickerSetDate(this IWebElement datePicker, string date)
    {
        var datePickerInput = datePicker.FindElement(By.TagName("input"));
        datePickerInput.Click();
        datePickerInput.SendKeys("2");
        datePickerInput.Clear();
        datePickerInput.SendKeys(date + Keys.Enter);
    }

    /// <summary>
    /// Scrolls the dropdown down by scrollY pixels.
    /// </summary>
    /// <param name="dropdown"></param>
    /// <param name="scrollY"></param>
    public static void ScrollDropdown(this IWebElement dropdown, int scrollY)
    {
        var driver = ((IWrapsDriver)dropdown).WrappedDriver;
        try
        {
            ((IJavaScriptExecutor)driver).ExecuteScript($"arguments[0].scrollBy(0, {scrollY});", dropdown);
        }
        catch (Exception ex)
        {
            Console.WriteLine($@"Error while scrolling: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets the height of the scrollable area.
    /// </summary>
    /// <param name="scroll"></param>
    /// <returns></returns>
    public static int GetScrollHeight(this IWebElement scroll)
    {
        var driver = ((IWrapsDriver)scroll).WrappedDriver;
        return driver.ExecuteJavaScript<int>("return arguments[0].scrollHeight", scroll);
    }

    /// <summary>
    /// Gets the index of the requested column in a table.
    /// </summary>
    /// <param name="table"></param>
    /// <param name="columnName"></param>
    /// <param name="useXPathIndex"></param>
    /// <returns></returns>
    /// <exception cref="NotFoundException"></exception>
    public static int GetColumnIndex(this IWebElement table, string columnName, bool useXPathIndex = true)
    {
        // Look in the header
        var headerCells = table.FindElements(By.XPath(".//th"));

        for (int i = 0; i < headerCells.Count; i++)
        {
            if (headerCells[i].Text.Contains(columnName, StringComparison.OrdinalIgnoreCase))
            {
                return useXPathIndex ? i + 1 : i;
            }
        }

        // If not found in th, look in the first row
        var firstRowCells = table.FindElements(By.XPath(".//tr[1]/td"));

        for (int i = 0; i < firstRowCells.Count; i++)
        {
            if (firstRowCells[i].Text.Contains(columnName, StringComparison.OrdinalIgnoreCase))
            {
                return useXPathIndex ? i + 1 : i;
            }
        }

        throw new NotFoundException($"Column '{columnName}' not found");
    }

    /// <summary>
    /// Checks whether a PrimeNG checkbox is checked, based on its CSS class.
    /// </summary>
    /// <param name="checkboxElement">The PrimeReact checkbox web element</param>
    /// <returns>true if the checkbox is checked, false otherwise</returns>
    public static bool IsChecked(this IWebElement checkboxElement)
    {
        return checkboxElement.FindElement(By.ClassName("p-checkbox")).GetAttribute("class")
            ?.Contains("p-checkbox-checked") == true;
    }

    /// <summary>
    /// Selects an item in a dropdown.
    /// </summary>
    /// <param name="select">The dropdown web element.</param>
    /// <param name="element">The text of the item to select.</param>
    /// <param name="withFilter">Whether to use the filter to find the item.</param>
    public static void Select(this IWebElement select, string element, bool withFilter = false)
    {
        var driver = ((IWrapsDriver)select).WrappedDriver;
        var wait = new Waiter(driver);

        // Wait for the chevrondownicon to appear
        bool IsDropdownLoaded()
        {
            try
            {
                var chevronIcon = driver.FindElements(By.TagName("chevrondownicon"));
                return chevronIcon.Any(e => e.Displayed);
            }
            catch
            {
                return false;
            }
        }

        wait.Until(_ => IsDropdownLoaded());

        // Expand the dropdown
        select.Click();

        if (withFilter)
        {
            var searchInput = wait.Until(_ =>
                driver.FindElement(By.XPath("//input[contains(@class, 'p-select-filter')]")));
            searchInput.Clear();
            searchInput.SendKeys(element);
        }

        // Select the item
        var objectMappingItem =
            wait.Until(d => d.FindElement(By.XPath($"//p-selectitem/li/span[text()='{element}']")));
        driver.ScrollToElement(objectMappingItem).Click();
    }

    /// <summary>
    /// Selects an item in a dropdown.
    /// </summary>
    /// <param name="select">The dropdown web element.</param>
    /// <param name="element">The text of the item to select.</param>
    /// <param name="withFilter">Whether to use the filter to find the item.</param>
    public static void SelectLike(this IWebElement select, string element, bool withFilter = false)
    {
        var driver = ((IWrapsDriver)select).WrappedDriver;
        var wait = new Waiter(driver);

        // Expand the dropdown
        select.Click();

        if (withFilter)
        {
            var searchInput = wait.Until(_ =>
                driver.FindElement(By.XPath("//input[contains(@class, 'p-select-filter')]")));
            searchInput.Clear();
            searchInput.SendKeys(element);
        }

        // Select the item
        var objectMappingItem =
            wait.Until(d => d.FindElement(By.XPath($"//p-selectitem/li/span[contains(text(), '{element}')]")));
        driver.ScrollToElement(objectMappingItem).Click();
    }

    /// <summary>
    /// Blinks an element (to verify that the correct element was found).
    /// </summary>
    /// <param name="element">The element</param>
    /// <param name="blinks">Number of blinks</param>
    /// <param name="delayMs">Delay between blinks</param>
    public static void BlinkElement(this IWebElement element, int blinks = 10, int delayMs = 200)
    {
        var driver = ((IWrapsDriver)element).WrappedDriver;
        var js = (IJavaScriptExecutor)driver;
        var originalStyle = element.GetAttribute("style");

        for (var i = 0; i < blinks; i++)
        {
            // Turn highlight on
            js.ExecuteScript("arguments[0].style.border='2px solid red'; arguments[0].style.backgroundColor='orange';",
                element);
            Thread.Sleep(delayMs);

            // Turn highlight off
            js.ExecuteScript("arguments[0].setAttribute('style', arguments[1]);", element, originalStyle);
            Thread.Sleep(delayMs);
        }
    }

    /// <summary>
    /// Clicks an element while holding a key. For example, Keys.Control
    /// </summary>
    /// <param name="element">The page element</param>
    /// <param name="key">The key</param>
    public static void ClickWithButton(this IWebElement element, string key)
    {
        var driver = ((IWrapsDriver)element).WrappedDriver;
        Actions actions = new Actions(driver);

        actions
            .KeyDown(key)
            .Click(element)
            .Build()
            .Perform();
    }

    /// <summary>
    /// Gets the parent element.
    /// </summary>
    public static IWebElement GetParent(this IWebElement element)
    {
        return element.FindElement(By.XPath(".."));
    }

    /// <summary>
    /// Selects an item in a virtual-scroll dropdown.
    /// </summary>
    /// <param name="element">The dropdown.</param>
    /// <param name="value">The text of the item to select.</param>
    /// <param name="withFilter">Whether to use the filter to find the item.</param>
    /// <param name="scrollStep"></param>
    public static void VirtualSelect(this IWebElement element, string value, bool withFilter = false,
        int scrollStep = 100)
    {
        var driver = ((IWrapsDriver)element).WrappedDriver;
        var wait = new Waiter(driver);

        // Expand the dropdown
        element.Click();

        // Enter the search text
        if (withFilter)
        {
            var searchInput = wait.UntilFindElement(By.CssSelector("input.p-select-filter"));
            searchInput.Clear();
            searchInput.SendKeys(value);
        }

        var scroll = driver.FindElement(By.CssSelector(".p-virtualscroller"));
        // Compute the number of element checks:
        // +1 to scroll all the way to the end,
        // +1 more to check the last element.
        var scrollStepCount = scroll.GetScrollHeight() / scrollStep + 2;

        wait.Until(d =>
        {
            try
            {
                d.FindElement(By.XPath($"//p-selectitem/li/span[text()='{value}']")).Click();
                return true;
            }
            catch (NotFoundException)
            {
                var virtualScroller = driver.FindElement(By.CssSelector(".p-virtualscroller"));
                virtualScroller.ScrollDropdown(scrollStep);
                return false;
            }
        }, TimeSpan.FromMilliseconds(25), scrollStepCount);
    }
}