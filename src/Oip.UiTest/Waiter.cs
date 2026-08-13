using System.Collections.ObjectModel;
using OpenQA.Selenium.Support.UI;
using Oip.UiTest.Extensions;

namespace Oip.UiTest;

/// <summary>
/// Web page actions with waiting.
/// </summary>
public class Waiter : WebDriverWait
{
    private readonly IWebDriver _driver;

    /// <summary>
    /// Default wait timeout in seconds for the Until methods.
    /// </summary>
    public const int DefaultWaitTimeout = 1;

    /// <summary>
    /// Default number of attempts for the Until methods.
    /// </summary>
    public const int DefaultMaxAttempts = 15;

    /// <summary>
    /// Maximum wait time in seconds for web element operations.
    /// </summary>
    public const int StandardTimeout = 15;

    /// <summary>
    /// .ctor
    /// </summary>
    /// <param name="driver"></param>
    public Waiter(IWebDriver driver) : base(driver, TimeSpan.FromSeconds(StandardTimeout))
    {
        _driver = driver;
    }

    /// <summary>
    /// .ctor
    /// </summary>
    /// <param name="clock"></param>
    /// <param name="driver"></param>
    /// <param name="timeout"></param>
    /// <param name="sleepInterval"></param>
    public Waiter(IClock clock, IWebDriver driver, TimeSpan timeout, TimeSpan sleepInterval)
        : base(clock, driver, timeout, sleepInterval)
    {
        _driver = driver;
    }

    /// <summary>
    /// Creates a new wait that waits for <paramref name="condition"/> for the given number of seconds in <paramref name="seconds"/>
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="seconds"></param>
    /// <param name="maxAttempts"></param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public TResult Until<TResult>(
        Func<IWebDriver, TResult?> condition,
        int seconds = DefaultWaitTimeout,
        int maxAttempts = DefaultMaxAttempts
    )
    {
        return Until(condition, TimeSpan.FromSeconds(seconds), maxAttempts);
    }

    /// <summary>
    /// Creates a new wait that waits for <paramref name="condition"/> for the given amount of time in <paramref name="timeSpan"/>
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="timeSpan"></param>
    /// <param name="maxAttempts"></param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public TResult Until<TResult>(Func<IWebDriver, TResult?> condition, TimeSpan timeSpan,
        int maxAttempts = DefaultMaxAttempts)
    {
        var exceptions = new List<Exception>();

        _driver.Manage().Timeouts().ImplicitWait = timeSpan;
        Timeout = timeSpan;

        try
        {
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    var result = base.Until(condition);
                    return result;
                }
                catch (WebDriverException ex)
                {
                    exceptions.Add(ex);

                    if (attempt < maxAttempts)
                        Thread.Sleep(timeSpan);
                }
            }
        }
        finally
        {
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(StandardTimeout);
        }

        throw new TimeoutException(
            $"Timed out after {timeSpan.Milliseconds * maxAttempts} milliseconds waiting for condition.",
            new AggregateException(exceptions)
        );
    }

    /// <summary>
    /// Waits for an action to complete.
    /// </summary>
    /// <param name="action">The action to perform</param>
    /// <param name="seconds">Wait time in seconds (defaults to DefaultWaitTimeout)</param>
    /// <param name="maxAttempts">Maximum number of attempts (defaults to DefaultMaxAttempts)</param>
    /// <remarks>Wraps the action in a function returning true, for compatibility with the main Until method</remarks>
    public void Until(Action<IWebDriver> action, int seconds = DefaultWaitTimeout, int maxAttempts = DefaultMaxAttempts)
    {
        Until(d =>
        {
            action(d);
            return true;
        }, seconds, maxAttempts);
    }

    /// <summary>
    /// Waits for an action to complete.
    /// </summary>
    /// <param name="action">The action to perform</param>
    /// <param name="timeSpan">Wait time in seconds (defaults to DefaultWaitTimeout)</param>
    /// <param name="maxAttempts">Maximum number of attempts (defaults to DefaultMaxAttempts)</param>
    /// <remarks>Wraps the action in a function returning true, for compatibility with the main Until method</remarks>
    public void Until(Action<IWebDriver> action, TimeSpan timeSpan, int maxAttempts = DefaultMaxAttempts)
    {
        Until(d =>
        {
            action(d);
            return true;
        }, timeSpan, maxAttempts);
    }

    /// <summary>
    /// Waits for an element to appear for the given selector.
    /// </summary>
    /// <param name="selector">The selector used to find the element</param>
    /// <param name="seconds">Wait time in seconds (defaults to DefaultWaitTimeout)</param>
    /// <param name="maxAttempts">Maximum number of attempts (defaults to DefaultMaxAttempts)</param>
    /// <returns>The found IWebElement</returns>
    public IWebElement UntilFindElement(By selector, int seconds = DefaultWaitTimeout,
        int maxAttempts = DefaultMaxAttempts)
    {
        return Until(d => d.FindElement(selector), seconds, maxAttempts);
    }

    /// <summary>
    /// Waits for an element to appear and immediately returns its text before it changes.
    /// </summary>
    /// <param name="selector">The selector used to find the element</param>
    /// <param name="seconds">Wait time in seconds (defaults to DefaultWaitTimeout)</param>
    /// <param name="maxAttempts">Maximum number of attempts (defaults to DefaultMaxAttempts)</param>
    /// <returns>The found element's text</returns>
    public string UntilFindElementText(By selector, int seconds = DefaultWaitTimeout,
        int maxAttempts = DefaultMaxAttempts)
    {
        return Until(d => d.FindElement(selector).Text, seconds, maxAttempts);
    }

    /// <summary>
    /// Waits until the number of elements becomes greater than the given value.
    /// </summary>
    /// <param name="selector">The selector used to find the elements</param>
    /// <param name="count">Maximum number of elements (the actual count is expected to be smaller)</param>
    /// <param name="seconds">Wait time in seconds (defaults to DefaultWaitTimeout)</param>
    /// <param name="maxAttempts">Maximum number of attempts (defaults to DefaultMaxAttempts)</param>
    public void UntilElementsLessThen(By selector, int count, int seconds = DefaultWaitTimeout,
        int maxAttempts = DefaultMaxAttempts)
    {
        Until(d => d.FindElements(selector).Count >= count, seconds, maxAttempts);
    }

    /// <summary>
    /// Waits until the element's text becomes equal to the given string.
    /// </summary>
    /// <param name="selector">The element selector</param>
    /// <param name="result">The text that should match</param>
    /// <param name="seconds">Wait time in seconds (defaults to DefaultWaitTimeout)</param>
    /// <param name="maxAttempts">Maximum number of attempts (defaults to DefaultMaxAttempts)</param>
    /// <remarks>The method waits until the element's text BECOMES equal to the given value</remarks>
    public void UntilTextNotEqual(By selector, string result, int seconds = DefaultWaitTimeout,
        int maxAttempts = DefaultMaxAttempts)
    {
        Until(d => d.FindElement(selector).Text == result, seconds, maxAttempts);
    }

    /// <summary>
    /// Waits until the element's text becomes non-empty.
    /// </summary>
    /// <param name="selector">The element selector</param>
    /// <param name="seconds">Wait time in seconds (defaults to DefaultWaitTimeout)</param>
    /// <param name="maxAttempts">Maximum number of attempts (defaults to DefaultMaxAttempts)</param>
    /// <remarks>The method waits until the element's text is NOT empty or whitespace-only</remarks>
    public void UntilTextIsEmpty(By selector, int seconds = DefaultWaitTimeout, int maxAttempts = DefaultMaxAttempts)
    {
        Until(d => !string.IsNullOrWhiteSpace(d.FindElement(selector).Text), seconds, maxAttempts);
    }

    /// <summary>
    /// Waits for several elements to appear for the given selector.
    /// </summary>
    /// <param name="selector">The selector used to find the elements</param>
    /// <param name="seconds">Wait time in seconds (defaults to DefaultWaitTimeout)</param>
    /// <param name="maxAttempts">Maximum number of attempts (defaults to DefaultMaxAttempts)</param>
    /// <returns>The collection of found elements</returns>
    public ReadOnlyCollection<IWebElement> UntilFindElements(By selector, int seconds = DefaultWaitTimeout,
        int maxAttempts = DefaultMaxAttempts)
    {
        return Until(d => d.FindElements(selector), seconds, maxAttempts);
    }

    /// <summary>
    /// Waits for an item to be selected in a dropdown.
    /// </summary>
    /// <param name="selector">The dropdown selector</param>
    /// <param name="item">The value to select</param>
    /// <param name="filter">Filtering flag (extra parameter for the Select method)</param>
    /// <param name="seconds">Wait time in seconds (defaults to DefaultWaitTimeout)</param>
    /// <param name="maxAttempts">Maximum number of attempts (defaults to DefaultMaxAttempts)</param>
    public void UntilSelect(By selector, string item, bool filter = false, int seconds = DefaultWaitTimeout,
        int maxAttempts = DefaultMaxAttempts)
    {
        Until(d => d.FindElement(selector).Select(item), seconds, maxAttempts);
    }

    /// <summary>
    /// Waits for a value to be entered into an element.
    /// </summary>
    /// <param name="selector">The dropdown selector</param>
    /// <param name="value">The value</param>
    /// <param name="seconds">Wait time in seconds (defaults to DefaultWaitTimeout)</param>
    /// <param name="maxAttempts">Maximum number of attempts (defaults to DefaultMaxAttempts)</param>
    public void UntilInput(By selector, string value, int seconds = DefaultWaitTimeout,
        int maxAttempts = DefaultMaxAttempts)
    {
        Until(d => d.FindElement(selector).SendKeys(value), seconds, maxAttempts);
    }

    /// <summary>
    /// Waits for a click on an element.
    /// </summary>
    /// <param name="selector">The clickable element selector</param>
    /// <param name="seconds">Wait time in seconds (defaults to DefaultWaitTimeout)</param>
    /// <param name="maxAttempts">Maximum number of attempts (defaults to DefaultMaxAttempts)</param>
    public void UntilClick(By selector, int seconds = DefaultWaitTimeout, int maxAttempts = DefaultMaxAttempts)
    {
        Until(d => d.FindElement(selector).Click(), seconds, maxAttempts);
    }

    /// <summary>
    /// Waits for an element to disappear from the page.
    /// </summary>
    /// <param name="selector">The selector of the element that should disappear</param>
    /// <param name="seconds">Wait time in seconds (defaults to DefaultWaitTimeout)</param>
    /// <param name="maxAttempts">Maximum number of attempts (defaults to DefaultMaxAttempts)</param>
    public void UntilDisappear(By selector, int seconds = DefaultWaitTimeout, int maxAttempts = DefaultMaxAttempts)
    {
        Until(d => d.FindElements(selector).Count == 0, seconds, maxAttempts);
    }

    /// <summary>
    /// Waits for a date to be set in a date picker element.
    /// </summary>
    /// <param name="selector">The date picker element selector</param>
    /// <param name="date">The date to set, as a string</param>
    /// <param name="seconds">Wait time in seconds (defaults to DefaultWaitTimeout)</param>
    /// <param name="maxAttempts">Maximum number of attempts (defaults to DefaultMaxAttempts)</param>
    public void UntilSetDate(By selector, string date, int seconds = DefaultWaitTimeout,
        int maxAttempts = DefaultMaxAttempts)
    {
        Until(d => d.FindElement(selector).DatePickerSetDate(date), seconds, maxAttempts);
    }

    /// <summary>
    /// Waits for a child element to appear inside a parent element.
    /// </summary>
    /// <param name="element">The parent element</param>
    /// <param name="selector">The selector used to find the child element</param>
    /// <param name="seconds">Wait time in seconds (defaults to DefaultWaitTimeout)</param>
    /// <param name="maxAttempts">Maximum number of attempts (defaults to DefaultMaxAttempts)</param>
    /// <returns>The found child element</returns>
    public IWebElement UntilFindOneChildren(IWebElement element, By selector, int seconds = DefaultWaitTimeout,
        int maxAttempts = DefaultMaxAttempts)
    {
        return Until(_ => element.FindElement(selector), seconds, maxAttempts);
    }

    /// <summary>
    /// Waits for several child elements to appear inside a parent element.
    /// </summary>
    /// <param name="element">The parent element</param>
    /// <param name="selector">The selector used to find the child elements</param>
    /// <param name="seconds">Wait time in seconds (defaults to DefaultWaitTimeout)</param>
    /// <param name="maxAttempts">Maximum number of attempts (defaults to DefaultMaxAttemps)</param>
    /// <returns>The collection of found child elements</returns>
    public ReadOnlyCollection<IWebElement> UntilFindChildren(IWebElement element, By selector,
        int seconds = DefaultWaitTimeout,
        int maxAttempts = DefaultMaxAttempts)
    {
        return Until(_ => element.FindElements(selector), seconds, maxAttempts);
    }

    /// <summary>
    /// Finds and returns a clickable web element.
    /// </summary>
    /// <param name="locator">The locator of the element to find.</param>
    /// <param name="seconds">Maximum wait time in seconds. Defaults to <see cref="DefaultWaitTimeout"/>.</param>
    /// <param name="maxAttempts">Maximum number of attempts to find the element. Defaults to 1.</param>
    /// <returns>The clickable web element.</returns>
    /// <exception cref="WebDriverException">Thrown if the element is not found or not clickable within the given time.</exception>
    /// <remarks>The method waits until the element becomes visible and enabled before returning it. If the element does not become clickable within the given time, an exception is thrown.</remarks>
    public IWebElement Clickable(By locator, int seconds = StandardTimeout, int maxAttempts = 1)
    {
        return Until(d =>
        {
            var element = d.FindElement(locator);
            if (element is { Displayed: true, Enabled: true })
            {
                return element;
            }

            throw new WebDriverException("element is not clickable");
        }, seconds, maxAttempts);
    }
}