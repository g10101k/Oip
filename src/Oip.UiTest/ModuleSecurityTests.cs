using Oip.UiTest.Pages;

namespace Oip.UiTest;

[Order(7)]
internal class ModuleSecurityTests : BaseTest
{
    private const string UserRole = "user";
    private const string FolderLabel = "#ModuleSecurityFolder";
    private const string ModuleLabel = "#ModuleSecurityModule";

    private readonly By _folder = SideMenu.Folder(FolderLabel);
    private readonly By _module = SideMenu.ItemInFolder(FolderLabel, ModuleLabel);
    private string _modulePath = null!;

    private ModuleSecurityTab Security => new(Driver);

    [OneTimeSetUp]
    public void CreateModuleInstance()
    {
        SignInAs(TestSetup.Admin);
        if (Menu.Contains(_folder))
            Menu.Delete(_folder);

        // A child is listed under its folder only when the user can read the folder as well.
        Menu.CreateRootFolder(FolderLabel);
        Menu.Edit(_folder).AddViewRole(UserRole).Save();

        // A new module instance is readable by administrators only.
        Menu.CreateModuleInstance(_folder, ModuleLabel, SideMenu.WeatherForecastModule);
        _modulePath = Menu.GetPath(_module);
    }

    [OneTimeTearDown]
    public void DeleteModuleInstance()
    {
        SignInAs(TestSetup.Admin);
        if (Menu.Contains(_folder))
            Menu.Delete(_folder);
    }

    [Test, Order(1)]
    public void UserSeesModuleAfterReadRightIsGranted()
    {
        SignInAs(TestSetup.Admin);
        OpenSecurityTab().Grant(ModuleSecurityTab.ReadRight, UserRole).Save();

        Menu.Reload();
        Assert.That(Security.Open().IsGranted(ModuleSecurityTab.ReadRight, UserRole), Is.True,
            "The granted right is not kept after a page reload");

        SignInAs(TestSetup.User);
        Assert.That(Menu.Contains(_module), Is.True, "The module is not listed in the menu of a regular user");
        Menu.Open(_module);
        Assert.That(Menu.CurrentPath, Is.EqualTo(_modulePath));
    }

    [Test, Order(2)]
    public void UserLosesModuleAfterReadRightIsRevoked()
    {
        SignInAs(TestSetup.Admin);
        var security = OpenSecurityTab();
        // Independent of the previous test: grant the right first if it is not granted yet.
        if (!security.IsGranted(ModuleSecurityTab.ReadRight, UserRole))
            security.Grant(ModuleSecurityTab.ReadRight, UserRole).Save();
        security.Revoke(ModuleSecurityTab.ReadRight, UserRole).Save();

        SignInAs(TestSetup.User);
        Assert.That(Menu.Contains(_module), Is.False, "The module is still listed in the menu of a regular user");
        Menu.Navigate(_modulePath);
        Menu.WaitForPath("/access");
    }

    private ModuleSecurityTab OpenSecurityTab()
    {
        Menu.Navigate(_modulePath);
        Menu.WaitForAppInteractive();
        return Security.Open();
    }
}
