using Microsoft.AspNetCore.Connections;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;

namespace Oip.UiTest;

/// <summary>
/// Test environment setup shared by all tests in the assembly.
/// Starts a single browser and signs in once instead of doing it in every test class.
/// </summary>
[SetUpFixture]
public class TestSetup
{
    #region Selectors

    private static readonly By OipSignInButton = By.Id("oip-unauthorized-error-sign-in-button");
    private static readonly By KeycloakUsername = By.Id("username");
    private static readonly By KeycloakPassword = By.Id("password");
    private static readonly By KeycloakLoginButton = By.Id("kc-login");
    private static readonly By LayoutSidebar = By.ClassName("layout-sidebar");

    #endregion

    #region Static fields

    private static IWebDriver? _driver;

    #endregion

    #region Test settings

    /// <summary>
    /// Base URL of the application.
    /// </summary>
    public static string BaseUrl { get; set; } = null!;

    /// <summary>
    /// Test run directory.
    /// </summary>
    public static string BaseDirectory { get; set; } = null!;

    /// <summary>
    /// Selenium address (used only when the RemoteDriverUrl parameter is set).
    /// </summary>
    public static string? RemoteDriverUrl { get; set; }

    /// <summary>
    /// Username used to sign in.
    /// </summary>
    public static string Username { get; set; } = null!;

    /// <summary>
    /// Password used to sign in.
    /// </summary>
    public static string Password { get; set; } = null!;

    #endregion Test settings

    #region Properties

    /// <summary>
    /// Global Actions object for performing mouse and keyboard actions in the test environment.
    /// </summary>
    public static Actions? GlobalActions { get; private set; }

    /// <summary>
    /// Global web driver instance used by all tests.
    /// </summary>
    public static IWebDriver GlobalDriver => _driver ?? throw new ConnectionAbortedException();

    /// <summary>
    /// Global wait object for working with web elements in tests.
    /// </summary>
    public static Waiter GlobalWait { get; private set; } = null!;

    #endregion

    private static void StartBrowser()
    {
        var windowsPosition = TestContext.Parameters["WindowsPosition"] ?? "1920,0";
        BaseUrl = TestContext.Parameters["BaseUrl"] ?? "https://localhost:50002";
        BaseDirectory = TestContext.Parameters["BaseDirectory"] ?? AppDomain.CurrentDomain.BaseDirectory;
        RemoteDriverUrl = TestContext.Parameters["RemoteDriverUrl"];
        Username = TestContext.Parameters["Username"] ?? "admin";
        Password = TestContext.Parameters["Password"] ?? "P@ssw0rd";

        var options = new ChromeOptions();
        options.AddArguments(new List<string>
        {
            "--no-sandbox",
            "--disable-web-security",
            "--start-maximized",
            "--disable-infobars",
            "--disable-extensions",
            "--disable-dev-shm-usage",
            $"--window-position={windowsPosition}",
            "--window-size=1920,1080"
        });

        // If a Selenium Grid address is provided, use it (e.g. when running in Docker),
        // otherwise start a local ChromeDriver as before.
        _driver = string.IsNullOrWhiteSpace(RemoteDriverUrl)
            ? new ChromeDriver(options)
            : new RemoteWebDriver(new Uri(RemoteDriverUrl), options);

        _driver.Manage().Window.Maximize();
        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(BaseTest.StandardTimeOutInSeconds);
        _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(BaseTest.StandardTimeOutInSeconds);
        _driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(BaseTest.StandardTimeOutInSeconds);
        GlobalActions = new Actions(_driver);
        GlobalWait = new Waiter(_driver);
    }

    private static void Login()
    {
        _driver?.Navigate().GoToUrl($"{BaseUrl}/unauthorized");
        GlobalWait.UntilClick(OipSignInButton, 1, 60);
        GlobalWait.UntilInput(KeycloakUsername, Username);
        GlobalWait.UntilInput(KeycloakPassword, Password);
        GlobalWait.UntilClick(KeycloakLoginButton);
        GlobalWait.UntilFindElement(LayoutSidebar);
    }

    /// <summary>
    /// Prepares the environment before any test runs.
    /// </summary>
    [OneTimeSetUp]
    public async Task RunBeforeAnyTests()
    {
        if (_driver is not null) return;
        StartBrowser();
        Login();
    }

    /// <summary>
    /// Cleans up after all tests have completed.
    /// </summary>
    [OneTimeTearDown]
    public async Task RunAfterAllTests()
    {
        _driver?.Quit();
        _driver?.Dispose();
    }
}