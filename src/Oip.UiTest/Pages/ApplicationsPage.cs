namespace Oip.UiTest.Pages;

/// <summary>
/// The application registry, administrators only. Rows are edited in place: the code, display name,
/// base URL, internal URL, icon and order columns turn into inputs.
/// </summary>
internal class ApplicationsPage(IWebDriver driver) : BasePage(driver)
{
    /// <summary>
    /// Path of the page.
    /// </summary>
    public const string Path = "/applications";

    private static readonly By AddButton = By.Id("oip-applications-add");
    private static readonly By Table = By.CssSelector("p-table");

    // A new row keeps its code editable, an edited existing row disables it.
    private static readonly By NewRow = By.XPath("//p-table//tbody/tr[td[1]//input[not(@disabled)]]");
    private static readonly By EditedRow = By.XPath("//p-table//tbody/tr[td[1]//input[@disabled]]");
    private static readonly By SaveRowButton = By.CssSelector("p-button[icon='pi pi-check']");
    private static readonly By EditRowButton = By.CssSelector("p-button[icon='pi pi-pencil']");
    private static readonly By DeleteRowButton = By.CssSelector("p-button[icon='pi pi-trash']");
    private static readonly By EnabledToggle = By.XPath("./td[7]//p-toggle-switch");
    private static readonly By EnabledTag = By.XPath("./td[7]//p-tag[contains(@class, 'p-tag-success')]");

    /// <summary>
    /// Opens the page and waits for the registry.
    /// </summary>
    public ApplicationsPage Open()
    {
        Navigate(Path);
        WaitForAppInteractive();
        Wait.UntilFindElement(Table);
        WaitForRows();
        return this;
    }

    /// <summary>
    /// Whether the registry lists an application with the given code.
    /// </summary>
    /// <param name="code">Application code.</param>
    public bool Contains(string code) => ExistsNow(Row(code));

    /// <summary>
    /// Display name of the application as the registry shows it.
    /// </summary>
    /// <param name="code">Application code.</param>
    public string GetDisplayName(string code) =>
        Wait.UntilFindElement(Row(code)).FindElement(By.XPath("./td[2]")).Text.Trim();

    /// <summary>
    /// Adds an enabled application and waits until the registry lists it.
    /// </summary>
    /// <param name="code">Application code.</param>
    /// <param name="displayName">Display name.</param>
    /// <param name="baseUrl">Public address; also used as the internal address.</param>
    public void Create(string code, string displayName, string baseUrl)
    {
        Wait.UntilClick(AddButton);
        var row = Wait.UntilFindElement(NewRow);
        SetCell(row, 1, code);
        SetCell(row, 2, displayName);
        SetCell(row, 3, baseUrl);
        SetCell(row, 4, baseUrl);

        new Toasts(Driver).ExpectSuccess(() => row.FindElement(SaveRowButton).Click());
        Wait.UntilFindElement(Row(code));
    }

    /// <summary>
    /// Tries to add an application without the internal address and returns the error the page reports.
    /// The draft row stays open.
    /// </summary>
    /// <param name="code">Application code.</param>
    /// <param name="displayName">Display name.</param>
    /// <param name="baseUrl">Public address.</param>
    public string CreateWithoutInternalUrl(string code, string displayName, string baseUrl)
    {
        Wait.UntilClick(AddButton);
        var row = Wait.UntilFindElement(NewRow);
        SetCell(row, 1, code);
        SetCell(row, 2, displayName);
        SetCell(row, 3, baseUrl);
        SetCell(row, 4, string.Empty);

        return new Toasts(Driver).ExpectErrorDetail(() => row.FindElement(SaveRowButton).Click());
    }

    /// <summary>
    /// Whether the registry shows the application as enabled.
    /// </summary>
    /// <param name="code">Application code.</param>
    public bool IsEnabled(string code) =>
        FindAllNow(Wait.UntilFindElement(Row(code)), EnabledTag).Count != 0;

    /// <summary>
    /// Enables or disables the application and waits until the registry shows the new state.
    /// </summary>
    /// <param name="code">Application code.</param>
    /// <param name="enabled">New state.</param>
    public void SetEnabled(string code, bool enabled)
    {
        Wait.UntilFindElement(Row(code)).FindElement(EditRowButton).Click();
        var row = Wait.UntilFindElement(EditedRow);
        var toggle = row.FindElement(EnabledToggle);
        if (toggle.GetAttribute("class")!.Contains("p-toggleswitch-checked") != enabled)
            toggle.Click();

        new Toasts(Driver).ExpectSuccess(() => row.FindElement(SaveRowButton).Click());
        Wait.Until(_ => ExistsNow(Row(code)) && IsEnabled(code) == enabled);
    }

    /// <summary>
    /// Changes the display name of the application and waits until the registry shows it.
    /// </summary>
    /// <param name="code">Application code.</param>
    /// <param name="displayName">New display name.</param>
    public void Rename(string code, string displayName)
    {
        Wait.UntilFindElement(Row(code)).FindElement(EditRowButton).Click();
        var row = Wait.UntilFindElement(EditedRow);
        SetCell(row, 2, displayName);

        new Toasts(Driver).ExpectSuccess(() => row.FindElement(SaveRowButton).Click());
        Wait.Until(_ => GetDisplayName(code) == displayName);
    }

    /// <summary>
    /// Deletes the application, confirming it, and waits until the registry no longer lists it.
    /// </summary>
    /// <param name="code">Application code.</param>
    public void Delete(string code)
    {
        new Toasts(Driver).ExpectSuccess(() =>
        {
            Wait.UntilFindElement(Row(code)).FindElement(DeleteRowButton).Click();
            new ConfirmDialog(Driver).Accept();
        });
        Wait.UntilDisappear(Row(code));
    }

    private void WaitForRows() => Wait.Until(_ => !ExistsNow(By.CssSelector("p-table .p-datatable-mask")));

    private static void SetCell(IWebElement row, int column, string value)
    {
        var input = row.FindElement(By.XPath($"./td[{column}]//input"));
        input.Clear();
        input.SendKeys(value);
    }

    private static By Row(string code) =>
        By.XPath($"//p-table//tbody/tr[td[1]//span[normalize-space()='{code}']]");
}
