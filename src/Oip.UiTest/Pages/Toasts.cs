namespace Oip.UiTest.Pages;

/// <summary>
/// The toast messages of MsgService. They live for two seconds only, so a toast left over from a previous
/// action can still be on screen; the messages already shown are marked and not taken for the new one.
/// </summary>
internal class Toasts(IWebDriver driver) : BasePage(driver)
{
    private const string SeenAttribute = "data-uitest-seen";

    private static readonly By Message = By.CssSelector(".p-toast-message");
    private static readonly By NewSuccess = By.CssSelector($".p-toast-message-success:not([{SeenAttribute}])");
    private static readonly By NewError = By.CssSelector($".p-toast-message-error:not([{SeenAttribute}])");
    private static readonly By Summary = By.CssSelector(".p-toast-summary");
    private static readonly By Detail = By.CssSelector(".p-toast-detail");

    /// <summary>
    /// Runs the action and waits for the success message it reports.
    /// </summary>
    /// <param name="action">Action that ends with a success message, e.g. a click on a save button.</param>
    public void ExpectSuccess(Action action)
    {
        MarkShown();
        action();
        try
        {
            Wait.UntilFindElement(NewSuccess);
        }
        catch (TimeoutException e)
        {
            // Report what the application said instead, usually an error from the backend. Messages live for
            // two seconds only, so they are collected as they appear.
            var shown = (IReadOnlyCollection<object>)((IJavaScriptExecutor)Driver).ExecuteScript("return window.__uitestToasts ?? [];")!;
            throw new AssertionException(
                $"No success message appeared. Messages shown: {(shown.Count == 0 ? "none" : string.Join(" | ", shown))}", e);
        }
    }

    /// <summary>
    /// Waits for an error message that was not on screen before the last <see cref="ExpectSuccess"/> call
    /// or page load and returns its summary.
    /// </summary>
    public string WaitForErrorSummary() => Wait.UntilFindElement(NewError).FindElement(Summary).Text.Trim();

    /// <summary>
    /// Runs the action and returns the detail of the error message it reports.
    /// </summary>
    /// <param name="action">Action that ends with an error message, e.g. a click on a save button.</param>
    public string ExpectErrorDetail(Action action)
    {
        MarkShown();
        action();
        return Wait.UntilFindElement(NewError).FindElement(Detail).Text.Trim();
    }

    /// <summary>
    /// Closes every toast on screen and waits until they are gone. Toasts show up in the top right corner over
    /// the top bar, and PrimeNG stops their timer while the mouse is over them, so a toast that appeared under
    /// the cursor after the previous click stays there and takes the clicks meant for the top bar buttons.
    /// </summary>
    public void CloseAll()
    {
        if (!ExistsNow(Message))
            return;

        // The close buttons are clicked from script: a toast stacked over another one would intercept a real click.
        // They are clicked again on every check, as a new toast may show up while the old ones are leaving.
        Wait.Until(_ =>
        {
            ((IJavaScriptExecutor)Driver).ExecuteScript(
                "document.querySelectorAll('.p-toast-close-button').forEach(button => button.click());");
            return !ExistsNow(Message);
        });
    }

    private void MarkShown() =>
        ((IJavaScriptExecutor)Driver).ExecuteScript($$"""
            document.querySelectorAll('.p-toast-message').forEach(m => m.setAttribute('{{SeenAttribute}}', ''));
            window.__uitestToasts = [];
            window.__uitestToastObserver ??= new MutationObserver(records => records
              .flatMap(record => [...record.addedNodes])
              .filter(node => node instanceof Element)
              .flatMap(node => node.matches('.p-toast-message') ? [node] : [...node.querySelectorAll('.p-toast-message')])
              .forEach(message => setTimeout(() => window.__uitestToasts?.push(message.innerText.replace(/\s+/g, ' ').trim()))));
            window.__uitestToastObserver.observe(document.body, { childList: true, subtree: true });
            """);
}
