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

    [Test, Order(1)]
    public void CreateRootFolder()
    {
        var layoutSidebar = Wait.Until(d => d.FindElement(_layoutSidebar));
        WaitForMenuLoaded();
        if (layoutSidebar.ExistsNow(_rootFolderLocator)) return;

        Actions.ContextClick(layoutSidebar).Perform();
        var contextMenu = Wait.Until(d => d.FindElement(_contextMenuSub));

        contextMenu.FindElement(_contextMenuNewItem).Click();
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
        if (layoutSidebar.ExistsNow(By.XPath($"//span[text()='{menuLabel}']"))) return;

        var rootFolderItem = Wait.Until(d => d.FindElement(_rootFolderLocator));

        Actions.ContextClick(rootFolderItem).Perform();
        var contextMenu = Driver.FindElement(_contextMenuSub);
        contextMenu.FindElement(_contextMenuNewItem).Click();

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

        GoToModuleInstance(menuLabel);
    }

    [Test, Order(3)]
    public void CreateWeatherModule()
    {
        const string menuLabel = "#WeatherForecastModule";
        var layoutSidebar = Wait.Until(d => d.FindElement(_layoutSidebar));
        WaitForMenuLoaded();
        if (layoutSidebar.ExistsNow(By.XPath($"//span[text()='{menuLabel}']"))) return;
        var folderItem = Wait.Until(d => d.FindElement(_rootFolderLocator));
        Actions.ContextClick(folderItem).Perform();

        var contextMenu = Driver.FindElement(_contextMenuSub);
        contextMenu.FindElement(_contextMenuNewItem).Click();

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
        GoToModuleInstance(menuLabel);
    }

    [Test, Order(3)]
    public void CreateAndDeleteWeatherModule()
    {
        const string menuLabel = "#WeatherForecastModuleForDelete";
        var layoutSidebar = Wait.Until(d => d.FindElement(_layoutSidebar));
        WaitForMenuLoaded();
        if (!layoutSidebar.ExistsNow(By.XPath($"//span[text()='{menuLabel}']")))
        {
            var folderItem = Wait.Until(d => d.FindElement(_rootFolderLocator));
            Actions.ContextClick(folderItem).Perform();

            Driver.FindElement(_contextMenuSub).FindElement(_contextMenuNewItem).Click();

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
            GoToModuleInstance(menuLabel);
        }

        layoutSidebar = Wait.Until(d => d.FindElement(_layoutSidebar));
        WaitForMenuLoaded();
        var moduleInstanceForDelete = layoutSidebar.FindElement(By.XPath($"//span[text()='{menuLabel}']"));
        Actions.ContextClick(moduleInstanceForDelete).Perform();

        Driver.FindElement(_contextMenuSub).FindElement(_contextMenuDeleteItem).Click();

        // The delete confirmation is a PrimeNG ConfirmDialog (<div class="p-confirmdialog p-dialog">),
        // not the <p-dialog> tag used by the create dialog above, so it needs its own locator.
        var deleteDialog = Wait.Until(d => d.FindElement(By.CssSelector(".p-confirmdialog")));
        deleteDialog.FindElement(ConfirmDialogAcceptButton).Click();

        Wait.UntilDisappear(By.XPath($"//span[text()='{menuLabel}']"));
    }

    [Test, Order(4)]
    public void ChangeMenuItemPosition()
    {
        const string firstMenuLabel = "#DashboardModule";
        const string secondMenuLabel = "#WeatherForecastModule";
        var firstItemLocator = By.XPath($"//span[text()='{firstMenuLabel}']");
        var secondItemLocator = By.XPath($"//span[text()='{secondMenuLabel}']");

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
        Driver.FindElement(_contextMenuSub).FindElement(moveDirectionIconLocator).Click();
    }

    [Test, Order(5)]
    public void DeleteRootFolder()
    {
        var layoutSidebar = Wait.Until(d => d.FindElement(_layoutSidebar));
        WaitForMenuLoaded();
        if (!layoutSidebar.ExistsNow(_rootFolderLocator)) return;

        var rootFolderItem = Wait.Until(d => d.FindElement(_rootFolderLocator));
        Actions.ContextClick(rootFolderItem).Perform();

        Driver.FindElement(_contextMenuSub).FindElement(_contextMenuDeleteItem).Click();

        var deleteDialog = Wait.Until(d => d.FindElement(By.CssSelector(".p-confirmdialog")));
        deleteDialog.FindElement(ConfirmDialogAcceptButton).Click();

        Wait.UntilDisappear(_rootFolderLocator);
    }
}