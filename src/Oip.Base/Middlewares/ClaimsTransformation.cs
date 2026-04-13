using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json.Linq;

namespace Oip.Base.Middlewares;

/// <summary>
/// Getter roles to claim
/// </summary>
public class ClaimsTransformation : IClaimsTransformation
{
    /// <summary>
    /// Get roles
    /// </summary>
    /// <param name="principal"></param>
    /// <returns></returns>
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var identity = new ClaimsIdentity();
        AddRolesFromRealmAccess(principal, identity);
        AddNameClaims(principal, identity);
        principal.AddIdentity(identity);
        return Task.FromResult(principal);
    }
    
    private static void AddRolesFromRealmAccess(ClaimsPrincipal currentUser, ClaimsIdentity identity)
    {
        var realmAccessJson = currentUser.FindFirst("realm_access")?.Value;
        if (realmAccessJson is null)
            return;
        var json = JObject.Parse(realmAccessJson);
        if (json["roles"] is JArray rolesArray)
        {
            foreach (var role in rolesArray)
                identity.AddClaim(new Claim(ClaimTypes.Role, role.ToString()));
        }
    }

    private static void AddNameClaims(ClaimsPrincipal currentUser, ClaimsIdentity identity)
    {
        AddClaimIfMissing(currentUser, identity, "given_name", ClaimTypes.GivenName);
        AddClaimIfMissing(currentUser, identity, "family_name", ClaimTypes.Surname);
        AddClaimIfMissing(currentUser, identity, "preferred_username", ClaimTypes.Name);

        var fullName = currentUser.FindFirst("name")?.Value;
        if (!string.IsNullOrWhiteSpace(fullName) &&
            !currentUser.HasClaim(c => c.Type == "name") &&
            !currentUser.HasClaim(c => c.Type == ClaimTypes.NameIdentifier))
        {
            identity.AddClaim(new Claim("name", fullName));
        }
    }

    private static void AddClaimIfMissing(
        ClaimsPrincipal currentUser,
        ClaimsIdentity identity,
        string sourceClaimType,
        string targetClaimType)
    {
        var claimValue = currentUser.FindFirst(sourceClaimType)?.Value;
        if (string.IsNullOrWhiteSpace(claimValue))
            return;

        if (currentUser.HasClaim(c => c.Type == targetClaimType))
            return;

        identity.AddClaim(new Claim(targetClaimType, claimValue));
    }
}
