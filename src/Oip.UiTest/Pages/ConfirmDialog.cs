namespace Oip.UiTest.Pages;

/// <summary>
/// A PrimeNG ConfirmDialog (<c>div.p-confirmdialog.p-dialog</c>), such as the logout or delete confirmation.
/// </summary>
internal class ConfirmDialog(IWebDriver driver) : BasePage(driver)
{
    private static readonly By Dialog = By.CssSelector(".p-confirmdialog");
    private static readonly By AcceptButton = By.CssSelector(".p-confirmdialog-accept-button");

    /// <summary>
    /// Waits for the dialog and confirms it.
    /// </summary>
    public void Accept()
    {
        var dialog = Wait.UntilFindElement(Dialog);
        dialog.FindElement(AcceptButton).Click();
    }
}
