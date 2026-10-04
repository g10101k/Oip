namespace Oip.UiTest;

/// <summary>
/// Sign-in scenarios beyond the plain login and logout covered by <see cref="LoginTests"/>.
/// Every test starts by signing out, the next one signs back in through <see cref="BaseTest.SignInAs"/>.
/// </summary>
[Order(6)]
internal class AuthFlowTests : BaseTest
{
    private static readonly By LayoutSidebar = By.ClassName("layout-sidebar");

    [OneTimeTearDown]
    public void RestoreAdminSession() => SignInAs(TestSetup.Admin);

    [Test, Order(1)]
    public void ProtectedPageIsNotAvailableAfterLogout()
    {
        SignOut();

        Menu.Navigate("/config");

        LoginPage.WaitForForm();
        Assert.That(Menu.ExistsNow(LayoutSidebar), Is.False, "The application is shown without a session");
    }

    [Test, Order(2)]
    public void SignInReturnsToRequestedPage()
    {
        SignOut();
        Menu.Navigate("/config");

        LoginPage.WaitForForm().SignIn(TestSetup.Admin);

        Menu.WaitForPath("/config");
    }

    [Test, Order(3)]
    public void UnauthorizedPageSignInReturnsToRequestedPage()
    {
        SignOut();
        Menu.Navigate("/unauthorized?returnUrl=%2Fmodules");

        LoginPage.ClickUnauthorizedPageSignIn().WaitForForm().SignIn(TestSetup.Admin);

        Menu.WaitForPath("/modules");
    }

    private void SignOut()
    {
        SignInAs(TestSetup.Admin);
        TopBar.Logout();
        // The unauthorized page re-triggers sign-in right after logout and lands on the Keycloak form.
        LoginPage.WaitForForm();
    }
}
