namespace Oip.UiTest.Pages;

/// <summary>
/// The Keycloak sign-in form and the Oip unauthorized page that leads to it.
/// </summary>
internal class KeycloakLoginPage(IWebDriver driver) : BasePage(driver)
{
    private static readonly By OipSignInButton = By.Id("oip-unauthorized-error-sign-in-button");
    private static readonly By Username = By.Id("username");
    private static readonly By Password = By.Id("password");
    private static readonly By LoginButton = By.Id("kc-login");
    private static readonly By LayoutSidebar = By.ClassName("layout-sidebar");

    /// <summary>
    /// The Keycloak login error alert (custom "oip" theme no longer renders
    /// the stock "input-error-username" element, only this alert banner).
    /// </summary>
    private static readonly By ErrorAlert = By.CssSelector(".oip-alert.oip-alert-error");

    /// <summary>
    /// Opens the Oip unauthorized page and follows its sign-in button to the Keycloak form.
    /// </summary>
    /// <param name="baseUrl">Base URL of the application.</param>
    public KeycloakLoginPage OpenFromUnauthorizedPage(string baseUrl)
    {
        Driver.Navigate().GoToUrl($"{baseUrl}/unauthorized");
        return ClickUnauthorizedPageSignIn();
    }

    /// <summary>
    /// Clicks the sign-in button of the Oip unauthorized page, which leads to the Keycloak form.
    /// </summary>
    public KeycloakLoginPage ClickUnauthorizedPageSignIn()
    {
        Wait.UntilClick(OipSignInButton, 1, 60);
        return this;
    }

    /// <summary>
    /// Waits until the Keycloak sign-in form is shown.
    /// </summary>
    public KeycloakLoginPage WaitForForm()
    {
        Wait.UntilFindElement(Username);
        return this;
    }

    /// <summary>
    /// Checks whether the Keycloak sign-in form is shown right now.
    /// </summary>
    public bool IsFormShown() => ExistsNow(Username);

    /// <summary>
    /// Fills in the credentials and submits the form.
    /// </summary>
    /// <param name="username">Keycloak username.</param>
    /// <param name="password">Keycloak password.</param>
    public KeycloakLoginPage Submit(string username, string password)
    {
        var usernameInput = Wait.UntilFindElement(Username);
        usernameInput.Clear();
        usernameInput.SendKeys(username);
        var passwordInput = Driver.FindElement(Password);
        passwordInput.Clear();
        passwordInput.SendKeys(password);
        Driver.FindElement(LoginButton).Click();
        return this;
    }

    /// <summary>
    /// Signs in and waits until the application layout is shown.
    /// </summary>
    /// <param name="user">The user to sign in with.</param>
    public void SignIn(TestUser user)
    {
        Submit(user.Username, user.Password);
        Wait.UntilFindElement(LayoutSidebar);
        // Only the shared session is tracked; a second browser opened by a test signs in on its own.
        if (ReferenceEquals(Driver, TestSetup.GlobalDriver))
            TestSetup.CurrentUser = user;
    }

    /// <summary>
    /// Waits until Keycloak reports a failed sign-in.
    /// </summary>
    public KeycloakLoginPage WaitForError()
    {
        Wait.UntilFindElement(ErrorAlert);
        return this;
    }
}
