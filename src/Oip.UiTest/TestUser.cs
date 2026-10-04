namespace Oip.UiTest;

/// <summary>
/// Credentials of a Keycloak user the tests sign in with.
/// </summary>
/// <param name="Username">Keycloak username.</param>
/// <param name="Password">Keycloak password.</param>
public record TestUser(string Username, string Password);
