using Oip.UiTest.Pages;

namespace Oip.UiTest;

/// <summary>
/// Checks what a user without the admin role can reach. The module instances are created by the
/// administrator: one readable by the user role and one readable by administrators only.
/// </summary>
[Order(4)]
internal class RoleAccessTests : BaseTest
{
    private const string UserRole = "user";
    private const string FolderLabel = "#RoleAccessFolder";
    private const string UserModuleLabel = "#UserReadableModule";
    private const string AdminModuleLabel = "#AdminOnlyModule";

    private readonly By _folder = SideMenu.Folder(FolderLabel);
    private string _userModulePath = null!;
    private string _adminModulePath = null!;

    private static By InFolder(string menuLabel) => SideMenu.ItemInFolder(FolderLabel, menuLabel);

    [OneTimeSetUp]
    public void CreateModuleInstances()
    {
        SignInAs(TestSetup.Admin);
        if (Menu.Contains(_folder))
            Menu.Delete(_folder);

        // A child is listed under its folder only when the user can read the folder as well.
        Menu.CreateRootFolder(FolderLabel);
        Menu.Edit(_folder).AddViewRole(UserRole).Save();

        Menu.CreateModuleInstance(_folder, UserModuleLabel, SideMenu.WeatherForecastModule);
        Menu.Edit(InFolder(UserModuleLabel)).AddViewRole(UserRole).Save();

        // A new module instance is readable by administrators only.
        Menu.CreateModuleInstance(_folder, AdminModuleLabel, SideMenu.WeatherForecastModule);

        _userModulePath = Menu.GetPath(InFolder(UserModuleLabel));
        _adminModulePath = Menu.GetPath(InFolder(AdminModuleLabel));
    }

    [OneTimeTearDown]
    public void DeleteModuleInstances()
    {
        SignInAs(TestSetup.Admin);
        if (Menu.Contains(_folder))
            Menu.Delete(_folder);
    }

    /// <summary>
    /// Control for <see cref="UserModuleHasNoSettingsAndSecurityTabs"/>: the tabs exist and are found by the test.
    /// </summary>
    [Test, Order(1)]
    public void AdminModuleHasSettingsAndSecurityTabs()
    {
        SignInAs(TestSetup.Admin);
        Menu.Navigate(_userModulePath);
        Menu.WaitForAppInteractive();

        TopBar.WaitForModuleTab("settings");
        TopBar.WaitForModuleTab("security");
    }

    [Order(2)]
    [TestCase("/modules")]
    [TestCase("/applications")]
    [TestCase("/sessions")]
    public void UserIsSentFromAdminPageToAccessDenied(string path)
    {
        SignInAs(TestSetup.User);

        Menu.Navigate(path);

        Menu.WaitForPath("/access");
    }

    [Test, Order(3)]
    public void UserContextMenuOffersOnlyStartPage()
    {
        SignInAs(TestSetup.User);

        var icons = Menu.GetContextMenuIcons(InFolder(UserModuleLabel));

        Assert.That(icons, Has.Count.EqualTo(1), "Only the start page entry is expected");
        // pi-star-fill sets the start page, pi-star clears it, depending on the current choice of the user.
        Assert.That(icons[0].Split(' '), Has.Some.Matches<string>(c => c is "pi-star" or "pi-star-fill"));
    }

    [Test, Order(4)]
    public void UserModuleHasNoSettingsAndSecurityTabs()
    {
        SignInAs(TestSetup.User);

        Menu.Open(InFolder(UserModuleLabel));

        Assert.Multiple(() =>
        {
            Assert.That(Menu.CurrentPath, Is.EqualTo(_userModulePath));
            Assert.That(TopBar.HasModuleTab("settings"), Is.False, "Settings tab is shown to a regular user");
            Assert.That(TopBar.HasModuleTab("security"), Is.False, "Security tab is shown to a regular user");
        });
    }

    [Test, Order(5)]
    public void UserIsSentFromUnreadableModuleToAccessDenied()
    {
        SignInAs(TestSetup.User);
        Assert.That(Menu.Contains(InFolder(AdminModuleLabel)), Is.False,
            "A module instance readable by administrators only is listed in the menu of a regular user");

        Menu.Navigate(_adminModulePath);

        Menu.WaitForPath("/access");
    }
}
