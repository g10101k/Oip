namespace Oip.Base.Controllers.Api;

public sealed class AuthCsrfTokenResponse
{
    public string Token { get; set; } = string.Empty;

    public string HeaderName { get; set; } = string.Empty;
}
