using Oip.UiTest.Extensions;

namespace Oip.UiTest;

/// <summary>
/// Represents a class containing UI tests for menu interactions.
/// </summary>
[Order(2)]
internal class MenuTests : BaseTest
{
    private readonly By _rootFolderLocator = By.XPath("//div[contains(text(),'#RootFolder')]");
    private readonly By _contextMenuSub = By.TagName("p-contextmenu-sub");
    private readonly By _layoutSidebar = By.ClassName("layout-sidebar");

    /// <summary>
    /// Creates a root folder in the application.
    /// </summary>
    [Test, Order(1)]
    public void CreateRootFolder()
    {
        var layoutSidebar = Wait.Until(d => d.FindElement(_layoutSidebar));
        Sleep();
        if (layoutSidebar.ExistsNow(_rootFolderLocator)) return;

        Actions.ContextClick(layoutSidebar).Perform();
        var contextMenu = Wait.Until(d => d.FindElement(_contextMenuSub));

        contextMenu.FindElement(By.XPath("//span[text()='New']")).Click();
        Wait.Until(d => d.FindElement(OipMenuCreateItemLabel));
        Sleep();
        Driver.FindElement(OipMenuCreateItemLabel).SendKeys(RootFolderName);
        Driver.FindElement(OipMenuItemCreateModule).Click();

        var dialog = Wait.Until(d => d.FindElement(By.CssSelector("p-dialog")));
        var objectMappingItem = Wait.Until(d => dialog.FindElement(By.XPath("//span[text()='FolderModule']")));
        Driver.ScrollToElement(objectMappingItem).Click();

        Driver.FindElement(OipMenuItemCreateSaveButton).Click();
        Sleep();
    }

    /// <summary>
    /// Creates a dashboard module.
    /// </summary>
    [Test, Order(2)]
    public void CreateDashboard()
    {
        const string menuLabel = "#DashboardModule";

        var layoutSidebar = Wait.Until(d => d.FindElement(_layoutSidebar));
        Sleep();
        if (layoutSidebar.ExistsNow(By.XPath($"//span[text()='{menuLabel}']"))) return;

        var rootFolderItem = Wait.Until(d => d.FindElement(_rootFolderLocator));

        Actions.ContextClick(rootFolderItem).Perform();
        var contextMenu = Driver.FindElement(_contextMenuSub);
        contextMenu.FindElement(By.XPath("//span[text()='Add']")).Click();

        var dialog = Wait.Until(d => d.FindElement(By.CssSelector("p-dialog")));
        var label = Driver.FindElement(OipMenuItemCreateLabel);
        label.Clear();
        label.SendKeys(menuLabel);

        var selectModule = Driver.FindElement(OipMenuItemCreateModule);
        selectModule.Click();

        var objectMappingItem = Wait.Until(d => dialog.FindElement(By.XPath("//span[text()='DashboardModule']")));
        Driver.ScrollToElement(objectMappingItem).Click();

        Driver.FindElement(OipMenuItemCreateSaveButton).Click();
        Sleep();

        GoToModuleInstance(menuLabel);
    }

    /// <summary>
    /// Creates a weather module.
    /// </summary>
    [Test, Order(3)]
    public void CreateWeatherModule()
    {
        const string menuLabel = "#WeatherForecastModule";
        var layoutSidebar = Wait.Until(d => d.FindElement(_layoutSidebar));
        Sleep();
        if (layoutSidebar.ExistsNow(By.XPath($"//span[text()='{menuLabel}']"))) return;
        var folderItem = Wait.Until(d => d.FindElement(_rootFolderLocator));
        Actions.ContextClick(folderItem).Perform();

        var contextMenu = Driver.FindElement(_contextMenuSub);
        contextMenu.FindElement(By.XPath("//span[text()='Add']")).Click();

        var dialog = Wait.Until(d => d.FindElement(By.CssSelector("p-dialog")));
        var label = Driver.FindElement(OipMenuItemCreateLabel);
        label.Clear();
        label.SendKeys(menuLabel);

        Driver.FindElement(OipMenuItemCreateModule).Click();

        var weatherModuleItem =
            Wait.Until(d => dialog.FindElement(By.XPath("//span[text()='WeatherForecastModule']")));
        Driver.ScrollToElement(weatherModuleItem).Click();

        Driver.FindElement(OipMenuItemCreateSaveButton).Click();
        Sleep();
        GoToModuleInstance(menuLabel);
    }

    /// <summary>
    /// </summary>
    [Test, Order(3)]
    public void CreateAndDeleteWeatherModule()
    {
        const string menuLabel = "#WeatherForecastModuleForDelete";
        var layoutSidebar = Wait.Until(d => d.FindElement(_layoutSidebar));
        Sleep();
        if (!layoutSidebar.ExistsNow(By.XPath($"//span[text()='{menuLabel}']")))
        {
            var folderItem = Wait.Until(d => d.FindElement(_rootFolderLocator));
            Actions.ContextClick(folderItem).Perform();

            Driver.FindElement(_contextMenuSub).FindElement(By.XPath("//span[text()='Add']")).Click();

            var dialog = Wait.Until(d => d.FindElement(By.CssSelector("p-dialog")));
            var label = Driver.FindElement(OipMenuItemCreateLabel);
            label.Clear();
            label.SendKeys(menuLabel);

            Driver.FindElement(OipMenuItemCreateModule).Click();

            var weatherModuleItem =
                Wait.Until(d => dialog.FindElement(By.XPath("//span[text()='WeatherForecastModule']")));
            Driver.ScrollToElement(weatherModuleItem).Click();

            Driver.FindElement(OipMenuItemCreateSaveButton).Click();
            Sleep();
            GoToModuleInstance(menuLabel);
        }

        layoutSidebar = Wait.Until(d => d.FindElement(_layoutSidebar));
        Sleep();
        var moduleInstanceForDelete = layoutSidebar.FindElement(By.XPath($"//span[text()='{menuLabel}']"));
        Actions.ContextClick(moduleInstanceForDelete).Perform();

        Driver.FindElement(_contextMenuSub).FindElement(By.XPath("//span[text()='Delete']")).Click();
        var deleteDialog = Wait.Until(d => d.FindElement(By.CssSelector("p-dialog")));
        deleteDialog.FindElement(By.XPath("//p-button/button/span[text()='Delete']")).Click();
        
        Sleep();
        
        layoutSidebar = Wait.Until(d => d.FindElement(_layoutSidebar));
        if (layoutSidebar.ExistsNow(By.XPath($"//span[text()='{menuLabel}']")))
            throw new InvalidOperationException();

    }
}