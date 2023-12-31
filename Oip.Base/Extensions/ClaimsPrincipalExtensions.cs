using System.Security.Claims;

namespace Oip.Base.Extensions;

/// <summary>
/// Claim extension
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Get user id from claim
    /// </summary>
    /// <param name="principal"></param>
    /// <returns></returns>
    public static string? GetUserId(this ClaimsPrincipal principal)
        => principal.FindFirst("sub")?.Value;

    /// <summary>
    /// Get user name from claim
    /// </summary>
    /// <param name="principal"></param>
    /// <returns></returns>
    public static string? GetUserName(this ClaimsPrincipal principal) =>
        principal.FindFirst(x => x.Type == ClaimTypes.Name)?.Value;
}
