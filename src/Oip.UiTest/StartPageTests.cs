using Oip.UiTest.Pages;

namespace Oip.UiTest;

/// <summary>
/// Checks which module instance the application root opens: the start page chosen by the user,
/// otherwise the first module of the menu, otherwise the no modules page.
/// </summary>
[Order(5)]
internal class StartPageTests : BaseTest
{
    private const string FolderLabel = "#StartPageFolder";
    private const string FirstModuleLabel = "#StartPageFirstModule";
    private const string SecondModuleLabel = "#StartPageSecondModule";

    private readonly By _folder = SideMenu.Folder(FolderLabel);
    private string _secondModulePath = null!;

    /// <summary>
    /// The start page the administrator had chosen before the tests, restored afterwards.
    /// </summary>
    private string? _originalStartPath;

    private static By InFolder(string menuLabel) => SideMenu.ItemInFolder(FolderLabel, menuLabel);

    [OneTimeSetUp]
    public void CreateModuleInstances()
    {
        SignInAs(TestSetup.Admin);
        if (Menu.Contains(_folder))
            Menu.Delete(_folder);

        // The root opens a chosen start page instead of the first module, which tells whether one is set.
        var landingPath = OpenApplicationRoot();
        if (landingPath != "/no-modules" && landingPath != Menu.GetFirstModulePath())
            _originalStartPath = landingPath;

        // Appended to the end of the menu, so they never become the first module unless the menu is empty.
        Menu.CreateRootFolder(FolderLabel);
        Menu.CreateModuleInstance(_folder, FirstModuleLabel, SideMenu.WeatherForecastModule);
        Menu.CreateModuleInstance(_folder, SecondModuleLabel, SideMenu.WeatherForecastModule);
        _secondModulePath = Menu.GetPath(InFolder(SecondModuleLabel));
    }

    [OneTimeTearDown]
    public void DeleteModuleInstances()
    {
        SignInAs(TestSetup.Admin);
        // Deleting the folder also drops the start page that pointed into it.
        if (Menu.Contains(_folder))
            Menu.Delete(_folder);

        if (_originalStartPath != null)
            Menu.SetStartModule(SideMenu.ItemByPath(_originalStartPath));
    }

    [Test, Order(1)]
    public void RootOpensStartModule()
    {
        SignInAs(TestSetup.Admin);
        Menu.SetStartModule(InFolder(SecondModuleLabel));

        Assert.That(OpenApplicationRoot(), Is.EqualTo(_secondModulePath));
    }

    [Test, Order(2)]
    public void RootOpensFirstModuleWithoutStartModule()
    {
        SignInAs(TestSetup.Admin);
        if (!Menu.IsStartModule(InFolder(SecondModuleLabel)))
            Menu.SetStartModule(InFolder(SecondModuleLabel));

        Menu.UnsetStartModule(InFolder(SecondModuleLabel));

        var landingPath = OpenApplicationRoot();
        Assert.That(landingPath, Is.EqualTo(Menu.GetFirstModulePath()));
        Assert.That(landingPath, Is.Not.EqualTo(_secondModulePath));
    }

    [Test, Order(3)]
    public void RootOpensNoModulesPageForUserWithoutModules()
    {
        SignInAs(TestSetup.User);
        if (Menu.CountItemsOnServer() > 0)
            Assert.Ignore("The user role can read module instances in this environment, " +
                          "the page is only reachable for it on a clean database.");

        Assert.That(OpenApplicationRoot(), Is.EqualTo("/no-modules"));
    }

    /// <summary>
    /// Loads the application root and returns the path the start page guard redirects to.
    /// </summary>
    private string OpenApplicationRoot()
    {
        Menu.Navigate("/");
        var path = Menu.WaitForRedirectFrom("/");
        Menu.WaitForAppInteractive();
        return path;
    }
}
