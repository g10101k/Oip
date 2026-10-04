using Oip.UiTest.Extensions;

namespace Oip.UiTest.Pages;

/// <summary>
/// The sidebar menu with module instances and its context menu.
/// </summary>
internal class SideMenu(IWebDriver driver) : BasePage(driver)
{
    private static readonly By LayoutSidebar = By.ClassName("layout-sidebar");
    private static readonly By MenuItem = By.CssSelector(".layout-menu li");
    private static readonly By ContextMenuSub = By.TagName("p-contextmenu-sub");

    // The context menu item labels are translated ("New"/"Add"/"Delete" etc. depending on locale),
    // so we target them by their icons instead of their text to stay locale-independent.
    private static readonly By ContextMenuNewItem = By.CssSelector(".p-contextmenu-item-icon.pi-plus");
    private static readonly By ContextMenuDeleteItem = By.CssSelector(".p-contextmenu-item-icon.pi-trash");
    private static readonly By ContextMenuMoveUpItem = By.CssSelector(".p-contextmenu-item-icon.pi-angle-up");
    private static readonly By ContextMenuMoveDownItem = By.CssSelector(".p-contextmenu-item-icon.pi-angle-down");
    private static readonly By ContextMenuEditItem = By.CssSelector(".p-contextmenu-item-icon.pi-file-edit");
    private static readonly By ContextMenuCopyItem = By.CssSelector(".p-contextmenu-item-icon.pi-copy");
    private static readonly By ContextMenuSetStartItem = By.CssSelector(".p-contextmenu-item-icon.pi-star-fill");
    private static readonly By ContextMenuUnsetStartItem = By.CssSelector(".p-contextmenu-item-icon.pi-star");
    private static readonly By ContextMenuItems = By.CssSelector("li.p-contextmenu-item");
    private static readonly By ContextMenuItemIcon = By.CssSelector(".p-contextmenu-item-icon");

    // A leaf module renders its routerLink as href; folders and separators have no href.
    private static readonly By ModuleLink = By.CssSelector(".layout-menu a[href]");

    /// <summary>
    /// Module instance used to create folders.
    /// </summary>
    public const string FolderModule = "FolderModule";

    /// <summary>
    /// Locates a folder by its label.
    /// </summary>
    /// <param name="label">Folder label.</param>
    public static By Folder(string label) => By.XPath($"//div[contains(text(),'{label}')]");

    /// <summary>
    /// Module used by the tests as a regular module instance.
    /// </summary>
    public const string WeatherForecastModule = "WeatherForecastModule";

    /// <summary>
    /// Locates the label of the module instance that opens the given path.
    /// </summary>
    /// <param name="path">Path of the module instance, e.g. <c>/weather-forecast-module/12</c>.</param>
    public static By ItemByPath(string path) => By.XPath($"//a[@href='{path}']/span[contains(@class,'layout-menuitem-text')]");

    /// <summary>
    /// Locates a module instance by its label inside the given folder.
    /// A plain "//span[text()=...]" matches the whole sidebar, so a leftover instance with the same
    /// label anywhere else in the menu would be found instead of the one in the folder.
    /// </summary>
    /// <param name="folderLabel">Label of the folder holding the item.</param>
    /// <param name="itemLabel">Label of the module instance.</param>
    public static By ItemInFolder(string folderLabel, string itemLabel) =>
        By.XPath($"//li[div[contains(text(),'{folderLabel}')]]//span[text()='{itemLabel}']");

    /// <summary>
    /// Waits until the menu has finished its (async) initial load, so a subsequent existence
    /// check for a menu item reflects the real state instead of racing the load.
    /// </summary>
    /// <returns>The sidebar element.</returns>
    public IWebElement WaitLoaded()
    {
        var sidebar = Wait.UntilFindElement(LayoutSidebar);
        // An empty menu renders nothing, so there is no element to wait for; ask the backend first.
        if (CountItemsOnServer() > 0)
            Wait.UntilFindElement(MenuItem);
        return sidebar;
    }

    /// <summary>
    /// Returns the number of top-level menu items the backend serves to the current user.
    /// </summary>
    public long CountItemsOnServer()
    {
        const string script = """
            const done = arguments[arguments.length - 1];
            fetch(new URL('api/menu/get', document.baseURI), { credentials: 'include' })
              .then(response => response.ok ? response.json() : Promise.reject(response.status))
              .then(menu => done(menu.length), error => done('error: ' + error));
            """;
        var result = ((IJavaScriptExecutor)Driver).ExecuteAsyncScript(script);
        return result as long? ?? throw new InvalidOperationException($"Failed to load the menu: {result}");
    }

    /// <summary>
    /// Checks whether the loaded menu contains the item right now.
    /// </summary>
    /// <param name="item">Locator of the menu item.</param>
    public bool Contains(By item) => WaitLoaded().ExistsNow(item);

    /// <summary>
    /// Creates a folder at the root level of the menu.
    /// </summary>
    /// <param name="label">Folder label.</param>
    public void CreateRootFolder(string label)
    {
        Actions.ContextClick(Wait.UntilFindElement(LayoutSidebar)).Perform();
        ClickContextMenuItem(ContextMenuNewItem);
        new MenuItemCreateDialog(Driver).Create(label, FolderModule);
        Wait.UntilFindElement(Folder(label));
    }

    /// <summary>
    /// Creates a module instance under the given parent item.
    /// </summary>
    /// <param name="parent">Locator of the parent menu item.</param>
    /// <param name="label">Label of the new menu item.</param>
    /// <param name="moduleName">Name of the module to instantiate.</param>
    public void CreateModuleInstance(By parent, string label, string moduleName)
    {
        OpenContextMenu(parent);
        ClickContextMenuItem(ContextMenuNewItem);
        new MenuItemCreateDialog(Driver).Create(label, moduleName);
    }

    /// <summary>
    /// Deletes the menu item and waits until it disappears.
    /// </summary>
    /// <param name="item">Locator of the menu item.</param>
    public void Delete(By item)
    {
        OpenContextMenu(item);
        ClickContextMenuItem(ContextMenuDeleteItem);
        new ConfirmDialog(Driver).Accept();
        Wait.UntilDisappear(item);
    }

    /// <summary>
    /// Moves the menu item one position up among its visible siblings.
    /// </summary>
    /// <param name="item">Locator of the menu item.</param>
    public void MoveUp(By item)
    {
        OpenContextMenu(item);
        ClickContextMenuItem(ContextMenuMoveUpItem);
    }

    /// <summary>
    /// Moves the menu item one position down among its visible siblings.
    /// </summary>
    /// <param name="item">Locator of the menu item.</param>
    public void MoveDown(By item)
    {
        OpenContextMenu(item);
        ClickContextMenuItem(ContextMenuMoveDownItem);
    }

    /// <summary>
    /// Opens the module instance and waits until the application accepts input again.
    /// </summary>
    /// <param name="item">Locator of the menu item.</param>
    public void Open(By item)
    {
        Wait.UntilFindElement(LayoutSidebar).FindElement(item).Click();
        WaitForAppInteractive();
    }

    /// <summary>
    /// Opens the edit dialog of the menu item.
    /// </summary>
    /// <param name="item">Locator of the menu item.</param>
    public MenuItemEditDialog Edit(By item)
    {
        OpenContextMenu(item);
        ClickContextMenuItem(ContextMenuEditItem);
        return new MenuItemEditDialog(Driver).WaitOpen();
    }

    /// <summary>
    /// Copies the module instance. The copy is labelled "&lt;label&gt; (copy)" and placed right below it.
    /// </summary>
    /// <param name="item">Locator of the menu item.</param>
    public void Copy(By item)
    {
        OpenContextMenu(item);
        ClickContextMenuItem(ContextMenuCopyItem);
    }

    /// <summary>
    /// Makes the module instance the start page of the current user.
    /// </summary>
    /// <param name="item">Locator of the menu item.</param>
    public void SetStartModule(By item)
    {
        OpenContextMenu(item);
        ClickContextMenuItem(ContextMenuSetStartItem);
        WaitForStartMark(item, ContextMenuUnsetStartItem);
    }

    /// <summary>
    /// Clears the start page of the current user, the module instance must be the current start page.
    /// </summary>
    /// <param name="item">Locator of the menu item.</param>
    public void UnsetStartModule(By item)
    {
        OpenContextMenu(item);
        ClickContextMenuItem(ContextMenuUnsetStartItem);
        WaitForStartMark(item, ContextMenuSetStartItem);
    }

    /// <summary>
    /// Checks whether the module instance is the start page of the current user.
    /// </summary>
    /// <param name="item">Locator of the menu item.</param>
    public bool IsStartModule(By item)
    {
        OpenContextMenu(item);
        var isStart = Wait.Until(d => d.FindElement(ContextMenuSub)).FindElements(ContextMenuUnsetStartItem).Count != 0;
        CloseContextMenu();
        return isStart;
    }

    /// <summary>
    /// Opens the context menu of the item and returns the icon classes of its entries, then closes it.
    /// </summary>
    /// <param name="item">Locator of the menu item.</param>
    public IReadOnlyList<string> GetContextMenuIcons(By item)
    {
        OpenContextMenu(item);
        var icons = Wait.Until(d =>
            d.FindElement(ContextMenuSub).FindElements(ContextMenuItems)
                .Select(entry => entry.FindElement(ContextMenuItemIcon).GetAttribute("class") ?? string.Empty)
                .ToList());
        CloseContextMenu();
        return icons;
    }

    /// <summary>
    /// Returns the icon classes of the menu item.
    /// </summary>
    /// <param name="item">Locator of the label of a module instance.</param>
    public string GetIcon(By item) =>
        Wait.UntilFindElement(item).FindElement(By.XPath("./preceding-sibling::i")).GetAttribute("class") ?? string.Empty;

    /// <summary>
    /// Returns the path the menu item opens, e.g. <c>/weather-forecast-module/12</c>.
    /// </summary>
    /// <param name="item">Locator of the label of a module instance.</param>
    public string GetPath(By item) =>
        ToPath(Wait.UntilFindElement(item).FindElement(By.XPath("./ancestor::a[1]")).GetAttribute("href"));

    /// <summary>
    /// Returns the path of the first module instance in the menu, the one opened when no start page is set.
    /// </summary>
    public string GetFirstModulePath()
    {
        WaitLoaded();
        return ToPath(Wait.UntilFindElement(ModuleLink).GetAttribute("href"));
    }

    /// <summary>
    /// Checks whether the menu item renders above the other one.
    /// </summary>
    public bool IsAbove(By item, By other) =>
        Wait.UntilFindElement(item).Location.Y < Wait.UntilFindElement(other).Location.Y;

    private static string ToPath(string? href) =>
        new Uri(href ?? throw new AssertionException("The menu item has no link")).AbsolutePath;

    /// <summary>
    /// Waits until the reloaded menu reflects the new start page: the context menu of the item offers
    /// the opposite action.
    /// </summary>
    private void WaitForStartMark(By item, By expectedAction)
    {
        Wait.Until(d =>
        {
            OpenContextMenu(item);
            var marked = d.FindElement(ContextMenuSub).FindElements(expectedAction).Count != 0;
            CloseContextMenu();
            return marked;
        });
    }

    /// <summary>
    /// Closes the open context menu; PrimeNG hides it on any document click.
    /// </summary>
    private void CloseContextMenu() =>
        ((IJavaScriptExecutor)Driver).ExecuteScript("document.body.click();");

    private void OpenContextMenu(By item) =>
        Actions.ContextClick(Wait.UntilFindElement(item)).Perform();

    /// <summary>
    /// Clicks an item of the currently open context menu, addressing it by its icon.
    /// PrimeNG rebuilds the shared context menu every time its model is replaced, so looking up the
    /// menu and its item in two separate steps races with that rebuild and goes stale; locate and
    /// click in one retried step instead.
    /// </summary>
    private void ClickContextMenuItem(By itemIconLocator) =>
        Wait.Until(d => d.FindElement(ContextMenuSub).FindElement(itemIconLocator).Click());
}
