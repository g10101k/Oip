using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Oip.Base.Services;

/// <summary>
/// User service
/// </summary>
public class UserService(IHttpContextAccessor httpContextAccessor)
{
    /// <summary>
    /// Get user e-mail
    /// </summary>
    /// <returns></returns>
    public string? GetUserEmail()
    {
        return GetClaimValue(ClaimTypes.Email);
    }

    /// <summary>
    /// Get user login
    /// </summary>
    /// <returns></returns>
    public string? GetUserLogin()
    {
        return GetClaimValue(ClaimTypes.Name) ??
               GetClaimValue("preferred_username");
    }

    /// <summary>
    /// Get user first name
    /// </summary>
    /// <returns></returns>
    public string? GetUserFirstName()
    {
        return GetClaimValue(ClaimTypes.GivenName) ??
               GetClaimValue("given_name");
    }

    /// <summary>
    /// Get user last name
    /// </summary>
    /// <returns></returns>
    public string? GetUserLastName()
    {
        return GetClaimValue(ClaimTypes.Surname) ??
               GetClaimValue("family_name");
    }

    /// <summary>
    /// Get user full name
    /// </summary>
    /// <returns></returns>
    public string? GetUserFullName()
    {
        var firstName = GetUserFirstName();
        var lastName = GetUserLastName();
        var fullName = string.Join(" ", new[] { firstName, lastName }.Where(v => !string.IsNullOrWhiteSpace(v)));

        return !string.IsNullOrWhiteSpace(fullName)
            ? fullName
            : GetClaimValue("name") ?? GetUserLogin() ?? GetUserEmail();
    }

    /// <summary>
    /// Get user roles from 
    /// </summary>
    /// <returns></returns>
    public List<string> GetUserRoles()
    {
        if (httpContextAccessor.HttpContext == null)
            return [];

        return httpContextAccessor.HttpContext.User.Claims.Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();
    }

    private string? GetClaimValue(string claimType)
    {
        return httpContextAccessor.HttpContext?.User.Claims
            .FirstOrDefault(c => c.Type == claimType)?.Value;
    }
}
