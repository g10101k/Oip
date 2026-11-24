namespace Oip.UiTest.Pages;

/// <summary>
/// Access control list editor (oip-object-acl-editor) inside a container: the security tab of a module or the menu
/// item dialog.
/// </summary>
/// <param name="driver">Web driver.</param>
/// <param name="scope">CSS selector of the container, prepended to every selector of the editor.</param>
internal class ObjectAclEditor(IWebDriver driver, string scope) : BasePage(driver)
{
    private const string Allow = "allow";
    private const string NotSet = "none";
    private const int StateCount = 3;

    private By SubjectSearch => Css("#oip-acl-subject-search");
    private By InheritSwitch => Css("#oip-acl-inherit");
    private By InheritLabel => Css("label[for='oip-acl-inherit']");

    /// <summary>
    /// Waits until the list of the object is loaded.
    /// </summary>
    public ObjectAclEditor WaitLoaded()
    {
        Wait.UntilFindElement(SubjectSearch);
        return this;
    }

    /// <summary>
    /// Allows the right to the role explicitly, adding the role to the list when it is not there yet.
    /// </summary>
    public ObjectAclEditor Grant(string right, string role)
    {
        EnsureRow(role);
        SetState(right, role, Allow);
        return this;
    }

    /// <summary>
    /// Clears the explicit right of the role. Rights inherited from the folder are not affected.
    /// </summary>
    public ObjectAclEditor Revoke(string right, string role)
    {
        if (Driver.FindElements(Row(role)).Count > 0)
            SetState(right, role, NotSet);
        return this;
    }

    /// <summary>
    /// Whether the right is allowed to the role explicitly.
    /// </summary>
    public bool IsGranted(string right, string role) =>
        Driver.FindElements(Cell(right, role)).FirstOrDefault()?.GetAttribute("data-state") == Allow;

    /// <summary>
    /// Turns the inheritance of the parent rights on or off.
    /// </summary>
    public ObjectAclEditor SetInheritance(bool inherit)
    {
        if (Wait.UntilFindElement(InheritSwitch).Selected != inherit)
            Wait.UntilClick(InheritLabel);
        Wait.Until(d => d.FindElement(InheritSwitch).Selected == inherit);
        return this;
    }

    private void EnsureRow(string role)
    {
        if (Driver.FindElements(Row(role)).Count > 0)
            return;

        Wait.UntilInput(SubjectSearch, role);
        // The suggestions are rendered in an overlay attached to the body, outside the container.
        Wait.UntilClick(By.CssSelector($"[data-subject-option='{Key(role)}']"));
        Wait.UntilFindElement(Row(role));
    }

    private void SetState(string right, string role, string state)
    {
        var button = Css($"tr[data-subject='{Key(role)}'] [data-right='{right}'] button");
        for (var i = 0; i < StateCount && Wait.UntilFindElement(Cell(right, role)).GetAttribute("data-state") != state; i++)
            Wait.UntilClick(button);

        Wait.Until(d => d.FindElement(Cell(right, role)).GetAttribute("data-state") == state);
    }

    private static string Key(string role) => $"role:{role}";

    private By Row(string role) => Css($"tr[data-subject='{Key(role)}']");

    private By Cell(string right, string role) => Css($"tr[data-subject='{Key(role)}'] [data-right='{right}']");

    private By Css(string selector) => By.CssSelector($"{scope} {selector}".Trim());
}
