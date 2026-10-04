using System.Text.Json;
using Oip.UiTest.Pages;

namespace Oip.UiTest;

/// <summary>
/// Terminating sessions. The session to terminate belongs to a second browser signed in as the regular user,
/// told apart by its user agent; the session of the shared browser must stay.
/// </summary>
[Order(14)]
internal class SessionsTests : BaseTest
{
    private SessionsPage Sessions => new(Driver);

    [Test]
    public void OwnSessionCannotBeTerminated()
    {
        SignInAs(TestSetup.Admin);

        Assert.That(Sessions.Open().IsCurrentSessionProtected, Is.True);
    }

    [Test]
    public void TerminatedSessionIsSignedOut()
    {
        SignInAs(TestSetup.Admin);
        var userAgent = $"OipUiTest-{Guid.NewGuid():N}";
        using var otherBrowser = TestSetup.CreateDriver(userAgent);
        new KeycloakLoginPage(otherBrowser).OpenFromUnauthorizedPage(TestSetup.BaseUrl).SignIn(TestSetup.User);
        Assert.That(IsSignedIn(otherBrowser), Is.True, "The second browser did not sign in");

        var sessions = Sessions.Open();
        Assert.That(sessions.Contains(userAgent), Is.True, "The session of the second browser is not listed");
        sessions.Terminate(userAgent);
        sessions.Reload();

        Assert.Multiple(() =>
        {
            Assert.That(sessions.Contains(userAgent), Is.False, "The terminated session is still listed");
            Assert.That(IsSignedIn(otherBrowser), Is.False, "The terminated session still works");
        });
    }

    // A terminated session is answered with 401.
    private static bool IsSignedIn(IWebDriver browser) =>
        new BackendApi(browser).TryGet("api/security/get-current-auth-session", out var session) &&
        session.ValueKind == JsonValueKind.Object && session.GetProperty("isAuthenticated").GetBoolean();
}
