namespace Oip.UiTest.Pages;

/// <summary>
/// Security tab of a module instance: the access control list editor of the instance.
/// </summary>
internal class ModuleSecurityTab(IWebDriver driver) : BasePage(driver)
{
    public const string ReadRight = "read";

    private static readonly By SaveButton = By.Id("oip-acl-save-button");

    private ObjectAclEditor Editor => new(Driver, "security");

    public ModuleSecurityTab Open()
    {
        new TopBar(Driver).OpenModuleTab("security");
        Wait.UntilFindElement(SaveButton);
        return this;
    }

    /// <inheritdoc cref="ObjectAclEditor.Grant" />
    public ModuleSecurityTab Grant(string right, string role)
    {
        Editor.Grant(right, role);
        return this;
    }

    /// <inheritdoc cref="ObjectAclEditor.Revoke" />
    public ModuleSecurityTab Revoke(string right, string role)
    {
        Editor.Revoke(right, role);
        return this;
    }

    /// <inheritdoc cref="ObjectAclEditor.IsGranted" />
    public bool IsGranted(string right, string role) => Editor.IsGranted(right, role);

    /// <inheritdoc cref="ObjectAclEditor.SetInheritance" />
    public ModuleSecurityTab SetInheritance(bool inherit)
    {
        Editor.SetInheritance(inherit);
        return this;
    }

    public void Save() => new Toasts(Driver).ExpectSuccess(() => Wait.UntilClick(SaveButton));
}
