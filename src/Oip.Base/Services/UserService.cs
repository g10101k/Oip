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
        var searchResult =
            httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
        if (searchResult is { } claim)
        {
            return claim.Value;
        }

        return null;
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
}