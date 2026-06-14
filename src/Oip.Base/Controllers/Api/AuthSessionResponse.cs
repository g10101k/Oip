namespace Oip.Api.Controllers.Api;

public sealed class AuthSessionResponse
{
    public bool IsAuthenticated { get; set; }

    public string? UserName { get; set; }

    public string? DisplayName { get; set; }

    public string? Email { get; set; }

    public List<string> Roles { get; set; } = new();
}
