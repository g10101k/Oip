namespace Oip.UiTest.Pages;

/// <summary>
/// The security tab of a module instance: the realm roles granted each right of the module.
/// </summary>
internal class ModuleSecurityTab(IWebDriver driver) : BasePage(driver)
{
    /// <summary>
    /// The right to see the module instance in the menu and to open it.
    /// </summary>
    public const string ReadRight = "read";

    private static readonly By SaveButton = By.Id("oip-security-save-button");

    /// <summary>
    /// Opens the security tab of the module instance shown in the browser.
    /// </summary>
    public ModuleSecurityTab Open()
    {
        new TopBar(Driver).OpenModuleTab("security");
        Wait.UntilFindElement(Roles(ReadRight));
        return this;
    }

    /// <summary>
    /// Grants the right to the role, keeping the roles already granted.
    /// </summary>
    /// <param name="right">Right code, e.g. <see cref="ReadRight"/>.</param>
    /// <param name="role">Realm role name.</param>
    public ModuleSecurityTab Grant(string right, string role)
    {
        new PrimeMultiSelect(Driver, Roles(right)).Select(role);
        return this;
    }

    /// <summary>
    /// Takes the right away from the role, keeping the other roles.
    /// </summary>
    /// <param name="right">Right code, e.g. <see cref="ReadRight"/>.</param>
    /// <param name="role">Realm role name.</param>
    public ModuleSecurityTab Revoke(string right, string role)
    {
        new PrimeMultiSelect(Driver, Roles(right)).Deselect(role);
        return this;
    }

    /// <summary>
    /// Checks whether the right is granted to the role in the form.
    /// </summary>
    /// <param name="right">Right code, e.g. <see cref="ReadRight"/>.</param>
    /// <param name="role">Realm role name.</param>
    public bool IsGranted(string right, string role) => new PrimeMultiSelect(Driver, Roles(right)).IsSelected(role);

    /// <summary>
    /// Saves the rights and waits until the backend confirms it.
    /// </summary>
    public void Save() => new Toasts(Driver).ExpectSuccess(() => Wait.UntilClick(SaveButton));

    private static By Roles(string right) => By.Id($"oip-security-multiselect-{right}");
}
