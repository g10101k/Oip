namespace Oip.UiTest;

[Order(1)]
internal class LoginTests : BaseTest
{
    [Test, Order(1)]
    public void LogoutTest()
    {
        Wait.Until(d => d.FindElement(OipAppTopBarLogoutButton)).Click();

        // Clicking the logout button opens a confirmation dialog (ConfirmDialog);
        // the actual sign-out only happens after confirming.
        Wait.Until(d => d.FindElement(ConfirmDialogAcceptButton)).Click();

        // The unauthorized page immediately re-triggers sign-in on logout
        // (see BffSecurityService.logout/authorizeAfterLogoutReturnUrlKey in security.service.ts),
        // so the app redirects straight to the Keycloak login form without showing the "Sign In" button.
        Wait.Until(d => d.FindElement(KeycloakUsername));
    }

    [Test, Order(2)]
    public void LoginFailedTest()
    {
        Wait.Until(d => d.FindElement(KeycloakUsername)).SendKeys("admin");
        Driver.FindElement(KeycloakPassword).SendKeys("P@ssw0rd1");
        Driver.FindElement(KeycloakLoginButton).Click();
        Driver.FindElement(KeycloakErrorUserName);

        // The driver is shared across the whole test assembly (see TestSetup), so after checking
        // the failed sign-in we need to restore an authenticated session for the tests that follow this class.
        Driver.FindElement(KeycloakUsername).Clear();
        Driver.FindElement(KeycloakUsername).SendKeys(TestSetup.Username);
        Driver.FindElement(KeycloakPassword).Clear();
        Driver.FindElement(KeycloakPassword).SendKeys(TestSetup.Password);
        Driver.FindElement(KeycloakLoginButton).Click();
        Wait.Until(d => d.FindElement(OipMenuContainer));
    }
}