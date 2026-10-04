namespace Oip.UiTest;

[Order(1)]
internal class LoginTests : BaseTest
{
    [Test, Order(1)]
    public void LogoutTest()
    {
        TopBar.Logout();

        // The unauthorized page immediately re-triggers sign-in on logout
        // (see BffSecurityService.logout/authorizeAfterLogoutReturnUrlKey in security.service.ts),
        // so the app redirects straight to the Keycloak login form without showing the "Sign In" button.
        LoginPage.WaitForForm();
    }

    [Test, Order(2)]
    public void LoginFailedTest()
    {
        LoginPage.Submit(TestSetup.Admin.Username, TestSetup.Admin.Password + "1").WaitForError();

        // The driver is shared across the whole test assembly (see TestSetup), so after checking
        // the failed sign-in we need to restore an authenticated session for the tests that follow this class.
        LoginPage.SignIn(TestSetup.Admin);
    }
}
