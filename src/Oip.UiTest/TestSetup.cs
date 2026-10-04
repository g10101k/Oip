using Microsoft.AspNetCore.Connections;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using Oip.UiTest.Pages;

namespace Oip.UiTest;

/// <summary>
/// Test environment setup shared by all tests in the assembly.
/// Starts a single browser and signs in once instead of doing it in every test class.
/// </summary>
[SetUpFixture]
public class TestSetup
{
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
    /// Administrator the tests sign in with by default.
    /// </summary>
    public static TestUser Admin { get; set; } = null!;

    /// <summary>
    /// Regular user without the admin role, used to check what non-administrators can access.
    /// </summary>
    public static TestUser User { get; set; } = null!;

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

    /// <summary>
    /// The user the shared browser session is currently signed in as, or null while signed out.
    /// </summary>
    public static TestUser? CurrentUser { get; set; }

    #endregion

    /// <summary>
    /// Host name the backend uses to reach servers started by the tests, see <see cref="ManifestServer"/>.
    /// </summary>
    public static string TestHost { get; set; } = null!;

    private static string _windowsPosition = null!;

    private static void StartBrowser()
    {
        _windowsPosition = TestContext.Parameters["WindowsPosition"] ?? "1920,0";
        TestHost = TestContext.Parameters["TestHost"] ?? "localhost";
        BaseUrl = TestContext.Parameters["BaseUrl"] ?? "https://localhost:50002";
        BaseDirectory = TestContext.Parameters["BaseDirectory"] ?? AppDomain.CurrentDomain.BaseDirectory;
        RemoteDriverUrl = TestContext.Parameters["RemoteDriverUrl"];
        Admin = new TestUser(
            TestContext.Parameters["Username"] ?? "admin",
            TestContext.Parameters["Password"] ?? "P@ssw0rd");
        User = new TestUser(
            TestContext.Parameters["UserUsername"] ?? "user",
            TestContext.Parameters["UserPassword"] ?? "P@ssw0rd");

        _driver = CreateDriver();
        GlobalActions = new Actions(_driver);
        GlobalWait = new Waiter(_driver);
    }

    /// <summary>
    /// Starts a browser with the settings of the test run. Besides the shared session, tests use it for
    /// a second browser, e.g. to have a session of another user.
    /// </summary>
    /// <param name="userAgent">User agent to send instead of the default one, to tell the browser apart.</param>
    public static IWebDriver CreateDriver(string? userAgent = null)
    {
        var options = new ChromeOptions { AcceptInsecureCertificates = true };
        options.AddArguments(new List<string>
        {
            "--no-sandbox",
            "--disable-web-security",
            "--start-maximized",
            "--disable-infobars",
            "--disable-extensions",
            "--disable-dev-shm-usage",
            $"--window-position={_windowsPosition}",
            "--window-size=1920,1080"
        });
        if (userAgent is not null)
            options.AddArgument($"--user-agent={userAgent}");
        // Smoke tests read the browser console to catch errors that do not break the page visibly.
        options.SetLoggingPreference(LogType.Browser, LogLevel.All);

        // If a Selenium Grid address is provided, use it (e.g. when running in Docker),
        // otherwise start a local ChromeDriver as before.
        IWebDriver driver = string.IsNullOrWhiteSpace(RemoteDriverUrl)
            ? new ChromeDriver(options)
            : new RemoteWebDriver(new Uri(RemoteDriverUrl), options);

        driver.Manage().Window.Maximize();
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(BaseTest.StandardTimeOutInSeconds);
        driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(BaseTest.StandardTimeOutInSeconds);
        driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(BaseTest.StandardTimeOutInSeconds);
        return driver;
    }

    private static void Login()
    {
        new KeycloakLoginPage(GlobalDriver)
            .OpenFromUnauthorizedPage(BaseUrl)
            .SignIn(Admin);
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