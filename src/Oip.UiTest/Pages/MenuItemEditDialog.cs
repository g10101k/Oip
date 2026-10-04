namespace Oip.UiTest.Pages;

/// <summary>
/// The dialog that edits the label, icon and view roles of a module instance.
/// </summary>
internal class MenuItemEditDialog(IWebDriver driver) : BasePage(driver)
{
    // The <p-dialog> custom element is always present in the DOM (even closed), so it can't be used
    // to detect open/closed state; the actual dialog panel is only rendered as this div while open.
    private static readonly By Dialog = By.CssSelector("div.p-dialog");
    private static readonly By Label = By.Id("oip-menu-item-edit-dialog-menu-input");
    private static readonly By Icon = By.Id("oip-menu-item-edit-dialog-icon");
    private static readonly By ViewRoles = By.Id("oip-menu-item-edit-dialog-roles-multi-select");
    private static readonly By SaveButton = By.Id("oip-menu-item-edit-dialog-save-edit-button");
    private static readonly By SelectFilter = By.CssSelector("input.p-select-filter");

    /// <summary>
    /// Waits until the dialog is open. It opens only after the realm roles and modules have loaded.
    /// </summary>
    public MenuItemEditDialog WaitOpen()
    {
        Wait.UntilFindElement(Label);
        return this;
    }

    /// <summary>
    /// Replaces the label of the menu item.
    /// </summary>
    /// <param name="label">New label.</param>
    public MenuItemEditDialog SetLabel(string label)
    {
        var input = Driver.FindElement(Label);
        input.Clear();
        input.SendKeys(label);
        return this;
    }

    /// <summary>
    /// Picks an icon by its PrimeIcons name without the <c>pi pi-</c> prefix, e.g. <c>heart</c>.
    /// </summary>
    /// <param name="iconName">Icon name as shown in the selector.</param>
    public MenuItemEditDialog SelectIcon(string iconName)
    {
        Driver.FindElement(Icon).Click();
        var filter = Wait.UntilFindElement(SelectFilter);
        filter.SendKeys(iconName);
        Wait.UntilClick(Option(iconName));
        return this;
    }

    /// <summary>
    /// Grants the read right to the realm role, keeping the roles already selected.
    /// </summary>
    /// <param name="role">Realm role name.</param>
    public MenuItemEditDialog AddViewRole(string role)
    {
        new PrimeMultiSelect(Driver, ViewRoles).Select(role);
        return this;
    }

    /// <summary>
    /// Saves the changes and waits until the dialog closes.
    /// </summary>
    public void Save()
    {
        Driver.FindElement(SaveButton).Click();
        Wait.UntilDisappear(Dialog);
    }

    private static By Option(string text) =>
        By.XPath($"//li[@role='option'][.//span[normalize-space()='{text}']]");
}
