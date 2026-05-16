using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;
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
        if (identity.Claims.Any())
            principal.AddIdentity(identity);
        return Task.FromResult(principal);
    }

    public static void AddRolesFromAccessToken(ClaimsPrincipal? principal, string? accessToken)
    {
        if (principal is null || string.IsNullOrWhiteSpace(accessToken))
            return;

        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(accessToken))
            return;

        var token = handler.ReadJwtToken(accessToken);
        var realmAccessJson = token.Claims.FirstOrDefault(claim => claim.Type == "realm_access")?.Value;
        if (realmAccessJson is null)
            return;

        var identity = principal.Identities.FirstOrDefault(current => current.IsAuthenticated) ??
                       principal.Identity as ClaimsIdentity;
        if (identity is null)
            return;

        AddRolesFromRealmAccessJson(realmAccessJson, principal, identity);
    }
    
    private static void AddRolesFromRealmAccess(ClaimsPrincipal currentUser, ClaimsIdentity identity)
    {
        var realmAccessJson = currentUser.FindFirst("realm_access")?.Value;
        if (realmAccessJson is null)
            return;

        AddRolesFromRealmAccessJson(realmAccessJson, currentUser, identity);
    }

    private static void AddRolesFromRealmAccessJson(
        string realmAccessJson,
        ClaimsPrincipal currentUser,
        ClaimsIdentity identity)
    {
        JObject json;
        try
        {
            json = JObject.Parse(realmAccessJson);
        }
        catch (JsonException)
        {
            return;
        }

        if (json["roles"] is JArray rolesArray)
        {
            foreach (var role in rolesArray)
                AddRoleIfMissing(currentUser, identity, role.ToString());
        }
    }

    private static void AddRoleIfMissing(ClaimsPrincipal currentUser, ClaimsIdentity identity, string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            return;

        if (currentUser.HasClaim(ClaimTypes.Role, role) ||
            identity.HasClaim(ClaimTypes.Role, role))
            return;

        identity.AddClaim(new Claim(ClaimTypes.Role, role));
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
