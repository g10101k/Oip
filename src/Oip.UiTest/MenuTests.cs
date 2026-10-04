using Oip.UiTest.Pages;

namespace Oip.UiTest;

[Order(2)]
internal class MenuTests : BaseTest
{
    private const string RootFolderName = "#RootFolder";
    private const string DashboardLabel = "#DashboardModule";
    private const string WeatherLabel = "#WeatherForecastModule";
    private const string WeatherForDeleteLabel = "#WeatherForecastModuleForDelete";
    private const string EditLabel = "#ModuleForEdit";
    private const string EditedLabel = "#ModuleEdited";
    private const string CopyLabel = "#ModuleForCopy";

    private const string DashboardModule = "DashboardModule";
    private const string WeatherForecastModule = SideMenu.WeatherForecastModule;

    /// <summary>
    /// Icon WeatherForecastModuleController declares for its module; a new instance takes it by default.
    /// </summary>
    private const string WeatherForecastModuleIcon = "pi-sun";

    private readonly By _rootFolder = SideMenu.Folder(RootFolderName);

    /// <summary>
    /// Locates a module instance by its label inside the folder these tests own, so a leftover
    /// instance with the same label elsewhere in the menu does not make the create tests skip their work.
    /// </summary>
    private static By InRootFolder(string menuLabel) => SideMenu.ItemInFolder(RootFolderName, menuLabel);

    /// <summary>
    /// Creates a module instance in the root folder unless the folder already holds one with that label.
    /// </summary>
    private void EnsureInRootFolder(string menuLabel, string moduleName)
    {
        if (Menu.Contains(InRootFolder(menuLabel))) return;

        Menu.CreateModuleInstance(_rootFolder, menuLabel, moduleName);
        Menu.Open(InRootFolder(menuLabel));
    }

    [Test, Order(1)]
    public void CreateRootFolder()
    {
        if (Menu.Contains(_rootFolder)) return;

        Menu.CreateRootFolder(RootFolderName);
    }

    [Test, Order(2)]
    public void CreateDashboard() => EnsureInRootFolder(DashboardLabel, DashboardModule);

    [Test, Order(3)]
    public void CreateWeatherModule() => EnsureInRootFolder(WeatherLabel, WeatherForecastModule);

    [Test, Order(3)]
    public void CreateAndDeleteWeatherModule()
    {
        EnsureInRootFolder(WeatherForDeleteLabel, WeatherForecastModule);

        Menu.WaitLoaded();
        Menu.Delete(InRootFolder(WeatherForDeleteLabel));
    }

    [Test, Order(4)]
    public void ChangeMenuItemPosition()
    {
        var firstItemLocator = InRootFolder(DashboardLabel);
        var secondItemLocator = InRootFolder(WeatherLabel);

        Menu.WaitLoaded();
        Wait.Until(d => d.FindElement(firstItemLocator));
        Wait.Until(d => d.FindElement(secondItemLocator));

        // Determine which of the two sibling items currently renders above the other,
        // since the order isn't guaranteed by the previous tests alone.
        var topLocator = Driver.FindElement(firstItemLocator).Location.Y < Driver.FindElement(secondItemLocator).Location.Y
            ? firstItemLocator
            : secondItemLocator;
        var bottomLocator = ReferenceEquals(topLocator, firstItemLocator) ? secondItemLocator : firstItemLocator;

        // Move the top item down via the context menu and verify it now renders below the other item.
        Menu.MoveDown(topLocator);
        Wait.Until(d => d.FindElement(topLocator).Location.Y > d.FindElement(bottomLocator).Location.Y);

        // Move it back up, restoring the original order for subsequent test runs.
        Menu.MoveUp(topLocator);
        Wait.Until(d => d.FindElement(topLocator).Location.Y < d.FindElement(bottomLocator).Location.Y);
    }

    [Test, Order(5)]
    public void CreatedModuleGetsModuleIcon()
    {
        EnsureInRootFolder(WeatherLabel, WeatherForecastModule);

        Assert.That(IconClasses(InRootFolder(WeatherLabel)), Has.Member(WeatherForecastModuleIcon));
    }

    [Test, Order(5)]
    public void EditMenuItem()
    {
        // A previous run that failed after saving leaves the edited item behind.
        if (Menu.Contains(InRootFolder(EditedLabel)))
            Menu.Delete(InRootFolder(EditedLabel));
        EnsureInRootFolder(EditLabel, WeatherForecastModule);

        Menu.Edit(InRootFolder(EditLabel))
            .SetLabel(EditedLabel)
            .SelectIcon("heart")
            .Save();

        Wait.UntilDisappear(InRootFolder(EditLabel));
        Assert.That(IconClasses(InRootFolder(EditedLabel)), Has.Member("pi-heart"));
    }

    [Test, Order(5)]
    public void CopyMenuItem()
    {
        var copyLabel = $"{CopyLabel} (copy)";
        if (Menu.Contains(InRootFolder(copyLabel)))
            Menu.Delete(InRootFolder(copyLabel));
        EnsureInRootFolder(CopyLabel, WeatherForecastModule);

        Menu.Copy(InRootFolder(CopyLabel));

        Wait.UntilFindElement(InRootFolder(copyLabel));
        Assert.Multiple(() =>
        {
            Assert.That(Menu.IsAbove(InRootFolder(CopyLabel), InRootFolder(copyLabel)), Is.True,
                "The copy is expected below the original");
            Assert.That(Menu.GetPath(InRootFolder(copyLabel)), Is.Not.EqualTo(Menu.GetPath(InRootFolder(CopyLabel))),
                "The copy is expected to be a separate module instance");
            Assert.That(Menu.GetIcon(InRootFolder(copyLabel)), Is.EqualTo(Menu.GetIcon(InRootFolder(CopyLabel))));
        });
    }

    private string[] IconClasses(By item) => Menu.GetIcon(item).Split(' ', StringSplitOptions.RemoveEmptyEntries);

    [Test, Order(6)]
    public void DeleteRootFolder()
    {
        if (!Menu.Contains(_rootFolder)) return;

        Menu.Delete(_rootFolder);
    }
}
