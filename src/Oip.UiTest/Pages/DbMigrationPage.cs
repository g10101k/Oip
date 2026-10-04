namespace Oip.UiTest.Pages;

/// <summary>
/// An instance of the database migration module: the migrations of the application database and their state.
/// </summary>
internal class DbMigrationPage(IWebDriver driver) : BasePage(driver)
{
    private static readonly By Rows = By.CssSelector("p-table tbody tr");
    private static readonly By MigrationName = By.CssSelector("p-table tbody tr td:first-child");
    private static readonly By Filter = By.CssSelector("p-toolbar input[pinputtext]");

    /// <summary>
    /// Waits until the migrations are listed.
    /// </summary>
    public DbMigrationPage WaitLoaded()
    {
        // The empty message is a single cell spanning the table, a migration row has a cell per column.
        Wait.UntilFindElement(By.CssSelector("p-table tbody tr td:nth-child(4)"));
        return this;
    }

    /// <summary>
    /// Filters the list by the migration name.
    /// </summary>
    /// <param name="text">Part of the migration name.</param>
    public DbMigrationPage FilterBy(string text)
    {
        var filter = Wait.UntilFindElement(Filter);
        filter.Clear();
        filter.SendKeys(text);
        Wait.Until(_ => GetNames().All(name => name.Contains(text, StringComparison.OrdinalIgnoreCase)));
        return this;
    }

    /// <summary>
    /// Names of the listed migrations.
    /// </summary>
    public IReadOnlyList<string> GetNames() =>
        FindAllNow(Driver, MigrationName).Select(cell => cell.Text.Trim()).ToList();

    /// <summary>
    /// Whether the listed migration with the given name is applied to the database.
    /// </summary>
    /// <param name="name">Part of the migration name.</param>
    public bool IsApplied(string name) => HasTag(name, column: 2, severity: "success");

    /// <summary>
    /// Whether the listed migration with the given name is pending.
    /// </summary>
    /// <param name="name">Part of the migration name.</param>
    public bool IsPending(string name) => HasTag(name, column: 4, severity: "warn");

    // The yes/no captions are translated, the tag severity is not.
    private bool HasTag(string name, int column, string severity) =>
        FindAllNow(Wait.UntilFindElement(By.XPath($"//p-table//tbody/tr[td[1][contains(., '{name}')]]/td[{column}]")),
            By.CssSelector($".p-tag-{severity}")).Count != 0;
}
