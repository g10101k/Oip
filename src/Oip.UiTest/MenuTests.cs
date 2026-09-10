using Oip.UiTest.Extensions;

namespace Oip.UiTest;

[Order(2)]
internal class MenuTests : BaseTest
{
    private readonly By _rootFolderLocator = By.XPath("//div[contains(text(),'#RootFolder')]");
    private readonly By _contextMenuSub = By.TagName("p-contextmenu-sub");
    private readonly By _layoutSidebar = By.ClassName("layout-sidebar");

    private readonly By _menuItem = By.CssSelector(".layout-menu li");

    // The <p-dialog> custom element is always present in the DOM (even closed), so it can't be used
    // to detect open/closed state; the actual dialog panel is only rendered as this div while open.
    private readonly By _createDialog = By.CssSelector("div.p-dialog");

    // The context menu item labels are translated ("New"/"Add"/"Delete" etc. depending on locale),
    // so we target them by their icons instead of their text to stay locale-independent.
    private readonly By _contextMenuNewItem = By.CssSelector(".p-contextmenu-item-icon.pi-plus");
    private readonly By _contextMenuDeleteItem = By.CssSelector(".p-contextmenu-item-icon.pi-trash");
    private readonly By _contextMenuMoveUpItem = By.CssSelector(".p-contextmenu-item-icon.pi-angle-up");
    private readonly By _contextMenuMoveDownItem = By.CssSelector(".p-contextmenu-item-icon.pi-angle-down");

    /// <summary>
    /// Waits until the menu has finished its (async) initial load, so a subsequent existence
    /// check for a menu item reflects the real state instead of racing the load.
    /// </summary>
    private void WaitForMenuLoaded() => Wait.Until(d => d.FindElement(_menuItem));

    /// <summary>
    /// Clicks an item of the currently open context menu, addressing it by its icon.
    /// PrimeNG rebuilds the shared context menu every time its model is replaced, so looking up the
    /// menu and its item in two separate steps races with that rebuild and goes stale; locate and
    /// click in one retried step instead.
    /// </summary>
    private void ClickContextMenuItem(By itemIconLocator) =>
        Wait.Until(d => d.FindElement(_contextMenuSub).FindElement(itemIconLocator).Click());

    /// <summary>
    /// Locates a module instance by its label inside the folder these tests own.
    /// A plain "//span[text()=...]" matches the whole sidebar, so a leftover instance with the same
    /// label anywhere else in the menu makes the create tests skip their work and leaves the later
    /// tests operating on items that are not siblings.
    /// </summary>
    private By InRootFolder(string menuLabel) =>
        By.XPath($"//li[div[contains(text(),'{RootFolderName}')]]//span[text()='{menuLabel}']");

    [Test, Order(1)]
    public void CreateRootFolder()
    {
        var layoutSidebar = Wait.Until(d => d.FindElement(_layoutSidebar));
        WaitForMenuLoaded();
        if (layoutSidebar.ExistsNow(_rootFolderLocator)) return;

        Actions.ContextClick(layoutSidebar).Perform();
        ClickContextMenuItem(_contextMenuNewItem);
        var createLabel = Wait.Until(d => d.FindElement(OipMenuCreateItemLabel));
        createLabel.SendKeys(RootFolderName);
        Driver.FindElement(OipMenuItemCreateModule).Click();

        var dialog = Wait.Until(d => d.FindElement(_createDialog));
        var objectMappingItem = Wait.Until(d => dialog.FindElement(By.XPath("//span[text()='FolderModule']")));
        Driver.ScrollToElement(objectMappingItem).Click();

        Driver.FindElement(OipMenuItemCreateSaveButton).Click();
        Wait.Until(d => d.FindElement(_rootFolderLocator));
    }

    [Test, Order(2)]
    public void CreateDashboard()
    {
        const string menuLabel = "#DashboardModule";

        var layoutSidebar = Wait.Until(d => d.FindElement(_layoutSidebar));
        WaitForMenuLoaded();
        if (layoutSidebar.ExistsNow(InRootFolder(menuLabel))) return;

        var rootFolderItem = Wait.Until(d => d.FindElement(_rootFolderLocator));

        Actions.ContextClick(rootFolderItem).Perform();
        ClickContextMenuItem(_contextMenuNewItem);

        var dialog = Wait.Until(d => d.FindElement(_createDialog));
        var label = Driver.FindElement(OipMenuItemCreateLabel);
        label.Clear();
        label.SendKeys(menuLabel);

        var selectModule = Driver.FindElement(OipMenuItemCreateModule);
        selectModule.Click();

        var objectMappingItem = Wait.Until(d => dialog.FindElement(By.XPath("//span[text()='DashboardModule']")));
        Driver.ScrollToElement(objectMappingItem).Click();

        Driver.FindElement(OipMenuItemCreateSaveButton).Click();
        Wait.UntilDisappear(_createDialog);

        GoToModuleInstance(InRootFolder(menuLabel));
    }

    [Test, Order(3)]
    public void CreateWeatherModule()
    {
        const string menuLabel = "#WeatherForecastModule";
        var layoutSidebar = Wait.Until(d => d.FindElement(_layoutSidebar));
        WaitForMenuLoaded();
        if (layoutSidebar.ExistsNow(InRootFolder(menuLabel))) return;
        var folderItem = Wait.Until(d => d.FindElement(_rootFolderLocator));
        Actions.ContextClick(folderItem).Perform();

        ClickContextMenuItem(_contextMenuNewItem);

        var dialog = Wait.Until(d => d.FindElement(_createDialog));
        var label = Driver.FindElement(OipMenuItemCreateLabel);
        label.Clear();
        label.SendKeys(menuLabel);

        Driver.FindElement(OipMenuItemCreateModule).Click();

        var weatherModuleItem =
            Wait.Until(d => dialog.FindElement(By.XPath("//span[text()='WeatherForecastModule']")));
        Driver.ScrollToElement(weatherModuleItem).Click();

        Driver.FindElement(OipMenuItemCreateSaveButton).Click();
        Wait.UntilDisappear(_createDialog);
        GoToModuleInstance(InRootFolder(menuLabel));
    }

    [Test, Order(3)]
    public void CreateAndDeleteWeatherModule()
    {
        const string menuLabel = "#WeatherForecastModuleForDelete";
        var layoutSidebar = Wait.Until(d => d.FindElement(_layoutSidebar));
        WaitForMenuLoaded();
        if (!layoutSidebar.ExistsNow(InRootFolder(menuLabel)))
        {
            var folderItem = Wait.Until(d => d.FindElement(_rootFolderLocator));
            Actions.ContextClick(folderItem).Perform();

            ClickContextMenuItem(_contextMenuNewItem);

            var dialog = Wait.Until(d => d.FindElement(_createDialog));
            var label = Driver.FindElement(OipMenuItemCreateLabel);
            label.Clear();
            label.SendKeys(menuLabel);

            Driver.FindElement(OipMenuItemCreateModule).Click();

            var weatherModuleItem =
                Wait.Until(d => dialog.FindElement(By.XPath("//span[text()='WeatherForecastModule']")));
            Driver.ScrollToElement(weatherModuleItem).Click();

            Driver.FindElement(OipMenuItemCreateSaveButton).Click();
            Wait.UntilDisappear(_createDialog);
            GoToModuleInstance(InRootFolder(menuLabel));
        }

        layoutSidebar = Wait.Until(d => d.FindElement(_layoutSidebar));
        WaitForMenuLoaded();
        var moduleInstanceForDelete = layoutSidebar.FindElement(InRootFolder(menuLabel));
        Actions.ContextClick(moduleInstanceForDelete).Perform();

        ClickContextMenuItem(_contextMenuDeleteItem);

        // The delete confirmation is a PrimeNG ConfirmDialog (<div class="p-confirmdialog p-dialog">),
        // not the <p-dialog> tag used by the create dialog above, so it needs its own locator.
        var deleteDialog = Wait.Until(d => d.FindElement(By.CssSelector(".p-confirmdialog")));
        deleteDialog.FindElement(ConfirmDialogAcceptButton).Click();

        Wait.UntilDisappear(InRootFolder(menuLabel));
    }

    [Test, Order(4)]
    public void ChangeMenuItemPosition()
    {
        const string firstMenuLabel = "#DashboardModule";
        const string secondMenuLabel = "#WeatherForecastModule";
        var firstItemLocator = InRootFolder(firstMenuLabel);
        var secondItemLocator = InRootFolder(secondMenuLabel);

        Wait.Until(d => d.FindElement(_layoutSidebar));
        WaitForMenuLoaded();
        Wait.Until(d => d.FindElement(firstItemLocator));
        Wait.Until(d => d.FindElement(secondItemLocator));

        // Determine which of the two sibling items currently renders above the other,
        // since the order isn't guaranteed by the previous tests alone.
        var topLocator = Driver.FindElement(firstItemLocator).Location.Y < Driver.FindElement(secondItemLocator).Location.Y
            ? firstItemLocator
            : secondItemLocator;
        var bottomLocator = ReferenceEquals(topLocator, firstItemLocator) ? secondItemLocator : firstItemLocator;

        // Move the top item down via the context menu and verify it now renders below the other item.
        MoveMenuItem(topLocator, _contextMenuMoveDownItem);
        Wait.Until(d => d.FindElement(topLocator).Location.Y > d.FindElement(bottomLocator).Location.Y);

        // Move it back up, restoring the original order for subsequent test runs.
        MoveMenuItem(topLocator, _contextMenuMoveUpItem);
        Wait.Until(d => d.FindElement(topLocator).Location.Y < d.FindElement(bottomLocator).Location.Y);
    }

    private void MoveMenuItem(By itemLocator, By moveDirectionIconLocator)
    {
        var item = Wait.Until(d => d.FindElement(itemLocator));
        Actions.ContextClick(item).Perform();
        ClickContextMenuItem(moveDirectionIconLocator);
    }

    [Test, Order(5)]
    public void DeleteRootFolder()
    {
        var layoutSidebar = Wait.Until(d => d.FindElement(_layoutSidebar));
        WaitForMenuLoaded();
        if (!layoutSidebar.ExistsNow(_rootFolderLocator)) return;

        var rootFolderItem = Wait.Until(d => d.FindElement(_rootFolderLocator));
        Actions.ContextClick(rootFolderItem).Perform();

        ClickContextMenuItem(_contextMenuDeleteItem);

        var deleteDialog = Wait.Until(d => d.FindElement(By.CssSelector(".p-confirmdialog")));
        deleteDialog.FindElement(ConfirmDialogAcceptButton).Click();

        Wait.UntilDisappear(_rootFolderLocator);
    }
}