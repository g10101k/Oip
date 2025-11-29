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
}