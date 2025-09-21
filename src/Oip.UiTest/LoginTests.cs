namespace Oip.UiTest;

/// <summary>
/// Represents a class containing login tests.
/// </summary>
[Order(1)]
internal class LoginTests : BaseTest
{

    /// <summary>
    /// Performs a logout action and verifies the redirection to the login page.
    /// </summary>
    [Test, Order(1)]
    public void LogoutTest()
    {
        Sleep();
        Driver.FindElement(OipAppTopBarLogoutButton).Click();
        Driver.FindElement(OipSignInButton);
    }

    /// <summary>
    /// Performs a login attempt with invalid credentials and verifies the error message.
    /// </summary>
    [Test, Order(2)]
    public void LoginFailedTest()
    {
        Sleep();
        Wait.Until(d => d.FindElement(OipSignInButton)).Click();
        Wait.Until(d => d.FindElement(KeycloakUsername)).SendKeys("admin");
        Driver.FindElement(KeycloakPassword).SendKeys("P@ssw0rd1");
        Driver.FindElement(KeycloakLoginButton).Click();
        Driver.FindElement(KeycloakErrorUserName);
    }
}