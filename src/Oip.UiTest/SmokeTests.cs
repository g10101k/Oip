namespace Oip.UiTest;

/// <summary>
/// Opens every built-in page by its address and checks it renders without browser console errors.
/// Catches broken lazy loading and dependency injection, which do not fail the build.
/// </summary>
[Order(3)]
internal class SmokeTests : BaseTest
{
    /// <summary>
    /// Errors of the ng serve development build only. The module federation build of ngx-build-plus emits
    /// styles.js with <c>import.meta</c> while index.html loads it as a classic script; the production build
    /// extracts the styles to CSS and has no styles.js.
    /// </summary>
    private static readonly string[] DevServerErrors = ["styles.js", "Cannot use 'import.meta' outside a module"];

    [OneTimeSetUp]
    public void SignInAsAdmin() => SignInAs(TestSetup.Admin);

    [TestCase("/config", "/config", "[qa-id='oip-app-config-language-select']")]
    [TestCase("/modules", "/modules", "#oip-app-modules-filter")]
    [TestCase("/applications", "/applications", "#oip-applications-filter")]
    [TestCase("/sessions", "/sessions", "#oip-auth-sessions-filter")]
    [TestCase("/notfound", "/notfound", "#oip-app-notfound-go-to-home-button")]
    [TestCase("/this-page-does-not-exist", "/notfound", "#oip-app-notfound-go-to-home-button")]
    [TestCase("/no-modules", "/no-modules", "app-no-modules")]
    public void PageOpensWithoutConsoleErrors(string path, string expectedPath, string contentSelector)
    {
        Console.Clear();

        Menu.Navigate(path);

        Menu.WaitForPath(expectedPath);
        Wait.UntilFindElement(By.CssSelector(contentSelector));
        Menu.WaitForAppInteractive();
        var errors = Console.ReadErrors()
            .Where(error => !DevServerErrors.All(error.Contains))
            .ToList();
        Assert.That(errors, Is.Empty, $"Browser console errors on {path}");
    }
}
