using Oip.UiTest.Extensions;

namespace Oip.UiTest.Pages;

/// <summary>
/// The dialog that creates a new module instance in the menu.
/// </summary>
internal class MenuItemCreateDialog(IWebDriver driver) : BasePage(driver)
{
    // The <p-dialog> custom element is always present in the DOM (even closed), so it can't be used
    // to detect open/closed state; the actual dialog panel is only rendered as this div while open.
    private static readonly By Dialog = By.CssSelector("div.p-dialog");
    private static readonly By Label = By.Id("oip-menu-item-create-label");
    private static readonly By Module = By.Id("oip-menu-item-create-module");
    private static readonly By SaveButton = By.Id("oip-menu-item-create-save");

    /// <summary>
    /// Fills in the dialog, saves it and waits until it closes.
    /// </summary>
    /// <param name="label">Label of the new menu item.</param>
    /// <param name="moduleName">Name of the module to instantiate, as shown in the module selector.</param>
    public void Create(string label, string moduleName)
    {
        Wait.UntilFindElement(Dialog);

        var labelInput = Wait.UntilFindElement(Label);
        labelInput.Clear();
        labelInput.SendKeys(label);

        Driver.FindElement(Module).Click();
        // Only the options of the select: a menu item may carry the same text as a module name.
        var moduleItem = Wait.Until(d => d.FindElement(By.XPath($"//li[@role='option'][.//span[text()='{moduleName}']]")));
        Driver.ScrollToElement(moduleItem).Click();
        // The options overlay closes with an animation and covers the save button until it is gone.
        Wait.UntilDisappear(By.CssSelector("li[role='option']"));

        Driver.FindElement(SaveButton).Click();
        Wait.UntilDisappear(Dialog);
    }
}
